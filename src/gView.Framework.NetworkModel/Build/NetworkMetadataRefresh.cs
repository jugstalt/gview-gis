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
    /// <summary>Refreshes network.json in the directory; returns the new metadata. Throws when the config or its tiers are invalid, or there is no network.</summary>
    public static NetworkMetadata Refresh(NetworkConfig config, string directory)
    {
        config.ValidateTiersAndRules();

        var path = Path.Combine(directory, FileNetworkStore.MetadataFile);
        var metadata = new FileNetworkStore(directory).ReadMetadata()
            ?? throw new FileNotFoundException($"No built network in {directory} ({FileNetworkStore.MetadataFile} missing); build it first.");

        config.ResolveTiers(metadata.ToTypeRegistry());

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
}
