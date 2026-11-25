using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Extensions;
using gView.Framework.NetworkModel.Store;
using System.Text.Json;

namespace gView.Framework.NetworkModel.Build;

/// <summary>
/// Rewrites the settings of a built network that do not depend on the data (supply sources, customers, rules, tiers) from
/// a config, without building the shards again. Searches see the change as soon as network.json is replaced
/// (the graph cache opens a network again when its stamp changes).
/// </summary>
public static class NetworkMetadataRefresh
{
    /// <summary>Refreshes network.json in the directory; returns the new metadata. Throws when the config, its tiers or its rules are invalid, or there is no network.</summary>
    public static NetworkMetadata Refresh(NetworkConfig config, string directory)
        => Refresh(config, directory, out _);

    /// <summary>
    /// Refreshes network.json in the directory; returns the new metadata and, in <paramref name="warnings"/>, the changes
    /// a refresh can't bring into the graph: with separate tiers the sources of a tier decide on build which lines connect
    /// at their ends, so changed sources need a new build.
    /// </summary>
    public static NetworkMetadata Refresh(NetworkConfig config, string directory, out List<string> warnings)
    {
        config.ValidateTiersAndRules();

        var path = Path.Combine(directory, FileNetworkStore.MetadataFile);
        var metadata = new FileNetworkStore(directory).ReadMetadata()
            ?? throw new FileNotFoundException($"No built network in {directory} ({FileNetworkStore.MetadataFile} missing); build it first.");

        var types = metadata.ToTypeRegistry();
        config.ResolveTiers(types);
        config.ResolveRules(types);

        warnings = config.SeparateTiers || metadata.Types.Any(t => t.Helper)
            ? ChangedTierSources(metadata.Tiers, config.Tiers)
            : [];

        metadata.SupplySources = config.SupplySources;
        metadata.Customers = config.Customers;
        metadata.Rules = config.Rules;
        metadata.Tiers = config.Tiers;

        // replace the file in one step, so a search never reads half a file
        var temp = path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(metadata, NetworkConfig.JsonOptions));
        File.Move(temp, path, overwrite: true);
        return metadata;
    }

    /// <summary>The tiers whose sources differ between the built network and the config, as warnings.</summary>
    internal static List<string> ChangedTierSources(IEnumerable<NetworkTierConfig> built, IEnumerable<NetworkTierConfig> config)
    {
        static string Sources(NetworkTierConfig? tier) => tier?.Sources?.Trim() ?? "";

        var before = built.GroupBy(t => t.Name.Trim(), StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var after = config.GroupBy(t => t.Name.Trim(), StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        return before.Keys.Union(after.Keys, StringComparer.OrdinalIgnoreCase)
            .Where(name => !Sources(before.GetValueOrDefault(name)).Equals(Sources(after.GetValueOrDefault(name)), StringComparison.Ordinal))
            .Order(StringComparer.OrdinalIgnoreCase)
            .Select(name => $"Tier '{name}': sources changed ({Text(Sources(before.GetValueOrDefault(name)))} -> {Text(Sources(after.GetValueOrDefault(name)))}). " +
                            "With separate tiers they decide on build where lines connect: build the network again.")
            .ToList();

        static string Text(string sources) => sources.Length == 0 ? "none" : sources;
    }
}
