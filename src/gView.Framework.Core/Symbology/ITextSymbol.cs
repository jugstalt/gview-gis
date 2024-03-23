using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;

namespace gView.Framework.Core.Symbology
{
    public interface ITextSymbol : ISymbol, ILabel, ISymbolTransformation, ISymbolRotation
    {
        GraphicsEngine.Abstraction.IFont Font { get; set; }

        float MaxFontSize { get; set; }
        float MinFontSize { get; set; }

        void Draw(IDisplay display, IGeometry geometry, TextSymbolAlignment symbolAlignment);
    }

    public enum SymbolSpacingType
    {
        // Never Change values
        None = 0,
        BoundingBox = 1,
        Margin = 2
    }

    public interface ISymbolSpacing
    {
        SymbolSpacingType SymbolSpacingType { get; set; }

        float SymbolSpacingX { get; set; }
        float SymbolSpacingY { get; set; }  
    }
}
