using gView.Carto.Core;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Plugins.Extensions;
using gView.Carto.Plugins.Services;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Carto;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("D6A44B2E-1883-4D69-984B-2468E98216CD")]
public class NewDocument : ICartoInitialTool
{
    public string Name => "New Map";

    public string ToolTip => "Create a new empty map";

    public ToolType ToolType => ToolType.Command;

    public string Icon => "basic:bulb-shining";

    public CartoToolTarget Target => CartoToolTarget.Map;

    public int SortOrder => 0;

    public void Dispose()
    {

    }

    public bool IsEnabled(ICartoApplicationScopeService scope)
    {
        return true;
    }

    async public Task<bool> OnEvent(ICartoApplicationScopeService scope)
    {
        var model = await scope.ShowModalDialog(typeof(gView.Carto.Razor.Components.Dialogs.NewMapDialog),
                                                    "Create New Map",
                                                    new Razor.Components.Dialogs.Models.NewMapModel());

        if (model != null)
        {
            ((CartoApplicationScopeService)scope).Document = new CartoDocument(model.Name.Trim());
        }

        return true;
    }
}
