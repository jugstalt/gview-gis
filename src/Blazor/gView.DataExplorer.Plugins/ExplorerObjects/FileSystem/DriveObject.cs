using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.Framework.DataExplorer.Abstraction;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.FileSystem;

public class DriveObject : ExplorerParentObject<IExplorerObject>,
                           IExplorerObject
{
    private readonly string _path = "";
    private readonly string _icon;
    private readonly string _type = "";
    private readonly string _name = "";

    public DriveObject(IExplorerObject parent, string name, string path, uint type)
        : base(parent, -1)
    {
        _path = path;
        switch (type)
        {
            case 2:
                _icon = "basic:folder";
                _name = _type = $"Floppy Disk ({name})";
                break;
            case 5:
                _icon = "basic:circle-pie-25";
                _name = _type = $"CD-ROM Drive ({name})";
                break;
            case 4:
                _icon = "basic:open-in-window";
                _name = _type = $"Mapped Folder: ({name})";
                break;
            case 998:
                _icon = "basic:open-in-window";
                _name = name;
                _type = $"Environment Variable: {_name}";
                break;
            case 999:
                _icon = "basic:open-in-window";
                _name = _path.Replace(@"\", "/").Split('/').Last();
                _type = $"Mapped Drive: {_path}";
                break;
            default:
                _icon = "basic:folder";
                _name = _type = $"Local Drive ({name})";
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
            return _name;
        }
    }

    public string FullName
    {
        get { return _path; }
    }

    public string? Type
    {
        get { return _type; }
    }

    public string Icon => _icon;

    public Task<object?> GetInstanceAsync()
    {
        return Task.FromResult<object?>(null);
    }

    public IExplorerObject? CreateInstanceByFullName(string FullName)
    {
        return null;
    }

    #endregion

    #region IExplorerParentObject Member

    async public override Task<bool> Refresh()
    {
        await base.Refresh();
        List<IExplorerObject> childs = await DirectoryObject.Refresh(this, FullName);
        if (childs == null)
        {
            return false;
        }

        foreach (IExplorerObject child in childs)
        {
            AddChildObject(child);
        }

        return true;
    }

    #endregion

    #region ISerializableExplorerObject Member

    public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
    {
        if (cache != null && cache.Contains(FullName))
        {
            return Task.FromResult(cache[FullName]);
        }

        return Task.FromResult<IExplorerObject?>(null);
    }

    #endregion
}
