using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.Data;
using gView.Framework.system;
using gView.Framework.Geometry;
using gView.Framework.FDB;

namespace gView.DataSources.Fdb.MSSql
{
    public class SqlFDBFeatureClass : IFeatureClass, IRefreshable
    {
        private SqlFDB _fdb;
        private IDataset _dataset;
        private string _name = "", _aliasname = "";
        private string m_idfield = "", m_shapeField = "";
        private Fields m_fields;
        private IEnvelope m_envelope = null;
        //private SqlFDBFeatureCursor _cursor=null;

        private BinarySearchTree _searchTree = null;
        private GeometryDef _geomDef;
        private string _dbSchema = String.Empty;

        public SqlFDBFeatureClass(SqlFDB fdb, IDataset dataset, GeometryDef geomDef)
        {
            _fdb = fdb;

            _dataset = dataset;
            _geomDef = (geomDef != null) ? geomDef : new GeometryDef();

            if (_geomDef != null && _geomDef.SpatialReference == null && dataset is IFeatureDataset)
                _geomDef.SpatialReference = ((IFeatureDataset)dataset).SpatialReference;

            m_fields = new Fields();
        }

        public SqlFDBFeatureClass(SqlFDB fdb, IDataset dataset, GeometryDef geomDef, BinarySearchTree tree)
            : this(fdb, dataset, geomDef)
        {
            _searchTree = tree;
        }

        #region FeatureClass Member

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                if (_fdb != null)
                    _dbSchema = _fdb.GetFeatureClassDbSchema(_name);
                else
                    _dbSchema = String.Empty;
            }
        }
        public string Aliasname { get { return _aliasname; } set { _aliasname = value; } }

        public int CountFeatures
        {
            get
            {
                if (_fdb == null) return -1;
                return _fdb.CountFeatures(_name);
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

        public IFeatureCursor GetFeatures(IQueryFilter filter/*, gView.Framework.Data.getFeatureQueryType type*/)
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
                return _fdb.QueryIDs(this, filter.SubFieldsAndAlias, ((IRowIDFilter)filter).IDs, filter.FeatureSpatialReference);
            }
            else
            {
                return _fdb.Query(this, filter);
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

            string SubFields = "";

            switch (type)
            {
                case getFeatureQueryType.All:
                case getFeatureQueryType.Attributes:
                    SubFields = "*";
                    break;
                case getFeatureQueryType.Geometry:
                    SubFields = "[FDB_OID],[FDB_SHAPE]";
                    break;
            }
            return _fdb.QueryIDs(_name, SubFields, ids);
		}
        */

        public ICursor Search(IQueryFilter filter)
        {
            return GetFeatures(filter);
        }

        public ISelectionSet Select(IQueryFilter filter)
        {
            filter.SubFields = this.IDFieldName;

            filter.AddField("FDB_SHAPE");
            filter.AddField("FDB_OID");
            IFeatureCursor cursor = (IFeatureCursor)_fdb.Query(this, filter);
            IFeature feat;

            SpatialIndexedIDSelectionSet selSet = new SpatialIndexedIDSelectionSet(this.Envelope);
            while ((feat = cursor.NextFeature) != null)
            {
                long nid = 0;
                foreach (FieldValue fv in feat.Fields)
                {
                    //if (fv.Name == "FDB_NID")
                    //{
                    //    nid = Convert.ToInt64(fv.Value);
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
            get { return _dataset; }
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

        //public GeometryFieldType GeometryFieldType
        //{
        //    get
        //    {
        //        return _geomDef.GeometryFieldType;
        //    }
        //}
        #endregion

        #region IRefreshable Member

        public void RefreshFrom(object obj)
        {
            if (!(obj is SqlFDBFeatureClass)) return;

            SqlFDBFeatureClass fc = (SqlFDBFeatureClass)obj;
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

        public string DbSchema
        {
            get { return _dbSchema; }
        }

        public string DbTableName
        {
            get
            {
                string name = _name;
                if (name.Contains("@"))
                    name = _fdb.SpatialViewNames(name)[1];
                else
                    name = "FC_" + name;

                return (String.IsNullOrEmpty(_dbSchema) ? name : _dbSchema + "." + name);
            }
        }

        public string SiDbTableName
        {
            get
            {
                string name = _name;
                if (name.Contains("@"))
                    name = _fdb.SpatialViewNames(name)[0];

                return (String.IsNullOrEmpty(_dbSchema) ? "FCSI_" + name : _dbSchema + ".FCSI_" + name);
            }
        }
    }

}
