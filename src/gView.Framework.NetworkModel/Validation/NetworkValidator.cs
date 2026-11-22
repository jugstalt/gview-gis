using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Extensions;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Query;
using gView.Framework.NetworkModel.Store;
using SpatialNET.Network;

namespace gView.Framework.NetworkModel.Validation;

/// <summary>The result of one rule over the whole network.</summary>
/// <param name="Rule">The rule.</param>
/// <param name="Checked">How many elements of the subject types were checked.</param>
/// <param name="Violations">How many violations were found (an edge rule may report both ends of an edge).</param>
public sealed record RuleSummary(NetworkRuleConfig Rule, long Checked, long Violations);

/// <summary>
/// Checks the rules of a network (SpatialNET ADR 0052) shard by shard over a <see cref="FileNetworkGraph"/>: every node
/// once, every edge once (in the shard of its from node). Helper nodes and edges are checked like all others.
/// </summary>
public sealed class NetworkValidator
{
    private readonly FileNetworkGraph _graph;
    private readonly List<(NetworkRuleConfig Config, NetworkRule Rule)> _rules;
    // per rule id: the tiers whose nodes a node rule is checked on (a node must belong to every one of them)
    private readonly Dictionary<string, List<ResolvedTier>> _nodeTiers = new(StringComparer.Ordinal);
    private readonly Dictionary<string, long> _checked = new(StringComparer.Ordinal);
    private readonly Dictionary<string, long> _violations = new(StringComparer.Ordinal);
    private readonly List<string> _skipped = [];

    /// <param name="graph">The network.</param>
    /// <param name="rules">The rules; a rule with a tier is restricted to it.</param>
    /// <param name="restrict">A tier (or the union of tiers) every rule is restricted to, as asked for by the query; null for none.</param>
    public NetworkValidator(FileNetworkGraph graph, IEnumerable<NetworkRuleConfig> rules, ResolvedTier? restrict = null)
    {
        _graph = graph;
        _rules = [];
        var problems = new List<string>();
        foreach (var config in rules)
        {
            try
            {
                var restrictions = new List<ResolvedTier>();
                if (!string.IsNullOrWhiteSpace(config.Tier))
                {
                    try
                    {
                        restrictions.Add(graph.Tiers.GetTier(config.Tier));
                    }
                    catch (ArgumentException ex)
                    {
                        throw new ArgumentException($"Rule '{config.Id}': {ex.Message}", ex);
                    }
                }
                if (restrict is not null)
                {
                    restrictions.Add(restrict);
                }

                NetworkRule? rule = config.Resolve(graph.Types);
                foreach (var restriction in restrictions)
                {
                    rule = config.RestrictToTier(rule, restriction);
                    if (rule is null)
                    {
                        break;
                    }
                }
                if (rule is null)
                {
                    _skipped.Add($"Rule '{config.Id}' skipped: its tier and the tier of the search have no edge types of the rule in common.");
                    continue;
                }
                if (config.Subject == RuleSubject.Node && restrictions.Count > 0)
                {
                    _nodeTiers[config.Id] = restrictions;
                }
                _rules.Add((config, rule));
            }
            catch (ArgumentException ex)
            {
                problems.Add(ex.Message);
            }
        }
        var duplicate = _rules.GroupBy(r => r.Rule.Id).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
        {
            problems.Add($"The rule id '{duplicate.Key}' is used twice.");
        }
        if (problems.Count > 0)
        {
            throw new InvalidDataException("Invalid rules:" + Environment.NewLine + string.Join(Environment.NewLine, problems.Select(p => "  " + p)));
        }
    }

    /// <summary>The rules that were left out because the tiers cut away everything they check, with the reason.</summary>
    public IReadOnlyList<string> Skipped => _skipped;

    public IReadOnlyList<NetworkRule> Rules => _rules.Select(r => r.Rule).ToList();

    /// <summary>
    /// Only the nodes for which this is true are checked, and the edges with at least one such end; their neighbors
    /// count whether inside or not. Only the shards of those nodes are read. Null: the whole network.
    /// </summary>
    public IReadOnlySet<long>? Area { get; init; }

