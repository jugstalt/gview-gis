using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.FDB;
using gView.Framework.UI.Dialogs;
using gView.Framework.system;
using gView.Framework.system.UI;
using gView.Framework.Db.UI;
using gView.Framework.Globalisation;
using gView.Framework.Db;
using gView.Framework.Network;
using gView.Framework.UI.Dialogs.Network;
using gView.DataSources.Fdb.MSSql;
using gView.DataSources.Fdb.UI.MSAccess;
using gView.DataSources.Fdb.MSAccess;
using gView.Framework.UI.Controls.Filter;

namespace gView.DataSources.Fdb.UI.MSSql
{
    [gView.Framework.system.RegisterPlugIn("3453D3AA-5A41-4b88-895E-B5DC7CA8B5B5")]
    public class SqlFDBExplorerGroupObject : ExplorerParentObject, IExplorerGroupObject
    {
        private IExplorerIcon _icon = new SqlFDBConnectionsIcon();

        public SqlFDBExplorerGroupObject() : base(null, null, 0) { }

        #region IExplorerGroupObject Members

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        #endregion

        #region IExplorerObject Members

        public string Name
        {
            get { return "MS SQL Server Feature Database Connections"; }
        }

        public string FullName
        {
            get { return "SqlFDBConnections"; }
        }

