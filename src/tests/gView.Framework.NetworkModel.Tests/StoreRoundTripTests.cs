using Xunit;
using gView.Framework.NetworkModel.Ids;
using gView.Framework.NetworkModel.Store;
using SpatialNET;
using SpatialNET.Network;

namespace gView.Framework.NetworkModel.Tests;

/// <summary>ShardBinary, NodeIndex and AliasIndex: what is written is read back.</summary>
public class StoreRoundTripTests : IDisposable
{
    private readonly string _directory = Directory.CreateTempSubdirectory("gview-network-tests-").FullName;

    public void Dispose() => Directory.Delete(_directory, true);

    [Fact]
    public void ShardBinary_WriteRead_KeepsNodesAndEdges()
    {
        var shard = new ShardData(42);
        var box = new FeatureRef(1, 23660);
        shard.Nodes.Add(new ShardNode(new GraphNode<long>(NetworkIds.NodeId(box), 7, new Coordinate(1.5, -2.25), Blocked: true), false, box));
        var helper = new NetworkIds(0.01).NewHelperNode(new Coordinate(3, 4));
        shard.Nodes.Add(new ShardNode(new GraphNode<long>(helper, 0, new Coordinate(3, 4)), true, null));
        var cable = new FeatureRef(2, 99);
        shard.Edges.Add(new ShardEdge(
            new GraphEdge<long, long>(NetworkIds.Compose(2, 99, 1), NetworkIds.NodeId(box), helper, 8, 12.5, EdgeDirection.Forward, true,
                new EdgeWeight[] { new(1, 0.5) }),
            cable, 2, 14.5, 42, 43));

        using var stream = new MemoryStream();
        ShardBinary.Write(stream, shard);
        stream.Position = 0;
        var back = ShardBinary.Read(stream);

        Assert.Equal(42, back.Id);
        Assert.Equal(shard.Nodes, back.Nodes);
        var edge = Assert.Single(back.Edges);
        Assert.Equal(shard.Edges[0].Edge with { Weights = default }, edge.Edge with { Weights = default });
        Assert.Equal(shard.Edges[0].Edge.Weights.ToArray(), edge.Edge.Weights.ToArray());
        Assert.Equal((cable, 2.0, 14.5, 42L, 43L), (edge.Source, edge.FromMeasure, edge.ToMeasure, edge.FromShard, edge.ToShard));
    }

    [Fact]
    public void ShardBinary_WrongMagic_Throws()
    {
        using var stream = new MemoryStream("XXXX0000000000000000"u8.ToArray());

        Assert.Throws<InvalidDataException>(() => ShardBinary.Read(stream));
    }

    [Fact]
    public void NodeIndex_WriteRead_FindsShardAndType()
    {
        var path = Path.Combine(_directory, NodeIndex.FileName);
        NodeIndex.Write(path, [(30, 2, 5), (10, 1, 4), (20, 1, 5)]);

        var index = NodeIndex.Read(path);

        Assert.Equal(3, index.Count);
        Assert.True(index.TryGet(10, out var shard, out var type));
        Assert.Equal((1L, 4), (shard, type));
        Assert.False(index.TryGet(15, out _, out _));
        Assert.Equal([20L, 30L], index.NodesOfTypes(t => t == 5).Order());
    }

    [Fact]
    public void AliasIndex_WriteRead_MapsBothWays()
    {
        var path = Path.Combine(_directory, AliasIndex.FileName);
        AliasIndex.Write(path, [(7, 1, 16), (5, 1, 16), (9, 2, 17)]);

        var index = AliasIndex.Read(path);

        Assert.Equal(3, index.Count);
        Assert.True(index.TryGetNode(9, out var node));
        Assert.Equal(2, node);
        Assert.Equal(17, index.TypeOf(9));
        Assert.Equal(0, index.TypeOf(1));
        Assert.Equal([5L, 7L], index.AliasesOf(1));
        Assert.Equal([1L, 1L], index.NodesOfAliasTypes(t => t == 16));
    }
}
