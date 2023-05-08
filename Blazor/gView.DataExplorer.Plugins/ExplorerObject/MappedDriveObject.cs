using gView.DataExplorer.Plugins.Abstraction;
using gView.DataExplorer.Plugins.Events;
using gView.Framework.IO;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObject;

internal class MappedDriveObject : DriveObject, IExplorerObjectDeletable
{
    public MappedDriveObject(IExplorerObject parent, string drive)
        : base(parent, drive, 999)
    {
    }

    #region IExplorerObjectDeletable
    public event ExplorerObjectDeletedEvent ExplorerObjectDeleted;

    public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
    {
        ConfigConnections configStream = new ConfigConnections("directories");
        configStream.Remove(this.Name);

        if (ExplorerObjectDeleted != null)
        {
            ExplorerObjectDeleted(this);
        }

        return Task.FromResult(true);
    }
    #endregion
}
