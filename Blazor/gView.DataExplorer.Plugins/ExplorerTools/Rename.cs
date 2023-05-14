using gView.DataExplorer.Plugins.Extensions;
using gView.Framework.DataExplorer;
using gView.Framework.DataExplorer.Abstraction;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerTools;

[gView.Framework.system.RegisterPlugIn("91F17A8E-D859-4840-A287-7FDE14152CB1")]
public class Rename : IExplorerTool
{
    #region IExplorerTool

    public string Name => "Rename";

    public string ToolTip => "";

    public string Icon => "basic:edit";

    public ExplorerToolTarget Target => ExplorerToolTarget.SelectedContextExplorerObjects;

    public bool IsEnabled(IExplorerApplicationScope scope)
    {
        var scopeService = scope.ToScopeService();

        return scopeService.ContextExplorerObjects?
            .Where(e => e is IExplorerObjectRenamable)
            .Count() > 0;
    }

    async public Task<bool> OnEvent(IExplorerApplicationScope scope)
    {
        await scope.ToScopeService().EventBus.FireFreshContentAsync();

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
