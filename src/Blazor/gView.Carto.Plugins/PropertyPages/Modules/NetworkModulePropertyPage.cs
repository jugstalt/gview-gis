using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;
using gView.Framework.NetworkModel.Module;

namespace gView.Carto.Plugins.PropertyPages.Modules;

[RegisterPlugIn("B7E3A1C4-5D2F-4E8B-9C61-2F0A7D3E5B94")]
internal class NetworkModulePropertyPage : IPropertyPageDefinition, IPropertyPageDescription
{
    public Type InterfaceType => typeof(IMapApplicationModule);

    public Type InstanceType => typeof(NetworkModule);

    public Type PropertyPageType => typeof(Razor.Components.Controls.Modules.NetworkModulePropertyPage);

    public string Title => "NetworkServer";

    public string Icon => "basic:share";
}
