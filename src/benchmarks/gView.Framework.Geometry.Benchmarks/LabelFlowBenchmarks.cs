using BenchmarkDotNet.Attributes;
using gView.Framework.Cartography;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Geometry;
using gView.Framework.Geometry.Engine;
using gView.Framework.Symbology;
using gView.GraphicsEngine;

namespace gView.Framework.Geometry.Benchmarks;

/// <summary>
/// A dense parcel map through the real <c>SimpleLabelRenderer</c> and label engine (many collisions):
/// the whole label flow of a map request, SpatialNET path against the classic one.
/// </summary>
[MemoryDiagnoser]
public class LabelFlowBenchmarks
{
    private sealed class CountingEngine(ILabelEngine inner) : ILabelEngine
    {
        public int Attempts, Placed;
        public void Init(IDisplay d, bool direct) => inner.Init(d, direct);
        public LabelAppendResult TryAppend(IDisplay d, IFeatureLayer l, ITextSymbol s, IGeometry g, bool c)
            => Count(inner.TryAppend(d, l, s, g, c));
        public LabelAppendResult TryAppend(IDisplay d, IFeatureLayer l, List<IAnnotationPolygonCollision> a, IGeometry g, bool c)
            => Count(inner.TryAppend(d, l, a, g, c));
        private LabelAppendResult Count(LabelAppendResult r) { Attempts++; if (r == LabelAppendResult.Succeeded) Placed++; return r; }
        public void AddBlockingGeometry(IDisplay d, IGeometry? g, float size = 10f, RenderLabelPriority p = RenderLabelPriority.Normal) => inner.AddBlockingGeometry(d, g, size, p);
        public void IndexBlockingGeometryToPriority(IDisplay d, RenderLabelPriority p) => inner.IndexBlockingGeometryToPriority(d, p);
        public void Draw(IDisplay d, ICancelTracker t) => inner.Draw(d, t);
        public void Release() => inner.Release();
        public gView.GraphicsEngine.Abstraction.ICanvas LabelCanvas => inner.LabelCanvas;
    }

    private sealed class ProbeDisplay : Display
    {
        public ProbeDisplay() : base(null) { Counter = new CountingEngine(_labelEngine); _labelEngine = Counter; }
        public CountingEngine Counter { get; }
    }

    [Params(0, int.MaxValue)]
    public int MinVertices;

    [Params(800, 3000)]
    public int Parcels;

    private ProbeDisplay _display = null!;
    private SimpleLabelRenderer _renderer = null!;
    private List<Feature> _features = null!;

    [GlobalSetup]
    public void Setup()
    {
        if (GraphicsEngine.Current.Engine == null)
        {
            gView.Framework.Common.SystemInfo.RegisterDefaultGraphicEngines();
        }

        _display = new ProbeDisplay { ImageWidth = 1110, ImageHeight = 691 };
        _display.ZoomTo(0, 0, 11100, 6910);   // 10 map units per pixel
        _display._canvas = GraphicsEngine.Current.Engine.CreateBitmap(1110, 691).CreateCanvas();

        _renderer = new SimpleLabelRenderer { UseExpression = true, LabelExpression = "[NAME]", TextSymbol = new SimpleTextSymbol() };
        typeof(SimpleLabelRenderer).GetField("_clipEnvelope",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(_renderer, new Envelope(0, 0, 11100, 6910));

        // parcels of 10-90 pixels, with 8 vertices, some cut by the map edge
        var random = new Random(7);
        _features = new List<Feature>();
        for (int i = 0; i < Parcels; i++)
        {
            double w = (10 + random.NextDouble() * 80) * 10, h = (10 + random.NextDouble() * 60) * 10;
            double x = random.NextDouble() * 11400 - 150, y = random.NextDouble() * 7200 - 150;
            var ring = new Ring(new List<IPoint>
            {
                new Point(x, y), new Point(x + w / 2, y - 5), new Point(x + w, y), new Point(x + w + 5, y + h / 2),
                new Point(x + w, y + h), new Point(x + w / 2, y + h + 5), new Point(x, y + h), new Point(x - 5, y + h / 2),
            });
            var feature = new Feature { Shape = new Polygon(ring) };
            feature.Fields.Add(new FieldValue("NAME", $"{random.Next(1, 2800)}/{random.Next(1, 20)}"));
            _features.Add(feature);
        }

        GeometryEngineOptions.LabelSpatialNetMinVertices = MinVertices;
        Console.WriteLine($"[{MinVertices}/{Parcels}] attempts {RunOnce()} placed {_display.Counter.Placed}");
    }

    private int RunOnce()
    {
        _display.Counter.Attempts = _display.Counter.Placed = 0;
        _display.LabelEngine.Init(_display, false);
        foreach (var feature in _features)
        {
            _renderer.Draw(_display, null!, feature);
        }

        return _display.Counter.Attempts;
    }

    [Benchmark]
    public int Flow()
    {
        GeometryEngineOptions.LabelSpatialNetMinVertices = MinVertices;
        return RunOnce();
    }
}
