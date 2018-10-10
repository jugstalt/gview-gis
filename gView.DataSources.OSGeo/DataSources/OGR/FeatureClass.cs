using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Geometry;
using gView.Framework.Data;

namespace gView.DataSources.OGR
{
    class FeatureClass : IFeatureClass
    {
        private IEnvelope _envelope;
        private string _shapeFieldName = "SHAPE", _idFieldName, _name;
        private int _countFeatures;
        private Dataset _dataset;
        private bool _hasZ = false, _hasM = false;
        private geometryType _geomType = geometryType.Unknown;
        private Fields _fields;
        private OSGeo.OGR.Layer _ogrLayer = null;

        public FeatureClass(Dataset dataset, OSGeo.OGR.Layer layer)
        {
            _dataset = dataset;
            _ogrLayer = layer;

            OSGeo.OGR.FeatureDefn defn = layer.GetLayerDefn();
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
                OSGeo.OGR.FieldDefn fdefn = defn.GetFieldDefn(i);
                Field field = new Field(fdefn.GetName());

                switch (fdefn.GetFieldTypeName(fdefn.GetFieldType()).ToLower())
                {
                    case "integer":
                        if (_idFieldName == String.Empty) _idFieldName = field.name;
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

            _countFeatures = layer.GetFeatureCount(1);
            OSGeo.OGR.Envelope env = new OSGeo.OGR.Envelope();
            layer.GetExtent(env, 1);
            _envelope = new Envelope(env.MinX, env.MinY, env.MaxX, env.MaxY);

            switch (defn.GetGeomType())
            {
                case OSGeo.OGR.wkbGeometryType.wkbPoint:
                    _geomType = geometryType.Point;
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbLineString:
                case OSGeo.OGR.wkbGeometryType.wkbMultiLineString:
                    _geomType = geometryType.Polyline;
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                case OSGeo.OGR.wkbGeometryType.wkbMultiPolygon:
                    _geomType=geometryType.Polygon;
                    break;
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

        public int CountFeatures
        {
            get { return _countFeatures; }
        }

        public IFeatureCursor GetFeatures(IQueryFilter filter)
        {
            return new FeatureCursor(_ogrLayer, filter);
        }

        #endregion

        #region ITableClass Member

        public ICursor Search(IQueryFilter filter)
        {
            return GetFeatures(filter);
        }

        public ISelectionSet Select(IQueryFilter filter)
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
                if (field.name == name) return field;
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
            get { return null; }
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
