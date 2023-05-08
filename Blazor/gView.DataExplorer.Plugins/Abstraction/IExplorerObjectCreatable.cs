using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.Abstraction;

public interface IExplorerObjectCreatable
{
    bool CanCreate(IExplorerObject parentExObject);
    Task<IExplorerObject> CreateExplorerObject(IExplorerObject parentExObject);
}