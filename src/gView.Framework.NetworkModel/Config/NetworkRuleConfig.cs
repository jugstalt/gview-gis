using gView.Framework.NetworkModel.Extensions;
using SpatialNET.Network;

namespace gView.Framework.NetworkModel.Config;

/// <summary>
/// A network rule as a table row (SpatialNET ADR 0052): the elements of the subject types (nodes, or edges) count their
/// direct neighbours of the count types, and the count must lie in min..max.
/// </summary>
public class NetworkRuleConfig
{
    /// <summary>The id of the rule; unique within the rules of a network.</summary>
    public string Id { get; set; } = "";

    /// <summary>Free text for the UI and reports; ignored by the check.</summary>
    public string? Description { get; set; }

    public RuleSeverity Severity { get; set; } = RuleSeverity.Error;

    /// <summary>Node: count the edges at each node. Edge: count the other edges at the ends of each edge.</summary>
    public RuleSubject Subject { get; set; } = RuleSubject.Node;

    /// <summary>The checked types as pattern text, e.g. [*,Kasten,*]; several patterns separated by |.</summary>
    public string SubjectTypes { get; set; } = "";

    /// <summary>Edge rules only: which ends are checked (All, NodeIs, NodeIsNot).</summary>
    public RuleEndFilter EndFilter { get; set; } = RuleEndFilter.All;

    /// <summary>The node types for NodeIs / NodeIsNot.</summary>
    public string? EndTypes { get; set; }

    /// <summary>The counted edge types as pattern text; several patterns separated by | (OR).</summary>
    public string CountTypes { get; set; } = "";

    /// <summary>Optional name of a tier: the rule then only looks at the edges and nodes of that tier (subject, counted edges).</summary>
    public string? Tier { get; set; }

    public int Min { get; set; }

    /// <summary>The largest allowed count; null for no upper limit.</summary>
    public int? Max { get; set; }

    /// <summary>The row for SpatialNET; a node rule has no ends, so its end filter and end types are left out.</summary>
    public NetworkRuleDefinition ToDefinition()
        => Subject == RuleSubject.Node || EndFilter == RuleEndFilter.All
            ? new(Id, Severity, Subject, SubjectTypes, RuleEndFilter.All, null, CountTypes, Min, Max)
            : new(Id, Severity, Subject, SubjectTypes, EndFilter, string.IsNullOrWhiteSpace(EndTypes) ? null : EndTypes, CountTypes, Min, Max);

    /// <summary>Resolves the rule against the types of a network; throws an <see cref="ArgumentException"/> naming the rule.</summary>
    public NetworkRule Resolve(TypeRegistry types) => NetworkRule.FromDefinition(ToDefinition(), types);

    /// <summary>
    /// The rule restricted to a tier: the subject (edge rules) and the counted edges are cut to the edge types of the tier.
    /// A node rule still has all nodes of its subject types; the nodes of the tier are chosen by the caller
    /// (<see cref="Query.NetworkTiers.ContainsNode"/>). End types are node types and stay as they are.
    /// </summary>
    public NetworkRule Resolve(TypeRegistry types, ResolvedTier tier, NetworkRule? rule = null)
    {
        rule ??= Resolve(types);
        var count = rule.CountTypes.Intersect(tier.EdgeTypes);
        NetworkRule restricted;
        if (rule.Subject == RuleSubject.Node)
        {
            restricted = NetworkRule.ForNodes(rule.Id, rule.SubjectTypes).CountEdges(count).Between(rule.Min, rule.Max);
        }
        else
        {
            var builder = NetworkRule.ForEdges(rule.Id, rule.SubjectTypes.Intersect(tier.EdgeTypes));
            if (rule.EndFilter == RuleEndFilter.NodeIs)
            {
                builder = builder.AtEndsWhereNode(rule.EndTypes!);
            }
            else if (rule.EndFilter == RuleEndFilter.NodeIsNot)
            {
                builder = builder.AtEndsWhereNodeIsNot(rule.EndTypes!);
            }
            restricted = builder.CountOtherEdges(count).Between(rule.Min, rule.Max);
        }
        return rule.Severity == RuleSeverity.Warning ? restricted.AsWarning() : restricted;
    }

    public override string ToString()
        => $"{Id}: {Subject} {SubjectTypes}{(EndFilter == RuleEndFilter.All ? "" : $" ends {EndFilter} {EndTypes}")} count {CountTypes} {Min}..{(Max?.ToString() ?? "*")}";
}
