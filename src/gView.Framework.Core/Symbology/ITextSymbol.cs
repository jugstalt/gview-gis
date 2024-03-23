using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;

namespace gView.Framework.Core.Symbology
{
    public interface ITextSymbol : ISymbol, ILabel, ISymbolTransformation, ISymbolRotation
    {
        GraphicsEngine.Abstraction.IFont Font { get; set; }

        float MaxFontSize { get; set; }
        float MinFontSize { get; set; }

        float SymbolSpacing { get; set; }

        void Draw(IDisplay display, IGeometry geometry, TextSymbolAlignment symbolAlignment);
    }
}
