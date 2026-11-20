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
/// <param name="Block">The blocks of all tiers, one entry per tier that has any (node references or patterns).</param>
/// <param name="Sources">The sources of the top tier (lowest order), or null when it has none (the global ones count then).</param>
/// <param name="Sinks">The sinks of the bottom tier (highest order), or null when it has none.</param>
public sealed record NetworkTiers(IReadOnlyList<ResolvedTier> Tiers, TypeSet EdgeTypes, IReadOnlyList<string> Block, string? Sources, string? Sinks)
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
            var tier = available.GetTier(name);
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
        return new NetworkTiers(
            resolved,
            resolved.Select(t => t.EdgeTypes).UnionAll(),
            resolved.Select(t => t.Config.Block.NullIfEmpty()).OfType<string>().ToList(),
            top.Config.Sources.NullIfEmpty(),
            bottom.Config.Sinks.NullIfEmpty());
    }

    /// <summary>The union of the selected tiers as one restriction.</summary>
    public ResolvedTier Union() => ResolvedTier.Union(Tiers);
}
