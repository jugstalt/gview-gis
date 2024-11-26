using gView.Framework.Core.Carto;

namespace gView.Framework.Core.Data
{
    public interface ILayerCloneBeforeRender
    {
        ILayer CloneBeforeRender(IDisplay display);
    }
}