using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.DataExplorer.Abstraction;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.ContextTools;

internal class ShrinkSpatialIndices : IExplorerObjectContextTool
{
    public string Name => "Shrink spatial indices...";

    public string Icon => "basic:warning_yellow";

    public bool IsEnabled(IApplicationScope scope, IExplorerObject exObject)
    {
        return true;
    }

    public Task<bool> OnEvent(IApplicationScope scope, IExplorerObject exObject)
    {
        return Task.FromResult(true);
    }
}
