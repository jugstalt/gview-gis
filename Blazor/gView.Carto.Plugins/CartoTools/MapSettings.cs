using gView.Carto.Plugins.Extensions;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Carto;
using gView.Framework.Carto.Abstraction;
using gView.Framework.system;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("20BB9506-D6AE-4A81-AC2D-733DEE4465A4")]
internal class MapSettings : ICartoTool
{
    public string Name => "Map Settings";

    public string ToolTip => "";

    public ToolType ToolType => ToolType.Click;

    public string Icon => "basic:settings";

    public CartoToolTarget Target => CartoToolTarget.Map;

    public int SortOrder => 99;

    public void Dispose()
    {

    }

    public bool IsEnabled(IApplicationScope scope) => true;

    async public Task<bool> OnEvent(IApplicationScope scope)
    {
        var scopeService = scope.ToCartoScopeService();

        var model = await scopeService.ShowModalDialog(typeof(gView.Carto.Razor.Components.Dialogs.MapSettingsDialog),
                                                    "Map Properties",
                                                    new Razor.Components.Dialogs.Models.MapSettingsModel());

        if (model != null)
        {

        }

        return true;
    }
}
