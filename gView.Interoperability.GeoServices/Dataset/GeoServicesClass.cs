using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system;
using gView.Framework.Web;
using gView.Interoperability.GeoServices.Rest.Json.Request;
using gView.Interoperability.GeoServices.Rest.Json.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace gView.Interoperability.GeoServices.Dataset
{
    public class GeoServicesClass : IWebServiceClass
    {
        internal event EventHandler ModifyResponseOuput = null;

        private GeoServicesDataset _dataset;
        private System.Drawing.Bitmap _legend = null;
        private GeorefBitmap _image = null;
        private List<IWebServiceTheme> _clonedThemes = null;

        public GeoServicesClass(GeoServicesDataset dataset)
        {
            _dataset = dataset;
            if (_dataset != null)
            {
                this.Name = _dataset._name;
            }
        }

        #region IWebServiceClass Member

        public event BeforeMapRequestEventHandler BeforeMapRequest = null;
        public event AfterMapRequestEventHandler AfterMapRequest = null;

        async public Task<bool> MapRequest(gView.Framework.Carto.IDisplay display)
        {

            if (_dataset == null)
            {
                return false;
            }

            List<IWebServiceTheme> themes = Themes;
            if (themes == null)
            {
                return false;
            }

            #region Check for visible Layers

            bool visFound = this.Themes.Where(l => l.Visible).Count() > 0;
            if (!visFound)
            {
                if (_image != null)
                {
                    _image.Dispose();
                    _image = null;
                }
                return true;
            }

            #endregion

            var serviceUrl = _dataset.ServiceUrl();

            //IServiceRequestContext context = display.Map as IServiceRequestContext;

            var jsonExportMap = new JsonExportMap();
            if (display?.Envelope != null)
            {
                var env = display.Envelope;
                jsonExportMap.BBox = $"{env.minx.ToDoubleString()},{env.miny.ToDoubleString()},{env.maxx.ToDoubleString()},{env.maxy.ToDoubleString()}";
            }

            var sRef = display.SpatialReference ?? this.SpatialReference;
            if (sRef != null)
            {
                jsonExportMap.BBoxSRef = sRef.Name.ToLower().Replace("epsg:", "");
            }
            jsonExportMap.Size = $"{display.iWidth},{display.iHeight}";

            var layerIds = this.Themes
                .Where(l => l.Visible && (l.Class is IWebFeatureClass || l.Class is IWebRasterClass))
                .Select(l =>
                {
                    if (l.Class is IWebFeatureClass)
                    {
                        return ((IWebFeatureClass)l.Class).ID;
                    }

                    if (l.Class is IWebRasterClass)
                    {
                        return ((IWebRasterClass)l.Class).ID;
                    }

                    return String.Empty;
                });

            jsonExportMap.Layers = $"show:{String.Join(",", layerIds)}";

            var urlParameters = SerializeToUrlParameters(jsonExportMap);

            var response = await _dataset.TryPostAsync<JsonExportResponse>(
                serviceUrl
                    .UrlAppendPath("export")
                    .UrlAppendParameters("f=json")
                    .UrlAppendParameters(urlParameters));

            bool hasImage = false;
            if(!String.IsNullOrWhiteSpace(response.Href))
            {
                var bm = WebFunctions.DownloadImage(response.Href);
                if (bm != null)
                {
                    hasImage = true;
                    _image = new GeorefBitmap(bm);
                    if (response.Extent != null)
                    {
                        _image.Envelope = new Envelope(response.Extent.Xmin, response.Extent.Ymin, response.Extent.Xmax, response.Extent.Ymax);
                    }
                    _image.SpatialReference = sRef;
                }
            }

            if(!hasImage)
            {
                if (_image != null)
                {
                    _image.Dispose();
                    _image = null;
                }
                return false;
            }

            return true;


            //            dotNETConnector connector = new dotNETConnector();
            //            if (!String.IsNullOrEmpty(user) || !String.IsNullOrEmpty(pwd))
            //            {
            //                connector.setAuthentification(user, pwd);
            //            }

            //            if (_dataset.State != DatasetState.opened)
            //            {
            //                if (!await _dataset.Open(context))
            //                {
            //                    return false;
            //                }
            //            }

            //            ISpatialReference sRef = (display.SpatialReference != null) ?
            //                display.SpatialReference.Clone() as ISpatialReference :
            //                null;

            //            int iWidth = display.iWidth;
            //            int iHeight = display.iHeight;

            //            if (BeforeMapRequest != null)
            //            {
            //                BeforeMapRequest(this, display, ref sRef, ref iWidth, ref iHeight);
            //            }

            //            try
            //            {
            //                StringBuilder sb = new StringBuilder();
            //                sb.Append("<?xml version='1.0' encoding='utf-8'?>");
            //                sb.Append("<ARCXML version='1.1'>");
            //                sb.Append("<REQUEST>");
            //                sb.Append("<GET_IMAGE>");
            //                sb.Append("<PROPERTIES>");
            //                IEnvelope bounds = display.DisplayTransformation.TransformedBounds(display);
            //                if (display.DisplayTransformation.UseTransformation == true)
            //                {
            //                    iWidth = (int)(bounds.Width * display.dpm / display.mapScale);
            //                    iHeight = (int)(bounds.Height * display.dpm / display.mapScale);
            //                }
            //                sb.Append("<ENVELOPE minx='" + bounds.minx.ToString() + "' miny='" + bounds.miny.ToString() + "' maxx='" + bounds.maxx.ToString() + "' maxy='" + bounds.maxy.ToString() + "' />");
            //                sb.Append("<IMAGESIZE width='" + iWidth + "' height='" + iHeight + "' />");
            //                sb.Append("<BACKGROUND color='" + Color2AXL(display.BackgroundColor) + "' transcolor='" + Color2AXL(display.TransparentColor) + "' />");

            //                if (sRef != null)
            //                {
            //                    //if (this.SpatialReference != null && !display.SpatialReference.Equals(this.SpatialReference))
            //                    {
            //                        string wkt = gView.Framework.Geometry.SpatialReference.ToESRIWKT(sRef);
            //                        string geotranwkt = gView.Framework.Geometry.SpatialReference.ToESRIGeotransWKT(sRef);

            //                        if (wkt != null)
            //                        {
            //                            //wkt = "PROJCS[\"MGI_M31\",GEOGCS[\"GCS_MGI\",DATUM[\"D_MGI\",SPHEROID[\"Bessel_1841\",6377397.155,0]],PRIMEM[\"Greenwich\",0.0],UNIT[\"degree\",0.0174532925199433]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"False_Easting\",450000],PARAMETER[\"False_Northing\",-5000000],PARAMETER[\"Central_Meridian\",13.3333333333333],PARAMETER[\"Scale_Factor\",1],PARAMETER[\"latitude_of_origin\",0],UNIT[\"Meter\",1]]";
            //                            //wkt = "PROJCS[\"MGI_M31\",GEOGCS[\"GCS_MGI\",DATUM[\"D_MGI\",SPHEROID[\"Bessel_1841\",6377397.155,299.1528128]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"False_Easting\",450000.0],PARAMETER[\"False_Northing\",-5000000.0],PARAMETER[\"Central_Meridian\",13.33333333333333],PARAMETER[\"Scale_Factor\",1.0],PARAMETER[\"Latitude_Of_Origin\",0.0],UNIT[\"Meter\",1.0]]";

            //                            //geotranwkt = "GEOGTRAN[\"MGISTMK_To_WGS_1984\",GEOGCS[\"MGISTMK\",DATUM[\"Militar_Geographische_Institute_STMK\",SPHEROID[\"Bessel_1841\",6377397.155,299.1528128]],PRIMEM[\"Greenwich\",0],UNIT[\"degree\",0.0174532925199433]],METHOD[\"Position_Vector\"],PARAMETER[\"X_Axis_Translation\",577.326],PARAMETER[\"Y_Axis_Translation\",90.129],PARAMETER[\"Z_Axis_Translation\",463.919],PARAMETER[\"X_Axis_Rotation\",5.1365988],PARAMETER[\"Y_Axis_Rotation\",1.4742],PARAMETER[\"Z_Axis_Rotation\",5.2970436],PARAMETER[\"Scale_Difference\",2.4232]]";
            //                            wkt = wkt.Replace("\"", "&quot;");
            //                            geotranwkt = geotranwkt.Replace("\"", "&quot;");
            //                            if (!String.IsNullOrEmpty(geotranwkt))
            //                            {
            //                                sb.Append("<FEATURECOORDSYS string=\"" + wkt + "\" datumtransformstring=\"" + geotranwkt + "\" />");
            //                                sb.Append("<FILTERCOORDSYS string=\"" + wkt + "\" datumtransformstring=\"" + geotranwkt + "\" />");
            //                            }
            //                            else
            //                            {
            //                                sb.Append("<FEATURECOORDSYS string=\"" + wkt + "\" />");
            //                                sb.Append("<FILTERCOORDSYS string=\"" + wkt + "\" />");
            //                            }
            //                            //sb.Append("<FEATURECOORDSYS string=\"" + wkt + "\" datumtransformid=\"8415\" />");
            //                            //sb.Append("<FILTERCOORDSYS string=\"" + wkt + "\" datumtransformid=\"8415\" />");
            //                        }
            //                    }
            //                }

            //                sb.Append("<LAYERLIST>");
            //                foreach (IWebServiceTheme theme in themes)
            //                {
            //                    sb.Append("<LAYERDEF id='" + theme.LayerID + "' visible='" + (theme.Visible && !theme.Locked).ToString() + "'");
            //                    XmlNode xmlnode;
            //                    if (LayerRenderer.TryGetValue(theme.LayerID, out xmlnode))
            //                    {
            //                        sb.Append(">\n" + xmlnode.OuterXml + "\n</LAYERDEF>");
            //                    }
            //                    else if (theme.FeatureRenderer != null)
            //                    {
            //                        string renderer = ObjectFromAXLFactory.ConvertToAXL(theme.FeatureRenderer);
            //                        sb.Append(">\n" + renderer + "\n</LAYERDEF>");
            //                    }
            //                    else
            //                    {
            //                        sb.Append("/>");
            //                    }
            //                }
            //                sb.Append("</LAYERLIST>");
            //                sb.Append("</PROPERTIES>");
            //                foreach (XmlNode additional in this.AppendedLayers)
            //                {
            //                    if (additional != null)
            //                    {
            //                        sb.Append(additional.OuterXml);
            //                    }
            //                }
            //                sb.Append("</GET_IMAGE>");
            //                sb.Append("</REQUEST>");
            //                sb.Append("</ARCXML>");

            //#if(DEBUG)
            //                gView.Framework.system.Logger.LogDebug("Start ArcXML SendRequest");
            //#endif
            //                await ArcIMSClass.LogAsync(display as IServiceRequestContext, "GetImage Request", server, service, sb);
            //                string resp = connector.SendRequest(sb, server, service);
            //                await ArcIMSClass.LogAsync(display as IServiceRequestContext, "GetImage Response", server, service, resp);
            //#if(DEBUG)
            //                gView.Framework.system.Logger.LogDebug("ArcXML SendRequest Finished");
            //#endif

            //                XmlDocument doc = new XmlDocument();
            //                doc.LoadXml(resp);

            //                XmlNode outputNode = doc.SelectSingleNode("//IMAGE/OUTPUT");
            //                XmlNode envelopeNode = doc.SelectSingleNode("//IMAGE/ENVELOPE");

            //                if (ModifyResponseOuput != null)
            //                {
            //                    ModifyResponseOuput(this, new ModifyOutputEventArgs(outputNode));
            //                }

            //                if (_image != null)
            //                {
            //                    _image.Dispose();
            //                    _image = null;
            //                }

            //#if(DEBUG)
            //                //gView.Framework.system.Logger.LogDebug("Start ArcXML DownloadImage");
            //#endif
            //                System.Drawing.Bitmap bm = null;
            //                if (outputNode != null)
            //                {
            //#if(DEBUG)
            //                    gView.Framework.system.Logger.LogDebug("Start ArcXML DownloadImage");
            //#endif
            //                    bm = WebFunctions.DownloadImage(outputNode/*_dataset._connector.Proxy*/);
            //#if(DEBUG)
            //                    gView.Framework.system.Logger.LogDebug("ArcXML DownloadImage Finished");
            //#endif
            //                }
            //                else
            //                {
            //                    bm = null;
            //                }
            //#if(DEBUG)
            //                //gView.Framework.system.Logger.LogDebug("ArcXML DownloadImage Finished");
            //#endif

            //                if (bm != null)
            //                {
            //                    _image = new GeorefBitmap(bm);
            //                    //_image.MakeTransparent(display.TransparentColor);
            //                    if (envelopeNode != null &&
            //                        envelopeNode.Attributes["minx"] != null &&
            //                        envelopeNode.Attributes["miny"] != null &&
            //                        envelopeNode.Attributes["maxx"] != null &&
            //                        envelopeNode.Attributes["maxy"] != null)
            //                    {
            //                        _image.Envelope = new Envelope(
            //                            Convert.ToDouble(envelopeNode.Attributes["minx"].Value.Replace(".", ",")),
            //                            Convert.ToDouble(envelopeNode.Attributes["miny"].Value.Replace(".", ",")),
            //                            Convert.ToDouble(envelopeNode.Attributes["maxx"].Value.Replace(".", ",")),
            //                            Convert.ToDouble(envelopeNode.Attributes["maxy"].Value.Replace(".", ",")));
            //                    }
            //                    _image.SpatialReference = display.SpatialReference;

            //                    if (AfterMapRequest != null)
            //                    {
            //                        AfterMapRequest(this, display, _image);
            //                    }
            //                }
            //                return _image != null;
            //            }
            //            catch (Exception ex)
            //            {
            //                await ArcIMSClass.ErrorLog(context, "MapRequest", server, service, ex);
            //                return false;
            //            }

            return true;
        }

        async public Task<bool> LegendRequest(gView.Framework.Carto.IDisplay display)
        {
            //if (_dataset == null)
            //{
            //    return false;
            //}

            //List<IWebServiceTheme> themes = Themes;
            //if (themes == null)
            //{
            //    return false;
            //}

            //#region Check for visible Layers
            //bool visFound = false;
            //foreach (IWebServiceTheme theme in themes)
            //{
            //    if (!theme.Visible)
            //    {
            //        continue;
            //    }

            //    if (theme.MinimumScale > 1 && theme.MinimumScale > display.mapScale)
            //    {
            //        continue;
            //    }

            //    if (theme.MaximumScale > 1 && theme.MaximumScale < display.mapScale)
            //    {
            //        continue;
            //    }

            //    visFound = true;
            //    break;
            //}
            //if (!visFound)
            //{
            //    if (_legend != null)
            //    {
            //        _legend.Dispose();
            //        _legend = null;
            //    }
            //    return true;
            //}
            //#endregion

            //string server = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "server");
            //string service = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "service");
            //string user = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "user");
            //string pwd = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "pwd");
            //IServiceRequestContext context = display.Map as IServiceRequestContext;

            //dotNETConnector connector = new dotNETConnector();
            //if (!String.IsNullOrEmpty(user) || !String.IsNullOrEmpty(pwd))
            //{
            //    connector.setAuthentification(user, pwd);
            //}

            //if (_dataset.State != DatasetState.opened)
            //{
            //    if (!await _dataset.Open(context))
            //    {
            //        return false;
            //    }
            //}

            //try
            //{
            //    StringBuilder sb = new StringBuilder();
            //    sb.Append("<?xml version='1.0' encoding='utf-8'?>");
            //    sb.Append("<ARCXML version='1.1'>");
            //    sb.Append("<REQUEST>");
            //    sb.Append("<GET_IMAGE>");
            //    sb.Append("<PROPERTIES>");
            //    sb.Append("<ENVELOPE minx='" + display.Envelope.minx.ToString() + "' miny='" + display.Envelope.miny.ToString() + "' maxx='" + display.Envelope.maxx.ToString() + "' maxy='" + display.Envelope.maxy.ToString() + "' />");
            //    sb.Append("<IMAGESIZE width='" + display.iWidth + "' height='" + display.iHeight + "' />");
            //    sb.Append("<BACKGROUND color='255,255,255' transcolor='255,255,255' />");

            //    sb.Append(_dataset._properties.PropertyString);

            //    sb.Append("<LAYERLIST>");
            //    foreach (IWebServiceTheme theme in themes)
            //    {
            //        sb.Append("<LAYERDEF id='" + theme.LayerID + "' visible='" + (theme.Visible && !theme.Locked).ToString() + "'");
            //        XmlNode xmlnode;
            //        if (LayerRenderer.TryGetValue(theme.LayerID, out xmlnode))
            //        {
            //            sb.Append(">\n" + xmlnode.OuterXml + "\n</LAYERDEF>");
            //        }
            //        else if (theme.FeatureRenderer != null)
            //        {
            //            string renderer = ObjectFromAXLFactory.ConvertToAXL(theme.FeatureRenderer);
            //            sb.Append(">\n" + renderer + "\n</LAYERDEF>");
            //        }
            //        else
            //        {
            //            sb.Append("/>");
            //        }
            //    }
            //    sb.Append("</LAYERLIST>");
            //    sb.Append("<DRAW map=\"false\" />");
            //    sb.Append("<LEGEND font=\"Arial\" autoextend=\"true\" columns=\"1\" width=\"165\" height=\"170\" backgroundcolor=\"255,255,255\" layerfontsize=\"11\" valuefontsize=\"10\">");
            //    sb.Append("<LAYERS />");
            //    sb.Append("</LEGEND>");
            //    sb.Append("</PROPERTIES>");
            //    foreach (XmlNode additional in this.AppendedLayers)
            //    {
            //        sb.Append(additional.OuterXml);
            //    }
            //    sb.Append("</GET_IMAGE>");
            //    sb.Append("</REQUEST>");
            //    sb.Append("</ARCXML>");

            //    string resp = connector.SendRequest(sb, server, service);

            //    XmlDocument doc = new XmlDocument();
            //    doc.LoadXml(resp);

            //    XmlNode output = doc.SelectSingleNode("//LEGEND");

            //    if (ModifyResponseOuput != null)
            //    {
            //        ModifyResponseOuput(this, new ModifyOutputEventArgs(output));
            //    }

            //    if (_legend != null)
            //    {
            //        _legend.Dispose();
            //    }

            //    _legend = WebFunctions.DownloadImage(output);
            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    await GeoServicesClass.ErrorLog(context, "LegendRequest", server, service, ex);
            //    return false;
            //}

            return true;
        }

        GeorefBitmap IWebServiceClass.Image
        {
            get { return _image; }
        }

        System.Drawing.Bitmap IWebServiceClass.Legend
        {
            get { return _legend; }
        }

        public IEnvelope Envelope
        {
            get;
            internal set;
        }

        public List<IWebServiceTheme> Themes
        {
            get
            {
                if (_clonedThemes != null)
                {
                    return _clonedThemes;
                }

                if (_dataset != null)
                {
                    return _dataset._themes;
                }
                return new List<IWebServiceTheme>();
            }
        }

        internal ISpatialReference _sptatialReference;
        public ISpatialReference SpatialReference
        {
            get { return _sptatialReference; }
            set
            {
                _sptatialReference = value;
                if (_dataset != null)
                {
                    _dataset.SetSpatialReference(value);
                }
            }
        }

        #endregion

        #region IClass Member

        public string Name
        {
            get; set;
        }

        public string Aliasname
        {
            get { return this.Name; }
        }

        public IDataset Dataset
        {
            get { return _dataset; }
        }

        #endregion

        #region IClone Member

        public object Clone()
        {
            GeoServicesClass clone = new GeoServicesClass(_dataset);
            clone._clonedThemes = new List<IWebServiceTheme>();

            var themes = (_clonedThemes != null) ?
                _clonedThemes :
                (_dataset?._themes ?? new List<IWebServiceTheme>());

            foreach (IWebServiceTheme theme in themes)
            {
                if (theme == null || theme.Class == null)
                {
                    continue;
                }

                clone._clonedThemes.Add(LayerFactory.Create(
                    theme.Class,
                    theme as ILayer, clone) as IWebServiceTheme);
            }
            clone.BeforeMapRequest = BeforeMapRequest;
            clone.AfterMapRequest = AfterMapRequest;
            clone.ModifyResponseOuput = ModifyResponseOuput;
            return clone;
        }

        #endregion

        private List<XmlNode> _appendedLayers = new List<XmlNode>();
        internal List<XmlNode> AppendedLayers
        {
            get { return _appendedLayers; }
        }

        private Dictionary<string, XmlNode> _layerRenderer = new Dictionary<string, XmlNode>();
        internal Dictionary<string, XmlNode> LayerRenderer
        {
            get { return _layerRenderer; }
        }


        #region Helper

        public string SerializeToUrlParameters(object obj)
        {
            StringBuilder sb = new StringBuilder();

            if (obj != null)
            {
                foreach (var propertyInfo in obj.GetType().GetProperties())
                {
                    var jsonPropertyAttribute = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
                    if (jsonPropertyAttribute == null)
                    {
                        continue;
                    }

                    string val = propertyInfo.GetValue(obj)?.ToString();
                    if (!String.IsNullOrEmpty(val))
                    {
                        if (propertyInfo.PropertyType == typeof(int) && val == "0")
                        {
                            continue;
                        }

                        if (sb.Length > 0)
                        {
                            sb.Append("&");
                        }

                        sb.Append(jsonPropertyAttribute.PropertyName + "=" + HttpUtility.UrlEncode(val));
                    }
                }
            }

            return sb.ToString();
        }

        #endregion
    }
}
