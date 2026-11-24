using gView.Framework.NetworkModel.Query;
using gView.Framework.Symbology.Palettes;
using gView.GraphicsEngine;
using System.Globalization;

namespace gView.Carto.Razor.Components.Tools.Context;

/// <summary>What a legend entry draws: points, lines or rule violations.</summary>
public enum NetworkLegendShape
{
    Node,
    Edge,
    Violation
}

/// <summary>feeders: what the colours of the edges stand for.</summary>
public enum NetworkFeederColoring
{
    Branch,
    Source
}

/// <summary>
/// One row of the legend of a result: the elements of one kind (nodes, edges or violations), or for feeders the edges of
/// one branch. Every element of the result belongs to exactly one entry, which gives its colour in the map.
/// </summary>
public sealed class NetworkLegendEntry
{
    /// <summary>Stays the same across reruns (kind or source/branch), so hidden entries stay hidden.</summary>
    public required string Key { get; init; }

    public required string Name { get; init; }

    /// <summary>feeders: the source (feeder) the branch belongs to; null for the summary rows.</summary>
    public string? Group { get; init; }

    public required NetworkLegendShape Shape { get; init; }

    public ArgbColor Color { get; init; }

    /// <summary>Points: the size of the marker; lines: the width.</summary>
    public float Size { get; init; }

    public bool Dashed { get; init; }

    /// <summary>Indexes into <see cref="NetworkQueryResult.Nodes"/>, merged points (aliases) included.</summary>
    public List<int> Nodes { get; } = [];

    /// <summary>Indexes into <see cref="NetworkQueryResult.Edges"/>.</summary>
    public List<int> Edges { get; } = [];

    /// <summary>Indexes into <see cref="NetworkQueryResult.Violations"/>.</summary>
    public List<int> Violations { get; } = [];

    /// <summary>How many elements the row stands for (merged points not counted, they are drawn with their node).</summary>
    public int Count { get; set; }

    /// <summary>Edges: their length together.</summary>
    public double Length { get; set; }

    public string CssColor => $"rgba({Color.R},{Color.G},{Color.B},{(Color.A / 255.0).ToString("0.##", CultureInfo.InvariantCulture)})";
}

/// <summary>An element of a legend row in the report: label, detail (type, rule) and where it is (network coordinates).</summary>
public sealed record NetworkLegendItem(string Label, string? Detail, (double X, double Y)[] Points);

/// <summary>
/// The legend of a network search result: one entry per kind of node, edge and violation that occurs, each with a colour
/// of its own (supply: Supplied green, Multiple amber, NotSupplied red and larger, ...). feeders: one entry per branch,
/// grouped by source, coloured by branch or by source from a qualitative palette, plus Multiple (red) and Open (grey, dashed).
/// </summary>
public sealed class NetworkResultLegend
{
    public static readonly ArgbColor EdgeColor = ArgbColor.FromArgb(200, 0, 120, 255);
    public static readonly ArgbColor NodeColor = ArgbColor.FromArgb(230, 0, 90, 200);
    public static readonly ArgbColor ErrorColor = ArgbColor.FromArgb(230, 220, 30, 30);
    public static readonly ArgbColor WarningColor = ArgbColor.FromArgb(230, 255, 170, 0);

    /// <summary>Colour and size per node kind; kinds not listed get the default node style.</summary>
    private static readonly Dictionary<string, (ArgbColor Color, float Size)> NodeStyles = new()
    {
        ["Start"] = (ArgbColor.FromArgb(230, 0, 140, 70), 10),
        ["From"] = (ArgbColor.FromArgb(230, 0, 140, 70), 10),
        ["Source"] = (ArgbColor.FromArgb(230, 0, 140, 70), 10),
        ["To"] = (ArgbColor.FromArgb(230, 130, 40, 180), 10),
        ["Via"] = (NodeColor, 7),
        ["Reached"] = (NodeColor, 7),
        ["Node"] = (ArgbColor.FromArgb(230, 0, 140, 70), 10),
        ["Neighbor"] = (NodeColor, 7),
        ["Nearest"] = (NodeColor, 8),
        ["OtherSource"] = (ArgbColor.FromArgb(230, 0, 160, 160), 9),
        ["Blocked"] = (ArgbColor.FromArgb(230, 40, 40, 40), 9),
        ["Supplied"] = (ArgbColor.FromArgb(230, 40, 170, 60), 7),
        ["Multiple"] = (WarningColor, 7),
        ["NotSupplied"] = (ErrorColor, 11),
        ["Affected"] = (ErrorColor, 10),
        ["NoBranch"] = (ErrorColor, 10),
    };

