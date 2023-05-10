using gView.Framework.DataExplorer.Abstraction;
using gView.DataExplorer.Plugins.ExplorerObject.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObject;

public class DriveObject : ExplorerParentObject, IExplorerObject
{
    private readonly string _drive = "";
    private readonly string _icon;
    private readonly string _type = "";

    public DriveObject(IExplorerObject parent, string drive, uint type)
        : base(parent, null, -1)
    {
        _drive = drive;
        switch (type)
        {
            case 2:
                _icon = "basic:folder";
                _type = $"Floppy Disk ({drive})";
                break;
            case 5:
                _icon = "basic:circle-pie-25";
                _type = $"CD-ROM Drive ({_drive})";
                break;
            case 4:
                _icon = "basic:open-in-window";
                _type = $"Mapped Drive ({_drive})";
                break;
            case 999:
                _icon = "basic:folder";
                _type = drive;
                break;
            default:
                _icon = "basic:folder";
                _type = $"Local Drive ({_drive})";
                break;
        }
    }

    #region IExplorerObject Members

    public string Filter
    {
        get { return ""; }
    }

    public string Name
    {
        get
        {
            return _type;
        }
    }

    public string FullName
    {
        get { return _drive + @"\"; }
    }

    public string Type
    {
        get { return _type; }
    }

    public string Icon => _icon;

    public Task<object> GetInstanceAsync()
    {
        return Task.FromResult<object>(null);
    }

    public IExplorerObject CreateInstanceByFullName(string FullName)
    {
        return null;
    }

    #endregion

    #region IExplorerParentObject Member

    async public override Task<bool> Refresh()
    {
        await base.Refresh();
        List<IExplorerObject> childs = await DirectoryObject.Refresh(this, this.FullName);
        if (childs == null)
        {
            return false;
        }

        foreach (IExplorerObject child in childs)
        {
            base.AddChildObject(child);
        }

        return true;
    }

    #endregion

    #region ISerializableExplorerObject Member

    public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
    {
        if (cache.Contains(FullName))
        {
            return Task.FromResult<IExplorerObject>(cache[FullName]);
        }

        return Task.FromResult<IExplorerObject>(null);
    }

    #endregion
}
