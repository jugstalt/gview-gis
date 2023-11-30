using gView.Framework.Core.Geometry;

namespace gView.Framework.Core.Symbology
{
    public interface ISymbolCreator
    {
        ISymbol CreateStandardSymbol(GeometryType type);
        ISymbol CreateStandardSelectionSymbol(GeometryType type);
        ISymbol CreateStandardHighlightSymbol(GeometryType type);
    }
}
