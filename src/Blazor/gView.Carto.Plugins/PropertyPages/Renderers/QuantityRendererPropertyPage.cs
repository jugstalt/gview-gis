using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.system;

namespace gView.Carto.Plugins.PropertyPages.Renderers;

[RegisterPlugIn("7235FD07-2889-4AA9-ABA2-B7284C24E42B")]
internal class QuantityRendererPropertyPage : IPropertyPageDefinition
{
    public Type InterfaceType => typeof(IFeatureRenderer);

    public Type InstanceType => typeof(QuantityRenderer);

    public Type PropertyPageType => typeof(Razor.Components.Controls.Renderers.QuantityRendererPropertyPage);
}