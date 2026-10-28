using BenchmarkDotNet.Attributes;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry.Engine;

namespace gView.Framework.Geometry.Benchmarks;

/// <summary>
/// <c>Buffer</c> of the four geometry types with a distance of 25 m.
/// </summary>
[MemoryDiagnoser]
public class BufferBenchmarks
{
    private const double Distance = 25;

    [Params(false, true)]
    public bool UseSpatialNet;

    private Point[] _points = null!;
    private MultiPoint _multiPoint = null!;
    private Polyline _polyline = null!;
    private Polygon _polygon = null!;

    [GlobalSetup]
    public void Setup()
    {
        GeometryEngineOptions.UseSpatialNetForBuffer = UseSpatialNet;

        var random = new Random(42);
        _points = Enumerable.Range(0, 100).Select(_ => Shapes.RandomPoint(random, 10000)).ToArray();
        _multiPoint = new MultiPoint(Enumerable.Range(0, 200).Select(_ => (IPoint)Shapes.RandomPoint(random, 2000)).ToList());
        _polyline = Shapes.Walk(random, Shapes.OriginX, Shapes.OriginY, 20, 500);
        _polygon = Shapes.Blob(random, Shapes.OriginX, Shapes.OriginY, 1000, 2000);
    }

    /// <summary>100 single points.</summary>
    [Benchmark]
    public int Points()
    {
        int rings = 0;
        foreach (var point in _points)
        {
            rings += point.Buffer(Distance).RingCount;
        }

        return rings;
    }

    /// <summary>200 points, some of them overlapping.</summary>
    [Benchmark]
    public int MultiPoint() => _multiPoint.Buffer(Distance).RingCount;

    /// <summary>A winding line with 500 vertices.</summary>
    [Benchmark]
    public int Polyline() => _polyline.Buffer(Distance).RingCount;

    /// <summary>A concave polygon with 2000 vertices.</summary>
    [Benchmark]
    public int Polygon() => _polygon.Buffer(Distance).RingCount;
}
