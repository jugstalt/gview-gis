using gView.Framework.Core.Carto;

namespace gView.Framework.Core.Symbology
{
    public interface ILineSymbol : ISymbol
    {
        void DrawPath(IDisplay display, GraphicsEngine.Abstraction.IGraphicsPath path);
    }
}
