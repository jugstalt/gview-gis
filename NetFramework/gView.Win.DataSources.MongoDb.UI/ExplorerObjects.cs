using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using gView.Framework.Globalisation;
using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.system.UI;
using gView.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.DataSources.MongoDb.UI
{
    [RegisterPlugIn("E25B4EAB-619C-4F45-8319-6218534A2F6A")]
    public class MongoDbGroupObject : ExplorerParentObject, IExplorerGroupObject
    {
        private MongoDbConnectionsIcon _icon = new MongoDbConnectionsIcon();

        public MongoDbGroupObject()
            : base(null, null, 0)
        {
        }

        #region IExplorerObject Member

        public string Name
        {
            get { return "MongoDb Connections"; }
        }

        public string FullName
        {
            get { return "MongoDbConnections"; }
        }

        public string Type
        {
            get { return "MongoDb Connections"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public Task<object> GetInstanceAsync()
        {
            return Task.FromResult<object>(null);
        }

        #endregion

        #region ISerializableExplorerObject Member

        public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
                return Task.FromResult<IExplorerObject>(cache[FullName]);

            if (this.FullName == FullName)
            {
                MongoDbGroupObject exObject = new MongoDbGroupObject();
                cache.Append(exObject);
                return Task.FromResult<IExplorerObject>(exObject);
            }

            return Task.FromResult<IExplorerObject>(null);
        }

        #endregion

        #region IExplorerParentObject Members

        async public override Task<bool> Refresh()
        {
            await base.Refresh();

            base.AddChildObject(new MongoDbNewConnectionObject(this));

            ConfigConnections conStream = new ConfigConnections("MongoDb", "546B0513-D71D-4490-9E27-94CD5D72C64A");
            Dictionary<string, string> DbConnectionStrings = conStream.Connections;
            foreach (string name in DbConnectionStrings.Keys)
            {
                var connectionString=DbConnectionStrings[name];
                base.AddChildObject(new MongoDbExplorerObject(this, name, connectionString));
            }

            return true;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("1C6FEDF3-5349-4142-935B-66CEDBCE9171")]
    public class MongoDbNewConnectionObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDoubleClick, IExplorerObjectCreatable
    {
        private IExplorerIcon _icon = new EventTableNewConnectionIcon();

        public MongoDbNewConnectionObject()
            : base(null, null, 1)
        {
        }

        public MongoDbNewConnectionObject(IExplorerObject parent)
            : base(parent, null, 1)
        {
        }

        #region IExplorerSimpleObject Members

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        #endregion

        #region IExplorerObject Members

        public string Name
        {
            get { return "New Connection..."; }
        }

        public string FullName
        {
            get { return ""; }
        }

        public string Type
        {
            get { return "New OracleSDO Connection"; }
        }

        public void Dispose()
        {

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

        #region IExplorerObjectDoubleClick Members

        public void ExplorerObjectDoubleClick(ExplorerObjectEventArgs e)
        {
            FormMongoDbConnection dlg = new FormMongoDbConnection();

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string connectionString = dlg.ConnectionString;
                string name = connectionString.ExtractConnectionStringParameter("database");

                ConfigConnections connStream = new ConfigConnections("MongoDb", "546B0513-D71D-4490-9E27-94CD5D72C64A");
                connStream.Add(name, connectionString);

                e.NewExplorerObject = new MongoDbExplorerObject(this.ParentExplorerObject, name, connectionString);
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
                return Task.FromResult<IExplorerObject>(cache[FullName]);

            return Task.FromResult<IExplorerObject>(null);
        }

        #endregion

        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            return (parentExObject is MongoDbGroupObject);
        }

        public Task<IExplorerObject> CreateExplorerObject(IExplorerObject parentExObject)
        {
            ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();
            ExplorerObjectDoubleClick(e);
            return Task.FromResult(e.NewExplorerObject);
        }

        #endregion
    }

    public class MongoDbExplorerObject : gView.Framework.OGC.UI.ExplorerObjectFeatureClassImport, IExplorerSimpleObject, IExplorerObjectDeletable, IExplorerObjectRenamable, ISerializableExplorerObject, IExplorerObjectContextMenu
    {
        private MongoDbConnectionsIcon _icon = new MongoDbConnectionsIcon();
        private string _connectionString = "";
        private string _name = "";
        private IFeatureDataset _dataset;
        private ToolStripItem[] _contextItems = null;

        public MongoDbExplorerObject() : base(null, typeof(IFeatureDataset)) { }
        public MongoDbExplorerObject(IExplorerObject parent, string name, string connectionString)
            : base(parent, typeof(IFeatureDataset))
        {
            _name = name;
            _connectionString = connectionString;

            List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();
            ToolStripMenuItem item = new ToolStripMenuItem(LocalizedResources.GetResString("Menu.ConnectionProperties", "Connection Properties..."));
            item.Click += new EventHandler(ConnectionProperties_Click);
            items.Add(item);

            _contextItems = items.ToArray();
        }

        void ConnectionProperties_Click(object sender, EventArgs e)
        {
            if (_connectionString == null) return;

            FormMongoDbConnection dlg = new FormMongoDbConnection();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string connectionString = dlg.ConnectionString;

                ConfigConnections connStream = new ConfigConnections("MongoDb", "546B0513-D71D-4490-9E27-94CD5D72C64A");
                connStream.Add(_name, connectionString);

                _connectionString = connectionString;
            }
        }

        internal string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        #region IExplorerObject Members

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string FullName
        {
            get
            {
                return @"MongoDb\" + _name;
            }
        }

        public string Type
        {
            get { return "MongoDb Database"; }
        }

        public IExplorerIcon Icon
        {
            get
            {
                return _icon;
            }
        }

        async public Task<object> GetInstanceAsync()
        {
            if (_dataset != null)
                _dataset.Dispose();

            _dataset = new MongoDbDataset();
            await _dataset.SetConnectionString(_connectionString);
            await _dataset.Open();
            return _dataset;
        }

        #endregion

        #region IExplorerParentObject Members

        async public override Task<bool> Refresh()
        {
            await base.Refresh();
            if (_connectionString == null)
                return false;

            var dataset = new MongoDbDataset();
            await dataset.SetConnectionString(_connectionString);
            await dataset.Open();

            List<IDatasetElement> elements = await dataset.Elements();

            if (elements == null)
                return false;

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

        async public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            MongoDbGroupObject group = new MongoDbGroupObject();
            if (FullName.IndexOf(group.FullName) != 0 || FullName.Length < group.FullName.Length + 2) return null;

            group = (MongoDbGroupObject)((cache.Contains(group.FullName)) ? cache[group.FullName] : group);

            foreach (IExplorerObject exObject in await group.ChildObjects())
            {
                if (exObject.FullName == FullName)
                {
                    cache.Append(exObject);
                    return exObject;
                }
            }
            return null;
        }

        #endregion

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted = null;

        public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            //ConfigTextStream stream = new ConfigTextStream("postgis_connections", true, true);
            //stream.Remove(this.Name, _connectionString);
            //stream.Close();
            //if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
            //return true;

            bool ret = false;
            if (_connectionString != null)
            {
                ConfigConnections stream = new ConfigConnections("MongoDb", "546B0513-D71D-4490-9E27-94CD5D72C64A");
                ret = stream.Remove(_name);
            }

            if (ret && ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
            return Task.FromResult(ret);
        }

        #endregion

        #region IExplorerObjectRenamable Member

        public event ExplorerObjectRenamedEvent ExplorerObjectRenamed = null;

        public Task<bool> RenameExplorerObject(string newName)
        {
            //bool ret = false;
            //ConfigTextStream stream = new ConfigTextStream("postgis_connections", true, true);
            //ret = stream.ReplaceHoleLine(ConfigTextStream.BuildLine(_server, _connectionString), ConfigTextStream.BuildLine(newName, _connectionString));
            //stream.Close();

            //if (ret == true)
            //{
            //    _server = newName;
            //    if (ExplorerObjectRenamed != null) ExplorerObjectRenamed(this);
            //}
            //return ret;

            bool ret = false;
            if (_connectionString != null)
            {
                ConfigConnections stream = new ConfigConnections("postgis", "546B0513-D71D-4490-9E27-94CD5D72C64A");
                ret = stream.Rename(_name, newName);
            }
            if (ret == true)
            {
                _name = newName;
                if (ExplorerObjectRenamed != null) ExplorerObjectRenamed(this);
            }
            return Task.FromResult(ret);
        }

        #endregion

        #region IExplorerObjectContextMenu Member

        public ToolStripItem[] ContextMenuItems
        {
            get { return _contextItems; }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("38FF7F06-BF15-4C8B-965A-9FE4503969D1")]
    public class PostGISFeatureClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, ISerializableExplorerObject, IExplorerObjectDeletable
    {
        private string _fcname = "", _type = "";
        private IExplorerIcon _icon = null;
        private IFeatureClass _fc = null;
        private MongoDbExplorerObject _parent = null;

        public PostGISFeatureClassExplorerObject() : base(null, typeof(IFeatureClass), 1) { }
        public PostGISFeatureClassExplorerObject(MongoDbExplorerObject parent, IDatasetElement element)
            : base(parent, typeof(IFeatureClass), 1)
        {
            if (element == null || !(element.Class is IFeatureClass)) return;

            _parent = parent;
            _fcname = element.Title;

            if (element.Class is IFeatureClass)
            {
                _fc = (IFeatureClass)element.Class;
                switch (_fc.GeometryType)
                {
                    case geometryType.Envelope:
                    case geometryType.Polygon:
                        _icon = new MongoDbPolygonIcon();
                        _type = "Polygon Featureclass";
                        break;
                    case geometryType.Multipoint:
                    case geometryType.Point:
                        _icon = new MongoDbPointIcon();
                        _type = "Point Featureclass";
                        break;
                    case geometryType.Polyline:
                        _icon = new MongoDbLineIcon();
                        _type = "Polyline Featureclass";
                        break;
                }
            }
        }

        internal string ConnectionString
        {
            get
            {
                if (_parent == null) return "";
                return _parent.ConnectionString;
            }
        }

        #region IExplorerObject Members

        public string Name
        {
            get { return _fcname; }
        }

        public string FullName
        {
            get
            {
                if (_parent == null) return "";
                return _parent.FullName + @"\" + this.Name;
            }
        }
        public string Type
        {
            get { return _type; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }
        public void Dispose()
        {
            if (_fc != null)
            {
                _fc = null;
            }
        }
        public Task<object> GetInstanceAsync()
        {
            return Task.FromResult<object>(_fc);
        }

        #endregion

        #region ISerializableExplorerObject Member

        async public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
                return cache[FullName];

            FullName = FullName.Replace("/", @"\");
            int lastIndex = FullName.LastIndexOf(@"\");
            if (lastIndex == -1) return null;

            string dsName = FullName.Substring(0, lastIndex);
            string fcName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

            MongoDbExplorerObject dsObject = new MongoDbExplorerObject();
            dsObject = await dsObject.CreateInstanceByFullName(dsName, cache) as MongoDbExplorerObject;

            var childObjects = await dsObject?.ChildObjects();
            if (childObjects == null)
                return null;

            foreach (IExplorerObject exObject in childObjects)
            {
                if (exObject.Name == fcName)
                {
                    cache.Append(exObject);
                    return exObject;
                }
            }
            return null;
        }

        #endregion

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted;

        async public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            var instance = await _parent.GetInstanceAsync();
            if (instance is IFeatureDatabase)
            {
                if (await ((IFeatureDatabase)instance).DeleteFeatureClass(this.Name))
                {
                    if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
                    return true;
                }
                else
                {
                    MessageBox.Show("ERROR: " + ((IFeatureDatabase)instance).LastErrorMessage);
                    return false;
                }
            }
            return false;
        }

        #endregion
    }

    #region Icons

    class MongoDbConnectionsIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("C94DDCE5-2766-468B-8D77-B711C386CFD0");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return global::gView.Win.DataSources.MongoDb.UI.Properties.Resources.mongo_16;
            }
        }

        #endregion
    }
    
    class EventTableNewConnectionIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("6D2304C3-95B0-42F9-88C8-7C66BE960B57");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return global::gView.Win.DataSources.MongoDb.UI.Properties.Resources._new;
            }
        }

        #endregion
    }

    public class MongoDbPointIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("8282A874-7276-49EE-8ABF-B11E119D778F"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Win.DataSources.MongoDb.UI.Properties.Resources.points; }
        }

        #endregion
    }

    public class MongoDbLineIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("F74E8CA3-E36E-4B3D-965E-00F0E9F25E85"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Win.DataSources.MongoDb.UI.Properties.Resources.lines; }
        }

        #endregion
    }

    public class MongoDbPolygonIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("27F5285D-1925-484B-938A-A04917989021"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Win.DataSources.MongoDb.UI.Properties.Resources.polygons; }
        }

        #endregion
    }

    #endregion
}
