using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Framework.DataExplorer.Services.Abstraction;
using System.Threading.Tasks;

namespace gView.Framework.DataExplorer.Abstraction;

public interface IExplorerObjectDoubleClick
{
    Task ExplorerObjectDoubleClick(IExplorerApplicationScopeService appScope, ExplorerObjectEventArgs e);
}