        public string Type
        {
            get { return "SqlFDB Connections"; }
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
            base.AddChildObject(new SqlFDBNewConnectionObject(this));

            ConfigTextStream stream = new ConfigTextStream("sqlfdb_connections");
            string connStr, id;
            while ((connStr = stream.Read(out id)) != null)
            {
                base.AddChildObject(new SqlFDBExplorerObject(this, id, connStr));
            }
            stream.Close();

            ConfigConnections conStream = new ConfigConnections("sqlfdb", "546B0513-D71D-4490-9E27-94CD5D72C64A");
            Dictionary<string, string> DbConnectionStrings = conStream.Connections;
            foreach (string DbConnName in DbConnectionStrings.Keys)
            {
                DbConnectionString dbConn = new DbConnectionString();
                dbConn.FromString(DbConnectionStrings[DbConnName]);
                base.AddChildObject(new SqlFDBExplorerObject(this, DbConnName, dbConn));
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            if (this.FullName == FullName)
            {
                SqlFDBExplorerGroupObject exObject = new SqlFDBExplorerGroupObject();
                cache.Append(exObject);
                return exObject;
            }

            return null;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("320E7A05-ACC5-4229-A989-2BF6D7CC62BF")]
    public class SqlFDBNewFDBDatabase : ExplorerObjectCls, IExplorerObject, IExplorerObjectCreatable
    {
        private SqlFDBIcon _icon = new SqlFDBIcon();
        private IExplorerObject _parent;

        public SqlFDBNewFDBDatabase()
            : base(null, null, 0)
        {
        }

        #region IExplorerObject Member

        public string Name
        {
            get
            {
                //return "Create new SQL Server Feature database";
                return LocalizedResources.GetResString("ArgString.New", "new SQL Server Feature database", "SQL-Server Feature-Database");
            }
        }

        public string FullName
        {
            get { return ""; }
        }

        public string Type
        {
            get { return "SQL Server Feature database"; }
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
            return (parentExObject is SqlFDBExplorerGroupObject);
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            FormCreateSQLFeatureDatabase dlg = new FormCreateSQLFeatureDatabase();
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

    [gView.Framework.system.RegisterPlugIn("8368C71F-51CE-42E8-B167-BE9AE414C1C8")]
    public class SqlFDBNewConnectionObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDoubleClick, IExplorerObjectCreatable
    {
        private IExplorerIcon _icon = new SqlFDBNewConnectionIcon();

        public SqlFDBNewConnectionObject()
            : base(null, null, 0) { }
        public SqlFDBNewConnectionObject(IExplorerObject parent)
            : base(parent, null, 0) { }

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
            get { return "New SqlFDB Connection"; }
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
            dlg.ProviderID = "mssql";
            dlg.UseProviderInConnectionString = false;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DbConnectionString dbConnStr = dlg.DbConnectionString;
                ConfigConnections connStream = new ConfigConnections("sqlfdb", "546B0513-D71D-4490-9E27-94CD5D72C64A");

                string connectionString = dbConnStr.ConnectionString;
                string id = ConfigTextStream.ExtractValue(connectionString, "database");
                id += "@" + ConfigTextStream.ExtractValue(connectionString, "server");
                if (id == "@") id = "SqlFDB Connection";
                id = connStream.GetName(id);

                connStream.Add(id, dbConnStr.ToString());
                e.NewExplorerObject = new SqlFDBExplorerObject(this.ParentExplorerObject, id, dbConnStr);

                //string connStr = dlg.ConnectionString;
                //ConfigTextStream stream = new ConfigTextStream("sqlfdb_connections", true, true);
                //string id = ConfigTextStream.ExtractValue(connStr, "database");
                //id += "@" + ConfigTextStream.ExtractValue(connStr, "server");
                //if (id == "@") id = "SqlFDB Connection";
                //stream.Write(connStr, ref id);
                //stream.Close();

                //e.NewExplorerObject = new SqlFDBExplorerObject(id, dlg.ConnectionString);
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
            return (parentExObject is SqlFDBExplorerGroupObject);
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();
            ExplorerObjectDoubleClick(e);
            return e.NewExplorerObject;
        }

        #endregion
    }

    public class SqlFDBExplorerObject : ExplorerParentObject, IExplorerSimpleObject, IExplorerObjectDeletable, IExplorerObjectRenamable, IExplorerObjectCommandParameters, ISerializableExplorerObject, IExplorerObjectContextMenu
    {
        private IExplorerIcon _icon = new SqlFDBIcon();
        private string _server = "", _connectionString = "", _errMsg = "";
        private ToolStripItem[] _contextItems = null;
        private DbConnectionString _dbConnectionString = null;

        public SqlFDBExplorerObject()
            : base(null, null, 1) { }
        public SqlFDBExplorerObject(IExplorerObject parent, string server, string connectionString)
            : base(parent, null, 1)
        {
            _server = server;
            _connectionString = connectionString;

            _contextItems = new ToolStripItem[1];
            _contextItems[0] = new ToolStripMenuItem("Tasks");

            ToolStripMenuItem item = new ToolStripMenuItem("Shrink database");
            item.Click += new EventHandler(ShrinkDatabase_Click);
            ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(item);
            ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(new ToolStripSeparator());

            item = new ToolStripMenuItem("Check SpatialEngine Version");
            item.Click += new EventHandler(CheckSpatialEngineVersion_Click);
            ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(item);
            item = new ToolStripMenuItem("Create Spatial Engine");
            item.Click += new EventHandler(CreateSpatialEngine_Click);
            ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(item);
            item = new ToolStripMenuItem("Drop Spatial Engine");
            item.Click += new EventHandler(DropSpatialEngine_Click);
            ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(item);
        }
        public SqlFDBExplorerObject(IExplorerObject parent, string server, DbConnectionString dbConnectionString)
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

        void CheckSpatialEngineVersion_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    DataTable tab = new DataTable("ASSEMBLIES");
                    SqlDataAdapter adapter = new SqlDataAdapter("select * from sys.assemblies where name='MSSqlSpatialEngine'", connection);
                    adapter.Fill(tab);

                    if (tab.Rows.Count == 0)
                    {
                        MessageBox.Show("SpatialEngine is not installed for this database...");
                        return;
                    }
                    adapter.Dispose();

                    string clr_name = tab.Rows[0]["clr_name"].ToString().Replace(",", ";");
                    string version = ConfigTextStream.ExtractValue(clr_name, "version");
                    MessageBox.Show("SpatialEngine is installed on this database.\nVersion: " + version);

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
            }
        }

        private bool ExecuteQueries(SqlConnection connection, StreamReader reader)
        {
            string line = "";
            StringBuilder sql = new StringBuilder();
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Trim().ToLower() == "go")
                {
                    SqlCommand command = new SqlCommand(sql.ToString(), connection);
                    try
                    {
                        command.ExecuteNonQuery();
                        sql = new StringBuilder();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return false;
                    }
                }
                else
                {
                    sql.Append(line + " ");
                }
            }
            return true;
        }

