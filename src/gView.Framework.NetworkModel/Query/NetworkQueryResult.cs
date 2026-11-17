namespace gView.Framework.NetworkModel.Query;

/// <summary>
/// What a search needs; which fields count depends on the operation:
/// trace/reach: Start; path: From, To; supply: Sources, Sinks; info: Node; validate: Rules (default: the rules of network.json), RuleIds.
/// Nodes are Layer#OID, raw ids or type patterns, comma separated.
/// </summary>
public sealed record NetworkQueryRequest
{
    public string? Start { get; init; }
    public string? From { get; init; }
    public string? To { get; init; }
    public string? Sources { get; init; }
    public string? Sinks { get; init; }
    public string? Node { get; init; }

    /// <summary>Only edges of these types (pattern).</summary>
    public string? Filter { get; init; }

    /// <summary>Nodes blocked for this search.</summary>
    public string? Block { get; init; }

    /// <summary>Nodes opened for this search.</summary>
    public string? Unblock { get; init; }

    /// <summary>The weight name; default length.</summary>
    public string? Weight { get; init; }

    public double? MaxCost { get; init; }

    /// <summary>validate: the rules to check instead of those of network.json.</summary>
    public List<Config.NetworkRuleConfig>? Rules { get; init; }

    /// <summary>validate: only these rule ids (comma separated).</summary>
    public string? RuleIds { get; init; }

    /// <summary>Whether the edges a search went along are wanted (trace computes them extra).</summary>
    public bool WithEdges { get; init; } = true;
}

/// <summary>A node of a result. Merged points (aliases) are rows of their own with <see cref="AliasOf"/> set.</summary>
/// <param name="Id">The network id.</param>
/// <param name="Label">Layer#OID or "helper".</param>
/// <param name="Layer">The layer path in the map; null for helper nodes.</param>
/// <param name="Oid">The feature id; null for helper nodes.</param>
/// <param name="Type">The type name.</param>
/// <param name="Kind">What the node is in the result (Reached, Blocked, Start, From, Via, To, Supplied, NotSupplied, ...).</param>
/// <param name="Cost">The cost to get there.</param>
/// <param name="Source">The node(s) it was reached from or is supplied by, as labels.</param>
/// <param name="AliasOf">For a merged point: the label of the node it was merged into.</param>
public sealed record NetworkResultNode(long Id, string Label, string? Layer, long? Oid, string Type, string Kind, double Cost, string Source, string AliasOf, double X, double Y);

/// <summary>An edge of a result: a part of a line feature between two measures.</summary>
public sealed record NetworkResultEdge(long Id, string Label, string Layer, long Oid, string Type, double Length, double FromMeasure, double ToMeasure, long From, long To, double FromX, double FromY, double ToX, double ToY);

/// <summary>A rule violation at a node or an edge.</summary>
public sealed record NetworkResultViolation(
    string Rule, string Severity, string? Message, string Kind,
    long Node, string NodeLabel, string? Layer, long? Oid,
    long? Edge, string? EdgeLabel, string? EdgeLayer, long? EdgeOid,
    string Type, long Count, double? Min, double? Max, double X, double Y, IReadOnlyList<string> Aliases);

/// <summary>One line per rule of a validation.</summary>
public sealed record NetworkResultRuleSummary(string Rule, string Severity, string? Description, long Checked, long Violations);

/// <summary>The result of a search, the same for gView.Cmd, Carto and the server.</summary>
public sealed class NetworkQueryResult
{
    public string Operation { get; init; } = "";

    /// <summary>False when nothing was found that the operation needs (no path, no rules...).</summary>
    public bool Success { get; set; } = true;

    /// <summary>A readable report, line by line.</summary>
    public List<string> Messages { get; } = [];

    public List<NetworkResultNode> Nodes { get; } = [];
    public List<NetworkResultEdge> Edges { get; } = [];
    public List<NetworkResultViolation> Violations { get; } = [];
    public List<NetworkResultRuleSummary> Rules { get; } = [];

    /// <summary>path: the cost of the path.</summary>
    public double? Cost { get; set; }

    /// <summary>The spatial reference of the coordinates, e.g. "epsg:31256".</summary>
    public string? Srs { get; init; }
}
