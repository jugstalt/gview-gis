using gView.Carto.Core;
using gView.Carto.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Filters;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Carto;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;
using gView.Framework.IO;
using gView.Framework.Common;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Plugins.Services;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("891D1698-1D8C-4785-9197-D8EFD8492C23")]
public class LoadDocument : ICartoInitialTool
{
    public string Name => "Open Map";

    public string ToolTip => "Open an existing map document";

    public ToolType ToolType => ToolType.Command;

    public string Icon => "basic:folder-white";

    public CartoToolTarget Target => CartoToolTarget.File;

    public int SortOrder => 2;

    public void Dispose()
    {

    }

    public bool IsEnabled(ICartoApplicationScopeService scope)
    {
        return true;
    }

    async public Task<bool> OnEvent(ICartoApplicationScopeService scope)
    {
        var model = await scope.ShowKnownDialog(KnownDialogs.ExplorerDialog,
                                                       title: "Load existing map",
                                                       model: new ExplorerDialogModel()
                                                       {
                                                           Filters = new List<ExplorerDialogFilter> {
                                                                new OpenFileFilter("Map", "*.mxl")
                                                           },
                                                           Mode = ExploerDialogMode.Open
                                                       });

        string? mxlFilePath = model?.Result.ExplorerObjects.FirstOrDefault()?.FullName;

        if (!String.IsNullOrEmpty(mxlFilePath))
        {
            return await ((CartoApplicationScopeService)scope).LoadCartoDocument(mxlFilePath);
        }

        return false;
    }
}
