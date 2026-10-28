using BenchmarkDotNet.Attributes;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry.Engine;

namespace gView.Framework.Geometry.Benchmarks;

/// <summary>
/// <c>SpatialRelation.Check</c>, the spatial filter fallback of the data sources: one filter polygon against
/// 3000 features (points, lines, small polygons), as a cursor runs it.
/// </summary>
[MemoryDiagnoser]
public class SpatialRelationBenchmarks
{
    [Params(false, true)]
    public bool UseSpatialNet;

    [Params(spatialRelation.SpatialRelationIntersects, spatialRelation.SpatialRelationContains)]
    public spatialRelation Relation;

    /// <summary>A simple and a detailed filter polygon (e.g. a drawn selection vs. a district boundary).</summary>
    [Params(50, 2000)]
    public int FilterVertices;

    private SpatialFilter _filter = null!;
    private IGeometry[] _features = null!;

    [GlobalSetup]
    public void Setup()
    {
        GeometryEngineOptions.UseSpatialNetForSpatialRelation = UseSpatialNet;

        var random = new Random(42);
        const double extent = 10000;

        _filter = new SpatialFilter
        {
            Geometry = Shapes.Blob(random, Shapes.OriginX + extent / 2, Shapes.OriginY + extent / 2, extent * 0.35, FilterVertices),
            SpatialRelation = Relation,
        };

        var features = new List<IGeometry>();
        for (int i = 0; i < 1000; i++)
        {
            features.Add(Shapes.RandomPoint(random, extent));

            var start = Shapes.RandomPoint(random, extent);
            features.Add(Shapes.Walk(random, start.X, start.Y, 15, 20));

            var center = Shapes.RandomPoint(random, extent);
            features.Add(Shapes.Blob(random, center.X, center.Y, 60, 30));
        }

        _features = features.ToArray();
    }

    /// <summary>The same filter for all features, as one cursor: SpatialNET prepares the filter once.</summary>
    [Benchmark]
    public int Cursor()
    {
        int hits = 0;
        foreach (var feature in _features)
        {
            if (SpatialRelation.Check(_filter, feature))
            {
                hits++;
            }
        }

        return hits;
    }

    /// <summary>A new filter geometry per call, so the preparation is part of every query.</summary>
    [Benchmark]
    public int CursorWithFreshFilter()
    {
        var filter = new SpatialFilter
        {
            Geometry = new Polygon((IPolygon)_filter.Geometry),
            SpatialRelation = Relation,
        };

        int hits = 0;
        foreach (var feature in _features)
        {
            if (SpatialRelation.Check(filter, feature))
            {
                hits++;
            }
        }

        return hits;
    }
}