    /// <summary>The violations of every rule, shard by shard. <see cref="Summaries"/> is complete once enumerated.</summary>
    public IEnumerable<RuleViolation<long, long>> Validate(IProgress<(int Done, int Total)>? progress = null, CancellationToken cancellationToken = default)
    {
        _checked.Clear();
        _violations.Clear();
        var rules = _rules.Select(r => r.Rule).ToList();
        var nodeRules = rules.Where(r => r.Subject == RuleSubject.Node).ToList();
        var edgeRules = rules.Where(r => r.Subject == RuleSubject.Edge).ToList();
        Func<long, bool>? area = Area is null ? null : Area.Contains;

        IEnumerable<ShardData> shards;
        int done = 0, total;
        var seenEdges = new HashSet<long>();
        if (area is null)
        {
            shards = _graph.ReadShards();
            total = _graph.Metadata.Shards.Count;
        }
        else
        {
            // the shards with nodes inside (an edge leaving such a shard is stored in it too)
            var shardIds = Area!
                .Select(id => _graph.TryGetShardOf(id, out var shard) ? shard : (long?)null)
                .OfType<long>()
                .Distinct()
                .Order()
                .ToList();
            shards = _graph.ReadShards(shardIds);
            total = shardIds.Count;
        }

        foreach (var shard in shards)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var nodes = area is null ? shard.Nodes : shard.Nodes.Where(n => area(n.Node.Id)).ToList();
            // nodes of a rule with tiers: those that edges of every tier touch (or the tier's node types)
            var nodesOf = new Dictionary<string, List<ShardNode>>(StringComparer.Ordinal);
            foreach (var rule in nodeRules.Where(r => _nodeTiers.ContainsKey(r.Id)))
            {
                nodesOf[rule.Id] = NodesInTiers(nodes, shard, _nodeTiers[rule.Id]);
            }
            foreach (var rule in nodeRules)
            {
                Add(_checked, rule.Id, (nodesOf.TryGetValue(rule.Id, out var inTier) ? inTier : nodes).Count(n => rule.SubjectTypes.Contains(n.Node.TypeId)));
            }
            // an edge between two shards is stored in both, it is checked in the shard of its from node
            // (with an area: once, in the first shard read that holds it)
            var edges = area is null
                ? shard.Edges.Where(e => e.FromShard == shard.Id).Select(e => e.Edge).ToList()
                : shard.Edges.Where(e => (area(e.Edge.From) || area(e.Edge.To)) && seenEdges.Add(e.Edge.Id)).Select(e => e.Edge).ToList();
            foreach (var rule in edgeRules)
            {
                Add(_checked, rule.Id, edges.Count(e => rule.SubjectTypes.Contains(e.TypeId)));
            }

            var violations = new List<RuleViolation<long, long>>();
            if (nodeRules.Count > 0)
            {
                var plain = nodeRules.Where(r => !nodesOf.ContainsKey(r.Id)).ToList();
                if (plain.Count > 0)
                {
                    violations.AddRange(_graph.ValidateNodes(nodes.Select(n => n.Node.Id), plain, cancellationToken));
                }
                foreach (var rule in nodeRules.Where(r => nodesOf.ContainsKey(r.Id)))
                {
                    violations.AddRange(_graph.ValidateNodes(nodesOf[rule.Id].Select(n => n.Node.Id), [rule], cancellationToken));
                }
            }
            if (edgeRules.Count > 0)
            {
                violations.AddRange(_graph.ValidateEdges(edges, edgeRules, cancellationToken));
            }
            foreach (var violation in violations)
            {
                Add(_violations, violation.RuleId, 1);
                yield return violation;
            }

            progress?.Report((++done, total));
        }
    }

    private static List<ShardNode> NodesInTiers(List<ShardNode> nodes, ShardData shard, List<ResolvedTier> tiers)
    {
        // the edge types at each node of the shard (an edge between shards is stored in both)
        var edgeTypesAt = new Dictionary<long, List<int>>();
        foreach (var edge in shard.Edges)
        {
            foreach (var end in new[] { edge.Edge.From, edge.Edge.To })
            {
                if (!edgeTypesAt.TryGetValue(end, out var list))
                {
                    edgeTypesAt[end] = list = [];
                }
                list.Add(edge.Edge.TypeId);
            }
        }
        return nodes
            .Where(n => tiers.All(t => t.ContainsNode(n.Node.TypeId, edgeTypesAt.GetValueOrDefault(n.Node.Id) ?? [])))
            .ToList();
    }

    /// <summary>One line per rule in the order of the rules.</summary>
    public IReadOnlyList<RuleSummary> Summaries
        => _rules.Select(r => new RuleSummary(r.Config, _checked.GetValueOrDefault(r.Rule.Id), _violations.GetValueOrDefault(r.Rule.Id))).ToList();

    private static void Add(Dictionary<string, long> counts, string id, long value)
        => counts[id] = counts.GetValueOrDefault(id) + value;
}
