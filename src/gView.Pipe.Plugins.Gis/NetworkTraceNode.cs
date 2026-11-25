using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Query;
using gView.Pipe.Abstraction;
using gView.Pipe.Abstraction.Data;
using gView.Pipe.Abstraction.Nodes;
using gView.Pipe.Plugins.Gis.Extensions;
using Microsoft.Extensions.Logging;
using SpatialNET.Network;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis;

public enum GisNetworkOperation
{
    [PipeEnumValue("Every line gets the source (e.g. transformer) and the branch (e.g. connector) it hangs on; the sources are a type pattern, one run for all of them")]
    Feeders,

    [PipeEnumValue("Like Feeders, but the sources are the input items, e.g. the transformers of one area read with Read Features", Label = "Feeders of the input items")]
    FeedersOfItems,

    [PipeEnumValue("The lines reachable from the node of each input item, one trace per item", Label = "Trace from the input items")]
    Trace
}

public sealed class NetworkTraceParameters
{
    [Required, PipeCredential(GisNetworkCredential.TypeId)]
    [PipeParameter(Label = "Network", Order = 1, Expressions = false, Description = "gView Network credential: the folder of a network built with gView")]
    public string? CredentialId { get; set; }

    [PipeParameter(Label = "Operation", Order = 2, Expressions = false,
        Description = "Feeders labels every line with its source and branch; Trace follows the network from each input item")]
    public GisNetworkOperation Operation { get; set; } = GisNetworkOperation.Feeders;

    [PipeParameter(Label = "Sources", Order = 3, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["feeders"], Placeholder = "[MediumVoltage,Transformer,*]",
        Description = "The sources: a type pattern, e.g. [MediumVoltage,Transformer,*] for all transformers, or nodes such as Transformer#4711, comma separated; empty takes the sources of the tier")]
    public string Sources { get; set; } = "";

    [PipeParameter(Label = "Node", Order = 4, ShowWhen = nameof(Operation), ShowWhenValues = ["feedersOfItems", "trace"], Placeholder = "Transformer#{{ $json.id }}",
        Description = "The network node of each input item, as Layer#OID: e.g. Transformer#{{ $json.id }} for items read from the transformer layer, or {{ $json.properties.node }} for a property holding the reference")]
    public string Node { get; set; } = "";

    [PipeParameter(Label = "Branches", Order = 5, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["feeders", "feedersOfItems"], Placeholder = "[LowVoltage,Connector,*]",
        Description = "Type pattern of the edges at a source that start a branch, e.g. [LowVoltage,Connector,*]; empty: every edge at a source is a branch")]
    public string Branches { get; set; } = "";

    [PipeParameter(Label = "Tier", Order = 6, Expressions = false, Placeholder = "LowVoltage",
        Description = "Search only in this tier of the network (several comma separated), e.g. LowVoltage. With a tier the search stops at the sources of the tier")]
    public string Tier { get; set; } = "";

    [PipeParameter(Label = "Output types", Order = 7, Expressions = false, Placeholder = "[LowVoltage,Line,*]",
        Description = "Type pattern of the edges that become items, e.g. [LowVoltage,Line,*]; empty: every edge found (Feeders: but the branches themselves)")]
    public string OutputTypes { get; set; } = "";

    [PipeParameter(Label = "Group parts", Order = 8, Expressions = false,
        Description = "On: one item per line, the parts of a line split at nodes go together. Off: one item per part, with part")]
    public bool GroupParts { get; set; } = true;

    [PipeParameter(Label = "Open lines", Order = 9, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["feeders", "feedersOfItems"],
        Description = "Also the lines at a reached node that no branch went along, e.g. switched off at the separation point of a ring: kind Open, with the branches of their ends")]
    public bool OpenLines { get; set; }

    [PipeParameter(Label = "Source id field", Order = 10, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["feeders", "feedersOfItems"], Placeholder = "TRANSFORMER_ID",
        Description = "Property that gets the object id of the source, e.g. the transformer; for Multiple and Open the cheapest. Empty: not written")]
    public string SourceIdField { get; set; } = "";

