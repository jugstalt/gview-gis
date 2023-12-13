using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;

namespace gView.Carto.Plugins.PropertyPages.Renderers;

[RegisterPlugIn("E44F0378-7876-4EB8-BC32-AB26322F5A73")]
internal class ScaleDependentLabelRendererPropertyPage : IPropertyPageDefinition
{
    public Type InterfaceType => typeof(ILabelRenderer);

    public Type InstanceType => typeof(ScaleDependentLabelRenderer);

    public Type PropertyPageType => typeof(gView.Carto.Razor.Components.Controls.Renderers.ScaleDependentLabelRendererPropertyPage);
}


