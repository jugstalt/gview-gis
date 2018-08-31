using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.Geometry;
using System.Xml;

namespace gView.Interoperability.OGC.Dataset.GML
{
    class FeatureClass : IFeatureClass
    {
        private string _idField = "oid", _shapeField = "geometry";
        private Fields _fields;
        private string _name;
        private Dataset _dataset;
        private geometryType _geomType = geometryType.Unknown;
        private GmlVersion _gmlVersion = GmlVersion.v1;

        public FeatureClass(Dataset dataset, string name, Fields fields)
        {
            _dataset = dataset;
            _name = name;
            _fields = fields;
        }

        #region IFeatureClass Member

        public string ShapeFieldName
        {
            get { return _shapeField; }
            set { _shapeField = value; }
        }

        public gView.Framework.Geometry.IEnvelope Envelope
        {
            get { return _dataset.Envelope; }
        }

        public int CountFeatures
        {
            get { return 0; }
        }

        public IFeatureCursor GetFeatures(IQueryFilter filter)
        {
            gView.Framework.OGC.GML.FeatureCursor2 cursor =
                new gView.Framework.OGC.GML.FeatureCursor2(this,
                new XmlTextReader(_dataset.GmlFileName),
                _dataset.NamespaceManager,
                filter, _gmlVersion);

            return cursor;
        }

        #endregion

        #region ITableClass Member

        public ICursor Search(IQueryFilter filter)
        {
            return GetFeatures(filter);
        }

        public ISelectionSet Select(IQueryFilter filter)
        {
            return null;
        }

        public IFields Fields
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
                if (field.name == name) return field;

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
            get { return _dataset.SpatialReference; }
        }

        public gView.Framework.Geometry.geometryType GeometryType
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
