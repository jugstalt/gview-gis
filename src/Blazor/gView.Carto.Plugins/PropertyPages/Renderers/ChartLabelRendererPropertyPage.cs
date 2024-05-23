using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;

namespace gView.Carto.Plugins.PropertyPages.Renderers;

[RegisterPlugIn("2FB4C6D5-90A8-4E4E-AA72-BF37FE098B2C")]
internal class ChartLabelRendererPropertyPage : IPropertyPageDefinition
{
    public Type InterfaceType => typeof(ILabelRenderer);

    public Type InstanceType => typeof(ChartLabelRenderer);

    public Type PropertyPageType => typeof(gView.Carto.Razor.Components.Controls.Renderers.ChartLabelRendererPropertyPage);
}