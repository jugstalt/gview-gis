#nullable enable

using gView.Framework.Core.Carto;

namespace gView.Framework.Cartography.Rendering.Abstractions
{
    public interface IRendererGroupItem
    {
        public IRenderer? Renderer { get; set; }
    }
}
