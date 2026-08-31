using gView.Framework.Core.Geometry;
using gView.Framework.Core.Geometry.Extensions;
using gView.Framework.Geometry;
using Path = gView.Framework.Geometry.Path;

namespace gView.Framework.Core.Tests.Geometry.Extensions;

/// <summary>
/// Tests for <see cref="GeometryExtensions.GetLength"/>/<see cref="GeometryExtensions.GetArea"/> -
/// the shared helpers behind ArcGIS Pro's <c>Length($feature)</c>/<c>Area($feature)</c> label
/// expression support (see <c>AprxLabelExpressionParser</c> and <c>SimpleLabelRenderer</c>).
/// Planar (Euclidean), matching the existing <c>ShapeLength</c>/<c>ShapeArea</c> auto-field
/// convention this reuses - not geodesic.
/// </summary>
public class GeometryExtensionsTests
{
    private static Polyline MultiPartPolyline() => new(new List<IPath>
    {
        new Path(new List<IPoint> { new Point(0, 0), new Point(3, 4) }),   // 3-4-5 triangle -> length 5
        new Path(new List<IPoint> { new Point(0, 0), new Point(0, 10) }),  // length 10
    });

    private static Polygon SquareWithHole()
    {
        // Outer ring: 10x10 square, explicitly closed (first point repeated at the end, matching
        // real-world ring data) -> perimeter 40, area 100.
        var outer = new Ring(new List<IPoint>
        {
            new Point(0, 0), new Point(10, 0), new Point(10, 10), new Point(0, 10), new Point(0, 0),
        });

        // Hole: 2x2 square fully inside the outer ring -> perimeter 8, area 4. Added as a plain
        // Ring - Polygon.Area/VerifyHoles() classifies it as a hole automatically based on
        // containment (Jordan test), no need to construct it as a Hole explicitly.
        var hole = new Ring(new List<IPoint>
        {
            new Point(2, 2), new Point(4, 2), new Point(4, 4), new Point(2, 4), new Point(2, 2),
        });

        var polygon = new Polygon(outer);
        polygon.AddRing(hole);
        return polygon;
    }

    // -----------------------------------------------------------------------
    // GetLength
    // -----------------------------------------------------------------------

    [Fact]
    public void GetLength_MultiPartPolyline_SumsAllPathLengths()
    {
        var length = ((IGeometry)MultiPartPolyline()).GetLength();

        Assert.Equal(15.0, length, precision: 6);
    }

    [Fact]
    public void GetLength_PolygonWithHole_SumsAllRingLengthsIncludingHole()
    {
        // Perimeter = every ring's boundary, hole included (not subtracted) - matches the
        // existing ShapeLength auto-field convention (see AutoFields/ShapeLength.cs).
        var length = ((IGeometry)SquareWithHole()).GetLength();

        Assert.Equal(40.0 + 8.0, length, precision: 6);
    }

    [Fact]
    public void GetLength_Point_ReturnsZero()
    {
        Assert.Equal(0.0, ((IGeometry)new Point(5, 5)).GetLength());
    }

    [Fact]
    public void GetLength_MultiPoint_ReturnsZero()
    {
        var multiPoint = new MultiPoint(new List<IPoint> { new Point(1, 1), new Point(2, 2) });

        Assert.Equal(0.0, ((IGeometry)multiPoint).GetLength());
    }

    [Fact]
    public void GetLength_Null_ReturnsZero()
    {
        IGeometry? geometry = null;

        Assert.Equal(0.0, geometry!.GetLength());
    }

    // -----------------------------------------------------------------------
    // GetArea
    // -----------------------------------------------------------------------

    [Fact]
    public void GetArea_PolygonWithHole_SubtractsHoleArea()
    {
        var area = ((IGeometry)SquareWithHole()).GetArea();

        Assert.Equal(100.0 - 4.0, area, precision: 6);
    }

    [Fact]
    public void GetArea_Polyline_ReturnsZero()
    {
        Assert.Equal(0.0, ((IGeometry)MultiPartPolyline()).GetArea());
    }

    [Fact]
    public void GetArea_Point_ReturnsZero()
    {
        Assert.Equal(0.0, ((IGeometry)new Point(5, 5)).GetArea());
    }

    [Fact]
    public void GetArea_Null_ReturnsZero()
    {
        IGeometry? geometry = null;

        Assert.Equal(0.0, geometry!.GetArea());
    }
}
