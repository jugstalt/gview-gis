using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Core.Common;
using gView.Framework.DataExplorer;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Framework.DataExplorer.Services.Abstraction;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerTools;

[RegisterPlugIn("4F54D455-1C22-469e-9DBB-78DBBEF6078D")]
public class Delete : IExplorerTool
{
    public string Name => "Delete";

    public string ToolTip => "";

    public string Icon => "basic:trashcan";

    public ExplorerToolTarget Target => ExplorerToolTarget.SelectedContextExplorerObjects;

    public bool IsEnabled(IExplorerApplicationScopeService scope)
    {
        return scope.ContextExplorerObjects?
            .Where(e => e is IExplorerObjectDeletable)
            .Count() > 0;
    }

    async public Task<bool> OnEvent(IExplorerApplicationScopeService scope)
    {
        if (scope.ContextExplorerObjects != null && scope.ContextExplorerObjects.Any())
        {
            var model = new DeleteObjectsModel()
            {
                ExplorerObjects = new List<SelectItemModel<IExplorerObject>>(
                    scope.ContextExplorerObjects
                        .Where(e => e is IExplorerObjectDeletable)
                        .Select(e => new SelectItemModel<IExplorerObject>(e)))
            };


            model = await scope.ShowModalDialog(
                typeof(Razor.Components.Dialogs.DeleteObjectsDialog),
                this.Name,
                model);

            if (model != null)
            {
                foreach (var exObject in model.SelectedExplorerItems)
                {
                    await (exObject as IExplorerObjectDeletable)!
                                 .DeleteExplorerObject(new ExplorerObjectEventArgs());
                }
            }

            await scope.ForceContentRefresh();
        }

        return true;
    }

    #region IDisposable

    public void Dispose()
    {

    }

    #endregion

    #region IOrder

    public int SortOrder => 25;

    #endregion
}
