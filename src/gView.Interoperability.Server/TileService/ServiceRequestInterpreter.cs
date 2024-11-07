//using System;
//using System.Collections.Generic;
//using System.Text;
//using gView.MapServer;
//using System.Xml;
//using gView.Framework.Common;
//using gView.Framework.Carto;
//using gView.Framework.Data;
//using gView.Framework.Geometry;
//using System.IO;
//using System.Threading.Tasks;

//namespace gView.Interoperability.Server.TileService
//{
//    [RegisterPlugIn("B6748883-23D6-4363-BA51-2B98C1291E0C")]
//    class ServiceRequestInterpreter : IServiceRequestInterpreter
//    {
//        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
//        private IMapServer _mapServer = null;

//        #region IServiceRequestInterpreter Member

//        public void OnCreate(IMapServer mapServer)
//        {
//            _mapServer = mapServer;
//        }

//        async public Task Request(IServiceRequestContext context)
//        {
//            if (context == null || context.ServiceRequest == null)
//                return;

//            if (_mapServer == null)
//            {
//                context.ServiceRequest.Response = "<FATALERROR>MapServer Object is not available!</FATALERROR>";
//                return;
//            }

//            string service = context.ServiceRequest.Service;
//            string request = context.ServiceRequest.Request;

//            try
//            {
//                XmlDocument doc = new XmlDocument();
//                doc.LoadXml(request);

//                XmlNode rNode = doc.SelectSingleNode("//TileRequest");
//                XmlNode rType = rNode.FirstChild;

//                switch (rType.Name)
//                {
//                    case "QueryTiles":
//                        PerformQueryTilesRequest(context, rType);
//                        break;
//                }
//            }
//            catch (Exception ex)
//            {
//                context.ServiceRequest.Response = CreateException(ex.Message);
//                return;
//            }
//        }

//        public string IntentityName
//        {
//            get { return String.Empty; }
//        }

//        public InterpreterCapabilities Capabilities
//        {
//            get { return null; }
//        }

//        public string IdentityName => "ServiceRequestInterpreter";

//        public string IdentityLongName => "ServiceRequestInterpreter";

//        #endregion

//        private void PerformQueryTilesRequest(IServiceRequestContext context, XmlNode rType)
//        {
//            if (context == null || context.ServiceRequest == null)
//            {
//                return;
//            }

//            ServiceRequest serviceRequest = context.ServiceRequest;

//            try
//            {
//                int level=0;
//                if (rType.Attributes["level"] != null)
//                    level = int.Parse(rType.Attributes["level"].Value);


//                using (IServiceMap map = _mapServer.GetServiceMapAsync(context).Result)
//                {
//                    if (map == null)
//                    {
//                        serviceRequest.Response = CreateException("Service not found");
//                        return;
//                    }

//                    #region QueryGeometry
//                    IGeometry queryGeometry = null;

//                    #region Envelope
//                    XmlNode envelopeNode = rType.SelectSingleNode("Envelope[@minx and @miny and @maxx and @maxy]");
//                    if (envelopeNode != null)
//                    {
//                        Envelope queryEnvelope = new Envelope(
//                            double.Parse(envelopeNode.Attributes["minx"].Value, _nhi),
//                            double.Parse(envelopeNode.Attributes["miny"].Value, _nhi),
//                            double.Parse(envelopeNode.Attributes["maxx"].Value, _nhi),
//                            double.Parse(envelopeNode.Attributes["maxy"].Value, _nhi));
//                        queryGeometry = queryEnvelope;
//                    }
//                    #endregion

//                    #region Polygon
//                    XmlNode polygonNode = rType.SelectSingleNode("Polygon");
//                    if (polygonNode != null)
//                    {
//                        Polygon polygon = new Polygon();
//                        foreach (XmlNode ringNode in polygonNode.SelectNodes("Ring"))
//                        {
//                            Ring ring = new Ring();
//                            foreach (XmlNode pointNode in ringNode.SelectNodes("Point[@x and @y]"))
//                            {
//                                ring.AddPoint(new Point(double.Parse(pointNode.Attributes["x"].Value, _nhi),
//                                                        double.Parse(pointNode.Attributes["y"].Value, _nhi)));
//                            }
//                            if (ring.PointCount > 2)
//                                polygon.AddRing(ring);
//                        }
//                        if (polygon.RingCount == 0)
//                        {
//                            serviceRequest.Response = CreateException("Invalid Polygon definition node");
//                            return;
//                        }
//                        queryGeometry = polygon;
//                    }
//                    #endregion

