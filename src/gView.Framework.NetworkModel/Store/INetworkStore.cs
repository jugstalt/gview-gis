using gView.Framework.NetworkModel.Ids;
using SpatialNET.Network;

namespace gView.Framework.NetworkModel.Store;

/// <summary>Where a built network is kept: one package of nodes and edges per shard plus the metadata.</summary>
public interface INetworkStore
{
    /// <summary>The metadata of the network built before, or null; its numbering is reused for stable ids.</summary>
    NetworkMetadata? ReadMetadata();

    /// <summary>Starts writing a new network; replaces the old one on <see cref="INetworkStoreWriter.Commit"/>.</summary>
    INetworkStoreWriter CreateWriter();
}

public interface INetworkStoreWriter : INetworkWriter<long, long, FeatureRef>, IDisposable
{
    /// <summary>Writes the shards and the metadata (shard and count entries are filled by the writer).</summary>
    void Commit(NetworkMetadata metadata);
}
