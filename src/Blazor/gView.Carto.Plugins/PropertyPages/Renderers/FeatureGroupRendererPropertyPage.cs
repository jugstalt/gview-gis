using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;

namespace gView.Carto.Plugins.PropertyPages.Renderers;

[RegisterPlugIn("EBF49160-220E-4153-B24B-FD69A177E8A1")]
internal class FeatureGroupRendererPropertyPage : IPropertyPageDefinition
{
    public Type InterfaceType => typeof(IFeatureRenderer);

    public Type InstanceType => typeof(FeatureGroupRenderer);

    public Type PropertyPageType => typeof(Razor.Components.Controls.Renderers.FeatureGroupRendererPropertyPage);
}
