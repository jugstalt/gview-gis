using gView.Framework.Cartography;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Geometry;
using gView.Framework.Geometry.Engine;
using gView.Framework.Symbology;
using gView.GraphicsEngine;

namespace gView.Framework.Cartography.Rendering.Tests;

/// <summary>
/// Polygon labels with the real label engine: when the label point of a polygon collides with an
/// existing label, the SpatialNET path (<see cref="GeometryEngineOptions.LabelSpatialNetMinVertices"/>)
/// takes an alternative point.
/// </summary>
public class LabelCollisionTests
{
    /// <summary>Real label engine, but every <c>TryAppend</c> is recorded.</summary>
    private sealed class RecordingEngine(ILabelEngine inner) : ILabelEngine
    {
        public List<(IPoint Point, LabelAppendResult Result)> Attempts { get; } = new();

        public void Init(IDisplay display, bool directDraw) => inner.Init(display, directDraw);

        public LabelAppendResult TryAppend(IDisplay display, IFeatureLayer layer, ITextSymbol symbol, IGeometry geometry, bool checkForOverlap)
        {
            var result = inner.TryAppend(display, layer, symbol, geometry, checkForOverlap);
            Record(geometry, result);
            return result;
        }

        public LabelAppendResult TryAppend(IDisplay display, IFeatureLayer layer, List<IAnnotationPolygonCollision> aPolygons, IGeometry geometry, bool checkForOverlap)
        {
            var result = inner.TryAppend(display, layer, aPolygons, geometry, checkForOverlap);
            Record(geometry, result);
            return result;
        }

        private void Record(IGeometry geometry, LabelAppendResult result)
        {
            if (geometry is IPoint point)
            {
                Attempts.Add((new Point(point.X, point.Y), result));
            }
        }

        public void AddBlockingGeometry(IDisplay display, IGeometry? geometry, float size = 10f, RenderLabelPriority priority = RenderLabelPriority.Normal)
            => inner.AddBlockingGeometry(display, geometry, size, priority);

        public void IndexBlockingGeometryToPriority(IDisplay display, RenderLabelPriority priority)
            => inner.IndexBlockingGeometryToPriority(display, priority);

        public void Draw(IDisplay display, ICancelTracker cancelTracker) => inner.Draw(display, cancelTracker);

        public void Release() => inner.Release();

        public gView.GraphicsEngine.Abstraction.ICanvas LabelCanvas => inner.LabelCanvas;
    }

    private sealed class ProbeDisplay : Display
    {
        public ProbeDisplay() : base(null)
        {
            Recorder = new RecordingEngine(_labelEngine);
            _labelEngine = Recorder;
        }

        public RecordingEngine Recorder { get; }
    }

    private static Polygon Rectangle(double x, double y, double width, double height)
        => new(new Ring(new List<IPoint>
        {
            new Point(x, y), new Point(x + width, y), new Point(x + width, y + height), new Point(x, y + height),
        }));

    private static Feature NewFeature(IPolygon polygon)
    {
        var feature = new Feature { Shape = polygon };
        feature.Fields.Add(new FieldValue("NAME", "Label"));
        return feature;
    }

    [Theory]
    [InlineData(0)]            // SpatialNET path for all polygons (default)
    [InlineData(int.MaxValue)] // classic path
    public void PolygonLabel_OnCollision_TakesAnAlternativePointInsideThePolygon(int minVertices)
    {
        int before = GeometryEngineOptions.LabelSpatialNetMinVertices;
        GeometryEngineOptions.LabelSpatialNetMinVertices = minVertices;
        try
        {
            var display = new ProbeDisplay { ImageWidth = 800, ImageHeight = 600 };
            display.ZoomTo(0, 0, 8000, 6000);
            display._canvas = GraphicsEngine.Current.Engine.CreateBitmap(800, 600).CreateCanvas();  // measures the label text
            display.LabelEngine.Init(display, false);

            double pixel = display.MapScale / display.Dpm;

            // two identical wide polygons: the label point of the second one collides with the label of the first
            var polygon = Rectangle(1000, 2500, 6000, 1000);
            var renderer = new SimpleLabelRenderer
            {
                UseExpression = true,
                LabelExpression = "[NAME]",
                TextSymbol = new SimpleTextSymbol(),
            };

            // what PrepareQueryFilter does for a layer: the visible bounds
            typeof(SimpleLabelRenderer)
                .GetField("_clipEnvelope", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(renderer, new Envelope(0, 0, 8000, 6000));

            renderer.Draw(display, null!, NewFeature(polygon));
            Assert.Single(display.Recorder.Attempts);
            Assert.Equal(LabelAppendResult.Succeeded, display.Recorder.Attempts[0].Result);

            renderer.Draw(display, null!, NewFeature(Rectangle(1000, 2500, 6000, 1000)));

            var second = display.Recorder.Attempts.Skip(1).ToList();
            Assert.True(second.Count > 1, "the first point of the second polygon should collide");
            Assert.NotEqual(LabelAppendResult.Succeeded, second[0].Result);

            // the last attempt is the one that fits: another point, still inside the polygon
            var placed = second[^1];
            Assert.Equal(LabelAppendResult.Succeeded, placed.Result);
            Assert.True(gView.Framework.Geometry.GeoProcessing.Algorithm.Jordan(polygon, placed.Point.X, placed.Point.Y));
            double distance = Math.Sqrt(
                Math.Pow(placed.Point.X - second[0].Point.X, 2) + Math.Pow(placed.Point.Y - second[0].Point.Y, 2));
            Assert.True(distance >= 19 * pixel, $"alternative point {distance / pixel:0.0} px from the first point");
        }
        finally
        {
            GeometryEngineOptions.LabelSpatialNetMinVertices = before;
        }
    }
}
