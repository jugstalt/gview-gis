using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Ids;
using SpatialNET.Network;
using System.Text.Json;

namespace gView.Framework.NetworkModel.Store;

/// <summary>
/// Keeps a network in a directory: network.json (metadata) and shards/{id}.bin, one binary file per shard
/// (optionally shards/{id}.json for debugging), plus nodes.idx, edges.idx, aliases.idx and spatial.idx.
/// An edge between two shards is stored in both.
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
        // read completely, so no file stays open and a new build can swap the directory
        using var stream = new MemoryStream(File.ReadAllBytes(Path.Combine(_directory, ShardDirectory, ShardFileName(shardId))));
        return ShardBinary.Read(stream);
    }

    public static string ShardFileName(long shardId) => $"{shardId}.bin";

    private sealed class Writer : INetworkStoreWriter
    {
        private readonly string _directory;
        private readonly bool _writeJson;
        private readonly Dictionary<long, ShardData> _shards = [];
        private readonly List<(long AliasId, long NodeId, int TypeId)> _aliases = [];
        private readonly List<SpatialEntry> _points = [];
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
            _points.Add(new SpatialEntry(node.Node.Id, node.Node.Id, node.Node.Location.X, node.Node.Location.Y));
            foreach (var merged in node.Merged ?? [])
            {
                _aliases.Add((merged.Id, node.Node.Id, merged.TypeId));
                _points.Add(new SpatialEntry(merged.Id, node.Node.Id, merged.Location.X, merged.Location.Y));
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

        /// <summary>
        /// Writes the whole network into {directory}.building, then swaps it with the directory ({directory}.old is
        /// deleted, or on the next build when a reader still holds it), so readers never see a half written network.
        /// </summary>
        public void Commit(NetworkMetadata metadata)
        {
            var target = Path.GetFullPath(_directory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var building = target + ".building";
            var old = target + ".old";
            TryDelete(old);
            if (Directory.Exists(building))
            {
                Directory.Delete(building, true);
            }
            var shardDir = Path.Combine(building, ShardDirectory);
            Directory.CreateDirectory(shardDir);

            var keysById = metadata.ShardKeys.ToDictionary(k => k.Value, k => k.Key);
            metadata.Shards.Clear();
            foreach (var shard in _shards.Values.OrderBy(s => s.Id))
            {
                var fileName = ShardFileName(shard.Id);
                using (var stream = File.Create(Path.Combine(shardDir, fileName)))
                {
                    ShardBinary.Write(stream, shard);
                }
                if (_writeJson)
                {
                    File.WriteAllText(Path.Combine(shardDir, $"{shard.Id}.json"), ShardJson.Serialize(shard));
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

            NodeIndex.Write(
                Path.Combine(building, NodeIndex.FileName),
                _shards.Values.SelectMany(s => s.Nodes.Select(n => (n.Node.Id, s.Id, n.Node.TypeId))));

            EdgeIndex.Write(
                Path.Combine(building, EdgeIndex.FileName),
                _shards.Values.SelectMany(s => s.Edges.Where(e => e.FromShard == s.Id).Select(e => (e.Edge.Id, s.Id))));

            AliasIndex.Write(Path.Combine(building, AliasIndex.FileName), _aliases);
            SpatialIndex.Write(Path.Combine(building, SpatialIndex.FileName), _points, metadata.Tolerance);
            metadata.AliasCount = _aliases.Count;

            File.WriteAllText(Path.Combine(building, MetadataFile), JsonSerializer.Serialize(metadata, NetworkConfig.JsonOptions));

            if (Directory.Exists(target))
            {
                Directory.Move(target, old);
            }
            Directory.Move(building, target);
            TryDelete(old);
        }

        private static void TryDelete(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        public void Dispose()
        {
            _shards.Clear();
            _aliases.Clear();
            _points.Clear();
        }
    }
}