        void DropSpatialEngine_Click(object sender, EventArgs e)
        {
            SqlCommandInterpreter interpreter = new SqlCommandInterpreter(_connectionString, SystemVariables.ApplicationDirectory + @"\sql\sqlFDB\dropSpatialEngine.sql");
            Thread thread = new Thread(new ParameterizedThreadStart(interpreter.Exectue));
            SqlCommandProgressReporter reporter = new SqlCommandProgressReporter(interpreter);

            FormProgress progress = new FormProgress(reporter, thread, null);
            progress.Text = "Execute Commands:";
            progress.Mode = ProgressMode.ProgressDisk;
            progress.ShowDialog();
        }

        void CreateSpatialEngine_Click(object sender, EventArgs e)
        {
            DropSpatialEngine_Click(sender, e);

            SqlCommandInterpreter interpreter = new SqlCommandInterpreter(_connectionString, SystemVariables.ApplicationDirectory + @"\sql\sqlFDB\createSpatialEngine.sql");
            Thread thread = new Thread(new ParameterizedThreadStart(interpreter.Exectue));
            SqlCommandProgressReporter reporter = new SqlCommandProgressReporter(interpreter);

            FormProgress progress = new FormProgress(reporter, thread, null);
            progress.Text = "Execute Commands:";
            progress.Mode = ProgressMode.ProgressDisk;
            progress.ShowDialog();
        }

        void ShrinkDatabase_Click(object sender, EventArgs e)
        {
            SqlCommandInterpreter interpreter = new SqlCommandInterpreter(_connectionString, SystemVariables.ApplicationDirectory + @"\sql\sqlFDB\shrinkDatabase.sql");
            interpreter.DatabaseName = ConfigTextStream.ExtractValue(_connectionString, "database");
            Thread thread = new Thread(new ParameterizedThreadStart(interpreter.Exectue));
            SqlCommandProgressReporter reporter = new SqlCommandProgressReporter(interpreter);

            FormProgress progress = new FormProgress(reporter, thread, null);
            progress.Text = "Execute Commands:";
            progress.Mode = ProgressMode.ProgressDisk;
            progress.ShowDialog();
        }