    [PipeParameter(Label = "Branch id field", Order = 11, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["feeders", "feedersOfItems"], Placeholder = "BRANCH_ID",
        Description = "Property that gets the object id of the branch, e.g. the connector; for Multiple and Open the cheapest. Empty: not written")]
    public string BranchIdField { get; set; } = "";

    [PipeParameter(Label = "Source ids field", Order = 12, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["feeders", "feedersOfItems"], Placeholder = "TRANSFORMER_IDS",
        Description = "Property that gets the object ids of all sources as text, comma separated and cheapest first, e.g. 4711,4712 for a line of kind Multiple; one id for Edge. Empty: not written")]
    public string SourceIdsField { get; set; } = "";

    [PipeParameter(Label = "Branch ids field", Order = 13, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["feeders", "feedersOfItems"], Placeholder = "BRANCH_IDS",
        Description = "Property that gets the object ids of all branches as text, comma separated and cheapest first, e.g. 2,3 for a line of kind Multiple. Empty: not written")]
    public string BranchIdsField { get; set; } = "";

    [PipeParameter(Label = "Kind field", Order = 14, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["feeders", "feedersOfItems"], Placeholder = "SUPPLY_KIND",
        Description = "Property that gets the kind as text: Edge (one branch), Multiple (more than one: a closed ring, double supply) or Open (see Open lines). Empty: not written")]
    public string KindField { get; set; } = "";

    [PipeParameter(Label = "Source class field", Order = 15, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["feeders", "feedersOfItems"], Placeholder = "SOURCE_CLASS",
        Description = "Property that gets the class of the source: 0 to Classes−1, derived from its object id (id modulo Classes), the same in every run; e.g. for a renderer with a fixed number of colours. Empty: not written")]
    public string SourceClassField { get; set; } = "";

    [PipeParameter(Label = "Branch class field", Order = 16, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["feeders", "feedersOfItems"], Placeholder = "BRANCH_CLASS",
        Description = "Property that gets the class of the branch, like Source class field; neighbouring branches may get the same class. Empty: not written")]
    public string BranchClassField { get; set; } = "";

    [PipeParameter(Label = "Classes", Order = 17, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["feeders", "feedersOfItems"],
        Description = "Number of classes of the class fields, e.g. 100 for 0 to 99")]
    [Range(1, int.MaxValue)]
    public int Classes { get; set; } = 100;

    [PipeParameter(Label = "Multiple class", Order = 18, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["feeders", "feedersOfItems"], Placeholder = "997",
        Description = "Class of both class fields for a line of kind Multiple, e.g. 997 to show rings and double supply on the map; empty: the class of the cheapest branch")]
    public int? MultipleClass { get; set; }

    [PipeParameter(Label = "Open class", Order = 19, Expressions = false, ShowWhen = nameof(OpenLines), ShowWhenValues = ["true"],
        Description = "Class of both class fields for a line of kind Open, e.g. 998, so separation points stand out on the map")]
    public int OpenClass { get; set; } = 998;

    [PipeParameter(Label = "Only the output fields", Order = 20, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["feeders", "feedersOfItems"],
        Description = "The items keep only id and the fields named above, ready for Write Features (update by object id) without a Set node")]
    public bool OnlyOutputFields { get; set; }

    [PipeParameter(Label = "Filter", Order = 21, Expressions = false, Advanced = true, Placeholder = "[LowVoltage,*,*]",
        Description = "Only edges of this type pattern are walked; the tier already limits the edges to its own")]
    public string Filter { get; set; } = "";

    [PipeParameter(Label = "Block", Order = 22, Expressions = false, Advanced = true, Placeholder = "[*,Switch,*]",
        Description = "Nodes the search stops at, besides those blocked in the network: a type pattern or nodes such as Switch#12, comma separated")]
    public string Block { get; set; } = "";

    [PipeParameter(Label = "Unblock", Order = 23, Expressions = false, Advanced = true, Placeholder = "Switch#12",
        Description = "Blocked nodes opened for this run, e.g. an open switch closed for a what-if")]
    public string Unblock { get; set; } = "";

    [PipeParameter(Label = "Disable", Order = 24, Expressions = false, Advanced = true, Placeholder = "Line#4576",
        Description = "Edges switched off for this run: Layer#OID (the whole line) or Layer#OID/part, comma separated")]
    public string Disable { get; set; } = "";

