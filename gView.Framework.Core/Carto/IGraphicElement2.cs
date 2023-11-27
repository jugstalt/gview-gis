using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;

namespace gView.Framework.Core.Carto
{
    public interface IGraphicElement2 : IGraphicElement, IGraphicsElementScaling, IGraphicsElementRotation, IIGraphicsElementTranslation, IGraphicsElementDesigning
    {
        string Name { get; }
        System.Drawing.Image Icon { get; }
        ISymbol Symbol { get; set; }
        void DrawGrabbers(IDisplay display);

        IGeometry Geometry { get; }
    }
}