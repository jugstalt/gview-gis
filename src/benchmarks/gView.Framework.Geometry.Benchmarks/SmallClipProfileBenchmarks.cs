using BenchmarkDotNet.Attributes;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry.Engine;
using gView.Framework.Geometry.GeoProcessing;
using SpatialNET;

namespace gView.Framework.Geometry.Benchmarks;

/// <summary>
/// Where the time of a SpatialNET clip of a small polygon goes: the clip itself, the result factory,
/// the conversions around it.
/// </summary>
[MemoryDiagnoser]
public class SmallClipProfileBenchmarks
{
    [Params(8, 50)]
    public int Vertices;

    private Polygon _polygon = null!;
    private Envelope _window = null!;
    private Envelope2D _rectangle;

    [GlobalSetup]
    public void Setup()
    {
        _polygon = Shapes.Blob(new Random(42), Shapes.OriginX, Shapes.OriginY, 5000, Vertices);
        double x = Shapes.OriginX, y = Shapes.OriginY, r = 5000;
        _window = new Envelope(x + r * 0.6, y - r * 0.2, x + r * 1.8, y + r * 0.2);
        _rectangle = new Envelope2D(_window.MinX, _window.MinY, _window.MaxX, _window.MaxY);
        GeometryEngineOptions.UseSpatialNetForClip = false;
    }

    [Benchmark(Baseline = true)]
    public int Clipper() => ((IPolygon)Clip.PerformClip(_window, _polygon)).RingCount;

    /// <summary>The bridge: conversion + Overlays.Clip + gView factory.</summary>
    [Benchmark]
    public int Bridge() => LargePolygonLabeling.Clip(_polygon, _window).RingCount;

    /// <summary>Overlays.Clip with the gView factory, without the bridge.</summary>
    [Benchmark]
    public int OverlaysClip_GViewFactory()
        => ((IPolygon)Overlays.Clip((IPolygon2D)_polygon, _rectangle, factory: GViewGeometryFactory.Instance)).RingCount;

    /// <summary>Overlays.Clip with SpatialNET's own geometry types.</summary>
    [Benchmark]
    public bool OverlaysClip_DefaultFactory()
        => Overlays.Clip((IPolygon2D)_polygon, _rectangle) is null;

    /// <summary>The envelope test alone, for reference.</summary>
    [Benchmark]
    public bool EnvelopeIntersects() => _window.Intersects(_polygon.Envelope);
}