    [PipeParameter(Label = "Stop at sources", Order = 25, Expressions = false, Advanced = true,
        Description = "With a tier the search stops at the sources of the tier (e.g. the transformers of the low voltage tier); off goes on through them")]
    public bool StopAtSources { get; set; } = true;

    [PipeParameter(Label = "Weight", Order = 26, Expressions = false, Advanced = true, Placeholder = "length",
        Description = "What cost and Max cost measure: length (default) or the name of a weight of network.json")]
    public string Weight { get; set; } = "";

    [PipeParameter(Label = "Max cost", Order = 27, Expressions = false, Advanced = true,
        Description = "The search ends at this cost, e.g. 500 for 500 m with the weight length; 0 has no limit")]
    [Range(0, double.MaxValue)]
    public double MaxCost { get; set; }

    [PipeParameter(Label = "Max items", Order = 28, Expressions = false, Advanced = true,
        Description = "The node fails if the result has more items, instead of cutting it; split the run (Feeders of the input items) or raise it")]
    [Range(1, int.MaxValue)]
    public int MaxItems { get; set; } = 100_000;
}

[PipeNode("f04bedfd-09c5-4720-a46e-af4d3f6f6ae7", "Network Trace",
    Category = "GIS",
    Description = "Searches a network built with gView GIS. Feeders labels every line with the source and branch it hangs on "
                  + "(e.g. transformer and connector); Trace follows the network from each input item. One item per line found, without geometry.")]
[PipeNodeExamples]
public sealed class NetworkTraceNode : IPipeNode<NetworkTraceParameters>
{
    public async Task<PipeNodeResult> ExecuteAsync(IPipeNodeContext context, CancellationToken cancellationToken)
    {
        var items = context.GetInput();
        var parameters = context.GetParameters<NetworkTraceParameters>(items.Count > 0 ? items[0] : null);
        if (string.IsNullOrEmpty(parameters.CredentialId))
        {
            throw new PipeException("Select a network");
        }
        var credential = await context.GetCredentialAsync<GisNetworkCredential>(parameters.CredentialId, cancellationToken);
        var directory = credential.BuiltDirectory();

        return await Task.Run(() =>
        {
            var log = new RunLog(context.Logger);
            var graph = NetworkGraphCache.Shared.Get(directory);
            log.Info($"Network {graph.Metadata.Name}: {graph.Metadata.NodeCount:N0} nodes, {graph.Metadata.EdgeCount:N0} edges; {parameters.Operation}"
                     + (parameters.Operation == GisNetworkOperation.Feeders ? "" : $" for {items.Count:N0} input item(s)"));
            var output = parameters.Operation switch
            {
                GisNetworkOperation.Trace => Trace(context, log, graph, parameters, items, cancellationToken),
                _ => Feeders(context, log, graph, parameters, items, cancellationToken)
            };
            log.Finish($"{output.Count:N0} item(s)");
            return PipeNodeResult.FromMain(output);
        }, cancellationToken);
    }

    static private List<PipeItem> Feeders(IPipeNodeContext context, RunLog log, FileNetworkGraph graph, NetworkTraceParameters parameters, IReadOnlyList<PipeItem> items, CancellationToken cancellationToken)
    {
        string? sources = parameters.Sources;
        if (parameters.Operation == GisNetworkOperation.FeedersOfItems)
        {
            var references = items.Select((item, i) => NodeReference(context, log, item, i, parameters)).OfType<string>().ToList();
            if (references.Count == 0)
            {
                log.Warning("No source among the input items");
                return [];
            }
            sources = string.Join(",", references);
        }

        var result = Run(log, graph, "feeders", Request(parameters) with { Sources = sources.TrimmedOrNull(), Branches = parameters.Branches.TrimmedOrNull(), OpenEdges = parameters.OpenLines }, cancellationToken);
        var outputTypes = string.IsNullOrWhiteSpace(parameters.OutputTypes)
            ? null
            : EdgeTypeNames(graph, parameters.OutputTypes);
        var branchTypes = outputTypes is null && !string.IsNullOrWhiteSpace(parameters.Branches)
            ? EdgeTypeNames(graph, parameters.Branches)
            : null;

        var edges = result.Edges
            .Where(e => e.Kind != "Boundary")
            .Where(e => outputTypes?.Contains(e.Type) ?? !(branchTypes?.Contains(e.Type) ?? false))
            .Select(e => new FoundEdge(e, e.Kind, e.Cost ?? 0, e.Source, e.Branch, null));
        return ToItems(graph, edges, parameters);
    }

