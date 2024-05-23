using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;

namespace gView.Framework.Core.Symbology
{
    public interface IPointSymbol : ISymbol, ISymbolTransformation
    {
        void DrawPoint(IDisplay display, IPoint point);
    }
}
