using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.Web;
using gView.Framework.IO;
using gView.MapServer;
using gView.Framework.XML;
using gView.Framework.Carto;
using gView.Framework.system;


[assembly: InternalsVisibleTo("gView.Interoperability.ArcXML.UI, PublicKey=0024000004800000940000000602000000240000525341310004000001000100ad6b16ba6c0a7a8717a5945266036366beaed6fccb2f87802f0efe09e2a59e5554056ab439042410b35708e13362eadb16e89d183a7518a8c426a6f6cee0c64a93b3e39e2dd52948410c7bfc5b70a77d173105861e5875a75471b7fb2f833e0157cbd1b6a5d9b23bb1e093e4049083fbbd5b60fb41f84220ff3d0692bc2683bd")]

namespace gView.Interoperability.ArcXML.Dataset
{
    internal class ArcIMSClass : IWebServiceClass
    {
        internal event EventHandler ModifyResponseOuput = null;

        private string _name;
        private ArcIMSDataset _dataset;
        private System.Drawing.Bitmap _legend = null;
        private GeorefBitmap _image = null;
        private List<IWebServiceTheme> _clonedThemes = null;

        public ArcIMSClass(ArcIMSDataset dataset)
        {
            _dataset = dataset;
            if (_dataset != null) _name = _dataset._name;
        }

        #region IWebServiceClass Member

        public event BeforeMapRequestEventHandler BeforeMapRequest = null;
        public event AfterMapRequestEventHandler AfterMapRequest = null;

