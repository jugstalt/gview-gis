using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Data.Cursors;
using gView.Framework.Data.Filters;
using gView.Framework.Data.Metadata;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.MSAccess
{
    /// <summary>
    /// Zusammenfassung für AccessFDBDataset.
    /// </summary>
    //[gView.Framework.system.RegisterPlugIn("6F540340-74C9-4b1a-BD4D-B1C5FE946CA1")]
    public class AccessFDBDataset : DatasetMetadata, IFeatureDataset2, IRasterDataset, IFDBDataset
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

        async public Task<List<IDatasetElement>> Elements()
        {
            if (_fdb == null)
            {
                return new List<IDatasetElement>();
            }

            if (_layers == null || _layers.Count == 0)
            {
                _layers = await _fdb.DatasetLayers(this);

                if (_layers != null && _addedLayers.Count != 0)
                {
                    List<IDatasetElement> list = new List<IDatasetElement>();
                    foreach (IDatasetElement element in _layers)
                    {
                        if (element is IFeatureLayer)
                        {
                            if (_addedLayers.Contains(((IFeatureLayer)element).FeatureClass.Name))
                            {
                                list.Add(element);
                            }
                        }
                        else if (element is IRasterLayer)
                        {
                            if (_addedLayers.Contains(((IRasterLayer)element).Title))
                            {
                                list.Add(element);
                            }
                        }
                        else
                        {
                            if (_addedLayers.Contains(element.Title))
                            {
                                list.Add(element);
                            }
                        }
                    }
                    _layers = list;
                }

            }

            if (_layers == null)
            {
                return new List<IDatasetElement>();
            }

            return _layers;
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
        async public Task<IEnvelope> Envelope()
        {
            if (_layers == null)
            {
                return null;
            }

            bool first = true;
            IEnvelope env = null;

            foreach (IDatasetElement layer in _layers)
            {
                if (!(layer.Class is IFeatureClass))
                {
                    continue;
                }

                IEnvelope envelope = ((IFeatureClass)layer.Class).Envelope;
                if (envelope.Width > 1e10 && await ((IFeatureClass)layer.Class).CountFeatures() == 0)
                {
                    envelope = null;
                }

                if (gView.Framework.Geometry.Envelope.IsNull(envelope))
                {
                    continue;
                }

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
        public Task<ISpatialReference> GetSpatialReference()
        {

            return Task.FromResult(_sRef);

            // wird sonst Zach, wenns bei jedem Bildaufbau von jedem Thema gelesen werden muß...
            //if(_sRef!=null) return _sRef;

            //if(_fdb==null) return null;
            //return _sRef=_fdb.SpatialReference(_dsname);
        }
        public void SetSpatialReference(ISpatialReference sRef)
        {
            _sRef = sRef;
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
                {
                    c = c.Replace(";;", ";");
                }

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
        }
        async public Task<bool> SetConnectionString(string value)
        {
            _connString = value;
            if (value == null)
            {
                return false;
            }

            while (_connString.IndexOf(";;") != -1)
            {
                _connString = _connString.Replace(";;", ";");
            }

            _dsname = ConfigTextStream.ExtractValue(value, "dsname");
            _addedLayers.Clear();
            foreach (string layername in ConfigTextStream.ExtractValue(value, "layers").Split('@'))
            {
                if (layername == "")
                {
                    continue;
                }

                if (_addedLayers.IndexOf(layername) != -1)
                {
                    continue;
                }

                _addedLayers.Add(layername);
            }

            if (_fdb == null)
            {
                throw new NullReferenceException("_fdb==null");
            }

            _fdb.DatasetRenamed += new AccessFDB.DatasetRenamedEventHandler(AccessFDB_DatasetRenamed);
            _fdb.FeatureClassRenamed += new AccessFDB.FeatureClassRenamedEventHandler(AccessFDB_FeatureClassRenamed);
            _fdb.TableAltered += new AccessFDB.AlterTableEventHandler(SqlFDB_TableAltered);
            await _fdb.Open(ConfigTextStream.ExtractValue(_connString, "mdb"));

            return true;
        }


        public DatasetState State
        {
            get { return _state; }
        }

        async public Task<bool> Open()
        {
            if (_connString == null || _connString == "" ||
                _dsname == null || _dsname == "" || _fdb == null)
            {
                return false;
            }

            _dsID = await _fdb.DatasetID(_dsname);
            if (_dsID == -1)
            {
                return false;
            }

            _sRef = await _fdb.SpatialReference(_dsname);
            _sIndexDef = await _fdb.SpatialIndexDef(_dsID);

            _state = DatasetState.opened;
            return true;
        }

        public string LastErrorMessage
        {
            get
            {
                return _errMsg;
            }
            set
            {
                _errMsg = value;
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

        async public Task RefreshClasses()
        {
            if (_fdb != null)
            {
                await _fdb.RefreshClasses(this);
            }
        }
        #endregion

        void AccessFDB_FeatureClassRenamed(string oldName, string newName)
        {
            if (_layers == null)
            {
                return;
            }

            foreach (IDatasetElement element in _layers)
            {
                if (element.Class is AccessFDBFeatureClass &&
                    ((AccessFDBFeatureClass)element.Class).Name == oldName)
                {
                    ((AccessFDBFeatureClass)element.Class).Name = newName;
                }
            }
        }

        async void SqlFDB_TableAltered(string table)
        {
            if (_layers == null)
            {
                return;
            }

            foreach (IDatasetElement element in _layers)
            {
                if (element.Class is AccessFDBFeatureClass &&
                    ((AccessFDBFeatureClass)element.Class).Name == table)
                {
                    var fields = await _fdb.FeatureClassFields(this._dsID, table);

                    AccessFDBFeatureClass fc = element.Class as AccessFDBFeatureClass;
                    ((FieldCollection)fc.Fields).Clear();

                    foreach (IField field in fields)
                    {
                        ((FieldCollection)fc.Fields).Add(field);
                    }
                }
            }
        }

        void AccessFDB_DatasetRenamed(string oldName, string newName)
        {
            if (_dsname == oldName)
            {
                _dsname = newName;
            }
        }

        async public Task<IDatasetElement> Element(string DatasetElementTitle)
        {
            if (_fdb == null)
            {
                return null;
            }

            IDatasetElement element = await _fdb.DatasetElement(this, DatasetElementTitle);

            if (element != null && element.Class is AccessFDBFeatureClass)
            {
                ((AccessFDBFeatureClass)element.Class).SpatialReference = _sRef;
            }

            return element;
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

        async public Task<bool> LoadAsync(IPersistStream stream)
        {
            string connectionString = (string)stream.Load("connectionstring", "");
            if (_fdb != null)
            {
                _fdb.Dispose();
                _fdb = null;
            }

            await this.SetConnectionString(connectionString);
            return await this.Open();
        }

        public void Save(IPersistStream stream)
        {
            stream.SaveEncrypted("connectionstring", this.ConnectionString);
        }

        #endregion

        #region IDataset2 Member

        async public Task<IDataset2> EmptyCopy()
        {
            AccessFDBDataset dataset = new AccessFDBDataset(_fdb);
            await dataset.SetConnectionString(ConnectionString);
            await dataset.Open();

            return dataset;
        }

        async public Task AppendElement(string elementName)
        {
            if (_fdb == null)
            {
                return;
            }

            if (_layers == null)
            {
                _layers = new List<IDatasetElement>();
            }

            foreach (IDatasetElement e in _layers)
            {
                if (e.Title == elementName)
                {
                    return;
                }
            }

            IDatasetElement element = await _fdb.DatasetElement(this, elementName);
            if (element != null)
            {
                _layers.Add(element);
            }
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
        private FieldCollection m_fields;
        private IEnvelope m_envelope = null;
        private BinarySearchTree _searchTree = null;
        private GeometryDef _geomDef;

        public AccessFDBFeatureClass(AccessFDB fdb, IDataset dataset, GeometryDef geomDef)
        {
            _fdb = fdb;
            _dataset = dataset;
            _geomDef = (geomDef != null) ? geomDef : new GeometryDef();
            m_fields = new FieldCollection();
        }
        public AccessFDBFeatureClass(AccessFDB fdb, IDataset dataset, GeometryDef geomDef, BinarySearchTree tree)
            : this(fdb, dataset, geomDef)
        {
            _searchTree = tree;
        }
        #region IFeatureClass Member

        public string Name { get { return _name; } set { _name = value; } }
        public string Aliasname { get { return _aliasname; } set { _aliasname = value; } }

        async public Task<int> CountFeatures()
        {
            if (_fdb == null)
            {
                return -1;
            }

            return await _fdb.CountFeatures(_name);
        }
        async public Task<IFeatureCursor> GetFeatures(IQueryFilter filter/*, gView.Framework.Data.getFeatureQueryType type*/)
        {
            if (_fdb == null)
            {
                return null;
            }

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
            IFeatureCursor cursor = await _fdb.Query(this, filter);
            IFeature feat;

            SpatialIndexedIDSelectionSet selSet = new SpatialIndexedIDSelectionSet(this.Envelope);
            while ((feat = await cursor.NextFeature()) != null)
            {
                //int nid = 0;
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

        public IFieldCollection Fields
        {
            get
            {
                return m_fields;
            }
        }

        public IField FindField(string name)
        {
            if (m_fields == null)
            {
                return null;
            }

            foreach (IField field in m_fields.ToEnumerable())
            {
                if (field.name == name)
                {
                    return field;
                }
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
            if (_fdb == null)
            {
                return;
            }

            _fdb.reportProgress += (AccessFDB.ProgressEvent)Delegate;
        }

        #endregion

        public void SetSpatialTreeInfo(gView.Framework.FDB.ISpatialTreeInfo info)
        {
            if (info == null)
            {
                return;
            }

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

        async public Task<List<SpatialIndexNode>> SpatialIndexNodes()
        {
            if (_fdb == null)
            {
                return null;
            }

            return await _fdb.SpatialIndexNodes2(_name);
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

        public GeometryType GeometryType
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
            if (!(obj is AccessFDBFeatureClass))
            {
                return;
            }

            AccessFDBFeatureClass fc = (AccessFDBFeatureClass)obj;
            if (fc.Name != this.Name)
            {
                return;
            }

            this.Envelope = fc.Envelope;
            this.SpatialReference = fc.SpatialReference;
            this.IDFieldName = fc.IDFieldName;
            this.ShapeFieldName = fc.ShapeFieldName;

            _geomDef.GeometryType = fc.GeometryType;
            _geomDef.HasZ = fc.HasZ;
            _geomDef.HasM = fc.HasM;

            FieldCollection fields = new FieldCollection(fc.Fields);
            if (fields != null)
            {
                fields.PrimaryDisplayField = m_fields.PrimaryDisplayField;
            }

            m_fields = fields;
        }

        #endregion
    }
}
