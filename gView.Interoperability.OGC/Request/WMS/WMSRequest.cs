using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.IO;
using gView.MapServer;
using gView.Framework.system;
using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.Data;
using gView.Framework.OGC.WMS;
using System.Globalization;
using gView.Framework.UI;
using gView.Framework.OGC.GML;
using System.Drawing;
using gView.Interoperability.OGC.SLD;
using gView.Framework.IO;
using System.Reflection;
using gView.Framework.Metadata;
using gView.Framework.OGC.WMS_C_1_4_0;
using System.Xml.Serialization;

namespace gView.Interoperability.OGC
{
    [gView.Framework.system.RegisterPlugIn("C4892F49-446C-4e22-BCC7-76F033F1F03B")]
    public class WMSRequest : IServiceRequestInterpreter
    {
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        private IMapServer _mapServer = null;
        //private IServiceRequestInterpreter _arcXML = null;
        protected bool _useTOC;
        private static Guid _tilemetaprovider = new Guid("D33D3DD2-DD63-4a47-9F84-F840FE0D01C0");

        public WMSRequest()
        {
            _useTOC = false;
        }
        #region IServiceRequestInterpreter Member

        public void OnCreate(IMapServer mapServer)
        {
            _mapServer = mapServer;

            //ComponentManager compMan = new ComponentManager();
            //_arcXML = compMan.getComponent(new Guid("BB294D9C-A184-4129-9555-398AA70284BC")) as IServiceRequestInterpreter;
            //if (_arcXML != null) _arcXML.OnCreate(mapServer);
        }

        public void Request(IServiceRequestContext context)
        {
            if (context == null || context.ServiceRequest == null)
                return;

            if (_mapServer == null)
            {
                return;
            }

            WMSParameterDescriptor parameters = new WMSParameterDescriptor();
            if (!parameters.ParseParameters(context.ServiceRequest.Request.Split('&')))
            {
                _mapServer.Log("Invalid WMS Parameters", loggingMethod.error, context.ServiceRequest.Request);
                return;
            }

            //ServiceRequestContext context = new ServiceRequestContext(_mapServer, this, request);
            switch (parameters.Request)
            {
                case WMSRequestType.GetCapabilities:
                    context.ServiceRequest.Response = WMS_GetCapabilities(context.ServiceRequest.OnlineResource, context.ServiceRequest.Service, parameters, context);
                    break;
                case WMSRequestType.GetMap:
                    context.ServiceRequest.Response = WMS_GetMap(context.ServiceRequest.Service, parameters, context);
                    break;
                case WMSRequestType.GetFeatureInfo:
                    context.ServiceRequest.Response = WMS_FeatureInfo(context.ServiceRequest.Service, parameters, context);
                    break;
                case WMSRequestType.DescriptTiles:
                    context.ServiceRequest.Response = WMSC_DescriptTiles(context.ServiceRequest.Service, parameters, context);
                    break;
                case WMSRequestType.GetTile:
                    context.ServiceRequest.Response = WMSC_GetTile(context.ServiceRequest.Service, parameters, context);
                    break;

                case WMSRequestType.GenerateTiles:
                    context.ServiceRequest.Response = WMS_GenerateTiles(context.ServiceRequest.Service, parameters, context);
                    break;
            }
        }

        virtual public string IntentityName
        {
            get { return "wms"; }
        }

        public InterpreterCapabilities Capabilities
        {
            get
            {
                return new InterpreterCapabilities(new InterpreterCapabilities.Capability[]{
                    new InterpreterCapabilities.LinkCapability("GetCapabilities","{onlineresource}VERSION=1.1.1&SERVICE=WMS&REQUEST=GetCapabilities","1.1.1"),
                    new InterpreterCapabilities.SimpleCapability("GetMap","{onlineresource}VERSION=1.1.1&SERVICE=WMS&REQUEST=GetMap&...","1.1.1"),
                    new InterpreterCapabilities.SimpleCapability("GetFeatureInfo","{onlineresource}VERSION=1.1.1&SERVICE=WMS&REQUEST=GetFeatureInfo&...","1.1.1"),
                    new InterpreterCapabilities.LinkCapability("GetCapabilities","{onlineresource}VERSION=1.3.0&SERVICE=WMS&REQUEST=GetCapabilities","1.3.0"),
                    new InterpreterCapabilities.SimpleCapability("GetMap","{onlineresource}VERSION=1.3.0&SERVICE=WMS&REQUEST=GetMap&...","1.3.0"),
                    new InterpreterCapabilities.SimpleCapability("GetFeatureInfo","{onlineresource}VERSION=1.3.0&SERVICE=WMS&REQUEST=GetFeatureInfo&...","1.3.0")
                }
                );
            }
        }

        #endregion