    /// <summary>Colour, width and dash per edge kind.</summary>
    private static readonly Dictionary<string, (ArgbColor Color, float Width, bool Dashed)> EdgeStyles = new()
    {
        ["Edge"] = (EdgeColor, 5, false),
        ["Boundary"] = (ArgbColor.FromArgb(200, 150, 60, 200), 5, true),
        ["Disabled"] = (ArgbColor.FromArgb(230, 40, 40, 40), 6, false),
        ["Dead"] = (ArgbColor.FromArgb(220, 255, 150, 0), 5, false),
        ["Multiple"] = (ErrorColor, 6, false),
        ["Open"] = (ArgbColor.FromArgb(200, 140, 140, 140), 4, true),
    };

    /// <summary>The order of the rows: the ones that matter first, the rest by name.</summary>
    private static readonly string[] Order =
        ["Start", "From", "Source", "Node", "To", "NotSupplied", "Affected", "NoBranch", "Multiple", "Supplied", "Blocked", "OtherSource", "Via", "Reached", "Neighbor", "Nearest", "Edge", "Disabled", "Dead", "Open", "Boundary"];

    /// <summary>The qualitative palettes for feeders (from the class renderers' Recolor).</summary>
    public static IEnumerable<string> FeederPalettes => ColorPalettes.All.Where(p => p.Type == ColorPaletteType.Qualitative).Select(p => p.Name);

    public const string DefaultFeederPalette = "Set1";

    public NetworkQueryResult Result { get; }

    public IReadOnlyList<NetworkLegendEntry> Entries { get; }

    /// <summary>feeders: the sources (groups of branch rows) in the order of the legend.</summary>
    public IReadOnlyList<string> Groups { get; }

    private readonly NetworkLegendEntry?[] _nodeEntries;
    private readonly NetworkLegendEntry?[] _edgeEntries;
    private readonly NetworkLegendEntry?[] _violationEntries;

    private NetworkResultLegend(NetworkQueryResult result, List<NetworkLegendEntry> entries, List<string> groups)
    {
        Result = result;
        Entries = entries;
        Groups = groups;
        _nodeEntries = new NetworkLegendEntry?[result.Nodes.Count];
        _edgeEntries = new NetworkLegendEntry?[result.Edges.Count];
        _violationEntries = new NetworkLegendEntry?[result.Violations.Count];
        foreach (var entry in entries)
        {
            entry.Nodes.ForEach(i => _nodeEntries[i] = entry);
            entry.Edges.ForEach(i => _edgeEntries[i] = entry);
            entry.Violations.ForEach(i => _violationEntries[i] = entry);
        }
    }

    public NetworkLegendEntry? NodeEntry(int index) => _nodeEntries[index];
    public NetworkLegendEntry? EdgeEntry(int index) => _edgeEntries[index];
    public NetworkLegendEntry? ViolationEntry(int index) => _violationEntries[index];

    /// <summary>The entries a key of a row stands for: the entry itself, or every branch of a source (<see cref="GroupKey"/>).</summary>
    public IReadOnlyList<NetworkLegendEntry> Find(string key)
        => key.StartsWith(GroupPrefix, StringComparison.Ordinal)
            ? Entries.Where(e => e.Group == key[GroupPrefix.Length..]).ToList()
            : Entries.Where(e => e.Key == key).ToList();

