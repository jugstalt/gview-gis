using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Ids;
using gView.Framework.NetworkModel.Store;
using gView.Framework.NetworkModel.Validation;
using SpatialNET.Network;
using System.Globalization;

namespace gView.Framework.NetworkModel.Query;

/// <summary>
/// The searches on a built network (trace, reach, path, supply, info, validate) for gView.Cmd, Carto and the server.
/// Thread-safe as long as the graph is: every call keeps its own state.
/// </summary>
public sealed class NetworkQueryService
{
    public static readonly IReadOnlyList<string> Operations = ["trace", "reach", "path", "supply", "info", "validate"];

    private readonly FileNetworkGraph _graph;

    public NetworkQueryService(FileNetworkGraph graph) => _graph = graph;

    public FileNetworkGraph Graph => _graph;

    public NetworkQueryResult Run(string operation, NetworkQueryRequest request, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
        => operation.ToLowerInvariant() switch
        {
            "trace" => Trace(request),
            "reach" => Reach(request),
            "path" => Path(request),
            "supply" => Supply(request, progress),
            "info" => Info(request),
            "validate" => Validate(request, progress, cancellationToken),
            _ => throw new ArgumentException($"Unknown operation '{operation}'. Operations: {string.Join(", ", Operations)}")
        };

    #region Operations

    public NetworkQueryResult Trace(NetworkQueryRequest request)
    {
        var result = NewResult("trace");
        var sources = _graph.ResolveNodes(Required(request.Start, "start"));
        result.Messages.Add($"Trace from {sources.Count} source(s)");

        var options = Options(request);
        var hits = _graph.Trace(sources, options).ToList();
        foreach (var group in hits.GroupBy(h => h.Kind).OrderBy(g => g.Key))
        {
            result.Messages.Add($"  {group.Key,-12} {group.Count()}");
        }
        AddTypes(result, hits.Where(h => h.Kind == TraceHitKind.Reached).Select(h => h.TypeId));

        foreach (var hit in hits)
        {
            AddNode(result, hit.Node, hit.TypeId, hit.Cost, hit.Kind.ToString(), _graph.NodeLabel(hit.Source));
        }
        if (request.WithEdges)
        {
            foreach (var edge in _graph.TraceEdges(sources, options).DistinctBy(e => e.Edge.Id))
            {
                AddEdge(result, edge.Edge.Id, edge.Edge.From);
            }
        }
        return result;
    }

    public NetworkQueryResult Reach(NetworkQueryRequest request)
    {
        var result = NewResult("reach");
        var starts = _graph.ResolveNodes(Required(request.Start, "start"));
        var options = Options(request);
        result.Messages.Add($"Reach from {starts.Count} start(s), max cost {options.MaxCost}, weight {options.Weight.Name}");

        var reached = _graph.Reach(starts, options).ToList();
        result.Messages.Add($"  reached {reached.Count} node(s), max cost {(reached.Count > 0 ? reached.Max(r => r.Cost) : 0):0.###}");
        AddTypes(result, reached.Select(r => r.TypeId));

        foreach (var r in reached)
        {
            AddNode(result, r.Node, r.TypeId, r.Cost, r.Blocked ? "Blocked" : r.IsStart ? "Start" : "Reached", _graph.NodeLabel(r.Start));
        }
        if (request.WithEdges)
        {
            foreach (var r in reached.Where(r => !r.IsStart).DistinctBy(r => r.Via))
            {
                AddEdge(result, r.Via, r.Node);
            }
        }
        return result;
    }

    public NetworkQueryResult Path(NetworkQueryRequest request)
    {
        var result = NewResult("path");
        var from = _graph.ResolveNode(Required(request.From, "from"));
        var to = _graph.ResolveNode(Required(request.To, "to"));
        var options = Options(request);

        var path = _graph.ShortestPath(from, to, options);
        if (path is null)
        {
            result.Messages.Add($"No path from {_graph.NodeLabel(from)} to {_graph.NodeLabel(to)}.");
            result.Success = false;
            return result;
        }

        result.Cost = path.Cost;
        result.Messages.Add($"Path {_graph.NodeLabel(from)} -> {_graph.NodeLabel(to)}: {path.Steps.Count} edge(s), cost {path.Cost:0.###} ({options.Weight.Name})");
        AddNode(result, from, NodeType(from), 0, "From", "");
        double cost = 0;
        foreach (var step in path.Steps)
        {
            cost += step.Cost;
            result.Messages.Add($"  {cost,10:0.###}  {_graph.EdgeLabel(step.Edge),-50} -> {_graph.NodeLabel(step.To)}");
            AddNode(result, step.To, NodeType(step.To), cost, step.To == to ? "To" : "Via", "");
            AddEdge(result, step.Edge, step.To);
        }
        return result;
    }

    public NetworkQueryResult Supply(NetworkQueryRequest request, IProgress<string>? progress = null)
    {
        var result = NewResult("supply");
        var sources = _graph.ResolveNodes(Required(request.Sources, "sources"));
        var sinks = _graph.ResolveNodes(Required(request.Sinks, "sinks"));
        result.Messages.Add($"Supply: {sources.Count} source(s), {sinks.Count} sink(s), {sinks.Sum(s => _graph.AliasesOf(s).Count)} merged point(s) at the sinks");

        var supplyProgress = progress is null ? null : new SyncProgress<SupplyProgress>(p =>
        {
            if (p.SourcesDone % 1000 == 0 || p.SourcesDone == p.SourcesTotal)
            {
                progress.Report($"  {p.SourcesDone}/{p.SourcesTotal} sources");
            }
        });
        var supply = _graph.Supply(sources, sinks, Options(request), supplyProgress).ToList();

        int unsupplied = supply.Count(s => s.Sources.Count == 0);
        int multiple = supply.Count(s => s.Sources.Count > 1);
        result.Messages.Add($"  supplied once: {supply.Count - unsupplied - multiple}");
        result.Messages.Add($"  not supplied:  {unsupplied}");
        result.Messages.Add($"  multiple:      {multiple}");
        foreach (var sink in supply.Where(s => s.Sources.Count != 1).Take(20))
        {
            result.Messages.Add($"    {_graph.NodeLabel(sink.Sink)}: {(sink.Sources.Count == 0 ? "not supplied" : string.Join(", ", sink.Sources.Select(_graph.NodeLabel)))}");
        }

        foreach (var s in supply)
        {
            AddNode(result, s.Sink, NodeType(s.Sink), 0,
                s.Sources.Count == 0 ? "NotSupplied" : s.Sources.Count == 1 ? "Supplied" : "Multiple",
                string.Join(" ", s.Sources.Select(_graph.NodeLabel)));
        }
        return result;
    }

    public NetworkQueryResult Info(NetworkQueryRequest request)
    {
        var result = NewResult("info");
        var id = _graph.ResolveNode(Required(request.Node, "node"));
        if (!_graph.TryGetNode(id, out var node))
        {
            result.Messages.Add("Node not found.");
            result.Success = false;
            return result;
        }
        _graph.TryGetShardOf(id, out var shard);
        var key = _graph.Metadata.Shards.FirstOrDefault(s => s.Id == shard)?.Key;

        result.Messages.Add($"Node    {_graph.NodeLabel(id)}  (id {id})");
        result.Messages.Add($"Type    {node.Node.TypeId} {_graph.TypeName(node.Node.TypeId)}");
        result.Messages.Add($"At      {node.Node.Location.X.ToString("0.###", CultureInfo.InvariantCulture)}, {node.Node.Location.Y.ToString("0.###", CultureInfo.InvariantCulture)}");
        result.Messages.Add($"Shard   {shard}{(key is null ? "" : $" ({key})")}");
        result.Messages.Add($"Blocked {node.Node.Blocked}");
        var aliases = _graph.AliasesOf(id);
        if (aliases.Count > 0)
        {
            result.Messages.Add($"Merged  {string.Join(", ", aliases.Select(a => $"{_graph.NodeLabel(a)} ({_graph.TypeName(_graph.AliasTypeOf(a))})"))}");
        }
        AddNode(result, id, node.Node.TypeId, 0, "Node", "");

        var edges = _graph.EdgesOf(id);
        result.Messages.Add($"Edges   {edges.Count}");
        foreach (var edge in edges)
        {
            var other = edge.Edge.From == id ? edge.Edge.To : edge.Edge.From;
            var otherType = NodeType(other);
            result.Messages.Add($"  {_graph.EdgeLabel(edge.Edge.Id),-50} {_graph.TypeName(edge.Edge.TypeId),-45} {edge.Edge.Length,9:0.##} m  {(edge.Edge.Disabled ? "disabled " : "")}-> {_graph.NodeLabel(other)} ({_graph.TypeName(otherType)})");
            AddEdge(result, edge);
            AddNode(result, other, otherType, edge.Edge.Length, "Neighbor", _graph.NodeLabel(id));
        }
        return result;
    }

    /// <summary>
    /// Checks the rules (of the request, else of network.json) over the whole network. Success is false when a rule of
    /// severity Error is violated or there are no rules; warnings do not count.
    /// </summary>
    public NetworkQueryResult Validate(NetworkQueryRequest request, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        var result = NewResult("validate");
        var rules = request.Rules ?? _graph.Metadata.Rules;
        if (!string.IsNullOrEmpty(request.RuleIds))
        {
            var ids = request.RuleIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var unknown = ids.Where(id => !rules.Any(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase))).ToList();
            if (unknown.Count > 0)
            {
                throw new ArgumentException($"Unknown rule(s): {string.Join(", ", unknown)}. Rules: {string.Join(", ", rules.Select(r => r.Id))}");
            }
            rules = rules.Where(r => ids.Contains(r.Id, StringComparer.OrdinalIgnoreCase)).ToList();
        }
        if (rules.Count == 0)
        {
            result.Messages.Add(request.Rules is null
                ? "No rules in network.json: add rules to the network config (or the network module of the map) and build again."
                : "No rules.");
            result.Success = false;
            return result;
        }
        result.Messages.Add($"Validate: {rules.Count} rule(s) from {(request.Rules is null ? "network.json" : "the request")}");

        var validator = new NetworkValidator(_graph, rules);
        var shardProgress = progress is null ? null : new SyncProgress<(int Done, int Total)>(p =>
        {
            if (p.Done % 500 == 0 || p.Done == p.Total)
            {
                progress.Report($"  {p.Done}/{p.Total} shards");
            }
        });
        var descriptions = rules.ToDictionary(r => r.Id, r => r.Description, StringComparer.Ordinal);
        foreach (var v in validator.Validate(shardProgress, cancellationToken))
        {
            var (layer, oid) = FeatureOf(v.Node);
            string? edgeLabel = null, edgeLayer = null;
            long? edgeId = null, edgeOid = null;
            if (v.Kind == ElementKind.Edge)
            {
                edgeId = v.Edge;
                edgeLabel = _graph.EdgeLabel(v.Edge);
                (edgeLayer, edgeOid) = FeatureOf(v.Edge);
            }
            result.Violations.Add(new NetworkResultViolation(
                v.RuleId, v.Severity.ToString(), descriptions.GetValueOrDefault(v.RuleId), v.Kind.ToString(),
                v.Node, _graph.NodeLabel(v.Node), layer, oid,
                edgeId, edgeLabel, edgeLayer, edgeOid,
                _graph.TypeName(v.TypeId), v.Count, v.Min, v.Max, v.Location.X, v.Location.Y,
                _graph.AliasesOf(v.Node).Select(_graph.NodeLabel).ToList()));
        }

        var summaries = validator.Summaries;
        int idWidth = Math.Max(4, summaries.Max(s => s.Rule.Id.Length));
        result.Messages.Add($"  {"rule".PadRight(idWidth)}  severity  {"checked",10}  {"violations",10}");
        foreach (var summary in summaries)
        {
            var description = string.IsNullOrEmpty(summary.Rule.Description) ? "" : "  " + summary.Rule.Description;
            result.Messages.Add($"  {summary.Rule.Id.PadRight(idWidth)}  {summary.Rule.Severity,-8}  {summary.Checked,10}  {summary.Violations,10}{description}");
            result.Rules.Add(new NetworkResultRuleSummary(summary.Rule.Id, summary.Rule.Severity.ToString(), summary.Rule.Description, summary.Checked, summary.Violations));
        }
        long errors = summaries.Where(s => s.Rule.Severity == RuleSeverity.Error).Sum(s => s.Violations);
        long warnings = summaries.Where(s => s.Rule.Severity == RuleSeverity.Warning).Sum(s => s.Violations);
        result.Messages.Add($"Errors: {errors}, warnings: {warnings}");
        result.Success = errors == 0;
        return result;
    }

    #endregion

    #region Helpers

    private NetworkQueryResult NewResult(string operation) => new() { Operation = operation, Srs = _graph.Metadata.Srs };

    private TraversalOptions<long, long> Options(NetworkQueryRequest request)
    {
        var options = new TraversalOptions<long, long>();

        if (!string.IsNullOrEmpty(request.Weight) && !request.Weight.Equals("length", StringComparison.OrdinalIgnoreCase))
        {
            var weight = _graph.Metadata.Weights.FirstOrDefault(w => w.Name.Equals(request.Weight, StringComparison.OrdinalIgnoreCase))
                         ?? throw new ArgumentException($"Unknown weight '{request.Weight}'. Weights: length, {string.Join(", ", _graph.Metadata.Weights.Select(w => w.Name))}");
            options = options with { Weight = new WeightDefinition(weight.Id, weight.Name, weight.Kind, weight.Fallback) };
        }
        if (request.MaxCost is double maxCost)
        {
            options = options with { MaxCost = maxCost };
        }
        if (!string.IsNullOrEmpty(request.Filter))
        {
            options = options with { EdgeTypes = _graph.Types.Edges(request.Filter) };
        }
        if (!string.IsNullOrEmpty(request.Block) || !string.IsNullOrEmpty(request.Unblock))
        {
            var state = new NetworkState<long, long>();
            foreach (var id in string.IsNullOrEmpty(request.Block) ? [] : _graph.ResolveNodes(request.Block))
            {
                state.Block(id);
            }
            foreach (var id in string.IsNullOrEmpty(request.Unblock) ? [] : _graph.ResolveNodes(request.Unblock))
            {
                state.Unblock(id);
            }
            options = options with { State = state };
        }
        return options;
    }

    private static string Required(string? value, string name)
        => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"{name} is required.") : value;

