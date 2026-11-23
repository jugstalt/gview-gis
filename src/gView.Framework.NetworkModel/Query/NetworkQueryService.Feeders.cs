using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Store;
using SpatialNET.Network;

namespace gView.Framework.NetworkModel.Query;

public sealed partial class NetworkQueryService
{
    /// <summary>
    /// Feeders (Abzweige): every edge reached from a source gets the source and the branch it hangs on, e.g. each low voltage
    /// line its transformer and the connector (Anlagenverbinder) it is fed through. A branch is an edge at a source of the
    /// type <see cref="NetworkQueryRequest.Branches"/> (every edge at the source without it); the search runs from the far end of
    /// each branch, the sources (and with a tier the sources of the tier) are stops. An edge reached from more than one branch
    /// is Kind Multiple and lists them all, cheapest first: a closed ring or a data error. With
    /// <see cref="NetworkQueryRequest.OpenEdges"/> an edge at a node a branch passed that no branch went along (switched off,
    /// against its direction) is Kind Open, with the branches of the ends that were reached: an open ring, a separation point.
    /// </summary>
    public NetworkQueryResult Feeders(NetworkQueryRequest request, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        var watch = new Watch(progress, cancellationToken);
        var result = NewResult("feeders");
        var scope = Scope(request, result);
        var tiers = SelectTiers(request, result);
        var sourcesPattern = Required(Pick(request.Sources, tiers?.Sources, _graph.Metadata.SupplySources), "sources");
        var sources = Nodes(sourcesPattern, "source", scope, result, tiers);
        var sourceLabels = SourceLabels(sourcesPattern, sources, tiers);

        var stops = new HashSet<long>(sources);
        if (tiers is not null && request.StopAtSources != false && !string.IsNullOrWhiteSpace(tiers.Sources))
        {
            stops.UnionWith(Nodes(tiers.Sources, "source", scope, result));
        }
        var options = Options(request, new(scope, tiers, ExtraBlocked: stops));
        var branchTypes = string.IsNullOrWhiteSpace(request.Branches) ? null : _graph.Types.Edges(request.Branches);

        // the branches: the edges a search could leave a source by
        var branches = new List<FeederBranch>();
        var withoutBranch = new List<long>();
        int done = 0;
        foreach (var source in sources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if ((++done & 255) == 0)
            {
                progress?.Report($"feeders: branches of {done:N0}/{sources.Count:N0} source(s), {branches.Count:N0} found");
            }
            int before = branches.Count;
            foreach (var edge in _graph.EdgesOf(source))
            {
                var e = edge.Edge;
                if (e.From == e.To
                    || (branchTypes is not null && !branchTypes.Contains(e.TypeId))
                    || (options.EdgeTypes is not null && !options.EdgeTypes.Contains(e.TypeId))
                    || (options.State?.IsDisabled(e) ?? e.Disabled))
                {
                    continue;
                }
                var direction = options.State?.GetDirection(e) ?? e.Direction;
                bool forward = e.From == source;
                if ((forward && direction == EdgeDirection.Backward) || (!forward && direction == EdgeDirection.Forward))
                {
                    continue;
                }
                var far = forward ? e.To : e.From;
                if (scope is not null && !scope.Contains(far))
                {
                    continue;
                }
                branches.Add(new FeederBranch(source, edge, far, EdgeCost(e, options.Weight), FeatureLabel(edge)));
            }
            if (branches.Count == before)
            {
                withoutBranch.Add(source);
            }
        }
        result.Messages.Add($"Feeders: {sources.Count} source(s), {branches.Count} branch(es)");
        if (withoutBranch.Count > 0)
        {
            result.Messages.Add($"  {withoutBranch.Count} source(s) without a branch: {string.Join(", ", withoutBranch.Take(5).Select(s => sourceLabels[s]))}{(withoutBranch.Count > 5 ? ", ..." : "")}");
        }

        // one run per far end (two branches to the same node share it)
        var byStart = branches.GroupBy(b => b.Start).ToDictionary(g => g.Key, g => g.ToList());
        var starts = byStart.Keys.ToList();
        var costs = new Dictionary<(long Start, long Node), double>();
        var passed = new List<(long Start, long Node, double Cost)>();   // nodes a run went through (not stops), for open edges
        foreach (var hit in watch.Count(_graph.Trace(starts, options, cancellationToken), "feeders", "node(s) found"))
        {
            if (costs.TryAdd((hit.Source, hit.Node), hit.Cost) && request.OpenEdges && hit.Kind is TraceHitKind.Source or TraceHitKind.Reached)
            {
                passed.Add((hit.Source, hit.Node, hit.Cost));
            }
        }

        var assigned = new Dictionary<long, (ShardEdge Edge, List<(FeederBranch Branch, double Cost)> Branches)>();
        void Assign(ShardEdge edge, FeederBranch branch, double cost)
        {
            if (!assigned.TryGetValue(edge.Edge.Id, out var entry))
            {
                assigned[edge.Edge.Id] = entry = (edge, []);
            }
            int index = entry.Branches.FindIndex(b => b.Branch == branch);
            if (index < 0)
            {
                entry.Branches.Add((branch, cost));
            }
            else if (cost < entry.Branches[index].Cost)
            {
                entry.Branches[index] = (branch, cost);
            }
        }

        foreach (var branch in branches)
        {
            Assign(branch.Edge, branch, 0);
        }
        foreach (var traced in watch.Count(_graph.TraceEdges(starts, options, cancellationToken), "feeders", "edge(s) found"))
        {
            var e = traced.Edge;
            if (!_graph.TryGetEdge(e.Id, e.From, out var edge) && !_graph.TryGetEdge(e.Id, e.To, out edge))
            {
                continue;
            }
            double near = Math.Min(
                costs.TryGetValue((traced.Source, e.From), out var fromCost) ? fromCost : double.PositiveInfinity,
                costs.TryGetValue((traced.Source, e.To), out var toCost) ? toCost : double.PositiveInfinity);
            foreach (var branch in byStart[traced.Source])
            {
                // the branch edge itself (the run walks back over it to the stopped source) has cost 0
                if (branch.Edge.Edge.Id != e.Id)
                {
                    Assign(edge, branch, branch.Cost + (double.IsFinite(near) ? near : 0));
                }
            }
        }

        // open edges: at a node a branch went through, but no branch went along them
        var open = new Dictionary<long, (ShardEdge Edge, List<(FeederBranch Branch, double Cost)> Branches)>();
        int checkedNodes = 0;
        foreach (var (start, node, cost) in passed)
        {
            if ((++checkedNodes & 1023) == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            foreach (var edge in _graph.EdgesOf(node))
            {
                var e = edge.Edge;
                if (assigned.ContainsKey(e.Id) || e.From == e.To
                    || (options.EdgeTypes is not null && !options.EdgeTypes.Contains(e.TypeId))
                    || (scope is not null && !(scope.Contains(e.From) && scope.Contains(e.To))))
                {
                    continue;
                }
                if (!open.TryGetValue(e.Id, out var entry))
                {
                    open[e.Id] = entry = (edge, []);
                }
                foreach (var branch in byStart[start])
                {
                    entry.Branches.Add((branch, branch.Cost + cost));
                }
            }
        }

        int multiple = 0;
        void Emit(ShardEdge edge, List<(FeederBranch Branch, double Cost)> found, string kind)
        {
            var ordered = found.OrderBy(b => b.Cost).ToList();
            AddEdge(result, edge, kind);
            result.Edges[^1] = result.Edges[^1] with
            {
                Source = string.Join(" ", ordered.Select(b => sourceLabels[b.Branch.Source]).Distinct()),
                Branch = string.Join(" ", ordered.Select(b => b.Branch.Label).Distinct()),
                Cost = ordered[0].Cost,
            };
        }
        foreach (var (_, (edge, found)) in assigned.OrderBy(a => a.Key))
        {
            bool isMultiple = found.Count > 1;
            if (isMultiple)
            {
                multiple++;
            }
            Emit(edge, found, isMultiple ? "Multiple" : "Edge");
        }
        foreach (var (_, (edge, found)) in open.OrderBy(a => a.Key))
        {
            Emit(edge, found, "Open");
        }
        result.Messages.Add($"  edges: {assigned.Count}, reached from more than one branch: {multiple}"
                            + (request.OpenEdges ? $", open: {open.Count}" : ""));

        foreach (var source in sources)
        {
            AddNode(result, source, NodeType(source), 0, withoutBranch.Contains(source) ? "NoBranch" : "Source", "");
        }
        AddBoundary(result, scope, true);
        return result;
    }

    /// <summary>A branch: the edge at a source and the node at its far end, where the search starts.</summary>
    private sealed record FeederBranch(long Source, ShardEdge Edge, long Start, double Cost, string Label);

    /// <summary>
    /// The label of each source; a source of the pattern that was merged into another node (an alias) keeps its own object,
    /// e.g. the transformer drawn on a box, so that the result names the transformer.
    /// </summary>
    private Dictionary<long, string> SourceLabels(string references, IReadOnlyList<long> sources, NetworkTiers? tiers)
    {
        var labels = new Dictionary<long, string>();
        var wanted = sources.ToHashSet();
        foreach (var reference in FileNetworkGraph.SplitReferences(references))
        {
            IReadOnlyList<long> resolved;
            try
            {
                resolved = _graph.ResolveNodes(reference, tiers?.Union());
            }
            catch (ArgumentException)
            {
                continue;
            }
            foreach (var node in resolved.Where(wanted.Contains))
            {
                if (labels.ContainsKey(node))
                {
                    continue;
                }
                var candidates = new[] { node }.Concat(_graph.AliasesOf(node));
                string? label;
                if (FileNetworkGraph.IsTypePattern(reference))
                {
                    var types = _graph.Types.Nodes(reference);
                    label = candidates
                        .Where(c => types.Contains(c == node ? NodeType(c) : _graph.AliasTypeOf(c)))
                        .Select(_graph.NodeLabel)
                        .FirstOrDefault();
                }
                else
                {
                    var text = reference.Trim();
                    label = candidates.Select(_graph.NodeLabel)
                        .FirstOrDefault(l => l.Equals(text, StringComparison.OrdinalIgnoreCase) || l.EndsWith("/" + text, StringComparison.OrdinalIgnoreCase));
                }
                labels[node] = label ?? _graph.NodeLabel(node);
            }
        }
        foreach (var node in sources.Where(s => !labels.ContainsKey(s)))
        {
            labels[node] = _graph.NodeLabel(node);
        }
        return labels;
    }

    /// <summary>Layer#OID of the line feature of an edge (without the part).</summary>
    private string FeatureLabel(ShardEdge edge)
        => $"{_graph.Metadata.FeatureClasses.FirstOrDefault(f => f.Index == edge.Source.FeatureClass)?.Layer ?? edge.Source.FeatureClass.ToString()}#{edge.Source.Oid}";

    /// <summary>The cost of an edge under a weight, as the search computes it.</summary>
    private static double EdgeCost(GraphEdge<long, long> edge, WeightDefinition weight)
    {
        double ValueOf()
        {
            foreach (var w in edge.Weights.Span)
            {
                if (w.WeightId == weight.Id)
                {
                    return w.Value;
                }
            }
            return weight.Fallback ?? double.NaN;
        }

        double cost = weight.Kind switch
        {
            WeightKind.Length => edge.Length,
            WeightKind.Absolute => ValueOf(),
            WeightKind.PerLength => ValueOf() * edge.Length,
            _ => edge.Length / ValueOf(),
        };
        return double.IsFinite(cost) && cost >= 0 ? cost : 0;
    }
}
