using gView.DataExplorer.Plugins.Extensions;
using gView.Framework.DataExplorer.Abstraction;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerTools;

[gView.Framework.system.RegisterPlugIn("54597B19-B37A-4fa2-BA45-8B4137F2910E")]
public class Refresh : IExplorerTool
{
    #region IExplorerTool

    public string Name => "Refresh";

    public bool Enabled => true;

    public string ToolTip => "";

    public string Icon => "basic:refresh";

    async public Task<bool> OnEvent(IExplorerApplicationScope scope)
    {
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
