using gView.Framework.Carto;

namespace gView.Framework.Symbology
{
    public interface ILineSymbol : ISymbol
    {
        void DrawPath(IDisplay display, GraphicsEngine.Abstraction.IGraphicsPath path);
    }
}
