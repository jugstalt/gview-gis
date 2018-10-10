using System;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using gView.Framework.IO;
using gView.Framework.Geometry;
using gView.Framework.Data;
using gView.Framework.Carto;
using gView.Framework.FDB;
using gView.Framework.system;
using System.Reflection;

namespace gView.DataSources.Fdb.MSSql
{
    /// <summary>
    /// Zusammenfassung für SqlFDBDataset.
    /// </summary>
    [gView.Framework.system.RegisterPlugIn("3B870AB5-8BE0-4a00-911D-ECC6C83DD6B4")]
    public class SqlFDBDataset : DatasetMetadata, IFeatureDataset2, IRasterDataset, IFDBDataset, IPersistable, gView.Framework.UI.IConnectionStringDialog
    {
        private int _order = 0;
        internal int _dsID = -1;
        private List<IDatasetElement> _layers;
        private string _errMsg = "";
        private string _dsname = "";
        internal SqlFDB _fdb = null;
        private string _connStr = "";
        private List<string> _addedLayers;
        private ISpatialReference _sRef = null;
        private DatasetState _state = DatasetState.unknown;
        private ISpatialIndexDef _sIndexDef = new gViewSpatialIndexDef();

        public SqlFDBDataset()
        {
            _addedLayers = new List<string>();
            _fdb = new SqlFDB();
        }
        internal SqlFDBDataset(SqlFDB fdb, string dsname)
            : this()
        {
            ConnectionString = fdb.ConnectionString + ";dsname=" + dsname;
            Open();
        }

        ~SqlFDBDataset()
        {
            this.Dispose();
        }
        public void Dispose()
        {
            if (_fdb != null)
            {
                _fdb.Dispose();
                _fdb = null;
            }
        }

        /*
        public ILayer getLayer(string id) 
        {
            foreach(ILayer layer in _layers) 
            {
                if(layer.id==id) return layer;
            }
            return null;
        }
        */

        #region IFeatureDataset Member

        public List<IDatasetElement> Elements
        {
            get
            {
                if (_layers == null || _layers.Count == 0)
                {
                    _layers = _fdb.DatasetLayers(this);

                    if (_layers != null && _addedLayers.Count != 0)
                    {
                        List<IDatasetElement> list = new List<IDatasetElement>();
                        foreach (IDatasetElement element in _layers)
                        {
                            if (element is IFeatureLayer)
                            {
                                if (_addedLayers.Contains(((IFeatureLayer)element).FeatureClass.Name))
                                    list.Add(element);
                            }
                            else if (element is IRasterLayer)
                            {
                                if (_addedLayers.Contains(((IRasterLayer)element).Title))
                                    list.Add(element);
                            }
                            else
                            {
                                if (_addedLayers.Contains(element.Title))
                                    list.Add(element);
                            }
                        }
                        _layers = list;
                    }

                }

                if (_layers == null) return new List<IDatasetElement>();
                return _layers;
            }
        }

        public bool renderImage(IDisplay display)
        {
            return false;
        }

        public System.Drawing.Image Bitmap
        {
            get
            {
                return null;
            }
        }

        public bool canRenderImage
        {
            get
            {
                return false;
            }
        }

        public IEnvelope Envelope
        {
            get
            {
                if (_layers == null) return null;

                bool first = true;
                IEnvelope env = null;

                foreach (IDatasetElement layer in _layers)
                {
                    IEnvelope envelope = null;
                    if (layer.Class is IFeatureClass)
                    {
                        envelope = ((IFeatureClass)layer.Class).Envelope;
                        if (envelope.Width > 1e10 && ((IFeatureClass)layer.Class).CountFeatures == 0)
                            envelope = null;
                    }
                    else if (layer.Class is IRasterClass)
                    {
                        if (((IRasterClass)layer.Class).Polygon == null) continue;
                        envelope = ((IRasterClass)layer.Class).Polygon.Envelope;
                    }
                    if (gView.Framework.Geometry.Envelope.IsNull(envelope)) continue;

                    if (first)
                    {
                        first = false;
                        env = new Envelope(envelope);
                    }
                    else
                    {
                        env.Union(envelope);
                    }
                }
                return env;
            }
        }

        public ISpatialReference SpatialReference
        {
            get
            {
                if (_sRef != null) return _sRef;

                if (_fdb == null) return null;
                return _sRef = _fdb.SpatialReference(_dsname);
            }
            set
            {
                // Nicht in Databank übernehmen !!!
                _sRef = value;
            }
        }
        #endregion

        #region IDataset Member

        public string DatasetName
        {
            get
            {
                return _dsname;
            }
        }

