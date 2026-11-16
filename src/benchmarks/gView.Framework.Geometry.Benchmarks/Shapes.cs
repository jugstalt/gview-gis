using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using Path = gView.Framework.Geometry.Path;

namespace gView.Framework.Geometry.Benchmarks;

/// <summary>
/// Deterministic test geometries in projected coordinates (UTM 33N, metres), so that classic and SpatialNET
/// compute the same thing; in degrees the classic Clipper paths round to about 100 m.
/// </summary>
internal static class Shapes
{
    public const double OriginX = 533000, OriginY = 5212000;

    /// <summary>A star-shaped, irregular polygon around a centre: a concave ring with many vertices.</summary>
    public static Polygon Blob(Random random, double centerX, double centerY, double radius, int vertices)
    {
        var points = new List<IPoint>(vertices);
        for (int i = 0; i < vertices; i++)
        {
            double angle = 2 * Math.PI * i / vertices;
            double r = radius * (0.7 + 0.3 * random.NextDouble());
            points.Add(new Point(centerX + r * Math.Cos(angle), centerY + r * Math.Sin(angle)));
        }

        return new Polygon(new Ring(points));
    }

    /// <summary>
    /// A smooth, concave polygon like a real border: a low-frequency shape with a little fine noise, so that
    /// neighbouring vertices are close together at any number of vertices.
    /// </summary>
    public static Polygon SmoothBlob(Random random, double centerX, double centerY, double radius, int vertices)
    {
        double phase1 = random.NextDouble() * 2 * Math.PI, phase2 = random.NextDouble() * 2 * Math.PI;
        var points = new List<IPoint>(vertices);
        for (int i = 0; i < vertices; i++)
        {
            double angle = 2 * Math.PI * i / vertices;
            double r = radius * (1 + 0.15 * Math.Sin(3 * angle + phase1) + 0.08 * Math.Sin(7 * angle + phase2)
                                 + 0.002 * (random.NextDouble() - 0.5));
            points.Add(new Point(centerX + r * Math.Cos(angle), centerY + r * Math.Sin(angle)));
        }

        return new Polygon(new Ring(points));
    }

    /// <summary>A random walk with the given number of vertices.</summary>
    public static Polyline Walk(Random random, double startX, double startY, double step, int vertices)
    {
        var points = new List<IPoint>(vertices);
        double x = startX, y = startY, heading = 0;
        for (int i = 0; i < vertices; i++)
        {
            points.Add(new Point(x, y));
            heading += (random.NextDouble() - 0.5) * 1.2;
            x += step * Math.Cos(heading);
            y += step * Math.Sin(heading);
        }

        return new Polyline(new Path(points));
    }

    public static Point RandomPoint(Random random, double extent) =>
        new(OriginX + random.NextDouble() * extent, OriginY + random.NextDouble() * extent);
}
