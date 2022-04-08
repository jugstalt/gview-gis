using gView.Framework.Carto;
using gView.Framework.Geometry;

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
