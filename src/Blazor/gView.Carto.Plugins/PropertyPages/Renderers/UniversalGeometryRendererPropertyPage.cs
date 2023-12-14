using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;

namespace gView.Carto.Plugins.PropertyPages.Renderers;

[RegisterPlugIn("0C85962C-C445-481B-9B5F-EC124F78D961")]
internal class UniversalGeometryRendererPropertyPage : IPropertyPageDefinition
{
    public Type InterfaceType => typeof(IFeatureRenderer);

    public Type InstanceType => typeof(UniversalGeometryRenderer);

    public Type PropertyPageType => typeof(Razor.Components.Controls.Renderers.UniversalGeometryRendererPropertyPage);
}
