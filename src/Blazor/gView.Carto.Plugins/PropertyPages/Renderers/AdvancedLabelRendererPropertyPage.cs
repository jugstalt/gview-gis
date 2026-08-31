using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;

namespace gView.Carto.Plugins.PropertyPages.Renderers;

[RegisterPlugIn("FB442515-4877-4541-A657-CAF7F11C6506")]
internal class AdvancedLabelRendererPropertyPage : IPropertyPageDefinition
{
    public Type InterfaceType => typeof(ILabelRenderer);

    public Type InstanceType => typeof(AdvancedLabelRenderer);

    public Type PropertyPageType => typeof(gView.Carto.Razor.Components.Controls.Renderers.AdvancedLabelRendererPropertyPage);
}