    static private List<PipeItem> Trace(IPipeNodeContext context, RunLog log, FileNetworkGraph graph, NetworkTraceParameters parameters, IReadOnlyList<PipeItem> items, CancellationToken cancellationToken)
    {
        var outputTypes = string.IsNullOrWhiteSpace(parameters.OutputTypes) ? null : EdgeTypeNames(graph, parameters.OutputTypes);
        var found = new List<FoundEdge>();
        var watch = System.Diagnostics.Stopwatch.StartNew();
        for (var i = 0; i < items.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (i > 0)
            {
                var done = i;
                log.Progress(() =>
                {
                    var left = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds / (double)done * (items.Count - done));
                    return $"Trace {done:N0}/{items.Count:N0} ({100.0 * done / items.Count:0}%): {found.Count:N0} edge(s) found, {watch.Elapsed.ToDurationText()} so far, about {left.ToDurationText()} left";
                });
            }
            var start = NodeReference(context, log, items[i], i, parameters);
            if (start is null)
            {
                continue;
            }

            NetworkQueryResult result;
            try
            {
                result = new NetworkQueryService(graph).Trace(Request(parameters) with { Start = start }, cancellationToken: cancellationToken);
            }
            catch (ArgumentException ex)
            {
                log.Warning($"Item {i}: {ex.Message}");
                continue;
            }

            // the cost of an edge: the cost of its near end
            var costs = new Dictionary<long, double>();
            foreach (var node in result.Nodes)
            {
                costs.TryAdd(node.Id, node.Cost);
            }
            foreach (var edge in result.Edges.Where(e => e.Kind != "Boundary" && (outputTypes?.Contains(e.Type) ?? true)))
            {
                var cost = Math.Min(costs.GetValueOrDefault(edge.From, double.PositiveInfinity), costs.GetValueOrDefault(edge.To, double.PositiveInfinity));
                found.Add(new FoundEdge(edge, "Edge", double.IsFinite(cost) ? cost : 0, null, null, start));
            }
        }
        log.Info($"Trace: {items.Count:N0} item(s), {found.Count:N0} edge(s) found in {watch.Elapsed.ToDurationText()}");
        return ToItems(graph, found, parameters);
    }

    /// <summary>An edge of a result with what the node knows about it.</summary>
    private sealed record FoundEdge(NetworkResultEdge Edge, string Kind, double Cost, string? Source, string? Branch, string? Start);

    static private List<PipeItem> ToItems(FileNetworkGraph graph, IEnumerable<FoundEdge> edges, NetworkTraceParameters parameters)
    {
        var srs = graph.Metadata.Srs.ToEpsgCode();
        var groups = parameters.GroupParts
            ? edges.GroupBy(e => (e.Edge.Layer, e.Edge.Oid, e.Source, e.Branch, e.Start))
                   .Select(g => g.OrderBy(e => e.Cost).ToList())
            : edges.Select(e => new List<FoundEdge> { e });

        var output = new List<PipeItem>();
        foreach (var parts in groups)
        {
            var first = parts[0];
            var edge = first.Edge;
            var properties = new JsonObject
            {
                ["layer"] = edge.Layer,
                ["oid"] = ObjectId(edge.Oid),
            };
            if (!parameters.GroupParts)
            {
                properties["part"] = PartOf(edge.Label);
                properties["edgeId"] = edge.Id;
            }
            properties["type"] = edge.Type;
            var kind = parts.Any(p => p.Kind == "Multiple") ? "Multiple" : parts.All(p => p.Kind == "Open") ? "Open" : "Edge";
            properties["kind"] = kind;
            if (first.Start is not null)
            {
                properties["start"] = first.Start;
            }
            if (first.Source is not null)
            {
                properties["source"] = first.Source;
                properties["sourceOid"] = OidOf(first.Source);
                properties["branch"] = first.Branch;
                properties["branchOid"] = OidOf(first.Branch);
            }
            properties["cost"] = Math.Round(first.Cost, 3);
            properties["length"] = Math.Round(parts.Sum(p => p.Edge.Length), 3);
            properties["parts"] = parts.Count;
            if (first.Source is not null && HasOutputFields(parameters))
            {
                properties = OutputFields(parameters, properties, kind, first.Source, first.Branch);
            }

            output.Add(new PipeItem(new JsonObject
            {
                ["type"] = "Feature",
                // the object id, as Read Features gives it: Write Features updates by it (the layer is in the properties)
                ["id"] = ObjectId(edge.Oid),
                ["geometry"] = null,
                ["properties"] = properties,
                ["srs"] = srs
            }));
            if (output.Count > parameters.MaxItems)
            {
                throw TooMany(parameters);
            }
        }
        return output;
    }

    static private NetworkQueryResult Run(RunLog log, FileNetworkGraph graph, string operation, NetworkQueryRequest request, CancellationToken cancellationToken)
    {
        NetworkQueryResult result;
        try
        {
            result = new NetworkQueryService(graph).Run(operation, request, log, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            throw new PipeException(ex.Message);
        }
        foreach (var message in result.Messages)
        {
            log.Info(message);
        }
        return result;
    }

    static private NetworkQueryRequest Request(NetworkTraceParameters parameters) => new()
    {
        Tier = parameters.Tier.TrimmedOrNull(),
        Filter = parameters.Filter.TrimmedOrNull(),
        Block = parameters.Block.TrimmedOrNull(),
        Unblock = parameters.Unblock.TrimmedOrNull(),
        Disable = parameters.Disable.TrimmedOrNull(),
        StopAtSources = parameters.StopAtSources,
        Weight = parameters.Weight.TrimmedOrNull(),
        MaxCost = parameters.MaxCost > 0 ? parameters.MaxCost : null,
        WithEdges = true,
    };

    /// <summary>The node of an input item: Node evaluated for the item; null (with a warning) when it is empty.</summary>
    static private string? NodeReference(IPipeNodeContext context, RunLog log, PipeItem item, int index, NetworkTraceParameters parameters)
    {
        // only Node is evaluated per item, not every parameter (a script engine per parameter and item), and not at all
        // without {{ }}: thousands of items are usual here
        var template = context.RawParameters["node"] is JsonValue raw && raw.TryGetValue<string>(out var text) ? text : null;
        var node = index == 0 || template is null || !template.Contains("{{", StringComparison.Ordinal)
            ? parameters.Node
            : context.Evaluate(template, item, index)?.ToString();
        if (string.IsNullOrWhiteSpace(node))
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new PipeException("Set Node, the network node of each input item, e.g. Transformer#{{ $json.id }}");
            }
            log.Warning($"Item {index}: Node is empty");
            return null;
        }
        return node.Trim();
    }

    /// <summary>The names of the edge types of a pattern, as the result carries them.</summary>
    static private HashSet<string> EdgeTypeNames(FileNetworkGraph graph, string pattern)
    {
        TypeSet types;
        try
        {
            types = graph.Types.Edges(pattern);
        }
        catch (ArgumentException ex)
        {
            throw new PipeException(ex.Message);
        }
        return graph.Metadata.Types
            .Where(t => t.Kind == ElementKind.Edge && types.Contains(t.Id))
            .Select(t => graph.TypeName(t.Id))
            .ToHashSet();
    }

    /// <summary>The OID of the first Layer#OID of a label list ("Transformer#4711 Transformer#4712" → 4711).</summary>
    static private long? OidOf(string? labels)
    {
        var first = labels?.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        int hash = first?.LastIndexOf('#') ?? -1;
        return hash >= 0 && long.TryParse(first![(hash + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var oid) ? oid : null;
    }

    /// <summary>The OIDs of a label list as text, in its order ("Transformer#4711 Transformer#4712" → "4711,4712").</summary>
    static private string? OidsOf(string? labels)
    {
        var oids = (labels ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(OidOf)
            .OfType<long>()
            .Distinct()
            .Select(oid => oid.ToString(CultureInfo.InvariantCulture))
            .ToList();
        return oids.Count == 0 ? null : string.Join(",", oids);
    }

    static private int PartOf(string label)
    {
        int slash = label.LastIndexOf('/');
        return slash >= 0 && int.TryParse(label[(slash + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var part) ? part : 0;
    }

    /// <summary>gView's object ids are int: as int, Write Features and expressions read them like those of Read Features.</summary>
    static private JsonNode ObjectId(long oid) => oid is >= int.MinValue and <= int.MaxValue ? JsonValue.Create((int)oid) : JsonValue.Create(oid);

    static private bool HasOutputFields(NetworkTraceParameters parameters)
        => new[] { parameters.SourceIdField, parameters.BranchIdField, parameters.SourceIdsField, parameters.BranchIdsField,
                   parameters.KindField, parameters.SourceClassField, parameters.BranchClassField }
            .Any(name => !string.IsNullOrWhiteSpace(name));

    /// <summary>
    /// The fields named in the parameters: ids of the (cheapest) source and branch, the kind, and their classes (id modulo
    /// Classes; Multiple class and Open class for those kinds). With Only the output fields nothing else.
    /// </summary>
    static private JsonObject OutputFields(NetworkTraceParameters parameters, JsonObject properties, string kind, string? sources, string? branches)
    {
        var sourceOid = OidOf(sources);
        var branchOid = OidOf(branches);
        var target = parameters.OnlyOutputFields ? new JsonObject() : properties;
        var classes = Math.Max(1, parameters.Classes);
        int? fixedClass = kind switch
        {
            "Open" => parameters.OpenClass,
            "Multiple" => parameters.MultipleClass,
            _ => null
        };
        JsonNode? Class(long? oid) => fixedClass is int value ? value : oid is long id ? ObjectId(id % classes) : null;

        void Set(string name, JsonNode? value)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                target[name.Trim()] = value;
            }
        }
        Set(parameters.SourceIdField, sourceOid is long source ? ObjectId(source) : null);
        Set(parameters.BranchIdField, branchOid is long branch ? ObjectId(branch) : null);
        Set(parameters.SourceIdsField, OidsOf(sources));
        Set(parameters.BranchIdsField, OidsOf(branches));
        Set(parameters.KindField, kind);
        Set(parameters.SourceClassField, Class(sourceOid));
        Set(parameters.BranchClassField, Class(branchOid));
        return target;
    }

    /// <summary>
    /// The log of a run, which is the live output in the editor: progress at most every 2 seconds (the run log keeps about
    /// 1,000 lines) and the first 20 warnings, then only their number.
    /// </summary>
    private sealed class RunLog(ILogger logger) : IProgress<string>
    {
        private const int ProgressMilliseconds = 2000, MaxWarnings = 20;
        private long _last = Environment.TickCount64;
        private int _warnings;

        public void Info(string message) => logger.LogInformation("{Message}", message);

        /// <summary>A progress line, built only when it is due.</summary>
        public void Progress(Func<string> message)
        {
            long now = Environment.TickCount64;
            if (now - _last >= ProgressMilliseconds)
            {
                _last = now;
                logger.LogInformation("{Message}", message());
            }
        }

        /// <summary>The progress lines of the network search itself.</summary>
        public void Report(string value) => Progress(() => value);

        public void Warning(string message)
        {
            if (++_warnings <= MaxWarnings)
            {
                logger.LogWarning("{Message}", message);
            }
            else if (_warnings == MaxWarnings + 1)
            {
                logger.LogWarning("More warnings are left out; their number follows at the end");
            }
        }

        public void Finish(string message)
            => logger.LogInformation("{Message}", _warnings > MaxWarnings ? $"{message}, {_warnings:N0} warning(s)" : message);
    }

    static private PipeException TooMany(NetworkTraceParameters parameters)
        => new($"The result has more than {parameters.MaxItems} items (Max items); split the run, e.g. with Sources from items, or raise Max items");
}
