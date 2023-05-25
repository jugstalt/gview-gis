using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataSources.EventTable;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Framework.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.EventTable;

[gView.Framework.system.RegisterPlugIn("653DA8F8-D321-4701-861F-23317E9FEC4D")]
public class EventTableGroupObject : ExplorerParentObject, IExplorerGroupObject
{
    public EventTableGroupObject()
        : base(null, null, 0)
    {
    }

    #region IExplorerObject Member

    public string Name => "Eventtable Connections";

    public string FullName => "EventTableConnections";

    public string Type => "Eventtable Connections";

    public string Icon => "basic:binoculars";

    public Task<object?> GetInstanceAsync() => Task.FromResult<object?>(null);

    #endregion

    #region ISerializableExplorerObject Member

    public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
    {
        if (cache.Contains(FullName))
        {
            return Task.FromResult<IExplorerObject?>(cache[FullName]);
        }

        if (this.FullName == FullName)
        {
            EventTableGroupObject exObject = new EventTableGroupObject();
            cache.Append(exObject);
            return Task.FromResult<IExplorerObject?>(exObject);
        }

        return Task.FromResult<IExplorerObject?>(null);
    }

    #endregion

    #region IExplorerParentObject Members
    async public override Task<bool> Refresh()
    {
        await base.Refresh();

        base.AddChildObject(new EventTableNewConnectionObject(this));

        ConfigConnections conStream = new ConfigConnections("eventtable", "546B0513-D71D-4490-9E27-94CD5D72C64A");
        Dictionary<string, string> DbConnectionStrings = conStream.Connections;
        foreach (string DbConnName in DbConnectionStrings.Keys)
        {
            EventTableConnection dbConn = new EventTableConnection();
            dbConn.FromXmlString(DbConnectionStrings[DbConnName]);
            
            // ToDo:
            //base.AddChildObject(new EventTableObject(this, DbConnName, dbConn));
        }

        return true;
    }
    #endregion

    #region IOgcGroupExplorerObject

    public void SetParentExplorerObject(IExplorerObject parentExplorerObject)
    {
        base._parent = parentExplorerObject;
    }

    #endregion
}

[gView.Framework.system.RegisterPlugIn("F45B7E98-B20A-47bf-A45D-E78D52F36314")]
public class EventTableNewConnectionObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDoubleClick, IExplorerObjectCreatable
{
    public EventTableNewConnectionObject()
        : base(null, null, 1)
    {
    }

    public EventTableNewConnectionObject(IExplorerObject parent)
        : base(parent, null, 1)
    {
    }

    #region IExplorerSimpleObject Members

    public string Icon => "basic:edit-table";

    #endregion

    #region IExplorerObject Members

    public string Name => "New Connection...";

    public string FullName => string.Empty;

    public string Type => "New Eventtable Connection"; 

    public void Dispose()
    {

    }

    public Task<object?> GetInstanceAsync() => Task.FromResult<object>(null);

    #endregion

    #region IExplorerObjectDoubleClick Members

    async public Task ExplorerObjectDoubleClick(IApplicationScope appScope, ExplorerObjectEventArgs e)
    {
        // ToDo:
        //FormEventTableConnection dlg = new FormEventTableConnection();

        //if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //{
        //    ConfigConnections connStream = new ConfigConnections("eventtable", "546B0513-D71D-4490-9E27-94CD5D72C64A");

        //    EventTableConnection etconn = new EventTableConnection(
        //        dlg.DbConnectionString,
        //        dlg.TableName,
        //        dlg.IdField, dlg.XField, dlg.YField,
        //        dlg.SpatialReference);

        //    string id = connStream.GetName(dlg.TableName);
        //    connStream.Add(id, etconn.ToXmlString());

        //    e.NewExplorerObject = new EventTableObject(this.ParentExplorerObject, id, etconn);
        //}
    }

    #endregion

    #region ISerializableExplorerObject Member

    public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
    {
        if (cache.Contains(FullName))
        {
            return Task.FromResult<IExplorerObject?>(cache[FullName]);
        }

        return Task.FromResult<IExplorerObject?>(null);
    }

    #endregion

    #region IExplorerObjectCreatable Member

    public bool CanCreate(IExplorerObject parentExObject)
    {
        return (parentExObject is EventTableGroupObject);
    }

    async public Task<IExplorerObject?> CreateExplorerObjectAsync(IApplicationScope appScope, IExplorerObject parentExObject)
    {
        ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();

        await ExplorerObjectDoubleClick(appScope, e);
        return e.NewExplorerObject;
    }

    #endregion
}

//[gView.Framework.system.RegisterPlugIn("B498C801-D7F2-4e1d-AA09-7A3599549DD8")]
//public class EventTableObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectContextMenu, IExplorerObjectDeletable, IExplorerObjectRenamable
//{
//    private EventTableConnection _etconn = null;
//    private EventTableIcon _icon = new EventTableIcon();
//    private ToolStripItem[] _contextItems = null;
//    private string _name = String.Empty;
//    private IFeatureClass _fc = null;

