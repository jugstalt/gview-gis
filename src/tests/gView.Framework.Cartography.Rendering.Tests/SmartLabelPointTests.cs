using gView.Framework.Cartography;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Framework.Geometry.GeoProcessing;

namespace gView.Framework.Cartography.Rendering.Tests;

/// <summary>
/// The lazy alternative label points give the same points in the same order as the eager ones.
/// </summary>
public class SmartLabelPointTests
{
    private static Polygon Square(double size)
    {
        var ring = new Ring(new List<IPoint>
        {
            new Point(0, 0), new Point(size, 0), new Point(size, size), new Point(0, size),
        });

        return new Polygon(ring);
    }

    private static ISmartLabelPoint SmartPoint(Polygon polygon)
    {
        var points = Algorithm.PolygonLabelPoints(polygon);
        Assert.Equal(1, points.PointCount);

        return Assert.IsAssignableFrom<ISmartLabelPoint>(points[0]);
    }

    [Fact]
    public void AlternativeLabelPointsLazy_SamePointsInSameOrderAsEager()
    {
        var display = new Display(null) { MapScale = 1000 };
        double size = 100 * display.MapScale / display.Dpm;  // 100 pixels wide
        var smart = SmartPoint(Square(size));

        var eager = smart.AlernativeLabelPoints(display);
        var lazy = smart.AlternativeLabelPointsLazy(display).ToList();

        Assert.True(eager.PointCount > 0);
        Assert.Equal(eager.PointCount, lazy.Count);
        for (int i = 0; i < lazy.Count; i++)
        {
            Assert.Equal(eager[i].X, lazy[i].X);
            Assert.Equal(eager[i].Y, lazy[i].Y);
        }
    }

    [Fact]
    public void AlternativeLabelPointsLazy_TakingTheFirstPoint_DoesNotNeedTheWholeRaster()
    {
        var display = new Display(null) { MapScale = 1000 };
        double size = 100 * display.MapScale / display.Dpm;
        var smart = SmartPoint(Square(size));

        var first = smart.AlternativeLabelPointsLazy(display).First();

        // The first raster ring is 20 pixels around the centre
        double expected = 20.0 * display.MapScale / display.Dpm;
        double distance = Math.Sqrt(Math.Pow(first.X - smart.X, 2) + Math.Pow(first.Y - smart.Y, 2));
        Assert.Equal(expected, distance, 6);
    }

    [Fact]
    public void LargePolygon_AlternativePoints_AreLazyInsideAndSeparated()
    {
        var display = new Display(null) { MapScale = 50000 };
        double pixel = display.MapScale / display.Dpm;
        var circle = new List<IPoint>();
        for (int i = 0; i < 15000; i++)
        {
            double a = 2 * Math.PI * i / 15000;
            circle.Add(new Point(1000 * pixel * Math.Cos(a), 1000 * pixel * Math.Sin(a)));
        }

        var polygon = new Polygon(new Ring(circle));
        var points = LargePolygonLabeling.LabelPoints(polygon, pixel, perPart: false);
        var smart = Assert.IsAssignableFrom<ISmartLabelPoint>(points[0]);

        var alternatives = smart.AlternativeLabelPointsLazy(display).Take(5).ToList();

        Assert.Equal(5, alternatives.Count);
        foreach (var point in alternatives)
        {
            Assert.True(Algorithm.Jordan(polygon, point.X, point.Y));
            Assert.True(Math.Sqrt(Math.Pow(point.X - smart.X, 2) + Math.Pow(point.Y - smart.Y, 2)) >= 20 * pixel * 0.99);
        }
    }
}