        public bool MapRequest(gView.Framework.Carto.IDisplay display)
        {
            if (_dataset == null || Themes == null) return false;

            List<IWebServiceTheme> themes = Themes;

            #region Check for visible Layers
            bool visFound = false;
            foreach (IWebServiceTheme theme in themes)
            {
                if (!theme.Visible) continue;
                if (theme.MinimumScale > 1 && theme.MinimumScale > display.mapScale) continue;
                if (theme.MaximumScale > 1 && theme.MaximumScale < display.mapScale) continue;

                visFound = true;
                break;
            }
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

            string server = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "server");
            string service = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "service");
            string user = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "user");
            string pwd = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "pwd");
            IServiceRequestContext context = display.Map as IServiceRequestContext;
            if ((user == "#" || user == "$") &&
                context != null && context.ServiceRequest != null && context.ServiceRequest.Identity != null)
            {
                string roles = String.Empty;
                if (user == "#" && context.ServiceRequest.Identity.UserRoles != null)
                {
                    foreach (string role in context.ServiceRequest.Identity.UserRoles)
                    {
                        if (String.IsNullOrEmpty(role)) continue;
                        roles += "|" + role;
                    }
                }
                user = context.ServiceRequest.Identity.UserName + roles;
                pwd = context.ServiceRequest.Identity.HashedPassword;
            }

            dotNETConnector connector = new dotNETConnector();
            if (!String.IsNullOrEmpty(user) || !String.IsNullOrEmpty(pwd))
                connector.setAuthentification(user, pwd);

            if (_dataset.State != DatasetState.opened)
            {
                if (!_dataset.Open(context)) return false;
            }

            ISpatialReference sRef = (display.SpatialReference != null) ?
                display.SpatialReference.Clone() as ISpatialReference :
                null;

            int iWidth = display.iWidth;
            int iHeight = display.iHeight;

            if (BeforeMapRequest != null)
                BeforeMapRequest(this, display, ref sRef, ref iWidth, ref iHeight);

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<?xml version='1.0' encoding='utf-8'?>");
                sb.Append("<ARCXML version='1.1'>");
                sb.Append("<REQUEST>");
                sb.Append("<GET_IMAGE>");
                sb.Append("<PROPERTIES>");
                IEnvelope bounds = display.DisplayTransformation.TransformedBounds(display);
                if (display.DisplayTransformation.UseTransformation == true)
                {
                    iWidth = (int)(bounds.Width * display.dpm / display.mapScale);
                    iHeight = (int)(bounds.Height * display.dpm / display.mapScale);
                }
                sb.Append("<ENVELOPE minx='" + bounds.minx.ToString() + "' miny='" + bounds.miny.ToString() + "' maxx='" + bounds.maxx.ToString() + "' maxy='" + bounds.maxy.ToString() + "' />");
                sb.Append("<IMAGESIZE width='" + iWidth + "' height='" + iHeight + "' />");
                sb.Append("<BACKGROUND color='" + Color2AXL(display.BackgroundColor) + "' transcolor='" + Color2AXL(display.TransparentColor) + "' />");

                string propertyString = _dataset._properties.PropertyString;
                if (propertyString != String.Empty)
                {
                    sb.Append(_dataset._properties.PropertyString);
                }
                else
                {
                    if (sRef != null)
                    {
                        //if (this.SpatialReference != null && !display.SpatialReference.Equals(this.SpatialReference))
                        {
                            string wkt = gView.Framework.Geometry.SpatialReference.ToESRIWKT(sRef);
                            string geotranwkt = gView.Framework.Geometry.SpatialReference.ToESRIGeotransWKT(sRef);

                            if (wkt != null)
                            {
                                //wkt = "PROJCS[\"MGI_M31\",GEOGCS[\"GCS_MGI\",DATUM[\"D_MGI\",SPHEROID[\"Bessel_1841\",6377397.155,0]],PRIMEM[\"Greenwich\",0.0],UNIT[\"degree\",0.0174532925199433]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"False_Easting\",450000],PARAMETER[\"False_Northing\",-5000000],PARAMETER[\"Central_Meridian\",13.3333333333333],PARAMETER[\"Scale_Factor\",1],PARAMETER[\"latitude_of_origin\",0],UNIT[\"Meter\",1]]";
                                //wkt = "PROJCS[\"MGI_M31\",GEOGCS[\"GCS_MGI\",DATUM[\"D_MGI\",SPHEROID[\"Bessel_1841\",6377397.155,299.1528128]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"False_Easting\",450000.0],PARAMETER[\"False_Northing\",-5000000.0],PARAMETER[\"Central_Meridian\",13.33333333333333],PARAMETER[\"Scale_Factor\",1.0],PARAMETER[\"Latitude_Of_Origin\",0.0],UNIT[\"Meter\",1.0]]";

                                //geotranwkt = "GEOGTRAN[\"MGISTMK_To_WGS_1984\",GEOGCS[\"MGISTMK\",DATUM[\"Militar_Geographische_Institute_STMK\",SPHEROID[\"Bessel_1841\",6377397.155,299.1528128]],PRIMEM[\"Greenwich\",0],UNIT[\"degree\",0.0174532925199433]],METHOD[\"Position_Vector\"],PARAMETER[\"X_Axis_Translation\",577.326],PARAMETER[\"Y_Axis_Translation\",90.129],PARAMETER[\"Z_Axis_Translation\",463.919],PARAMETER[\"X_Axis_Rotation\",5.1365988],PARAMETER[\"Y_Axis_Rotation\",1.4742],PARAMETER[\"Z_Axis_Rotation\",5.2970436],PARAMETER[\"Scale_Difference\",2.4232]]";
                                wkt = wkt.Replace("\"", "&quot;");
                                geotranwkt = geotranwkt.Replace("\"", "&quot;");
                                if (!String.IsNullOrEmpty(geotranwkt))
                                {
                                    sb.Append("<FEATURECOORDSYS string=\"" + wkt + "\" datumtransformstring=\"" + geotranwkt + "\" />");
                                    sb.Append("<FILTERCOORDSYS string=\"" + wkt + "\" datumtransformstring=\"" + geotranwkt + "\" />");
                                }
                                else
                                {
                                    sb.Append("<FEATURECOORDSYS string=\"" + wkt + "\" />");
                                    sb.Append("<FILTERCOORDSYS string=\"" + wkt + "\" />");
                                }
                                //sb.Append("<FEATURECOORDSYS string=\"" + wkt + "\" datumtransformid=\"8415\" />");
                                //sb.Append("<FILTERCOORDSYS string=\"" + wkt + "\" datumtransformid=\"8415\" />");
                            }
                        }
                    }
                }
                
                sb.Append("<LAYERLIST>");
                foreach (IWebServiceTheme theme in themes)
                {
                    sb.Append("<LAYERDEF id='" + theme.LayerID + "' visible='" + (theme.Visible && !theme.Locked).ToString() + "'");
                    XmlNode xmlnode;
                    if (LayerRenderer.TryGetValue(theme.LayerID, out xmlnode))
                    {
                        sb.Append(">\n" + xmlnode.OuterXml + "\n</LAYERDEF>");
                    }
                    else if (theme.FeatureRenderer != null)
                    {
                        string renderer = ObjectFromAXLFactory.ConvertToAXL(theme.FeatureRenderer);
                        sb.Append(">\n" + renderer + "\n</LAYERDEF>");
                    }
                    else
                    {
                        sb.Append("/>");
                    }
                }
                sb.Append("</LAYERLIST>");
                sb.Append("</PROPERTIES>");
                foreach (XmlNode additional in this.AppendedLayers)
                {
                    if (additional != null)
                        sb.Append(additional.OuterXml);
                }
                sb.Append("</GET_IMAGE>");
                sb.Append("</REQUEST>");
                sb.Append("</ARCXML>");

