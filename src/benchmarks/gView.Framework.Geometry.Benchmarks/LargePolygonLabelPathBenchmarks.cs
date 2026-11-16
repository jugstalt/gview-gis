using BenchmarkDotNet.Attributes;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry.GeoProcessing;

namespace gView.Framework.Geometry.Benchmarks;

/// <summary>
/// The large polygon path of <c>SimpleLabelRenderer</c> per feature: always clip first (before) against
/// the label point of the whole polygon first, clip only if it is not visible (now).
/// </summary>
[MemoryDiagnoser]
public class LargePolygonLabelPathBenchmarks
{
    [Params(12000, 100000)]
    public int Vertices;

    [Params("Smooth", "Jagged")]
    public string Shape = "Smooth";

    /// <summary>
    /// Inside: the map shows the middle, label point visible, polygon cut at all sides;
    /// Hidden: the map shows only the edge of the polygon, the label point is outside;
    /// Complete: the whole polygon is visible.
    /// </summary>
    [Params("Inside", "Hidden", "Complete")]
    public string Scenario = "Inside";

    private Polygon _polygon = null!;
    private Envelope _map = null!;
    private const double PixelSize = 50000 / 3779.5;  // 1 pixel at 1:50000

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _polygon = Shape == "Smooth"
            ? Shapes.SmoothBlob(random, Shapes.OriginX, Shapes.OriginY, 5000, Vertices)
            : Shapes.Blob(random, Shapes.OriginX, Shapes.OriginY, 5000, Vertices);

        double x = Shapes.OriginX, y = Shapes.OriginY;
        _map = Scenario switch
        {
            "Inside" => new Envelope(x - 2000, y - 2000, x + 2000, y + 2000),
            "Hidden" => new Envelope(x + 3000, y - 1000, x + 9000, y + 1000),
            _ => new Envelope(x - 20000, y - 20000, x + 20000, y + 20000),
        };
    }

    /// <summary>Before: clip to the map, then the label point of the clipped polygon.</summary>
    [Benchmark(Baseline = true)]
    public int AlwaysClip()
    {
        var clipped = LargePolygonLabeling.Clip(_polygon, _map);
        return clipped.RingCount == 0 ? 0 : LargePolygonLabeling.LabelPoints(clipped, PixelSize, false, _map).PointCount;
    }

    /// <summary>Now: <c>SimpleLabelRenderer.LargePolygonLabelPoints</c>.</summary>
    [Benchmark]
    public int LabelFirst()
        => SimpleLabelRenderer.LargePolygonLabelPoints(_polygon, _map, PixelSize, false)?.PointCount ?? 0;
}
