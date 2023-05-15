using System.Threading.Tasks;

namespace gView.Framework.DataExplorer.Abstraction;

public interface IExplorerObjectCreatable : IExplorerObject
{
    bool CanCreate(IExplorerObject parentExObject);
    Task<IExplorerObject?> CreateExplorerObjectAsync(IExplorerApplicationScope scope,
                                                IExplorerObject parentExObject);
}