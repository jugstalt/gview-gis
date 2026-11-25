using gView.Framework.NetworkModel.Config;

namespace gView.Framework.NetworkModel.Extensions;

internal static class ResolvedTierExtensions
{
    /// <summary>The tier of that name (ignoring case and surrounding white space); null when there is none.</summary>
    public static ResolvedTier? FindTier(this IEnumerable<ResolvedTier> tiers, string? name)
        => tiers.FirstOrDefault(t => t.Name.Equals(name?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>The tier of that name; throws an <see cref="ArgumentException"/> listing the available tiers when there is none.</summary>
    public static ResolvedTier GetTier(this IReadOnlyCollection<ResolvedTier> tiers, string? name)
        => tiers.FindTier(name)
            ?? throw new ArgumentException($"Unknown tier '{name}'. Tiers: {(tiers.Count == 0 ? "none (the tiers are the values of the first facet)" : string.Join(", ", tiers.Select(t => t.Name)))}");
}
