using gView.DataExplorer.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Filters;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.DataExplorer;
using gView.Framework.DataExplorer.Abstraction;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerTools;

[gView.Framework.system.RegisterPlugIn("54597B19-B37A-4fa2-BA45-8B4137F2910E")]
public class Refresh : IExplorerTool
{
    #region IExplorerTool

    public string Name => "Refresh";

    public bool IsEnabled(IExplorerApplicationScope scope) => true;

    public string ToolTip => "";

    public string Icon => "basic:refresh";

    public ExplorerToolTarget Target => ExplorerToolTarget.General;

    async public Task<bool> OnEvent(IExplorerApplicationScope scope)
    {
        //var model = await scope.ToScopeService().ShowModalDialog(
        //    typeof(Razor.Components.Dialogs.ExplorerDilaog),
        //    this.Name,
        //    new ExplorerDialogModel()
        //    {
        //        Filters = new List<ExplorerDialogFilter> {
        //            new OpenFDBFeatureclassFilter(),
        //            new OpenShapeFilter()
        //        },
        //        Mode = ExploerDialogMode.Open
        //    });

        //var model = await scope.ToScopeService().ShowModalDialog(
        //    typeof(Razor.Components.Dialogs.ExplorerDilaog),
        //    this.Name,
        //    new ExplorerDialogModel()
        //    {
        //        Filters = SaveFeatureClassFilters.DatabaseFilters,
        //        Mode = ExploerDialogMode.Save
        //    });

        await scope.ToScopeService().EventBus.FireFreshContentAsync();

        return true;
    }

    #endregion

    #region IOrder

    public int SortOrder => 15;

    #endregion

    #region IDisposable

    public void Dispose()
    {

    }

    #endregion
}
