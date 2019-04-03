using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.system.UI;
using gView.Framework.UI;
using gView.Framework.IO;
using gView.Framework.Db;
using gView.Framework.Globalisation;
using gView.Framework.Db.UI;
using System.Windows.Forms;
using gView.Framework.UI.Dialogs;
using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using gView.Framework.UI.Dialogs.Network;
using gView.DataSources.Fdb.MSSql;
using gView.DataSources.Fdb.PostgreSql;
using gView.DataSources.Fdb.UI.MSSql;
using gView.DataSources.Fdb.UI.MSAccess;
using gView.DataSources.Fdb.MSAccess;
using gView.Framework.UI.Controls.Filter;

namespace gView.DataSources.Fdb.UI.PostgreSql
{
    [gView.Framework.system.RegisterPlugIn("9E7CE708-F920-4554-A86B-EBA84EC6C2A0")]
    public class ExplorerGroupObject : ExplorerParentObject, IExplorerGroupObject
    {
        private IExplorerIcon _icon = new gView.DataSources.Fdb.UI.MSSql.SqlFDBConnectionsIcon();

        public ExplorerGroupObject() : base(null, null, 0) { }

        #region IExplorerGroupObject Members

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        #endregion

        #region IExplorerObject Members

        public string Name
        {
            get { return "Postgre Feature Database Connections"; }
        }

        public string FullName
        {
            get { return "PostgreFDBConnections"; }
        }

        public string Type
        {
            get { return "PostgreFDB Connections"; }
        }

        public new object Object
        {
            get { return null; }
        }

        public IExplorerObject CreateInstanceByFullName(string FullName)
        {
            return null;
        }

        #endregion

        #region IExplorerParentObject Members