    /// <summary>
    /// The elements of an entry for the report, each with its places in the spatial reference of the network: nodes
    /// (merged points left out), edges (both ends), violations.
    /// </summary>
    public IReadOnlyList<NetworkLegendItem> Items(NetworkLegendEntry entry)
    {
        var items = new List<NetworkLegendItem>();
        foreach (var node in entry.Nodes.Select(i => Result.Nodes[i]).Where(n => n.AliasOf.Length == 0))
        {
            items.Add(new NetworkLegendItem(node.Label, node.Type, [(node.X, node.Y)]));
        }
        foreach (var edge in entry.Edges.Select(i => Result.Edges[i]))
        {
            items.Add(new NetworkLegendItem(edge.Label, $"{edge.Type}, {edge.Length.ToString("N1", CultureInfo.InvariantCulture)}", [(edge.FromX, edge.FromY), (edge.ToX, edge.ToY)]));
        }
        foreach (var violation in entry.Violations.Select(i => Result.Violations[i]))
        {
            items.Add(new NetworkLegendItem(violation.EdgeLabel ?? violation.NodeLabel, violation.Rule, [(violation.X, violation.Y)]));
        }
        return items;
    }

    /// <summary>The features (layer, oid) of entries: nodes with their merged points, edges, the nodes and edges of violations; helper nodes have none.</summary>
    public Dictionary<string, HashSet<long>> ObjectIdsByLayer(IEnumerable<NetworkLegendEntry> entries)
    {
        var ids = new Dictionary<string, HashSet<long>>(StringComparer.OrdinalIgnoreCase);
        void Add(string? layer, long? oid)
        {
            if (layer is null || oid is null)
            {
                return;
            }
            if (!ids.TryGetValue(layer, out var set))
            {
                ids[layer] = set = [];
            }
            set.Add(oid.Value);
        }
        foreach (var entry in entries)
        {
            entry.Nodes.ForEach(i => Add(Result.Nodes[i].Layer, Result.Nodes[i].Oid));
            entry.Edges.ForEach(i => Add(Result.Edges[i].Layer, Result.Edges[i].Oid));
            foreach (var violation in entry.Violations.Select(i => Result.Violations[i]))
            {
                Add(violation.Layer, violation.Oid);
                Add(violation.EdgeLayer, violation.EdgeOid);
            }
        }
        return ids;
    }

    private const string GroupPrefix = "group:";

    public static string GroupKey(string group) => GroupPrefix + group;

