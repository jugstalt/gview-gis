using gView.Framework.Geometry;

namespace gView.Framework.Symbology
{
    public interface ISymbolCreator
    {
        ISymbol CreateStandardSymbol(GeometryType type);
        ISymbol CreateStandardSelectionSymbol(GeometryType type);
        ISymbol CreateStandardHighlightSymbol(GeometryType type);
    }
}
