using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Extensions;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Ids;
using gView.Framework.NetworkModel.Store;
using gView.Framework.NetworkModel.Validation;
using SpatialNET.Network;
using System.Globalization;

namespace gView.Framework.NetworkModel.Query;

/// <summary>
/// The searches on a built network (trace, reach, path, supply, outage, feeders, info, nearest, validate) for gView.Cmd, Carto and the server.
/// Thread-safe as long as the graph is: every call keeps its own state.
/// </summary>
public sealed partial class NetworkQueryService
{
    public static readonly IReadOnlyList<string> Operations = ["trace", "reach", "path", "supply", "outage", "feeders", "info", "nearest", "validate"];

    private readonly FileNetworkGraph _graph;

    public NetworkQueryService(FileNetworkGraph graph) => _graph = graph;

    public FileNetworkGraph Graph => _graph;

    /// <summary>
    /// Runs an operation. <paramref name="progress"/> gets a line now and then while it runs (nodes found so far, sources
    /// done, ...); <paramref name="cancellationToken"/> stops it (<see cref="OperationCanceledException"/>).
    /// </summary>
    public NetworkQueryResult Run(string operation, NetworkQueryRequest request, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
        => operation.ToLowerInvariant() switch
        {
            "trace" => Trace(request, progress, cancellationToken),
            "reach" => Reach(request, progress, cancellationToken),
            "path" => Path(request, progress, cancellationToken),
            "supply" => Supply(request, progress, cancellationToken),
            "outage" => Outage(request, progress, cancellationToken),
            "feeders" => Feeders(request, progress, cancellationToken),
            "info" => Info(request),
            "nearest" => Nearest(request),
            "validate" => Validate(request, progress, cancellationToken),
            _ => throw new ArgumentException($"Unknown operation '{operation}'. Operations: {string.Join(", ", Operations)}")
        };

    #region Operations

    public NetworkQueryResult Trace(NetworkQueryRequest request, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        var watch = new Watch(progress, cancellationToken);
        var result = NewResult("trace");
        var scope = Scope(request, result);
        var tiers = SelectTiers(request, result);
        var sources = Nodes(Required(request.Start, "start"), "start", scope, result, tiers);
        result.Messages.Add($"Trace from {sources.Count} source(s)");

        // with a tier the trace stops at the sources the tier itself names (blocked: settled, not expanded); the global supply
        // sources are not a stop: with a tier without sources every transformer of the network would end the trace
        var stops = new HashSet<long>();
        if (tiers is not null && request.StopAtSources != false)
        {
            var pattern = Pick(request.Sources, tiers.Sources, null);
            if (pattern is not null)
            {
                stops.UnionWith(Nodes(pattern, "source", scope, result).Except(sources));
                result.Messages.Add($"  stops at {stops.Count} source(s) of the tier");
            }
        }

        var options = Options(request, new(scope, tiers, ExtraBlocked: stops));
        var blocked = BlockedNodes(request, tiers); // a node that is blocked anyway stays "Blocked", only the stops are "Source"
        var hits = watch.Count(_graph.Trace(sources, options, cancellationToken), "trace", "node(s) found").ToList();
        foreach (var group in hits.GroupBy(h => h.Kind).OrderBy(g => g.Key))
        {
            result.Messages.Add($"  {group.Key,-12} {group.Count()}");
        }
        AddTypes(result, hits.Where(h => h.Kind == TraceHitKind.Reached).Select(h => (h.Node, h.TypeId)));

        foreach (var hit in watch.Count(hits, "result", "node(s) labelled"))
        {
            AddNode(result, hit.Node, hit.TypeId, hit.Cost, hit.Kind == TraceHitKind.Blocked && stops.Contains(hit.Node) && !blocked.Contains(hit.Node) ? "Source" : hit.Kind.ToString(), _graph.NodeLabel(hit.Source));
        }
        if (request.WithEdges)
        {
            foreach (var edge in watch.Count(_graph.TraceEdges(sources, options, cancellationToken), "edges", "edge(s) found").DistinctBy(e => e.Edge.Id))
            {
                AddEdge(result, edge.Edge.Id, edge.Edge.From);
            }
        }
        AddBoundary(result, scope, request.WithEdges);
        return result;
    }

    public NetworkQueryResult Reach(NetworkQueryRequest request, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        var watch = new Watch(progress, cancellationToken);
        var result = NewResult("reach");
        var scope = Scope(request, result);
        var tiers = SelectTiers(request, result);
        var starts = Nodes(Required(request.Start, "start"), "start", scope, result, tiers);
        var options = Options(request, new(scope, tiers));
        result.Messages.Add($"Reach from {starts.Count} start(s), max cost {options.MaxCost}, weight {options.Weight.Name}");

        var reached = watch.Count(_graph.Reach(starts, options, cancellationToken), "reach", "node(s) reached").ToList();
        result.Messages.Add($"  reached {reached.Count} node(s), max cost {(reached.Count > 0 ? reached.Max(r => r.Cost) : 0):0.###}");
        AddTypes(result, reached.Select(r => (r.Node, r.TypeId)));

        foreach (var r in watch.Count(reached, "result", "node(s) labelled"))
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
        AddBoundary(result, scope, request.WithEdges);
        return result;
    }

    public NetworkQueryResult Path(NetworkQueryRequest request, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        var result = NewResult("path");
        var scope = Scope(request, result);
        var tiers = SelectTiers(request, result);
        var from = Inside(_graph.ResolveNode(Required(request.From, "from"), tiers?.Union()), request.From!, "from", scope);
        var to = Inside(_graph.ResolveNode(Required(request.To, "to"), tiers?.Union()), request.To!, "to", scope);
        var options = Options(request, new(scope, tiers));

        progress?.Report("path: searching...");
        var path = _graph.ShortestPath(from, to, options, cancellationToken);
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

    public NetworkQueryResult Supply(NetworkQueryRequest request, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        var result = NewResult("supply");
        var scope = Scope(request, result);
        var tiers = SelectTiers(request, result);
        var sources = Nodes(Required(Pick(request.Sources, tiers?.Sources, _graph.Metadata.SupplySources), "sources"), "source", scope, result, tiers);
        var sinks = Nodes(Required(Pick(request.Sinks, tiers?.Sinks, _graph.Metadata.Customers), "sinks"), "sink", scope, result, tiers);
        result.Messages.Add($"Supply: {sources.Count} source(s), {sinks.Count} sink(s), {sinks.Sum(s => _graph.AliasesOf(s).Count)} merged point(s) at the sinks");

        var supply = _graph.Supply(sources, sinks, Options(request, new(scope, tiers)), SupplyProgress(progress, "supply"), cancellationToken).ToList();

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
        AddBoundary(result, scope, request.WithEdges);
        return result;
    }

    /// <summary>
    /// Switches the edges of <see cref="NetworkQueryRequest.Disable"/> off and reports the customers (sinks) that lose
    /// their supply. Only the part of the network connected to those edges is computed: supply with and without them.
    /// </summary>
    public NetworkQueryResult Outage(NetworkQueryRequest request, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        var watch = new Watch(progress, cancellationToken);
        var result = NewResult("outage");
        var scope = Scope(request, result);
        var disabled = _graph.ResolveEdges(Required(request.Disable, "disable"));
        var tiers = SelectTiers(request, result);
        var sourcesPattern = Required(Pick(request.Sources, tiers?.Sources, _graph.Metadata.SupplySources), "sources");
        var sinksPattern = Required(Pick(request.Sinks, tiers?.Sinks, _graph.Metadata.Customers), "sinks");

        var normal = Options(request, new(scope, tiers, WithDisable: false));
        var switchedOff = Options(request, new(scope, tiers));

        // the connected part around the switched off edges, as it is without switching them off
        var ends = new HashSet<long>();
        foreach (var id in disabled)
        {
            if (_graph.TryFindEdge(id, out var edge))
            {
                if (scope is not null && !(scope.Contains(edge.Edge.From) && scope.Contains(edge.Edge.To)))
                {
                    throw new ArgumentException($"disable: {_graph.EdgeLabel(id)} is (partly) outside the area.");
                }
                ends.Add(edge.Edge.From);
                ends.Add(edge.Edge.To);
            }
        }
        // without the directions of the edges: from a box the way back to the station runs against them
        var component = watch.Count(new UndirectedGraph(_graph).Reach(ends.ToList(), normal with { MaxCost = double.PositiveInfinity }, cancellationToken), "connected part", "node(s) found")
            .Select(r => r.Node).ToHashSet();
        var sources = Nodes(sourcesPattern, "source", scope, result, tiers).Where(component.Contains).ToList();
        var sinks = Nodes(sinksPattern, "sink", scope, result, tiers).Where(component.Contains).ToList();
        result.Messages.Add($"Outage: {disabled.Count} edge(s) switched off, {component.Count} node(s) connected, {sources.Count} source(s), {sinks.Count} customer(s) there");

        var before = _graph.Supply(sources, sinks, normal, SupplyProgress(progress, "supply before"), cancellationToken).ToDictionary(s => s.Sink, s => s.Sources);
        var after = _graph.Supply(sources, sinks, switchedOff, SupplyProgress(progress, "supply after"), cancellationToken).ToDictionary(s => s.Sink, s => s.Sources);

        var affected = sinks.Where(s => before[s].Count > 0 && after[s].Count == 0).ToList();
        int stillSupplied = sinks.Count(s => before[s].Count > 0 && after[s].Count > 0);
        int unsuppliedBefore = sinks.Count(s => before[s].Count == 0);
        result.Messages.Add($"  affected:              {affected.Count} ({affected.Sum(s => 1 + _graph.AliasesOf(s).Count)} point(s) with the merged ones)");
        result.Messages.Add($"  still supplied:        {stillSupplied}");
        result.Messages.Add($"  not supplied before:   {unsuppliedBefore}");
        AddTypes(result, affected.Select(n => (n, NodeType(n))));

        foreach (var sink in affected)
        {
            AddNode(result, sink, NodeType(sink), 0, "Affected", string.Join(" ", before[sink].Select(_graph.NodeLabel)));
        }

        if (request.WithEdges)
        {
            var supplied = watch.Count(_graph.TraceEdges(sources, normal, cancellationToken), "edges before", "edge(s) found").Select(e => e.Edge.Id).ToHashSet();
            var stillLive = watch.Count(_graph.TraceEdges(sources, switchedOff, cancellationToken), "edges after", "edge(s) found").Select(e => e.Edge.Id).ToHashSet();
            foreach (var id in disabled)
            {
                if (_graph.TryFindEdge(id, out var edge))
                {
                    AddEdge(result, edge, "Disabled");
                }
            }
            foreach (var id in supplied.Where(id => !stillLive.Contains(id) && !disabled.Contains(id)))
            {
                if (_graph.TryFindEdge(id, out var edge))
                {
                    AddEdge(result, edge, "Dead");
                }
            }
        }
        AddBoundary(result, scope, request.WithEdges);
        return result;
    }

    /// <summary>The nodes nearest to a location (real nodes, edge ends, merged points standing for their node).</summary>
    public NetworkQueryResult Nearest(NetworkQueryRequest request)
    {
        var result = NewResult("nearest");
        if (request.X is not double x || request.Y is not double y)
        {
            throw new ArgumentException("x and y are required.");
        }
        int count = Math.Max(1, request.Count ?? 1);
        double radius = request.Radius ?? _graph.DefaultSearchRadius;
        // several merged points may stand for one node: ask for more, keep each node once
        var hits = _graph.Nearest(x, y, radius, count * 4 + 8)
            .GroupBy(h => h.Entry.Node)
            .Select(g => g.First())
            .Take(count)
            .ToList();
        result.Messages.Add($"Nearest to {x.ToString("0.###", CultureInfo.InvariantCulture)}, {y.ToString("0.###", CultureInfo.InvariantCulture)} within {radius:0.###}: {hits.Count} node(s)");
        if (hits.Count == 0)
        {
            result.Success = false;
        }
        foreach (var hit in hits)
        {
            var node = hit.Entry.Node;
            result.Messages.Add($"  {hit.Distance,10:0.###}  {_graph.NodeLabel(node)} ({_graph.TypeName(NodeType(node))})");
            AddNode(result, node, NodeType(node), hit.Distance, "Nearest", "");
        }
        return result;
    }

    public NetworkQueryResult Info(NetworkQueryRequest request)
    {
        var result = NewResult("info");
        var id = _graph.ResolveNode(Required(request.Node, "node"), NetworkTiers.Select(_graph, request.Tier)?.Union());
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
        var scope = Scope(request, result);
        var tiers = SelectTiers(request, result);

        var validator = new NetworkValidator(_graph, rules, tiers?.Union()) { Area = scope?.Nodes };
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
        foreach (var skipped in validator.Skipped)
        {
            result.Messages.Add(skipped);
        }
        int idWidth = Math.Max(4, summaries.Select(s => s.Rule.Id.Length).DefaultIfEmpty(0).Max());
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

    /// <summary>The tiers of the request (see <see cref="NetworkQueryRequest.Tier"/>); null without a tier.</summary>
    private NetworkTiers? SelectTiers(NetworkQueryRequest request, NetworkQueryResult result)
    {
        var tiers = NetworkTiers.Select(_graph, request.Tier);
        if (tiers is not null)
        {
            result.Messages.Add($"Tier: {tiers.Names}");
        }
        return tiers;
    }

    /// <summary>The first text that is not empty.</summary>
    private static string? Pick(params string?[] texts) => texts.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));

    /// <summary>The area of the request resolved to its nodes; null without an area.</summary>
    private NetworkAreaScope? Scope(NetworkQueryRequest request, NetworkQueryResult result)
    {
        if (request.Area is null)
        {
            return null;
        }
        var scope = new NetworkAreaScope(request.Area, _graph.NodesIn(request.Area));
        result.Messages.Add($"Area: {request.Area.Description}, {scope.Count} node(s) inside");
        return scope;
    }

    /// <summary>
    /// Resolves node references; with an area, nodes of a type pattern outside are left out (with a message) and an
    /// explicit node (Layer#OID, id, @x,y) outside is an error.
    /// </summary>
    private IReadOnlyList<long> Nodes(string references, string what, NetworkAreaScope? scope, NetworkQueryResult result, NetworkTiers? tiers = null)
    {
        var tier = tiers?.Union();
        if (scope is null)
        {
            return _graph.ResolveNodes(references, tier);
        }
        var ids = new List<long>();
        int left = 0;
        foreach (var reference in FileNetworkGraph.SplitReferences(references))
        {
            var resolved = _graph.ResolveNodes(reference, tier);
            if (FileNetworkGraph.IsTypePattern(reference))
            {
                ids.AddRange(resolved.Where(scope.Contains));
                left += resolved.Count(id => !scope.Contains(id));
            }
            else
            {
                ids.AddRange(resolved.Select(id => Inside(id, reference, what, scope)));
            }
        }
        if (left > 0)
        {
            result.Messages.Add($"  {left} {what} node(s) outside the area left out");
        }
        return ids.Distinct().ToList();
    }

    private long Inside(long id, string reference, string what, NetworkAreaScope? scope)
        => scope is null || scope.Contains(id)
            ? id
            : throw new ArgumentException($"{what}: {reference} ({_graph.NodeLabel(id)}) is outside the area.");

    /// <summary>The edges leaving the area that the search came to: a message and, when edges are wanted, the edges as Boundary.</summary>
    private void AddBoundary(NetworkQueryResult result, NetworkAreaScope? scope, bool withEdges)
    {
        if (scope is null)
        {
            return;
        }
        result.Messages.Add($"  boundary edges: {scope.Boundary.Count} (leave the area, not followed)");
        if (withEdges)
        {
            foreach (var (edgeId, inside) in scope.Boundary.OrderBy(b => b.Key))
            {
                if (_graph.TryGetEdge(edgeId, inside, out var edge))
                {
                    AddEdge(result, edge, "Boundary");
                }
            }
        }
    }

    /// <summary>What a search adds to the options of its request.</summary>
    /// <param name="Scope">The area the search is restricted to, or null.</param>
    /// <param name="Tiers">The tiers the search is restricted to, or null.</param>
    /// <param name="WithDisable">False leaves the switched off edges of the request out (the state before an outage).</param>
    /// <param name="ExtraBlocked">Nodes the search stops at besides those the request and the tiers block.</param>
    private sealed record OptionsContext(NetworkAreaScope? Scope, NetworkTiers? Tiers = null, bool WithDisable = true, IReadOnlyCollection<long>? ExtraBlocked = null);

    /// <summary>The nodes blocked by the request and by the tiers (their references or patterns resolved).</summary>
    private HashSet<long> BlockedNodes(NetworkQueryRequest request, NetworkTiers? tiers)
        => new[] { request.Block }.Concat(tiers?.Block ?? []).OfType<string>().Where(b => !string.IsNullOrWhiteSpace(b))
            .SelectMany(b => _graph.ResolveNodes(b)).ToHashSet();

    private TraversalOptions<long, long> Options(NetworkQueryRequest request, OptionsContext context)
    {
        var (scope, tiers, withDisable, extraBlocked) = context;
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
        // the tiers' edge types, cut by the filter of the request
        var edgeTypes = tiers?.EdgeTypes;
        if (!string.IsNullOrEmpty(request.Filter))
        {
            var filter = _graph.Types.Edges(request.Filter);
            edgeTypes = edgeTypes is null ? filter : edgeTypes.Intersect(filter);
        }
        if (edgeTypes is not null)
        {
            if (edgeTypes.Count == 0)
            {
                throw new ArgumentException($"The tier ({tiers?.Names}) and the filter have no edge type in common.");
            }
            options = options with { EdgeTypes = edgeTypes };
        }
        var block = BlockedNodes(request, tiers);
        bool disable = withDisable && !string.IsNullOrEmpty(request.Disable);
        if (block.Count > 0 || !string.IsNullOrEmpty(request.Unblock) || disable || extraBlocked is { Count: > 0 })
        {
            var state = new NetworkState<long, long>();
            foreach (var id in disable ? _graph.ResolveEdges(request.Disable!) : [])
            {
                state.Disable(id);
            }
            foreach (var id in block)
            {
                state.Block(id);
            }
            foreach (var id in extraBlocked ?? [])
            {
                state.Block(id);
            }
            foreach (var id in string.IsNullOrEmpty(request.Unblock) ? [] : _graph.ResolveNodes(request.Unblock))
            {
                state.Unblock(id);
            }
            options = options with { State = state };
        }
        if (scope is not null)
        {
            options = options with { State = scope.Wrap(options.State) };
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

    /// <summary>Largest number of nodes of one type that are listed one by one in the messages.</summary>
    private const int ListedNodesPerType = 5;

    /// <summary>
    /// The nodes per type, most frequent first. Rare types (a few nodes among thousands) are listed with label and location,
    /// so a surprise such as a low voltage joint in a medium voltage trace can be found on the map.
    /// </summary>
    private void AddTypes(NetworkQueryResult result, IEnumerable<(long Node, int TypeId)> nodes)
    {
        foreach (var group in nodes.GroupBy(n => n.TypeId).OrderByDescending(g => g.Count()))
        {
            result.Messages.Add($"    {group.Count(),8}  {_graph.TypeName(group.Key)}");
            if (group.Key != 0 && !(_graph.Types.TryGetType(group.Key, out var type) && type.Helper) && group.Count() <= ListedNodesPerType)
            {
                foreach (var (node, _) in group)
                {
                    var (x, y) = Location(node);
                    result.Messages.Add(string.Create(CultureInfo.InvariantCulture, $"              {_graph.NodeLabel(node)}  at {x:0.###}, {y:0.###}"));
                }
            }
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

    private void AddEdge(NetworkQueryResult result, ShardEdge edge, string kind = "Edge")
    {
        var e = edge.Edge;
        var (fromX, fromY) = Location(e.From);
        var (toX, toY) = Location(e.To);
        var layer = _graph.Metadata.FeatureClasses.FirstOrDefault(f => f.Index == edge.Source.FeatureClass)?.Layer ?? edge.Source.FeatureClass.ToString();
        result.Edges.Add(new NetworkResultEdge(
            e.Id, _graph.EdgeLabel(e.Id), layer, edge.Source.Oid, _graph.TypeName(e.TypeId), e.Length,
            edge.FromMeasure, edge.ToMeasure, e.From, e.To,
            fromX, fromY, toX, toY) { Kind = kind });
    }

    private (double X, double Y) Location(long nodeId)
        => _graph.TryGetNode(nodeId, out var node) ? (node.Node.Location.X, node.Node.Location.Y) : (double.NaN, double.NaN);

    /// <summary>The graph with every edge walkable both ways (finding what is connected, whatever the flow direction).</summary>
    private sealed class UndirectedGraph(IGraph<long, long> graph) : IGraph<long, long>
    {
        public IEnumerable<GraphNode<long>> GetNodes(IReadOnlyCollection<long> ids) => graph.GetNodes(ids);

        public IEnumerable<GraphEdge<long, long>> GetEdges(IReadOnlyCollection<long> nodeIds)
            => graph.GetEdges(nodeIds).Select(e => e.Direction == EdgeDirection.Both ? e : e with { Direction = EdgeDirection.Both });
    }

    /// <summary>The supply progress as lines: sources done of all, at most about 4 per second.</summary>
    private static IProgress<SupplyProgress>? SupplyProgress(IProgress<string>? progress, string phase)
    {
        if (progress is null)
        {
            return null;
        }
        long last = 0;
        return new SyncProgress<SupplyProgress>(p =>
        {
            long now = Environment.TickCount64;
            if (now - last >= 250 || p.SourcesDone == p.SourcesTotal)
            {
                last = now;
                progress.Report($"{phase}: {p.SourcesDone}/{p.SourcesTotal} source(s)");
            }
        });
    }

    /// <summary>Counts what a search streams, reports the count at most about 4 times a second and stops on cancellation.</summary>
    private sealed class Watch(IProgress<string>? progress, CancellationToken cancellationToken)
    {
        public IEnumerable<T> Count<T>(IEnumerable<T> items, string phase, string what)
        {
            long count = 0, last = Environment.TickCount64;
            foreach (var item in items)
            {
                count++;
                if ((count & 255) == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    long now = Environment.TickCount64;
                    if (progress is not null && now - last >= 250)
                    {
                        last = now;
                        progress.Report($"{phase}: {count.ToString("N0", CultureInfo.InvariantCulture)} {what}");
                    }
                }
                yield return item;
            }
            progress?.Report($"{phase}: {count.ToString("N0", CultureInfo.InvariantCulture)} {what}");
        }
    }

    private sealed class SyncProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }

    #endregion
}
