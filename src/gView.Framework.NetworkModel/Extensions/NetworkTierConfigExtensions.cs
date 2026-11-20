using gView.Framework.NetworkModel.Config;

namespace gView.Framework.NetworkModel.Extensions;

internal static class NetworkTierConfigExtensions
{
    /// <summary>The tier of that name (ignoring case and surrounding white space); null when there is none.</summary>
    public static NetworkTierConfig? FindTier(this IEnumerable<NetworkTierConfig> tiers, string? name)
        => tiers.FirstOrDefault(t => t.Name.Equals(name?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>The tier of that name; throws an <see cref="ArgumentException"/> listing the available tiers when there is none.</summary>
    public static NetworkTierConfig GetTier(this IReadOnlyCollection<NetworkTierConfig> tiers, string? name)
        => tiers.FindTier(name)
            ?? throw new ArgumentException($"Unknown tier '{name}'. Tiers: {(tiers.Count == 0 ? "none (add tiers to the network config and refresh the metadata)" : string.Join(", ", tiers.Select(t => t.Name)))}");
}
