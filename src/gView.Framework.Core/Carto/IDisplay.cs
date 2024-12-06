using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;

namespace gView.Framework.Core.Carto
{
    public interface IDisplay
    {
        event MapScaleChangedEvent MapScaleChanged;
        event RenderOverlayImageEvent RenderOverlayImage;

        IEnvelope Envelope{ get; }
        IEnvelope Limit { get; set; }
        void ZoomTo(IEnvelope envelope);

        double ReferenceScale { get; set; }
        double MapScale { get; set; }

        float WebMercatorScaleLevel { get; }
        WebMercatorScaleBehavoir WebMercatorScaleBehavoir { get; set; }

        int ImageWidth { get; set; }
        int ImageHeight { get; set; }

        double Dpm { get; }
        double Dpi { get; set; }

        GraphicsEngine.Abstraction.IBitmap Bitmap { get; }
        GraphicsEngine.Abstraction.ICanvas Canvas { get; }

        GraphicsEngine.ArgbColor BackgroundColor { get; set; }
        GraphicsEngine.ArgbColor TransparentColor { get; set; }

        bool MakeTransparent { get; set;}

        void World2Image(ref double x, ref double y);
        void Image2World(ref double x, ref double y);

        ISpatialReference SpatialReference { get; set; }

        IGeometricTransformer GeometricTransformer { get; set; }

        void Draw(ISymbol symbol, IGeometry geometry);
        void DrawOverlay(IGraphicsContainer container, bool clearOld);
        void ClearOverlay();

        IGraphicsContainer GraphicsContainer { get; }

        ILabelEngine LabelEngine { get; }

        GeoUnits MapUnits { get; set; }
        GeoUnits DisplayUnits { get; set; }

        IScreen Screen { get; }
        IMap Map { get; }

        IDisplayRotation DisplayTransformation { get; }
    }
}