//    public EventTableObject() : base(null, typeof(IFeatureClass), 1) { }
//    public EventTableObject(IExplorerObject parent, string name, EventTableConnection etconn)
//        : base(parent, typeof(IFeatureClass), 1)
//    {
//        _name = name;
//        _etconn = etconn;

//        List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();
//        ToolStripMenuItem item = new ToolStripMenuItem(LocalizedResources.GetResString("Menu.ConnectionProperties", "Connection Properties..."));
//        item.Click += new EventHandler(ConnectionProperties_Click);
//        items.Add(item);

//        _contextItems = items.ToArray();
//    }

//    void ConnectionProperties_Click(object sender, EventArgs e)
//    {
//        if (_etconn == null)
//        {
//            return;
//        }

//        FormEventTableConnection dlg = new FormEventTableConnection();
//        dlg.DbConnectionString = _etconn.DbConnectionString;
//        dlg.TableName = _etconn.TableName;
//        dlg.IdField = _etconn.IdFieldName;
//        dlg.XField = _etconn.XFieldName;
//        dlg.YField = _etconn.YFieldName;
//        dlg.SpatialReference = _etconn.SpatialReference;

//        if (dlg.ShowDialog() == DialogResult.OK)
//        {
//            EventTableConnection etcon = new EventTableConnection(
//                dlg.DbConnectionString,
//                dlg.TableName,
//                dlg.IdField, dlg.XField, dlg.YField,
//                dlg.SpatialReference);

//            ConfigConnections connStream = new ConfigConnections("eventtable", "546B0513-D71D-4490-9E27-94CD5D72C64A");
//            connStream.Add(dlg.TableName, etcon.ToXmlString());
//            _etconn = etcon;
//        }
//    }

//    #region IExplorerObject Member

//    public string Name
//    {
//        get
//        {
//            if (_etconn == null)
//            {
//                return "???";
//            }

//            return _name;
//        }
//    }

//    public string FullName
//    {
//        get
//        {
//            if (_etconn == null)
//            {
//                return "???";
//            }

//            return @"EventTableConnections\" + _name;
//        }
//    }

//    public string Type
//    {
//        get { return "Database Event Table"; ; }
//    }

//    public IExplorerIcon Icon
//    {
//        get { return _icon; }
//    }

//    async public Task<object> GetInstanceAsync()
//    {
//        if (_fc != null)
//        {
//            return _fc;
//        }

//        if (_etconn != null)
//        {
//            try
//            {
//                Dataset ds = new Dataset();
//                await ds.SetConnectionString(_etconn.ToXmlString());
//                await ds.Open();
//                _fc = (await ds.Elements())[0].Class as IFeatureClass;
//                return _fc;
//            }
//            catch
//            {
//                _fc = null;
//            }
//        }

//        return null;
//    }

//    #endregion

//    #region IDisposable Member

//    public void Dispose()
//    {

//    }

//    #endregion

//    #region ISerializableExplorerObject Member

//    async public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
//    {
//        if (cache.Contains(FullName))
//        {
//            return cache[FullName];
//        }

//        EventTableGroupObject group = new EventTableGroupObject();
//        if (FullName.StartsWith(group.FullName))
//        {
//            foreach (IExplorerObject exObject in await group.ChildObjects())
//            {
//                if (exObject.FullName == FullName)
//                {
//                    cache.Append(exObject);
//                    return exObject;
//                }
//            }
//        }

//        return null;
//    }

//    #endregion

//    #region IExplorerObjectContextMenu Member

//    public System.Windows.Forms.ToolStripItem[] ContextMenuItems
//    {
//        get { return _contextItems; }
//    }

//    #endregion

//    #region IExplorerObjectDeletable Member

//    public event ExplorerObjectDeletedEvent ExplorerObjectDeleted = null;

//    public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
//    {
//        ConfigConnections stream = new ConfigConnections("eventtable", "546B0513-D71D-4490-9E27-94CD5D72C64A");
//        bool ret = stream.Remove(_name);

//        if (ret)
//        {
//            if (ExplorerObjectDeleted != null)
//            {
//                ExplorerObjectDeleted(this);
//            }
//        }
//        return Task.FromResult(ret);
//    }

//    #endregion

//    #region IExplorerObjectRenamable Member

//    public event ExplorerObjectRenamedEvent ExplorerObjectRenamed;

//    public Task<bool> RenameExplorerObject(string newName)
//    {
//        ConfigConnections stream = new ConfigConnections("eventtable", "546B0513-D71D-4490-9E27-94CD5D72C64A");
//        bool ret = stream.Rename(_name, newName);

//        if (ret == true)
//        {
//            _name = newName;
//            if (ExplorerObjectRenamed != null)
//            {
//                ExplorerObjectRenamed(this);
//            }
//        }
//        return Task.FromResult(ret);
//    }

//    #endregion
//}