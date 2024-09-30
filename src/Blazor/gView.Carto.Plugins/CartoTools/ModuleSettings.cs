using gView.Carto.Core;
using gView.Carto.Core.Reflection;
using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("7B0A8E9D-78AA-463C-A87F-51627AD8143C")]
[RestorePointAction(RestoreAction.SetRestorePointOnClick)]
internal class ModuleSettings: ICartoButton
{
    public string Name => "Module Settings";

    public string ToolTip => "";

    public string Icon => "basic:settings";

    public CartoToolTarget Target => CartoToolTarget.Tools;

    public int SortOrder => 99;

    public void Dispose()
    {

    }

    public bool IsEnabled(ICartoApplicationScopeService scope) => true;

    async public Task<bool> OnClick(ICartoApplicationScopeService scope)
    {
        var model = await scope.ShowModalDialog(
                typeof(gView.Carto.Razor.Components.Dialogs.ModuleSettingsDialog),
                "Module Settings",
                new Razor.Components.Dialogs.Models.ModuleSettingsModel()
                {
                    Modules = scope.Document.Modules
                }
           );

        if (model != null)
        {

        }

        return true;
    }
}
