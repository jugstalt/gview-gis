using gView.Framework.Geometry;
using gView.Framework.Symbology;

namespace gView.Framework.Carto
{
    public interface IGraphicElement2 : IGraphicElement, IGraphicsElementScaling, IGraphicsElementRotation, IIGraphicsElementTranslation, IGraphicsElementDesigning
    {
        string Name { get; }
        global::System.Drawing.Image Icon { get; }
        ISymbol Symbol { get; set; }
        void DrawGrabbers(IDisplay display);

        IGeometry Geometry { get; }
    }
}