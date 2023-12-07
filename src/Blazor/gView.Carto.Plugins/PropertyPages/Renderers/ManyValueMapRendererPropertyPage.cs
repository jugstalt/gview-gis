using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.system;

namespace gView.Carto.Plugins.PropertyPages.Renderers;

[RegisterPlugIn("40600022-779D-4E49-B97F-164276742B0D")]
internal class ManyValueMapRendererPropertyPage : IPropertyPageDefinition
{
    public Type InterfaceType => typeof(IFeatureRenderer);

    public Type InstanceType => typeof(ManyValueMapRenderer);

    public Type PropertyPageType => typeof(Razor.Components.Controls.Renderers.ManyValueMapRendererPropertyPage);
}
