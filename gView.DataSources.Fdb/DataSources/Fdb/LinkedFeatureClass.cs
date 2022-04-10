using gView.Framework.Data;
using gView.Framework.Data.Cursors;
using gView.Framework.Data.Filters;
using gView.Framework.FDB;
using System;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb
{
    public class LinkedFeatureClass : IFeatureClass, IDatabaseNames
    {
        private IFeatureClass _fc;
        private IDataset _dataset;
        private string _name;

        public LinkedFeatureClass(IDataset dataset, IFeatureClass fc, string name)
        {
            _dataset = dataset;
            _fc = fc;
            _name = name;
        }

        #region IFeatureClass Members

        public string ShapeFieldName
        {
            get { return _fc != null ? _fc.ShapeFieldName : String.Empty; }
        }

        public Framework.Geometry.IEnvelope Envelope
        {
            get { return _fc != null ? _fc.Envelope : null; }
        }

        async public Task<int> CountFeatures()
        {
            return _fc != null ? await _fc.CountFeatures() : 0;
        }

        async public Task<IFeatureCursor> GetFeatures(IQueryFilter filter)
        {
            if (_fc == null)
            {
                return null;
            }

            return await _fc.GetFeatures(filter);
        }

        #endregion

        #region ITableClass Members

        async public Task<ICursor> Search(IQueryFilter filter)
        {
            if (_fc == null)
            {
                return null;
            }

            return await _fc.Search(filter);
        }

        async public Task<ISelectionSet> Select(IQueryFilter filter)
        {
            if (_fc == null)
            {
                return null;
            }

            return await Select(filter);
        }

        public IFieldCollection Fields
        {
            get { return _fc != null ? _fc.Fields : new FieldCollection(); }
        }

        public IField FindField(string name)
        {
            return _fc != null ? _fc.FindField(name) : null;
        }

        public string IDFieldName
        {
            get { return _fc != null ? _fc.IDFieldName : String.Empty; }
        }

        #endregion

        #region IClass Members

        public string Name
        {
            get { return _name; }
        }

        public string Aliasname
        {
            get { return _name; }
        }

        public IDataset Dataset
        {
            get
            {
                return _dataset;
            }
        }

        #endregion

        #region IGeometryDef Members

        public bool HasZ
        {
            get { return _fc != null ? _fc.HasZ : false; }
        }

        public bool HasM
        {
            get { return _fc != null ? _fc.HasM : false; }
        }

        public Framework.Geometry.ISpatialReference SpatialReference
        {
            get { return _fc?.SpatialReference; }
        }

        public Framework.Geometry.GeometryType GeometryType
        {
            get { return _fc != null ? _fc.GeometryType : gView.Framework.Geometry.GeometryType.Unknown; }
        }

        #endregion

        #region IDatabaseNames Members

        public string TableName(string tableName)
        {
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is IDatabaseNames))
            {
                return tableName;
            }

            return ((IDatabaseNames)_fc.Dataset.Database).TableName(tableName);
        }

        public string DbColName(string fieldName)
        {
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is IDatabaseNames))
            {
                return fieldName;
            }

            return ((IDatabaseNames)_fc.Dataset.Database).DbColName(fieldName);
        }

        #endregion
    }
}
