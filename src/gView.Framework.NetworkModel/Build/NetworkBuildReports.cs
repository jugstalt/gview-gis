using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Extensions;
using gView.Framework.NetworkModel.Ids;
using gView.Framework.NetworkModel.Query;
using gView.Framework.NetworkModel.Store;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace gView.Framework.NetworkModel.Build;

/// <summary>The report files of a build that list problems, one category each.</summary>
public enum NetworkProblemCategory
{
    /// <summary>conflicts.json: the data cannot form a network (only after a failed build).</summary>
    Conflicts,

    /// <summary>
    /// diagnostics.json, the connections a connect rule (or split default) refused or connected and reported: the work list
    /// of data errors, grouped by the rule.
    /// </summary>
    Rules,

    /// <summary>diagnostics.json without the rules: what the builder noticed (isolated nodes, merged nodes, short parts, ...).</summary>
    Diagnostics,

    /// <summary>duplicates.json: points of one layer at the same place; one was kept, the others left out.</summary>
    Duplicates,

    /// <summary>merged.json: points merged into the point of a layer higher in the TOC.</summary>
    Merged
}

/// <summary>How bad the problems of a category are.</summary>
public enum NetworkProblemSeverity
{
    Info,
    Warning,
    Error
}

/// <summary>A feature a problem is about: layer path, object id and its role (kept, dropped, merged, node, line, source).</summary>
public sealed record NetworkProblemFeature(string Layer, long Oid, string Role)
{
    public string Label => $"{Layer}#{Oid}";
}

/// <summary>
/// One problem of a build report: its category, the group it is listed under (the kind, or the layer(s) for duplicates
/// and merged points), a readable message, the place in the spatial reference of the network and the features involved.
/// </summary>
public sealed record NetworkProblem(NetworkProblemCategory Category, string Group, string Message, double X, double Y, IReadOnlyList<NetworkProblemFeature> Features);

/// <summary>
/// Reads the reports a build writes next to the network (conflicts.json, diagnostics.json, duplicates.json, merged.json,
/// build.log) as problems, for Carto and later the server. Works without a built network (failed build); with its
/// metadata (network.json) the ids and type numbers in older diagnostics messages become readable.
/// </summary>
public sealed class NetworkBuildReports
{
    public const string ConflictsFile = "conflicts.json";
    public const string DiagnosticsFile = "diagnostics.json";
    public const string DuplicatesFile = "duplicates.json";
    public const string MergedFile = "merged.json";

    public static IReadOnlyList<NetworkProblemCategory> Categories { get; } =
        [NetworkProblemCategory.Conflicts, NetworkProblemCategory.Rules, NetworkProblemCategory.Diagnostics, NetworkProblemCategory.Duplicates, NetworkProblemCategory.Merged];

    /// <summary>The diagnostics of connections, decided by the connect rules; they go to <see cref="NetworkProblemCategory.Rules"/>.</summary>
    public static IReadOnlySet<string> ConnectionKinds { get; } =
        new HashSet<string>(["ConnectionRefused", "SplitRefused", "ConnectionReported", "SplitReported"], StringComparer.OrdinalIgnoreCase);

    public string Directory { get; }

    /// <summary>network.json of the directory; null when the network is not built (failed build).</summary>
    public NetworkMetadata? Metadata { get; }

    /// <summary>When the newest report was written; null when there is none.</summary>
    public DateTime? Written { get; }

    /// <summary>build.log; null when missing.</summary>
    public string? Log { get; }

    /// <summary>The problems per category; a category without its file has none.</summary>
    public IReadOnlyDictionary<NetworkProblemCategory, IReadOnlyList<NetworkProblem>> Problems { get; }

    /// <summary>The categories whose file exists (also when empty).</summary>
    public IReadOnlySet<NetworkProblemCategory> Present { get; }

    /// <summary>Whether any report file was found.</summary>
    public bool Any => Present.Count > 0 || Log is not null;

    private NetworkBuildReports(string directory, NetworkMetadata? metadata, DateTime? written, string? log,
        Dictionary<NetworkProblemCategory, IReadOnlyList<NetworkProblem>> problems, HashSet<NetworkProblemCategory> present)
    {
        Directory = directory;
        Metadata = metadata;
        Written = written;
        Log = log;
        Problems = problems;
        Present = present;
    }

    public IReadOnlyList<NetworkProblem> Of(NetworkProblemCategory category)
        => Problems.TryGetValue(category, out var list) ? list : [];

    public static string FileName(NetworkProblemCategory category) => category switch
    {
        NetworkProblemCategory.Conflicts => ConflictsFile,
        NetworkProblemCategory.Diagnostics or NetworkProblemCategory.Rules => DiagnosticsFile,
        NetworkProblemCategory.Duplicates => DuplicatesFile,
        _ => MergedFile
    };

