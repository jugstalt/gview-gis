using Xunit;
using gView.Framework.NetworkModel.Ids;
using SpatialNET;

namespace gView.Framework.NetworkModel.Tests;

public class NetworkIdsTests
{
    [Theory]
    [InlineData(0, 0L, 0)]
    [InlineData(5, 23660L, 0)]
    [InlineData(17, 123456789L, 3)]
    [InlineData(NetworkIds.MaxFeatureClass, NetworkIds.MaxOid, NetworkIds.MaxParts - 1)]
    public void Compose_Decompose_RoundTrip(int featureClass, long oid, int part)
    {
        var id = NetworkIds.Compose(featureClass, oid, part);

        Assert.Equal((featureClass, oid, part), NetworkIds.Decompose(id));
        Assert.True(id >= 0);
        Assert.False(NetworkIds.IsHelper(id));
    }

    [Theory]
    [InlineData(-1, 1L, 0)]
    [InlineData(NetworkIds.HelperFeatureClass + 1, 1L, 0)]
    [InlineData(1, -1L, 0)]
    [InlineData(1, NetworkIds.MaxOid + 1, 0)]
    [InlineData(1, 1L, NetworkIds.MaxParts)]
    public void Compose_OutOfRange_Throws(int featureClass, long oid, int part)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => NetworkIds.Compose(featureClass, oid, part));
    }

    [Fact]
    public void NodeId_IsPartZeroOfTheFeature()
    {
        Assert.Equal(NetworkIds.Compose(3, 42), NetworkIds.NodeId(new FeatureRef(3, 42)));
    }

    [Fact]
    public void NewHelperNode_SameLocationInNewBuild_GivesSameId()
    {
        var first = new NetworkIds(0.01).NewHelperNode(new Coordinate(1000.001, 2000.004), 0);
        var second = new NetworkIds(0.01).NewHelperNode(new Coordinate(1000.002, 2000.001), 0);

        Assert.Equal(first, second);
        Assert.True(NetworkIds.IsHelper(first));
    }

    [Fact]
    public void NewHelperNode_SameCellTwiceInOneBuild_GivesDistinctIds()
    {
        var ids = new NetworkIds(0.01);

        var a = ids.NewHelperNode(new Coordinate(5, 5), 0);
        var b = ids.NewHelperNode(new Coordinate(5, 5), 0);

        Assert.NotEqual(a, b);
        Assert.True(NetworkIds.IsHelper(b));
    }

    [Fact]
    public void NewEdge_UsesFeatureAndPart()
    {
        var id = new NetworkIds(0.01).NewEdge(new FeatureRef(2, 77), 4, 9);

        Assert.Equal((2, 77L, 4), NetworkIds.Decompose(id));
    }
}
