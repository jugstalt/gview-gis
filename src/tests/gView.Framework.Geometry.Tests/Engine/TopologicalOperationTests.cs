using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using Path = gView.Framework.Geometry.Path;

namespace gView.Framework.Geometry.Tests.Engine;

/// <summary>
/// Tests for the ITopologicalOperation methods repaired with SpatialNET in phase 1 (ADR 0002):
/// Polygon.Difference/SymDifference (used to compute Intersect) and the Polyline operations
/// (used to throw "not implemented"). Polygon.Union stays on gView's Merge.
/// </summary>
public class TopologicalOperationTests
{
    private static Polygon Square(double minX, double minY, double size, int? srs = null)
    {
        var ring = new Ring(new List<IPoint>
        {
            new Point(minX, minY), new Point(minX + size, minY),
            new Point(minX + size, minY + size), new Point(minX, minY + size), new Point(minX, minY),
        });

        return new Polygon(ring) { Srs = srs };
    }

    private static Polyline Line(params (double X, double Y)[] points) =>
        new(new Path(points.Select(p => (IPoint)new Point(p.X, p.Y)).ToList()));

    private static double NetArea(IPolygon polygon) =>
        polygon.Rings.Sum(r => r is IHole ? -Math.Abs(r.Area) : Math.Abs(r.Area));

    #region Polygon

    [Fact]
    public void Polygon_Difference_RemovesTheOverlap()
    {
        var polygon = Square(0, 0, 10, srs: 31256);

        polygon.Difference(Square(5, 5, 10));

        Assert.Equal(75, NetArea(polygon), 9);
        Assert.Equal(31256, polygon.Srs);
    }

    [Fact]
    public void Polygon_Difference_WithInnerPolygon_CreatesHole()
    {
        var polygon = Square(0, 0, 10);

        polygon.Difference(Square(2, 2, 2));

        Assert.Equal(96, NetArea(polygon), 9);
        Assert.Single(polygon.Holes);
    }

    [Fact]
    public void Polygon_Difference_WithDisjointPolygon_KeepsArea()
    {
        var polygon = Square(0, 0, 10);

        polygon.Difference(Square(20, 20, 10));

        Assert.Equal(100, NetArea(polygon), 9);
    }

    [Fact]
    public void Polygon_SymDifference_KeepsBothNonOverlappingParts()
    {
        var polygon = Square(0, 0, 10);

        polygon.SymDifference(Square(5, 5, 10));

        Assert.Equal(150, NetArea(polygon), 9);
    }

    [Fact]
    public void Polygon_SymDifference_WithPolyline_IsNotSupported()
    {
        var polygon = Square(0, 0, 10);

        Assert.Throws<NotSupportedException>(() => polygon.SymDifference(Line((-5, 5), (15, 5))));
    }

    [Fact]
    public void Polygon_Union_WithPolygon_StillUsesMerge()
    {
        var polygon = Square(0, 0, 10);

        polygon.Union(Square(5, 5, 10));

        Assert.Equal(175, NetArea(polygon), 6);
    }

    #endregion

    #region Polyline

    [Fact]
    public void Polyline_Intersect_WithPolygon_ClipsTheLine()
    {
        var line = Line((-5, 5), (15, 5));

        line.Intersect(Square(0, 0, 10));

        Assert.Equal(10, line.Length, 9);
        Assert.All(line[0].ToPointArray(), p => Assert.InRange(p.X, 0, 10));
    }

    [Fact]
    public void Polyline_Intersect_WithPoint_IsNotSupportedInPlace_ButWorksWithOutResult()
    {
        var line = Line((0, 0), (10, 0));

        Assert.Throws<NotSupportedException>(() => line.Intersect(new Point(5, 0)));

        line.Intersect(new Point(5, 0), out IGeometry result);
        Assert.Equal(GeometryType.Multipoint, result.GeometryType);
    }

    [Fact]
    public void Polyline_Difference_WithPolygon_KeepsTheOutsideParts()
    {
        var line = Line((-5, 5), (15, 5));

        line.Difference(Square(0, 0, 10));

        Assert.Equal(2, line.PathCount);
        Assert.Equal(10, line.Length, 9);
    }

    [Fact]
    public void Polyline_Difference_WithPolyline_RemovesTheOverlap()
    {
        var line = Line((0, 0), (10, 0));

        line.Difference(Line((4, 0), (6, 0)));

        Assert.Equal(8, line.Length, 9);
    }

    [Fact]
    public void Polyline_Union_WithPolyline_ContainsBothLines()
    {
        var line = Line((0, 0), (10, 0));

        line.Union(Line((5, -5), (5, 5)));

        Assert.Equal(20, line.Length, 9);
    }

    [Fact]
    public void Polyline_SymDifference_WithPolyline_DropsTheSharedPart()
    {
        var line = Line((0, 0), (10, 0));

        line.SymDifference(Line((5, 0), (15, 0)));

        Assert.Equal(10, line.Length, 9);
    }

    [Fact]
    public void Polyline_Union_WithPolygon_IsNotSupported()
    {
        var line = Line((0, 0), (10, 0));

        Assert.Throws<NotSupportedException>(() => line.Union(Square(0, 0, 10)));
    }

    #endregion
}

internal static class PointCollectionTestExtensions
{
    public static IPoint[] ToPointArray(this IPointCollection points) =>
        Enumerable.Range(0, points.PointCount).Select(i => points[i]).ToArray();
}