        public override void Refresh()
        {
            base.Refresh();
            base.AddChildObject(new NewConnectionObject());

            ConfigTextStream stream = new ConfigTextStream("postgrefdb_connections");
            string connStr, id;
            while ((connStr = stream.Read(out id)) != null)
            {
                base.AddChildObject(new ExplorerObject(this, id, connStr));
            }
            stream.Close();

            ConfigConnections conStream = new ConfigConnections("postgrefdb", "546B0513-D71D-4490-9E27-94CD5D72C64A");
            Dictionary<string, string> DbConnectionStrings = conStream.Connections;
            foreach (string DbConnName in DbConnectionStrings.Keys)
            {
                DbConnectionString dbConn = new DbConnectionString();
                dbConn.FromString(DbConnectionStrings[DbConnName]);
                base.AddChildObject(new ExplorerObject(this, DbConnName, dbConn));
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            if (this.FullName == FullName)
            {
                ExplorerGroupObject exObject = new ExplorerGroupObject();
                cache.Append(exObject);
                return exObject;
            }

            return null;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("F95513B4-572E-40fc-9BD9-F6ABD4BB1D66")]
    public class NewFDBDatabase : ExplorerObjectCls, IExplorerObject, IExplorerObjectCreatable
    {
        private SqlFDBIcon _icon = new SqlFDBIcon();

        public NewFDBDatabase() : base(null, null, 1) { }

        #region IExplorerObject Member

        public string Name
        {
            get
            {
                //return "Create new Postgre Feature database";
                return LocalizedResources.GetResString("ArgString.New", "new Postgre Feature database", "Postgre Feature-Database");
            }
        }

        public string FullName
        {
            get { return String.Empty; }
        }

        public string Type
        {
            get { return "Postgre Feature database"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public void Dispose()
        {

        }

        public new object Object
        {
            get { return null; }
        }

        #endregion

        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            return (parentExObject is ExplorerGroupObject);
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            FormCreatePostgreFeatureDatabase dlg = new FormCreatePostgreFeatureDatabase();
            dlg.ShowDialog();
            return dlg.ResultExplorerObject;
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            return null;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("C6BCD1F5-EBE3-4142-95E3-EE91B5EB35BB")]
    public class NewConnectionObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDoubleClick, IExplorerObjectCreatable
    {
        private IExplorerIcon _icon = new SqlFDBNewConnectionIcon();

        public NewConnectionObject() : base(null, null, 1) { }

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
            get { return String.Empty; }
        }

        public string Type
        {
            get { return "New PostgreFDB Connection"; }
        }

        public void Dispose()
        {

        }

        public new object Object
        {
            get { return null; }
        }

        public IExplorerObject CreateInstanceByFullName(string FullName)
        {
            return null;
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
                ConfigConnections connStream = new ConfigConnections("postgrefdb", "546B0513-D71D-4490-9E27-94CD5D72C64A");

                string connectionString = dbConnStr.ConnectionString;
                string id = ConfigTextStream.ExtractValue(connectionString, "database");
                id += "@" + ConfigTextStream.ExtractValue(connectionString, "server");
                if (id == "@") id = "PostgreFDB Connection";
                id = connStream.GetName(id);

                connStream.Add(id, dbConnStr.ToString());
                e.NewExplorerObject = new ExplorerObject(this, id, dbConnStr);
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            return null;
        }

        #endregion

        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            return (parentExObject is ExplorerGroupObject);
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();
            ExplorerObjectDoubleClick(e);
            return e.NewExplorerObject;
        }

        #endregion
    }

    public class ExplorerObject : ExplorerParentObject, IExplorerSimpleObject, IExplorerObjectDeletable, IExplorerObjectRenamable, IExplorerObjectCommandParameters, ISerializableExplorerObject, IExplorerObjectContextMenu
    {
        private IExplorerIcon _icon = new SqlFDBIcon();
        private string _server = String.Empty, _connectionString = String.Empty, _errMsg = String.Empty;
        private ToolStripItem[] _contextItems = null;
        private DbConnectionString _dbConnectionString = null;

        public ExplorerObject(int priority) : base(null, null, priority) { }
        public ExplorerObject(IExplorerObject parent, string server, string connectionString)
            : base(parent, null, parent != null ? parent.Priority : 1)
        {
            _server = server;
            _connectionString = connectionString;

            _contextItems = new ToolStripItem[1];
            _contextItems[0] = new ToolStripMenuItem("Tasks");
        }
        public ExplorerObject(IExplorerObject parent, string server, DbConnectionString dbConnectionString)
            : this(parent, server, (dbConnectionString != null) ? dbConnectionString.ConnectionString : String.Empty)
        {
            _dbConnectionString = dbConnectionString;
            _icon = new SqlFDBIcon2();

            List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();
            if (_contextItems != null)
            {
                foreach (ToolStripMenuItem i in _contextItems)
                    items.Add(i);
            }

            ToolStripMenuItem item = new ToolStripMenuItem(LocalizedResources.GetResString("Menu.ConnectionProperties", "Connection Properties..."));
            item.Click += new EventHandler(ConnectionProperties_Click);
            items.Add(item);

            _contextItems = items.ToArray();
        }

        void ConnectionProperties_Click(object sender, EventArgs e)
        {
            if (_dbConnectionString == null) return;

            FormConnectionString dlg = new FormConnectionString(_dbConnectionString);
            dlg.ProviderID = "postgre";
            dlg.UseProviderInConnectionString = false;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                DbConnectionString dbConnStr = dlg.DbConnectionString;

                ConfigConnections connStream = new ConfigConnections("postgrefdb", "546B0513-D71D-4490-9E27-94CD5D72C64A");
                connStream.Add(_server, dbConnStr.ToString());

                _dbConnectionString = dbConnStr;
                _connectionString = dbConnStr.ConnectionString;
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
                return _server;
            }
        }

        public string FullName
        {
            get
            {
                return @"PostgreFDBConnections\" + _server;
            }
        }

        public string Type
        {
            get { return "Postgre Feature Database"; }
        }

        public IExplorerIcon Icon
        {
            get
            {
                return _icon;
            }
        }

        public void Dispose() { }
        public object Object { get { return null; } }

        #endregion

        private string[] DatasetNames
        {
            get
            {
                try
                {
                    pgFDB fdb = new pgFDB();
                    if (!fdb.Open(_connectionString))
                    {
                        _errMsg = fdb.lastErrorMsg;
                        return null;
                    }
                    string[] ds = fdb.DatasetNames;
                    string[] dsMod = new string[ds.Length];

                    int i = 0;
                    foreach (string dsname in ds)
                    {
                        string imageSpace;
                        if (fdb.IsImageDataset(dsname, out imageSpace))
                        {
                            dsMod[i++] = "#" + dsname;
                        }
                        else
                        {
                            dsMod[i++] = dsname;
                        }
                    }
                    if (ds == null) _errMsg = fdb.lastErrorMsg;
                    fdb.Dispose();

                    return dsMod;
                }
                catch (Exception ex)
                {
                    _errMsg = ex.Message;
                    return null;
                }
            }
        }

        #region IExplorerParentObject Members

        public override void Refresh()
        {
            base.Refresh();
            string[] ds = DatasetNames;
            if (ds == null)
            {
                System.Windows.Forms.MessageBox.Show(_errMsg, "ERROR");
            }
            else
            {
                foreach (string dsname in ds)
                {
                    if (dsname == String.Empty) continue;
                    base.AddChildObject(new DatasetExplorerObject(this, dsname));
                }
            }
        }
        #endregion

        #region IExplorerObjectCommandParameters Members

        public Dictionary<string, string> Parameters
        {
            get
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("Connection String", _connectionString);
                return parameters;
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            ExplorerGroupObject group = new ExplorerGroupObject();
            if (FullName.IndexOf(group.FullName) != 0 || FullName.Length < group.FullName.Length + 2) return null;

            group = (ExplorerGroupObject)((cache.Contains(group.FullName)) ? cache[group.FullName] : group);

            foreach (IExplorerObject exObject in group.ChildObjects)
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

        public bool DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            if (_dbConnectionString != null)
            {
                ConfigConnections stream = new ConfigConnections("postgrefdb", "546B0513-D71D-4490-9E27-94CD5D72C64A");
                stream.Remove(_server);
            }
            else
            {
                ConfigTextStream stream = new ConfigTextStream("postgrefdb_connections", true, true);
                stream.Remove(this.Name, _connectionString);
                stream.Close();
            }
            if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
            return true;
        }

        #endregion

        #region IExplorerObjectRenamable Member

        public event ExplorerObjectRenamedEvent ExplorerObjectRenamed = null;

        public bool RenameExplorerObject(string newName)
        {
            bool ret = false;
            if (_dbConnectionString != null)
            {
                ConfigConnections stream = new ConfigConnections("postgrefdb", "546B0513-D71D-4490-9E27-94CD5D72C64A");
                ret = stream.Rename(_server, newName);
            }
            else
            {
                ConfigTextStream stream = new ConfigTextStream("postgrefdb_connections", true, true);
                ret = stream.ReplaceHoleLine(ConfigTextStream.BuildLine(_server, _connectionString), ConfigTextStream.BuildLine(newName, _connectionString));
                stream.Close();
            }
            if (ret == true)
            {
                _server = newName;
                if (ExplorerObjectRenamed != null) ExplorerObjectRenamed(this);
            }
            return ret;
        }

        #endregion

        #region IExplorerObjectContextMenu Member

        public ToolStripItem[] ContextMenuItems
        {
            get
            {
                return _contextItems;
            }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("E1A2BCA0-A6E1-47f2-8AA2-201D2CE21815")]
    public class DatasetExplorerObject : ExplorerObjectFeatureClassImport, IExplorerSimpleObject, IExplorerObjectDeletable, ISerializableExplorerObject, IExplorerObjectCreatable, IExplorerObjectContextMenu, IExplorerObjectRenamable
    {
        private IExplorerIcon _icon = null;
        private ExplorerObject _parent = null;
        private ToolStripItem[] _contextItems = null;
        private bool _isImageDataset = false;

        public DatasetExplorerObject() : base(null, typeof(pgDataset)) { }
        public DatasetExplorerObject(ExplorerObject parent, string dsname)
            : base(parent, typeof(pgDataset))
        {
            _parent = parent;

            if (dsname.IndexOf("#") == 0)
            {
                _isImageDataset = true;
                dsname = dsname.Substring(1, dsname.Length - 1);
                _icon = new gView.DataSources.Fdb.UI.MSAccess.AccessFDBImageDatasetIcon();
            }
            else
            {
                _isImageDataset = false;
                _icon = new gView.DataSources.Fdb.UI.MSAccess.AccessFDBDatasetIcon();
            }

            _dsname = dsname;

            _dataset = new pgDataset();
            _dataset.ConnectionString = this.ConnectionString + ";dsname=" + _dsname;
            _dataset.Open();

            _contextItems = new ToolStripItem[2];
            _contextItems[0] = new ToolStripMenuItem("Spatial Reference...");
            _contextItems[0].Click += new EventHandler(SpatialReference_Click);
            _contextItems[1] = new ToolStripMenuItem("Shrink Spatial Indices...");
            _contextItems[1].Click += new EventHandler(ShrinkSpatialIndices_Click);
        }

        void SpatialReference_Click(object sender, EventArgs e)
        {
            if (_dataset == null || _fdb == null)
            {
                Refresh();
                if (_dataset == null || _fdb == null)
                {
                    MessageBox.Show("Can't open dataset...");
                    return;
                }
            }

            FormSpatialReference dlg = new FormSpatialReference(_dataset.SpatialReference);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                int id = _fdb.CreateSpatialReference(dlg.SpatialReference);
                if (id == -1)
                {
                    MessageBox.Show("Can't create Spatial Reference!\n", _fdb.lastErrorMsg);
                    return;
                }
                if (!_fdb.SetSpatialReferenceID(_dataset.DatasetName, id))
                {
                    MessageBox.Show("Can't set Spatial Reference!\n", _fdb.lastErrorMsg);
                    return;
                }
                _dataset.SpatialReference = dlg.SpatialReference;
            }
        }

        void ShrinkSpatialIndices_Click(object sender, EventArgs e)
        {
            if (_dataset == null) return;

            List<IClass> classes = new List<IClass>();
            foreach (IDatasetElement element in _dataset.Elements)
            {
                if (element == null || element.Class == null) continue;
                classes.Add(element.Class);
            }

            SpatialIndexShrinker rebuilder = new SpatialIndexShrinker();
            rebuilder.RebuildIndices(classes);
        }

        internal string ConnectionString
        {
            get
            {
                if (_parent == null) return String.Empty;
                return _parent.ConnectionString;
            }
        }

        internal bool IsImageDataset
        {
            get { return _isImageDataset; }
        }

        #region IExplorerObject Members

        public string Name
        {
            get { return _dsname; }
        }

        public string FullName
        {
            get
            {
                if (_parent == null) return String.Empty;
                return _parent.FullName + @"\" + this.Name;
            }
        }
        public string Type
        {
            get { return "Postgre Feature Database Dataset"; }
        }
        public IExplorerIcon Icon
        {
            get
            {
                if (_icon == null)
                    return new gView.DataSources.Fdb.UI.MSAccess.AccessFDBDatasetIcon();
                return _icon;
            }
        }
        public override void Dispose()
        {
            base.Dispose();

            _fdb = null;
            if (_dataset != null)
            {
                _dataset.Dispose();
                _dataset = null;
            }
        }
        public object Object { get { return _dataset; } }

        #endregion

        #region IExplorerParentObject Members

        public override void Refresh()
        {
            base.Refresh();
            this.Dispose();

            _dataset = new pgDataset();
            _dataset.ConnectionString = this.ConnectionString + ";dsname=" + _dsname;

            if (_dataset.Open())
            {
                foreach (IDatasetElement element in _dataset.Elements)
                {
                    base.AddChildObject(new FeatureClassExplorerObject(this, _dsname, element));
                }
            }
            _fdb = (pgFDB)_dataset.Database;
        }

        #endregion

        internal bool DeleteFeatureClass(string name)
        {
            if (_dataset == null || !(_dataset.Database is IFeatureDatabase)) return false;

            if (!((IFeatureDatabase)_dataset.Database).DeleteFeatureClass(name))
            {
                MessageBox.Show(_dataset.Database.lastErrorMsg);
                return false;
            }
            return true;
        }

        internal bool DeleteDataset(string dsname)
        {
            if (_dataset == null || !(_dataset.Database is IFeatureDatabase)) return false;

            if (!((IFeatureDatabase)_dataset.Database).DeleteDataset(dsname))
            {
                MessageBox.Show(_dataset.Database.lastErrorMsg);
                return false;
            }
            return true;
        }

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            FullName = FullName.Replace("/", @"\");
            int lastIndex = FullName.LastIndexOf(@"\");
            if (lastIndex == -1) return null;

            string fdbName = FullName.Substring(0, lastIndex);
            string dsName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

            ExplorerObject fdbObject = new ExplorerObject(1);
            fdbObject = (ExplorerObject)fdbObject.CreateInstanceByFullName(fdbName, cache);
            if (fdbObject == null || fdbObject.ChildObjects == null) return null;

            foreach (IExplorerObject exObject in fdbObject.ChildObjects)
            {
                if (exObject.Name == dsName)
                {
                    cache.Append(exObject);
                    return exObject;
                }
            }
            return null;
        }

        #endregion

        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            if (parentExObject is ExplorerObject) return true;

            return false;
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            if (!CanCreate(parentExObject)) return null;

            pgFDB fdb = new pgFDB();
            fdb.Open(((ExplorerObject)parentExObject).ConnectionString);

            FormNewDataset dlg = new FormNewDataset();
            if (fdb.FdbVersion >= new Version(1, 2, 0))
                dlg.ShowSpatialIndexTab = true;

            if (dlg.ShowDialog() != DialogResult.OK) return null;

            ISpatialReference sRef = dlg.SpatialReferene;
            ISpatialIndexDef sIndexDef = dlg.SpatialIndexDef;

            if (fdb.FdbVersion >= new Version(1, 2, 0) &&
                sIndexDef is MSSpatialIndex &&
                ((MSSpatialIndex)sIndexDef).GeometryType == GeometryFieldType.MsGeography)
                sRef = SpatialReference.FromID("epsg:4326");

            int dsID = -1;

            string datasetName = dlg.DatasetName;
            switch (dlg.DatasetType)
            {
                case FormNewDataset.datasetType.FeatureDataset:
                    dsID = fdb.CreateDataset(datasetName, sRef, sIndexDef);
                    break;
                case FormNewDataset.datasetType.ImageDataset:
                    dsID = fdb.CreateImageDataset(datasetName, sRef, sIndexDef, dlg.ImageSpace, dlg.AdditionalFields);
                    datasetName = "#" + datasetName;
                    break;
            }

            if (dsID == -1)
            {
                MessageBox.Show(fdb.lastErrorMsg, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return new DatasetExplorerObject((ExplorerObject)parentExObject, datasetName);
        }

        #endregion

        #region IExplorerObjectContextMenu Member

        public ToolStripItem[] ContextMenuItems
        {
            get
            {
                return _contextItems;
            }
        }

        #endregion

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted = null;

        public bool DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            if (DeleteDataset(_dsname))
            {
                if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
                return true;
            }
            return false;
        }

        #endregion

        #region IExplorerObjectRenamable Member

        public event ExplorerObjectRenamedEvent ExplorerObjectRenamed;

        public bool RenameExplorerObject(string newName)
        {
            if (newName == this.Name) return false;

            if (_dataset == null || !(_dataset.Database is pgFDB))
            {
                MessageBox.Show("Can't rename dataset...\nUncorrect dataset !!!");
                return false;
            }
            if (!((pgFDB)_dataset.Database).RenameDataset(this.Name, newName))
            {
                MessageBox.Show("Can't rename dataset...\n" + ((pgFDB)_dataset.Database).lastErrorMsg);
                return false;
            }

            _dsname = newName;

            if (ExplorerObjectRenamed != null) ExplorerObjectRenamed(this);
            return true;
        }

        #endregion

        public override void Content_DragEnter(DragEventArgs e)
        {
            if (_icon is gView.DataSources.Fdb.UI.MSAccess.AccessFDBImageDatasetIcon)
            {
                e.Effect = DragDropEffects.None;
            }
            else
            {
                base.Content_DragEnter(e);
            }
        }
    }

    [gView.Framework.system.RegisterPlugIn("ACFAD6F4-882D-4944-BD84-5B6080988717")]
    public class FeatureClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDeletable, ISerializableExplorerObject, IExplorerObjectContextMenu, IExplorerObjectRenamable, IExplorerObjectCreatable, IExporerOjectSchema
    {
        private string _dsname = String.Empty, _fcname = String.Empty, _type = String.Empty;
        private IExplorerIcon _icon = null;
        private IFeatureClass _fc = null;
        private IRasterClass _rc = null;
        private DatasetExplorerObject _parent = null;
        private ToolStripItem[] _contextItems = null;
        private bool _isNetwork = false;

        public FeatureClassExplorerObject() : base(null, typeof(pgFeatureClass), 1) { }
        public FeatureClassExplorerObject(DatasetExplorerObject parent, string dsname, IDatasetElement element)
            : base(parent, typeof(pgFeatureClass), 1)
        {
            if (element == null) return;

            _parent = parent;
            _dsname = dsname;
            _fcname = element.Title;

            string typePrefix = String.Empty;
            bool isLinked = false;
            if (element.Class is LinkedFeatureClass)
            {
                typePrefix = "Linked ";
                isLinked = true;
            }

            if (element.Class is IRasterCatalogClass)
            {
                _icon = new AccessFDBRasterIcon();
                _type = typePrefix + "Raster Catalog Layer";
                _rc = (IRasterClass)element.Class;
            }
            else if (element.Class is IRasterClass)
            {
                _icon = new AccessFDBRasterIcon();
                _type = typePrefix + "Raster Layer";
                _rc = (IRasterClass)element.Class;
            }
            else if (element.Class is IFeatureClass)
            {
                _fc = (IFeatureClass)element.Class;
                switch (_fc.GeometryType)
                {
                    case geometryType.Envelope:
                    case geometryType.Polygon:
                        if (isLinked)
                            _icon = new AccessFDBLinkedPolygonIcon();
                        else
                            _icon = new AccessFDBPolygonIcon();
                        _type = typePrefix + "Polygon Featureclass";
                        break;
                    case geometryType.Multipoint:
                    case geometryType.Point:
                        if (isLinked)
                            _icon = new AccessFDBLinkedPointIcon();
                        else
                            _icon = new AccessFDBPointIcon();
                        _type = typePrefix + "Point Featureclass";
                        break;
                    case geometryType.Polyline:
                        if (isLinked)
                            _icon = new AccessFDBLinkedLineIcon();
                        else
                            _icon = new AccessFDBLineIcon();
                        _type = typePrefix + "Polyline Featureclass";
                        break;
                    case geometryType.Network:
                        _icon = new gView.DataSources.Fdb.UI.MSAccess.AccessFDBNetworkIcon();
                        _type = "Networkclass";
                        _isNetwork = true;
                        break;
                }

            }

            if (!_isNetwork)
            {
                _contextItems = new ToolStripItem[1];
                _contextItems[0] = new ToolStripMenuItem("Tasks");

                //ToolStripMenuItem item = new ToolStripMenuItem("Alter table...");
                //item.Click += new EventHandler(Altertable_Click);
                //((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(item);
                //((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(new ToolStripSeparator());
                ToolStripMenuItem item = new ToolStripMenuItem("Shrink Spatial Index...");
                item.Click += new EventHandler(ShrinkSpatialIndex_Click);
                ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(item);
                item = new ToolStripMenuItem("Spatial Index Definition...");
                item.Click += new EventHandler(SpatialIndexDef_Click);
                ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(item);
                item = new ToolStripMenuItem("Repair Spatial Index...");
                item.Click += new EventHandler(RepairSpatialIndex_Click);
                ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(item);
                ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(new ToolStripSeparator());
                item = new ToolStripMenuItem("Truncate");
                item.Click += new EventHandler(Truncate_Click);
                ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(item);
            }
        }

        internal string ConnectionString
        {
            get
            {
                if (_parent == null) return String.Empty;
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
                if (_parent == null) return String.Empty;
                return _parent.FullName + @"\" + this.Name;
            }
        }
        public string Type
        {
            get { return _type != String.Empty ? _type : "PostgreFDB Featureclass"; }
        }

        public IExplorerIcon Icon
        {
            get
            {
                if (_icon == null)
                    return new gView.DataSources.Fdb.UI.MSAccess.AccessFDBPolygonIcon();
                return _icon;
            }
        }
        public void Dispose()
        {
            if (_fc != null)
            {
                _fc = null;
            }
            if (_rc != null)
            {
                _rc = null;
            }
        }
        public object Object
        {
            get
            {
                if (_fc != null) return _fc;
                if (_rc != null) return _rc;
                return null;
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            FullName = FullName.Replace("/", @"\");
            int lastIndex = FullName.LastIndexOf(@"\");
            if (lastIndex == -1) return null;

            string dsName = FullName.Substring(0, lastIndex);
            string fcName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

            DatasetExplorerObject dsObject = new DatasetExplorerObject();
            dsObject = (DatasetExplorerObject)dsObject.CreateInstanceByFullName(dsName, cache);
            if (dsObject == null || dsObject.ChildObjects == null) return null;

            foreach (IExplorerObject exObject in dsObject.ChildObjects)
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

        #region IExplorerObjectContextMenu Member

        public ToolStripItem[] ContextMenuItems
        {
            get
            {
                return _contextItems;
            }
        }

        #endregion

        void ShrinkSpatialIndex_Click(object sender, EventArgs e)
        {
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is pgFDB))
            {
                MessageBox.Show("Can't shrink index...\nUncorrect feature class !!!");
                return;
            }

            List<IClass> classes = new List<IClass>();
            classes.Add(_fc);

            SpatialIndexShrinker rebuilder = new SpatialIndexShrinker();
            rebuilder.RebuildIndices(classes);
            _fc.Dataset.RefreshClasses();
        }

        void SpatialIndexDef_Click(object sender, EventArgs e)
        {
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is pgFDB))
            {
                MessageBox.Show("Can't show spatial index definition...\nUncorrect feature class !!!");
                return;
            }

            FormRebuildSpatialIndexDef dlg = new FormRebuildSpatialIndexDef((pgFDB)_fc.Dataset.Database, _fc);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
            }
        }

        void RepairSpatialIndex_Click(object sender, EventArgs e)
        {
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is pgFDB))
            {
                MessageBox.Show("Can't show spatial index definition...\nUncorrect feature class !!!");
                return;
            }

