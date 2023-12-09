using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.system;

namespace gView.Carto.Plugins.PropertyPages.Renderers;

[RegisterPlugIn("F76CFBDA-04DC-4712-B9DA-446F5C1A2459")]
internal class ScaleDependentRendererPropertyPage : IPropertyPageDefinition
{
    public Type InterfaceType => typeof(IFeatureRenderer);

    public Type InstanceType => typeof(ScaleDependentRenderer);

    public Type PropertyPageType => typeof(Razor.Components.Controls.Renderers.ScaleDependentRendererPropertyPage);
}
