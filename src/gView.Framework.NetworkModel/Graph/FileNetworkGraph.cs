using gView.Framework.NetworkModel.Ids;
using gView.Framework.NetworkModel.Store;
using SpatialNET.Network;
using System.Collections.Concurrent;
using System.Globalization;

namespace gView.Framework.NetworkModel.Graph;

/// <summary>
/// The graph of a network kept by <see cref="FileNetworkStore"/>: finds the shard of a node in nodes.idx and loads a
/// shard only when a search touches it, then keeps it (at most <see cref="MaxLoadedShards"/>, least recently used dropped first).
/// Thread-safe: loaded shards are read-only and shared, searches keep their state on their own.
/// </summary>
public sealed class FileNetworkGraph : IGraph<long, long>
{
    private sealed class LoadedShard
    {
        public Dictionary<long, ShardNode> Nodes { get; } = [];
        public Dictionary<long, List<ShardEdge>> EdgesByNode { get; } = [];
        public Dictionary<long, ShardEdge> EdgesById { get; } = [];
        public long LastUsed;
    }

    private readonly FileNetworkStore _store;
    private readonly NodeIndex _index;
    private readonly AliasIndex _aliases;
    private readonly ConcurrentDictionary<long, Lazy<LoadedShard>> _shards = new();
    private long _clock;

    public FileNetworkGraph(string directory)
    {
        _store = new FileNetworkStore(directory);
        Metadata = _store.ReadMetadata() ?? throw new FileNotFoundException($"No network in {directory} ({FileNetworkStore.MetadataFile} missing).");
        var indexPath = Path.Combine(directory, NodeIndex.FileName);
        if (!File.Exists(indexPath))
        {
            throw new FileNotFoundException($"{indexPath} is missing; build the network again.");
        }
        _index = NodeIndex.Read(indexPath);
        _aliases = AliasIndex.Read(Path.Combine(directory, AliasIndex.FileName));
        Types = new TypeRegistry(
            Metadata.Facets.Select(f => new Facet(f.Name, f.Values)).ToList(),
            Metadata.Types.Select(t => new NetworkType(t.Id, t.Kind, t.Facets, t.Name)));
    }

    public NetworkMetadata Metadata { get; }

    public TypeRegistry Types { get; }

    /// <summary>The number of shards loaded so far.</summary>
    public int LoadedShardCount => _shards.Count;

    /// <summary>How many shards are kept at most; 0 for no limit.</summary>
    public int MaxLoadedShards { get; set; } = 512;

    #region IGraph

    public IEnumerable<GraphNode<long>> GetNodes(IReadOnlyCollection<long> ids)
    {
        foreach (var id in ids)
        {
            if (TryGetNode(id, out var node))
            {
                yield return node.Node;
            }
        }
    }

    public IEnumerable<GraphEdge<long, long>> GetEdges(IReadOnlyCollection<long> nodeIds)
    {
        var seen = new HashSet<long>();
        foreach (var nodeId in nodeIds)
        {
            if (!_index.TryGet(nodeId, out var shardId, out _))
            {
                continue;
            }
            if (Shard(shardId).EdgesByNode.TryGetValue(nodeId, out var edges))
            {
                foreach (var edge in edges)
                {
                    if (seen.Add(edge.Edge.Id))
                    {
                        yield return edge.Edge;
                    }
                }
            }
        }
    }

    #endregion

    #region Lookups

    public bool TryGetNode(long id, out ShardNode node)
    {
        node = default;
        return _index.TryGet(id, out var shardId, out _) && Shard(shardId).Nodes.TryGetValue(id, out node);
    }

    /// <summary>The ids of the points merged into a node (aliases), e.g. customers drawn on a box; empty for none.</summary>
    public IReadOnlyList<long> AliasesOf(long nodeId) => _aliases.AliasesOf(nodeId);

    /// <summary>The type of a merged point (alias).</summary>
    public int AliasTypeOf(long aliasId) => _aliases.TypeOf(aliasId);

