using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Framework.Geometry.GeoProcessing;

namespace gView.Framework.Geometry.Tests.Engine;

/// <summary>
/// Clip and label points with SpatialNET for polygons with more than 12000 vertices (SimpleLabelRenderer).
/// </summary>
public class LargePolygonLabelingTests
{
    // A circle with many vertices around (cx, cy)
    private static Ring Circle(double cx, double cy, double r, int n)
    {
        var points = new List<IPoint>();
        for (int i = 0; i < n; i++)
        {
            double a = 2 * Math.PI * i / n;
            points.Add(new Point(cx + r * Math.Cos(a), cy + r * Math.Sin(a)));
        }

        return new Ring(points);
    }

    private static Polygon Large(params Ring[] rings)
    {
        var polygon = new Polygon();
        foreach (var ring in rings)
        {
            polygon.AddRing(ring);
        }

        return polygon;
    }

    [Fact]
    public void LabelPoints_LargePolygon_PointInsidePolygon()
    {
        var polygon = Large(Circle(0, 0, 1000, 15000));
        Assert.True(polygon.TotalPointCount > 12000);

        var points = LargePolygonLabeling.LabelPoints(polygon, 1.0, perPart: false);

        Assert.Equal(1, points.PointCount);
        Assert.True(Algorithm.Jordan(polygon, points[0].X, points[0].Y));
        Assert.Equal(0, points[0].X, 5);
        Assert.Equal(0, points[0].Y, 5);
    }

    [Fact]
    public void LabelPoints_TwoParts_OnePointForLargestOrOnePerPart()
    {
        var polygon = Large(Circle(0, 0, 1000, 8000), Circle(5000, 0, 100, 8000));

        var largest = LargePolygonLabeling.LabelPoints(polygon, 1.0, perPart: false);
        var perPart = LargePolygonLabeling.LabelPoints(polygon, 1.0, perPart: true);

        Assert.Equal(1, largest.PointCount);
        Assert.True(largest[0].X < 1100);  // in the large circle
        Assert.Equal(2, perPart.PointCount);
    }

    [Fact]
    public void Clip_ThenLabelPoint_LiesInVisiblePart()
    {
        var polygon = Large(Circle(0, 0, 1000, 15000));
        var clipped = LargePolygonLabeling.Clip(polygon, new Envelope(0, -2000, 2000, 2000));

        var points = LargePolygonLabeling.LabelPoints(clipped, 1.0, perPart: false);

        Assert.Equal(1, points.PointCount);
        Assert.True(points[0].X > 0 && points[0].X <= 1000);
    }
}
