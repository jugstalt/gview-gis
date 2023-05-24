using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.DataExplorer.Abstraction;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.ContextTools;
public class UpdateConnectionString : IExplorerObjectContextTool
{
    #region IExplorerObjectContextTool

    public string Name => "Connection String";

    public string Icon => "basic:edit-database";

    public bool IsEnabled(IApplicationScope scope, IExplorerObject exObject)
    {
        return true;
    }

    public Task<bool> OnEvent(IApplicationScope scope, IExplorerObject exObject)
    {
        return Task.FromResult(true);
    }

    #endregion
}
