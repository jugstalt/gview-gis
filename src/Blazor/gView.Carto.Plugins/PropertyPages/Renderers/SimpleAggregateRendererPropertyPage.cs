using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.system;

namespace gView.Carto.Plugins.PropertyPages.Renderers;

[RegisterPlugIn("85A32A55-ECB5-4434-A46A-5651D987CFEC")]
internal class SimpleAggregateRendererPropertyPage : IPropertyPageDefinition
{
    public Type InterfaceType => typeof(IFeatureRenderer);

    public Type InstanceType => typeof(SimpleAggregateRenderer);

    public Type PropertyPageType => typeof(gView.Carto.Razor.Components.Controls.Renderers.SimpleAggregateRendererPropertyPage);
}