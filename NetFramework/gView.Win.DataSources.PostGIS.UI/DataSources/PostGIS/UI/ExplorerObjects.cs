using gView.Framework.Data;
using gView.Framework.Db;
using gView.Framework.Db.UI;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using gView.Framework.Globalisation;
using gView.Framework.IO;
using gView.Framework.OGC.UI;
using gView.Framework.system;
using gView.Framework.system.UI;
using gView.Framework.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.DataSources.PostGIS.UI
{
    [RegisterPlugInAttribute("9F42DCF6-920E-433f-9D3C-610E4350EE35")]
    public class PostGISExplorerGroupObject : ExplorerParentObject, IOgcGroupExplorerObject, IPlugInDependencies
    {
        private IExplorerIcon _icon = new PostGISIcon();

        public PostGISExplorerGroupObject() : base(null, null, 0) { }

        #region IExplorerGroupObject Members

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        #endregion

        #region IExplorerObject Members

        public string Name
        {
            get { return "PostGIS"; }
        }

        public string FullName
        {
            get { return @"OGC\PostGIS"; }
        }

        public string Type
        {
            get { return "PostGIS Connection"; }
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

        #region IExplorerParentObject Members

        async public override Task<bool> Refresh()
        {
            await base.Refresh();
            base.AddChildObject(new PostGISNewConnectionObject(this));

            //ConfigTextStream stream = new ConfigTextStream("postgis_connections");
            //string connStr, id;
            //while ((connStr = stream.Read(out id)) != null)
            //{
            //    base.AddChildObject(new PostGISExplorerObject(id, connStr));
            //}
            //stream.Close();

            ConfigConnections conStream = new ConfigConnections("postgis", "546B0513-D71D-4490-9E27-94CD5D72C64A");
            Dictionary<string, string> DbConnectionStrings = conStream.Connections;
            foreach (string DbConnName in DbConnectionStrings.Keys)
            {
                DbConnectionString dbConn = new DbConnectionString();
                dbConn.FromString(DbConnectionStrings[DbConnName]);
                base.AddChildObject(new PostGISExplorerObject(this, DbConnName, dbConn));
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

            if (this.FullName == FullName)
            {
                PostGISExplorerGroupObject exObject = new PostGISExplorerGroupObject();
                cache.Append(exObject);
                return Task.FromResult<IExplorerObject>(exObject);
            }

            return Task.FromResult<IExplorerObject>(null);
        }

        #endregion

        #region IPlugInDependencies Member

        public bool HasUnsolvedDependencies()
        {
            return PostGISDataset.hasUnsolvedDependencies;
        }

        #endregion
    }

    [RegisterPlugInAttribute("003ED96C-4041-4657-A544-E92A3B7A96BF")]
    public class PostGISNewConnectionObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDoubleClick, IExplorerObjectCreatable
    {
        private IExplorerIcon _icon = new PostGISNewConnectionIcon();

        public PostGISNewConnectionObject()
            : base(null, null, 0)
        {
        }

        public PostGISNewConnectionObject(IExplorerObject parent)
            : base(parent, null, 0)
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
            get { return LocalizedResources.GetResString("String.NewConnection", "New Connection..."); }
        }

        public string FullName
        {
            get { return ""; }
        }

        public string Type
        {
            get { return "New PostGIS Connection"; }
        }

        public void Dispose()
        {

        }

        public Task<object> GetInstanceAsync()
        {
            return Task.FromResult<object>(null);
        }

        public Task<IExplorerObject> CreateInstanceByFullName(string FullName)
        {
            return Task.FromResult<IExplorerObject>(null);
        }
        #endregion

        #region IExplorerObjectDoubleClick Members

        public void ExplorerObjectDoubleClick(ExplorerObjectEventArgs e)
        {
            FormConnectionString dlg = new FormConnectionString();
            dlg.ProviderID = "postgre";
            dlg.UseProviderInConnectionString = false;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DbConnectionString dbConnStr = dlg.DbConnectionString;
                ConfigConnections connStream = new ConfigConnections("postgis", "546B0513-D71D-4490-9E27-94CD5D72C64A");

                string connectionString = dbConnStr.ConnectionString;
                string id = ConfigTextStream.ExtractValue(connectionString, "database");
                id = connStream.GetName(id);
                connStream.Add(id, dbConnStr.ToString());

                e.NewExplorerObject = new PostGISExplorerObject(this.ParentExplorerObject, id, dbConnStr);

                //string connStr = dlg.ConnectionString;
                //ConfigTextStream stream = new ConfigTextStream("postgis_connections", true, true);
                //string id = ConfigTextStream.ExtractValue(connStr, "Database");
                //id += "@" + ConfigTextStream.ExtractValue(connStr, "Server");
                //if (id == "@") id = "PostGIS Connection";
                //stream.Write(connStr, ref id);
                //stream.Close();

                //e.NewExplorerObject = new PostGISExplorerObject(id, dlg.ConnectionString);
            }
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

        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            return (parentExObject is PostGISExplorerGroupObject);
        }

        public Task<IExplorerObject> CreateExplorerObject(IExplorerObject parentExObject)
        {
            ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();
            ExplorerObjectDoubleClick(e);
            return Task.FromResult(e.NewExplorerObject);
        }

        #endregion
    }

    public class PostGISExplorerObject : gView.Framework.OGC.UI.ExplorerObjectFeatureClassImport, IExplorerSimpleObject, IExplorerObjectDeletable, IExplorerObjectRenamable, ISerializableExplorerObject, IExplorerObjectContextMenu
    {
        private PostGISIcon _icon = new PostGISIcon();
        private string _server = "";
        private IFeatureDataset _dataset;
        private DbConnectionString _connectionString;
        private ToolStripItem[] _contextItems = null;

        public PostGISExplorerObject() : base(null, typeof(IFeatureDataset)) { }
        public PostGISExplorerObject(IExplorerObject parent, string server, DbConnectionString connectionString)
            : base(parent, typeof(IFeatureDataset))
        {
            _server = server;
            _connectionString = connectionString;

            List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();
            ToolStripMenuItem item = new ToolStripMenuItem(LocalizedResources.GetResString("Menu.ConnectionProperties", "Connection Properties..."));
            item.Click += new EventHandler(ConnectionProperties_Click);
            items.Add(item);

            _contextItems = items.ToArray();
        }

        void ConnectionProperties_Click(object sender, EventArgs e)
        {
            if (_connectionString == null)
            {
                return;
            }

            FormConnectionString dlg = new FormConnectionString(_connectionString);
            dlg.ProviderID = "postgre";
            dlg.UseProviderInConnectionString = false;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                DbConnectionString dbConnStr = dlg.DbConnectionString;

                ConfigConnections connStream = new ConfigConnections("postgis", "546B0513-D71D-4490-9E27-94CD5D72C64A");
                connStream.Add(_server, dbConnStr.ToString());

                _connectionString = dbConnStr;
            }
        }

        internal string ConnectionString
        {
            get
            {
                return _connectionString.ConnectionString;
            }
        }

        #region IExplorerObject Members

        public string Name
        {
            get
            {
                return _server;
            }
        }

        public string FullName
        {
            get
            {
                return @"OGC\PostGIS\" + _server;
            }
        }

        public string Type
        {
            get { return "PostGIS Database"; }
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

        async public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
            {
                return cache[FullName];
            }

            PostGISExplorerGroupObject group = new PostGISExplorerGroupObject();
            if (FullName.IndexOf(group.FullName) != 0 || FullName.Length < group.FullName.Length + 2)
            {
                return null;
            }

            group = (PostGISExplorerGroupObject)((cache.Contains(group.FullName)) ? cache[group.FullName] : group);

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

        #region IExplorerObjectContextMenu Member

        public ToolStripItem[] ContextMenuItems
        {
            get { return _contextItems; }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("56E94E3B-CB00-4481-9293-AE45E2E360D2")]
    public class PostGISFeatureClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, ISerializableExplorerObject, IExplorerObjectDeletable, IPlugInDependencies
    {
        private string _fcname = "", _type = "";
        private IExplorerIcon _icon = null;
        private IFeatureClass _fc = null;
        private PostGISExplorerObject _parent = null;

        public PostGISFeatureClassExplorerObject() : base(null, typeof(IFeatureClass), 1) { }
        public PostGISFeatureClassExplorerObject(PostGISExplorerObject parent, IDatasetElement element)
            : base(parent, typeof(IFeatureClass), 1)
        {
            if (element == null || !(element.Class is IFeatureClass))
            {
                return;
            }

            _parent = parent;
            _fcname = element.Title;

            if (element.Class is IFeatureClass)
            {
                _fc = (IFeatureClass)element.Class;
                switch (_fc.GeometryType)
                {
                    case GeometryType.Envelope:
                    case GeometryType.Polygon:
                        _icon = new PostGISPolygonIcon();
                        _type = "Polygon Featureclass";
                        break;
                    case GeometryType.Multipoint:
                    case GeometryType.Point:
                        _icon = new PostGISPointIcon();
                        _type = "Point Featureclass";
                        break;
                    case GeometryType.Polyline:
                        _icon = new PostGISLineIcon();
                        _type = "Polyline Featureclass";
                        break;
                }
            }
        }

        internal string ConnectionString
        {
            get
            {
                if (_parent == null)
                {
                    return "";
                }

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
                if (_parent == null)
                {
                    return "";
                }

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
            {
                return cache[FullName];
            }

            FullName = FullName.Replace("/", @"\");
            int lastIndex = FullName.LastIndexOf(@"\");
            if (lastIndex == -1)
            {
                return null;
            }

            string dsName = FullName.Substring(0, lastIndex);
            string fcName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

            PostGISExplorerObject dsObject = new PostGISExplorerObject();
            dsObject = await dsObject.CreateInstanceByFullName(dsName, cache) as PostGISExplorerObject;

            var childObjects = await dsObject?.ChildObjects();
            if (childObjects == null)
            {
                return null;
            }

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
                    if (ExplorerObjectDeleted != null)
                    {
                        ExplorerObjectDeleted(this);
                    }

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

        #region IPlugInDependencies Member

        public bool HasUnsolvedDependencies()
        {
            return PostGISDataset.hasUnsolvedDependencies;
        }

        #endregion
    }

    class PostGISIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("CCB8C05B-8668-4235-8C53-2051B952DA11");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return global::gView.Win.DataSources.PostGIS.UI.Properties.Resources.cat6;
            }
        }

        #endregion
    }

    class PostGISNewConnectionIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("3699078B-3E38-4472-87DF-D32EF8C9A566");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return global::gView.Win.DataSources.PostGIS.UI.Properties.Resources.gps_point;
            }
        }

        #endregion
    }

    public class PostGISPointIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("3B3ECA05-D604-42c4-B552-1A8CC245A19B"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Win.DataSources.PostGIS.UI.Properties.Resources.img_32; }
        }

        #endregion
    }

    public class PostGISLineIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("50BEE332-873F-475c-83EF-DBA41908074D"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Win.DataSources.PostGIS.UI.Properties.Resources.img_33; }
        }

        #endregion
    }

    public class PostGISPolygonIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("A139AA23-B06B-4139-9284-C1AB35B68DC3"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Win.DataSources.PostGIS.UI.Properties.Resources.img_34; }
        }

        #endregion
    }
}
