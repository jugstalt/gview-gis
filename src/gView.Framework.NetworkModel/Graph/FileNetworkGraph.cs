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
    private readonly string _directory;
    private readonly NodeIndex _index;
    private readonly AliasIndex _aliases;
    private readonly Lazy<SpatialIndex> _spatial;
    private readonly Lazy<EdgeIndex> _edgeIndex;
    private readonly ConcurrentDictionary<long, Lazy<LoadedShard>> _shards = new();
    private long _clock;

    public FileNetworkGraph(string directory)
    {
        _directory = directory;
        _store = new FileNetworkStore(directory);
        Metadata = _store.ReadMetadata() ?? throw new FileNotFoundException($"No network in {directory} ({FileNetworkStore.MetadataFile} missing).");
        var indexPath = Path.Combine(directory, NodeIndex.FileName);
        if (!File.Exists(indexPath))
        {
            throw new FileNotFoundException($"{indexPath} is missing; build the network again.");
        }
        _index = NodeIndex.Read(indexPath);
        _aliases = AliasIndex.Read(Path.Combine(directory, AliasIndex.FileName));
        _spatial = new Lazy<SpatialIndex>(() => SpatialIndex.Read(RequiredIndex(SpatialIndex.FileName)), LazyThreadSafetyMode.ExecutionAndPublication);
        _edgeIndex = new Lazy<EdgeIndex>(() => EdgeIndex.Read(RequiredIndex(EdgeIndex.FileName)), LazyThreadSafetyMode.ExecutionAndPublication);
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

    /// <summary>
    /// The nodes nearest to x/y (in the spatial reference of the network) within the radius, nearest first: real nodes,
    /// helper nodes (edge ends) and merged points, which stand for their node. Default radius: <see cref="DefaultSearchRadius"/>.
    /// </summary>
    public IReadOnlyList<SpatialHit> Nearest(double x, double y, double? radius = null, int count = 1)
        => _spatial.Value.Nearest(x, y, radius ?? DefaultSearchRadius, count);

    /// <summary>The radius of @x,y and nearest without one: 10 x the build tolerance.</summary>
    public double DefaultSearchRadius => Metadata.Tolerance > 0 ? Metadata.Tolerance * 10 : 1;

    /// <summary>The ids of all parts of a line feature (empty when the feature is no edge of the network).</summary>
    public IReadOnlyList<long> EdgePartsOf(int featureClass, long oid) => _edgeIndex.Value.PartsOf(featureClass, oid);

    /// <summary>Any edge by id (loads the shard of its from node when needed).</summary>
    public bool TryFindEdge(long id, out ShardEdge edge)
    {
        if (_edgeIndex.Value.TryGet(id, out var shardId) && Shard(shardId).EdgesById.TryGetValue(id, out edge))
        {
            return true;
        }
        edge = default;
        return false;
    }

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

    /// <summary>Reads these shards from the store, one after the other, without keeping them.</summary>
    public IEnumerable<ShardData> ReadShards(IEnumerable<long> shardIds)
    {
        foreach (var id in shardIds)
        {
            yield return _store.ReadShard(id);
        }
    }

    /// <summary>
    /// The nodes (real and helper nodes, not merged points) inside the area: by location for a box or polygon
    /// (on the border within the build tolerance counts as inside), by shard for shard keys.
    /// </summary>
    public HashSet<long> NodesIn(Query.NetworkArea area)
    {
        if (area.Kind == Query.NetworkAreaKind.Shards)
        {
            var shards = new HashSet<long>();
            foreach (var key in area.ShardKeys)
            {
                var matches = Metadata.Shards.Where(s => key.Equals(s.Key, StringComparison.OrdinalIgnoreCase)).ToList();
                if (matches.Count == 0)
                {
                    throw new ArgumentException($"area: no shard '{key}' in the network.");
                }
                shards.UnionWith(matches.Select(s => s.Id));
            }
            return _index.NodesOfShards(shards).ToHashSet();
        }

        double tolerance = Math.Max(Metadata.Tolerance, 0);
        return _spatial.Value
            .Within(area.MinX - tolerance, area.MinY - tolerance, area.MaxX + tolerance, area.MaxY + tolerance)
            .Where(e => e.Id == e.Node && area.Contains(e.X, e.Y, tolerance))
            .Select(e => e.Node)
            .ToHashSet();
    }

    /// <summary>Whether a node reference is a type pattern (it may mean many nodes) and not one node (Layer#OID, id, @x,y).</summary>
    public static bool IsTypePattern(string reference)
    {
        reference = reference.Trim();
        if (reference.StartsWith('@'))
        {
            return false;
        }
        int hash = reference.LastIndexOf('#');
        if (hash > 0 && long.TryParse(reference[(hash + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
        {
            return false;
        }
        return !long.TryParse(reference, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
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
    /// a raw node id, a type pattern (e.g. [*,Trafo,*], Niederspannung.Kasten, ![*,Kunde,*]), which means every node of
    /// that type, or a location @x,y[~radius]: the nearest node, also a helper node at edge ends (in the spatial reference
    /// of the network, default radius <see cref="DefaultSearchRadius"/>). A merged point (alias) resolves to the node it
    /// was merged into.
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
        if (reference.StartsWith('@'))
        {
            var (x, y, radius) = ParseLocation(reference);
            var hits = Nearest(x, y, radius);
            if (hits.Count == 0)
            {
                throw new ArgumentException($"No network node within {(radius ?? DefaultSearchRadius).ToString("0.###", CultureInfo.InvariantCulture)} of {reference}.");
            }
            return [hits[0].Entry.Node];
        }

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

    /// <summary>
    /// Resolves a comma separated list of edge references: Layer#OID (every part of the line), Layer#OID/part or a raw
    /// edge id.
    /// </summary>
    public IReadOnlyList<long> ResolveEdges(string references)
    {
        var result = new List<long>();
        foreach (var reference in SplitReferences(references))
        {
            result.AddRange(ResolveEdgeReference(reference));
        }
        return result.Distinct().ToList();
    }

    private IReadOnlyList<long> ResolveEdgeReference(string reference)
    {
        int hash = reference.LastIndexOf('#');
        if (hash > 0)
        {
            var layer = reference[..hash].Trim();
            var rest = reference[(hash + 1)..];
            int slash = rest.IndexOf('/');
            if (!long.TryParse(slash < 0 ? rest : rest[..slash], NumberStyles.Integer, CultureInfo.InvariantCulture, out var oid))
            {
                throw new ArgumentException($"'{reference}' is no edge reference (Layer#OID or Layer#OID/part).");
            }
            var featureClasses = Metadata.FeatureClasses
                .Where(f => f.Kind == ElementKind.Edge
                         && (f.Layer.Equals(layer, StringComparison.OrdinalIgnoreCase)
                          || f.Layer.Split('/')[^1].Equals(layer, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            if (featureClasses.Count != 1)
            {
                throw new ArgumentException(featureClasses.Count == 0
                    ? $"No edge layer '{layer}' in the network. Edge layers: {string.Join(", ", Metadata.FeatureClasses.Where(f => f.Kind == ElementKind.Edge).Select(f => f.Layer))}"
                    : $"Layer name '{layer}' is ambiguous, use the path: {string.Join(", ", featureClasses.Select(f => f.Layer))}");
            }
            if (slash < 0)
            {
                var parts = EdgePartsOf(featureClasses[0].Index, oid);
                return parts.Count > 0 ? parts : throw new ArgumentException($"{reference} is not an edge of the network (left out by a filter?).");
            }
            if (!int.TryParse(rest[(slash + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var part) || part < 0 || part >= NetworkIds.MaxParts)
            {
                throw new ArgumentException($"'{reference}': invalid part.");
            }
            var id = NetworkIds.Compose(featureClasses[0].Index, oid, part);
            return _edgeIndex.Value.TryGet(id, out _) ? [id] : throw new ArgumentException($"{reference} is not an edge of the network.");
        }

        if (long.TryParse(reference, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rawId))
        {
            return _edgeIndex.Value.TryGet(rawId, out _) ? [rawId] : throw new ArgumentException($"Edge id {rawId} is not in the network.");
        }
        throw new ArgumentException($"'{reference}' is no edge reference (Layer#OID, Layer#OID/part or an edge id).");
    }

    /// <summary>@x,y or @x,y~radius.</summary>
    private static (double X, double Y, double? Radius) ParseLocation(string reference)
    {
        var text = reference[1..];
        double? radius = null;
        int tilde = text.IndexOf('~');
        if (tilde >= 0)
        {
            if (!double.TryParse(text[(tilde + 1)..], NumberStyles.Float, CultureInfo.InvariantCulture, out var r) || r < 0)
            {
                throw new ArgumentException($"'{reference}': invalid radius.");
            }
            radius = r;
            text = text[..tilde];
        }
        var parts = text.Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length != 2
            || !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
        {
            throw new ArgumentException($"'{reference}' is no location; use @x,y or @x,y~radius.");
        }
        return (x, y, radius);
    }

    /// <summary>Splits at commas outside of [ ] (patterns contain commas); the first comma after @ belongs to the location.</summary>
    public static IEnumerable<string> SplitReferences(string references)
    {
        int depth = 0, start = 0;
        bool locationComma = false;
        for (int i = 0; i <= references.Length; i++)
        {
            if (i < references.Length && references[i] == ',' && depth == 0 && !locationComma && references[start..i].TrimStart().StartsWith('@'))
            {
                locationComma = true;
                continue;
            }
            if (i == references.Length || (references[i] == ',' && depth == 0))
            {
                locationComma = false;
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

    /// <summary>
    /// A readable name: Layer#OID for real nodes; for helper nodes (edge ends) the lines that meet there,
    /// e.g. "end of Verbinder#12, Leitung#3456".
    /// </summary>
    public string NodeLabel(long id)
    {
        var (fc, oid, _) = NetworkIds.Decompose(id);
        if (fc == NetworkIds.HelperFeatureClass)
        {
            var lines = EdgesOf(id).Select(e => FeatureLabel(e.Source)).Distinct().ToList();
            return lines.Count == 0
                ? "helper"
                : $"end of {string.Join(", ", lines.Take(4))}{(lines.Count > 4 ? $" (+{lines.Count - 4})" : "")}";
        }
        return $"{Metadata.FeatureClasses.FirstOrDefault(f => f.Index == fc)?.Layer ?? fc.ToString()}#{oid}";
    }

    private string FeatureLabel(FeatureRef source)
        => $"{Metadata.FeatureClasses.FirstOrDefault(f => f.Index == source.FeatureClass)?.Layer.Split('/')[^1] ?? source.FeatureClass.ToString()}#{source.Oid}";

    /// <summary>A readable name: Layer#OID/part.</summary>
    public string EdgeLabel(long id)
    {
        var (fc, oid, part) = NetworkIds.Decompose(id);
        return $"{Metadata.FeatureClasses.FirstOrDefault(f => f.Index == fc)?.Layer ?? fc.ToString()}#{oid}/{part}";
    }

    public string TypeName(int typeId) => typeId == 0 ? "untyped" : Types.Display(typeId);

    #endregion

    private string RequiredIndex(string fileName)
    {
        var path = Path.Combine(_directory, fileName);
        return File.Exists(path)
            ? path
            : throw new FileNotFoundException($"The network has no {fileName} (built before edge ends could be picked); build the network again.");
    }

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
