using gView.DataExplorer.Plugins.Events;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.Abstraction;

public interface IExplorerObjectRenamable
{
    event ExplorerObjectRenamedEvent ExplorerObjectRenamed;
    Task<bool> RenameExplorerObject(string newName);
}