#if(DEBUG)
                gView.Framework.system.Logger.LogDebug("Start ArcXML SendRequest");
#endif
                ArcIMSClass.Log(display as IServiceRequestContext, "GetImage Request", server, service, sb);
                string resp = connector.SendRequest(sb, server, service);
                ArcIMSClass.Log(display as IServiceRequestContext, "GetImage Response", server, service, resp);
#if(DEBUG)
                gView.Framework.system.Logger.LogDebug("ArcXML SendRequest Finished");
#endif

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(resp);

                XmlNode outputNode = doc.SelectSingleNode("//IMAGE/OUTPUT");
                XmlNode envelopeNode = doc.SelectSingleNode("//IMAGE/ENVELOPE");

                if (ModifyResponseOuput != null)
                    ModifyResponseOuput(this, new ModifyOutputEventArgs(outputNode));

                if (_image != null)
                {
                    _image.Dispose();
                    _image = null;
                }

#if(DEBUG)
                //gView.Framework.system.Logger.LogDebug("Start ArcXML DownloadImage");
#endif
                System.Drawing.Bitmap bm=null;
                if (outputNode != null)
                {
#if(DEBUG)
                    gView.Framework.system.Logger.LogDebug("Start ArcXML DownloadImage");
#endif
                    bm = WebFunctions.DownloadImage(outputNode/*_dataset._connector.Proxy*/);
#if(DEBUG)
                    gView.Framework.system.Logger.LogDebug("ArcXML DownloadImage Finished");
#endif
                }
                else
                {
                    bm = null;
                }
#if(DEBUG)
                //gView.Framework.system.Logger.LogDebug("ArcXML DownloadImage Finished");
