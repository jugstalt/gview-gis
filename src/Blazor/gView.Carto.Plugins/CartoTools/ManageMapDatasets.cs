using gView.Carto.Core;
using gView.Carto.Core.Reflection;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Razor.Components.Dialogs.Models;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("97DA61CF-120B-4ADB-BF31-C702F184A5F6")]
[AuthorizedPlugin(RequireAdminRole = true)]
[RestorePointAction(RestoreAction.SetRestorePointOnClick)]
public class ManageMapDatasets : ICartoButton
{
    public string ToolTip => "Manage Map Dataset and Connections";

    public string Icon => "basic:database";

    public CartoToolTarget Target => CartoToolTarget.Map;

    public int SortOrder => 98;

    public string Name => "Map Datasets";

    public bool IsEnabled(ICartoApplicationScopeService scope) => true;

    async public Task<bool> OnClick(ICartoApplicationScopeService scope)
    {
        var model = await scope.ShowModalDialog(
                typeof(Razor.Components.Dialogs.ManageMapDatasetsDialog),
                "Manage Map Datasets",
                new ManageMapDatasetsModel()
                {
                    Map = scope.Document.Map,
                }/*,
                new ModalDialogOptions()
                {
                    Width = ModalDialogWidth.ExtraExtraLarge,
                    FullWidth = true
                }*/);

        await scope.EventBus.FireMapSettingsChangedAsync();

        return true;
    }
}
