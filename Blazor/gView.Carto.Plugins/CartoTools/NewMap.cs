using gView.Carto.Plugins.Extensions;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Carto;
using gView.Framework.Carto.Abstraction;
using gView.Framework.system;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("D6A44B2E-1883-4D69-984B-2468E98216CD")]
public class NewMap : ICartoTool
{
    public string Name => "New Map";

    public string ToolTip => "Create a new empty map";

    public ToolType ToolType => ToolType.Command;

    public string Icon => "basic:bulb-shining";

    public CartoToolTarget Target => CartoToolTarget.General;

    public int SortOrder => 0;

    public void Dispose()
    {
        
    }

    public bool IsEnabled(IApplicationScope scope)
    {
        return true;
    }

    async public Task<bool> OnEvent(IApplicationScope scope)
    {
        var scopeService = scope.ToCartoScopeService();

        var model = await scopeService.ShowModalDialog(typeof(gView.Carto.Razor.Components.Dialogs.NewMapDialog),
                                                    "Create New Map",
                                                    new Razor.Components.Dialogs.Models.NewMapModel());

        if (model != null)
        {
            scope.ToCartoScopeService().Map = new Map() { Name = model.Name };
        }

        return true;
    }
}
