using gView.Framework.Cartography.Rendering.Abstractions;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using System.ComponentModel;

namespace gView.Carto.Razor.Components.Controls.Renderers.Models;
internal class RendererGroupItem : IRendererGroupItem, ICopy
{
    [Browsable(false)]
    public IRenderer? Renderer { get; set; }

    public object Copy() => new RendererGroupItem() { Renderer = Renderer };

    public override string ToString()
        => Renderer?.Name ?? "Unknown";
}