//                    if (queryGeometry == null)
//                    {
//                        serviceRequest.Response = CreateException("No geometry (Envelope,Polygon) definition node");
//                        return;
//                    }
//                    #endregion

//                    #region Layer/Featureclass
//                    XmlNode layerNode = rType.SelectSingleNode("Layer[@id]");
//                    if (layerNode == null)
//                    {
//                        serviceRequest.Response = CreateException("No layer definition node");
//                        return;
//                    }
//                    string id = layerNode.Attributes["id"].Value;
//                    MapServerHelper.Layers layers = MapServerHelper.FindMapLayers(map, true, id);
//                    if (layers.Count != 1)
//                    {
//                        serviceRequest.Response = CreateException("Can't find layer with id='" + id + "'");
//                        return;
//                    }
//                    IFeatureClass fc = layers[0].Class as IFeatureClass;
//                    if (fc == null || fc.FindField("GRID_LEVEL") == null ||
//                                   fc.FindField("GRID_ROW") == null ||
//                                   fc.FindField("GRID_COLUMN") == null ||
//                                   fc.FindField("FILE") == null)
//                    {
//                        serviceRequest.Response = CreateException("Featureclass is not a tilegrid");
//                        return;
//                    }
//                    #endregion

//                    #region Query
//                    SpatialFilter filter = new SpatialFilter();
//                    filter.AddField(fc.IDFieldName);
//                    filter.AddField(fc.ShapeFieldName);
//                    filter.AddField("GRID_LEVEL");
//                    filter.AddField("GRID_ROW");
//                    filter.AddField("GRID_COLUMN");
//                    filter.AddField("FILE");
//                    filter.Geometry = queryGeometry;
//                    filter.WhereClause = "GRID_LEVEL=" + level;

//                    StringBuilder sb = new StringBuilder();
//                    sb.Append("<TileRequest><Tiles>");
//                    using (IFeatureCursor cursor = fc.GetFeatures(filter))
//                    {
//                        IFeature feature;
//                        while ((feature = cursor.NextFeature) != null)
//                        {
//                            sb.Append("<Tile");

//                            #region Envelope
//                            IEnvelope env = feature.Shape.Envelope;
//                            sb.Append(" id='" + feature.OID + "'");
//                            sb.Append(" minx='" + env.minx.ToString(_nhi) + "'");
//                            sb.Append(" miny='" + env.miny.ToString(_nhi) + "'");
//                            sb.Append(" maxx='" + env.maxx.ToString(_nhi) + "'");
//                            sb.Append(" maxy='" + env.maxy.ToString(_nhi) + "'");
//                            #endregion

//                            #region File
//                            FileInfo fi = new FileInfo(feature["FILE"].ToString());
//                            sb.Append(" path='/" + feature["GRID_LEVEL"].ToString() + "/" + feature["GRID_ROW"].ToString() + "/" + feature["GRID_COLUMN"].ToString() + fi.Extension + "'");
//                            #endregion
//                            sb.Append(" />");
//                        }
//                    }
//                    sb.Append("</Tiles></TileRequest>");
//                    #endregion

//                    serviceRequest.Response = sb.ToString();
//                }
//            }
//            catch (Exception ex)
//            {
//                serviceRequest.Response = CreateException(ex.Message);
//                return;
//            }
//        }

//        #region Helper
//        private string CreateException(string msg)
//        {
//            return "<ERROR>" + msg + "</ERROR>";
//        }

//        public AccessTypes RequiredAccessTypes(IServiceRequestContext context)
//        {
//            throw new NotImplementedException();
//        }
//        #endregion
//    }
//}
