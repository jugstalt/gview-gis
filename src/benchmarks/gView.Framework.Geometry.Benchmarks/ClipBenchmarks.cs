using BenchmarkDotNet.Attributes;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry.Engine;
using gView.Framework.Geometry.GeoProcessing;

namespace gView.Framework.Geometry.Benchmarks;

/// <summary>
/// Clipping polygons to an envelope through <c>Clip.PerformClip</c>, as the renderers call it, including its
/// quick checks for polygons that are disjoint or completely inside.
/// </summary>
[MemoryDiagnoser]
public class ClipBenchmarks
{
    [Params(false, true)]
    public bool UseSpatialNet;

    private Polygon _large = null!;
    private Envelope _largeEnvelope = null!;
    private Polygon[] _small = null!;
    private Envelope _mapEnvelope = null!;

    [GlobalSetup]
    public void Setup()
    {
        GeometryEngineOptions.UseSpatialNetForClip = UseSpatialNet;

        var random = new Random(42);

        // a detailed polygon (e.g. a raster footprint or a highlighted district) cut by the map border
        _large = Shapes.Blob(random, Shapes.OriginX, Shapes.OriginY, 5000, 5000);
        _largeEnvelope = new Envelope(Shapes.OriginX - 2000, Shapes.OriginY - 2000, Shapes.OriginX + 6000, Shapes.OriginY + 6000);

        // polygon labels: many small polygons, a part of them on the map border
        const double extent = 10000;
        _small = Enumerable.Range(0, 2000)
            .Select(_ =>
            {
                var center = Shapes.RandomPoint(random, extent);
                return Shapes.Blob(random, center.X, center.Y, 80, 20);
            })
            .ToArray();
        _mapEnvelope = new Envelope(Shapes.OriginX + 2000, Shapes.OriginY + 2000, Shapes.OriginX + 8000, Shapes.OriginY + 8000);
    }

    /// <summary>One polygon with 5000 vertices, cut by the envelope.</summary>
    [Benchmark]
    public int LargePolygon() => ((IPolygon)Clip.PerformClip(_largeEnvelope, _large)).RingCount;

    /// <summary>2000 polygons with 20 vertices: most inside or outside (quick checks), some cut.</summary>
    [Benchmark]
    public int ManySmallPolygons()
    {
        int rings = 0;
        foreach (var polygon in _small)
        {
            if (Clip.PerformClip(_mapEnvelope, polygon) is IPolygon clipped)
            {
                rings += clipped.RingCount;
            }
        }

        return rings;
    }
}
