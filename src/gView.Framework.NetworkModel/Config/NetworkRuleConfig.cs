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

    public int Min { get; set; }

    /// <summary>The largest allowed count; null for no upper limit.</summary>
    public int? Max { get; set; }

    public NetworkRuleDefinition ToDefinition()
        => new(Id, Severity, Subject, SubjectTypes, EndFilter, string.IsNullOrWhiteSpace(EndTypes) ? null : EndTypes, CountTypes, Min, Max);

    /// <summary>Resolves the rule against the types of a network; throws an <see cref="ArgumentException"/> naming the rule.</summary>
    public NetworkRule Resolve(TypeRegistry types) => NetworkRule.FromDefinition(ToDefinition(), types);

    public override string ToString()
        => $"{Id}: {Subject} {SubjectTypes}{(EndFilter == RuleEndFilter.All ? "" : $" ends {EndFilter} {EndTypes}")} count {CountTypes} {Min}..{(Max?.ToString() ?? "*")}";
}
