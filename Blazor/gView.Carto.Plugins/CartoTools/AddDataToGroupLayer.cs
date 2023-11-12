using gView.Carto.Core.Models.Tree;
using gView.Carto.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Filters;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Carto;
using gView.Framework.Carto.Abstraction;
using gView.Framework.system;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("14A1B7E6-6D73-4062-A7BF-5CB7600A6DB3")]
internal class AddDataToGroupLayer : ICartoTool
{
    public string Name => "Add Data";

    public string ToolTip => "Add spatial data to group";

    public ToolType ToolType => ToolType.Command;

    public string Icon => "basic:database-plus";

    public CartoToolTarget Target => CartoToolTarget.SelectedTocItem;

    public int SortOrder => 0;

    public void Dispose()
    {

    }

    public bool IsEnabled(IApplicationScope scope)
    {
        var scopeService = scope.ToCartoScopeService();

        return scopeService.SelectedTocTreeNode is TocParentNode;
    }

    async public Task<bool> OnEvent(IApplicationScope scope)
    {
        var scopeService = scope.ToCartoScopeService();

        var model = await scopeService.ShowKnownDialog(
            KnownDialogs.ExplorerDialog,
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
