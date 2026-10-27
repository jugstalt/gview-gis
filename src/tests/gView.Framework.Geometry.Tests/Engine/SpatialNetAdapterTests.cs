using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Framework.Geometry.Engine;
using Path = gView.Framework.Geometry.Path;

namespace gView.Framework.Geometry.Tests.Engine;

/// <summary>
/// Tests for the SpatialNET adapter (ADR 0002): gView geometries seen through the SpatialNET
/// interfaces, and SpatialNET results created as gView geometries by <see cref="GViewGeometryFactory"/>.
/// No gView behaviour changes here; the bridge is not wired into ITopologicalOperation yet.
/// </summary>
public class SpatialNetAdapterTests
{
    private static Ring Square(double minX, double minY, double size, bool closed = true)
    {
        var points = new List<IPoint>
        {
            new Point(minX, minY), new Point(minX + size, minY),
            new Point(minX + size, minY + size), new Point(minX, minY + size),
        };
        if (closed)
        {
            points.Add(new Point(minX, minY));
        }

        return new Ring(points);
    }

    private static Hole SquareHole(double minX, double minY, double size) =>
        new(PointsOf(Square(minX, minY, size)));

    private static List<IPoint> PointsOf(IPointCollection points) =>
        Enumerable.Range(0, points.PointCount).Select(i => points[i]).ToList();

    private static Polygon PolygonOf(params IRing[] rings)
    {
        var polygon = new Polygon();
        foreach (var ring in rings)
        {
            polygon.AddRing(ring);
        }

        return polygon;
    }

    private static double NetArea(IPolygon polygon) =>
        polygon.Rings.Sum(r => r is IHole ? -Math.Abs(r.Area) : Math.Abs(r.Area));

    #region Interface views

    [Fact]
    public void Polygon_IsSeenAsSpatialPolygon_WithItsRingsAndPoints()
    {
        var polygon = PolygonOf(Square(0, 0, 10), SquareHole(2, 2, 2));

        SpatialNET.IPolygon2D spatial = polygon;

        Assert.Equal(2, spatial.RingCount);
        Assert.Same(polygon[1], spatial[1]);
        Assert.Equal(5, spatial[0].PointCount);
        Assert.Equal(10, spatial[0][1].X);
    }

    [Fact]
    public void Point_IsPoint3D_ButNotPointM()
    {
        var point = new Point(1, 2, 3);

        Assert.IsAssignableFrom<SpatialNET.IPoint3D>(point);
        Assert.IsNotAssignableFrom<SpatialNET.IPointM>(point);
        Assert.IsNotAssignableFrom<SpatialNET.IPointM>(new PointM(1, 2, "tag"));
    }

    #endregion

    #region Factory round trip

    [Fact]
    public void Difference_CreatesShellAsRingAndHoleAsHole()
    {
        var outer = PolygonOf(Square(0, 0, 10));
        var inner = PolygonOf(Square(2, 2, 2));

        var result = Assert.IsType<Polygon>(SpatialNetBridge.Difference(outer, inner));

        Assert.Equal(2, result.RingCount);
        Assert.Single(result.Rings, r => r is IHole);
        Assert.Single(result.Rings, r => r is not IHole);
        Assert.Equal(96, NetArea(result), 9);
    }

    [Fact]
    public void Result_TakesSrsFromFirstInput()
    {
        var a = PolygonOf(Square(0, 0, 10));
        a.Srs = 31256;
        var b = PolygonOf(Square(5, 5, 10));

        var result = SpatialNetBridge.Intersection(a, b);

        Assert.Equal(31256, result.Srs);
    }

    [Fact]
    public void Result_KeepsZ()
    {
        // Both inputs at height 5: every result vertex, whichever input it comes from, has Z = 5
        static Polygon AtHeight5(double minX, double minY)
        {
            var ring = new Ring();
            ring.AddPoint(new Point(minX, minY, 5));
            ring.AddPoint(new Point(minX + 10, minY, 5));
            ring.AddPoint(new Point(minX + 10, minY + 10, 5));
            ring.AddPoint(new Point(minX, minY + 10, 5));
            return PolygonOf(ring);
        }

        var result = Assert.IsType<Polygon>(SpatialNetBridge.Intersection(AtHeight5(0, 0), AtHeight5(5, 5)));

        Assert.All(PointsOf(result[0]), p => Assert.Equal(5, p.Z, 9));
    }

    #endregion

    #region Input conventions

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Ring_WithOrWithoutClosingPoint_GivesSameResult(bool closed)
    {
        var a = PolygonOf(Square(0, 0, 10, closed));
        var b = PolygonOf(Square(5, 5, 10, closed));

        var result = Assert.IsType<Polygon>(SpatialNetBridge.Intersection(a, b));

        Assert.Equal(25, NetArea(result), 9);
    }

    [Fact]
    public void UntypedInnerRing_IsTreatedAsHole()
    {
        // Data sources often deliver holes as plain rings; SpatialNET uses even-odd nesting instead of types
        var withPlainRing = PolygonOf(Square(0, 0, 10), Square(2, 2, 2));
        var all = PolygonOf(Square(-1, -1, 20));

        var result = Assert.IsType<Polygon>(SpatialNetBridge.Intersection(withPlainRing, all));

        Assert.Equal(96, NetArea(result), 9);
        Assert.Single(result.Rings, r => r is IHole);
    }

    [Fact]
    public void Polyline_IntersectedWithPolygon_IsPolyline()
    {
        var line = new Polyline(new Path(new List<IPoint> { new Point(-5, 5), new Point(15, 5) }));
        var area = PolygonOf(Square(0, 0, 10));

        var result = Assert.IsType<Polyline>(SpatialNetBridge.Intersection(line, area));

        Assert.Equal(10, result.Length, 9);
    }

    [Theory]
    [InlineData(15.4395, 47.0707, 0.001)]        // geographic, degrees (Graz)
    [InlineData(-68250.0, 215360.0, 100.0)]      // projected, metres (GK M34)
    [InlineData(533000.0, 5212000.0, 100.0)]     // projected, metres (UTM 33N)
    public void Overlap_IsExact_InDegreesAndMetres(double x, double y, double size)
    {
        var a = PolygonOf(Square(x, y, size));
        var b = PolygonOf(Square(x + size / 2, y + size / 2, size));

        var result = Assert.IsType<Polygon>(SpatialNetBridge.Intersection(a, b));

        var expected = size * size / 4;
        Assert.Equal(expected, NetArea(result), expected * 1e-6);
    }

    #endregion

    #region Unsupported input

    [Fact]
    public void Envelope_IsNotSupported()
    {
        var envelope = new Envelope(0, 0, 10, 10);

        Assert.Throws<NotSupportedException>(() => SpatialNetBridge.ToSpatial(envelope));
    }

    #endregion
}