    private int NodeType(long id) => _graph.TryGetNode(id, out var n) ? n.Node.TypeId : 0;

    /// <summary>The layer path and OID of a node or edge id; null for helper nodes.</summary>
    private (string? Layer, long? Oid) FeatureOf(long id)
    {
        var (fc, oid, _) = NetworkIds.Decompose(id);
        if (fc == NetworkIds.HelperFeatureClass)
        {
            return (null, null);
        }
        return (_graph.Metadata.FeatureClasses.FirstOrDefault(f => f.Index == fc)?.Layer, oid);
    }

    private void AddTypes(NetworkQueryResult result, IEnumerable<int> typeIds)
    {
        foreach (var group in typeIds.GroupBy(t => t).OrderByDescending(g => g.Count()))
        {
            result.Messages.Add($"    {group.Count(),8}  {_graph.TypeName(group.Key)}");
        }
    }

    /// <summary>The node, followed by one row per point merged into it (customers drawn on a box).</summary>
    private void AddNode(NetworkQueryResult result, long id, int typeId, double cost, string kind, string source)
    {
        var (x, y) = Location(id);
        var (layer, oid) = FeatureOf(id);
        var label = _graph.NodeLabel(id);
        result.Nodes.Add(new NetworkResultNode(id, label, layer, oid, _graph.TypeName(typeId), kind, cost, source, "", x, y));
        foreach (var alias in _graph.AliasesOf(id))
        {
            var (aliasLayer, aliasOid) = FeatureOf(alias);
            result.Nodes.Add(new NetworkResultNode(alias, _graph.NodeLabel(alias), aliasLayer, aliasOid, _graph.TypeName(_graph.AliasTypeOf(alias)), kind, cost, source, label, x, y));
        }
    }

    private void AddEdge(NetworkQueryResult result, long edgeId, long atNode)
    {
        if (_graph.TryGetEdge(edgeId, atNode, out var edge))
        {
            AddEdge(result, edge);
        }
    }

    private void AddEdge(NetworkQueryResult result, ShardEdge edge)
    {
        var e = edge.Edge;
        var (fromX, fromY) = Location(e.From);
        var (toX, toY) = Location(e.To);
        var layer = _graph.Metadata.FeatureClasses.FirstOrDefault(f => f.Index == edge.Source.FeatureClass)?.Layer ?? edge.Source.FeatureClass.ToString();
        result.Edges.Add(new NetworkResultEdge(
            e.Id, _graph.EdgeLabel(e.Id), layer, edge.Source.Oid, _graph.TypeName(e.TypeId), e.Length,
            edge.FromMeasure, edge.ToMeasure, e.From, e.To,
            fromX, fromY, toX, toY));
    }

    private (double X, double Y) Location(long nodeId)
        => _graph.TryGetNode(nodeId, out var node) ? (node.Node.Location.X, node.Node.Location.Y) : (double.NaN, double.NaN);

    private sealed class SyncProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }

    #endregion
}
