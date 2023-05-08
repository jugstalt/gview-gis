using gView.DataExplorer.Plugins.Events;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.Abstraction;

public interface IExplorerObjectDeletable
{
    event ExplorerObjectDeletedEvent ExplorerObjectDeleted;
    Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e);
}
