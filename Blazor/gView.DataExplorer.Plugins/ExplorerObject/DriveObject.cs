using gView.DataExplorer.Plugins.Abstraction;
using gView.DataExplorer.Plugins.ExplorerObject.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObject;

public class DriveObject : ExplorerParentObject, IExplorerObject
{
    private string _drive = "";
    private int _imageIndex = 0;
    private string _type = "";

    public DriveObject(IExplorerObject parent, string drive, uint type)
        : base(parent, null, -1)
    {
        _drive = drive;
        switch (type)
        {
            case 2:
                _imageIndex = 7;
                _type = "Floppy Disk (" + _drive + ")";
                break;
            case 5:
                _imageIndex = 4;
                _type = "CD-ROM Drive (" + _drive + ")";
                break;
            case 4:
                _imageIndex = 5;
                _type = "Mapped Drive (" + _drive + ")";
                break;
            case 999:
                _imageIndex = 5;
                _type = drive;
                break;
            default:
                _imageIndex = 6;
                _type = "Local Drive (" + _drive + ")";
                break;
        }
    }

    public int ImageIndex { get { return _imageIndex; } }

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

    public IExplorerIcon Icon
    {
        get
        {
            return new ObjectIcon(_imageIndex);
        }
    }

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
