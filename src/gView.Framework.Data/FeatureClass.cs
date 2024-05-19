using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using System;
using System.Threading.Tasks;

namespace gView.Framework.Data
{
    public class FeatureClass : IFeatureClass
    {
        private string _name = String.Empty, _idFieldName = String.Empty, _shapeFieldName = String.Empty;
        private IFieldCollection _fields = new FieldCollection();
        private IEnvelope _envelope = null;

        #region IFeatureClass Member

        virtual public Task<IFeatureCursor> GetFeatures(IQueryFilter filter/*, gView.Framework.Data.getFeatureQueryType type*/)
        {
            return Task.FromResult<IFeatureCursor>(null);
        }

        public Task<int> CountFeatures()
        {
            return Task.FromResult(0);
        }

        public string ShapeFieldName
        {
            get
            {
                return _shapeFieldName;
            }
            set { _shapeFieldName = value; }
        }

        public IEnvelope Envelope
        {
            get
            {
                return _envelope;
            }
            set { _envelope = value; }
        }

        #endregion

        #region ITableClass Member

        public string IDFieldName
        {
            get
            {
                return _idFieldName;
            }
            set { _idFieldName = value; }
        }

        public string Aliasname
        {
            get
            {
                return _name;
            }
        }

        public IField FindField(string name)
        {
            foreach (IField field in _fields.ToEnumerable())
            {
                if (field.name == name)
                {
                    return field;
                }
            }
            return null;
        }

        virtual public Task<ICursor> Search(IQueryFilter filter)
        {
            return Task.FromResult<ICursor>(null);
        }

        virtual public Task<ISelectionSet> Select(IQueryFilter filter)
        {
            return Task.FromResult<ISelectionSet>(null);
        }

        public IFieldCollection Fields
        {
            get
            {
                return _fields;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public virtual IDataset Dataset
        {
            get { return null; }
        }
        #endregion

        #region IGeometryDef Member

        private bool _hasZ = false, _hasM = false;
        private GeometryType _geomType = GeometryType.Unknown;
        private ISpatialReference _sRef = null;
        private GeometryFieldType _geometryFieldType = GeometryFieldType.Default;

        public bool HasZ
        {
            get { return _hasZ; }
            set { _hasZ = value; }
        }

        public bool HasM
        {
            get { return _hasM; }
            set { _hasM = value; }
        }

        public GeometryType GeometryType
        {
            get { return _geomType; }
            set { _geomType = value; }
        }

        public ISpatialReference SpatialReference
        {
            get
            {
                return _sRef;
            }
            set
            {
                _sRef = value;
            }
        }

        public GeometryFieldType GeometryFieldType
        {
            get
            {
                return _geometryFieldType;
            }
            set
            {
                _geometryFieldType = value;
            }
        }
        #endregion
    }
}
