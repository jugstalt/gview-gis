using gView.Carto.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Filters;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Carto;
using gView.Framework.Carto.Abstraction;
using gView.Framework.system;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("891D1698-1D8C-4785-9197-D8EFD8492C23")]
public class LoadMap : ICartoTool
{
    public string Name => "Open Map";

    public string ToolTip => "Open an existing map document";

    public ToolType ToolType => ToolType.Command;

    public string Icon => "basic:folder-white";

    public CartoToolTarget Target => CartoToolTarget.General;

    public int SortOrder => 1;

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

        var model = await scopeService.ShowKnownDialog(KnownDialogs.ExplorerDialog,
                                                       title: "Load existing map",
                                                       model: new ExplorerDialogModel()
                                                       {
                                                           Filters = new List<ExplorerDialogFilter> {
                                                                new OpenFileFilter("Map", "*.mxl")
                                                           },
                                                           Mode = ExploerDialogMode.Open
                                                       });

        string? mxlFilename = model?.Result.ExplorerObjects.FirstOrDefault()?.FullName;
        if(!String.IsNullOrEmpty(mxlFilename))
        {

        }

        return true;
    }
}
