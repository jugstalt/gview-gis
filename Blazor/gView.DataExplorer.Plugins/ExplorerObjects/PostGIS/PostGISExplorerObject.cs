using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataSources.PostGIS;
using gView.Framework.Data;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Framework.Db;
using gView.Framework.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.PostGIS;

public class PostGISExplorerObject : ExplorerParentObject, 
                                     IExplorerSimpleObject, 
                                     IExplorerObjectDeletable, 
                                     IExplorerObjectRenamable, 
                                     ISerializableExplorerObject 
                                     //,IExplorerObjectContextMenu
{
    private string _server = "";
    private IFeatureDataset? _dataset;
    private DbConnectionString? _connectionString;
    //private ToolStripItem[] _contextItems = null;

    public PostGISExplorerObject() : base(null, typeof(IFeatureDataset), 0) { }
    public PostGISExplorerObject(IExplorerObject parent, string server, DbConnectionString connectionString)
        : base(parent, typeof(IFeatureDataset), 0)
    {
        _server = server;
        _connectionString = connectionString;

        // ToDo:
        //List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();
        //ToolStripMenuItem item = new ToolStripMenuItem(LocalizedResources.GetResString("Menu.ConnectionProperties", "Connection Properties..."));
        //item.Click += new EventHandler(ConnectionProperties_Click);
        //items.Add(item);

        //_contextItems = items.ToArray();
    }

    //void ConnectionProperties_Click(object sender, EventArgs e)
    //{
    //    if (_connectionString == null)
    //    {
    //        return;
    //    }

    //    FormConnectionString dlg = new FormConnectionString(_connectionString);
    //    dlg.ProviderID = "postgre";
    //    dlg.UseProviderInConnectionString = false;

    //    if (dlg.ShowDialog() == DialogResult.OK)
    //    {
    //        DbConnectionString dbConnStr = dlg.DbConnectionString;

    //        ConfigConnections connStream = new ConfigConnections("postgis", "546B0513-D71D-4490-9E27-94CD5D72C64A");
    //        connStream.Add(_server, dbConnStr.ToString());

    //        _connectionString = dbConnStr;
    //    }
    //}

    internal string ConnectionString => _connectionString.ConnectionString;


    #region IExplorerObject Members

    public string Name => _server;

    public string FullName => @"OGC\PostGIS\" + _server;

    public string Type=>"PostGIS Database";

    public string Icon => "basic:database";

    async public Task<object?> GetInstanceAsync()
    {
        if (_dataset != null)
        {
            _dataset.Dispose();
        }

        _dataset = new PostGISDataset();
        await _dataset.SetConnectionString(_connectionString.ConnectionString);
        await _dataset.Open();
        return _dataset;
    }

    #endregion

    #region IExplorerParentObject Members

    async public override Task<bool> Refresh()
    {
        await base.Refresh();
        if (_connectionString == null)
        {
            return false;
        }

        PostGISDataset dataset = new PostGISDataset();
        await dataset.SetConnectionString(_connectionString.ConnectionString);
        await dataset.Open();

        List<IDatasetElement> elements = await dataset.Elements();

        if (elements == null)
        {
            return false;
        }

        foreach (IDatasetElement element in elements)
        {
            if (element.Class is IFeatureClass)
            {
                base.AddChildObject(new PostGISFeatureClassExplorerObject(this, element));
            }
        }

        return true;
    }

    #endregion

    #region ISerializableExplorerObject Member

    async public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
    {
        if (cache.Contains(FullName))
        {
            return cache[FullName];
        }

        PostGISExplorerGroupObject? group = new PostGISExplorerGroupObject();
        if (FullName.IndexOf(group.FullName) != 0 || FullName.Length < group.FullName.Length + 2)
        {
            return null;
        }

        group = (PostGISExplorerGroupObject?)((cache.Contains(group.FullName)) ? cache[group.FullName] : group);

        if (group != null)
        {
            foreach (IExplorerObject exObject in await group.ChildObjects())
            {
                if (exObject.FullName == FullName)
                {
                    cache.Append(exObject);
                    return exObject;
                }
            }
        }

        return null;
    }

    #endregion

    #region IExplorerObjectDeletable Member

    public event ExplorerObjectDeletedEvent? ExplorerObjectDeleted = null;

    public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
    {
        bool ret = false;
        if (_connectionString != null)
        {
            ConfigConnections stream = new ConfigConnections("postgis", "546B0513-D71D-4490-9E27-94CD5D72C64A");
            ret = stream.Remove(_server);
        }

        if (ret && ExplorerObjectDeleted != null)
        {
            ExplorerObjectDeleted(this);
        }

        return Task.FromResult(ret);
    }

    #endregion

    #region IExplorerObjectRenamable Member

    public event ExplorerObjectRenamedEvent? ExplorerObjectRenamed = null;

    public Task<bool> RenameExplorerObject(string newName)
    {
        bool ret = false;
        if (_connectionString != null)
        {
            ConfigConnections stream = new ConfigConnections("postgis", "546B0513-D71D-4490-9E27-94CD5D72C64A");
            ret = stream.Rename(_server, newName);
        }
        if (ret == true)
        {
            _server = newName;
            if (ExplorerObjectRenamed != null)
            {
                ExplorerObjectRenamed(this);
            }
        }
        return Task.FromResult(ret);
    }

    #endregion

    //#region IExplorerObjectContextMenu Member

    //public ToolStripItem[] ContextMenuItems
    //{
    //    get { return _contextItems; }
    //}

    //#endregion
}