        void ConnectionProperties_Click(object sender, EventArgs e)
        {
            if (_dbConnectionString == null) return;

            FormConnectionString dlg = new FormConnectionString(_dbConnectionString);
            dlg.ProviderID = "mssql";
            dlg.UseProviderInConnectionString = false;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                DbConnectionString dbConnStr = dlg.DbConnectionString;

                ConfigConnections connStream = new ConfigConnections("sqlfdb", "546B0513-D71D-4490-9E27-94CD5D72C64A");
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
                return @"SqlFDBConnections\" + _server;
            }
        }

        public string Type
        {
            get { return "Sql Feature Database"; }
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
                    SqlFDB fdb = new SqlFDB();
                    if (!fdb.Open(_connectionString))
                    {
                        _errMsg = fdb.lastErrorMsg;
                        return null;
                    }
                    if (fdb.lastException != null)
                    {
                        FormException dlg = new FormException(fdb.lastException);
                        dlg.ShowDialog();
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
                    if (dsname == "") continue;
                    base.AddChildObject(new SqlFDBDatasetExplorerObject(this, dsname));
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

            SqlFDBExplorerGroupObject group = new SqlFDBExplorerGroupObject();
            if (FullName.IndexOf(group.FullName) != 0 || FullName.Length < group.FullName.Length + 2) return null;

            group = (SqlFDBExplorerGroupObject)((cache.Contains(group.FullName)) ? cache[group.FullName] : group);

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
                ConfigConnections stream = new ConfigConnections("sqlfdb", "546B0513-D71D-4490-9E27-94CD5D72C64A");
                stream.Remove(_server);
            }
            else
            {
                ConfigTextStream stream = new ConfigTextStream("sqlfdb_connections", true, true);
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
                ConfigConnections stream = new ConfigConnections("sqlfdb", "546B0513-D71D-4490-9E27-94CD5D72C64A");
                ret = stream.Rename(_server, newName);
            }
            else
            {
                ConfigTextStream stream = new ConfigTextStream("sqlfdb_connections", true, true);
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

    internal class SqlCommandInterpreter
    {
        public delegate void ExecuteCommandEventHandler(string command);
        public event ExecuteCommandEventHandler ExecuteCommand;

        private string _connectionString = "";
        private string _path;
        private CancelTracker _cancelTracker = new CancelTracker();
        private string _database = "";

        public SqlCommandInterpreter(string connectionString, string path)
        {
            _connectionString = connectionString;
            _path = path;
        }

        public string DatabaseName
        {
            get { return _database; }
            set { _database = value; }
        }
        public ICancelTracker CancelTracker
        {
            get { return _cancelTracker; }
        }
        public void Exectue(object parameter)
        {
            try
            {
                StreamReader sr = new StreamReader(_path);
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    ExecuteQueries(connection, sr);
                    connection.Close();
                }
                sr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool ExecuteQueries(SqlConnection connection, StreamReader reader)
        {
            string line = "";
            StringBuilder sql = new StringBuilder();
            while ((line = reader.ReadLine()) != null)
            {
                if (!_cancelTracker.Continue) return false;
                if (line.Trim().ToLower() == "go")
                {
                    SqlCommand command = new SqlCommand(sql.ToString().Replace("#FDB#", _database), connection);
                    command.CommandTimeout = 0;
                    try
                    {
                        if (ExecuteCommand != null) ExecuteCommand(command.CommandText);
                        command.ExecuteNonQuery();
                        sql = new StringBuilder();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return false;
                    }
                }
                else
                {
                    sql.Append(line + " ");
                }
            }
            return true;
        }
    }
    internal class SqlCommandProgressReporter : IProgressReporter
    {
        private ProgressReport _report = new ProgressReport();
        private ICancelTracker _cancelTracker = null;
        private int _progress = 0;

        public SqlCommandProgressReporter(SqlCommandInterpreter interpreter)
        {
            if (interpreter == null) return;
            _cancelTracker = interpreter.CancelTracker;

            interpreter.ExecuteCommand += new SqlCommandInterpreter.ExecuteCommandEventHandler(interpreter_ExecuteCommand);
        }

        void interpreter_ExecuteCommand(string command)
        {
            if (ReportProgress == null) return;

            _report.Message = command.Substring(0, Math.Min(255, command.Length));
            _report.featurePos++;
            ReportProgress(_report);
        }

        #region IProgressReporter Member

        public event ProgressReporterEvent ReportProgress;

        public ICancelTracker CancelTracker
        {
            get { return _cancelTracker; }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("42610C64-F3A9-4241-A7A8-1FEE4A4FE9FE")]
    public class SqlFDBDatasetExplorerObject : ExplorerObjectFeatureClassImport, IExplorerSimpleObject, IExplorerObjectDeletable, ISerializableExplorerObject, IExplorerObjectCreatable, IExplorerObjectContextMenu, IExplorerObjectRenamable
    {
        private IExplorerIcon _icon = null;
        private SqlFDBExplorerObject _parent = null;
        private ToolStripItem[] _contextItems = null;
        private bool _isImageDataset = false;
        //private SqlFDBDataset _dataset = null;

        public SqlFDBDatasetExplorerObject() : base(null, typeof(SqlFDBDataset)) { }
        public SqlFDBDatasetExplorerObject(SqlFDBExplorerObject parent, string dsname)
            : base(parent, typeof(SqlFDBDataset))
        {
            _parent = parent;

            if (dsname.IndexOf("#") == 0)
            {
                _isImageDataset = true;
                dsname = dsname.Substring(1, dsname.Length - 1);
                _icon = new AccessFDBImageDatasetIcon();
            }
            else
            {
                _isImageDataset = false;
                _icon = new AccessFDBDatasetIcon();
            }

            _dsname = dsname;

            _dataset = new SqlFDBDataset();
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
                if (_parent == null) return "";
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
                if (_parent == null) return "";
                return _parent.FullName + @"\" + this.Name;
            }
        }
        public string Type
        {
            get { return "Sql Feature Database Dataset"; }
        }
        public IExplorerIcon Icon
        {
            get
            {
                if (_icon == null)
                    return new AccessFDBDatasetIcon();
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

            _dataset = new SqlFDBDataset();
            _dataset.ConnectionString = this.ConnectionString + ";dsname=" + _dsname;

            if (_dataset.Open())
            {
                foreach (IDatasetElement element in _dataset.Elements)
                {
                    base.AddChildObject(new SqlFDBFeatureClassExplorerObject(this, _dsname, element));
                }
            }
            _fdb = (SqlFDB)_dataset.Database;
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

            SqlFDBExplorerObject fdbObject = new SqlFDBExplorerObject();
            fdbObject = (SqlFDBExplorerObject)fdbObject.CreateInstanceByFullName(fdbName, cache);
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
            if (parentExObject is SqlFDBExplorerObject) return true;

            return false;
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            if (!CanCreate(parentExObject)) return null;

            SqlFDB fdb = new SqlFDB();
            fdb.Open(((SqlFDBExplorerObject)parentExObject).ConnectionString);

            FormNewDataset dlg = new FormNewDataset();
            if (fdb.FdbVersion >= new Version(1, 2, 0))
                dlg.ShowSpatialIndexTab = true;

            Version sqlVersion = fdb.SqlServerVersion;
            if (fdb.SqlServerVersion < new Version(10, 0))
            {
                dlg.IndexTypeIsEditable = false;
            }

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

            return new SqlFDBDatasetExplorerObject((SqlFDBExplorerObject)parentExObject, datasetName);
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

            if (_dataset == null || !(_dataset.Database is AccessFDB))
            {
                MessageBox.Show("Can't rename dataset...\nUncorrect dataset !!!");
                return false;
            }
            if (!((AccessFDB)_dataset.Database).RenameDataset(this.Name, newName))
            {
                MessageBox.Show("Can't rename dataset...\n" + ((AccessFDB)_dataset.Database).lastErrorMsg);
                return false;
            }

            _dsname = newName;

            if (ExplorerObjectRenamed != null) ExplorerObjectRenamed(this);
            return true;
        }

        #endregion

        public override void Content_DragEnter(DragEventArgs e)
        {
            if (_icon is AccessFDBImageDatasetIcon)
            {
                e.Effect = DragDropEffects.None;
            }
            else
            {
                base.Content_DragEnter(e);
            }
        }
    }


    [gView.Framework.system.RegisterPlugIn("FE6E1EA7-1300-400c-8674-68465859E991")]
    public class SqlFDBFeatureClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDeletable, ISerializableExplorerObject, IExplorerObjectContextMenu, IExplorerObjectRenamable, IExplorerObjectCreatable, IExporerOjectSchema
    {
        private interface _IMClass : IFeatureClass, IRasterClass { }

        private string _dsname = "", _fcname = "", _type = "";
        private IExplorerIcon _icon = null;
        private IFeatureClass _fc = null;
        private IRasterClass _rc = null;
        private SqlFDBDatasetExplorerObject _parent = null;
        private ToolStripItem[] _contextItems = null;
        private bool _isNetwork = false;

        public SqlFDBFeatureClassExplorerObject() : base(null, typeof(_IMClass), 1) { }
        public SqlFDBFeatureClassExplorerObject(SqlFDBDatasetExplorerObject parent, string dsname, IDatasetElement element)
            : base(parent, typeof(_IMClass), 1)
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
                        _icon = new AccessFDBNetworkIcon();
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
            get { return _type != "" ? _type : "SqlFDB Featureclass"; }
        }

        public IExplorerIcon Icon
        {
            get
            {
                if (_icon == null)
                    return new AccessFDBPolygonIcon();
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

            SqlFDBDatasetExplorerObject dsObject = new SqlFDBDatasetExplorerObject();
            dsObject = (SqlFDBDatasetExplorerObject)dsObject.CreateInstanceByFullName(dsName, cache);
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
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is AccessFDB))
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
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is AccessFDB))
            {
                MessageBox.Show("Can't show spatial index definition...\nUncorrect feature class !!!");
                return;
            }

            FormRebuildSpatialIndexDef dlg = new FormRebuildSpatialIndexDef((AccessFDB)_fc.Dataset.Database, _fc);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
            }
        }

        void RepairSpatialIndex_Click(object sender, EventArgs e)
        {
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is AccessFDB))
            {
                MessageBox.Show("Can't show spatial index definition...\nUncorrect feature class !!!");
                return;
            }

            FormRepairSpatialIndexProgress dlg = new FormRepairSpatialIndexProgress((AccessFDB)_fc.Dataset.Database, _fc);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
            }
        }

        void Truncate_Click(object sender, EventArgs e)
        {
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is AccessFDB))
            {
                MessageBox.Show("Can't rebuild index...\nUncorrect feature class !!!");
                return;
            }

            ((AccessFDB)_fc.Dataset.Database).TruncateTable(_fc.Name);
            _fc.Dataset.RefreshClasses();
        }

        void Altertable_Click(object sender, EventArgs e)
        {
            //if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is AccessFDB))
            //{
            //    MessageBox.Show("Can't rebuild index...\nUncorrect feature class !!!");
            //    return;
            //}

            //if (_fc == null) return;
            //FormNewFeatureclass dlg = new FormNewFeatureclass(_fc);
            //if(dlg.ShowDialog()!=DialogResult.OK) return;

            //((AccessFDB)_fc.Dataset.Database).AlterTable(_fc.Name, dlg.Fields);
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
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is AccessFDB))
            {
                MessageBox.Show("Can't rename featureclass...\nUncorrect feature class !!!");
                return false;
            }

            if (!((AccessFDB)_fc.Dataset.Database).RenameFeatureClass(this.Name, newName))
            {
                MessageBox.Show("Can't rename featureclass...\n" + ((AccessFDB)_fc.Dataset.Database).lastErrorMsg);
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
            if (parentExObject is SqlFDBDatasetExplorerObject &&
                !((SqlFDBDatasetExplorerObject)parentExObject).IsImageDataset) return true;
            return false;
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            if (!CanCreate(parentExObject)) return null;

            if (!(parentExObject.Object is IFeatureDataset) || !(((IDataset)parentExObject.Object).Database is SqlFDB))
            {
                return null;
            }
            SqlFDB fdb = ((IDataset)parentExObject.Object).Database as SqlFDB;

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
            if (fdb is SqlFDB &&
                (sIndexDef.GeometryType == GeometryFieldType.MsGeometry ||
                 sIndexDef.GeometryType == GeometryFieldType.MsGeography))
            {
                MSSpatialIndex index = dlg.MSSpatialIndexDef;
                ((SqlFDB)fdb).SetMSSpatialIndex(index, dlg.FeatureclassName);
            }
            else
            {
                fdb.SetSpatialIndexBounds(dlg.FeatureclassName, "BinaryTree2", dlg.SpatialIndexExtents, 0.55, 200, dlg.SpatialIndexLevels);
            }

            IDatasetElement element = ((IFeatureDataset)parentExObject.Object)[dlg.FeatureclassName];
            return new SqlFDBFeatureClassExplorerObject(
                parentExObject as SqlFDBDatasetExplorerObject,
                parentExObject.Name,
                element);
        }

        #endregion

        #region IExporerOjectSchema Member

        public string Schema
        {
            get
            {
                if (_fc is SqlFDBFeatureClass)
                    return ((SqlFDBFeatureClass)_fc).DbSchema;

                return String.Empty;
            }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("97914E6A-3084-4fc0-8B31-4A6D2C990F72")]
    public class SqlFDBNetworkClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectCreatable
    {
        public SqlFDBNetworkClassExplorerObject() : base(null, typeof(SqlFDBNetworkFeatureclass), 0) { }

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
            get { return new AccessFDBNetworkIcon(); }
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
            if (parentExObject is SqlFDBDatasetExplorerObject) return true;
            return false;
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            if (!(parentExObject is SqlFDBDatasetExplorerObject))
                return null;

            IFeatureDataset dataset = ((SqlFDBDatasetExplorerObject)parentExObject).Object as IFeatureDataset;
            if (dataset == null || !(dataset.Database is SqlFDB))
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
            return new SqlFDBFeatureClassExplorerObject(
                parentExObject as SqlFDBDatasetExplorerObject,
                parentExObject.Name,
                element);
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("11D0739F-66FC-4ca7-BA58-887DBB6F088C")]
    public class SqlFDBGeographicViewExplorerObject : IExplorerSimpleObject, IExplorerObjectCreatable
    {
        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            return parentExObject is SqlFDBDatasetExplorerObject;
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            SqlFDBDatasetExplorerObject parent = (SqlFDBDatasetExplorerObject)parentExObject;

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
                return new SqlFDBFeatureClassExplorerObject(
                    parentExObject as SqlFDBDatasetExplorerObject,
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

    [gView.Framework.system.RegisterPlugIn("19AF8E6C-8324-4290-AF7C-5B19E31A952E")]
    public class SqlFDBLinkedFeatureclassExplorerObject : IExplorerSimpleObject, IExplorerObjectCreatable
    {
        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            return parentExObject is SqlFDBDatasetExplorerObject;
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            SqlFDBDatasetExplorerObject parent = (SqlFDBDatasetExplorerObject)parentExObject;

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
                        if (fcid < 0)
                        {
                            MessageBox.Show(fdb.lastErrorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            continue;
                        }
                        if (ret == null)
                        {
                            IDatasetElement element = dataset[((IFeatureClass)exObj.Object).Name];
                            if (element != null)
                            {
                                ret = new SqlFDBFeatureClassExplorerObject(
                                    parentExObject as SqlFDBDatasetExplorerObject,
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

    //[gView.Framework.system.RegisterPlugIn("C3D1F9CA-69B5-46e9-B4DB-05534512F8B9")]
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
            get { return new AccessFDBRasterIcon(); }
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
            if (parentExObject is SqlFDBDatasetExplorerObject) return true;
            return false;
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            if (!(parentExObject is SqlFDBDatasetExplorerObject))
                return null;

            IFeatureDataset dataset = ((SqlFDBDatasetExplorerObject)parentExObject).Object as IFeatureDataset;
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
            return new SqlFDBFeatureClassExplorerObject(
                parentExObject as SqlFDBDatasetExplorerObject,
                parentExObject.Name,
                element);
        }

        #endregion
    }

    class SqlFDBConnectionsIcon : IExplorerIcon
    {
        System.Drawing.Image _image = (new Icons()).imageList1.Images[0];
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("3453D3AA-5A41-4b88-895E-B5DC7CA8B5B5");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return _image;
            }
        }

        #endregion
    }

    class SqlFDBNewConnectionIcon : IExplorerIcon
    {
        System.Drawing.Image _image = (new Icons()).imageList1.Images[1];
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("BBBB7574-D57E-40e7-AF1F-885F24FDCF27");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return _image;
            }
        }

        #endregion
    }

    class SqlFDBIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("7FEC121F-1BFE-4fd4-A697-E084EB733605");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return (new Icons()).imageList1.Images[2];
            }
        }

        #endregion
    }

    class SqlFDBIcon2 : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("25C07246-4C1C-4652-9E97-C99273A7CC25");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return (new Icons()).imageList1.Images[8];
            }
        }

        #endregion
    }
}