    /// <summary>The node a merged point (alias) was merged into.</summary>
    public bool TryGetAliasTarget(long aliasId, out long nodeId) => _aliases.TryGetNode(aliasId, out nodeId);

    public bool TryGetShardOf(long nodeId, out long shardId) => _index.TryGet(nodeId, out shardId, out _);

    /// <summary>An edge that a search returned, found in the loaded shards.</summary>
    public bool TryGetEdge(long id, out ShardEdge edge)
    {
        foreach (var shard in _shards.Values)
        {
            if (shard.IsValueCreated && shard.Value.EdgesById.TryGetValue(id, out edge))
            {
                return true;
            }
        }
        edge = default;
        return false;
    }

    /// <summary>An edge at one of its nodes (loads the shard of the node when needed).</summary>
    public bool TryGetEdge(long id, long nodeId, out ShardEdge edge)
    {
        foreach (var e in EdgesOf(nodeId))
        {
            if (e.Edge.Id == id)
            {
                edge = e;
                return true;
            }
        }
        return TryGetEdge(id, out edge);
    }

    public IReadOnlyList<ShardEdge> EdgesOf(long nodeId)
        => _index.TryGet(nodeId, out var shardId, out _) && Shard(shardId).EdgesByNode.TryGetValue(nodeId, out var edges)
            ? edges
            : [];

    /// <summary>Reads every shard from the store, one after the other, without keeping them (for checks over the whole network).</summary>
    public IEnumerable<ShardData> ReadShards()
    {
        foreach (var shard in Metadata.Shards)
        {
            yield return _store.ReadShard(shard.Id);
        }
    }

    /// <summary>Drops the loaded shards; they are loaded again when a search touches them.</summary>
    public void ClearCache()
    {
        _shards.Clear();
    }

    #endregion

    #region References

    /// <summary>
    /// Resolves a comma separated list of node references: Layer#OID (e.g. Trafo#5929, Niederspannung/Kasten#23660),
    /// a raw node id, or a type pattern (e.g. [*,Trafo,*], Niederspannung.Kasten), which means every node of that type.
    /// A merged point (alias) resolves to the node it was merged into.
    /// </summary>
    public IReadOnlyList<long> ResolveNodes(string references)
    {
        var result = new List<long>();
        foreach (var reference in SplitReferences(references))
        {
            result.AddRange(ResolveNodeReference(reference));
        }
        return result.Distinct().ToList();
    }

    public long ResolveNode(string reference)
    {
        var ids = ResolveNodes(reference);
        return ids.Count == 1 ? ids[0] : throw new ArgumentException($"'{reference}' means {ids.Count} nodes, one is expected.");
    }

