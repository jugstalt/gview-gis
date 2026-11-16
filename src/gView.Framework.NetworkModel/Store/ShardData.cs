using gView.Framework.NetworkModel.Ids;
using SpatialNET;
using SpatialNET.Network;
using System.Text;
using System.Text.Json;

namespace gView.Framework.NetworkModel.Store;

/// <summary>The nodes and edges of one shard as stored.</summary>
public sealed class ShardData(long id)
{
    public long Id { get; } = id;
    public List<ShardNode> Nodes { get; } = [];
    public List<ShardEdge> Edges { get; } = [];
}

/// <param name="Node">The node.</param>
/// <param name="IsHelper">A helper node where line ends meet without a point.</param>
/// <param name="Source">The feature of a real node.</param>
public readonly record struct ShardNode(GraphNode<long> Node, bool IsHelper, FeatureRef? Source);

/// <param name="Edge">The edge.</param>
/// <param name="Source">The line feature.</param>
/// <param name="FromMeasure">Where the edge starts on the line (geometric length from its start).</param>
/// <param name="ToMeasure">Where the edge ends on the line.</param>
/// <param name="FromShard">The shard of the from node.</param>
/// <param name="ToShard">The shard of the to node.</param>
public readonly record struct ShardEdge(GraphEdge<long, long> Edge, FeatureRef Source, double FromMeasure, double ToMeasure, long FromShard, long ToShard);

/// <summary>
/// The binary shard format, little endian:
/// "SNSH" | version int32 | shard id int64 | node count int32 | nodes | edge count int32 | edges.
/// Node: id int64 | type int32 | x, y double | flags byte (1 blocked, 2 helper) | source fc int32 | source oid int64.
/// Edge: id, from, to int64 | type int32 | length double | direction byte | disabled byte | weight count int32 |
/// (weight id int32, value double)* | source fc int32 | source oid int64 | from/to measure double | from/to shard int64.
/// </summary>
public static class ShardBinary
{
    private const int Version = 1;
    private static readonly byte[] Magic = "SNSH"u8.ToArray();

    public static void Write(Stream stream, ShardData shard)
    {
        using var w = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        w.Write(Magic);
        w.Write(Version);
        w.Write(shard.Id);

        w.Write(shard.Nodes.Count);
        foreach (var n in shard.Nodes)
        {
            w.Write(n.Node.Id);
            w.Write(n.Node.TypeId);
            w.Write(n.Node.Location.X);
            w.Write(n.Node.Location.Y);
            w.Write((byte)((n.Node.Blocked ? 1 : 0) | (n.IsHelper ? 2 : 0)));
            w.Write(n.Source?.FeatureClass ?? -1);
            w.Write(n.Source?.Oid ?? -1);
        }

        w.Write(shard.Edges.Count);
        foreach (var e in shard.Edges)
        {
            w.Write(e.Edge.Id);
            w.Write(e.Edge.From);
            w.Write(e.Edge.To);
            w.Write(e.Edge.TypeId);
            w.Write(e.Edge.Length);
            w.Write((byte)e.Edge.Direction);
            w.Write((byte)(e.Edge.Disabled ? 1 : 0));
            var weights = e.Edge.Weights.Span;
            w.Write(weights.Length);
            foreach (var weight in weights)
            {
                w.Write(weight.WeightId);
                w.Write(weight.Value);
            }
            w.Write(e.Source.FeatureClass);
            w.Write(e.Source.Oid);
            w.Write(e.FromMeasure);
            w.Write(e.ToMeasure);
            w.Write(e.FromShard);
            w.Write(e.ToShard);
        }
    }

    public static ShardData Read(Stream stream)
    {
        using var r = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        if (!r.ReadBytes(4).SequenceEqual(Magic))
        {
            throw new InvalidDataException("Not a network shard file.");
        }
        int version = r.ReadInt32();
        if (version != Version)
        {
            throw new InvalidDataException($"Unsupported shard format version {version}.");
        }

        var shard = new ShardData(r.ReadInt64());

        int nodeCount = r.ReadInt32();
        for (int i = 0; i < nodeCount; i++)
        {
            long id = r.ReadInt64();
            int type = r.ReadInt32();
            double x = r.ReadDouble(), y = r.ReadDouble();
            byte flags = r.ReadByte();
            int fc = r.ReadInt32();
            long oid = r.ReadInt64();
            shard.Nodes.Add(new ShardNode(
                new GraphNode<long>(id, type, new Coordinate(x, y), (flags & 1) != 0),
                (flags & 2) != 0,
                fc >= 0 ? new FeatureRef(fc, oid) : null));
        }

        int edgeCount = r.ReadInt32();
        for (int i = 0; i < edgeCount; i++)
        {
            long id = r.ReadInt64(), from = r.ReadInt64(), to = r.ReadInt64();
            int type = r.ReadInt32();
            double length = r.ReadDouble();
            var direction = (EdgeDirection)r.ReadByte();
            bool disabled = r.ReadByte() != 0;
            var weights = new EdgeWeight[r.ReadInt32()];
            for (int k = 0; k < weights.Length; k++)
            {
                weights[k] = new EdgeWeight(r.ReadInt32(), r.ReadDouble());
            }
            var source = new FeatureRef(r.ReadInt32(), r.ReadInt64());
            double fromMeasure = r.ReadDouble(), toMeasure = r.ReadDouble();
            long fromShard = r.ReadInt64(), toShard = r.ReadInt64();
            shard.Edges.Add(new ShardEdge(
                new GraphEdge<long, long>(id, from, to, type, length, direction, disabled, weights),
                source, fromMeasure, toMeasure, fromShard, toShard));
        }

        return shard;
    }
}

/// <summary>A readable copy of a shard, for debugging only.</summary>
public static class ShardJson
{
    public static string Serialize(ShardData shard)
        => JsonSerializer.Serialize(new
        {
            shard.Id,
            Nodes = shard.Nodes.Select(n => new
            {
                n.Node.Id,
                n.Node.TypeId,
                n.Node.Location.X,
                n.Node.Location.Y,
                n.Node.Blocked,
                n.IsHelper,
                Source = n.Source?.ToString()
            }),
            Edges = shard.Edges.Select(e => new
            {
                e.Edge.Id,
                e.Edge.From,
                e.Edge.To,
                e.Edge.TypeId,
                e.Edge.Length,
                Direction = e.Edge.Direction.ToString(),
                e.Edge.Disabled,
                Weights = e.Edge.Weights.ToArray().Select(w => new { w.WeightId, w.Value }),
                Source = e.Source.ToString(),
                e.FromMeasure,
                e.ToMeasure,
                e.FromShard,
                e.ToShard
            })
        }, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
}
