using gView.Framework.Geometry;

namespace gView.Framework.Symbology
{
    public interface INullSymbol : ISymbol
    {
        GeometryType GeomtryType { get; set; }
    }
}
