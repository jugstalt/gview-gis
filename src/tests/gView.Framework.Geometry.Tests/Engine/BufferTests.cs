using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Framework.Geometry.Engine;
using gView.Framework.Geometry.GeoProcessing;
using Path = gView.Framework.Geometry.Path;

namespace gView.Framework.Geometry.Tests.Engine;

/// <summary>
/// Tests for Buffer on SpatialNET (ADR 0002, phase 2, step 2): the comparison with gView's own buffers and
/// gView's conventions for distances ≤ 0. Both variants are called directly, so these tests do not depend on
/// the global switch. The results need not match vertex by vertex: the area of the symmetric difference of
/// old and new must stay below 1 % of the buffer area.
/// </summary>
public class BufferTests
{
    private const double MaxRelativeDeviation = 0.01;

    #region Geometry helpers

    private static Ring RingOf(params (double X, double Y)[] points) =>
        new(points.Select(p => (IPoint)new Point(p.X, p.Y)).ToList());

    private static Polygon PolygonOf(params Ring[] rings)
    {
        var polygon = new Polygon();
        foreach (var ring in rings)
        {
            polygon.AddRing(ring);
        }

        return polygon;
    }

    private static Path PathOf(params (double X, double Y)[] points) =>
        new(points.Select(p => (IPoint)new Point(p.X, p.Y)).ToList());

    private static Polyline LineOf(params Path[] paths) => new(paths.Cast<IPath>().ToList());

    private static MultiPoint MultiPointOf(params (double X, double Y)[] points) =>
        new(points.Select(p => (IPoint)new Point(p.X, p.Y)).ToList());

    private static double Area(IPolygon polygon) =>
        SpatialNET.Measures.Area((SpatialNET.IPolygon2D)polygon);

    #endregion

    #region Classic reference

    // gView's own buffers, as the Buffer() methods compute them with the switch off
    private static IPolygon ClassicBuffer(IGeometry geometry, double distance)
    {
        switch (geometry)
        {
            case Point point:
                return Algorithm.PointBuffer(point, distance);
            case MultiPoint multiPoint:
                var circles = Enumerable.Range(0, multiPoint.PointCount)
                    .Select(i => Algorithm.PointBuffer(multiPoint[i], distance))
                    .ToList();
                return Algorithm.FastMergePolygon(circles, null, null);
            case Polyline polyline:
                return Algorithm.PolylineBuffer(polyline, distance);
            case Polygon polygon:
                polygon.VerifyHoles();
                return Algorithm.PolygonBuffer(polygon, distance);
            default:
                throw new NotSupportedException(geometry.GetType().Name);
        }
    }

    #endregion

    #region Comparison old against new

    // Each shape is built at an origin and a scale; the distance is given for scale 1
    private static readonly (string Name, Func<double, double, double, IGeometry> Geometry, double Distance)[] Shapes =
    {
        ("point", (x, y, s) => new Point(x + 5 * s, y + 5 * s), 3),
        ("multipoint, overlapping", (x, y, s) => MultiPointOf((x, y), (x + 4 * s, y), (x + 20 * s, y + 3 * s)), 3),
        ("line, straight", (x, y, s) => LineOf(PathOf((x, y), (x + 50 * s, y + 10 * s))), 4),
        ("line, bent", (x, y, s) => LineOf(PathOf((x, y), (x + 30 * s, y), (x + 30 * s, y + 30 * s), (x + 5 * s, y + 40 * s))), 4),
        ("line, two paths", (x, y, s) => LineOf(PathOf((x, y), (x + 30 * s, y)), PathOf((x, y + 5 * s), (x + 30 * s, y + 20 * s))), 4),
        ("polygon, square", (x, y, s) => PolygonOf(RingOf((x, y), (x + 20 * s, y), (x + 20 * s, y + 20 * s), (x, y + 20 * s))), 3),
        ("polygon, concave L", (x, y, s) => PolygonOf(RingOf((x, y), (x + 30 * s, y), (x + 30 * s, y + 10 * s), (x + 10 * s, y + 10 * s), (x + 10 * s, y + 30 * s), (x, y + 30 * s))), 3),
        ("polygon with hole", (x, y, s) => PolygonOf(
            RingOf((x, y), (x + 40 * s, y), (x + 40 * s, y + 40 * s), (x, y + 40 * s)),
            RingOf((x + 15 * s, y + 15 * s), (x + 25 * s, y + 15 * s), (x + 25 * s, y + 25 * s), (x + 15 * s, y + 25 * s))), 3),
        ("polygon, shrunk", (x, y, s) => PolygonOf(RingOf((x, y), (x + 40 * s, y), (x + 40 * s, y + 30 * s), (x, y + 30 * s))), -5),
    };

    private const double DegreeScale = 0.0001;

    public static TheoryData<string, IGeometry, double> ProjectedCases()
    {
        var data = new TheoryData<string, IGeometry, double>();
        foreach (var (name, geometry, distance) in Shapes)
        {
            data.Add($"{name} (plain)", geometry(0, 0, 1), distance);
            data.Add($"{name} (UTM)", geometry(533000, 5212000, 10), distance * 10);
        }

        return data;
    }

