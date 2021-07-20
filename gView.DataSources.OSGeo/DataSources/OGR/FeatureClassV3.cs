using gView.Framework.Data;
using gView.Framework.Geometry;
using System;
using System.Threading.Tasks;

namespace gView.DataSources.OGR
{
    class FeatureClassV3 : IFeatureClass
    {
        private IEnvelope _envelope;
        private string _shapeFieldName = "SHAPE", _idFieldName, _name;
        private long _countFeatures;
        private Dataset _dataset;
        private OSGeo_v3.OGR.DataSource _dataSource;
        private bool _hasZ = false, _hasM = false;
        private geometryType _geomType = geometryType.Unknown;
        private int _layerIndex;
        private Fields _fields;

        public FeatureClassV3(Dataset dataset, OSGeo_v3.OGR.DataSource dataSource, int layerIndex)
        {
            _dataset = dataset;
            _dataSource = dataSource;
            _layerIndex = layerIndex;

            using (var ogrLayer = _dataSource.GetLayerByIndex(layerIndex))
            {

                OSGeo_v3.OGR.FeatureDefn defn = ogrLayer.GetLayerDefn();
                _name = defn.GetName();
                if (dataset.ConnectionString.ToLower().EndsWith(".dxf"))
                {
                    try
                    {
                        System.IO.FileInfo fi = new System.IO.FileInfo(dataset.ConnectionString);
                        _name = fi.Name;
                    }
                    catch { }
                }
                _fields = new Fields();
                for (int i = 0; i < defn.GetFieldCount(); i++)
                {
                    OSGeo_v3.OGR.FieldDefn fdefn = defn.GetFieldDefn(i);
                    Field field = new Field(fdefn.GetName());

                    switch (fdefn.GetFieldTypeName(fdefn.GetFieldType()).ToLower())
                    {
                        case "integer":
                            if (_idFieldName == String.Empty)
                            {
                                _idFieldName = field.name;
                            }

                            field.type = FieldType.integer;
                            break;
                        case "real":
                            field.type = FieldType.Double;
                            break;
                        case "string":
                            field.type = FieldType.String;
                            field.size = fdefn.GetWidth();
                            break;

                    }
                    _fields.Add(field);
                }

                _countFeatures = ogrLayer.GetFeatureCount(1);
                OSGeo_v3.OGR.Envelope env = new OSGeo_v3.OGR.Envelope();
                ogrLayer.GetExtent(env, 1);
                _envelope = new Envelope(env.MinX, env.MinY, env.MaxX, env.MaxY);

                switch (defn.GetGeomType())
                {
                    case OSGeo_v3.OGR.wkbGeometryType.wkbPoint:
                        _geomType = geometryType.Point;
                        break;
                    case OSGeo_v3.OGR.wkbGeometryType.wkbLineString:
                    case OSGeo_v3.OGR.wkbGeometryType.wkbMultiLineString:
                        _geomType = geometryType.Polyline;
                        break;
                    case OSGeo_v3.OGR.wkbGeometryType.wkbPolygon:
                    case OSGeo_v3.OGR.wkbGeometryType.wkbMultiPolygon:
                        _geomType = geometryType.Polygon;
                        break;
                }
            }
        }

        #region IFeatureClass Member

        public string ShapeFieldName
        {
            get { return _shapeFieldName; }
        }

        public IEnvelope Envelope
        {
            get { return _envelope; }
        }

        public Task<int> CountFeatures()
        {
            return Task.FromResult((int)_countFeatures);
        }

        public Task<IFeatureCursor> GetFeatures(IQueryFilter filter)
        {
            return Task.FromResult<IFeatureCursor>(new FeatureCursorV3(_dataSource.GetLayerByIndex(_layerIndex), filter));
        }

        #endregion

        #region ITableClass Member

        async public Task<ICursor> Search(IQueryFilter filter)
        {
            return await GetFeatures(filter);
        }

        public Task<ISelectionSet> Select(IQueryFilter filter)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IFields Fields
        {
            get
            {
                //List<IField> fields = new List<IField>();
                //foreach (IField field in _fields)
                //    fields.Add(field);

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
            get { return _idFieldName; }
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
            get { return _hasZ; }
        }

        public bool HasM
        {
            get { return _hasM; }
        }

        public ISpatialReference SpatialReference
        {
            get; set;
        }

        public geometryType GeometryType
        {
            get { return _geomType; }
        }

        public GeometryFieldType GeometryFieldType
        {
            get { return GeometryFieldType.Default; }
        }

        #endregion
    }
}
