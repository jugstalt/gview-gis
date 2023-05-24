using gView.DataExplorer.Core.Services.Abstraction;
using gView.DataExplorer.Plugins.ExplorerObjects;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.system;
using System;
using System.Threading.Tasks;

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