#endif

                if (bm != null)
                {
                    _image = new GeorefBitmap(bm);
                    //_image.MakeTransparent(display.TransparentColor);
                    if (envelopeNode != null &&
                        envelopeNode.Attributes["minx"] != null &&
                        envelopeNode.Attributes["miny"] != null &&
                        envelopeNode.Attributes["maxx"] != null &&
                        envelopeNode.Attributes["maxy"] != null)
                    {
                        _image.Envelope = new Envelope(
                            Convert.ToDouble(envelopeNode.Attributes["minx"].Value.Replace(".", ",")),
                            Convert.ToDouble(envelopeNode.Attributes["miny"].Value.Replace(".", ",")),
                            Convert.ToDouble(envelopeNode.Attributes["maxx"].Value.Replace(".", ",")),
                            Convert.ToDouble(envelopeNode.Attributes["maxy"].Value.Replace(".", ",")));
                    }
                    _image.SpatialReference = display.SpatialReference;

                    if (AfterMapRequest != null)
                        AfterMapRequest(this, display, _image);
                }
                return _image != null;
            }
            catch (Exception ex)
            {
                ArcIMSClass.ErrorLog(context, "MapRequest", server, service, ex);
                return false;
            }
        }

        public bool LegendRequest(gView.Framework.Carto.IDisplay display)
        {
            if (_dataset == null) return false;

            List<IWebServiceTheme> themes = Themes;
            if (themes == null) return false;

            #region Check for visible Layers
            bool visFound = false;
            foreach (IWebServiceTheme theme in themes)
            {
                if (!theme.Visible) continue;
                if (theme.MinimumScale > 1 && theme.MinimumScale > display.mapScale) continue;
                if (theme.MaximumScale > 1 && theme.MaximumScale < display.mapScale) continue;

                visFound = true;
                break;
            }
            if (!visFound)
            {
                if (_legend != null)
                {
                    _legend.Dispose();
                    _legend = null;
                }
                return true;
            }
            #endregion

            string server = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "server");
            string service = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "service");
            string user = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "user");
            string pwd = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "pwd");
            IServiceRequestContext context = display.Map as IServiceRequestContext;
            if ((user == "#" || user == "$") &&
                context != null && context.ServiceRequest != null && context.ServiceRequest.Identity != null)
            {
                string roles = String.Empty;
                if (user == "#" && context.ServiceRequest.Identity.UserRoles != null)
                {
                    foreach (string role in context.ServiceRequest.Identity.UserRoles)
                    {
                        if (String.IsNullOrEmpty(role)) continue;
                        roles += "|" + role;
                    }
                }
                user = context.ServiceRequest.Identity.UserName + roles;
                pwd = context.ServiceRequest.Identity.HashedPassword;
            }

            dotNETConnector connector = new dotNETConnector();
            if (!String.IsNullOrEmpty(user) || !String.IsNullOrEmpty(pwd))
                connector.setAuthentification(user, pwd);

            if (_dataset.State != DatasetState.opened)
            {
                if (!_dataset.Open(context)) return false;
            }

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<?xml version='1.0' encoding='utf-8'?>");
                sb.Append("<ARCXML version='1.1'>");
                sb.Append("<REQUEST>");
                sb.Append("<GET_IMAGE>");
                sb.Append("<PROPERTIES>");
                sb.Append("<ENVELOPE minx='" + display.Envelope.minx.ToString() + "' miny='" + display.Envelope.miny.ToString() + "' maxx='" + display.Envelope.maxx.ToString() + "' maxy='" + display.Envelope.maxy.ToString() + "' />");
                sb.Append("<IMAGESIZE width='" + display.iWidth + "' height='" + display.iHeight + "' />");
                sb.Append("<BACKGROUND color='255,255,255' transcolor='255,255,255' />");

                sb.Append(_dataset._properties.PropertyString);

                sb.Append("<LAYERLIST>");
                foreach (IWebServiceTheme theme in themes)
                {
                    sb.Append("<LAYERDEF id='" + theme.LayerID + "' visible='" + (theme.Visible && !theme.Locked).ToString() + "'");
                    XmlNode xmlnode;
                    if (LayerRenderer.TryGetValue(theme.LayerID, out xmlnode))
                    {
                        sb.Append(">\n" + xmlnode.OuterXml + "\n</LAYERDEF>");
                    }
                    else if (theme.FeatureRenderer != null)
                    {
                        string renderer = ObjectFromAXLFactory.ConvertToAXL(theme.FeatureRenderer);
                        sb.Append(">\n" + renderer + "\n</LAYERDEF>");
                    }
                    else
                    {
                        sb.Append("/>");
                    }
                }
                sb.Append("</LAYERLIST>");
                sb.Append("<DRAW map=\"false\" />");
                sb.Append("<LEGEND font=\"Arial\" autoextend=\"true\" columns=\"1\" width=\"165\" height=\"170\" backgroundcolor=\"255,255,255\" layerfontsize=\"11\" valuefontsize=\"10\">");
                sb.Append("<LAYERS />");
                sb.Append("</LEGEND>");
                sb.Append("</PROPERTIES>");
                foreach (XmlNode additional in this.AppendedLayers)
                {
                    sb.Append(additional.OuterXml);
                }
                sb.Append("</GET_IMAGE>");
                sb.Append("</REQUEST>");
                sb.Append("</ARCXML>");

                ArcIMSClass.Log(display as IServiceRequestContext, "GetLegend Request", server, service, sb);
                string resp = connector.SendRequest(sb, server, service);
                ArcIMSClass.Log(display as IServiceRequestContext, "GetLegend Response", server, service, resp);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(resp);

                XmlNode output = doc.SelectSingleNode("//LEGEND");

                if (ModifyResponseOuput != null)
                    ModifyResponseOuput(this, new ModifyOutputEventArgs(output));

                if (_legend != null)
                {
                    _legend.Dispose();
                }

                _legend = WebFunctions.DownloadImage(output/*_dataset._connector.Proxy*/);
                return true;
            }
            catch (Exception ex)
            {
                ArcIMSClass.ErrorLog(context, "LegendRequest", server, service, ex);
                return false;
            }
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
            get
            {
                if (_dataset == null) return null;
                if (_dataset.State != DatasetState.opened) _dataset.Open();
                return _dataset.Envelope;
            }
        }

        public List<IWebServiceTheme> Themes
        {
            get
            {
                if (_clonedThemes != null) return _clonedThemes;
                if (_dataset != null)
                {
                    if (_dataset.State != DatasetState.opened) _dataset.Open();
                    return _dataset._themes;
                }
                return new List<IWebServiceTheme>();
            }
        }

        public ISpatialReference SpatialReference
        {
            get
            {
                if (_dataset != null) 
                    return _dataset.SpatialReference;

                return null;
            }
            set
            {
                if (_dataset != null)
                    _dataset.SpatialReference = value;
            }
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

        #region IClone Member

        public object Clone()
        {
            ArcIMSClass clone = new ArcIMSClass(_dataset);
            clone._clonedThemes = new List<IWebServiceTheme>();

            foreach (IWebServiceTheme theme in Themes)
            {
                if (theme == null || theme.Class == null) continue;
                clone._clonedThemes.Add(LayerFactory.Create(theme.Class, theme as ILayer, clone) as IWebServiceTheme);
            }
            clone.BeforeMapRequest = BeforeMapRequest;
            clone.AfterMapRequest = AfterMapRequest;
            clone.ModifyResponseOuput = ModifyResponseOuput;
            return clone;
        }

        #endregion

        private string Color2AXL(System.Drawing.Color col)
        {
            return col.R + "," + col.G + "," + col.B;
        }

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

        public static void Log(IServiceRequestContext context, string header, string server, string service, string axl)
        {
            if (context == null ||
                context.MapServer == null ||
                context.MapServer.LoggingEnabled(loggingMethod.request_detail_pro) == false) return;

            StringBuilder sb = new StringBuilder();
            sb.Append("\n");
            sb.Append(header);
            sb.Append("\n");
            sb.Append("Server: " + server + " Service: " + service);
            sb.Append("\n");
            sb.Append(axl);

            context.MapServer.Log("gView.Interoperability.ArcXML", loggingMethod.request_detail_pro, sb.ToString());
        }
        public static void Log(IServiceRequestContext context, string header, string server, string service, StringBuilder axl)
        {
            if (context == null ||
                context.MapServer == null ||
                context.MapServer.LoggingEnabled(loggingMethod.request_detail_pro) == false) return;

            Log(context, header, server, service, axl.ToString());
        }
        public static void ErrorLog(IServiceRequestContext context, string header, string server, string service, Exception ex)
        {
            if (context == null ||
                context.MapServer == null ||
                context.MapServer.LoggingEnabled(loggingMethod.error) == false) return;

            StringBuilder msg = new StringBuilder();
            if (ex != null)
            {
                msg.Append(ex.Message + "\n");
                Exception inner = ex;
                while ((inner = inner.InnerException) != null)
                {
                    msg.Append(inner.Message + "\n");
                }
            }

            context.MapServer.Log(server + "-" + service + ": " + header, loggingMethod.error,
                msg.ToString() +
                ex.Source + "\n" +
                ex.StackTrace + "\n");
        }
    }

    internal class ModifyOutputEventArgs : EventArgs
    {
        public XmlNode OutputNode = null;

        public ModifyOutputEventArgs(XmlNode outputNode)
        {
            OutputNode = outputNode;
        }
    }
}
