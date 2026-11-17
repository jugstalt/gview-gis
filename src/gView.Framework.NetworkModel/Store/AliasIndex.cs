using System.Text;

namespace gView.Framework.NetworkModel.Store;

/// <summary>
/// aliases.idx: the ids of real nodes merged into another node (points of different layers within the tolerance,
/// SpatialNET ADR 0054) and the id of the node they were merged into, sorted by alias id, so Layer#OID of a merged
/// point still finds its node and a result can list the merged points of a node.
/// The type of the merged point is kept too, so a type pattern (all customers) also finds the merged ones.
/// "SNAL" | version int32 | count int32 | alias ids int64[] | node ids int64[] | alias types int32[].
/// </summary>
public sealed class AliasIndex
{
    public const string FileName = "aliases.idx";
    private const int Version = 1;
    private static readonly byte[] Magic = "SNAL"u8.ToArray();

    private readonly long[] _aliases;
    private readonly long[] _nodes;
    private readonly int[] _types;
    private Dictionary<long, List<long>>? _aliasesOf;

    private AliasIndex(long[] aliases, long[] nodes, int[] types)
    {
        _aliases = aliases;
        _nodes = nodes;
        _types = types;
    }

    public static AliasIndex Empty { get; } = new([], [], []);

    public int Count => _aliases.Length;

    /// <summary>The node an alias id was merged into.</summary>
    public bool TryGetNode(long aliasId, out long nodeId)
    {
        int i = Array.BinarySearch(_aliases, aliasId);
        nodeId = i >= 0 ? _nodes[i] : 0;
        return i >= 0;
    }

    /// <summary>The type of a merged point.</summary>
    public int TypeOf(long aliasId)
    {
        int i = Array.BinarySearch(_aliases, aliasId);
        return i >= 0 ? _types[i] : 0;
    }

    /// <summary>The nodes that merged points of an accepted type were merged into (with repeats).</summary>
    public IEnumerable<long> NodesOfAliasTypes(Func<int, bool> acceptType)
    {
        for (int i = 0; i < _aliases.Length; i++)
        {
            if (acceptType(_types[i]))
            {
                yield return _nodes[i];
            }
        }
    }

    /// <summary>The alias ids merged into a node, ascending; empty for none.</summary>
    public IReadOnlyList<long> AliasesOf(long nodeId)
    {
        if (_aliasesOf is null)
        {
            _aliasesOf = [];
            for (int i = 0; i < _aliases.Length; i++)
            {
                if (!_aliasesOf.TryGetValue(_nodes[i], out var list))
                {
                    _aliasesOf[_nodes[i]] = list = [];
                }
                list.Add(_aliases[i]);
            }
        }
        return _aliasesOf.TryGetValue(nodeId, out var aliases) ? aliases : [];
    }

    public static void Write(string path, IEnumerable<(long AliasId, long NodeId, int TypeId)> aliases)
    {
        var sorted = aliases.OrderBy(a => a.AliasId).ToArray();
        using var w = new BinaryWriter(File.Create(path), Encoding.UTF8);
        w.Write(Magic);
        w.Write(Version);
        w.Write(sorted.Length);
        foreach (var a in sorted) w.Write(a.AliasId);
        foreach (var a in sorted) w.Write(a.NodeId);
        foreach (var a in sorted) w.Write(a.TypeId);
    }

    /// <summary>Reads the index; an empty one if the file is missing (a network built before aliases existed).</summary>
    public static AliasIndex Read(string path)
    {
        if (!File.Exists(path))
        {
            return Empty;
        }
        using var r = new BinaryReader(File.OpenRead(path), Encoding.UTF8);
        if (!r.ReadBytes(4).SequenceEqual(Magic))
        {
            throw new InvalidDataException("Not a network alias index.");
        }
        int version = r.ReadInt32();
        if (version != Version)
        {
            throw new InvalidDataException($"Unsupported alias index version {version}.");
        }
        int count = r.ReadInt32();
        var aliases = new long[count];
        var nodes = new long[count];
        for (int i = 0; i < count; i++) aliases[i] = r.ReadInt64();
        var types = new int[count];
        for (int i = 0; i < count; i++) nodes[i] = r.ReadInt64();
        for (int i = 0; i < count; i++) types[i] = r.ReadInt32();
        return new AliasIndex(aliases, nodes, types);
    }
}
