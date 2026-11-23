using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Query;
using gView.Pipe.Abstraction;
using gView.Pipe.Abstraction.Data;
using gView.Pipe.Abstraction.Nodes;
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

    [PipeParameter(Label = "Sources", Order = 3, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["feeders"], Placeholder = "[Mittelspannung,Trafo,*]",
        Description = "The sources: a type pattern, e.g. [Mittelspannung,Trafo,*] for all transformers, or nodes such as Trafo#4711, comma separated; empty takes the sources of the tier")]
    public string Sources { get; set; } = "";

    [PipeParameter(Label = "Node", Order = 4, ShowWhen = nameof(Operation), ShowWhenValues = ["feedersOfItems", "trace"], Placeholder = "Trafo#{{ $json.id }}",
        Description = "The network node of each input item, as Layer#OID: e.g. Trafo#{{ $json.id }} for items read from the transformer layer, or {{ $json.properties.node }} for a property holding the reference")]
    public string Node { get; set; } = "";

    [PipeParameter(Label = "Branches", Order = 5, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["feeders", "feedersOfItems"], Placeholder = "[Niederspannung,Anlagenverbinder,*]",
        Description = "Type pattern of the edges at a source that start a branch, e.g. [Niederspannung,Anlagenverbinder,*]; empty: every edge at a source is a branch")]
    public string Branches { get; set; } = "";

    [PipeParameter(Label = "Tier", Order = 6, Expressions = false, Placeholder = "Niederspannung",
        Description = "Search only in this tier (an Ebene of network.json; several comma separated), e.g. Niederspannung. With a tier the search stops at the sources of the tier")]
    public string Tier { get; set; } = "";

    [PipeParameter(Label = "Output types", Order = 7, Expressions = false, Placeholder = "[Niederspannung,Leitung,*]",
        Description = "Type pattern of the edges that become items, e.g. [Niederspannung,Leitung,*]; empty: every edge found (Feeders: but the branches themselves)")]
    public string OutputTypes { get; set; } = "";

    [PipeParameter(Label = "Group parts", Order = 8, Expressions = false,
        Description = "On: one item per line, the parts of a line split at nodes go together. Off: one item per part, with part")]
    public bool GroupParts { get; set; } = true;

    [PipeParameter(Label = "Open lines", Order = 9, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["feeders", "feedersOfItems"],
        Description = "Also the lines at a reached node that no branch went along, e.g. switched off at a separation point of a ring, with kind Open and the branches of their ends")]
    public bool OpenLines { get; set; }

    [PipeParameter(Label = "Colour fields", Order = 10, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["feeders", "feedersOfItems"],
        Description = "Adds a colour class for the source and for the branch of each line (object id modulo Colours), e.g. for a map coloured by transformer and branch")]
    public bool ColorFields { get; set; }

    [PipeParameter(Label = "Colours", Order = 11, Expressions = false, ShowWhen = nameof(ColorFields), ShowWhenValues = ["true"],
        Description = "Number of colour classes: the fields get 0 … Colours−1, e.g. 100 for 0 to 99")]
    [Range(1, int.MaxValue)]
    public int Colors { get; set; } = 100;

    [PipeParameter(Label = "Source colour field", Order = 12, Expressions = false, ShowWhen = nameof(ColorFields), ShowWhenValues = ["true"], Placeholder = "STAT_COLOR",
        Description = "Name of the property with the colour class of the source, e.g. STAT_COLOR, the field of the line feature class")]
    public string SourceColorField { get; set; } = "sourceColor";

    [PipeParameter(Label = "Branch colour field", Order = 13, Expressions = false, ShowWhen = nameof(ColorFields), ShowWhenValues = ["true"], Placeholder = "ABZW_COLOR",
        Description = "Name of the property with the colour class of the branch, e.g. ABZW_COLOR")]
    public string BranchColorField { get; set; } = "branchColor";

    [PipeParameter(Label = "Open colour", Order = 14, Expressions = false, ShowWhen = nameof(ColorFields), ShowWhenValues = ["true"],
        Description = "Both colour fields of an open line (kind Open, see Open lines) get this value, e.g. 998, so separation points stand out on the map")]
    public int OpenColor { get; set; } = 998;

    [PipeParameter(Label = "Only the colour fields", Order = 15, Expressions = false, ShowWhen = nameof(ColorFields), ShowWhenValues = ["true"],
        Description = "The items keep only id and the two colour fields, ready for Write Features (update by object id) without a Set node")]
    public bool ColorFieldsOnly { get; set; }

    [PipeParameter(Label = "Filter", Order = 21, Expressions = false, Advanced = true, Placeholder = "[Niederspannung,*,*]",
        Description = "Only edges of this type pattern are walked; the tier already limits the edges to its own")]
    public string Filter { get; set; } = "";

    [PipeParameter(Label = "Block", Order = 22, Expressions = false, Advanced = true, Placeholder = "[*,Trenner,*]",
        Description = "Nodes the search stops at, besides those blocked in the network: a type pattern or nodes such as Schalter#12, comma separated")]
    public string Block { get; set; } = "";

    [PipeParameter(Label = "Unblock", Order = 23, Expressions = false, Advanced = true, Placeholder = "Schalter#12",
        Description = "Blocked nodes opened for this run, e.g. an open switch closed for a what-if")]
    public string Unblock { get; set; } = "";

    [PipeParameter(Label = "Disable", Order = 24, Expressions = false, Advanced = true, Placeholder = "Leitung#4576",
        Description = "Edges switched off for this run: Layer#OID (the whole line) or Layer#OID/part, comma separated")]
    public string Disable { get; set; } = "";

    [PipeParameter(Label = "Stop at sources", Order = 25, Expressions = false, Advanced = true,
        Description = "With a tier the search stops at the sources of the tier (the transformers of Niederspannung); off goes on through them")]
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

        var result = Run(log, graph, "feeders", Request(parameters) with { Sources = Empty(sources), Branches = Empty(parameters.Branches), OpenEdges = parameters.OpenLines }, cancellationToken);
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
                    return $"Trace {done:N0}/{items.Count:N0} ({100.0 * done / items.Count:0}%): {found.Count:N0} edge(s) found, {Duration(watch.Elapsed)} so far, about {Duration(left)} left";
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
        log.Info($"Trace: {items.Count:N0} item(s), {found.Count:N0} edge(s) found in {Duration(watch.Elapsed)}");
        return ToItems(graph, found, parameters);
    }

    /// <summary>An edge of a result with what the node knows about it.</summary>
    private sealed record FoundEdge(NetworkResultEdge Edge, string Kind, double Cost, string? Source, string? Branch, string? Start);

    static private List<PipeItem> ToItems(FileNetworkGraph graph, IEnumerable<FoundEdge> edges, NetworkTraceParameters parameters)
    {
        var srs = Epsg(graph.Metadata.Srs);
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
            if (parameters.ColorFields && first.Source is not null)
            {
                var colors = Math.Max(1, parameters.Colors);
                var colored = parameters.ColorFieldsOnly ? new JsonObject() : properties;
                if (kind == "Open")
                {
                    colored[FieldName(parameters.SourceColorField, "sourceColor")] = parameters.OpenColor;
                    colored[FieldName(parameters.BranchColorField, "branchColor")] = parameters.OpenColor;
                }
                else
                {
                    colored[FieldName(parameters.SourceColorField, "sourceColor")] = OidOf(first.Source) is long sourceOid ? sourceOid % colors : null;
                    colored[FieldName(parameters.BranchColorField, "branchColor")] = OidOf(first.Branch) is long branchOid ? branchOid % colors : null;
                }
                properties = colored;
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
        Tier = Empty(parameters.Tier),
        Filter = Empty(parameters.Filter),
        Block = Empty(parameters.Block),
        Unblock = Empty(parameters.Unblock),
        Disable = Empty(parameters.Disable),
        StopAtSources = parameters.StopAtSources,
        Weight = Empty(parameters.Weight),
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
                throw new PipeException("Set Node, the network node of each input item, e.g. Trafo#{{ $json.id }}");
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

    /// <summary>The OID of the first Layer#OID of a label list ("Trafo#4711 Trafo#4712" → 4711).</summary>
    static private long? OidOf(string? labels)
    {
        var first = labels?.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        int hash = first?.LastIndexOf('#') ?? -1;
        return hash >= 0 && long.TryParse(first![(hash + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var oid) ? oid : null;
    }

    static private int PartOf(string label)
    {
        int slash = label.LastIndexOf('/');
        return slash >= 0 && int.TryParse(label[(slash + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var part) ? part : 0;
    }

    static private int? Epsg(string? srs)
    {
        if (string.IsNullOrEmpty(srs))
        {
            return null;
        }
        var code = srs[(srs.LastIndexOf(':') + 1)..];
        return int.TryParse(code, NumberStyles.Integer, CultureInfo.InvariantCulture, out var epsg) ? epsg : null;
    }

    /// <summary>gView's object ids are int: as int, Write Features and expressions read them like those of Read Features.</summary>
    static private JsonNode ObjectId(long oid) => oid is >= int.MinValue and <= int.MaxValue ? JsonValue.Create((int)oid) : JsonValue.Create(oid);

    static private string FieldName(string? name, string fallback) => string.IsNullOrWhiteSpace(name) ? fallback : name.Trim();

    static private string Duration(TimeSpan time)
        => time.TotalHours >= 1 ? $"{(int)time.TotalHours} h {time.Minutes} min"
         : time.TotalMinutes >= 1 ? $"{(int)time.TotalMinutes} min {time.Seconds} s"
         : $"{time.TotalSeconds:0.#} s";

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

    static private string? Empty(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    static private PipeException TooMany(NetworkTraceParameters parameters)
        => new($"The result has more than {parameters.MaxItems} items (Max items); split the run, e.g. with Sources from items, or raise Max items");
}
