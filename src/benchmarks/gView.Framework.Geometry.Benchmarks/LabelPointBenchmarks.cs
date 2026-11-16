using BenchmarkDotNet.Attributes;
using gView.Framework.Cartography;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry.Engine;
using gView.Framework.Geometry.GeoProcessing;

namespace gView.Framework.Geometry.Benchmarks;

/// <summary>
/// Labelling polygons as <c>SimpleLabelRenderer</c> does it: the clip to the map, the first label point and
/// the alternative points after a collision. Classic gView against SpatialNET, by number of vertices
/// (12000 is the limit between <c>HandleSmallPolygons</c> and <c>HandleLargePolygons</c>).
/// </summary>
[MemoryDiagnoser]
public class LabelPointBenchmarks
{
    [Params(1000, 12000, 100000)]
    public int Vertices;

    /// <summary>Smooth like a border, or jagged (spiky) like a random star: the hard case for overlays.</summary>
    [Params("Smooth", "Jagged")]
    public string Shape = "Smooth";

    private Polygon _polygon = null!;
    private Envelope _cutEnvelope = null!;
    private double _tolerance;
    private Display _display = null!;
    private ISmartLabelPoint _smart = null!;
    private ISmartLabelPoint _spatialSmart = null!;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _polygon = Shape == "Smooth"
            ? Shapes.SmoothBlob(random, Shapes.OriginX, Shapes.OriginY, 5000, Vertices)
            : Shapes.Blob(random, Shapes.OriginX, Shapes.OriginY, 5000, Vertices);

        // a map section that cuts the polygon in half
        _cutEnvelope = new Envelope(Shapes.OriginX, Shapes.OriginY - 6000, Shapes.OriginX + 6000, Shapes.OriginY + 6000);

        if (gView.GraphicsEngine.Current.Engine == null)
        {
            gView.Framework.Common.SystemInfo.RegisterDefaultGraphicEngines();
        }

        _display = new Display(null) { MapScale = 50000 };
        _tolerance = _display.MapScale / _display.Dpm;  // 1 pixel

        _smart = (ISmartLabelPoint)Algorithm.PolygonLabelPoints(_polygon)[0];
        _spatialSmart = (ISmartLabelPoint)LargePolygonLabeling.LabelPoints(_polygon, _tolerance, false)[0];
    }

    // ---- first label point ------------------------------------------------------------------------

    /// <summary>Classic: centroid and inside test per ring.</summary>
    [Benchmark(Baseline = true)]
    public int FirstPoint_Classic() => Algorithm.PolygonLabelPoints(_polygon).PointCount;

    /// <summary>SpatialNET: <c>FastPolygonLabel</c>.</summary>
    [Benchmark]
    public int FirstPoint_SpatialNetCentroid() => LargePolygonLabeling.LabelPoints(_polygon, _tolerance, false).PointCount;

    /// <summary>SpatialNET: pole of inaccessibility (what the large path used before).</summary>
    [Benchmark]
    public bool FirstPoint_SpatialNetPolylabel() => SpatialNET.Shapes.Polylabel(_polygon, _tolerance).HasValue;

    // ---- alternative points after a collision --------------------------------------------------------

    /// <summary>Classic: the whole raster at once.</summary>
    [Benchmark]
    public int Alternatives_Eager() => _smart.AlernativeLabelPoints(_display).PointCount;

    /// <summary>Classic, lazy: the label engine takes the first alternative point that fits.</summary>
    [Benchmark]
    public int Alternatives_LazyFirst() => _smart.AlternativeLabelPointsLazy(_display).Take(1).Count();

    /// <summary>Classic, lazy: the fourth alternative point fits.</summary>
    [Benchmark]
    public int Alternatives_LazyFirstFour() => _smart.AlternativeLabelPointsLazy(_display).Take(4).Count();

    /// <summary>SpatialNET, lazy: the first alternative point (includes building what it needs).</summary>
    [Benchmark]
    public int Alternatives_SpatialNetFirst() => _spatialSmart.AlternativeLabelPointsLazy(_display).Take(1).Count();

    /// <summary>SpatialNET, lazy: the fourth alternative point fits.</summary>
    [Benchmark]
    public int Alternatives_SpatialNetFirstFour() => _spatialSmart.AlternativeLabelPointsLazy(_display).Take(4).Count();

    // ---- clip --------------------------------------------------------------------------------------

    /// <summary>Classic clip (Clipper) of a polygon cut by the map border.</summary>
    [Benchmark]
    public int Clip_Classic()
    {
        GeometryEngineOptions.UseSpatialNetForClip = false;
        return ((IPolygon)Clip.PerformClip(_cutEnvelope, _polygon)).RingCount;
    }

    /// <summary>SpatialNET clip of a polygon cut by the map border.</summary>
    [Benchmark]
    public int Clip_SpatialNet() => LargePolygonLabeling.Clip(_polygon, _cutEnvelope).RingCount;

    // ---- whole feature: clip + first point ------------------------------------------------------

    /// <summary>The small path: classic clip and classic label point.</summary>
    [Benchmark]
    public int Feature_SmallPath()
    {
        GeometryEngineOptions.UseSpatialNetForClip = false;
        var clipped = (IPolygon)Clip.PerformClip(_cutEnvelope, _polygon);
        return Algorithm.PolygonLabelPoints(clipped).PointCount;
    }

    /// <summary>The large path: SpatialNET clip and centroid.</summary>
    [Benchmark]
    public int Feature_LargePath()
    {
        var clipped = LargePolygonLabeling.Clip(_polygon, _cutEnvelope);
        return LargePolygonLabeling.LabelPoints(clipped, _tolerance, false).PointCount;
    }
}
