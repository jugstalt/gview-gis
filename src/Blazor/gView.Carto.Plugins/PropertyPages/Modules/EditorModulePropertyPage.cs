using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;
using gView.Plugins.Modules;

namespace gView.Carto.Plugins.PropertyPages.Modules;

[RegisterPlugIn("CA10D726-1FAC-4429-9BB6-76E3CDCC1D46")]
internal class EditorModulePropertyPage : IPropertyPageDefinition, IPropertyPageDescription
{
    public Type InterfaceType => typeof(IMapApplicationModule);

    public Type InstanceType => typeof(EditorModule);

    public Type PropertyPageType => typeof(Razor.Components.Controls.Modules.EditorModulePropertyPage);

    public string Title => "FeatureServer";

    public string Icon => "basic:edit";
}
