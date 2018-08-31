using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.Geometry;
using System.IO;
using System.Xml;
using gView.Framework.system;
using gView.Framework.Carto;
using gView.Framework.OGC.GML;
using gView.Framework.FDB;

namespace gView.Interoperability.OGC.Dataset.GML
{
    [gView.Framework.system.RegisterPlugIn("DBABE7F1-FE46-4731-AB2B-8A324C60554E")]
    public class Dataset : DatasetMetadata, IFeatureDataset
    {
        private string _connectionString;
        private DatasetState _state = DatasetState.unknown;
        private IEnvelope _envelope = null;
        private ISpatialReference _sRef = null;
        private string _errMsg = "";
        private XmlDocument _doc = null;
        private XmlNamespaceManager _ns = null;
        private XmlNode _featureCollection = null;
        private List<IDatasetElement> _elements = new List<IDatasetElement>();
        private string _gml_file = "", _xsd_file = "";
        private Database _database = new Database();
        private GmlVersion _gmlVersion = GmlVersion.v1;

        public XmlNamespaceManager NamespaceManager
        {
            get { return _ns; }
        }

        public XmlNode FeatureCollection
        {
            get { return _featureCollection; }
        }
        public string GmlFileName
        {
            get { return _gml_file; }
        } 

        public bool Delete()
        {
            try
            {
                if (_state != DatasetState.opened) return false;

                FileInfo fi = new FileInfo(_gml_file);
                if(fi.Exists) fi.Delete();
                fi = new FileInfo(_xsd_file);
                if(fi.Exists) fi.Delete();

                return true;
            }
            catch
            {
                return false;
            }
        }

        internal GMLFile GMLFile
        {
            get
            {
                try
                {
                    if (_state != DatasetState.opened) return null;

                    FileInfo fi = new FileInfo(_connectionString);
                    if (fi.Exists)
                    {
                        return new GMLFile(_connectionString);
                    }
                    return null;
                }
                catch
                {
                    return null;
                }
            }
        }

        internal string targetNamespace
        {
            get
            {
                if (_ns == null) return String.Empty;
                return _ns.LookupNamespace("myns");
            }
        }

        #region IFeatureDataset Member

        public gView.Framework.Geometry.IEnvelope Envelope
        {
            get { return _envelope; }
        }

        public gView.Framework.Geometry.ISpatialReference SpatialReference
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

        #endregion

        #region IDataset Member

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                _connectionString = value;
                _database.DirectoryName = _connectionString;
            }
        }

        public string DatasetGroupName
        {
            get { return "OGC/GML Dataset"; }
        }

        public string DatasetName
        {
            get { return "GML Dataset"; }
        }

        public string ProviderName
        {
            get { return "gView GML Provider"; }
        }

        public DatasetState State
        {
            get { return _state; }
        }

        public bool Open()
        {
            try
            {
                _state = DatasetState.unknown;
                _elements.Clear();

                FileInfo fi_gml = new FileInfo(_connectionString);
                if (!fi_gml.Exists) return false;
                FileInfo fi_xsd = new FileInfo(fi_gml.FullName.Substring(0, fi_gml.FullName.Length - fi_gml.Extension.Length) + ".xsd");
                if (!fi_xsd.Exists) return false;

                _gml_file = fi_gml.FullName;
                _xsd_file = fi_xsd.FullName;

                XmlDocument schema = new XmlDocument();
                schema.Load(fi_xsd.FullName);
                XmlSchemaReader schemaReader = new XmlSchemaReader(schema);
                string targetNamespace = schemaReader.TargetNamespaceURI;
                if (targetNamespace == String.Empty) return false;

                PlugInManager compMan = new PlugInManager();
                foreach (string elementName in schemaReader.ElementNames)
                {
                    string shapeFieldName;
                    geometryType geomType;
                    Fields fields = schemaReader.ElementFields(elementName, out shapeFieldName, out geomType);
                    FeatureClass fc = new FeatureClass(this, elementName, fields);
                    fc.ShapeFieldName = shapeFieldName;
                    fc.GeometryType = geomType;
                    IFeatureLayer layer = LayerFactory.Create(fc) as IFeatureLayer;
                    if (layer == null) continue;

                    //layer.FeatureRenderer = compMan.getComponent(KnownObjects.Carto_UniversalGeometryRenderer) as IFeatureRenderer;

                    _elements.Add(layer);
                }

                XmlTextReader xmlTextReader = new XmlTextReader(fi_gml.FullName);
                xmlTextReader.ReadToDescendant("boundedBy", "http://www.opengis.net/gml");
                string boundedBy = xmlTextReader.ReadOuterXml();

                _doc = new XmlDocument();
                _doc.LoadXml(boundedBy);
                _ns = new XmlNamespaceManager(_doc.NameTable);
                _ns.AddNamespace("GML", "http://www.opengis.net/gml");
                _ns.AddNamespace("WFS", "http://www.opengis.net/wfs");
                _ns.AddNamespace("OGC", "http://www.opengis.net/ogc");
                _ns.AddNamespace("myns", targetNamespace);
                XmlNode boundedByNode = _doc.ChildNodes[0];

                if (boundedByNode != null)
                {
                    XmlNode geomNode = boundedByNode.SelectSingleNode("GML:*", _ns);
                    if (geomNode != null)
                    {
                        _envelope = GeometryTranslator.GML2Geometry(geomNode.OuterXml, _gmlVersion) as IEnvelope;
                        if (geomNode.Attributes["srsName"] != null)
                        {
                            _sRef = gView.Framework.Geometry.SpatialReference.FromID(geomNode.Attributes["srsName"].Value);
                        }
                    }
                }

                _state = DatasetState.opened;
                return true;
            }
            catch(Exception ex)
            {
                _errMsg = ex.Message;
                return false;
            }
        }

        public string lastErrorMsg
        {
            get { return _errMsg; }
        }

        public List<IDatasetElement> Elements
        {
            get 
            {
                return ListOperations<IDatasetElement>.Clone(_elements);
            }
        }

        public string Query_FieldPrefix
        {
            get { return ""; }
        }

        public string Query_FieldPostfix
        {
            get { return ""; }
        }

        public gView.Framework.FDB.IDatabase Database
        {
            get { return _database; }
        }

        public IDatasetElement this[string title]
        {
            get
            {
                foreach (IDatasetElement element in _elements)
                    if (element.Title == title) return element;

                try
                {
                    DirectoryInfo di = new DirectoryInfo(_connectionString);
                    if (!di.Exists) return null;

                    Dataset ds = new Dataset();
                    ds.ConnectionString = di + @"\" + title + ".gml";
                    if (ds.Open())
                    {
                        return ds[title];
                    }
                }
                catch { }

                return null;
            }
        }

        public void RefreshClasses()
        {
        }
        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            
        }

        #endregion
    }
}
