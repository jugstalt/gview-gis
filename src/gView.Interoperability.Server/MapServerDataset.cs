using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Common;
using gView.Framework.Data;
using gView.Framework.Data.Metadata;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.Web.Abstraction;
using gView.Framework.Web.Services;
using gView.Server.Connector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace gView.Interoperability.Server
{
    [RegisterPlugIn("654AF5E2-6159-4342-ABA8-0332CD71D990")]
    public class MapServerDataset : DatasetMetadata, IFeatureDataset, IPersistable
    {
        internal readonly IHttpService _http;

        internal string _connection = "";
        internal string _name = "";

        //internal List<string> _layerIDs = new List<string>();
        internal List<IWebServiceTheme> _themes = new List<IWebServiceTheme>();

        internal bool _opened = false;
        private IWebServiceClass _class = null;
        private IEnvelope _envelope;
        private DatasetState _state = DatasetState.unknown;

        public MapServerDataset()
        {
            _http = HttpService.CreateInstance();
        }

        public MapServerDataset(string connection, string name)
            : this()
        {
            _connection = connection;
            _name = name;

            _class = new MapServerClass(this);
        }

        #region IFeatureDataset Member

        public Task<IEnvelope> Envelope()
        {
            return Task.FromResult(_envelope);
        }

        public Task<ISpatialReference> GetSpatialReference()
        {
            return Task.FromResult<ISpatialReference>(_class?.SpatialReference);
        }

        public void SetSpatialReference(ISpatialReference sRef)
        {
        }

        #endregion IFeatureDataset Member

        #region IDataset Member

        public void Dispose()
        {
        }

        public string ConnectionString
        {
            get
            {
                return _connection + ";service=" + _name;
            }
            set
            {
                _connection = "server=" + ConfigTextStream.ExtractValue(value, "server") +
                                ";user=" + ConfigTextStream.ExtractValue(value, "user") +
                                ";pwd=" + ConfigTextStream.ExtractValue(value, "pwd");
                _name = ConfigTextStream.ExtractValue(value, "service");
            }
        }

        public Task<bool> SetConnectionString(string connectionString)
        {
            this.ConnectionString = connectionString;
            return Task.FromResult(true);
        }

        public string DatasetGroupName
        {
            get { return "gView.MapServer"; }
        }

        public string DatasetName
        {
            get { return "MapServer Map"; }
        }

        public string ProviderName
        {
            get { return "gView"; }
        }

        public DatasetState State
        {
            get { return _state; }
        }

        async public Task<bool> Open()
        {
            try
            {
                _opened = true;
                _themes.Clear();

                ServerConnection server = new ServerConnection(ConfigTextStream.ExtractValue(_connection, "server"));
                string axl = "<ARCXML version=\"1.1\"><REQUEST><GET_SERVICE_INFO fields=\"true\" envelope=\"true\" renderer=\"false\" extensions=\"false\" gv_meta=\"true\" /></REQUEST></ARCXML>";
                axl = await server.SendAsync(_name, axl, "BB294D9C-A184-4129-9555-398AA70284BC",
                    ConfigTextStream.ExtractValue(_connection, "user"),
                    ConfigTextStream.ExtractValue(_connection, "pwd"));

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(axl);

                if (_class == null)
                {
                    _class = new MapServerClass(this);
                }

                double dpi = 96.0;
                XmlNode screen = doc.SelectSingleNode("//ENVIRONMENT/SCREEN");
                if (screen != null)
                {
                    if (screen.Attributes["dpi"] != null)
                    {
                        dpi = Convert.ToDouble(screen.Attributes["dpi"].Value.Replace(".", ","));
                    }
                }
                double dpm = (dpi / 0.0254);

                XmlNode spatialReference = doc.SelectSingleNode("//PROPERTIES/SPATIALREFERENCE");
                if (spatialReference != null)
                {
                    if (spatialReference.Attributes["param"] != null)
                    {
                        SpatialReference sRef = new SpatialReference();
                        gView.Framework.Geometry.SpatialReference.FromProj4(sRef, spatialReference.Attributes["param"].Value);

                        if (spatialReference.Attributes["name"] != null)
                        {
                            sRef.Name = spatialReference.Attributes["name"].Value;
                        }

                        _class.SpatialReference = sRef;
                    }
                }
                else
                {
                    XmlNode FeatureCoordSysNode = doc.SelectSingleNode("ARCXML/RESPONSE/SERVICEINFO/PROPERTIES/FEATURECOORDSYS");
                    if (FeatureCoordSysNode != null)
                    {
                        if (FeatureCoordSysNode.Attributes["id"] != null)
                        {
                            _class.SpatialReference = gView.Framework.Geometry.SpatialReference.FromID("epsg:" + FeatureCoordSysNode.Attributes["id"].Value);
                        }
                        else if (FeatureCoordSysNode.Attributes["string"] != null)
                        {
                            _class.SpatialReference = gView.Framework.Geometry.SpatialReference.FromWKT(FeatureCoordSysNode.Attributes["string"].Value);
                        }

                        // TODO: Geogr. Datum aus "datumtransformid" und "datumtransformstring"
                        //if (_sRef != null && FeatureCoordSysNode.Attributes["datumtransformstring"] != null)
                        //{
                        //}
                    }
                }

                foreach (XmlNode envelopeNode in doc.SelectNodes("//ENVELOPE"))
                {
                    if (_envelope == null)
                    {
                        _envelope = (new Envelope(envelopeNode)).MakeValid();
                    }
                    else
                    {
                        _envelope.Union((new Envelope(envelopeNode)).MakeValid());
                    }
                }
                foreach (XmlNode layerNode in doc.SelectNodes("//LAYERINFO[@id]"))
                {
                    bool visible = true;

                    ISpatialReference sRef = _class.SpatialReference;
                    /*
                    spatialReference = doc.SelectSingleNode("//PROPERTIES/SPATIALREFERENCE");
                    if (spatialReference != null)
                    {
                        if (spatialReference.Attributes["param"] != null)
                        {
                            sRef = new SpatialReference();
                            gView.Framework.Geometry.SpatialReference.FromProj4(sRef, spatialReference.Attributes["param"].Value);

                            if (spatialReference.Attributes["name"] != null)
                                ((SpatialReference)sRef).Name = spatialReference.Attributes["name"].Value;
                        }
                    }
                    else
                    {
                        XmlNode FeatureCoordSysNode = doc.SelectSingleNode("ARCXML/RESPONSE/SERVICEINFO/PROPERTIES/FEATURECOORDSYS");
                        if (FeatureCoordSysNode != null)
                        {
                            if (FeatureCoordSysNode.Attributes["id"] != null)
                            {
                                sRef = gView.Framework.Geometry.SpatialReference.FromID("epsg:" + FeatureCoordSysNode.Attributes["id"].Value);
                            }
                            else if (FeatureCoordSysNode.Attributes["string"] != null)
                            {
                                sRef = gView.Framework.Geometry.SpatialReference.FromWKT(FeatureCoordSysNode.Attributes["string"].Value);
                            }

                            // TODO: Geogr. Datum aus "datumtransformid" und "datumtransformstring"
                            //if (_sRef != null && FeatureCoordSysNode.Attributes["datumtransformstring"] != null)
                            //{
                            //}
                        }
                    }
                    */

                    if (layerNode.Attributes["visible"] != null)
                    {
                        bool.TryParse(layerNode.Attributes["visible"].Value, out visible);
                    }

                    IClass themeClass = null;
                    IWebServiceTheme theme = null;
                    if (layerNode.Attributes["type"] != null && layerNode.Attributes["type"].Value == "featureclass")
                    {
                        themeClass = new MapThemeFeatureClass(this, layerNode.Attributes["id"].Value);
                        ((MapThemeFeatureClass)themeClass).Name = layerNode.Attributes["name"] != null ? layerNode.Attributes["name"].Value : layerNode.Attributes["id"].Value;
                        ((MapThemeFeatureClass)themeClass).fieldsFromAXL = layerNode.InnerXml;
                        ((MapThemeFeatureClass)themeClass).SpatialReference = sRef;

                        XmlNode FCLASS = layerNode.SelectSingleNode("FCLASS[@type]");
                        if (FCLASS != null)
                        {
                            ((MapThemeFeatureClass)themeClass).fClassTypeString = FCLASS.Attributes["type"].Value;
                        }
                        theme = LayerFactory.Create(themeClass, _class) as IWebServiceTheme;
                        if (theme == null)
                        {
                            continue;
                        }

                        theme.Visible = visible;
                    }
                    else if (layerNode.Attributes["type"] != null && layerNode.Attributes["type"].Value == "image")
                    {
                        if (layerNode.SelectSingleNode("gv_meta/class/implements[@type='gView.Framework.Data.IPointIdentify']") != null)
                        {
                            themeClass = new MapThemeQueryableRasterClass(this, layerNode.Attributes["id"].Value);
                            ((MapThemeQueryableRasterClass)themeClass).Name = layerNode.Attributes["name"] != null ? layerNode.Attributes["name"].Value : layerNode.Attributes["id"].Value;
                        }
                        else
                        {
                            themeClass = new MapThemeRasterClass(this, layerNode.Attributes["id"].Value);
                            ((MapThemeRasterClass)themeClass).Name = layerNode.Attributes["name"] != null ? layerNode.Attributes["name"].Value : layerNode.Attributes["id"].Value;
                        }
                        theme = new WebServiceTheme(
                            themeClass,
                            themeClass.Name,
                            layerNode.Attributes["id"].Value,
                            visible,
                            _class);
                    }
                    else
                    {
                        continue;
                    }

                    try
                    {
                        if (layerNode.Attributes["minscale"] != null)
                        {
                            theme.MinimumScale = Convert.ToDouble(layerNode.Attributes["minscale"].Value.Replace(".", ",")) * dpm;
                        }

                        if (layerNode.Attributes["maxscale"] != null)
                        {
                            theme.MaximumScale = Convert.ToDouble(layerNode.Attributes["maxscale"].Value.Replace(".", ",")) * dpm;
                        }
                    }
                    catch { }
                    _themes.Add(theme);
                }
                _state = DatasetState.opened;
                return true;
            }
            catch (Exception /*ex*/)
            {
                _state = DatasetState.unknown;
                _class = null;

                return false;
            }
        }

        public string LastErrorMessage
        {
            get { return ""; }
            set { }
        }

        public int order
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        public IDatasetEnum DatasetEnum
        {
            get { return null; }
        }

        public Task<List<IDatasetElement>> Elements()
        {
            List<IDatasetElement> elements = new List<IDatasetElement>();
            if (_class != null)
            {
                elements.Add(new DatasetElement(_class));
            }
            return Task.FromResult(elements);
        }

        public string Query_FieldPrefix
        {
            get { return ""; }
        }

        public string Query_FieldPostfix
        {
            get { return ""; }
        }

        public IDatabase Database
        {
            get { return null; }
        }

        public Task<IDatasetElement> Element(string title)
        {
            if (_class != null && title == _class.Name)
            {
                return Task.FromResult<IDatasetElement>(new DatasetElement(_class));
            }

            return Task.FromResult<IDatasetElement>(null);
        }

        public Task RefreshClasses()
        {
            return Task.CompletedTask;
        }

        #endregion IDataset Member

        #region IPersistable Member

        public string PersistID
        {
            get { return ""; }
        }

        public Task<bool> LoadAsync(IPersistStream stream)
        {
            Load(stream);
            return Task.FromResult(true);
        }

        public void Load(IPersistStream stream)
        {
            this.ConnectionString = (string)stream.Load("ConnectionString", "");
            _class = new MapServerClass(this);
            var _ = Open().Result;  // todo: is it nessessary to open on load?!
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("ConnectionString", this.ConnectionString);
        }

        #endregion IPersistable Member
    }
}