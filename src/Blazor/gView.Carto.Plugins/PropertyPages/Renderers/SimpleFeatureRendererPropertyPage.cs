using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.system;

namespace gView.Carto.Plugins.PropertyPages.Renderers;

[RegisterPlugIn("CFB9B4B6-E7A7-44F6-AF57-C0F4ADED93D1")]
internal class SimpleFeatureRendererPropertyPage : IPropertyPageDefinition
{
    public Type InterfaceType => typeof(IFeatureRenderer);

    public Type InstanceType => typeof(SimpleRenderer);

    public Type PropertyPageType => typeof(gView.Carto.Razor.Components.Controls.Renderers.SimpleFeatureRendererPropertyPage);
}