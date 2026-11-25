namespace gView.Framework.NetworkModel.Query;

/// <summary>
/// What a search needs; which fields count depends on the operation:
/// trace/reach: Start; path: From, To; supply: Sources, Sinks; feeders: Sources, Branches; info: Node; validate: Rules (default: the rules of network.json), RuleIds;
/// nearest: X, Y, Radius, Count; outage: Disable, Sources, Sinks (supply and outage default to SupplySources/Customers of network.json).
/// Nodes are Layer#OID, raw ids, type patterns or @x,y[~radius], comma separated; edges Layer#OID[/part] or raw ids.
/// </summary>
public sealed record NetworkQueryRequest
{
    public string? Start { get; init; }
    public string? From { get; init; }
    public string? To { get; init; }
    public string? Sources { get; init; }
    public string? Sinks { get; init; }
    public string? Node { get; init; }

    /// <summary>
    /// feeders: the type pattern of the edges at a source that start a branch (Abzweig), e.g. [Niederspannung,Anlagenverbinder,*];
    /// empty: every edge at a source starts a branch.
    /// </summary>
    public string? Branches { get; init; }

    /// <summary>
    /// feeders: also the edges at a node a branch went through that no branch went along (switched off, against their
    /// direction), as Kind Open with the branches of the ends that were reached.
    /// </summary>
    public bool OpenEdges { get; init; }

    /// <summary>Only edges of these types (pattern).</summary>
    public string? Filter { get; init; }

    /// <summary>
    /// trace, reach, path, supply, outage, validate: only these tiers (Ebenen, names from network.json, comma separated).
    /// The edge types searched are those of the tiers (together with <see cref="Filter"/>), the tiers' blocks are added to
    /// <see cref="Block"/>, and supply/outage use the sources of the top and the sinks of the bottom tier instead of the
    /// global ones. validate checks every rule on the tiers only.
    /// </summary>
    public string? Tier { get; init; }

    /// <summary>
    /// trace with a tier: stop at the sources of the tier (default true, like a subnetwork trace); false goes on through them.
    /// </summary>
    public bool? StopAtSources { get; init; }

    /// <summary>Nodes blocked for this search.</summary>
    public string? Block { get; init; }

    /// <summary>Nodes opened for this search.</summary>
    public string? Unblock { get; init; }

    /// <summary>Edges switched off for this search (Layer#OID = every part of the line, Layer#OID/part, raw ids); outage: the edges to switch off.</summary>
    public string? Disable { get; init; }

    /// <summary>nearest: the location, in the spatial reference of the network.</summary>
    public double? X { get; init; }

    /// <summary>nearest: the location, in the spatial reference of the network.</summary>
    public double? Y { get; init; }

    /// <summary>nearest: the search radius; default 10 x the build tolerance.</summary>
    public double? Radius { get; init; }

    /// <summary>nearest: how many nodes at most; default 1.</summary>
    public int? Count { get; init; }

    /// <summary>The weight name; default length.</summary>
    public string? Weight { get; init; }

    public double? MaxCost { get; init; }

    /// <summary>validate: the rules to check instead of those of network.json.</summary>
    public List<Config.NetworkRuleConfig>? Rules { get; init; }

    /// <summary>validate: only these rule ids (comma separated).</summary>
    public string? RuleIds { get; init; }

    /// <summary>
    /// trace, reach, path, supply, outage, validate: only the part of the network inside this area. Nodes outside do not
    /// exist for the search; edges leaving the area are reported as Boundary. Explicit nodes (Layer#OID, id, @x,y)
    /// outside are an error, nodes of a type pattern outside are left out. validate checks the nodes inside and the edges
    /// with an end inside (with all their neighbors).
    /// </summary>
    public NetworkArea? Area { get; init; }

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
public sealed record NetworkResultEdge(long Id, string Label, string Layer, long Oid, string Type, double Length, double FromMeasure, double ToMeasure, long From, long To, double FromX, double FromY, double ToX, double ToY)
{
    /// <summary>What the edge is in the result: Edge, Boundary (leaves the area of the search, not followed), for outage Disabled (switched off) and Dead (no supply any more), for feeders Multiple (reached from more than one branch) and Open (not gone along, at a node a branch went through).</summary>
    public string Kind { get; init; } = "Edge";

    /// <summary>feeders: the source (feeder) of the edge as Layer#OID; several, space separated and cheapest first, for Kind Multiple.</summary>
    public string? Source { get; init; }

    /// <summary>feeders: the branch (Abzweig) of the edge, the feature of its first edge at the source as Layer#OID; several like <see cref="Source"/>.</summary>
    public string? Branch { get; init; }

    /// <summary>feeders: the cost from the source to the near end of the edge (the cheapest for Kind Multiple).</summary>
    public double? Cost { get; init; }
}

/// <summary>A rule violation at a node or an edge.</summary>
public sealed record NetworkResultViolation(
    string Rule, string Severity, string? Message, string Kind,
    long Node, string NodeLabel, string? Layer, long? Oid,
    long? Edge, string? EdgeLabel, string? EdgeLayer, long? EdgeOid,
    string Type, long Count, double? Min, double? Max, double X, double Y, IReadOnlyList<string> Aliases)
{
    /// <summary>
    /// problems: every feature involved (layer path, oid), also beyond the two of <see cref="Layer"/>/<see cref="Oid"/>
    /// and <see cref="EdgeLayer"/>/<see cref="EdgeOid"/>; null for rule violations.
    /// </summary>
    public IReadOnlyList<(string Layer, long Oid)>? Features { get; init; }
}

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

    /// <summary>True when the result was cut at a limit (see <see cref="Truncate"/>).</summary>
    public bool ExceededLimit { get; private set; }

    /// <summary>
    /// Keeps the first <paramref name="maxNodes"/> nodes, of the rest violations, and the first <paramref name="maxNodes"/>
    /// edges; marks <see cref="ExceededLimit"/>. Too many edges alone cut the result too (feeders: few sources, all edges).
    /// The limit applies to the result, not to the search itself.
    /// </summary>
    public void Truncate(int maxNodes)
    {
        if (maxNodes <= 0 || (Nodes.Count + Violations.Count <= maxNodes && Edges.Count <= maxNodes))
        {
            return;
        }
        ExceededLimit = true;
        if (Nodes.Count > maxNodes)
        {
            Nodes.RemoveRange(maxNodes, Nodes.Count - maxNodes);
        }
        int violations = Math.Max(0, maxNodes - Nodes.Count);
        if (Violations.Count > violations)
        {
            Violations.RemoveRange(violations, Violations.Count - violations);
        }
        if (Edges.Count > maxNodes)
        {
            Edges.RemoveRange(maxNodes, Edges.Count - maxNodes);
        }
        Messages.Add($"Result cut at {maxNodes} nodes.");
    }

    /// <summary>path: the cost of the path.</summary>
    public double? Cost { get; set; }

    /// <summary>The spatial reference of the coordinates, e.g. "epsg:31256".</summary>
    public string? Srs { get; set; }
}
