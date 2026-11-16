using BenchmarkDotNet.Attributes;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry.GeoProcessing;

namespace gView.Framework.Geometry.Benchmarks;

/// <summary>
/// Typical (small) polygons through the label path of <c>SimpleLabelRenderer</c>: the classic path
/// (Clipper clip, then <c>Algorithm.PolygonLabelPoints</c>) against the SpatialNET path
/// (<c>LargePolygonLabelPoints</c>), i.e. what a threshold of 0 would do.
/// </summary>
[MemoryDiagnoser]
public class SmallPolygonLabelPathBenchmarks
{
    [Params(8, 50, 200)]
    public int Vertices;

    /// <summary>Radius in map units: 5000 m is about 750 pixels at 1:50000, 200 m about 30 pixels.</summary>
    [Params(200, 5000)]
    public double Radius;

    /// <summary>
    /// Inside: the map shows the middle of the polygon (cut at all sides, only for the big ones);
    /// Complete: the whole polygon is visible; Hidden: only the edge is visible, the label point is outside.
    /// </summary>
    [Params("Inside", "Complete", "Hidden")]
    public string Scenario = "Complete";

    private Polygon _polygon = null!;
    private Envelope _map = null!;
    private const double PixelSize = 50000 / 3779.5;  // 1 pixel at 1:50000

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _polygon = Vertices <= 12
            ? Shapes.Blob(random, Shapes.OriginX, Shapes.OriginY, Radius, Vertices)
            : Shapes.SmoothBlob(random, Shapes.OriginX, Shapes.OriginY, Radius, Vertices);

        double x = Shapes.OriginX, y = Shapes.OriginY, r = Radius;
        _map = Scenario switch
        {
            "Inside" => new Envelope(x - r * 0.4, y - r * 0.4, x + r * 0.4, y + r * 0.4),
            "Hidden" => new Envelope(x + r * 0.6, y - r * 0.2, x + r * 1.8, y + r * 0.2),
            _ => new Envelope(x - r * 4, y - r * 4, x + r * 4, y + r * 4),
        };
    }

    /// <summary>The classic path: <c>HandleSmallPolygons</c> up to the label points.</summary>
    [Benchmark(Baseline = true)]
    public int Classic()
    {
        var polygon = (IPolygon)_polygon;
        ((ITopologicalOperation)polygon).Clip(new Envelope(_map), out IGeometry g);
        if (g == null)
        {
            return 0;
        }

        if (g is IPolygon clipped)
        {
            polygon = clipped;
        }

        return Algorithm.PolygonLabelPoints(polygon).PointCount;
    }

    /// <summary>The SpatialNET path: <c>HandleLargePolygons</c> up to the label points.</summary>
    [Benchmark]
    public int SpatialNet()
        => SimpleLabelRenderer.LargePolygonLabelPoints(_polygon, _map, PixelSize, false)?.PointCount ?? 0;
}
