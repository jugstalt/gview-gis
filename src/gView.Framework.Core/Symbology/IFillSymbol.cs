using gView.Framework.Core.Carto;

namespace gView.Framework.Core.Symbology
{
    public interface IFillSymbol : ISymbol
    {
        void FillPath(IDisplay display, GraphicsEngine.Abstraction.IGraphicsPath path);
    }
}
