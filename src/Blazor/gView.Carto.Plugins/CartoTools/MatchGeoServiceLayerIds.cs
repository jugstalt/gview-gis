using gView.Carto.Core;
using gView.Carto.Core.Reflection;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Razor.Components.Dialogs;
using gView.Carto.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor.Models;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("E81A05E5-4FD4-49C4-9AE1-644115E92197")]
[RestorePointAction(RestoreAction.SetRestorePointOnClick)]
internal class MatchGeoServiceLayerIds : ICartoButton
{
    public string Name => "Match Service";

    public string ToolTip => "Match GeoService Layer Ids";

    public string Icon => "basic:globe";

    public CartoToolTarget Target => CartoToolTarget.Map;

    public int SortOrder => 999;

    public void Dispose()
    {

    }

    public bool IsEnabled(ICartoApplicationScopeService scope) => true;

    async public Task<bool> OnClick(ICartoApplicationScopeService scope)
    {
        await scope.ShowModalDialog(
                typeof(MatchGeoServiceLayerIdsDialog),
                "Match GeoService Layer Ids",
                new MatchGeoServiceLayerIdsModel()
                {
                    Map = scope.Document.Map
                },
                new ModalDialogOptions() {  }
            );

        return true;
    }
}