    private IEnumerable<long> ResolveNodeReference(string reference)
    {
        int hash = reference.LastIndexOf('#');
        if (hash > 0 && long.TryParse(reference[(hash + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var oid))
        {
            var layer = reference[..hash].Trim();
            var featureClasses = Metadata.FeatureClasses
                .Where(f => f.Kind == ElementKind.Node
                         && (f.Layer.Equals(layer, StringComparison.OrdinalIgnoreCase)
                          || f.Layer.Split('/')[^1].Equals(layer, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            if (featureClasses.Count != 1)
            {
                throw new ArgumentException(featureClasses.Count == 0
                    ? $"No node layer '{layer}' in the network. Node layers: {string.Join(", ", Metadata.FeatureClasses.Where(f => f.Kind == ElementKind.Node).Select(f => f.Layer))}"
                    : $"Layer name '{layer}' is ambiguous, use the path: {string.Join(", ", featureClasses.Select(f => f.Layer))}");
            }
            var id = NetworkIds.Compose(featureClasses[0].Index, oid);
            if (_aliases.TryGetNode(id, out var target))
            {
                return [target];
            }
            if (!_index.TryGet(id, out _, out _))
            {
                throw new ArgumentException($"{reference} is not a node of the network (left out by a filter or as a duplicate?).");
            }
            return [id];
        }

        if (long.TryParse(reference, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rawId))
        {
            if (_aliases.TryGetNode(rawId, out var target))
            {
                return [target];
            }
            return _index.TryGet(rawId, out _, out _) ? [rawId] : throw new ArgumentException($"Node id {rawId} is not in the network.");
        }

        var types = Types.Nodes(reference);
        // merged points of the type (customers drawn on a box) stand for the node they were merged into
        return _index.NodesOfTypes(types.Contains).Concat(_aliases.NodesOfAliasTypes(types.Contains)).Distinct().ToList();
    }

    /// <summary>Splits at commas outside of [ ] (patterns contain commas).</summary>
    private static IEnumerable<string> SplitReferences(string references)
    {
        int depth = 0, start = 0;
        for (int i = 0; i <= references.Length; i++)
        {
            if (i == references.Length || (references[i] == ',' && depth == 0))
            {
                var part = references[start..i].Trim();
                if (part.Length > 0)
                {
                    yield return part;
                }
                start = i + 1;
            }
            else if (references[i] == '[') depth++;
            else if (references[i] == ']') depth--;
        }
    }

    /// <summary>A readable name: Layer#OID for real nodes, "helper" for helper nodes.</summary>
    public string NodeLabel(long id)
    {
        var (fc, oid, _) = NetworkIds.Decompose(id);
        if (fc == NetworkIds.HelperFeatureClass)
        {
            return "helper";
        }
        return $"{Metadata.FeatureClasses.FirstOrDefault(f => f.Index == fc)?.Layer ?? fc.ToString()}#{oid}";
    }

    /// <summary>A readable name: Layer#OID/part.</summary>
    public string EdgeLabel(long id)
    {
        var (fc, oid, part) = NetworkIds.Decompose(id);
        return $"{Metadata.FeatureClasses.FirstOrDefault(f => f.Index == fc)?.Layer ?? fc.ToString()}#{oid}/{part}";
    }

    public string TypeName(int typeId) => typeId == 0 ? "untyped" : Types.Display(typeId);

    #endregion

    private LoadedShard Shard(long shardId)
    {
        var lazy = _shards.GetOrAdd(shardId, id => new Lazy<LoadedShard>(() => Load(id), LazyThreadSafetyMode.ExecutionAndPublication));
        LoadedShard loaded;
        try
        {
            loaded = lazy.Value;
        }
        catch
        {
            _shards.TryRemove(new KeyValuePair<long, Lazy<LoadedShard>>(shardId, lazy));
            throw;
        }
        Interlocked.Exchange(ref loaded.LastUsed, Interlocked.Increment(ref _clock));
        if (MaxLoadedShards > 0 && _shards.Count > MaxLoadedShards)
        {
            Evict(shardId);
        }
        return loaded;
    }

    /// <summary>Drops the least recently used shards down to <see cref="MaxLoadedShards"/>; searches holding them keep their copy.</summary>
    private void Evict(long keep)
    {
        var victims = _shards
            .Where(s => s.Key != keep && s.Value.IsValueCreated)
            .OrderBy(s => Interlocked.Read(ref s.Value.Value.LastUsed))
            .Take(Math.Max(0, _shards.Count - MaxLoadedShards))
            .ToList();
        foreach (var victim in victims)
        {
            _shards.TryRemove(victim);
        }
    }

    private LoadedShard Load(long shardId)
    {
        var loaded = new LoadedShard();
        var data = _store.ReadShard(shardId);
        foreach (var node in data.Nodes)
        {
            loaded.Nodes[node.Node.Id] = node;
        }
        foreach (var edge in data.Edges)
        {
            loaded.EdgesById[edge.Edge.Id] = edge;
            Add(loaded, edge.Edge.From, edge);
            if (edge.Edge.To != edge.Edge.From)
            {
                Add(loaded, edge.Edge.To, edge);
            }
        }
        return loaded;

        static void Add(LoadedShard shard, long nodeId, ShardEdge edge)
        {
            if (!shard.EdgesByNode.TryGetValue(nodeId, out var list))
            {
                shard.EdgesByNode[nodeId] = list = [];
            }
            list.Add(edge);
        }
    }
}
