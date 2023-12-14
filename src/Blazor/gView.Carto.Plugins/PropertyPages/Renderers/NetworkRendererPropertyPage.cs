using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;

namespace gView.Carto.Plugins.PropertyPages.Renderers;

[RegisterPlugIn("B76C20A9-47C7-4C84-9FD2-E9823011AB70")]
internal class NetworkRendererPropertyPage : IPropertyPageDefinition
{
    public Type InterfaceType => typeof(IFeatureRenderer);

    public Type InstanceType => typeof(NetworkRenderer);

    public Type PropertyPageType => typeof(Razor.Components.Controls.Renderers.NetworkRendererPropertyPage);
}
