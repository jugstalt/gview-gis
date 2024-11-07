using gView.Framework.Core.Geometry;

namespace gView.Framework.Core.Symbology
{
    public interface INullSymbol : ISymbol
    {
        GeometryType GeomtryType { get; set; }
    }
}
