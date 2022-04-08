using gView.Framework.Carto;
using gView.Framework.Geometry;

namespace gView.Framework.Symbology
{
    public interface IPointSymbol : ISymbol, ISymbolTransformation
    {
        void DrawPoint(IDisplay display, IPoint point);
    }
}
