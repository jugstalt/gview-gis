using gView.DataExplorer.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Filters;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.DataExplorer;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.Geometry;
using gView.Razor.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerTools;

[gView.Framework.system.RegisterPlugIn("54597B19-B37A-4fa2-BA45-8B4137F2910E")]
public class Refresh : IExplorerTool
{
    #region IExplorerTool

    public string Name => "Refresh";

    public bool IsEnabled(IApplicationScope scope) => true;

    public string ToolTip => "";

    public string Icon => "basic:refresh";

    public ExplorerToolTarget Target => ExplorerToolTarget.General;

    async public Task<bool> OnEvent(IApplicationScope scope)
    {
        //var model = await scope.ToScopeService().ShowKnownDialog(
        //    Framework.Blazor.KnownDialogs.ExplorerDialog,
        //    model: new ExplorerDialogModel()
        //    {
        //        Filters = new List<ExplorerDialogFilter> {
        //            new OpenFDBFeatureclassFilter(),
        //            new OpenShapeFilter()
        //        },
        //        Mode = ExploerDialogMode.Open
        //    });

        //var model2 = await scope.ToScopeService().ShowKnownDialog(
        //    Framework.Blazor.KnownDialogs.ExplorerDialog,
        //    model: new ExplorerDialogModel()
        //    {
        //        Filters = SaveFeatureClassFilters.DatabaseFilters,
        //        Mode = ExploerDialogMode.Save
        //    });

        var model3 = await scope.ToScopeService().ShowKnownDialog<BaseDialogModel<ISpatialReference>>(
            Framework.Blazor.KnownDialogs.SpatialReferenceDialog,
            /* optional */ model: new BaseDialogModel<ISpatialReference>() { Value = new SpatialReference("epsg:31256") }
            );

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