    public static TheoryData<string> ShapeNames()
    {
        var data = new TheoryData<string>();
        foreach (var shape in Shapes)
        {
            data.Add(shape.Name);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(ProjectedCases))]
    public void Classic_And_SpatialNet_Buffers_CoverTheSameArea(string name, IGeometry geometry, double distance)
    {
        var classic = ClassicBuffer(geometry, distance);
        var spatialNet = SpatialNetBridge.Buffer(geometry, distance);

        Assert.NotNull(classic);
        Assert.NotNull(spatialNet);

        double area = Area(spatialNet);
        double deviation = Area((IPolygon)SpatialNET.Overlays.SymDifference(
            (SpatialNET.IPolygon2D)classic, (SpatialNET.IPolygon2D)spatialNet, factory: GViewGeometryFactory.Instance));

        Assert.True(area > 0, $"{name}: empty buffer");
        Assert.True(deviation < MaxRelativeDeviation * area,
            $"{name}: symmetric difference {deviation / area:P2} of the buffer area (classic {Area(classic):G6}, SpatialNET {area:G6})");
    }

    /// <summary>
    /// In degrees the classic buffers are no reference: gView's Clipper integration rounds to a grid of
    /// 0.001 map units (<c>Acc = 1000</c>), about 100 m in degrees. A buffer of 0.0003° vanishes there, and
    /// merged circles snap to that grid. So the SpatialNET buffer in degrees is checked against the same shape
    /// in plain coordinates, which is compared with the classic buffer above.
    /// </summary>
    [Theory]
    [MemberData(nameof(ShapeNames))]
    public void SpatialNet_Buffer_InDegrees_IsTheScaledPlainBuffer(string name)
    {
        var (_, geometry, distance) = Shapes.Single(s => s.Name == name);

        double plainArea = Area(SpatialNetBridge.Buffer(geometry(0, 0, 1), distance));
        double degreeArea = Area(SpatialNetBridge.Buffer(geometry(15.4, 47.0, DegreeScale), distance * DegreeScale));

        double expected = plainArea * DegreeScale * DegreeScale;
        Assert.True(Math.Abs(degreeArea - expected) < MaxRelativeDeviation * expected,
            $"{name}: degrees {degreeArea:G6}, scaled plain {expected:G6}");
    }

    [Fact]
    public void ClassicPolygonBuffer_InDegrees_LosesSmallDistances()
    {
        // documents why the classic buffer is no reference in degrees (see above)
        var square = PolygonOf(RingOf((15.4, 47.0), (15.402, 47.0), (15.402, 47.002), (15.4, 47.002)));

        double classic = Area(ClassicBuffer(square, 0.0003));
        double spatialNet = Area(SpatialNetBridge.Buffer(square, 0.0003));

        double exact = 0.002 * 0.002 + 4 * 0.002 * 0.0003 + Math.PI * 0.0003 * 0.0003;
        Assert.Equal(0.002 * 0.002, classic, 1e-12);
        Assert.Equal(exact, spatialNet, exact * 0.002);
    }

    [Fact]
    public void Buffer_TakesSrsFromInput()
    {
        var point = new Point(5, 5) { Srs = 31256 };

        Assert.Equal(31256, SpatialNetBridge.Buffer(point, 1).Srs);
    }

    #endregion

    #region Distances ≤ 0 (gView conventions)

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    public void PointsAndLines_WithDistanceNotPositive_GiveNull(double distance)
    {
        Assert.Null(SpatialNetBridge.Buffer(new Point(5, 5), distance));
        Assert.Null(SpatialNetBridge.Buffer(MultiPointOf((0, 0), (5, 5)), distance));
        Assert.Null(SpatialNetBridge.Buffer(LineOf(PathOf((0, 0), (10, 0))), distance));
    }

    [Fact]
    public void Polygon_WithDistanceZero_IsReturnedItself()
    {
        var polygon = PolygonOf(RingOf((0, 0), (10, 0), (10, 10), (0, 10)));

        Assert.Same(polygon, SpatialNetBridge.Buffer(polygon, 0));
    }

    [Fact]
    public void Polygon_ShrunkAway_GivesEmptyPolygon()
    {
        var polygon = PolygonOf(RingOf((0, 0), (10, 0), (10, 10), (0, 10)));

        var buffer = SpatialNetBridge.Buffer(polygon, -6);

        Assert.NotNull(buffer);
        Assert.Equal(0, buffer.RingCount);
    }

    #endregion
}

/// <summary>
/// The only tests that set the global switch; they run alone so that no other test sees it.
/// </summary>
[CollectionDefinition(nameof(GlobalSwitchCollection), DisableParallelization = true)]
public class GlobalSwitchCollection { }

[Collection(nameof(GlobalSwitchCollection))]
public class BufferSwitchTests
{
    [Fact]
    public void Switch_RoutesBufferToSpatialNet()
    {
        var line = new Polyline(new Path(new List<IPoint> { new Point(0, 0), new Point(10, 0), new Point(10, 10) }));
        bool before = GeometryEngineOptions.UseSpatialNetForBuffer;
        try
        {
            GeometryEngineOptions.UseSpatialNetForBuffer = true;
            var buffer = line.Buffer(1);

            // SpatialNET buffers the whole polyline in one pass: one outer ring, no merge artefacts
            Assert.Equal(1, buffer.RingCount);
            Assert.Equal(SpatialNET.Measures.Area((SpatialNET.IPolygon2D)SpatialNetBridge.Buffer(line, 1)),
                SpatialNET.Measures.Area((SpatialNET.IPolygon2D)buffer), 9);
        }
        finally
        {
            GeometryEngineOptions.UseSpatialNetForBuffer = before;
        }
    }
}
