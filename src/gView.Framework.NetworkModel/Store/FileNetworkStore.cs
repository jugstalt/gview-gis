using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Ids;
using SpatialNET.Network;
using System.Text.Json;

namespace gView.Framework.NetworkModel.Store;

/// <summary>
/// Keeps a network in a directory: network.json (metadata) and shards/{id}.bin, one binary file per shard
/// (optionally shards/{id}.json for debugging). An edge between two shards is stored in both.
/// </summary>
public sealed class FileNetworkStore : INetworkStore
{
    public const string MetadataFile = "network.json";
    public const string ShardDirectory = "shards";

    private readonly string _directory;
    private readonly bool _writeJson;

    public FileNetworkStore(string directory, bool writeJson = false)
    {
        _directory = directory;
        _writeJson = writeJson;
    }

    public NetworkMetadata? ReadMetadata()
    {
        var path = Path.Combine(_directory, MetadataFile);
        return File.Exists(path)
            ? JsonSerializer.Deserialize<NetworkMetadata>(File.ReadAllText(path), NetworkConfig.JsonOptions)
            : null;
    }

    public INetworkStoreWriter CreateWriter() => new Writer(_directory, _writeJson);

    /// <summary>Reads one shard back (nodes and edges with their sources).</summary>
    public ShardData ReadShard(long shardId)
    {
        using var stream = File.OpenRead(Path.Combine(_directory, ShardDirectory, ShardFileName(shardId)));
        return ShardBinary.Read(stream);
    }

    public static string ShardFileName(long shardId) => $"{shardId}.bin";

    private sealed class Writer : INetworkStoreWriter
    {
        private readonly string _directory;
        private readonly bool _writeJson;
        private readonly Dictionary<long, ShardData> _shards = [];
        private readonly List<(long AliasId, long NodeId, int TypeId)> _aliases = [];
        private int _nodeCount, _edgeCount;

        public Writer(string directory, bool writeJson)
        {
            _directory = directory;
            _writeJson = writeJson;
        }

        private ShardData Shard(long id)
        {
            if (!_shards.TryGetValue(id, out var shard))
            {
                _shards[id] = shard = new ShardData(id);
            }
            return shard;
        }

        public void WriteNode(in BuiltNode<long, FeatureRef> node)
        {
            Shard(node.Shard).Nodes.Add(new ShardNode(node.Node, node.IsHelper, node.IsHelper ? null : node.Source));
            _nodeCount++;
            foreach (var merged in node.Merged ?? [])
            {
                _aliases.Add((merged.Id, node.Node.Id, merged.TypeId));
            }
        }

        public void WriteEdge(in BuiltEdge<long, long, FeatureRef> edge)
        {
            var shardEdge = new ShardEdge(edge.Edge, edge.Source, edge.FromMeasure, edge.ToMeasure, edge.FromShard, edge.ToShard);
            Shard(edge.FromShard).Edges.Add(shardEdge);
            if (edge.ToShard != edge.FromShard)
            {
                Shard(edge.ToShard).Edges.Add(shardEdge);
            }
            _edgeCount++;
        }

        public void Commit(NetworkMetadata metadata)
        {
            Directory.CreateDirectory(_directory);
            var shardDir = Path.Combine(_directory, ShardDirectory);

            // write into a new directory, then swap, so a failed build leaves the old network intact
            var tempDir = shardDir + ".new";
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            Directory.CreateDirectory(tempDir);

            var keysById = metadata.ShardKeys.ToDictionary(k => k.Value, k => k.Key);
            metadata.Shards.Clear();
            foreach (var shard in _shards.Values.OrderBy(s => s.Id))
            {
                var fileName = ShardFileName(shard.Id);
                using (var stream = File.Create(Path.Combine(tempDir, fileName)))
                {
                    ShardBinary.Write(stream, shard);
                }
                if (_writeJson)
                {
                    File.WriteAllText(Path.Combine(tempDir, $"{shard.Id}.json"), ShardJson.Serialize(shard));
                }
                metadata.Shards.Add(new NetworkShardMetadata
                {
                    Id = shard.Id,
                    Key = keysById.GetValueOrDefault(shard.Id),
                    NodeCount = shard.Nodes.Count,
                    EdgeCount = shard.Edges.Count,
                    File = $"{ShardDirectory}/{fileName}"
                });
            }
            metadata.NodeCount = _nodeCount;
            metadata.EdgeCount = _edgeCount;

            if (Directory.Exists(shardDir))
            {
                Directory.Delete(shardDir, true);
            }
            Directory.Move(tempDir, shardDir);

            NodeIndex.Write(
                Path.Combine(_directory, NodeIndex.FileName),
                _shards.Values.SelectMany(s => s.Nodes.Select(n => (n.Node.Id, s.Id, n.Node.TypeId))));

            AliasIndex.Write(Path.Combine(_directory, AliasIndex.FileName), _aliases);
            metadata.AliasCount = _aliases.Count;

            File.WriteAllText(Path.Combine(_directory, MetadataFile), JsonSerializer.Serialize(metadata, NetworkConfig.JsonOptions));
        }

        public void Dispose()
        {
            _shards.Clear();
            _aliases.Clear();
        }
    }
}
