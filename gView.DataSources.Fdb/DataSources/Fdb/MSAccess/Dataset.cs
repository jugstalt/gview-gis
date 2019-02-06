using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using gView.Framework;
using gView.Framework.Geometry;
using gView.Framework.Data;
using gView.Framework.Carto;
using gView.Framework.IO;
using gView.Framework.FDB;
using gView.Framework.system;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.MSAccess
{
    /// <summary>
    /// Zusammenfassung für AccessFDBDataset.
    /// </summary>
    [gView.Framework.system.RegisterPlugIn("6F540340-74C9-4b1a-BD4D-B1C5FE946CA1")]
    public class AccessFDBDataset : DatasetMetadata, IFeatureDataset2, IRasterDataset, IFDBDataset, IPersistable
    {
        internal int _dsID = -1;
        private List<IDatasetElement> _layers;
        private string _errMsg = "";
        private string _connString = "";
        private string _dsname = "";
        internal AccessFDB _fdb = null;
        private List<string> _addedLayers;
        private ISpatialReference _sRef = null;
        private DatasetState _state = DatasetState.unknown;
        private ISpatialIndexDef _sIndexDef = new gViewSpatialIndexDef();

        public AccessFDBDataset(AccessFDB fdb)
        {
            _addedLayers = new List<string>();
            _fdb = fdb;
        }
        internal AccessFDBDataset(AccessFDB fdb, string dsname, ISpatialIndexDef sIndexDef)
            : this(fdb)
        {
            if (fdb == null)
                return;

            ConnectionString = "mdb=" + fdb.ConnectionString + ";dsname=" + dsname;
            Open();
        }

        ~AccessFDBDataset()
        {
            this.Dispose();
        }
        public void Dispose()
        {
            //System.Windows.Forms.MessageBox.Show("Dataset Dispose "+this.DatasetName);
            if (_fdb != null)
            {
                _fdb.Dispose();
                _fdb = null;
            }
        }

        #region IFeatureDataset Member

        public List<IDatasetElement> Elements
        {
            get
            {
                if (_fdb == null) return new List<IDatasetElement>();

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

        public bool canRenderImage
        {
            get
            {
                return false;
            }
        }
        public bool renderImage(IDisplay display)
        {
            return false;
        }

        public bool renderLayer(IDisplay display, ILayer layer)
        {
            return false;
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
                    if (!(layer.Class is IFeatureClass)) continue;
                    IEnvelope envelope = ((IFeatureClass)layer.Class).Envelope;
                    if (envelope.Width > 1e10 && ((IFeatureClass)layer.Class).CountFeatures == 0)
                        envelope = null;

                    if (gView.Framework.Geometry.Envelope.IsNull(envelope)) continue;

                    if (first)
                    {
                        first = false;
                        env = new Envelope(((IFeatureClass)layer.Class).Envelope);
                    }
                    else
                    {
                        env.Union(((IFeatureClass)layer.Class).Envelope);
                    }
                }
                return env;
            }
        }

        /*
        public string imageUrl
        {
            get
            {
                return "";
            }
            set
            {
				
            }
        }

        public string imagePath
        {
            get
            {
                return _imagePath;
            }
            set
            {
            }
        }

        */
        public System.Drawing.Image Bitmap
        {
            get
            {
                return null;
            }
        }
        public ISpatialReference SpatialReference
        {
            get
            {
                return _sRef;

                // wird sonst Zach, wenns bei jedem Bildaufbau von jedem Thema gelesen werden muß...
                //if(_sRef!=null) return _sRef;

                //if(_fdb==null) return null;
                //return _sRef=_fdb.SpatialReference(_dsname);
            }
            set
            {
                // nicht in DB schreiben!!

                _sRef = value;
            }
        }

        #endregion

        #region IDataset Member

        public IDatasetEnum DatasetEnum
        {
            get
            {
                return null;
            }
        }

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
                string c = "mdb=" + ConfigTextStream.ExtractValue(_connString, "mdb") + ";" +
                    "dsname=" + ConfigTextStream.ExtractValue(_connString, "dsname") + ";";

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
                    //c += "layers=" + ConfigTextStream.ExtractValue(_connString, "layers");
                }
                 * */
                return c;
            }
            set
            {
                _connString = value;
                if (value == null) return;
                while (_connString.IndexOf(";;") != -1)
                    _connString = _connString.Replace(";;", ";");

                _dsname = ConfigTextStream.ExtractValue(value, "dsname");
                _addedLayers.Clear();
                foreach (string layername in ConfigTextStream.ExtractValue(value, "layers").Split('@'))
                {
                    if (layername == "") continue;
                    if (_addedLayers.IndexOf(layername) != -1) continue;
                    _addedLayers.Add(layername);
                }

                if (_fdb == null)
                    throw new NullReferenceException("_fdb==null");

                _fdb.DatasetRenamed += new AccessFDB.DatasetRenamedEventHandler(AccessFDB_DatasetRenamed);
                _fdb.FeatureClassRenamed += new AccessFDB.FeatureClassRenamedEventHandler(AccessFDB_FeatureClassRenamed);
                _fdb.TableAltered += new AccessFDB.AlterTableEventHandler(SqlFDB_TableAltered);
                _fdb.Open(ConfigTextStream.ExtractValue(_connString, "mdb"));
            }
        }

        public DatasetState State
        {
            get { return _state; }
        }

        public bool Open()
        {
            if (_connString == null || _connString == "" ||
                _dsname == null || _dsname == "" || _fdb == null) return false;

            _dsID = _fdb.DatasetID(_dsname);
            if (_dsID == -1) return false;

            _sRef = _fdb.SpatialReference(_dsname);
            _sIndexDef = _fdb.SpatialIndexDef(_dsID);

            _state = DatasetState.opened;
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
                return "AccessFDB";
            }
        }

        public string ProviderName
        {
            get
            {
                return "Access Feature Database";
            }
        }

        public string Query_FieldPrefix { get { return "["; } }
        public string Query_FieldPostfix { get { return "]"; } }

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

        void AccessFDB_FeatureClassRenamed(string oldName, string newName)
        {
            if (_layers == null) return;

            foreach (IDatasetElement element in _layers)
            {
                if (element.Class is AccessFDBFeatureClass &&
                    ((AccessFDBFeatureClass)element.Class).Name == oldName)
                {
                    ((AccessFDBFeatureClass)element.Class).Name = newName;
                }
            }
        }

        void SqlFDB_TableAltered(string table)
        {
            if (_layers == null) return;

            foreach (IDatasetElement element in _layers)
            {
                if (element.Class is AccessFDBFeatureClass &&
                    ((AccessFDBFeatureClass)element.Class).Name == table)
                {
                    var fields = _fdb.FeatureClassFields(this._dsID, table);

                    AccessFDBFeatureClass fc = element.Class as AccessFDBFeatureClass;
                    ((Fields)fc.Fields).Clear();

                    foreach (IField field in fields)
                    {
                        ((Fields)fc.Fields).Add(field);
                    }
                }
            }
        }

        void AccessFDB_DatasetRenamed(string oldName, string newName)
        {
            if (_dsname == oldName)
                _dsname = newName;
        }

        public IDatasetElement this[string DatasetElementTitle]
        {
            get
            {
                if (_fdb == null) return null;
                IDatasetElement element = _fdb.DatasetElement(this, DatasetElementTitle);

                if (element != null && element.Class is AccessFDBFeatureClass)
                {
                    ((AccessFDBFeatureClass)element.Class).SpatialReference = _sRef;
                }

                return element;
            }
        }

        public override string ToString()
        {
            return _dsname;
        }

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

            /*
            if (_layers == null) _layers = new List<IDatasetElement>();

            IFeatureLayer layer;
            while ((layer = (IFeatureLayer)stream.Load("IFeatureLayer", null, new AccessFDBDatasetElement(_fdb,this, null))) != null)
            {
                IDatasetElement e = this[layer.Title];
                if (e is IFeatureLayer)
                {
                    IFeatureLayer l = (IFeatureLayer)e;

                    l.MinimumScale = layer.MinimumScale;
                    l.MaximumScale = layer.MaximumScale;
                    l.Visible = layer.Visible;

                    l.FeatureRenderer = layer.FeatureRenderer;
                    l.LabelRenderer = layer.LabelRenderer;

                    _layers.Add(l);
                }
                if (e == null && layer is AccessFDBDatasetElement)
                {
                    ((AccessFDBDatasetElement)layer).SetEmpty();
                    _layers.Add(layer);
                }
            }
             * */
        }

        public void Save(IPersistStream stream)
        {
            stream.SaveEncrypted("connectionstring", this.ConnectionString);

            /*
            foreach(IDatasetElement element in _layers) 
            {
                if(element is IFeatureLayer)
                    stream.Save("IFeatureLayer",element);
            }
             * */
        }

        #endregion

        #region IDataset2 Member

        public IDataset2 EmptyCopy()
        {
            AccessFDBDataset dataset = new AccessFDBDataset(_fdb);
            dataset.ConnectionString = ConnectionString;
            dataset.Open();

            return dataset;
        }

        public void AppendElement(string elementName)
        {
            if (_fdb == null) return;

            if (_layers == null) _layers = new List<IDatasetElement>();

            foreach (IDatasetElement e in _layers)
            {
                if (e.Title == elementName) return;
            }

            IDatasetElement element = _fdb.DatasetElement(this, elementName);
            if (element != null) _layers.Add(element);
        }

        #endregion

        #region IFDBDataset Member

        public ISpatialIndexDef SpatialIndexDef
        {
            get { return _sIndexDef; }
        }

        #endregion
    }

    [UseWithinSelectableDatasetElements(true)]
    internal class AccessFDBFeatureClass : IFeatureClass, IReportProgress, ISpatialTreeInfo, IRefreshable
    {
        private AccessFDB _fdb;
        private IDataset _dataset;
        private string _name = "", _aliasname = "";
        private string m_idfield = "", m_shapeField = "";
        private geometryType m_fClassType;
        private Fields m_fields;
        private IEnvelope m_envelope = null;
        private BinarySearchTree _searchTree = null;
        //private AccessFDBFeatureCursor _cursor=null;
        private IFeatureCursor _cursor = null;
        private int _pos = -1;
        private GeometryDef _geomDef;
        //private QueryResult _queryResult=null;

        public AccessFDBFeatureClass(AccessFDB fdb, IDataset dataset, GeometryDef geomDef)
        {
            _fdb = fdb;
            _dataset = dataset;
            _geomDef = (geomDef != null) ? geomDef : new GeometryDef();
            m_fields = new Fields();
        }
        public AccessFDBFeatureClass(AccessFDB fdb, IDataset dataset, GeometryDef geomDef, BinarySearchTree tree)
            : this(fdb, dataset, geomDef)
        {
            _searchTree = tree;
        }
        #region IFeatureClass Member

        public string Name { get { return _name; } set { _name = value; } }
        public string Aliasname { get { return _aliasname; } set { _aliasname = value; } }

        public int CountFeatures
        {
            get
            {
                if (_fdb == null) return -1;
                return _fdb.CountFeatures(_name);
            }
        }
        async public Task<IFeatureCursor> GetFeatures(IQueryFilter filter/*, gView.Framework.Data.getFeatureQueryType type*/)
        {
            if (_fdb == null) return null;

            if (filter != null)
            {
                filter.AddField("FDB_OID");
                filter.fieldPrefix = "[";
                filter.fieldPostfix = "]";
            }

            if (filter is IRowIDFilter)
            {
                return await _fdb.QueryIDs(this, filter.SubFieldsAndAlias, ((IRowIDFilter)filter).IDs, filter.FeatureSpatialReference);
            }
            else
            {
                return await _fdb.Query(this, filter);
            }
        }

        /*
		public IFeature GetFeature(int id, gView.Framework.Data.getFeatureQueryType type)
		{
			if(_fdb==null) return null;

			string sql="[FDB_OID]="+id.ToString();
			QueryFilter filter=new QueryFilter();
			filter.WhereClause=sql;
			
			switch(type) 
			{
                case getFeatureQueryType.All:
                case getFeatureQueryType.Attributes:
                    filter.SubFields = "*";
                    break;
                case getFeatureQueryType.Geometry:
                    filter.SubFields = "[FDB_OID],[FDB_SHAPE]";
                    break;
			}

			IFeatureCursor cursor=_fdb.Query(this,filter);
			if(cursor==null) return null;
			return cursor.NextFeature;
		}

		IFeatureCursor gView.Framework.Data.IFeatureClass.GetFeatures(List<int> ids, gView.Framework.Data.getFeatureQueryType type)
		{
			if(_fdb==null) return null;

			
            //QueryFilter filter=new QueryFilter();
            //StringBuilder sql=new StringBuilder();
            //int count=0;
            //foreach(object id in ids) 
            //{
            //    if(count!=0) sql.Append(",");
            //    sql.Append(id.ToString());
            //    count++;
            //}
			
			string SubFields="";

			switch(type) 
			{
                case getFeatureQueryType.All:
                case getFeatureQueryType.Attributes:
                    SubFields = "*";
                    break;
                case getFeatureQueryType.Geometry:
                    SubFields = "[FDB_OID],[FDB_SHAPE]";
                    break;
			}
			return _fdb.QueryIDs(this,SubFields,ids);
		}
        */

        async public Task<ICursor> Search(IQueryFilter filter)
        {
            return await GetFeatures(filter);
        }

        async public Task<ISelectionSet> Select(IQueryFilter filter)
        {
            filter.SubFields = this.IDFieldName;

            filter.AddField("FDB_OID");
            filter.AddField("FDB_SHAPE");
            IFeatureCursor cursor = (IFeatureCursor)_fdb.Query(this, filter);
            IFeature feat;

            SpatialIndexedIDSelectionSet selSet = new SpatialIndexedIDSelectionSet(this.Envelope);
            while ((feat = await cursor.NextFeature()) != null)
            {
                int nid = 0;
                foreach (FieldValue fv in feat.Fields)
                {
                    //if (fv.Name == "FDB_NID")
                    //{
                    //    nid = (int)fv.Value;
                    //    break;
                    //}
                }
                selSet.AddID(feat.OID, feat.Shape);
            }
            cursor.Dispose();
            return selSet;
        }

        public IFields Fields
        {
            get
            {
                return m_fields;
            }
        }

        public IField FindField(string name)
        {
            if (m_fields == null) return null;

            foreach (IField field in m_fields.ToEnumerable())
            {
                if (field.name == name) return field;
            }
            return null;
        }

        public string IDFieldName
        {
            get
            {
                return m_idfield;
            }
            set { m_idfield = value; }
        }
        public string ShapeFieldName
        {
            get { return m_shapeField; }
            set { m_shapeField = value; }
        }
        public IEnvelope Envelope
        {
            get { return m_envelope; }
            set { m_envelope = value; }
        }

        public IDataset Dataset
        {
            get
            {
                return _dataset;
            }
        }
        #endregion

        #region IReportProgress Member

        public void AddDelegate(object Delegate)
        {
            if (_fdb == null) return;
            _fdb.reportProgress += (AccessFDB.ProgressEvent)Delegate;
        }

        #endregion

        public void SetSpatialTreeInfo(gView.Framework.FDB.ISpatialTreeInfo info)
        {
            if (info == null) return;
            try
            {
                _si_type = info.type;
                _si_bounds = info.Bounds;
                _si_spatialRatio = info.SpatialRatio;
                _si_maxPerNode = info.MaxFeaturesPerNode;
            }
            catch { }
        }

        #region ISpatialTreeInfo Member

        private string _si_type = "";
        private IEnvelope _si_bounds = null;
        private double _si_spatialRatio = 0.0;
        private int _si_maxPerNode = 0;
        public IEnvelope Bounds
        {
            get
            {
                return _si_bounds;
            }
        }

        public double SpatialRatio
        {
            get
            {
                return _si_spatialRatio;
            }
        }

        public string type
        {
            get
            {
                return _si_type;
            }
        }

        public int MaxFeaturesPerNode
        {
            get
            {
                return _si_maxPerNode;
            }
        }

        public List<SpatialIndexNode> SpatialIndexNodes
        {
            get
            {
                if (_fdb == null) return null;
                return _fdb.SpatialIndexNodes2(_name);
            }
        }
        #endregion

        #region IGeometryDef Member

        public bool HasZ
        {
            get { return _geomDef.HasZ; }
        }

        public bool HasM
        {
            get { return _geomDef.HasM; }
        }

        public geometryType GeometryType
        {
            get { return _geomDef.GeometryType; }
        }

        public ISpatialReference SpatialReference
        {
            get
            {
                return _geomDef.SpatialReference;
            }
            set
            {
                _geomDef.SpatialReference = value;
            }
        }

        public gView.Framework.Data.GeometryFieldType GeometryFieldType
        {
            get
            {
                return GeometryFieldType.Default;
            }
        }

        #endregion

        #region IRefreshable Member

        public void RefreshFrom(object obj)
        {
            if (!(obj is AccessFDBFeatureClass)) return;

            AccessFDBFeatureClass fc = (AccessFDBFeatureClass)obj;
            if (fc.Name != this.Name) return;

            this.Envelope = fc.Envelope;
            this.SpatialReference = fc.SpatialReference;
            this.IDFieldName = fc.IDFieldName;
            this.ShapeFieldName = fc.ShapeFieldName;

            _geomDef.GeometryType = fc.GeometryType;
            _geomDef.HasZ = fc.HasZ;
            _geomDef.HasM = fc.HasM;

            Fields fields = new Fields(fc.Fields);
            if (fields != null)
            {
                fields.PrimaryDisplayField = m_fields.PrimaryDisplayField;
            }

            m_fields = fields;
        }

        #endregion
    }
}
