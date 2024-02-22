using gView.Framework.Core.IO;
using gView.Framework.DataExplorer.Events;
using gView.Framework.IO;
using System.Threading.Tasks;

namespace gView.Framework.DataExplorer.Abstraction;

public interface IExplorerObjectDeletable
{
    event ExplorerObjectDeletedEvent ExplorerObjectDeleted;
    Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e);
}

public interface IExplorerObjectAccessability
{
    ConfigAccessability Accessability { get; set; }
}
