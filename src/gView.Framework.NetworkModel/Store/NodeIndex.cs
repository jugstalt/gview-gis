using System.Text;

namespace gView.Framework.NetworkModel.Store;

/// <summary>
/// nodes.idx: every node id with its shard and type, sorted by id, so a graph can find the shard of a node without
/// loading all shards, and start sets by type pattern can be taken without loading any.
/// "SNIX" | version int32 | count int32 | ids int64[] | shards int64[] | types int32[].
/// </summary>
public sealed class NodeIndex
{
    public const string FileName = "nodes.idx";
    private const int Version = 1;
    private static readonly byte[] Magic = "SNIX"u8.ToArray();

    private readonly long[] _ids;
    private readonly long[] _shards;
    private readonly int[] _types;

    private NodeIndex(long[] ids, long[] shards, int[] types)
    {
        _ids = ids;
        _shards = shards;
        _types = types;
    }

    public int Count => _ids.Length;

    public bool TryGet(long nodeId, out long shard, out int typeId)
    {
        int i = Array.BinarySearch(_ids, nodeId);
        if (i < 0)
        {
            shard = 0;
            typeId = 0;
            return false;
        }
        shard = _shards[i];
        typeId = _types[i];
        return true;
    }

    /// <summary>The ids of all nodes whose type is accepted, ascending.</summary>
    public IEnumerable<long> NodesOfTypes(Func<int, bool> acceptType)
    {
        for (int i = 0; i < _ids.Length; i++)
        {
            if (acceptType(_types[i]))
            {
                yield return _ids[i];
            }
        }
    }

    /// <summary>The ids of all nodes in these shards, ascending.</summary>
    public IEnumerable<long> NodesOfShards(IReadOnlySet<long> shards)
    {
        for (int i = 0; i < _ids.Length; i++)
        {
            if (shards.Contains(_shards[i]))
            {
                yield return _ids[i];
            }
        }
    }

    public static void Write(string path, IEnumerable<(long Id, long Shard, int TypeId)> nodes)
    {
        var sorted = nodes.OrderBy(n => n.Id).ToArray();
        using var w = new BinaryWriter(File.Create(path), Encoding.UTF8);
        w.Write(Magic);
        w.Write(Version);
        w.Write(sorted.Length);
        foreach (var n in sorted) w.Write(n.Id);
        foreach (var n in sorted) w.Write(n.Shard);
        foreach (var n in sorted) w.Write(n.TypeId);
    }

    public static NodeIndex Read(string path)
    {
        using var r = new BinaryReader(File.OpenRead(path), Encoding.UTF8);
        if (!r.ReadBytes(4).SequenceEqual(Magic))
        {
            throw new InvalidDataException("Not a network node index.");
        }
        int version = r.ReadInt32();
        if (version != Version)
        {
            throw new InvalidDataException($"Unsupported node index version {version}.");
        }
        int count = r.ReadInt32();
        var ids = new long[count];
        var shards = new long[count];
        var types = new int[count];
        for (int i = 0; i < count; i++) ids[i] = r.ReadInt64();
        for (int i = 0; i < count; i++) shards[i] = r.ReadInt64();
        for (int i = 0; i < count; i++) types[i] = r.ReadInt32();
        return new NodeIndex(ids, shards, types);
    }
}
