using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;

namespace gView.Carto.Plugins.PropertyPages.Renderers;

[RegisterPlugIn("ADD9CB8E-3A9C-4CEC-9BA4-74AD205D6D0D")]
internal class SimpleLabelRendererPropertyPage : IPropertyPageDefinition
{
    public Type InterfaceType => typeof(ILabelRenderer);

    public Type InstanceType => typeof(SimpleLabelRenderer);

    public Type PropertyPageType => typeof(gView.Carto.Razor.Components.Controls.Renderers.SimpleLabelRendererPropertyPage);
}