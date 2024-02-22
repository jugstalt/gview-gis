using gView.DataExplorer.Core.Extensions;
using gView.Framework.Core.IO;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Framework.IO;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.FileSystem;

internal class MappedDriveObject : DriveObject,
                                   IExplorerObjectAccessability,
                                   IExplorerObjectDeletable
{
    private string _drive;

    public MappedDriveObject(IExplorerObject parent, string drive)
        : base(parent, drive, 999)
    {
        _drive = drive;
    }

    #region IExplorerObjectDeletable

    public event ExplorerObjectDeletedEvent? ExplorerObjectDeleted;

    public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
    {
        var configStream = GetConfigConnections();
        configStream.Remove(_drive);

        if (ExplorerObjectDeleted != null)
        {
            ExplorerObjectDeleted(this);
        }

        return Task.FromResult(true);
    }

    #endregion

    #region IExplorerObjectAccessability

    public ConfigAccessability Accessability
    {
        get => GetConfigConnections().GetAccessability(_drive);
        set => GetConfigConnections().SetAccessability(_drive, value);
    }

    #endregion

    #region Helper

    private ConfigConnections GetConfigConnections()
        => ConfigConnections.Create(this.ConfigStorage(), "directories");

    #endregion
}
