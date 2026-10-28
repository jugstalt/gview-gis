using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Framework.Geometry.Engine;
using gView.Framework.Geometry.GeoProcessing;
using gView.Framework.Geometry.GeoProcessing.Clipper;
using Path = gView.Framework.Geometry.Path;

namespace gView.Framework.Geometry.Tests.Engine;

/// <summary>
/// Tests for ADR 0002, phase 2, step 4: clipping polygons to an envelope on SpatialNET, compared with
/// Clipper, and the repaired <c>Polygon.Intersect</c> with geometries other than envelopes.
/// The classic clip is called with the switch off (its default); <see cref="ClipSwitchTests"/> sets it.
/// </summary>
public class ClipTests
{
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

    internal static Polygon Square(double x, double y, double size) =>
        PolygonOf(RingOf((x, y), (x + size, y), (x + size, y + size), (x, y + size)));

    internal static double Area(IPolygon polygon) =>
        SpatialNET.Measures.Area((SpatialNET.IPolygon2D)polygon);

    #endregion

    #region Clip: comparison old against new

    // Each polygon is built at an origin and a scale; the clip envelope is (x + 5s, y + 5s) .. (x + 25s, y + 25s)
    private static readonly (string Name, Func<double, double, double, Polygon> Polygon)[] Polygons =
    {
        ("square crossing the border", (x, y, s) => Square(x, y, 15 * s)),
        ("concave U", (x, y, s) => PolygonOf(RingOf(
            (x, y + 10 * s), (x + 30 * s, y + 10 * s), (x + 30 * s, y + 40 * s), (x + 20 * s, y + 40 * s),
            (x + 20 * s, y + 20 * s), (x + 10 * s, y + 20 * s), (x + 10 * s, y + 40 * s), (x, y + 40 * s)))),
        ("polygon with hole on the border", (x, y, s) => PolygonOf(
            RingOf((x, y), (x + 30 * s, y), (x + 30 * s, y + 30 * s), (x, y + 30 * s)),
            RingOf((x + 20 * s, y + 20 * s), (x + 28 * s, y + 20 * s), (x + 28 * s, y + 28 * s), (x + 20 * s, y + 28 * s)))),
        ("polygon covering the envelope", (x, y, s) => Square(x, y, 40 * s)),
    };

    private static Envelope ClipEnvelope(double x, double y, double s) =>
        new(x + 5 * s, y + 5 * s, x + 25 * s, y + 25 * s);

    public static TheoryData<string> PolygonNames()
    {
        var data = new TheoryData<string>();
        foreach (var polygon in Polygons)
        {
            data.Add(polygon.Name);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(PolygonNames))]
    public void Classic_And_SpatialNet_Clip_CoverTheSameArea(string name)
    {
        var (_, polygon) = Polygons.Single(p => p.Name == name);

        // plain and UTM coordinates; integer coordinates are exact on Clipper's 0.001 grid
        foreach (var (x, y, s) in new[] { (0.0, 0.0, 1.0), (533000.0, 5212000.0, 10.0) })
        {
            var classic = ClipperExtensions.Clip(polygon(x, y, s), ClipEnvelope(x, y, s));
            var spatialNet = SpatialNetBridge.ClipPolygon(polygon(x, y, s), ClipEnvelope(x, y, s));

            double area = Area(spatialNet);
            double deviation = Area((IPolygon)SpatialNET.Overlays.SymDifference(
                (SpatialNET.IPolygon2D)classic, (SpatialNET.IPolygon2D)spatialNet, factory: GViewGeometryFactory.Instance));

            Assert.True(area > 0, $"{name} at scale {s}: empty clip");
            Assert.True(deviation < 1e-9 * area,
                $"{name} at scale {s}: classic {Area(classic):G9}, SpatialNET {area:G9}, symmetric difference {deviation:G6}");
        }
    }

    [Fact]
    public void Clip_CutsConcavePolygonIntoTwoParts()
    {
        var u = Polygons.Single(p => p.Name == "concave U").Polygon(0, 0, 1);

        // the envelope cuts through both legs (x 0..10 and 20..30, y 20..40) above the bar
        var clipped = SpatialNetBridge.ClipPolygon(u, new Envelope(5, 25, 25, 35));

        Assert.Equal(2, clipped.RingCount);
        Assert.Equal(2 * 5 * 10, Area(clipped), 9);
    }

    [Fact]
    public void Clip_OfDisjointPolygon_IsEmpty_AsWithClipper()
    {
        var polygon = Square(100, 100, 10);
        var envelope = new Envelope(0, 0, 10, 10);

        Assert.Equal(0, ClipperExtensions.Clip(polygon, envelope).RingCount);
        Assert.Equal(0, SpatialNetBridge.ClipPolygon(polygon, envelope).RingCount);
    }

    [Fact]
    public void Clip_InDegrees_IsExact()
    {
        // a square of 0.002° clipped to 0.0012° x 0.0012°: far below Clipper's 0.001 grid
        var polygon = Square(15.4, 47.0, 0.002);
        var envelope = new Envelope(15.4015, 47.0015, 15.4027, 47.0027);

        var clipped = SpatialNetBridge.ClipPolygon(polygon, envelope);

        double exact = 0.0005 * 0.0005;
        Assert.Equal(exact, Area(clipped), exact * 1e-6);
    }

    #endregion

    #region Polygon.Intersect (repair)

    [Fact]
    public void Intersect_WithPolygon_ComputesTheIntersection()
    {
        // PerformClip used to return null for polygon clippers, so Intersect changed nothing
        var polygon = Square(0, 0, 10);

        polygon.Intersect(Square(5, 5, 10));

        Assert.Equal(25, Area(polygon), 9);
    }

    [Fact]
    public void Intersect_WithPolyline_GivesPolylineAsOutResult()
    {
        var polygon = Square(0, 0, 10);
        var line = new Polyline(new Path(new List<IPoint> { new Point(-5, 5), new Point(15, 5) }));

        polygon.Intersect(line, out IGeometry result);

        var clipped = Assert.IsType<Polyline>(result);
        Assert.Equal(10, clipped.Length, 9);
        Assert.Throws<NotSupportedException>(() => Square(0, 0, 10).Intersect(line));
    }

    [Fact]
    public void Intersect_WithEnvelope_StaysClassic()
    {
        var polygon = Square(0, 0, 10);

        polygon.Intersect(new Envelope(5, 5, 15, 15));

        Assert.Equal(25, Area(polygon), 9);
    }

    #endregion
}

[Collection(nameof(GlobalSwitchCollection))]
public class ClipSwitchTests
{
    [Fact]
    public void Switch_RoutesPolygonClipToSpatialNet()
    {
        bool before = GeometryEngineOptions.UseSpatialNetForClip;
        try
        {
            GeometryEngineOptions.UseSpatialNetForClip = true;

            var envelope = new Envelope(15.4015, 47.0015, 15.4027, 47.0027);
            double exact = 0.0005 * 0.0005;

            var clipped = Clip.PerformClip(envelope, ClipTests.Square(15.4, 47.0, 0.002));
            Assert.Equal(exact, ClipTests.Area(Assert.IsType<Polygon>(clipped)), exact * 1e-6);

            var polygon = ClipTests.Square(15.4, 47.0, 0.002);
            polygon.Clip(envelope);
            Assert.Equal(exact, ClipTests.Area(polygon), exact * 1e-6);
        }
        finally
        {
            GeometryEngineOptions.UseSpatialNetForClip = before;
        }
    }
}
