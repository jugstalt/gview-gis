using gView.DataExplorer.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Core.Common;
using gView.Framework.DataExplorer;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerTools;

[RegisterPlugIn("91F17A8E-D859-4840-A287-7FDE14152CB1")]
public class Rename : IExplorerTool
{
    #region IExplorerTool

    public string Name => "Rename";

    public string ToolTip => "";

    public string Icon => "basic:edit";

    public ExplorerToolTarget Target => ExplorerToolTarget.SelectedContextExplorerObjects;

    public bool IsEnabled(IExplorerApplicationScopeService scope)
    {
        return scope.ContextExplorerObjects?
            .Where(e => e is IExplorerObjectRenamable)
            .Count() == 1;
    }

    async public Task<bool> OnEvent(IExplorerApplicationScopeService scope)
    {
        var exObject = scope.ContextExplorerObjects?
            .Where(e => e is IExplorerObjectRenamable)
            .FirstOrDefault();

        if (exObject != null)
        {
            var model = new RenameObjectModel() { ExplorerObject = exObject };

            model = await scope.ShowModalDialog(
                   typeof(Razor.Components.Dialogs.RenameObjectDialog),
                   this.Name,
                   model);

            if (model != null)
            {
                await (exObject as IExplorerObjectRenamable)!
                             .RenameExplorerObject(model.NewName);

                await scope.ForceContentRefresh();
            }
        }

        return true;
    }

    #endregion

    #region IOrder

    public int SortOrder => 16;

    #endregion

    #region IDisposable

    public void Dispose()
    {

    }

    #endregion
}
