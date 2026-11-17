using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Graph;
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
    private readonly Dictionary<string, long> _checked = new(StringComparer.Ordinal);
    private readonly Dictionary<string, long> _violations = new(StringComparer.Ordinal);

    public NetworkValidator(FileNetworkGraph graph, IEnumerable<NetworkRuleConfig> rules)
    {
        _graph = graph;
        _rules = [];
        var problems = new List<string>();
        foreach (var config in rules)
        {
            try
            {
                _rules.Add((config, config.Resolve(graph.Types)));
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

    public IReadOnlyList<NetworkRule> Rules => _rules.Select(r => r.Rule).ToList();

    /// <summary>The violations of every rule, shard by shard. <see cref="Summaries"/> is complete once enumerated.</summary>
    public IEnumerable<RuleViolation<long, long>> Validate(IProgress<(int Done, int Total)>? progress = null, CancellationToken cancellationToken = default)
    {
        _checked.Clear();
        _violations.Clear();
        var rules = _rules.Select(r => r.Rule).ToList();
        var nodeRules = rules.Where(r => r.Subject == RuleSubject.Node).ToList();
        var edgeRules = rules.Where(r => r.Subject == RuleSubject.Edge).ToList();

        int done = 0, total = _graph.Metadata.Shards.Count;
        foreach (var shard in _graph.ReadShards())
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var rule in nodeRules)
            {
                Add(_checked, rule.Id, shard.Nodes.Count(n => rule.SubjectTypes.Contains(n.Node.TypeId)));
            }
            // an edge between two shards is stored in both, it is checked in the shard of its from node
            var edges = shard.Edges.Where(e => e.FromShard == shard.Id).Select(e => e.Edge).ToList();
            foreach (var rule in edgeRules)
            {
                Add(_checked, rule.Id, edges.Count(e => rule.SubjectTypes.Contains(e.TypeId)));
            }

            var violations = new List<RuleViolation<long, long>>();
            if (nodeRules.Count > 0)
            {
                violations.AddRange(_graph.ValidateNodes(shard.Nodes.Select(n => n.Node.Id), nodeRules, cancellationToken));
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

    /// <summary>One line per rule in the order of the rules.</summary>
    public IReadOnlyList<RuleSummary> Summaries
        => _rules.Select(r => new RuleSummary(r.Config, _checked.GetValueOrDefault(r.Rule.Id), _violations.GetValueOrDefault(r.Rule.Id))).ToList();

    private static void Add(Dictionary<string, long> counts, string id, long value)
        => counts[id] = counts.GetValueOrDefault(id) + value;
}
