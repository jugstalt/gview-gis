using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.MapServer;
using gView.Framework.Core.Common;
using gView.Framework.Data;
using gView.Framework.IO;
using gView.Framework.Common;
using gView.Framework.Web;
using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;
using gView.Server.Connector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace gView.Interoperability.Server
{
    public class MapServerClass : IWebServiceClass
    {
        private string _name;
        private MapServerDataset _dataset;
        private IBitmap _legend = null;
        private GeorefBitmap _image = null;
        private ISpatialReference _sRef = null;
        private List<IWebServiceTheme> _clonedThemes = null;

        public MapServerClass(MapServerDataset dataset)
        {
            _dataset = dataset;
            if (_dataset != null)
            {
                _name = _dataset._name;
            }
        }

        #region IClass Member

        public string Name
        {
            get { return _name; }
        }

        public string Aliasname
        {
            get { return (_dataset != null) ? _dataset.DatasetName : ""; }
        }

        public IDataset Dataset
        {
            get { return _dataset; }
        }

        #endregion IClass Member

        #region IWebServiceClass Member

        public event AfterMapRequestEventHandler AfterMapRequest;

        async public Task<bool> MapRequest(IDisplay display)
        {
            if (_dataset == null)
            {
                return false;
            }

            if (!_dataset._opened)
            {
                await _dataset.Open();
            }

            IServiceRequestContext context = display.Map as IServiceRequestContext;
            try
            {
                ISpatialReference sRef = (display.SpatialReference != null) ?
                display.SpatialReference.Clone() as ISpatialReference :
                null;

                int iWidth = display.ImageWidth, iHeight = display.ImageHeight;

                StringBuilder sb = new StringBuilder();
                sb.Append("<?xml version='1.0' encoding='utf-8'?>");
                sb.Append("<ARCXML version='1.1'>");
                sb.Append("<REQUEST>");
                sb.Append("<GET_IMAGE>");
                sb.Append("<PROPERTIES>");
                sb.Append("<ENVELOPE minx='" + display.Envelope.minx.ToString() + "' miny='" + display.Envelope.miny.ToString() + "' maxx='" + display.Envelope.maxx.ToString() + "' maxy='" + display.Envelope.maxy.ToString() + "' />");
                sb.Append("<IMAGESIZE width='" + iWidth + "' height='" + iHeight + "' />");
                sb.Append("<BACKGROUND color='255,255,255' transcolor='255,255,255' />");
                //if (display.SpatialReference != null && !display.SpatialReference.Equals(_sRef))
                //{
                //    string map_param = gView.Framework.Geometry.SpatialReference.ToProj4(display.SpatialReference);
                //    sb.Append("<SPATIALREFERENCE name='" + display.SpatialReference.Name + "' param='" + map_param + "' />");
                //}

                if (sRef != null)
                {
                    string map_param = gView.Framework.Geometry.SpatialReference.ToProj4(display.SpatialReference);
                    sb.Append("<SPATIALREFERENCE name='" + display.SpatialReference.Name + "' param='" + map_param + "' />");

                    string wkt = gView.Framework.Geometry.SpatialReference.ToESRIWKT(sRef);
                    string geotranwkt = gView.Framework.Geometry.SpatialReference.ToESRIGeotransWKT(sRef);

                    if (wkt != null)
                    {
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
                    }
                }



                sb.Append("<LAYERLIST>");
                foreach (IWebServiceTheme theme in Themes)
                {
                    sb.Append("<LAYERDEF id='" + theme.LayerID + "' visible='" + (theme.Visible && !theme.Locked).ToString() + "' />");
                }
                sb.Append("</LAYERLIST>");
                sb.Append("</PROPERTIES>");
                sb.Append("</GET_IMAGE>");
                sb.Append("</REQUEST>");
                sb.Append("</ARCXML>");

                string user = ConfigTextStream.ExtractValue(_dataset._connection, "user");
                string pwd = Identity.HashPassword(ConfigTextStream.ExtractValue(_dataset._connection, "pwd"));

                if ((user == "#" || user == "$") &&
                    context != null && context.ServiceRequest != null && context.ServiceRequest.Identity != null)
                {
                    string roles = String.Empty;
                    if (user == "#" && context.ServiceRequest.Identity.UserRoles != null)
                    {
                        foreach (string role in context.ServiceRequest.Identity.UserRoles)
                        {
                            if (String.IsNullOrEmpty(role))
                            {
                                continue;
                            }

                            roles += "|" + role;
                        }
                    }
                    user = context.ServiceRequest.Identity.UserName + roles;
                    // ToDo:
                    //pwd = context.ServiceRequest.Identity.HashedPassword;
                }

#if(DEBUG)
                //Logger.LogDebug("Start gView Mapserver Request");
#endif
                ServerConnection service = new ServerConnection(ConfigTextStream.ExtractValue(_dataset._connection, "server"));
                string resp = service.Send(_name, sb.ToString(), "BB294D9C-A184-4129-9555-398AA70284BC", user, pwd);

#if(DEBUG)
                //Logger.LogDebug("gView Mapserver Request Finished");
#endif

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(resp);

                IBitmap bitmap = null;
                XmlNode output = doc.SelectSingleNode("//OUTPUT[@file]");
                if (_image != null)
                {
                    _image.Dispose();
                    _image = null;
                }

                bitmap = await WebFunctions.DownloadImage(_dataset._http, output);

                if (bitmap != null)
                {
                    _image = new GeorefBitmap(bitmap);
                    _image.SpatialReference = display.SpatialReference;
                    _image.Envelope = display.Envelope;

                    AfterMapRequest?.Invoke(this, display, _image);
                }

                return _image != null;
            }
            catch (Exception ex)
            {
                MapServerClass.ErrorLog(context, "MapRequest", ConfigTextStream.ExtractValue(_dataset._connection, "server"), _name, ex);
                return false;
            }
        }

        public Task<bool> LegendRequest(IDisplay display)
        {
            return Task.FromResult(false);
        }

        GeorefBitmap IWebServiceClass.Image
        {
            get { return _image; }
        }

        IBitmap IWebServiceClass.Legend
        {
            get { return _legend; }
        }

        public IEnvelope Envelope
        {
            get
            {
                if (_dataset == null)
                {
                    return null;
                }

                if (!_dataset._opened)
                {
                    _dataset.Open();
                }

                return _dataset.Envelope().Result;
            }
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
                    if (!_dataset._opened)
                    {
                        _dataset.Open();
                    }

                    return _dataset._themes;
                }
                return new List<IWebServiceTheme>();
            }
        }

        public ISpatialReference SpatialReference
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

        #endregion IWebServiceClass Member

        #region IClone Member

        public object Clone()
        {
            MapServerClass clone = new MapServerClass(_dataset);
            clone._clonedThemes = new List<IWebServiceTheme>();

            foreach (IWebServiceTheme theme in Themes)
            {
                if (theme == null || theme.Class == null)
                {
                    continue;
                }

                clone._clonedThemes.Add(LayerFactory.Create(theme.Class, theme, clone) as IWebServiceTheme);
            }
            clone.AfterMapRequest = AfterMapRequest;
            return clone;
        }

        #endregion IClone Member

        public static void Log(IServiceRequestContext context, string header, string server, string service, StringBuilder axl)
        {
            if (context == null ||
                context.MapServer == null ||
                context.MapServer.LoggingEnabled(loggingMethod.request_detail_pro) == false)
            {
                return;
            }
        }

        public static void ErrorLog(IServiceRequestContext context, string header, string server, string service, Exception ex)
        {
            if (context == null ||
                context.MapServer == null ||
                context.MapServer.LoggingEnabled(loggingMethod.error) == false)
            {
                return;
            }

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
        }
    }
}