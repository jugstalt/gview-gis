using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataExplorer.Plugins.ExplorerObjects.Web.Wms.ContextTools;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Framework.IO;
using gView.Interoperability.OGC.Dataset.WMS;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Web.Wms;

public class WmsServiceExplorerObject : ExplorerObjectCls, 
                                        IExplorerSimpleObject, 
                                        IExplorerObjectDeletable, 
                                        IExplorerObjectRenamable,
                                        IExplorerObjectContextTools
{
    private string _icon = "webgis:globe";
    private string _name = "", _connectionString = "";
    private WMSClass? _class = null;
    new private IExplorerParentObject? _parent;
    private string _type = "OGC WMS Service";
    private IEnumerable<IExplorerObjectContextTool>? _contextTools = null;

    internal WmsServiceExplorerObject(IExplorerParentObject parent, string name, string connectionString)
        : base(parent as IExplorerObject, typeof(WMSClass), 1)
    {
        _name = name;
        _connectionString = connectionString;

        switch (ConfigTextStream.ExtractValue(_connectionString, "service").ToUpper())
        {
            case "WMS":
                _type = "OGC WMS Service";
                _icon = "basic:globe";
                break;
            case "WFS":
                _type = "OGC WFS Service";
                _icon = "basic:globe";
                break;
            case "WMS_WFS":
                _type = "OGC WMS/WFS Service";
                _icon = "basic:globe";
                break;
            default:
                _type = "Unknown OGC Service Type!!";
                break;
        }
        _parent = parent;

        _contextTools = new IExplorerObjectContextTool[]
        {
            new UpdateConnectionString()
        };
    }

    internal string GetConnectionString() => _connectionString;

    internal Task<bool> UpdateConnectionString(string connectionString)
    {
        ConfigConnections connStream = new ConfigConnections("ogc_web_connection", "546B0513-D71D-4490-9E27-94CD5D72C64A");
        connStream.Add(_name, connectionString);

        _connectionString = connectionString;

        return Task.FromResult(true);
    }

    #region IExplorerObject Member

    public string Name => _name;
    public string FullName
    {
        get
        {
            if (!(_parent is IExplorerObject))
            {
                return "";
            }

            return @$"{((IExplorerObject)_parent).FullName}\{_name}";
        }
    }

    public string Type=> _type;

    public string Icon => _icon;

    public void Dispose()
    {

    }

    async public Task<object?> GetInstanceAsync()
    {
        if (_class == null)
        {
            WMSDataset dataset = await WMSDataset.Create(_connectionString, _name);
            //dataset.Open(); // kein open, weil sonst ein GET_SERVICE_INFO durchgeführt wird...
            switch (ConfigTextStream.ExtractValue(_connectionString, "service").ToUpper())
            {
                case "WMS":
                case "WMS_WFS":
                    if ((await dataset.Elements()).Count == 0)
                    {
                        dataset.Dispose();
                        return null;
                    }
                    return _class = (await dataset.Elements()).First().Class as WMSClass;
                case "WFS":
                    return dataset;
                default:
                    return null;
            }
        }

        return _class;
    }

    #endregion

    #region IExplorerObjectContextTools

    public IEnumerable<IExplorerObjectContextTool> ContextTools => _contextTools ?? Array.Empty<IExplorerObjectContextTool>();

    #endregion

    #region ISerializableExplorerObject Member

    async public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
    {
        if (cache.Contains(FullName))
        {
            return cache[FullName];
        }

        FullName = FullName.Replace("/", @"\");
        int lastIndex = FullName.LastIndexOf(@"\");
        if (lastIndex == -1)
        {
            return null;
        }

        string cnName = FullName.Substring(0, lastIndex);
        string svName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

        WmsExplorerObject? cnObject = new WmsExplorerObject(this);
        cnObject = await cnObject.CreateInstanceByFullName(cnName, cache) as WmsExplorerObject;
        if (cnObject == null || await cnObject.ChildObjects() == null)
        {
            return null;
        }

        foreach (IExplorerObject exObject in await cnObject.ChildObjects())
        {
            if (exObject.Name == svName)
            {
                cache.Append(exObject);
                return exObject;
            }
        }
        return null;
    }

    #endregion

    #region IExplorerObjectDeletable Member

    public event ExplorerObjectDeletedEvent? ExplorerObjectDeleted;

    public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
    {
        ConfigTextStream stream = new ConfigTextStream("ogc_connections", true, true);

        stream.Remove(this.Name, _connectionString);
        stream.Close();
        
        if (ExplorerObjectDeleted != null)
        {
            ExplorerObjectDeleted(this);
        }

        return Task.FromResult(true);
    }

    #endregion

    #region IExplorerObjectRenamable Member

    public event ExplorerObjectRenamedEvent? ExplorerObjectRenamed;

    public Task<bool> RenameExplorerObject(string newName)
    {
        var result = false;
        ConfigTextStream stream = new ConfigTextStream("ogc_connections", true, true);
        
        result = stream.ReplaceHoleLine(ConfigTextStream.BuildLine(this.Name, _connectionString), ConfigTextStream.BuildLine(newName, _connectionString));
        stream.Close();

        if (result == true)
        {
            _name = newName;
            if (ExplorerObjectRenamed != null)
            {
                ExplorerObjectRenamed(this);
            }
        }
        return Task.FromResult(result);
    }

    #endregion
}
