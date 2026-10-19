using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataExplorer.Plugins.ExplorerObjects.RasterNet.Cog.ContextTools;
using gView.DataSources.RasterNet;
using gView.Framework.Core.Data;
using gView.Framework.Core.IO;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Framework.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.RasterNet.Cog;

/// <summary>
/// One stored COG connection: a raster read by RasterNet over HTTP(S). Added to a map, it is a RasterNet layer whose
/// dataset stores the URL like a file name, so map services read it the same way.
/// </summary>
public class CogConnectionExplorerObject : ExplorerObjectCls<CogGroupExplorerObject, RasterClass>,
                                           IExplorerSimpleObject,
                                           IExplorerObjectDeletable,
                                           IExplorerObjectRenamable,
                                           IExplorerObjectContextTools,
                                           IExplorerObjectAccessability,
                                           IExplorerObjectCustomContentValues
{
    private string _name = "";
    private string _url = "";
    private IRasterClass? _class;
    private readonly IEnumerable<IExplorerObjectContextTool> _contextTools = new IExplorerObjectContextTool[] { new UpdateCogUrl() };

    public CogConnectionExplorerObject() : base() { }

    public CogConnectionExplorerObject(CogGroupExplorerObject parent, string name, string url)
        : base(parent, 1)
    {
        _name = name;
        _url = url;
    }

    internal string Url => _url;

    internal void UpdateUrl(string url)
    {
        CogConnections.Open(this).Add(_name, url);
        _url = url;
        _class = null;
    }

    #region IExplorerObject Members

    public string Name => _name;

    public string FullName => @$"{this.Parent.FullName}\{_name}";

    public string Type => "Cloud Optimized GeoTIFF";

    public string Icon => "webgis:georef-image";

    public void Dispose() => _class = null;

    public Task<object?> GetInstanceAsync()
    {
        if (_class == null)
        {
            // AddRasterFile returns null if the URL cannot be read (the error is in the dataset's LastErrorMessage).
            var dataset = new Dataset();
            _class = dataset.AddRasterFile(_url)?.Class as IRasterClass;
            if (_class == null)
            {
                throw new Exception(dataset.LastErrorMessage);
            }
        }
        return Task.FromResult<object?>(_class);
    }

    #endregion

    #region IExplorerObjectCustomContentValues

    public IDictionary<string, object?> GetCustomContentValues()
        => new Dictionary<string, object?> { ["Url"] = _url.Split('?')[0] };

    #endregion

    #region IExplorerObjectContextTools

    public IEnumerable<IExplorerObjectContextTool> ContextTools => _contextTools;

    #endregion

    #region ISerializableExplorerObject Member

    async public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
    {
        if (cache?.Contains(FullName) == true)
        {
            return cache[FullName];
        }

        var group = new CogGroupExplorerObject();
        if (FullName.IndexOf(group.FullName) != 0 || FullName.Length < group.FullName.Length + 2)
        {
            return null;
        }
        group = (CogGroupExplorerObject?)(cache?.Contains(group.FullName) == true ? cache[group.FullName] : group);
        if (group != null)
        {
            foreach (var exObject in await group.ChildObjects())
            {
                if (exObject.FullName == FullName)
                {
                    cache?.Append(exObject);
                    return exObject;
                }
            }
        }
        return null;
    }

    #endregion

    #region IExplorerObjectDeletable Member

    public event ExplorerObjectDeletedEvent? ExplorerObjectDeleted;

    public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
    {
        bool deleted = CogConnections.Open(this).Remove(_name);
        if (deleted)
        {
            ExplorerObjectDeleted?.Invoke(this);
        }
        return Task.FromResult(deleted);
    }

    #endregion

    #region IExplorerObjectRenamable Member

    public event ExplorerObjectRenamedEvent? ExplorerObjectRenamed;

    public Task<bool> RenameExplorerObject(string newName)
    {
        bool renamed = CogConnections.Open(this).Rename(_name, newName);
        if (renamed)
        {
            _name = newName;
            ExplorerObjectRenamed?.Invoke(this);
        }
        return Task.FromResult(renamed);
    }

    #endregion

    #region IExplorerObjectAccessability

    public ConfigAccessability Accessability
    {
        get => CogConnections.Open(this).GetAccessability(_name);
        set => CogConnections.Open(this).SetAccessability(_name, value);
    }

    #endregion
}
