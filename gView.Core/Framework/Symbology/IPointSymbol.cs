using gView.Framework.Geometry;
using gView.Framework.Carto;

namespace gView.Framework.Symbology
{
    public interface IPointSymbol : ISymbol, ISymbolTransformation
    {
        void DrawPoint(IDisplay display, IPoint point);
    }
}
