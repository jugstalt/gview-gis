using Xunit;
using gView.Framework.NetworkModel.Build;
using SpatialNET;

namespace gView.Framework.NetworkModel.Tests;

public class NetworkBuildServiceTests
{
    private static (long Oid, Coordinate Location, int TypeId, bool Blocked) Point(long oid, double x, double y) => (oid, new Coordinate(x, y), 1, false);

    [Fact]
    public void RemoveDuplicates_PointsWithinTolerance_KeepsLowestOidAndReportsTheOthers()
    {
        var points = new List<(long, Coordinate, int, bool)> { Point(9, 0.005, 0), Point(3, 0, 0), Point(5, 10, 10) };
        var duplicates = new List<DuplicatePoint>();

        var kept = NetworkBuildService.RemoveDuplicates(points, 0.01, "NS/Kasten", duplicates);

        Assert.Equal([3L, 5L], kept.Select(p => p.Oid));
        var duplicate = Assert.Single(duplicates);
        Assert.Equal(("NS/Kasten", 3L, 9L), (duplicate.Layer, duplicate.KeptOid, duplicate.DroppedOid));
    }

    [Fact]
    public void RemoveDuplicates_NeighbouringGridCells_AreFound()
    {
        // 0.0099 and 0.0101 lie in different cells of a 0.01 grid but within the tolerance
        var points = new List<(long, Coordinate, int, bool)> { Point(1, 0.0099, 0.0099), Point(2, 0.0101, 0.0101) };
        var duplicates = new List<DuplicatePoint>();

        var kept = NetworkBuildService.RemoveDuplicates(points, 0.01, "L", duplicates);

        Assert.Single(kept);
        Assert.Single(duplicates);
    }

    [Fact]
    public void RemoveDuplicates_PointsFartherThanTolerance_AreKept()
    {
        var points = new List<(long, Coordinate, int, bool)> { Point(1, 0, 0), Point(2, 0.02, 0) };
        var duplicates = new List<DuplicatePoint>();

        var kept = NetworkBuildService.RemoveDuplicates(points, 0.01, "L", duplicates);

        Assert.Equal(2, kept.Count);
        Assert.Empty(duplicates);
    }
}
