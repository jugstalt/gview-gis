using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.system;

namespace gView.Carto.Plugins.PropertyPages.Renderers;

[RegisterPlugIn("EE79E6E1-CCE5-4BD0-9CE8-BF51F2B6DF43")]
internal class DimensionRendererPropertyPage : IPropertyPageDefinition
{
    public Type InterfaceType => typeof(IFeatureRenderer);

    public Type InstanceType => typeof(DimensionRenderer);

    public Type PropertyPageType => typeof(Razor.Components.Controls.Renderers.DimensionRendererPropertyPage);
}