        public string ConnectionString
        {
            get
            {
                if (_fdb == null) return "";
                string c = _fdb._conn.ConnectionString + ";" +
                    "dsname=" + ConfigTextStream.ExtractValue(_connStr, "dsname") + ";";

                while (c.IndexOf(";;") != -1)
                    c = c.Replace(";;", ";");

                /*
                if (_layers != null && _layers.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (IDatasetElement element in _layers)
                    {
                        if (sb.Length != 0) sb.Append("@");
                        sb.Append(element.Title);
                    }
                    c += "layers=" + sb.ToString();
                }
                else
                {
                    //c += "layers=" + ConfigTextStream.ExtractValue(_connStr, "layers");
                }
                 * */
                return c;
            }
            set
            {
                _connStr = value;
                if (value == null) return;
                while (_connStr.IndexOf(";;") != -1)
                    _connStr = _connStr.Replace(";;", ";");

                _dsname = ConfigTextStream.ExtractValue(value, "dsname");
                _addedLayers.Clear();
                foreach (string layername in ConfigTextStream.ExtractValue(value, "layers").Split('@'))
                {
                    if (layername == "") continue;
                    if (_addedLayers.IndexOf(layername) != -1) continue;
                    _addedLayers.Add(layername);
                }
                if (_fdb == null) _fdb = new SqlFDB();

                _fdb.DatasetRenamed += new gView.DataSources.Fdb.MSAccess.AccessFDB.DatasetRenamedEventHandler(SqlFDB_DatasetRenamed);
                _fdb.FeatureClassRenamed += new gView.DataSources.Fdb.MSAccess.AccessFDB.FeatureClassRenamedEventHandler(SqlFDB_FeatureClassRenamed);
                _fdb.TableAltered += new gView.DataSources.Fdb.MSAccess.AccessFDB.AlterTableEventHandler(SqlFDB_TableAltered);
                _fdb.Open(_connStr);
            }
        }

        public DatasetState State
        {
            get { return _state; }
        }

        public bool Open()
        {
            if (_fdb == null) return false;

            _dsID = _fdb.DatasetID(_dsname);
            if (_dsID < 0) return false;

            _sRef = this.SpatialReference;
            _state = DatasetState.opened;
            _sIndexDef = _fdb.SpatialIndexDef(_dsID);

            return true;
        }

        public string lastErrorMsg
        {
            get
            {
                return _errMsg;
            }
        }

        public string DatasetGroupName
        {
            get
            {
                return "Sql FDB";
            }
        }

        public string ProviderName
        {
            get
            {
                return "SQL Feature Database";
            }
        }

        public string Query_FieldPrefix { get { return "["; } }
        public string Query_FieldPostfix { get { return "]"; } }

        public IDatasetElement this[string DatasetElementTitle]
        {
            get
            {
                if (_fdb == null) return null;
                IDatasetElement element = _fdb.DatasetElement(this, DatasetElementTitle);

                if (element != null && element.Class is SqlFDBFeatureClass)
                {
                    ((SqlFDBFeatureClass)element.Class).SpatialReference = _sRef;
                }

                return element;
            }
        }

        public IDatabase Database
        {
            get { return _fdb; }
        }

        public void RefreshClasses()
        {
            if (_fdb != null)
                _fdb.RefreshClasses(this);
        }
        #endregion

        #region IPersistable Member

        public string PersistID
        {
            get
            {
                return null;
            }
        }

        public void Load(IPersistStream stream)
        {
            string connectionString = (string)stream.Load("connectionstring", "");
            if (_fdb != null)
            {
                _fdb.Dispose();
                _fdb = null;
            }

            this.ConnectionString = connectionString;
            this.Open();
        }

        public void Save(IPersistStream stream)
        {
            stream.SaveEncrypted("connectionstring", this.ConnectionString);
        }

        #endregion

        #region IDataset2 Member

        public IDataset2 EmptyCopy()
        {
            SqlFDBDataset dataset = new SqlFDBDataset();
            dataset.ConnectionString = ConnectionString;
            dataset.Open();

            return dataset;
        }

        public void AppendElement(string elementName)
        {
            if (_layers == null) _layers = new List<IDatasetElement>();

            foreach (IDatasetElement e in _layers)
            {
                if (e.Title == elementName) return;
            }

            IDatasetElement element = _fdb.DatasetElement(this, elementName);
            if (element != null) _layers.Add(element);
        }

        #endregion

        void SqlFDB_FeatureClassRenamed(string oldName, string newName)
        {
            if (_layers == null) return;

            foreach (IDatasetElement element in _layers)
            {
                if (element.Class is SqlFDBFeatureClass &&
                    ((SqlFDBFeatureClass)element.Class).Name == oldName)
                {
                    ((SqlFDBFeatureClass)element.Class).Name = newName;
                }
            }
        }

        void SqlFDB_TableAltered(string table)
        {
            if (_layers == null) return;

            foreach (IDatasetElement element in _layers)
            {
                if (element.Class is SqlFDBFeatureClass &&
                    ((SqlFDBFeatureClass)element.Class).Name == table)
                {
                    var fields = _fdb.FeatureClassFields(this._dsID, table);

                    SqlFDBFeatureClass fc = element.Class as SqlFDBFeatureClass;
                    ((Fields)fc.Fields).Clear();

                    foreach (IField field in fields)
                    {
                        ((Fields)fc.Fields).Add(field);
                    }
                }
            }
        }

        void SqlFDB_DatasetRenamed(string oldName, string newName)
        {
            if (_dsname == oldName)
                _dsname = newName;
        }

        #region IFDBDataset Member

        public ISpatialIndexDef SpatialIndexDef
        {
            get { return _sIndexDef; }
        }

        #endregion

        #region IConnectionStringDialog Member

        public string ShowConnectionStringDialog(string initConnectionString)
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.DataSources.Fdb.UI.dll");

            gView.Framework.UI.IConnectionStringDialog p = uiAssembly.CreateInstance("gView.DataSources.Fdb.UI.MSSql.SqlFdbConnectionStringDialog") as gView.Framework.UI.IConnectionStringDialog;
            if (p != null)
                return p.ShowConnectionStringDialog(initConnectionString);

            return null;
        }

        #endregion
    }
}
