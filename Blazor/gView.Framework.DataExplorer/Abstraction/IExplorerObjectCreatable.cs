using System.Threading.Tasks;

namespace gView.Framework.DataExplorer.Abstraction;

public interface IExplorerObjectCreatable
{
    bool CanCreate(IExplorerObject parentExObject);
    Task<IExplorerObject> CreateExplorerObject(IExplorerObject parentExObject);
}