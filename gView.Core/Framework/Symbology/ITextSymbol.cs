using gView.Framework.Geometry;
using gView.Framework.Carto;

namespace gView.Framework.Symbology
{
    public interface ITextSymbol : ISymbol, ILabel, ISymbolTransformation, ISymbolRotation
    {
        GraphicsEngine.Abstraction.IFont Font { get; set; }

        float MaxFontSize { get; set; }
        float MinFontSize { get; set; }

        void Draw(IDisplay display, IGeometry geometry, TextSymbolAlignment symbolAlignment);
    }
}
