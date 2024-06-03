using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.OGC.WFS;
using gView.Framework.Web;
using gView.Interoperability.OGC.Dataset.GML;
using gView.Interoperability.OGC.Dataset.WMS;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace gView.Interoperability.OGC.Dataset.WFS
{
    class WFSFeatureClass : CacheableFeatureClass, IWMSStyle, IFeatureClass
    {
        private WFSDataset _dataset;
        private WMSClass.SRS _srs;
        private string _name, _shapefieldName = "wsGeometry", _idFieldname = String.Empty, _targetNamespace = "";
        private FieldCollection _fields = new FieldCollection();
        private GeometryType _geomtype = GeometryType.Unknown;
        private ISpatialReference _sRef = null;

        private WFSFeatureClass(WFSDataset dataset, string name, WMSClass.SRS srs)
        {
            _dataset = dataset;
            _name = name;
            _srs = srs;

            if (_srs.Srs.Count > 0)
            {
                _sRef = gView.Framework.Geometry.SpatialReference.FromID(_srs.Srs[_srs.SRSIndex]);
            }
        }

        async public static Task<WFSFeatureClass> CreateAsync(WFSDataset dataset, string name, WMSClass.SRS srs)
        {
            var fc = new WFSFeatureClass(dataset, name, srs);
            try
            {
                string param = "VERSION=1.0.0&REQUEST=DescribeFeatureType&TYPENAME=" + fc._name;
                if (fc._dataset._decribeFeatureType.Get_OnlineResource.IndexOf("&SERVICE=") == -1 &&
                    fc._dataset._decribeFeatureType.Get_OnlineResource.IndexOf("?SERVICE=") == -1)
                {
                    param = "SERVICE=WFS&" + param;
                }

                string url = WMSDataset.Append2Url(fc._dataset._decribeFeatureType.Get_OnlineResource, param);
                string response = await WebFunctions.HttpSendRequestAsync(url, "GET", null);
                response = WMSDataset.RemoveDOCTYPE(response);

                XmlDocument schema = new XmlDocument();
                schema.LoadXml(response);
                XmlSchemaReader schemaReader = new XmlSchemaReader(schema);
                fc._targetNamespace = schemaReader.TargetNamespaceURI;
                if (fc._targetNamespace == String.Empty)
                {
                    return fc;
                }

                fc._fields = schemaReader.ElementFields(name, out fc._shapefieldName, out fc._geomtype);

                // Id Feld suchen
                foreach (IField field in fc._fields.ToEnumerable())
                {
                    if (field.type == FieldType.ID)
                    {
                        fc._idFieldname = field.name;
                        break;
                    }
                }
                // passendes feld suchen...
                //if (_idFieldname == String.Empty)
                //{
                //    foreach (IField field in _fields)
                //    {
                //        if (!(field is Field)) continue;
                //        switch (field.name.ToLower())
                //        {
                //            case "fdb_oid":
                //            case "oid":
                //            case "fid":
                //            case "objectid":
                //            case "featureid":
                //            case "ogc_fid":
                //                ((Field)field).type = FieldType.ID;
                //                _idFieldname = field.name;
                //                break;
                //        }
                //        if (_idFieldname != String.Empty) break;
                //    }
                //}
            }
            catch { }

            return fc;
        }

        #region IFeatureClass Member

        override public string ShapeFieldName
        {
            get { return _shapefieldName; }
        }

        public IEnvelope Envelope
        {
            get { return null; }
        }

        public Task<int> CountFeatures()
        {
            return Task.FromResult(0);
        }

        #endregion

        #region ITableClass Member

        public override IFieldCollection Fields
        {
            get
            {
                //List<IField> fields = new List<IField>();
                //if (_fields == null) return fields;
                //foreach (IField field in _fields)
                //{
                //    fields.Add(field);
                //}
                //return fields;
                return _fields;
            }
        }

        override public IField FindField(string name)
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

        override public string IDFieldName
        {
            get { return _idFieldname; }
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
            get
            {
                return _dataset._dataset;  // WMSDataset is parent! 
            }
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

        public ISpatialReference SpatialReference
        {
            get { return _sRef; }
            set { _sRef = value; }
        }

        public GeometryType GeometryType
        {
            get { return _geomtype; }
        }

        public GeometryFieldType GeometryFieldType
        {
            get { return GeometryFieldType.Default; }
        }
        #endregion

        #region IWMSStyle Member

        private string _style;
        public string Style
        {
            get { return _style; }
            internal set { _style = value; }
        }

        #endregion

        async protected override Task<IFeatureCursor> FeatureCursor(IQueryFilter filter)
        {
            string response = "";

            string srsName = _sRef != null ? _sRef.Name : String.Empty;
            if (filter is ISpatialFilter)
            {
                filter = SpatialFilter.Project(filter as ISpatialFilter, this.SpatialReference);
                ((ISpatialFilter)filter).FilterSpatialReference = this.SpatialReference;
            }

            if (_dataset._getCapabilities.Post_OnlineResource == String.Empty &&
                _dataset._getCapabilities.Get_OnlineResource != String.Empty)
            {
                string param = "VERSION=1.0.0&REQUEST=GetFeature&TYPENAME=" + this.Name + "&MAXFEATURES=10&FILTER=";
                if (_dataset._getCapabilities.Get_OnlineResource.IndexOf("&SERVICE=") == -1 &&
                    _dataset._getCapabilities.Get_OnlineResource.IndexOf("?SERVICE=") == -1)
                {
                    param = "SERVICE=WFS&" + param;
                }

                string wfsFilter = await Filter.ToWFS(this, filter, _dataset._filter_capabilites, _dataset._gmlVersion);
                string url = _dataset._getFeature.Get_OnlineResource;

                response = await WebFunctions.HttpSendRequestAsync(url, "GET", null);
            }
            else if (_dataset._getCapabilities.Post_OnlineResource != String.Empty)
            {
                string url = _dataset._getFeature.Post_OnlineResource;
                if (_dataset._getCapabilities.Get_OnlineResource.IndexOf("&SERVICE=") == -1 &&
                    _dataset._getCapabilities.Get_OnlineResource.IndexOf("?SERVICE=") == -1)
                {
                    url = WMSDataset.Append2Url(url, "SERVICE=WFS");
                }

                string wfsFilter = GetFeatureRequest.Create(this, this.Name, filter, srsName, _dataset._filter_capabilites, _dataset._gmlVersion);

                response = await WebFunctions.HttpSendRequestAsync(url, "POST",
                    Encoding.UTF8.GetBytes(wfsFilter));
            }
            if (response == String.Empty)
            {
                return null;
            }

            try
            {
                StringReader stringReader = new StringReader(response);
                XmlTextReader xmlReader = new XmlTextReader(stringReader);

                XmlDocument doc = new XmlDocument();
                //doc.LoadXml(response);
                XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
                ns.AddNamespace("GML", "http://www.opengis.net/gml");
                ns.AddNamespace("WFS", "http://www.opengis.net/wfs");
                ns.AddNamespace("OGC", "http://www.opengis.net/ogc");
                ns.AddNamespace("myns", _targetNamespace);

                //XmlNode featureCollection = doc.SelectSingleNode("WFS:FeatureCollection", ns);
                //if (featureCollection == null)
                //    featureCollection = doc.SelectSingleNode("GML:FeatureCollection", ns);
                //if (featureCollection == null) return null;

                return new gView.Framework.OGC.GML.FeatureCursor2(this, xmlReader, ns, filter, _dataset._gmlVersion, _dataset._filter_capabilites);
            }
            catch { }
            return null;
        }
    }
}
