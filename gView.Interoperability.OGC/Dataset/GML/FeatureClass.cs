using gView.Framework.Data;
using gView.Framework.Data.Cursors;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using System.Threading.Tasks;
using System.Xml;

namespace gView.Interoperability.OGC.Dataset.GML
{
    class FeatureClass : IFeatureClass
    {
        private string _idField = "oid", _shapeField = "geometry";
        private FieldCollection _fields;
        private string _name;
        private Dataset _dataset;
        private GeometryType _geomType = GeometryType.Unknown;
        private GmlVersion _gmlVersion = GmlVersion.v1;

        private FeatureClass(Dataset dataset, string name, FieldCollection fields)
        {
            _dataset = dataset;
            _name = name;
            _fields = fields;
        }

        async static public Task<FeatureClass> CreateAsync(Dataset dataset, string name, FieldCollection fields)
        {
            var fc = new FeatureClass(dataset, name, fields);
            if (dataset != null)
            {
                fc.SpatialReference = await dataset.GetSpatialReference();
                fc.Envelope = await dataset.Envelope();
            }
            return fc;
        }

        #region IFeatureClass Member

        public string ShapeFieldName
        {
            get { return _shapeField; }
            set { _shapeField = value; }
        }

        public gView.Framework.Geometry.IEnvelope Envelope
        {
            get;
            private set;
        }

        public Task<int> CountFeatures()
        {
            return Task.FromResult(0);
        }

        public Task<IFeatureCursor> GetFeatures(IQueryFilter filter)
        {
            gView.Framework.OGC.GML.FeatureCursor2 cursor =
                new gView.Framework.OGC.GML.FeatureCursor2(this,
                new XmlTextReader(_dataset.GmlFileName),
                _dataset.NamespaceManager,
                filter, _gmlVersion);

            return Task.FromResult<IFeatureCursor>(cursor);
        }

        #endregion

        #region ITableClass Member

        async public Task<ICursor> Search(IQueryFilter filter)
        {
            return await GetFeatures(filter);
        }

        public Task<ISelectionSet> Select(IQueryFilter filter)
        {
            return Task.FromResult<ISelectionSet>(null);
        }

        public IFieldCollection Fields
        {
            get
            {
                //List<IField> fields=new List<IField>();
                //foreach (IField field in _fields)
                //    fields.Add(field);

                //return fields;
                return _fields;
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

        public string IDFieldName
        {
            get { return _idField; }
        }

        #endregion

        #region IClass Member

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
            get { return _dataset; }
        }

        #endregion

        #region IGeometryDef Member

        public bool HasZ
        {
            get { return false; }
        }

        public bool HasM
        {
            get { return false; }
        }

        public gView.Framework.Geometry.ISpatialReference SpatialReference
        {
            get;
            set;
        }

        public gView.Framework.Geometry.GeometryType GeometryType
        {
            get { return _geomType; }
            set { _geomType = value; }
        }

        public GeometryFieldType GeometryFieldType
        {
            get { return GeometryFieldType.Default; }
        }
        #endregion
    }
}
