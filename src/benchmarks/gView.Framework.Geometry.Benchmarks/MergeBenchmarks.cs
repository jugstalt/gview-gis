using BenchmarkDotNet.Attributes;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry.Engine;
using gView.Framework.Geometry.GeoProcessing;

namespace gView.Framework.Geometry.Benchmarks;

/// <summary>
/// Merging many partly overlapping polygons (32 vertices each), as buffer queries and aggregate geometries do.
/// </summary>
[MemoryDiagnoser]
public class MergeBenchmarks
{
    [Params(false, true)]
    public bool UseSpatialNet;

    [Params(100, 1000)]
    public int Count;

    private List<IPolygon> _polygons = null!;

    [GlobalSetup]
    public void Setup()
    {
        GeometryEngineOptions.UseSpatialNetForMerge = UseSpatialNet;

        var random = new Random(42);
        // the extent grows with the count, so the overlap density stays about the same
        double extent = 100 * Math.Sqrt(Count);
        _polygons = Enumerable.Range(0, Count)
            .Select(_ =>
            {
                var center = Shapes.RandomPoint(random, extent);
                return (IPolygon)Shapes.Blob(random, center.X, center.Y, 40, 32);
            })
            .ToList();
    }

    [Benchmark]
    public int MergePolygons() => Algorithm.MergePolygons(_polygons).RingCount;

    [Benchmark]
    public int FastMergePolygon() => Algorithm.FastMergePolygon(_polygons, null, null).RingCount;
}
