using gView.Framework.NetworkModel.Ids;
using System.Text;

namespace gView.Framework.NetworkModel.Store;

/// <summary>
/// edges.idx: every edge id with the shard of its from node, sorted by id. The parts of one line feature have
/// neighbouring ids (| feature class | oid | part |), so Layer#OID finds all of them as one range.
/// "SNEX" | version int32 | count int32 | ids int64[] | shards int64[].
/// </summary>
public sealed class EdgeIndex
{
    public const string FileName = "edges.idx";
    private const int Version = 1;
    private static readonly byte[] Magic = "SNEX"u8.ToArray();

    private readonly long[] _ids;
    private readonly long[] _shards;

    private EdgeIndex(long[] ids, long[] shards)
    {
        _ids = ids;
        _shards = shards;
    }

    public int Count => _ids.Length;

    public bool TryGet(long edgeId, out long shard)
    {
        int i = Array.BinarySearch(_ids, edgeId);
        shard = i >= 0 ? _shards[i] : 0;
        return i >= 0;
    }

    /// <summary>The ids of all parts of a line feature, ascending.</summary>
    public IReadOnlyList<long> PartsOf(int featureClass, long oid)
    {
        long first = NetworkIds.Compose(featureClass, oid, 0);
        long last = NetworkIds.Compose(featureClass, oid, NetworkIds.MaxParts - 1);
        int i = Array.BinarySearch(_ids, first);
        if (i < 0)
        {
            i = ~i;
        }
        var parts = new List<long>();
        for (; i < _ids.Length && _ids[i] <= last; i++)
        {
            parts.Add(_ids[i]);
        }
        return parts;
    }

    public static void Write(string path, IEnumerable<(long Id, long Shard)> edges)
    {
        var sorted = edges.DistinctBy(e => e.Id).OrderBy(e => e.Id).ToArray();
        using var w = new BinaryWriter(File.Create(path), Encoding.UTF8);
        w.Write(Magic);
        w.Write(Version);
        w.Write(sorted.Length);
        foreach (var e in sorted) w.Write(e.Id);
        foreach (var e in sorted) w.Write(e.Shard);
    }

    public static EdgeIndex Read(string path)
    {
        using var r = new BinaryReader(new MemoryStream(File.ReadAllBytes(path)), Encoding.UTF8);
        if (!r.ReadBytes(4).SequenceEqual(Magic))
        {
            throw new InvalidDataException("Not a network edge index.");
        }
        int version = r.ReadInt32();
        if (version != Version)
        {
            throw new InvalidDataException($"Unsupported edge index version {version}.");
        }
        int count = r.ReadInt32();
        var ids = new long[count];
        var shards = new long[count];
        for (int i = 0; i < count; i++) ids[i] = r.ReadInt64();
        for (int i = 0; i < count; i++) shards[i] = r.ReadInt64();
        return new EdgeIndex(ids, shards);
    }
}