        private string WMS_GetCapabilities(string OnlineResource, string service, WMSParameterDescriptor parameters, IServiceRequestContext context)
        {
            try
            {
                if (_mapServer == null || parameters == null) return "";
                _mapServer.Log("Service:" + service, loggingMethod.request, "WMS GetCapabilities");

                //if (parameters != null)
                //    _mapServer.Log(loggingMethod.request_detail, "WMS: " + Concat(parameters, "&"));

                using (IServiceMap map = context.ServiceMap)// _mapServer[context];
                {
                    if (map == null) return "";

                    WMS_Export_Metadata exMetadata = map.MetadataProvider(new Guid("0F6317BC-38FD-41d3-8E1A-82AB1873C526")) as WMS_Export_Metadata;
                    WMS_Export_Metadata.Metadata metadata = ((exMetadata != null) ? exMetadata.Data : null);
                    if (metadata == null)
                    {
                        metadata = new WMS_Export_Metadata.Metadata(
                            ((map.Display != null && map.Display.SpatialReference != null) ?
                            map.Display.SpatialReference.Name : String.Empty));
                    }

                    NumberFormatInfo nfi = new NumberFormatInfo();
                    nfi.NumberDecimalSeparator = ".";

                    string sOnlineResource = OnlineResource + "?SERVICE=WMS&";
                    sOnlineResource = sOnlineResource.Replace("??", "?");

                    if (parameters.Version == "1.1.1")
                    {
                        gView.Framework.OGC.WMS.Version_1_1_1.WMT_MS_Capabilities caps = new Framework.OGC.WMS.Version_1_1_1.WMT_MS_Capabilities();

                        #region Service
                        caps.Service = new Framework.OGC.WMS.Version_1_1_1.Service();
                        caps.Service.Name = map.Name;
                        caps.Service.Title = "gView Gis OS WMS: " + map.Name;
                        caps.Service.OnlineResource = new Framework.OGC.WMS.Version_1_1_1.OnlineResource();
                        caps.Service.OnlineResource.href = sOnlineResource;
                        caps.Service.Fees = "none";
                        caps.Service.ContactInformation = new Framework.OGC.WMS.Version_1_1_1.ContactInformation();
                        caps.Service.AccessConstraints = "none";
                        #endregion

                        #region Capability
                        caps.Capability = new Framework.OGC.WMS.Version_1_1_1.Capability();

                        #region Request
                        caps.Capability.Request = new Framework.OGC.WMS.Version_1_1_1.Request();

                        #region GetCapabilities
                        caps.Capability.Request.GetCapabilities = new Framework.OGC.WMS.Version_1_1_1.GetCapabilities();
                        caps.Capability.Request.GetCapabilities.Format = new string[] { "application/vnd.ogc.wms_xml" };
                        caps.Capability.Request.GetCapabilities.DCPType = new Framework.OGC.WMS.Version_1_1_1.DCPType[]{
                        new Framework.OGC.WMS.Version_1_1_1.DCPType()
                    };
                        caps.Capability.Request.GetCapabilities.DCPType[0].HTTP = new object[]{
                        new gView.Framework.OGC.WMS.Version_1_1_1.Get()
                    };
                        ((gView.Framework.OGC.WMS.Version_1_1_1.Get)caps.Capability.Request.GetCapabilities.DCPType[0].HTTP[0]).OnlineResource = new Framework.OGC.WMS.Version_1_1_1.OnlineResource();
                        ((gView.Framework.OGC.WMS.Version_1_1_1.Get)caps.Capability.Request.GetCapabilities.DCPType[0].HTTP[0]).OnlineResource.href = sOnlineResource;
                        #endregion

                        #region GetMap
                        caps.Capability.Request.GetMap = new Framework.OGC.WMS.Version_1_1_1.GetMap();
                        caps.Capability.Request.GetMap.Format = new string[] { "image/png", "image/jpeg" };
                        caps.Capability.Request.GetMap.DCPType = new Framework.OGC.WMS.Version_1_1_1.DCPType[]{
                        new Framework.OGC.WMS.Version_1_1_1.DCPType()
                    };
                        caps.Capability.Request.GetMap.DCPType[0].HTTP = new object[]{
                        new gView.Framework.OGC.WMS.Version_1_1_1.Get()
                    };
                        ((gView.Framework.OGC.WMS.Version_1_1_1.Get)caps.Capability.Request.GetMap.DCPType[0].HTTP[0]).OnlineResource = new Framework.OGC.WMS.Version_1_1_1.OnlineResource();
                        ((gView.Framework.OGC.WMS.Version_1_1_1.Get)caps.Capability.Request.GetMap.DCPType[0].HTTP[0]).OnlineResource.href = sOnlineResource;
                        #endregion

                        #region GetFeatureInfo

                        List<string> gfiFormats = new List<string>(new string[] { "text/plain", "text/html", "text/xml", "application/vnd.ogc.gml" });
                        DirectoryInfo di = new DirectoryInfo(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\misc\wms\GetFeatureInfo\xsl");
                        if (di.Exists)
                        {
                            foreach (FileInfo fi in di.GetFiles("*.xsl"))
                            {
                                gfiFormats.Add("xsl/" + fi.Name);
                            }
                        }

                        caps.Capability.Request.GetFeatureInfo = new Framework.OGC.WMS.Version_1_1_1.GetFeatureInfo();
                        caps.Capability.Request.GetFeatureInfo.Format = gfiFormats.ToArray();
                        caps.Capability.Request.GetFeatureInfo.DCPType = new Framework.OGC.WMS.Version_1_1_1.DCPType[]{
                        new Framework.OGC.WMS.Version_1_1_1.DCPType()
                    };
                        caps.Capability.Request.GetFeatureInfo.DCPType[0].HTTP = new object[]{
                        new gView.Framework.OGC.WMS.Version_1_1_1.Get()
                    };
                        ((gView.Framework.OGC.WMS.Version_1_1_1.Get)caps.Capability.Request.GetFeatureInfo.DCPType[0].HTTP[0]).OnlineResource = new Framework.OGC.WMS.Version_1_1_1.OnlineResource();
                        ((gView.Framework.OGC.WMS.Version_1_1_1.Get)caps.Capability.Request.GetFeatureInfo.DCPType[0].HTTP[0]).OnlineResource.href = sOnlineResource;
                        #endregion

                        #endregion

                        #region Exception
                        caps.Capability.Exception = new string[] { "application/vnd.ogc.se_xml", "application/vnd.ogc.se_inimage", "application/vnd.ogc.se_blank" };
                        #endregion

                        #region UserDefinedSymbolization
                        caps.Capability.UserDefinedSymbolization = new Framework.OGC.WMS.Version_1_1_1.UserDefinedSymbolization();
                        caps.Capability.UserDefinedSymbolization.SupportSLD = Framework.OGC.WMS.Version_1_1_1.UserDefinedSymbolizationSupportSLD.Item1;
                        caps.Capability.UserDefinedSymbolization.RemoteWFS = Framework.OGC.WMS.Version_1_1_1.UserDefinedSymbolizationRemoteWFS.Item0;
                        caps.Capability.UserDefinedSymbolization.UserStyle = Framework.OGC.WMS.Version_1_1_1.UserDefinedSymbolizationUserStyle.Item1;
                        caps.Capability.UserDefinedSymbolization.UserLayer = Framework.OGC.WMS.Version_1_1_1.UserDefinedSymbolizationUserLayer.Item0;
                        #endregion

                        #region Layer
                        caps.Capability.Layer = new Framework.OGC.WMS.Version_1_1_1.Layer();
                        caps.Capability.Layer.Name = map.Name;
                        caps.Capability.Layer.SRS = metadata.EPSGCodes;

                        IEnvelope env4326 = GetEPSG4326Envelope(map);
                        if (env4326 != null)
                        {
                            Framework.OGC.WMS.Version_1_1_1.BoundingBox[] bboxes = new Framework.OGC.WMS.Version_1_1_1.BoundingBox[0];
                            foreach (string s in caps.Capability.Layer.SRS)
                            {
                                IEnvelope env = TransFormEPSG4326Envelope(env4326, s);
                                if (env == null) continue;

                                Array.Resize<Framework.OGC.WMS.Version_1_1_1.BoundingBox>(ref bboxes, bboxes.Length + 1);
                                bboxes[bboxes.Length - 1] = new Framework.OGC.WMS.Version_1_1_1.BoundingBox();
                                bboxes[bboxes.Length - 1].SRS = s;
                                bboxes[bboxes.Length - 1].minx = env.minx.ToString(_nhi);
                                bboxes[bboxes.Length - 1].miny = env.miny.ToString(_nhi);
                                bboxes[bboxes.Length - 1].maxx = env.maxx.ToString(_nhi);
                                bboxes[bboxes.Length - 1].maxy = env.maxy.ToString(_nhi);
                            }
                            caps.Capability.Layer.BoundingBox = bboxes;
                            caps.Capability.Layer.LatLonBoundingBox = new Framework.OGC.WMS.Version_1_1_1.LatLonBoundingBox();
                            caps.Capability.Layer.LatLonBoundingBox.minx = env4326.minx.ToString(_nhi);
                            caps.Capability.Layer.LatLonBoundingBox.miny = env4326.miny.ToString(_nhi);
                            caps.Capability.Layer.LatLonBoundingBox.maxx = env4326.maxx.ToString(_nhi);
                            caps.Capability.Layer.LatLonBoundingBox.maxy = env4326.maxy.ToString(_nhi);
                        }

                        List<Framework.OGC.WMS.Version_1_1_1.Layer> fTypes = new List<Framework.OGC.WMS.Version_1_1_1.Layer>();
                        foreach (MapServerHelper.Layers layers in MapServerHelper.MapLayers(map, _useTOC))
                        {
                            if (layers == null || layers.FirstLayer == null || layers.FirstClass == null) continue;

                            Framework.OGC.WMS.Version_1_1_1.Layer fType = new Framework.OGC.WMS.Version_1_1_1.Layer();
                            fType.Name = "c" + layers.ID;
                            fType.Title = layers.Title;
                            IClass c = layers.FirstClass;
                            fType.queryable = c is IFeatureClass ? Framework.OGC.WMS.Version_1_1_1.LayerQueryable.Item1 : Framework.OGC.WMS.Version_1_1_1.LayerQueryable.Item0;

                            if (layers.MinScale > 1.0 || layers.MaxScale > 1.0)
                            {
                                fType.ScaleHint = new Framework.OGC.WMS.Version_1_1_1.ScaleHint();
                                if (layers.MinScale > 1.0)
                                    fType.ScaleHint.min = (layers.MinScale / 2004.4).ToString(_nhi);
                                if (layers.MaxScale > 1.0)
                                    fType.ScaleHint.max = (layers.MaxScale / 2004.4).ToString(_nhi);
                            }
                            fTypes.Add(fType);
                        }
                        caps.Capability.Layer.Layer1 = fTypes.ToArray();
                        #endregion

                        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                        ns.Add("xlink", "http://www.w3.org/1999/xlink");
                        XsdSchemaSerializer<gView.Framework.OGC.WMS.Version_1_1_1.WMT_MS_Capabilities> ser = new XsdSchemaSerializer<gView.Framework.OGC.WMS.Version_1_1_1.WMT_MS_Capabilities>();
                        string xml = ser.Serialize(caps, ns);
                        return xml;

                        #endregion

                    }
                    else if (parameters.Version == "1.3.0")
                    {
                        gView.Framework.OGC.WMS.Version_1_3_0.WMS_Capabilities caps = new Framework.OGC.WMS.Version_1_3_0.WMS_Capabilities();

                        #region Service
                        caps.Service = new Framework.OGC.WMS.Version_1_3_0.Service();
                        caps.Service.Name = new Framework.OGC.WMS.Version_1_3_0.ServiceName();
                        caps.Service.Title = map.Name;
                        caps.Service.OnlineResource = new Framework.OGC.WMS.Version_1_3_0.OnlineResource();
                        caps.Service.OnlineResource.href = sOnlineResource;
                        caps.Service.ContactInformation = new Framework.OGC.WMS.Version_1_3_0.ContactInformation();
                        caps.Service.Fees = "none";
                        caps.Service.AccessConstraints = "none";
                        caps.Service.MaxHeight = caps.Service.MaxWidth = "8000";
                        #endregion

                        #region Capability
                        caps.Capability = new Framework.OGC.WMS.Version_1_3_0.Capability();

                        #region Request
                        caps.Capability.Request = new Framework.OGC.WMS.Version_1_3_0.Request();

                        #region GetCapabilities
                        caps.Capability.Request.GetCapabilities = new Framework.OGC.WMS.Version_1_3_0.OperationType();
                        caps.Capability.Request.GetCapabilities.Format = new string[] { "text/xml" };
                        caps.Capability.Request.GetCapabilities.DCPType = new Framework.OGC.WMS.Version_1_3_0.DCPType[]{
                        new Framework.OGC.WMS.Version_1_3_0.DCPType()
                    };
                        caps.Capability.Request.GetCapabilities.DCPType[0].HTTP = new Framework.OGC.WMS.Version_1_3_0.HTTP();
                        caps.Capability.Request.GetCapabilities.DCPType[0].HTTP.Get = new Framework.OGC.WMS.Version_1_3_0.Get();
                        caps.Capability.Request.GetCapabilities.DCPType[0].HTTP.Get.OnlineResource = new Framework.OGC.WMS.Version_1_3_0.OnlineResource();
                        caps.Capability.Request.GetCapabilities.DCPType[0].HTTP.Get.OnlineResource.href = sOnlineResource;
                        #endregion

                        #region GetMap
                        caps.Capability.Request.GetMap = new Framework.OGC.WMS.Version_1_3_0.OperationType();
                        caps.Capability.Request.GetMap.Format = new string[] { "image/png", "image/jpeg" };
                        caps.Capability.Request.GetMap.DCPType = new Framework.OGC.WMS.Version_1_3_0.DCPType[]{
                        new Framework.OGC.WMS.Version_1_3_0.DCPType()
                    };
                        caps.Capability.Request.GetMap.DCPType[0].HTTP = new Framework.OGC.WMS.Version_1_3_0.HTTP();
                        caps.Capability.Request.GetMap.DCPType[0].HTTP.Get = new Framework.OGC.WMS.Version_1_3_0.Get();
                        caps.Capability.Request.GetMap.DCPType[0].HTTP.Get.OnlineResource = new Framework.OGC.WMS.Version_1_3_0.OnlineResource();
                        caps.Capability.Request.GetMap.DCPType[0].HTTP.Get.OnlineResource.href = sOnlineResource;
                        #endregion

                        #region GetFeatureInfo

                        List<string> gfiFormats = new List<string>(new string[] { "text/plain", "text/html", "text/xml", "application/vnd.ogc.gml" });
                        DirectoryInfo di = new DirectoryInfo(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\misc\wms\GetFeatureInfo\xsl");
                        if (di.Exists)
                        {
                            foreach (FileInfo fi in di.GetFiles("*.xsl"))
                            {
                                gfiFormats.Add("xsl/" + fi.Name);
                            }
                        }

                        caps.Capability.Request.GetFeatureInfo = new Framework.OGC.WMS.Version_1_3_0.OperationType();
                        caps.Capability.Request.GetFeatureInfo.Format = gfiFormats.ToArray();
                        caps.Capability.Request.GetFeatureInfo.DCPType = new Framework.OGC.WMS.Version_1_3_0.DCPType[]{
                        new Framework.OGC.WMS.Version_1_3_0.DCPType()
                    };
                        caps.Capability.Request.GetFeatureInfo.DCPType[0].HTTP = new Framework.OGC.WMS.Version_1_3_0.HTTP();
                        caps.Capability.Request.GetFeatureInfo.DCPType[0].HTTP.Get = new Framework.OGC.WMS.Version_1_3_0.Get();
                        caps.Capability.Request.GetFeatureInfo.DCPType[0].HTTP.Get.OnlineResource = new Framework.OGC.WMS.Version_1_3_0.OnlineResource();
                        caps.Capability.Request.GetFeatureInfo.DCPType[0].HTTP.Get.OnlineResource.href = sOnlineResource;
                        #endregion

                        #region Exception
                        caps.Capability.Exception = new string[] { "XML", "INIMAGE", "BLANK" };
                        #endregion

                        #region UserDefinedSymbolization

                        #endregion

                        #region Layer
                        caps.Capability.Layer = new Framework.OGC.WMS.Version_1_3_0.Layer[]{
                        new Framework.OGC.WMS.Version_1_3_0.Layer()
                    };
                        caps.Capability.Layer[0].Name = map.Name;
                        caps.Capability.Layer[0].Title = map.Name;
                        caps.Capability.Layer[0].CRS = metadata.EPSGCodes;

                        IEnvelope env4326 = GetEPSG4326Envelope(map);
                        if (env4326 != null)
                        {
                            Framework.OGC.WMS.Version_1_3_0.BoundingBox[] bboxes = new Framework.OGC.WMS.Version_1_3_0.BoundingBox[0];
                            foreach (string s in caps.Capability.Layer[0].CRS)
                            {
                                IEnvelope env = TransFormEPSG4326Envelope(env4326, s);
                                if (env == null) continue;

                                Array.Resize<Framework.OGC.WMS.Version_1_3_0.BoundingBox>(ref bboxes, bboxes.Length + 1);
                                bboxes[bboxes.Length - 1] = new Framework.OGC.WMS.Version_1_3_0.BoundingBox();
                                bboxes[bboxes.Length - 1].CRS = s;
                                bboxes[bboxes.Length - 1].minx = env.minx;
                                bboxes[bboxes.Length - 1].miny = env.miny;
                                bboxes[bboxes.Length - 1].maxx = env.maxx;
                                bboxes[bboxes.Length - 1].maxy = env.maxy;
                            }
                            caps.Capability.Layer[0].BoundingBox = bboxes;
                            caps.Capability.Layer[0].EX_GeographicBoundingBox = new Framework.OGC.WMS.Version_1_3_0.EX_GeographicBoundingBox();
                            caps.Capability.Layer[0].EX_GeographicBoundingBox.westBoundLongitude = env4326.minx;
                            caps.Capability.Layer[0].EX_GeographicBoundingBox.southBoundLatitude = env4326.miny;
                            caps.Capability.Layer[0].EX_GeographicBoundingBox.eastBoundLongitude = env4326.maxx;
                            caps.Capability.Layer[0].EX_GeographicBoundingBox.northBoundLatitude = env4326.maxy;
                        }

                        List<Framework.OGC.WMS.Version_1_3_0.Layer> fTypes = new List<Framework.OGC.WMS.Version_1_3_0.Layer>();
                        foreach (MapServerHelper.Layers layers in MapServerHelper.MapLayers(map, _useTOC))
                        {
                            if (layers == null || layers.FirstLayer == null || layers.FirstClass == null) continue;

                            Framework.OGC.WMS.Version_1_3_0.Layer fType = new Framework.OGC.WMS.Version_1_3_0.Layer();
                            fType.Name = "c" + layers.ID;
                            fType.Title = layers.Title;
                            IClass c = layers.FirstClass;
                            fType.queryable = c is IFeatureClass;

                            if (layers.MinScale > 0.0)
                            {
                                fType.MinScaleDenominator = layers.MinScale;
                                fType.MinScaleDenominatorSpecified = true;
                            }
                            if (layers.MaxScale > 0.0)
                            {
                                fType.MaxScaleDenominator = layers.MaxScale;
                                fType.MaxScaleDenominatorSpecified = true;
                            }

                            //fType.Style = new Framework.OGC.WMS.Version_1_3_0.Style[] {
                            //};

                            fTypes.Add(fType);
                        }
                        caps.Capability.Layer[0].Layer1 = fTypes.ToArray();
                        #endregion
                        #endregion
                        #endregion

                        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                        ns.Add("xlink", "http://www.w3.org/1999/xlink");
                        ns.Add("sld", "http://www.opengis.net/sld");
                        ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                        XsdSchemaSerializer<gView.Framework.OGC.WMS.Version_1_3_0.WMS_Capabilities> ser = new XsdSchemaSerializer<gView.Framework.OGC.WMS.Version_1_3_0.WMS_Capabilities>();
                        string xml = ser.Serialize(caps, ns);
                        return xml;
                    }

                    return String.Empty;
                }
            }
            catch (Exception ex)
            {
                _mapServer.Log("Service:" + service, loggingMethod.error, ex.Message + "\r\n" + ex.StackTrace);
                return "";
            }
        }

        virtual public string WMS_GetMap(string service, WMSParameterDescriptor parameters, IServiceRequestContext context)
        {
            // Immer neue Requestklasse erzeugen, damit Request multithreadfähig
            // ist.
            // Parameter wie Layer müssen erhalten bleiben, wenn ServicMap später
            // BeforeRenderlayers aufruft...

            _mapServer.Log("Service:" + service, loggingMethod.request, "WMS GetMap");

            WMS_GetMapRequest request = new WMS_GetMapRequest(_mapServer, service, parameters, _useTOC, context);
            //request.GetLayerByIDCallback = GetLayerByID;

            return request.Request();
        }

        private static object thisLock = new object();
        private static Dictionary<string, object> lockers = new Dictionary<string, object>();
        virtual public string WMS_GenerateTiles(string service, WMSParameterDescriptor parameters, IServiceRequestContext context)
        {
            if (String.IsNullOrEmpty(parameters.RequestKey))
                return String.Empty;

            _mapServer.Log("Service:" + service, loggingMethod.request, "WMS GenerateTiles");

            lock (thisLock)
            {
                if (!lockers.ContainsKey(parameters.RequestKey))
                    lockers.Add(parameters.RequestKey, new object());
            }

            lock (lockers[parameters.RequestKey])
            {
                // todo: zZ nur PNG!!
                string imagePath = context.MapServer.OutputPath + @"\wms_" + parameters.RequestKey + "_{0}_{1}_{2}" + ".png";
                FileInfo fi = new FileInfo(String.Format(imagePath, parameters.ZoomLevel, parameters.TileRow, parameters.TileCol));
                if (fi.Exists)
                {
                    //return context.MapServer.OutputUrl + "/" + fi.Name;
                    return fi.FullName;
                }

                if (parameters.BBoxSRS > 0)
                {
                    ISpatialReference fromSrs = SpatialReference.FromID("epsg:" + parameters.BBoxSRS);
                    ISpatialReference toSrs = SpatialReference.FromID("epsg:" + parameters.SRS);

                    parameters.BBOX = new Envelope((IPoint)GeometricTransformer.Transform2D(parameters.BBOX.LowerLeft, fromSrs, toSrs),
                                                   (IPoint)GeometricTransformer.Transform2D(parameters.BBOX.UpperRight, fromSrs, toSrs));
                }

                int n = (int)Math.Pow(2.0, parameters.ZoomLevel);
                double x0 = -20037508.342789244, y0 = 20037508.342789244;
                double w = 20037508.342789244 * 2 / n, h = w;

                double c0_ = (parameters.BBOX.minx - x0) / w, c1_ = (parameters.BBOX.maxx - x0) / w;
                double r0_ = (y0 - parameters.BBOX.miny) / h, r1_ = (y0 - parameters.BBOX.maxy) / h;

                int c0__ = Math.Max((int)Math.Round(c0_), 0), c1__ = Math.Min((int)Math.Round(c1_), n - 1);
                int r1__ = Math.Min((int)Math.Round(r0_), n - 1), r0__ = Math.Min((int)Math.Round(r1_), 0);
                int c0 = Math.Min(c0__, c1__), c1 = Math.Max(c0__, c1__);
                int r0 = Math.Min(r0__, r1__), r1 = Math.Max(r0__, r1__);

                parameters.Width = (Math.Abs(c1 - c0) + 1) * 256;
                parameters.Height = (Math.Abs(r1 - r0) + 1) * 256;

                parameters.BBOX = new Envelope(x0 + c0 * w,
                                               y0 - r0 * w,
                                               x0 + (c1 + 1) * w,
                                               y0 - (r1 + 1) * h);
                // Immer neue Requestklasse erzeugen, damit Request multithreadfähig
                // ist.
                // Parameter wie Layer müssen erhalten bleiben, wenn ServicMap später
                // BeforeRenderlayers aufruft...
                WMS_GetMapRequest request = new WMS_GetMapRequest(_mapServer, service, parameters, _useTOC, context);
                //request.GetLayerByIDCallback = GetLayerByID;

                string wmsImagePath = request.Request();
                if (!String.IsNullOrEmpty(wmsImagePath))
                {
                    using (Image image = Image.FromFile(wmsImagePath))
                    {
                        for (int r = r0; r <= r1; r++)
                        {
                            for (int c = c0; c <= c1; c++)
                            {
                                using (Bitmap bm = new Bitmap(256, 256))
                                using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bm))
                                {
                                    gr.DrawImage(image, new Rectangle(0, 0, 256, 256), new Rectangle((c - c0) * 256, (r - r0) * 256, 256, 256), GraphicsUnit.Pixel);
                                    string outPath = String.Format(imagePath, parameters.ZoomLevel, r, c);
                                    switch (fi.Extension.ToLower())
                                    {
                                        case ".png":
                                            bm.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
                                            break;
                                        default:
                                            bm.Save(outPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }

                lockers.Remove(parameters.RequestKey);
                return String.Format(imagePath, parameters.ZoomLevel, parameters.TileRow, parameters.TileCol);
            }
        }

        private class WMS_GetMapRequest
        {
            private string _service;
            private WMSParameterDescriptor _parameters;
            private IMapServer _mapServer;
            private bool _useTOC;
            private IServiceRequestContext _context;

            public WMS_GetMapRequest(IMapServer mapServer, string service, WMSParameterDescriptor parameters, bool useTOC, IServiceRequestContext context)
            {
                _mapServer = mapServer;
                _service = service;
                _parameters = parameters;
                _useTOC = useTOC;
                _context = context;
            }

            public string Request()
            {
                if (_mapServer == null) return "";

                using (IServiceMap map = _context.ServiceMap) //_mapServer[_context]
                {
                    if (map == null) return "";

                    ISpatialReference sRef = SpatialReference.FromID("epsg:" + _parameters.SRS);
                    map.Display.SpatialReference = sRef;

                    string ret = "";

                    if (_parameters.Version == "1.3.0" && map.Display.SpatialReference != null)
                    {
                        IPoint ll = _parameters.BBOX.LowerLeft;
                        IPoint ur = _parameters.BBOX.UpperRight;

                        ll = gView.Framework.OGC.GML.GeometryTranslator.Gml3Point(ll, map.Display.SpatialReference);
                        ur = gView.Framework.OGC.GML.GeometryTranslator.Gml3Point(ur, map.Display.SpatialReference);

                        _parameters.BBOX = new Envelope(ll, ur);
                    }

                    map.Display.dpi = _parameters.dpi;
                    map.Display.iWidth = _parameters.Width;
                    map.Display.iHeight = _parameters.Height;
                    map.Display.Limit = _parameters.BBOX;
                    map.Display.ZoomTo(_parameters.BBOX);

                    if (_parameters.Transparent)
                    {
                        map.Display.MakeTransparent = true;
                        map.Display.TransparentColor = System.Drawing.Color.White;
                    }
                    map.BeforeRenderLayers += new BeforeRenderLayersEvent(map_BeforeRenderLayers);
                    map.Render();

                    if (map.MapImage != null)
                    {
                        ret = _mapServer.OutputPath + @"\" + map.Name.Replace(",", "_") + "_" + System.Guid.NewGuid().ToString("N") + ".png";

                        //
                        // Zurück in das richtige Koordinatenfenster verzerren
                        //
                        double minx, miny, maxx, maxy;
                        minx = _parameters.BBOX.minx; miny = _parameters.BBOX.miny;
                        maxx = _parameters.BBOX.maxx; maxy = _parameters.BBOX.maxy;

                        map.Display.World2Image(ref minx, ref miny);
                        map.Display.World2Image(ref maxx, ref maxy);

                        if ((int)Math.Round(minx) != 0 || (int)Math.Round(maxy) != 0 ||
                            (int)Math.Round(maxx) != _parameters.Width || (int)Math.Round(miny) != _parameters.Height)
                        {
                            using (Bitmap bm = new Bitmap(_parameters.Width, _parameters.Height))
                            {
                                using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bm))
                                {
                                    RectangleF sourceRect = new RectangleF((float)minx, (float)maxy, (float)Math.Abs(maxx - minx), (float)Math.Abs(miny - maxy));
                                    gr.DrawImage(map.MapImage,
                                        new RectangleF(0f, 0f, (float)bm.Width, (float)bm.Height),
                                        sourceRect,
                                        GraphicsUnit.Pixel);
                                }

                                if (_parameters.Transparent)
                                {
                                    bm.MakeTransparent(Color.White);
                                }
                                bm.Save(ret, _parameters.GetImageFormat());
                                return "image:" + ret;
                            }
                        }
                        else
                        {
                            if (map.SaveImage(ret, _parameters.GetImageFormat()))
                            {
                                return "image:" + ret;
                            }
                        }
                    }
                    return "";
                }
            }

            void map_BeforeRenderLayers(IServiceMap sender, List<ILayer> layers)
            {
                foreach (ILayer layer in layers)
                {
                    if (layer == null) continue;
                    layer.Visible = false;
                }

                foreach (string id in _parameters.Layers)
                {
                    if (id == String.Empty || id[0] != 'c') continue;

                    foreach (ILayer layer in MapServerHelper.FindMapLayers(sender, _useTOC, id.Substring(1, id.Length - 1), layers))
                    {
                        layer.Visible = true;
                    }
                }

                // SLD
                XmlDocument sld = null;
                if (_parameters.SLD_BODY != String.Empty)
                {
                    try
                    {
                        sld = new XmlDocument();
                        sld.LoadXml(_parameters.SLD_BODY);

                        foreach (XmlNode namedLayer in sld.SelectNodes("StyledLayerDescriptor/NamedLayer"))
                        {
                            XmlNode nameNode = namedLayer.SelectSingleNode("Name");
                            if (nameNode == null) continue;
                            string id = nameNode.InnerText;
                            if (id == String.Empty || id[0] != 'c') continue;

                            SLDRenderer renderer = new SLDRenderer(namedLayer.OuterXml);
                            foreach (ILayer layer in MapServerHelper.FindMapLayers(sender, _useTOC, id.Substring(1, id.Length - 1), layers))
                            {
                                if (layer is IFeatureLayer)
                                    ((IFeatureLayer)layer).FeatureRenderer = renderer;
                            }
                        }
                    }
                    catch /*(Exception ex)*/
                    {
                        //string err = ex.Message;
                    }
                }
            }
        }

        public string WMS_FeatureInfo(string service, WMSParameterDescriptor parameters, IServiceRequestContext context)
        {
            if (parameters == null) return "";

            if (_mapServer == null) return "";

            using (IServiceMap map = context.ServiceMap) // _mapServer[context];
            {
                if (map == null || map.TOC == null) return "";

                ISpatialReference sRef = SpatialReference.FromID("epsg:" + parameters.SRS);

                IPoint featureInfoPoint = new gView.Framework.Geometry.Point(parameters.FeatureInfoX, parameters.FeatureInfoY);
                if (parameters.Version == "1.3.0" && sRef != null)
                {
                    IPoint ll = parameters.BBOX.LowerLeft;
                    IPoint ur = parameters.BBOX.UpperRight;

                    ll = gView.Framework.OGC.GML.GeometryTranslator.Gml3Point(ll, sRef);
                    ur = gView.Framework.OGC.GML.GeometryTranslator.Gml3Point(ur, sRef);

                    parameters.BBOX = new Envelope(ll, ur);
                    //featureInfoPoint = gView.Framework.OGC.GML.GeometryTranslator.Gml3Point(featureInfoPoint, sRef);
                }

                map.Display.SpatialReference = sRef;
                map.Display.iWidth = parameters.Width;
                map.Display.iHeight = parameters.Height;
                map.Display.Limit = parameters.BBOX;
                map.Display.ZoomTo(parameters.BBOX);

                double x = featureInfoPoint.X;
                double y = featureInfoPoint.Y;
                map.Display.Image2World(ref x, ref y);

                double tol = 5.0 * map.Display.mapScale / (96 / 0.0254);  // [m]
                if (sRef != null &&
                    sRef.SpatialParameters.IsGeographic)
                {
                    tol = (180.0 * tol / Math.PI) / 6370000.0;
                }

                SpatialFilter filter = new SpatialFilter();
                filter.Geometry = new Envelope(x - tol, y - tol, x + tol, y + tol);
                filter.FilterSpatialReference = sRef;
                filter.SubFields = "*";

                // Get Layers
                List<ILayer> queryLayers = new List<ILayer>();
                foreach (string l in parameters.QueryLayers)
                {
                    if (l == String.Empty || l[0] != 'c') continue;

                    MapServerHelper.Layers layers = MapServerHelper.FindMapLayers(map, _useTOC, l.Substring(1, l.Length - 1));
                    if (layers == null) continue;

                    foreach (ILayer layer in layers)
                    {
                        queryLayers.Add(layer);
                    }
                }


                // Query Layers
                StringBuilder response = new StringBuilder();
                List<FeatureType> features = new List<FeatureType>();

                foreach (ILayer layer in queryLayers)
                {
                    if (layer is IFeatureLayer && ((IFeatureLayer)layer).FeatureClass != null)
                    {
                        using (IFeatureCursor cursor = ((IFeatureLayer)layer).FeatureClass.Search(filter) as IFeatureCursor)
                        {
                            IFeature feature = null;
                            while ((feature = cursor.NextFeature) != null)
                            {
                                features.Add(new FeatureType("c" + layer.SID, ((IFeatureLayer)layer).FeatureClass, feature));
                            }
                        }
                    }
                }

                GetFeatureResponse(features, response, parameters.InfoFormat, parameters.InfoFormatXsl);

                return response.ToString();
            }
        }

        public string WMSC_DescriptTiles(string service, WMSParameterDescriptor parameters, IServiceRequestContext context)
        {
            try
            {
                TileServiceMetadata metadata = context.ServiceMap.MetadataProvider(_tilemetaprovider) as TileServiceMetadata;
                if (metadata == null || metadata.Use == false)
                {
                    return "<Exception>Service is not used with Tile Service</Exception>";
                }

                WMS_DescribeTilesResponse wms_descripeTiles = new WMS_DescribeTilesResponse();
                wms_descripeTiles.version = "1.4.0";

                wms_descripeTiles.TiledLayer = new TiledLayer[] { new TiledLayer() };
                wms_descripeTiles.TiledLayer[0].name = "_alllayers";

                wms_descripeTiles.TiledLayer[0].TiledStyles = new string[] { "default" };
                wms_descripeTiles.TiledLayer[0].TiledFormats = new string[] { "image/png", "image/jpeg" };
                wms_descripeTiles.TiledLayer[0].TiledDimension = new TiledDimension[] { new TiledDimension() };
                wms_descripeTiles.TiledLayer[0].TiledDimension[0].name = "unkown";
                wms_descripeTiles.TiledLayer[0].TiledDimension[0].Value = new string[] { "unkown" };

                List<TiledCrs> crss = new List<TiledCrs>();
                foreach (int epsg in metadata.EPSGCodes)
                {
                    IEnvelope bounds = metadata.GetEPSGEnvelope(epsg);
                    if (bounds == null || bounds.Width == 0.0 || bounds.Height == 0.0)
                        continue;

                    TiledCrs crs = new TiledCrs();
                    crs.name = "EPSG:" + epsg;
                    TileMatrixSet matrixSet = crs.TileMatrixSet = new TileMatrixSet();
                    matrixSet.TileWidth = metadata.TileWidth.ToString();
                    matrixSet.TileHeight = metadata.TileHeight.ToString();

                    PointType point = new PointType();
                    point.Item = new DirectPositionType();
                    ((DirectPositionType)point.Item).Text = bounds.minx.ToString(_nhi) + " " + bounds.maxy.ToString(_nhi);
                    point.srsDimension = "2";

                    List<TileMatrix> matrixs = new List<TileMatrix>();
                    foreach (double scale in metadata.Scales)
                    {
                        TileMatrix matrix = new TileMatrix();
                        matrix.scale = scale;
                        matrix.Point = point;
                        double res = matrix.scale / (96.0 / 0.0254);
                        int mw = (int)(bounds.Width / (double.Parse(matrixSet.TileWidth) * res)) + 1;
                        int mh = (int)(bounds.Height / (double.Parse(matrixSet.TileHeight) * res)) + 1;
                        matrix.MatrixWidth = mw.ToString();
                        matrix.MatrixHeight = mh.ToString();
                        matrixs.Add(matrix);
                    }
                    matrixSet.TileMatrix = matrixs.ToArray();
                    crss.Add(crs);
                }
                wms_descripeTiles.TiledLayer[0].TiledCrs = crss.ToArray();

                XsdSchemaSerializer<WMS_DescribeTilesResponse> serializer = new XsdSchemaSerializer<WMS_DescribeTilesResponse>();
                string xml = serializer.Serialize(wms_descripeTiles);

                _mapServer.Log("DescipeTiles:" + context.ServiceMap.Name, loggingMethod.request_detail, xml);

                byte[] bytes = Encoding.UTF8.GetBytes(xml);
                return Convert.ToBase64String(bytes);
                //return xml;
            }
            catch (Exception ex)
            {
                return "<Exception>" + ex.Message + "</Exception>";
            }
        }

        public string WMSC_GetTile(string service, WMSParameterDescriptor parameters, IServiceRequestContext context)
        {
            try
            {
                _mapServer.Log("GetTile", loggingMethod.request_detail, String.Empty);

                TileServiceMetadata metadata = context.ServiceMap.MetadataProvider(_tilemetaprovider) as TileServiceMetadata;
                if (metadata == null || metadata.Use == false)
                    return "<Exception>Service is not used with Tile Service</Exception>";

                //if (!metadata.Scales.Contains(parameters.Scale))
                //    return "<Exception>Scale is not defined with Tile Service</Exception>";
                parameters.Scale = metadata.Scales.GetScale(parameters.Scale);
                if (parameters.Scale <= 0.0)
                    return "<Exception>Scale is not defined with Tile Service</Exception>";

                if (!metadata.EPSGCodes.Contains(parameters.SRS))
                    return "<Exception>SRS is not defined with Tile Service</Exception>";

                IEnvelope bounds = metadata.GetEPSGEnvelope(parameters.SRS);
                if (bounds == null || bounds.Width == 0.0 || bounds.Height == 0.0)
                    return "<Exception>Extent is not defined with Tile Service</Exception>";

                string path = _mapServer.TileCachePath + @"\" + service + @"\_alllayers\" +
                    TileServiceMetadata.TilePath(
                        gView.Framework.Geometry.Tiling.GridOrientation.UpperLeft,
                        parameters.SRS, parameters.Scale, parameters.TileRow, parameters.TileCol);

                string ext = String.Empty;
                switch (parameters.Format)
                {
                    case WMSImageFormat.png:
                        if (metadata.FormatPng == false)
                            return "<Exception>image/png not supported</Exception>";
                        ext = ".png";
                        break;
                    case WMSImageFormat.jpeg:
                        if (metadata.FormatJpg == false)
                            return "<Exception>image/jpeg not supported</Exception>";
                        ext = ".jpg";
                        break;
                    default:
                        return "<Exception>Unsupported image format</Exception>";
                }
                FileInfo fi = new FileInfo(path + ext);
                if (metadata.UpperLeftCacheTiles == true && fi.Exists)
                    return fi.FullName;

                if (!fi.Directory.Exists)
                    fi.Directory.Create();

                ISpatialReference sRef = SpatialReference.FromID("epsg:" + parameters.SRS);
                using (IServiceMap map = context.ServiceMap)
                {
                    map.Display.SpatialReference = sRef;

                    map.Display.iWidth = metadata.TileWidth;
                    map.Display.iHeight = metadata.TileHeight;

                    double res = (double)parameters.Scale / (96.0 / 0.0254);
                    if (map.Display.MapUnits != GeoUnits.Meters)
                    {
                        GeoUnitConverter converter = new GeoUnitConverter();
                        res = converter.Convert(res, GeoUnits.Meters, map.Display.MapUnits);
                    }
                    double H = metadata.TileHeight * res;
                    double y = bounds.maxy - H * (parameters.TileRow + 1);

                    double W = metadata.TileWidth * res;
                    double x = bounds.minx + W * parameters.TileCol;

                    map.Display.ZoomTo(new Envelope(x, y, x + W, y + H));
                    map.Render();

                    bool maketrans = map.Display.MakeTransparent;
                    map.Display.MakeTransparent = true;
                    switch (parameters.Format)
                    {
                        case WMSImageFormat.png:
                            map.SaveImage(fi.FullName, System.Drawing.Imaging.ImageFormat.Png);
                            break;
                        case WMSImageFormat.jpeg:
                            map.SaveImage(fi.FullName, System.Drawing.Imaging.ImageFormat.Jpeg);
                            break;
                    }
                    map.Display.MakeTransparent = maketrans;

                    _mapServer.Log("CreateTile:", loggingMethod.request_detail, fi.FullName);

                    return fi.FullName;
                }
            }
            catch (Exception ex)
            {
                _mapServer.Log("GetTile", loggingMethod.error, ex.Message);
                return "<Exception>" + ex.Message + "</Exception>";
            }
        }

        #region Helper
        private string getValue(string parameter, string[] parameters)
        {
            foreach (string p in parameters)
            {
                if (p.ToLower().IndexOf(parameter.ToLower() + "=") == 0)
                {
                    return p.Substring(parameter.Length + 1, p.Length - parameter.Length - 1);
                }
            }
            return null;
        }

        private string Concat(string[] parameters, string divider)
        {
            if (parameters == null) return "";

            StringBuilder sb = new StringBuilder();
            foreach (string str in parameters)
            {
                if (sb.Length > 0) sb.Append(divider);
                sb.Append(str);
            }
            return sb.ToString();
        }

        private string TransformXML(string xml, string xslPath)
        {
            try
            {
                XslCompiledTransform transformer = new XslCompiledTransform();
                transformer.Load(xslPath);

                StringReader xmlStream = new StringReader(xml);
                XPathDocument xpathdoc = new XPathDocument(xmlStream);

                MemoryStream ms = new MemoryStream();
                XmlTextWriter xWriter = new XmlTextWriter(ms, System.Text.Encoding.UTF8);
                transformer.Transform(xpathdoc, xWriter);

                ms.Position = 0;
                StreamReader sr = new StreamReader(ms);
                string ret = sr.ReadToEnd();
                sr.Close();
                ms.Close();
                xWriter.Close();

                return ret;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }

        private IEnvelope GetEPSG4326Envelope(IServiceMap map)
        {
            if (map == null) return null;

            ISpatialReference sRef_map = map.Display.SpatialReference;
            ISpatialReference sRef_4326 = SpatialReference.FromID("epsg:4326");

            IEnvelope envelope = null;
            foreach (IDatasetElement layer in map.MapElements)
            {
                if (layer == null) continue;

                IEnvelope env = null;
                ISpatialReference sRef = null;
                if (layer.Class is IFeatureClass)
                {
                    sRef = ((IFeatureClass)layer.Class).SpatialReference;
                    if (sRef == null) sRef = sRef_map;
                    env = ((IFeatureClass)layer.Class).Envelope;
                }
                else if (layer.Class is IRasterClass && ((IRasterClass)layer.Class).Polygon != null)
                {
                    sRef = ((IRasterClass)layer.Class).SpatialReference;
                    if (sRef == null) sRef = sRef_map;
                    env = ((IRasterClass)layer.Class).Polygon.Envelope;
                }
                else if (layer is IWebServiceLayer && ((IWebServiceLayer)layer).WebServiceClass != null && ((IWebServiceLayer)layer).WebServiceClass.Themes != null)
                {
                    IWebServiceLayer wLayer = (IWebServiceLayer)layer;
                    if (wLayer.WebServiceClass.Envelope != null)
                    {
                        sRef = wLayer.WebServiceClass.SpatialReference;
                        if (sRef == null) sRef = sRef_map;
                        env = wLayer.WebServiceClass.Envelope;
                    }
                    else
                    {
                        foreach (IWebServiceTheme theme in wLayer.WebServiceClass.Themes)
                        {
                            if (theme == null) continue;

                            if (theme.FeatureClass is IFeatureClass &&
                                theme.FeatureClass.Envelope != null)
                            {
                                sRef = theme.FeatureClass.SpatialReference;
                                if (sRef == null) sRef = sRef_map;
                                env = theme.FeatureClass.Envelope;
                            }
                        }
                    }
                }
                if (env == null || sRef == null) continue;

                env = GeometricTransformer.Transform2D(env, sRef, sRef_4326).Envelope;
                if (envelope == null)
                    envelope = env;
                else
                    envelope.Union(env);
            }

            return (envelope != null) ? envelope : new Envelope(-180, -90, 180, 90);
        }

        private IEnvelope TransFormEPSG4326Envelope(IEnvelope env4326, string projID)
        {
            if (projID.ToLower() == "epsg:4326") return env4326;
            if (env4326 == null) return null;

            ISpatialReference from = SpatialReference.FromID("epsg:4326");
            ISpatialReference to = SpatialReference.FromID(projID);
            if (from == null || to == null || from.Equals(to)) return env4326;

            return GeometricTransformer.Transform2D(env4326, from, to).Envelope;
        }

        private void GetFeatureResponse(List<FeatureType> features, StringBuilder sb, WMSInfoFormat format, string formatXsl)
        {

            switch (format)
            {
                case WMSInfoFormat.html:
                    GetFeatureResponseHTML(features, sb);
                    break;
                case WMSInfoFormat.xml:
                    GetFeatureResponseXML(features, sb);
                    break;
                case WMSInfoFormat.gml:
                    GetFeatureResponseGML(features, sb);
                    break;
                case WMSInfoFormat.text:
                    GetFeatureResponseText(features, sb);
                    break;
                case WMSInfoFormat.xsl:
                    StringBuilder xml = new StringBuilder();
                    GetFeatureResponseXML(features, xml);

                    string output = String.Empty;
                    try
                    {
                        output = this.TransformXML(xml.ToString(), System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\misc\wms\GetFeatureInfo\" + formatXsl.Replace("/", "\\"));
                    
                        /*
                        var xslTrans = new XslCompiledTransform();
                        xslTrans.Load(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\misc\wms\GetFeatureInfo\" + formatXsl.Replace("/", "\\"));
                        using (StringReader xmlStrReader = new StringReader(xml.ToString()))
                        using (XmlReader xmlReader = XmlReader.Create(xmlStrReader))
                        using (StringWriter strWriter = new StringWriter())
                        using (XmlWriter xmlWriter = XmlWriter.Create(strWriter, xslTrans.OutputSettings))
                        {
                            xslTrans.Transform(xmlReader, xmlWriter);
                            output = strWriter.ToString();
                        }
                         * */
                    }
                    catch (Exception ex)
                    {
                        output = "Exception: " + ex.Message;
                    }

                    sb.Append(output);

                    break;
            }
        }

        private void GetFeatureResponseHTML(List<FeatureType> features, StringBuilder sb)
        {
            // Table
            sb.Append("<html><body><table style=\"border: solid 1px black;\">");

            // Header
            List<string> Columns = new List<string>();
            foreach (FeatureType feature in features)
            {
                foreach (FieldValue fv in feature.Feature.Fields)
                {
                    if (!Columns.Contains(fv.Name)) Columns.Add(fv.Name);
                }
            }
            sb.Append("<tr>");
            foreach (string col in Columns)
            {
                sb.Append("<th>"); sb.Append(col); sb.Append("</th>");
            }
            sb.Append("</tr>");

            foreach (FeatureType feature in features)
            {
                sb.Append("<tr>");
                foreach (string col in Columns)
                {
                    sb.Append("<td>");

                    object obj = feature.Feature[col];
                    if (obj == null)
                        sb.Append("&nbsp;");
                    else
                        sb.Append(obj.ToString());

                    sb.Append("</td>");
                }
                sb.Append("</tr>");
            }

            sb.Append("</table></body></html>");
        }

        private void GetFeatureResponseXML(List<FeatureType> features, StringBuilder sb)
        {
            sb.Append(@"<?xml version=""1.0"" encoding=""UTF-8"" ?>");
            sb.Append(@"<gview_wms:FeatureInfoResponse version=""1.1.0"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:gview_wms=""http://www.gviewgis.com/wms"" xmlns=""http://www.gviewgis.com/wms"">");

            string layername = null;

            bool closeFeatureCollectionTag = false;

            foreach (FeatureType feature in features)
            {
                if (feature == null)
                    continue;

                if (layername != feature.FeatureClass.Name)
                {
                    layername = feature.FeatureClass.Name;
                    if (closeFeatureCollectionTag)
                        sb.Append("</gview_wms:FeatureInfoCollection>");
                    sb.Append("<gview_wms:FeatureInfoCollection layername=\"" + layername + "\">");
                    closeFeatureCollectionTag = true;
                }

                sb.Append("<gview_wms:FeatureInfo>");
                foreach (FieldValue fv in feature.Feature.Fields)
                {
                    sb.Append("<gview_wms:Field>");

                    string val = fv.Value!=null ? fv.Value.ToString() : String.Empty;
                    // Encode for XML
                    val = System.Security.SecurityElement.Escape(val);

                    sb.Append("<gview_wms:FieldName>" + fv.Name + "</gview_wms:FieldName>");
                    sb.Append("<gview_wms:FieldValue>" + val + "</gview_wms:FieldValue>");

                    sb.Append("</gview_wms:Field>");
                }
                sb.Append("</gview_wms:FeatureInfo>");
            }
            if (closeFeatureCollectionTag)
                sb.Append(@"</gview_wms:FeatureInfoCollection>");
            sb.Append(@"</gview_wms:FeatureInfoResponse>");
            /*
            sb.Append("<GETFEATURERESPONSE>");
            sb.Append("<FEATURES>");
            foreach (FeatureType feature in features)
            {
                if (feature == null) continue;

                sb.Append("<FEATURE>");
                sb.Append("<FIELDS>");
                foreach (FieldValue fv in feature.Feature.Fields)
                {
                    sb.Append("<FIELD name=\"" + fv.Name + "\" value=\"" + fv.Value + "\" />");
                }
                sb.Append("</FIELDS>");
                sb.Append("</FEATURE>");
            }
            sb.Append("</FEATURES>");
            sb.Append("</GETFEATURERESPONSE>");
             * */
        }

        private void GetFeatureResponseGML(List<FeatureType> features, StringBuilder sb)
        {
            sb.Append(@"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<wfs:FeatureCollection
   xmlns=""http://www.gViewGIS.com/server""
   xmlns:gv=""http://www.gViewGIS.com/server""
   xmlns:wfs=""http://www.opengis.net/wfs""
   xmlns:gml=""http://www.opengis.net/gml""
   xmlns:ogc=""http://www.opengis.net/ogc""
   xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">");

            // TODO: Bounding Box einfügen !!!

            foreach (FeatureType feature in features)
            {
                FeatureTranslator.Feature2GML(
                    feature.Feature,
                    feature.FeatureClass,
                    feature.Id,
                    sb,
                    (feature.FeatureClass.SpatialReference != null) ? feature.FeatureClass.SpatialReference.Name : String.Empty,
                    null, GmlVersion.v1);
            }

            sb.Append(@"</wfs:FeatureCollection>");
            string ret = sb.ToString().Trim();
            _mapServer.Log(String.Empty, loggingMethod.request_detail, ret);
        }

        private void GetFeatureResponseText(List<FeatureType> features, StringBuilder sb)
        {
            if (features == null || features.Count == 0) return;

            sb.Append("GetFeatureInfo results:\n\n");
            foreach (FeatureType feature in features)
            {
                if (feature == null ||
                    feature.FeatureClass == null ||
                    feature.Feature == null) continue;

                sb.Append("Layer '" + feature.FeatureClass.Name + "'\n");
                sb.Append("  Feature " + feature.Feature.OID + "\n");
                foreach (FieldValue fv in feature.Feature.Fields)
                {
                    if (fv.Value == null)
                    {
                        //sb.Append("    " + fv.Name + " = <null>\n");
                    }
                    else
                    {
                        sb.Append("    " + fv.Name + " = '" + fv.Value.ToString() + "'\n");
                    }
                }
            }
        }

        private class FeatureType
        {
            public string Id;
            public IFeatureClass FeatureClass;
            public IFeature Feature;

            public FeatureType(string id, IFeatureClass fc, IFeature feat)
            {
                Id = id;
                FeatureClass = fc;
                Feature = feat;
            }
        }
        #endregion
    }

    /*
    [gView.Framework.system.RegisterPlugIn("1242DC9C-EF07-48af-98F9-16D90082B888")]
    public class WMSRequest2 : WMSRequest
    {
        public WMSRequest2()
            : base()
        {
            _useTOC = true;
        }

        public override string IntentityName
        {
            get
            {
                return "wms2";
            }
        }
    }
    */

    public interface IEPSGMetadata
    {
        string[] EPSGCodes { get; set; }
    }

    [gView.Framework.system.RegisterPlugIn("0F6317BC-38FD-41d3-8E1A-82AB1873C526")]
    public class WMS_Export_Metadata : IMetadataProvider, IPropertyPage, IEPSGMetadata
    {
        private IServiceMap _map = null;
        private Metadata _metadata;

        #region IMetadataProvider Member

        public bool ApplyTo(object Object)
        {
            if (Object is IServiceMap)
            {
                _map = (IServiceMap)Object;
                return true;
            }
            return false;
        }

        public string Name
        {
            get { return "WMS Export"; }
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            //string epsg = String.Empty;
            //if (map.Display != null && map.Display.SpatialReference != null)
            //    epsg = map.Display.SpatialReference.Name;

            _metadata = new Metadata(String.Empty);
            stream.Load("WMS_Export", null, _metadata);
        }

        public void Save(IPersistStream stream)
        {
            if (_metadata == null && _map != null)
            {
                string epsg = String.Empty;
                if (_map.Display != null && _map.Display.SpatialReference != null)
                    epsg = _map.Display.SpatialReference.Name;

                _metadata = new Metadata(epsg);
            }
            stream.Save("WMS_Export", _metadata);
        }


        #endregion

        #region Classes
        internal class Metadata : IPersistable
        {
            private string _epsg;
            private IndexList<string> _epsgCodes = new IndexList<string>();

            public Metadata(string epsg)
            {
                _epsg = epsg;

                SetDefaultEPSGCodes();
            }

            public string[] EPSGCodes
            {
                get
                {
                    return _epsgCodes.ToArray();
                }
                internal set
                {
                    if (value == null) return;

                    _epsgCodes = new IndexList<string>();
                    foreach (string code in value)
                    {
                        _epsgCodes.Add(code);
                    }
                }
            }
            private void SetDefaultEPSGCodes()
            {
                _epsgCodes = new IndexList<string>();
                if (!String.IsNullOrEmpty(_epsg))
                    _epsgCodes.Add(_epsg.ToUpper());

                foreach (string srs in WMSConfig.SRS.Split(';'))
                    _epsgCodes.Add(srs.ToUpper());
            }
            #region IPersistable Member

            public void Load(IPersistStream stream)
            {
                _epsgCodes = new IndexList<string>();
                XmlStreamStringArray epsg = stream.Load("EPSG_Codes") as XmlStreamStringArray;
                if (epsg != null &&
                    epsg.Value != null &&
                    epsg.Value.GetType() == typeof(string[]) &&
                    ((string[])epsg.Value).Length > 0)
                {
                    foreach (string epsgCode in (string[])epsg.Value)
                    {
                        _epsgCodes.Add(epsgCode.ToUpper());
                    }
                }
                else
                {
                    SetDefaultEPSGCodes();
                }
            }

            public void Save(IPersistStream stream)
            {
                XmlStreamStringArray epsg = new XmlStreamStringArray(_epsgCodes.ToArray());
                stream.Save("EPSG_Codes", epsg);
            }

            #endregion
        }
        #endregion

        internal Metadata Data
        {
            get { return _metadata; }
        }

        #region IPropertyPage Member

        public object PropertyPage(object initObject)
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Interoperability.OGC.UI.dll");

            IPlugInParameter p = uiAssembly.CreateInstance("gView.Interoperability.OGC.UI.Dataset.WMS.WMSMetadata") as IPlugInParameter;
            if (p != null)
                p.Parameter = this;

            return p;
        }

        public object PropertyPageObject()
        {
            return this;
        }

        #endregion

        #region IEPSGMetadata
        public string[] EPSGCodes
        {
            get
            {
                if (_metadata != null)
                    return _metadata.EPSGCodes;

                return null;
            }
            set
            {
                if (_metadata != null)
                    _metadata.EPSGCodes = value;
            }
        }
        #endregion
    }
}
