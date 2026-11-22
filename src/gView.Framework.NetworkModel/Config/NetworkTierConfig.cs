using gView.Framework.NetworkModel.Extensions;
using gView.Framework.NetworkModel.Graph;
using SpatialNET.Network;

namespace gView.Framework.NetworkModel.Config;

/// <summary>
/// A tier ("Ebene"), e.g. high, medium or low voltage: a part of the network that searches can be restricted to.
/// The tier of a type is the value of its first facet: every value of the first facet that edge types use is a tier,
/// an entry here only adds settings (order, sources, sinks, block). Edges belong to the tier of their type; a node
/// belongs to the tier of its type and to every tier whose edges touch it (a transformer between medium and low
/// voltage is in both). Written into network.json on build and by refresh metadata.
/// </summary>
public class NetworkTierConfig
{
    /// <summary>The name: a value of the first facet, e.g. Mittelspannung (compared ignoring case).</summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// The rank: lower is higher up (high voltage before medium before low). Decides which tier's sources and sinks count
    /// with several tiers. Empty: the number of the facet value.
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    /// The sources of the tier (node references or pattern); replace the global supply sources when the tier is searched.
    /// With separate tiers, line ends of the tier may connect to nodes of these types (the transformer of a low voltage tier).
    /// </summary>
    public string? Sources { get; set; }

    /// <summary>The sinks of the tier (node references or pattern); replace the global customers when the tier is searched.</summary>
    public string? Sinks { get; set; }

    /// <summary>Nodes blocked in every search on the tier (node references or pattern).</summary>
    public string? Block { get; set; }

    /// <summary>
    /// Checks the tier against the types of a network: its name must be a value of the first facet that edge types use,
    /// its patterns must be valid. Throws an <see cref="ArgumentException"/> naming the tier.
    /// </summary>
    public ResolvedTier Resolve(TypeRegistry types)
    {
        if (string.IsNullOrWhiteSpace(Name) || Name.Contains(','))
        {
            throw new ArgumentException($"Tier '{Name}': a tier needs a name without a comma.");
        }

        var facet = types.Facets[0];
        int value = facet.Values.FirstOrDefault(v => v.Value.Equals(Name.Trim(), StringComparison.OrdinalIgnoreCase)).Key;
        if (value == 0)
        {
            throw new ArgumentException($"Tier '{Name}': the tier is a value of the first facet '{facet.Name}', which has no value '{Name}'. Values: {string.Join(", ", facet.Values.OrderBy(v => v.Key).Select(v => v.Value))}");
        }

        var edges = TypeSet.Of(types.Types.Where(t => t.Kind == ElementKind.Edge && t.Facets.Count > 0 && t.Facets[0] == value).Select(t => t.Id));
        if (edges.Count == 0)
        {
            throw new ArgumentException($"Tier '{Name}': no edge type has '{Name}' in the facet '{facet.Name}'.");
        }

        var nodes = TypeSet.Of(types.Types.Where(t => t.Kind == ElementKind.Node && t.Facets.Count > 0 && t.Facets[0] == value).Select(t => t.Id));

        // references to single nodes can only be checked against the network; patterns against its types
        ResolveNodes(types, "sources", Sources);
        ResolveNodes(types, "sinks", Sinks);
        ResolveNodes(types, "block", Block);
        return new ResolvedTier(this, value, Order ?? value, edges, nodes);
    }

    /// <summary>The node types of the type patterns among the references (sources, sinks, block), or null when there are none.</summary>
    internal TypeSet? ResolveNodes(TypeRegistry types, string what, string? references)
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

    public override string ToString() => Order is { } order ? $"{Name} (order {order})" : Name;

    /// <summary>
    /// Every tier of a network, by rank: the entries, and an implicit tier for every value of the first facet that edge
    /// types use and that has no entry. Throws an <see cref="ArgumentException"/> naming a faulty or duplicate entry.
    /// </summary>
    public static List<ResolvedTier> ResolveAll(IEnumerable<NetworkTierConfig> entries, TypeRegistry types)
    {
        var resolved = new List<ResolvedTier>();
        foreach (var entry in entries)
        {
            var tier = entry.Resolve(types);
            if (resolved.Any(t => t.Value == tier.Value))
            {
                throw new ArgumentException($"The tier '{entry.Name}' is defined twice.");
            }
            resolved.Add(tier);
        }

        var used = types.Types.Where(t => t.Kind == ElementKind.Edge && t.Facets.Count > 0 && t.Facets[0] != 0).Select(t => t.Facets[0]).Distinct();
        foreach (int value in used.Where(v => !resolved.Any(t => t.Value == v)))
        {
            resolved.Add(new NetworkTierConfig { Name = types.Facets[0].Values[value] }.Resolve(types));
        }

        return resolved.OrderBy(t => t.Rank).ThenBy(t => t.Value).ToList();
    }
}

/// <summary>A tier with its types resolved against a network.</summary>
/// <param name="Config">The tier; an entry of the config, or an implicit one for a value of the first facet without entry.</param>
/// <param name="Value">The value of the first facet.</param>
/// <param name="Rank">The order: <see cref="NetworkTierConfig.Order"/>, or the facet value.</param>
/// <param name="EdgeTypes">The edge types of the tier.</param>
/// <param name="NodeTypes">The node types of the tier (helper types included).</param>
public sealed record ResolvedTier(NetworkTierConfig Config, int Value, int Rank, TypeSet EdgeTypes, TypeSet NodeTypes)
{
    public string Name => Config.Name;

    /// <summary>The edge types of the tier as a pattern, e.g. [Mittelspannung,*,*].</summary>
    public string Pattern(TypeRegistry types) => $"[{string.Join(",", Enumerable.Repeat("*", types.Facets.Count - 1).Prepend(Name))}]";

    /// <summary>Whether a node belongs to the tier: its type is one of the tier, or an edge of the tier touches it.</summary>
    public bool ContainsNode(int nodeTypeId, IEnumerable<int> edgeTypeIdsAtNode)
        => NodeTypes.Contains(nodeTypeId) || edgeTypeIdsAtNode.Any(EdgeTypes.Contains);

    /// <summary>The union of several tiers: all their edge types and node types.</summary>
    public static ResolvedTier Union(IReadOnlyList<ResolvedTier> tiers)
    {
        if (tiers.Count == 1)
        {
            return tiers[0];
        }
        int rank = tiers.Min(t => t.Rank);
        return new ResolvedTier(
            new NetworkTierConfig { Name = string.Join(",", tiers.Select(t => t.Name)), Order = rank },
            0,
            rank,
            tiers.Select(t => t.EdgeTypes).UnionAll(),
            tiers.Select(t => t.NodeTypes).UnionAll());
    }
}