            FormRepairSpatialIndexProgress dlg = new FormRepairSpatialIndexProgress((pgFDB)_fc.Dataset.Database, _fc);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
            }
        }

        void Truncate_Click(object sender, EventArgs e)
        {
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is pgFDB))
            {
                MessageBox.Show("Can't rebuild index...\nUncorrect feature class !!!");
                return;
            }

            ((pgFDB)_fc.Dataset.Database).TruncateTable(_fc.Name);
            _fc.Dataset.RefreshClasses();
        }

        void Altertable_Click(object sender, EventArgs e)
        {
        }

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted = null;

        public bool DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            if (_parent == null) return false;
            if (_parent.DeleteFeatureClass(_fcname))
            {
                if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
                return true;
            }
            return false;
        }

        #endregion

        #region IExplorerObjectRenamable Member

        public event ExplorerObjectRenamedEvent ExplorerObjectRenamed;

        public bool RenameExplorerObject(string newName)
        {
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is pgFDB))
            {
                MessageBox.Show("Can't rename featureclass...\nUncorrect feature class !!!");
                return false;
            }

            if (!((pgFDB)_fc.Dataset.Database).RenameFeatureClass(this.Name, newName))
            {
                MessageBox.Show("Can't rename featureclass...\n" + ((pgFDB)_fc.Dataset.Database).lastErrorMsg);
                return false;
            }

            _fcname = newName;

            if (ExplorerObjectRenamed != null) ExplorerObjectRenamed(this);
            return true;
        }

        #endregion

        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            if (parentExObject is DatasetExplorerObject &&
                !((DatasetExplorerObject)parentExObject).IsImageDataset) return true;
            return false;
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            if (!CanCreate(parentExObject)) return null;

            if (!(parentExObject.Object is IFeatureDataset) || !(((IDataset)parentExObject.Object).Database is pgFDB))
            {
                return null;
            }
            pgFDB fdb = ((IDataset)parentExObject.Object).Database as pgFDB;

            FormNewFeatureclass dlg = new FormNewFeatureclass(parentExObject.Object as IFeatureDataset);
            if (dlg.ShowDialog() != DialogResult.OK) return null;

            IGeometryDef gDef = dlg.GeometryDef;

            int FCID = fdb.CreateFeatureClass(
                parentExObject.Name,
                dlg.FeatureclassName,
                gDef,
                dlg.Fields);

            if (FCID < 0)
            {
                MessageBox.Show("ERROR: " + fdb.lastErrorMsg);
                return null;
            }

            ISpatialIndexDef sIndexDef = fdb.SpatialIndexDef(parentExObject.Name);
            fdb.SetSpatialIndexBounds(dlg.FeatureclassName, "BinaryTree2", dlg.SpatialIndexExtents, 0.55, 200, dlg.SpatialIndexLevels);

            IDatasetElement element = ((IFeatureDataset)parentExObject.Object)[dlg.FeatureclassName];
            return new FeatureClassExplorerObject(
                parentExObject as DatasetExplorerObject,
                parentExObject.Name,
                element);
        }

        #endregion

        #region IExporerOjectSchema Member

        public string Schema
        {
            get
            {
                if (_fc is pgFeatureClass)
                    return ((pgFeatureClass)_fc).DbSchema;

                return String.Empty;
            }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("18830A3E-FFEA-477c-B3E1-E4624F828034")]
    public class NetworkClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectCreatable
    {
        public NetworkClassExplorerObject() : base(null, typeof(pgFeatureClass), 1) { }

        #region IExplorerObject Member

        public string Name
        {
            get { return String.Empty; }
        }

        public string FullName
        {
            get { return String.Empty; }
        }

        public string Type
        {
            get { return "Network Class"; }
        }

        public IExplorerIcon Icon
        {
            get
            {
                return new gView.DataSources.Fdb.UI.MSAccess.AccessFDBNetworkIcon();
            }
        }

        public new object Object
        {
            get { return null; }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            return null;
        }

        #endregion

        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            if (parentExObject is DatasetExplorerObject) return true;
            return false;
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            if (!(parentExObject is DatasetExplorerObject))
                return null;

            IFeatureDataset dataset = ((DatasetExplorerObject)parentExObject).Object as IFeatureDataset;
            if (dataset == null || !(dataset.Database is pgFDB))
                return null;

            FormNewNetworkclass dlg = new FormNewNetworkclass(dataset, typeof(CreateFDBNetworkFeatureclass));
            if (dlg.ShowDialog() != DialogResult.OK)
                return null;

            CreateFDBNetworkFeatureclass creator = new CreateFDBNetworkFeatureclass(
                dataset, dlg.NetworkName,
                dlg.EdgeFeatureclasses,
                dlg.NodeFeatureclasses);
            creator.SnapTolerance = dlg.SnapTolerance;
            creator.ComplexEdgeFcIds = dlg.ComplexEdgeFcIds;
            creator.GraphWeights = dlg.GraphWeights;
            creator.SwitchNodeFcIdAndFieldnames = dlg.SwitchNodeFcIds;
            creator.NodeTypeFcIds = dlg.NetworkNodeTypeFcIds;

            FormProgress progress = new FormProgress();
            progress.ShowProgressDialog(creator, null, creator.Thread);

            IDatasetElement element = ((IFeatureDataset)parentExObject.Object)[dlg.NetworkName];
            return new FeatureClassExplorerObject(
                parentExObject as DatasetExplorerObject,
                parentExObject.Name,
                element);
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("af5f4d50-745b-11e1-b0c4-0800200c9a66")]
    public class GeographicViewExplorerObject : IExplorerSimpleObject, IExplorerObjectCreatable
    {
        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            return parentExObject is DatasetExplorerObject;
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            DatasetExplorerObject parent = (DatasetExplorerObject)parentExObject;

            IFeatureDataset dataset = parent.Object as IFeatureDataset;
            if (dataset == null)
                return null;

            AccessFDB fdb = dataset.Database as AccessFDB;
            if (fdb == null)
                return null;

            FormRegisterGeographicView dlg = new FormRegisterGeographicView(parent.Object as IFeatureDataset);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                int fc_id = fdb.CreateSpatialView(dataset.DatasetName, dlg.SpatialViewAlias);

                IDatasetElement element = ((IFeatureDataset)parentExObject.Object)[dlg.SpatialViewAlias];
                return new FeatureClassExplorerObject(
                    parentExObject as DatasetExplorerObject,
                    parentExObject.Name,
                    element);
            } 
            return null;
        }

        #endregion

        #region IExplorerObject Member

        public string Name
        {
            get { return "Geographic View"; }
        }

        public string FullName
        {
            get { return "Geographic View"; }
        }

        public string Type
        {
            get { return String.Empty; }
        }

        public IExplorerIcon Icon
        {
            get { return new AccessFDBGeographicViewIcon(); }
        }

        public IExplorerObject ParentExplorerObject
        {
            get { return null; }
        }

        public new object Object
        {
            get { return null; }
        }

        public Type ObjectType
        {
            get { return null; }
        }

        public int Priority { get { return 1; } }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            return null;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("D320CA4D-E63F-4DD4-96F4-CA11DC95A39E")]
    public class LinkedFeatureclassExplorerObject : IExplorerSimpleObject, IExplorerObjectCreatable
    {
        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            return parentExObject is DatasetExplorerObject;
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            DatasetExplorerObject parent = (DatasetExplorerObject)parentExObject;

            IFeatureDataset dataset = parent.Object as IFeatureDataset;
            if (dataset == null)
                return null;

            AccessFDB fdb = dataset.Database as AccessFDB;
            if (fdb == null)
                return null;

            List<ExplorerDialogFilter> filters = new List<ExplorerDialogFilter>();
            filters.Add(new OpenFeatureclassFilter());
            ExplorerDialog dlg = new ExplorerDialog("Select Featureclass", filters, true);

            IExplorerObject ret = null;

            if (dlg.ShowDialog() == DialogResult.OK &&
                dlg.ExplorerObjects != null)
            {
                foreach (IExplorerObject exObj in dlg.ExplorerObjects)
                {
                    if (exObj.Object is IFeatureClass)
                    {
                        int fcid = fdb.CreateLinkedFeatureClass(dataset.DatasetName, (IFeatureClass)exObj.Object);
                        if (ret == null)
                        {
                            IDatasetElement element = dataset[((IFeatureClass)exObj.Object).Name];
                            if (element != null)
                            {
                                ret = new FeatureClassExplorerObject(
                                    parentExObject as DatasetExplorerObject,
                                    parentExObject.Name,
                                    element);
                            }
                        }
                    }
                }
            }
            return ret;
        }

        #endregion

        #region IExplorerObject Member

        public string Name
        {
            get { return "Linked Featureclass"; }
        }

        public string FullName
        {
            get { return "Linked Featureclass"; }
        }

        public string Type
        {
            get { return String.Empty; }
        }

        public IExplorerIcon Icon
        {
            get { return new AccessFDBGeographicViewIcon(); }
        }

        public IExplorerObject ParentExplorerObject
        {
            get { return null; }
        }

        public new object Object
        {
            get { return null; }
        }

        public Type ObjectType
        {
            get { return null; }
        }

        public int Priority { get { return 1; } }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            return null;
        }

        #endregion
    }

    //[gView.Framework.system.RegisterPlugIn("9B5B718C-2ECA-47ee-851F-9D33E3D82C55")]
    public class SqlFDBTileGridClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectCreatable
    {
        public SqlFDBTileGridClassExplorerObject() : base(null, null, 1) { }

        #region IExplorerObject Member

        public string Name
        {
            get { return String.Empty; }
        }

        public string FullName
        {
            get { return String.Empty; }
        }

        public string Type
        {
            get { return "Tile Grid Class"; }
        }

        public IExplorerIcon Icon
        {
            get
            {
                return new gView.DataSources.Fdb.UI.MSAccess.AccessFDBRasterIcon();
            }
        }

        public new object Object
        {
            get { return null; }
        }

        public int Priority { get { return 1; } }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            return null;
        }

        #endregion

        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            if (parentExObject is DatasetExplorerObject) return true;
            return false;
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            if (!(parentExObject is DatasetExplorerObject))
                return null;

            IFeatureDataset dataset = ((DatasetExplorerObject)parentExObject).Object as IFeatureDataset;
            if (dataset == null || !(dataset.Database is SqlFDB))
                return null;

            FormNewTileGridClass dlg = new FormNewTileGridClass();
            if (dlg.ShowDialog() != DialogResult.OK)
                return null;

            CreateTileGridClass creator = new CreateTileGridClass(
                dlg.GridName,
                dataset, dlg.SpatialIndexDefinition,
                dlg.RasterDataset,
                dlg.TileSizeX, dlg.TileSizeY, dlg.ResolutionX, dlg.ResolutionY, dlg.Levels,
                dlg.TileCacheDirectory,
                dlg.TileGridType);
            creator.CreateTiles = dlg.GenerateTileCache;
            creator.TileLevelType = dlg.TileLevelType;
            creator.CreateLevels = dlg.CreateLevels;

            FormProgress progress = new FormProgress();
            progress.ShowProgressDialog(creator, null, creator.Thread);

            IDatasetElement element = ((IFeatureDataset)parentExObject.Object)[dlg.GridName];
            return new FeatureClassExplorerObject(
                parentExObject as DatasetExplorerObject,
                parentExObject.Name,
                element);
        }

        #endregion
    }
}
