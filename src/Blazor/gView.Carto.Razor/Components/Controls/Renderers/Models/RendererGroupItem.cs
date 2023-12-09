using gView.Framework.Cartography.Rendering.Abstractions;
using gView.Framework.Core.Carto;
using System.ComponentModel;

namespace gView.Carto.Razor.Components.Controls.Renderers.Models;
internal class RendererGroupItem : IRendererGroupItem
{
    [Browsable(false)]
    public IRenderer? Renderer { get; set; }
}
