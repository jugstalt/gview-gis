using gView.Framework.Cartography.Rendering.Abstractions;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using System.ComponentModel;

namespace gView.Carto.Razor.Components.Controls.Renderers.Models;
internal class ScaleDependentRendererGroupItem : IRendererGroupItem, ICopy
{
    [Browsable(false)]
    public IRenderer? Renderer { get; set; }

    [Browsable(true)]
    [DisplayName("Minimum Scale (higher scale dominator) 1:")]
    public int MaximumScale { get; set; }

    [Browsable(true)]
    [DisplayName("Maximum Scale (lower scale dominator) 1:")]
    public int MinimumScale { get; set; }

    public object Copy() => new ScaleDependentRendererGroupItem()
    {
        Renderer = Renderer,
        MaximumScale = MaximumScale,
        MinimumScale = MinimumScale
    };
    
    public override string ToString()
        => $"1:{MaximumScale} - 1:{MinimumScale} - {Renderer?.Name ?? "Unknown"}";
}
