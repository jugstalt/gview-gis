using gView.Framework.DataExplorer.Services.Abstraction;
using System.Threading.Tasks;

namespace gView.Framework.DataExplorer.Abstraction;

public interface IExplorerObjectCreatable : IExplorerObject
{
    bool CanCreate(IExplorerObject parentExObject);
    Task<IExplorerObject?> CreateExplorerObjectAsync(IExplorerApplicationScopeService scope,
                                                     IExplorerObject parentExObject);
}
