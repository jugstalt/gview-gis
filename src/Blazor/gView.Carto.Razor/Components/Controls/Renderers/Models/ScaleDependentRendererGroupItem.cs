using gView.Framework.Cartography.Rendering.Abstractions;
using gView.Framework.Core.Carto;
using System.ComponentModel;

namespace gView.Carto.Razor.Components.Controls.Renderers.Models;
internal class ScaleDependentRendererGroupItem : IRendererGroupItem
{
    [Browsable(false)]
    public IRenderer? Renderer { get; set; }

    [Browsable(true)]
    [DisplayName("Minimum Scale (higher scale dominator) 1:")]
    public int MinimumScale { get; set; }

    [Browsable(true)]
    [DisplayName("Maximum Scale (lower scale dominator) 1:")]
    public int MaximumScale { get; set; }

    public override string ToString()
        => $"1:{MinimumScale} - 1:{MaximumScale} - {Renderer?.Name ?? "Unknown"}";
}
