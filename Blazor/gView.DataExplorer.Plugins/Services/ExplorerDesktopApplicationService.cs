using gView.DataExplorer.Core.Services.Abstraction;
using gView.DataExplorer.Plugins.ExplorerObject;
using gView.Framework.DataExplorer.Abstraction;

namespace gView.DataExplorer.Plugins.Services;
internal class ExplorerDesktopApplicationService : IExplorerApplicationService
{
    private readonly IExplorerObject _rootExplorerObject;

    public ExplorerDesktopApplicationService()
    {
        _rootExplorerObject = new ComputerObject();
    }

    public IExplorerObject RootExplorerObject => _rootExplorerObject;
}
