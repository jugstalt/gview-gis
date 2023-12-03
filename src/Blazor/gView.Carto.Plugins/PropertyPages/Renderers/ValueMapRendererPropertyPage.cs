using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.system;

namespace gView.Carto.Plugins.PropertyPages.Renderers;

[RegisterPlugIn("4FD15C29-C6E3-44C9-8F83-56C6711E3148")]
internal class ValueMapRendererPropertyPage : IPropertyPageDefinition
{
    public Type InterfaceType => typeof(IFeatureRenderer);

    public Type InstanceType => typeof(ValueMapRenderer);

    public Type PropertyPageType => typeof(Razor.Components.Controls.Renderers.ValueMapRendererPropertyPage);
}