    public static NetworkResultLegend Build(NetworkQueryResult result, NetworkFeederColoring coloring = NetworkFeederColoring.Branch, string? palette = null)
    {
        var entries = new Dictionary<string, NetworkLegendEntry>();
        var groups = new List<string>();
        NetworkLegendEntry Entry(string key, Func<NetworkLegendEntry> create)
        {
            if (!entries.TryGetValue(key, out var entry))
            {
                entries[key] = entry = create();
            }
            return entry;
        }

        for (int i = 0; i < result.Nodes.Count; i++)
        {
            var node = result.Nodes[i];
            var style = NodeStyles.TryGetValue(node.Kind, out var s) ? s : (NodeColor, 7);
            var entry = Entry("node:" + node.Kind, () => new NetworkLegendEntry
            {
                Key = "node:" + node.Kind,
                Name = Readable(node.Kind),
                Shape = NetworkLegendShape.Node,
                Color = style.Item1,
                Size = style.Item2
            });
            entry.Nodes.Add(i);
            if (node.AliasOf.Length == 0)
            {
                entry.Count++;
            }
        }

        bool feeders = result.Operation == "feeders";
        var branchEdges = new List<int>();
        for (int i = 0; i < result.Edges.Count; i++)
        {
            var edge = result.Edges[i];
            if (feeders && edge.Kind == "Edge" && !string.IsNullOrEmpty(edge.Branch))
            {
                branchEdges.Add(i);
                continue;
            }
            var style = EdgeStyles.TryGetValue(edge.Kind, out var s) ? s : (EdgeColor, 5, false);
            var entry = Entry("edge:" + edge.Kind, () => new NetworkLegendEntry
            {
                Key = "edge:" + edge.Kind,
                Name = edge.Kind == "Edge" ? "Edges" : Readable(edge.Kind) + (feeders && edge.Kind == "Multiple" ? " (several branches)" : ""),
                Shape = NetworkLegendShape.Edge,
                Color = style.Item1,
                Size = style.Item2,
                Dashed = style.Item3
            });
            entry.Edges.Add(i);
            entry.Count++;
            entry.Length += edge.Length;
        }

        for (int i = 0; i < result.Violations.Count; i++)
        {
            var severity = result.Violations[i].Severity;
            var entry = Entry("violation:" + severity, () => new NetworkLegendEntry
            {
                Key = "violation:" + severity,
                Name = $"Violations ({severity})",
                Shape = NetworkLegendShape.Violation,
                Color = severity == "Error" ? ErrorColor : WarningColor,
                Size = 11
            });
            entry.Violations.Add(i);
            entry.Count++;
        }

        var ordered = entries.Values
            .OrderBy(e => e.Shape == NetworkLegendShape.Violation ? 0 : 1)
            .ThenBy(e => Rank(e.Key[(e.Key.IndexOf(':') + 1)..]))
            .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (branchEdges.Count > 0)
        {
            var colors = (ColorPalettes.Find(palette) ?? ColorPalettes.Find(DefaultFeederPalette)!).Colors;
            ArgbColor Color(int index) => ArgbColor.FromArgb(220, colors[index % colors.Count]);

            var bySource = branchEdges
                .GroupBy(i => result.Edges[i].Source!)
                .OrderBy(g => g.Key, NaturalComparer.Instance)
                .ToList();
            var branchRows = new List<NetworkLegendEntry>();
            for (int s = 0; s < bySource.Count; s++)
            {
                var source = bySource[s];
                groups.Add(source.Key);
                var byBranch = source.GroupBy(i => result.Edges[i].Branch!).OrderBy(g => g.Key, NaturalComparer.Instance).ToList();
                for (int b = 0; b < byBranch.Count; b++)
                {
                    var entry = new NetworkLegendEntry
                    {
                        Key = $"branch:{source.Key}|{byBranch[b].Key}",
                        Name = byBranch[b].Key,
                        Group = source.Key,
                        Shape = NetworkLegendShape.Edge,
                        Color = coloring == NetworkFeederColoring.Source ? Color(s) : Color(b),
                        Size = 5
                    };
                    entry.Edges.AddRange(byBranch[b]);
                    entry.Count = entry.Edges.Count;
                    entry.Length = entry.Edges.Sum(i => result.Edges[i].Length);
                    branchRows.Add(entry);
                }
            }
            // the summary rows (sources, Multiple, Open, Boundary) first, then the branches by source
            ordered.AddRange(branchRows);
        }

        return new NetworkResultLegend(result, ordered, groups);
    }

    private static int Rank(string kind)
    {
        int index = Array.IndexOf(Order, kind);
        return index < 0 ? Order.Length : index;
    }

    /// <summary>NotSupplied → "Not supplied".</summary>
    public static string Readable(string kind)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var c in kind)
        {
            if (char.IsUpper(c) && sb.Length > 0)
            {
                sb.Append(' ').Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    /// <summary>Layer#9 before Layer#10.</summary>
    private sealed class NaturalComparer : IComparer<string>
    {
        public static readonly NaturalComparer Instance = new();

        public int Compare(string? x, string? y)
        {
            if (x is null || y is null)
            {
                return string.Compare(x, y, StringComparison.Ordinal);
            }
            int hx = x.LastIndexOf('#'), hy = y.LastIndexOf('#');
            if (hx >= 0 && hy >= 0
                && string.Equals(x[..hx], y[..hy], StringComparison.OrdinalIgnoreCase)
                && long.TryParse(x[(hx + 1)..], out var a) && long.TryParse(y[(hy + 1)..], out var b))
            {
                return a.CompareTo(b);
            }
            return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
        }
    }
}
