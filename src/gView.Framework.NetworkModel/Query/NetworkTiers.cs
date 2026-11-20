using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Extensions;
using SpatialNET.Network;

namespace gView.Framework.NetworkModel.Query;

/// <summary>
/// The tiers a search is restricted to, resolved against a network: the edge types searched, the nodes blocked and the
/// sources and sinks to use (see <see cref="NetworkQueryRequest.Tier"/>).
/// </summary>
/// <param name="Tiers">The selected tiers, in the order of the request.</param>
/// <param name="EdgeTypes">The union of the edge types of the tiers.</param>
/// <param name="Block">The blocks of all tiers (node references), or null.</param>
/// <param name="Sources">The sources of the top tier (lowest order), or null when it has none (the global ones count then).</param>
/// <param name="Sinks">The sinks of the bottom tier (highest order), or null when it has none.</param>
public sealed record NetworkTiers(IReadOnlyList<ResolvedTier> Tiers, TypeSet EdgeTypes, string? Block, string? Sources, string? Sinks)
{
    public string Names => string.Join(", ", Tiers.Select(t => t.Name));

    /// <summary>The tiers of a comma separated list of names; null for an empty list. Throws an <see cref="ArgumentException"/> for an unknown name.</summary>
    public static NetworkTiers? Select(FileNetworkGraph graph, string? names)
    {
        if (string.IsNullOrWhiteSpace(names))
        {
            return null;
        }
        var available = graph.Metadata.Tiers;
        var selected = new List<NetworkTierConfig>();
        foreach (var name in names.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var tier = available.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                ?? throw new ArgumentException($"Unknown tier '{name}'. Tiers: {(available.Count == 0 ? "none (add tiers to the network config and refresh the metadata)" : string.Join(", ", available.Select(t => t.Name)))}");
            if (!selected.Contains(tier))
            {
                selected.Add(tier);
            }
        }
        if (selected.Count == 0)
        {
            return null;
        }
        var resolved = selected.Select(t => t.Resolve(graph.Types)).ToList();
        var top = resolved.MinBy(t => t.Config.Order)!;
        var bottom = resolved.MaxBy(t => t.Config.Order)!;
        var blocks = resolved.Select(t => t.Config.Block).Where(b => !string.IsNullOrWhiteSpace(b)).ToList();
        return new NetworkTiers(
            resolved,
            resolved.Select(t => t.EdgeTypes).Aggregate((a, b) => a.Union(b)),
            blocks.Count == 0 ? null : string.Join(",", blocks),
            NullIfEmpty(top.Config.Sources),
            NullIfEmpty(bottom.Config.Sinks));
    }

    /// <summary>The union of the selected tiers as one restriction.</summary>
    public ResolvedTier Union() => ResolvedTier.Union(Tiers);

    /// <summary>Whether a node belongs to the tier: an edge of the tier touches it, or its type is one of the tier's node types.</summary>
    public static bool ContainsNode(ResolvedTier tier, int nodeTypeId, IEnumerable<int> edgeTypeIdsAtNode)
        => (tier.NodeTypes?.Contains(nodeTypeId) ?? false) || edgeTypeIdsAtNode.Any(tier.EdgeTypes.Contains);

    private static string? NullIfEmpty(string? text) => string.IsNullOrWhiteSpace(text) ? null : text;
}