    public static NetworkProblemSeverity Severity(NetworkProblemCategory category) => category switch
    {
        NetworkProblemCategory.Conflicts => NetworkProblemSeverity.Error,
        NetworkProblemCategory.Diagnostics or NetworkProblemCategory.Rules => NetworkProblemSeverity.Warning,
        _ => NetworkProblemSeverity.Info
    };

    /// <summary>What a category or a kind (of a conflict or diagnostic) means; null for an unknown kind.</summary>
    public static string? Describe(string kindOrCategory) => Descriptions.TryGetValue(kindOrCategory, out var text) ? text : null;

    private static readonly Dictionary<string, string> Descriptions = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(NetworkProblemCategory.Conflicts)] = "The data cannot form a network: the build stopped before writing anything. Fix these first.",
        [nameof(NetworkProblemCategory.Rules)] = "Connections a connect rule refused (Report) or made anyway and reported (Connect / split + report): the places to fix in the data, by rule.",
        [nameof(NetworkProblemCategory.Diagnostics)] = "What the builder noticed while joining, splitting and sharding; the network was built anyway.",
        [nameof(NetworkProblemCategory.Duplicates)] = "Points of one layer at the same place (within the tolerance): the first was kept, the others were left out.",
        [nameof(NetworkProblemCategory.Merged)] = "Points within the tolerance of a point of a layer higher in the TOC were merged into it (conflict mode PriorityByToc).",

        // NodeConflictKind
        ["RealNodesTooClose"] = "Two or more points (real nodes) lie within the tolerance of each other, e.g. two boxes at one place.",
        ["DuplicateEdgeId"] = "The same edge id came up twice (a line with more parts than the id allows, or duplicate object ids).",
        ["HelperIdTaken"] = "A line end got an id another node already has.",

        // BuildDiagnosticKind
        ["OutsideShardRegions"] = "Nodes outside every shard region got the default shard or a grid cell; one report per shard.",
        ["OverlappingShardRegions"] = "Two shard regions both contain a node; the first region wins. One report per pair of regions.",
        ["ClusterWiderThanTolerance"] = "A chain of line ends joined into one node reaches further than the tolerance: check for lines drawn too densely.",
        ["IsolatedNode"] = "A point (real node) without any line.",
        ["ShortPartDropped"] = "A part of a line shorter than the tolerance that starts and ends at the same node was left out.",
        ["NodesMerged"] = "Points within the tolerance of each other were merged into one node.",
        ["ConnectionRefused"] = "A line end within the tolerance of a point did not connect to it: a connect rule answered Report.",
        ["SplitRefused"] = "A point within the tolerance of the inside of a line did not split it: a split rule answered Report.",
        ["ConnectionReported"] = "A line end within the tolerance of a point connected to it, and the rule wants it fixed (Connect / split + report).",
        ["SplitReported"] = "A point within the tolerance of the inside of a line split it, and the rule wants it fixed, e.g. the line is not broken up there (Connect / split + report).",
    };

    /// <summary>Reads the reports of a network directory; <paramref name="metadata"/> (network.json) makes older messages readable.</summary>
    public static NetworkBuildReports Read(string directory, NetworkMetadata? metadata = null)
    {
        metadata ??= new FileNetworkStore(directory).ReadMetadata();

        var problems = new Dictionary<NetworkProblemCategory, IReadOnlyList<NetworkProblem>>();
        var present = new HashSet<NetworkProblemCategory>();
        DateTime? written = null;

        void Stamp(string path)
        {
            var time = File.GetLastWriteTime(path);
            if (written is null || time > written)
            {
                written = time;
            }
        }

        foreach (var category in Categories.Where(c => c != NetworkProblemCategory.Rules))
        {
            var path = Path.Combine(directory, FileName(category));
            if (!File.Exists(path))
            {
                continue;
            }
            Stamp(path);
            present.Add(category);
            var json = File.ReadAllText(path);
            if (category == NetworkProblemCategory.Diagnostics)
            {
                // one file, two categories: the connections decided by the rules, and the rest
                var all = ReadDiagnostics(json, metadata);
                present.Add(NetworkProblemCategory.Rules);
                problems[NetworkProblemCategory.Rules] = all.Where(p => p.Category == NetworkProblemCategory.Rules).ToList();
                problems[category] = all.Where(p => p.Category == NetworkProblemCategory.Diagnostics).ToList();
                continue;
            }
            problems[category] = category switch
            {
                NetworkProblemCategory.Conflicts => ReadConflicts(json),
                NetworkProblemCategory.Duplicates => ReadDuplicates(json),
                _ => ReadMerged(json)
            };
        }

        string? log = null;
        var logPath = Path.Combine(directory, NetworkBuildRunner.LogFile);
        if (File.Exists(logPath))
        {
            Stamp(logPath);
            log = File.ReadAllText(logPath);
        }

        return new NetworkBuildReports(directory, metadata, written, log, problems, present);
    }

    #region Read

    private sealed record KindRecord(string? Kind, double X, double Y, string? Message, List<string?>? Sources, string? Rule = null, int? RuleIndex = null);
    private sealed record DuplicateRecord(string? Layer, long KeptOid, long DroppedOid, double X, double Y);
    private sealed record MergedRecord(string? KeptLayer, long KeptOid, string? MergedLayer, long MergedOid, double X, double Y);

    private static List<T> Deserialize<T>(string json)
        => string.IsNullOrWhiteSpace(json) ? [] : JsonSerializer.Deserialize<List<T>>(json, NetworkConfig.JsonOptions) ?? [];

    internal static IReadOnlyList<NetworkProblem> ReadConflicts(string json)
        => Deserialize<KindRecord>(json).Select(c =>
        {
            var features = Features(c.Sources, "source");
            var kind = c.Kind ?? "Conflict";
            var message = features.Count > 0 ? string.Join(", ", features.Select(f => f.Label)) : string.Join(", ", c.Sources ?? []);
            // RealNodesTooClose by the layers involved, like the build log
            var group = kind == "RealNodesTooClose" && features.Count > 0
                ? string.Join(" / ", features.Select(f => f.Layer).Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase))
                : kind;
            return new NetworkProblem(NetworkProblemCategory.Conflicts, group, message, c.X, c.Y, features);
        }).ToList();

    internal static IReadOnlyList<NetworkProblem> ReadDiagnostics(string json, NetworkMetadata? metadata)
    {
        var types = metadata is null || metadata.Types.Count == 0 ? null : metadata.ToTypeRegistry();
        return Deserialize<KindRecord>(json).Select(d =>
        {
            var kind = d.Kind ?? "Diagnostic";
            // the node first, then the line (ConnectionRefused, SplitRefused)
            var features = Features(d.Sources, d.Sources?.Count == 2 ? null : "source");
            var message = metadata is null ? d.Message ?? "" : ResolveMessage(d.Message ?? "", metadata, types);
            if (ConnectionKinds.Contains(kind))
            {
                // by the rule that decided (builds before rule names: by the kind)
                var rule = string.IsNullOrWhiteSpace(d.Rule) ? kind : d.RuleIndex is int index ? $"{index}: {d.Rule}" : d.Rule;
                return new NetworkProblem(NetworkProblemCategory.Rules, rule, $"{Readable(kind)}: {message}", d.X, d.Y, features);
            }
            return new NetworkProblem(NetworkProblemCategory.Diagnostics, kind, message, d.X, d.Y, features);
        }).ToList();
    }

    internal static IReadOnlyList<NetworkProblem> ReadDuplicates(string json)
        => Deserialize<DuplicateRecord>(json).Select(d =>
        {
            var layer = d.Layer ?? "";
            return new NetworkProblem(NetworkProblemCategory.Duplicates, layer,
                $"{layer}#{d.DroppedOid} left out, {layer}#{d.KeptOid} kept", d.X, d.Y,
                [new NetworkProblemFeature(layer, d.DroppedOid, "dropped"), new NetworkProblemFeature(layer, d.KeptOid, "kept")]);
        }).ToList();

    internal static IReadOnlyList<NetworkProblem> ReadMerged(string json)
        => Deserialize<MergedRecord>(json).Select(m =>
        {
            string merged = m.MergedLayer ?? "", kept = m.KeptLayer ?? "";
            return new NetworkProblem(NetworkProblemCategory.Merged, $"{merged} → {kept}",
                $"{merged}#{m.MergedOid} merged into {kept}#{m.KeptOid}", m.X, m.Y,
                [new NetworkProblemFeature(merged, m.MergedOid, "merged"), new NetworkProblemFeature(kept, m.KeptOid, "kept")]);
        }).ToList();

    /// <summary>ConnectionReported → "Connection reported".</summary>
    private static string Readable(string kind)
        => string.Concat(kind.Select((c, i) => i > 0 && char.IsUpper(c) ? " " + char.ToLowerInvariant(c) : c.ToString()));

    /// <summary>"Layer #oid" (or "Layer#oid") sources as features; others (ids of id conflicts) are left out.</summary>
    private static List<NetworkProblemFeature> Features(List<string?>? sources, string? role)
    {
        var features = new List<NetworkProblemFeature>();
        if (sources is null)
        {
            return features;
        }
        for (int i = 0; i < sources.Count; i++)
        {
            if (ParseFeature(sources[i]) is (string layer, long oid))
            {
                features.Add(new NetworkProblemFeature(layer, oid, role ?? (i == 0 ? "node" : "line")));
            }
        }
        return features;
    }

    /// <summary>"Layer/Path #12" or "Layer/Path#12" → (layer, oid); null otherwise.</summary>
    internal static (string Layer, long Oid)? ParseFeature(string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return null;
        }
        int hash = source.LastIndexOf('#');
        if (hash <= 0 || !long.TryParse(source[(hash + 1)..].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var oid))
        {
            return null;
        }
        var layer = source[..hash].Trim();
        return layer.Length == 0 ? null : (layer, oid);
    }

    #endregion

    #region Messages

    private static readonly Regex NodeId = new(@"\bnode (\d+)", RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private static readonly Regex TypeId = new(@"\btype (\d+)\b", RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private static readonly Regex MergedList = new(@"(merged into the node [^:]+: )([\d, ]+)(\.?)$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>
    /// The network ids and type numbers of a builder message made readable with the metadata of the network:
    /// "the node 2251799813685248" → "the node Schacht#0", "type 3" → "type Wasser.Leitung". Idempotent.
    /// </summary>
    public static string ResolveMessage(string message, NetworkMetadata metadata)
        => ResolveMessage(message, metadata, metadata.Types.Count == 0 ? null : metadata.ToTypeRegistry());

    /// <summary>
    /// <see cref="ResolveMessage(string, NetworkMetadata)"/> with the type registry of the metadata made once for many
    /// messages (a build may report hundreds of thousands).
    /// </summary>
    public static string ResolveMessage(string message, NetworkMetadata metadata, SpatialNET.Network.TypeRegistry? types)
    {
        if (string.IsNullOrEmpty(message))
        {
            return message;
        }
        string Node(string text)
        {
            if (!long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
            {
                return text;
            }
            var (fc, oid, _) = NetworkIds.Decompose(id);
            if (fc == NetworkIds.HelperFeatureClass)
            {
                return "(line end)";
            }
            var layer = metadata.FeatureClasses.FirstOrDefault(f => f.Index == fc)?.Layer;
            return layer is null ? text : $"{layer}#{oid}";
        }

        message = MergedList.Replace(message, m =>
            m.Groups[1].Value + string.Join(", ", m.Groups[2].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(Node)) + m.Groups[3].Value);
        message = NodeId.Replace(message, m => "node " + Node(m.Groups[1].Value));
        if (types is not null)
        {
            message = TypeId.Replace(message, m =>
            {
                int id = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                try
                {
                    return "type " + (id == 0 ? "untyped" : types.Display(id));
                }
                catch (Exception)
                {
                    return m.Value;
                }
            });
        }
        return message;
    }

    #endregion

    #region Result

    /// <summary>
    /// The problems of a category as a result of the operation "problems", so Carto (and the server) can list, draw,
    /// select and download them like violations: Rule = the group, Kind = the category, Layer/Oid = the first feature
    /// (the dropped or merged point), EdgeLayer/EdgeOid = the second, <see cref="NetworkResultViolation.Features"/> all.
    /// </summary>
    public NetworkQueryResult ToResult(NetworkProblemCategory category, string? srs)
    {
        var problems = Of(category);
        var severity = Severity(category).ToString();
        var result = new NetworkQueryResult { Operation = NetworkQueryService.ProblemsOperation, Srs = srs };
        result.Messages.Add($"{category}: {problems.Count} problem{(problems.Count == 1 ? "" : "s")} ({FileName(category)}{(Written is DateTime w ? $", written {w:yyyy-MM-dd HH:mm}" : "")})");
        if (Describe(category.ToString()) is string description)
        {
            result.Messages.Add(description);
        }
        foreach (var group in problems.GroupBy(p => p.Group).OrderByDescending(g => g.Count()))
        {
            result.Messages.Add($"  {group.Count(),6}  {group.Key}");
        }
        if (!Present.Contains(category))
        {
            result.Messages.Add($"{FileName(category)} not found: {(category == NetworkProblemCategory.Conflicts ? "the last build had no conflicts" : "build the network first")}.");
        }

        foreach (var p in problems)
        {
            var first = p.Features.Count > 0 ? p.Features[0] : null;
            var second = p.Features.Count > 1 ? p.Features[1] : null;
            result.Violations.Add(new NetworkResultViolation(
                p.Group, severity, p.Message, category.ToString(),
                0, first?.Label ?? "", first?.Layer, first?.Oid,
                null, second?.Label, second?.Layer, second?.Oid,
                "", p.Features.Count, null, null, p.X, p.Y, p.Features.Select(f => f.Label).ToList())
            {
                Features = p.Features.Select(f => (f.Layer, f.Oid)).ToList()
            });
        }
        return result;
    }

    #endregion
}
