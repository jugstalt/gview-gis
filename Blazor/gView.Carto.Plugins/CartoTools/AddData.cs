﻿using gView.Carto.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Filters;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Carto;
using gView.Framework.Carto.Abstraction;
using gView.Framework.system;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("7C9CA04B-7DFC-4028-8CEF-25D2A02272ED")]
internal class AddData : ICartoTool
{
    public string Name => "Add Data";

    public string ToolTip => "Add spatial data to map";

    public ToolType ToolType => ToolType.Command;

    public string Icon => "basic:database-plus";

    public CartoToolTarget Target => CartoToolTarget.General;

    public int SortOrder => 0;

    public void Dispose()
    {

    }

    public bool IsEnabled(IApplicationScope scope) => true;

    async public Task<bool> OnEvent(IApplicationScope scope)
    {
        var scopeService = scope.ToCartoScopeService();

        var model = await scopeService.ShowKnownDialog(KnownDialogs.ExplorerDialog,
                                                       title: "Add Data",
                                                       model: new ExplorerDialogModel()
                                                       {
                                                           Filters = new List<ExplorerDialogFilter> {
                                                                new OpenDataFilter()
                                                           },
                                                           Mode = ExploerDialogMode.Open
                                                       });

        return true;
    }
}