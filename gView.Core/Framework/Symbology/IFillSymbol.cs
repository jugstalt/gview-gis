using gView.Framework.Carto;

namespace gView.Framework.Symbology
{
    public interface IFillSymbol : ISymbol
    {
        void FillPath(IDisplay display, GraphicsEngine.Abstraction.IGraphicsPath path);
    }
}
