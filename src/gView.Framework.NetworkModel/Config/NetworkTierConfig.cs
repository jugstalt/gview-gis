using gView.Framework.NetworkModel.Graph;
using SpatialNET.Network;

namespace gView.Framework.NetworkModel.Config;

/// <summary>
/// A tier ("Ebene"), e.g. high, medium or low voltage: a part of the network that searches can be restricted to.
/// Edges belong to a tier by their type (<see cref="Filter"/>); a node belongs to every tier whose edges touch it
/// (a transformer between medium and low voltage is in both), plus the nodes of <see cref="Nodes"/>.
/// Written into network.json on build and by refresh metadata.
/// </summary>
public class NetworkTierConfig
{
    /// <summary>The name; unique within the tiers (compared ignoring case), no comma.</summary>
    public string Name { get; set; } = "";

    /// <summary>The rank: lower is higher up (high voltage before medium before low). Decides which tier's sources and sinks count with several tiers.</summary>
    public int Order { get; set; }

    /// <summary>The edge types of the tier as pattern text, e.g. Mittelspannung.*; see the pattern syntax (| &amp; !).</summary>
    public string Filter { get; set; } = "";

    /// <summary>Optional node pattern: nodes that belong to the tier although no edge of it touches them.</summary>
    public string? Nodes { get; set; }

    /// <summary>The sources of the tier (node references or pattern); replace the global supply sources when the tier is searched.</summary>
    public string? Sources { get; set; }

    /// <summary>The sinks of the tier (node references or pattern); replace the global customers when the tier is searched.</summary>
    public string? Sinks { get; set; }

    /// <summary>Nodes blocked in every search on the tier (node references or pattern).</summary>
    public string? Block { get; set; }

    /// <summary>Checks the patterns against the types of a network; throws an <see cref="ArgumentException"/> naming the tier.</summary>
    public ResolvedTier Resolve(TypeRegistry types)
    {
        if (string.IsNullOrWhiteSpace(Name) || Name.Contains(','))
        {
            throw new ArgumentException($"Tier '{Name}': a tier needs a name without a comma.");
        }
        if (string.IsNullOrWhiteSpace(Filter))
        {
            throw new ArgumentException($"Tier '{Name}': the filter (edge types) is empty.");
        }

        TypeSet edges;
        try
        {
            edges = types.Edges(Filter);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Tier '{Name}': filter is not valid. {ex.Message}", ex);
        }

        var nodes = ResolveNodes(types, "nodes", Nodes);
        // references to single nodes can only be checked against the network; patterns against its types
        ResolveNodes(types, "sources", Sources);
        ResolveNodes(types, "sinks", Sinks);
        ResolveNodes(types, "block", Block);
        return new ResolvedTier(this, edges, nodes);
    }

    private TypeSet? ResolveNodes(TypeRegistry types, string what, string? references)
    {
        if (string.IsNullOrWhiteSpace(references))
        {
            return null;
        }
        var patterns = FileNetworkGraph.SplitReferences(references).Where(FileNetworkGraph.IsTypePattern).ToArray();
        if (patterns.Length == 0)
        {
            return null;
        }
        try
        {
            return types.Nodes(patterns);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Tier '{Name}': {what} is not valid. {ex.Message}", ex);
        }
    }

    public override string ToString() => $"{Name} (order {Order}): {Filter}";
}

/// <summary>A tier with its patterns resolved against the types of a network.</summary>
/// <param name="Config">The tier.</param>
/// <param name="EdgeTypes">The edge types of the tier.</param>
/// <param name="NodeTypes">The node types of <see cref="NetworkTierConfig.Nodes"/>, or null.</param>
public sealed record ResolvedTier(NetworkTierConfig Config, TypeSet EdgeTypes, TypeSet? NodeTypes)
{
    public string Name => Config.Name;

    /// <summary>The union of several tiers: all their edge types, and the node types of those that have any.</summary>
    public static ResolvedTier Union(IReadOnlyList<ResolvedTier> tiers)
    {
        if (tiers.Count == 1)
        {
            return tiers[0];
        }
        var edges = tiers.Select(t => t.EdgeTypes).Aggregate((a, b) => a.Union(b));
        var nodeSets = tiers.Where(t => t.NodeTypes is not null).Select(t => t.NodeTypes!).ToList();
        return new ResolvedTier(
            new NetworkTierConfig { Name = string.Join(",", tiers.Select(t => t.Name)), Order = tiers.Min(t => t.Config.Order) },
            edges,
            nodeSets.Count == 0 ? null : nodeSets.Aggregate((a, b) => a.Union(b)));
    }
}
