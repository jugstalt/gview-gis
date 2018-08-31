using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using gView.Framework.UI;
using gView.Framework.Carto;
using gView.Framework.Symbology;
using gView.Framework.system;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.MapServer;
using System.Globalization;

namespace gView.Framework.XML
{

    //public class AXL
    //{
    //    string _rootElement, _version;
    //    public AXL(string rootElement,string version)
    //    {
    //        _rootElement = rootElement;
    //        _version = version;
    //    }

    //    public IServiceMap Map(IMapServer server, string serviceName)
    //    {
    //        if (server == null) return null;

    //        return server[serviceName];
    //    }

    //    public string GETCLIENTSERVICES(IMapServer server)
    //    {
    //        StringBuilder axl = new StringBuilder();
    //        MemoryStream ms = new MemoryStream();
    //        XmlTextWriter xWriter = new XmlTextWriter(ms, Encoding.UTF8);

    //        xWriter.WriteStartDocument();
    //        xWriter.WriteStartElement(_rootElement);
    //        xWriter.WriteAttributeString("version", _version);

    //        xWriter.WriteStartElement("RESPONSE");
    //        xWriter.WriteStartElement("SERVICES");

    //        foreach(IMap map in server.Maps)
    //        {
    //            xWriter.WriteStartElement("SERVICE");
    //            xWriter.WriteAttributeString("access", "PUBLIC");
    //            xWriter.WriteAttributeString("name", map.Name);
    //            xWriter.WriteAttributeString("private", "true");
    //            xWriter.WriteAttributeString("servicegroup", "ImageServer1");
    //            xWriter.WriteAttributeString("status", "ENABLED");
    //            xWriter.WriteAttributeString("type", "ImageServer");
    //            xWriter.WriteAttributeString("version", "");

    //            xWriter.WriteStartElement("IMAGE");
    //            xWriter.WriteAttributeString("type", "PNG");
    //            xWriter.WriteEndElement(); // IMAGE

    //            xWriter.WriteStartElement("ENVIRONMENT");
    //            xWriter.WriteStartElement("LOCALE");
    //            xWriter.WriteAttributeString("country", "AT");
    //            xWriter.WriteAttributeString("language", "de");
    //            xWriter.WriteAttributeString("variant", "");
    //            xWriter.WriteEndElement(); // LOCALE
    //            xWriter.WriteStartElement("UIFONT");
    //            xWriter.WriteAttributeString("name", "Arial");
    //            xWriter.WriteEndElement(); // UIFONG
    //            xWriter.WriteEndElement(); // ENVIRONMENT

    //            xWriter.WriteStartElement("CLEANUP");
    //            xWriter.WriteAttributeString("interval", "5");
    //            xWriter.WriteEndElement(); // CLEANUP;

    //            xWriter.WriteEndElement(); // SERVICE
    //        }
    //        xWriter.WriteEndElement(); 
    //        xWriter.WriteEndDocument();
    //        xWriter.Flush();

    //        ms.Position = 0;
    //        StreamReader sr = new StreamReader(ms);
    //        axl.Append(sr.ReadToEnd());
    //        sr.Close();
    //        ms.Close();
    //        xWriter.Close();

    //        return axl.ToString();
    //    }

    //    private int GetAxlFieldType(FieldType fieldType)
    //    {
    //        switch (fieldType)
    //        {
    //            case FieldType.ID: return -99;
    //            case FieldType.Shape: return -98;
    //            case FieldType.boolean: return -7;
    //            case FieldType.biginteger: return -5;
    //            case FieldType.character: return 1;
    //            case FieldType.integer: return 4;
    //            case FieldType.smallinteger: return 5;
    //            case FieldType.Float: return 6;
    //            case FieldType.Double: return 8;
    //            case FieldType.String: return 12;
    //            case FieldType.Date: return 91;
    //        }
    //        return 0;
    //    }

    //    public string GET_SERVICE_INFO(IMapServer server, string serviceName, bool fields, bool envelope)
    //    {
    //        IServiceMap map = Map(server, serviceName);
    //        if (map == null) return "";

    //        double dpm = 96.0 / 0.0254;

    //        StringBuilder axl = new StringBuilder();
    //        MemoryStream ms = new MemoryStream();
    //        XmlTextWriter xWriter = new XmlTextWriter(ms, Encoding.UTF8);

    //        xWriter.WriteStartDocument();
    //        xWriter.WriteStartElement(_rootElement);

    //        xWriter.WriteAttributeString("version", _version);

    //        xWriter.WriteStartElement("ONLINE_RES");
    //        xWriter.WriteAttributeString("service_name", map.Name);
    //        xWriter.WriteAttributeString("xlink_href", "http://P0600130/gViewConnector/wms.aspx?ServiceName="+map.Name+"&");
    //        xWriter.WriteEndElement(); // ONLINE_RES

    //        xWriter.WriteStartElement("RESPONSE");
    //        xWriter.WriteStartElement("SERVICEINFO");
    //        xWriter.WriteAttributeString("service", map.Name);
    //        xWriter.WriteStartElement("ENVIRONMENT");
    //        xWriter.WriteStartElement("SCREEN");
    //        xWriter.WriteAttributeString("dpi", "96");
    //        xWriter.WriteEndElement(); // SCREEN
    //        xWriter.WriteEndElement(); // ENVIRONMENT

    //        List<IDatasetElement> layers;

    //        xWriter.WriteStartElement("PROPERTIES");
    //        if (envelope)
    //        {
    //            Envelope mapEnvelope = null;
    //            layers = map.MapElements;

    //            foreach (IDatasetElement element in map.MapElements)
    //            {
    //                IEnvelope env = null;
    //                if (element is IFeatureLayer)
    //                {
    //                    if (((IFeatureLayer)element).FeatureClass == null) continue;
    //                    IGeometry geom = GeometricTransformer.Transform2D(((IFeatureLayer)element).FeatureClass.Envelope, ((IFeatureLayer)element).FeatureClass.SpatialReference, map.Display.SpatialReference);
    //                    if (geom != null) env = geom.Envelope;
    //                }
    //                else if (element is IRasterLayer && ((IRasterLayer)element).RasterClass != null)
    //                {
    //                    if (((IRasterLayer)element).RasterClass.Polygon == null) continue;
    //                    IGeometry geom = GeometricTransformer.Transform2D(((IRasterLayer)element).RasterClass.Polygon.Envelope, ((IRasterLayer)element).RasterClass.SpatialReference, map.Display.SpatialReference);
    //                    if (geom != null) env = geom.Envelope;
    //                }
    //                else if (element is IWebServiceLayer && ((IWebServiceLayer)element).WebServiceClass != null)
    //                {
    //                    if (((IWebServiceLayer)element).WebServiceClass.Envelope == null) continue;
    //                    IGeometry geom = GeometricTransformer.Transform2D(((IWebServiceLayer)element).WebServiceClass.Envelope, ((IWebServiceLayer)element).WebServiceClass.SpatialReference, map.Display.SpatialReference);
    //                    if (geom != null) env = geom.Envelope;
    //                }
    //                if (env == null) continue;
    //                if (mapEnvelope == null)
    //                    mapEnvelope = new Envelope(env);
    //                else
    //                    mapEnvelope.Union(env);
    //            }
    //            ENVELOPE(xWriter, mapEnvelope);
    //        }

    //        // SpatialReference
    //        if (map.Display.SpatialReference != null)
    //        {
    //            xWriter.WriteStartElement("SPATIALREFERENCE");
    //            xWriter.WriteAttributeString("map", SpatialReference.GetParameterString(map.Display.SpatialReference));
    //            xWriter.WriteEndElement();
    //        }
    //        xWriter.WriteEndElement(); // PROPERTIES

    //        layers = map.MapElements;

    //        foreach(IDatasetElement element in layers)
    //        {
    //            string name = "";
    //            ITOCElement tocElement = null;
    //            if (map.TOC != null && element is ILayer)
    //            {
    //                tocElement = map.TOC.GetTOCElement(element as ILayer);
    //                if (tocElement != null) name = tocElement.name;
    //            }
    //            if (element is IFeatureLayer)
    //            {
    //                IFeatureLayer fLayer = (IFeatureLayer)element;
    //                if (fLayer.FeatureClass != null)
    //                {
    //                    xWriter.WriteStartElement("LAYERINFO");
    //                    xWriter.WriteAttributeString("type", "featureclass");
    //                    xWriter.WriteAttributeString("name", (name!="") ? name : fLayer.Title.Replace(".","_"));
    //                    xWriter.WriteAttributeString("id", fLayer.ID.ToString());
    //                    xWriter.WriteAttributeString("visible", fLayer.Visible.ToString().ToLower());
    //                    if (fLayer.MinimumScale > 1)
    //                        xWriter.WriteAttributeString("minscale", (fLayer.MinimumScale / dpm).ToString());
    //                    if (fLayer.MaximumScale > 1)
    //                        xWriter.WriteAttributeString("maxscale", (fLayer.MaximumScale / dpm).ToString());

    //                    if (tocElement != null)
    //                    {
    //                        xWriter.WriteAttributeString("aliasname", tocElement.name);
    //                        string group = "";
    //                        ITOCElement parentElement=tocElement;
    //                        while ((parentElement = parentElement.ParentGroup) != null)
    //                        {
    //                            group = parentElement.name + ((group != "") ? "/" + group : ""); 
    //                        }
    //                        xWriter.WriteAttributeString("group", group);
    //                    }
    //                    xWriter.WriteStartElement("FCLASS");
    //                    switch (fLayer.FeatureClass.GeometryType)
    //                    {
    //                        case geometryType.Envelope:
    //                        case geometryType.Polygon:
    //                            xWriter.WriteAttributeString("type", "polygon");
    //                            break;
    //                        case geometryType.Polyline:
    //                            xWriter.WriteAttributeString("type", "line");
    //                            break;
    //                        case geometryType.Point:
    //                        case geometryType.Multipoint:
    //                            xWriter.WriteAttributeString("type", "point");
    //                            break;
    //                    }
    //                    if (envelope)
    //                    {
    //                        IGeometry geom = GeometricTransformer.Transform2D(fLayer.FeatureClass.Envelope, fLayer.FeatureClass.SpatialReference,map.Display.SpatialReference);
    //                        if (geom != null)
    //                        {
    //                            ENVELOPE(xWriter, geom.Envelope);
    //                        }
    //                    }
    //                    if (fields)
    //                    {
    //                        foreach (IField field in fLayer.FeatureClass.Fields)
    //                        {
    //                            xWriter.WriteStartElement("FIELD");
    //                            xWriter.WriteAttributeString("name", field.name);
    //                            xWriter.WriteAttributeString("type", GetAxlFieldType(field.type).ToString());
    //                            xWriter.WriteAttributeString("size", field.size.ToString());
    //                            xWriter.WriteAttributeString("precision", field.precision.ToString());
    //                            xWriter.WriteEndElement(); // FIELD
    //                        }
    //                    }
    //                    xWriter.WriteEndElement(); // FCLASS
    //                    xWriter.WriteEndElement(); // LAYERINFO
    //                }
    //            }
    //            else if (element is IRasterLayer && ((IRasterLayer)element).RasterClass!=null)
    //            {
    //                IRasterLayer rLayer = (IRasterLayer)element;
    //                xWriter.WriteStartElement("LAYERINFO");
    //                xWriter.WriteAttributeString("type", "image");
    //                xWriter.WriteAttributeString("name", (name!="") ? name : rLayer.Title.Replace(".","_"));
    //                xWriter.WriteAttributeString("id", rLayer.ID.ToString());
    //                xWriter.WriteAttributeString("visible", rLayer.Visible.ToString().ToLower());

    //                if (rLayer.MinimumScale > 1)
    //                    xWriter.WriteAttributeString("minscale", (rLayer.MinimumScale / dpm).ToString());
    //                if (rLayer.MaximumScale > 1)
    //                    xWriter.WriteAttributeString("maxscale", (rLayer.MaximumScale / dpm).ToString());

    //                //ITOCElement tocElement = map.TOC.GetTOCElement((ILayer)element);
    //                if (tocElement != null)
    //                {
    //                    xWriter.WriteAttributeString("aliasname", tocElement.name);
    //                    string group = "";
    //                    ITOCElement parentElement = tocElement;
    //                    while ((parentElement = parentElement.ParentGroup) != null)
    //                    {
    //                        group = parentElement.name + ((group != "") ? "/" + group : "");
    //                    }
    //                    xWriter.WriteAttributeString("group", group);
    //                }

    //                if (envelope && rLayer.RasterClass.Polygon != null)
    //                {
    //                    IGeometry geom = GeometricTransformer.Transform2D(rLayer.RasterClass.Polygon.Envelope, rLayer.RasterClass.SpatialReference,map.Display.SpatialReference);
    //                    if (geom != null)
    //                    {
    //                        ENVELOPE(xWriter, geom.Envelope);
    //                    }
    //                }
    //                xWriter.WriteEndElement(); // LAYERINFO
    //            }
    //            else if (element is IWebServiceLayer && ((IWebServiceLayer)element).WebServiceClass != null && ((IWebServiceLayer)element).WebServiceClass.Themes != null)
    //            {
    //                IWebServiceLayer wLayer = (IWebServiceLayer)element;
    //                foreach (IWebServiceTheme theme in wLayer.WebServiceClass.Themes)
    //                {
    //                    if (theme == null) continue;
    //                    if (map.TOC != null && element is ILayer)
    //                    {
    //                        tocElement = map.TOC.GetTOCElement(theme as ILayer);
    //                        if (tocElement != null) name = tocElement.name;
    //                    }
    //                    if (theme.FeatureClass is IFeatureClass)
    //                    {
    //                        xWriter.WriteStartElement("LAYERINFO");
    //                        xWriter.WriteAttributeString("type", "featureclass");
    //                        xWriter.WriteAttributeString("name", (name != "") ? name : theme.Title.Replace(".", "_"));
    //                        xWriter.WriteAttributeString("id", theme.ID.ToString());
    //                        xWriter.WriteAttributeString("visible", theme.Visible.ToString().ToLower());
    //                        if (theme.MinimumLabelScale > 1)
    //                            xWriter.WriteAttributeString("minscale", (theme.MinimumScale / dpm).ToString());
    //                        if (theme.MaximumScale > 1)
    //                            xWriter.WriteAttributeString("maxscale", (theme.MaximumScale / dpm).ToString());

    //                        if (tocElement != null)
    //                        {
    //                            xWriter.WriteAttributeString("aliasname", tocElement.name);
    //                            string group = "";
    //                            ITOCElement parentElement = tocElement;
    //                            while ((parentElement = parentElement.ParentGroup) != null)
    //                            {
    //                                group = parentElement.name + ((group != "") ? "/" + group : "");
    //                            }
    //                            xWriter.WriteAttributeString("group", group);
    //                        }
    //                        xWriter.WriteStartElement("FCLASS");
    //                        switch (theme.FeatureClass.GeometryType)
    //                        {
    //                            case geometryType.Envelope:
    //                            case geometryType.Polygon:
    //                                xWriter.WriteAttributeString("type", "polygon");
    //                                break;
    //                            case geometryType.Polyline:
    //                                xWriter.WriteAttributeString("type", "line");
    //                                break;
    //                            case geometryType.Point:
    //                            case geometryType.Multipoint:
    //                                xWriter.WriteAttributeString("type", "point");
    //                                break;
    //                        }
    //                        if (envelope)
    //                        {
    //                            IGeometry geom = GeometricTransformer.Transform2D(theme.FeatureClass.Envelope, theme.FeatureClass.SpatialReference, map.Display.SpatialReference);
    //                            if (geom != null)
    //                            {
    //                                ENVELOPE(xWriter, geom.Envelope);
    //                            }
    //                        }
    //                        if (fields)
    //                        {
    //                            foreach (IField field in theme.FeatureClass.Fields)
    //                            {
    //                                xWriter.WriteStartElement("FIELD");
    //                                xWriter.WriteAttributeString("name", field.name);
    //                                xWriter.WriteAttributeString("type", GetAxlFieldType(field.type).ToString());
    //                                xWriter.WriteAttributeString("size", field.size.ToString());
    //                                xWriter.WriteAttributeString("precision", field.precision.ToString());
    //                                xWriter.WriteEndElement(); // FIELD
    //                            }
    //                        }
    //                        xWriter.WriteEndElement(); // FCLASS
    //                        xWriter.WriteEndElement(); // LAYERINFO
    //                    }
    //                    else  // Raster
    //                    {
    //                        xWriter.WriteStartElement("LAYERINFO");
    //                        xWriter.WriteAttributeString("type", "image");
    //                        xWriter.WriteAttributeString("name", (name != "") ? name : theme.Title.Replace(".", "_"));
    //                        xWriter.WriteAttributeString("id", theme.ID.ToString());
    //                        xWriter.WriteAttributeString("visible", theme.Visible.ToString().ToLower());

    //                        if (theme.MinimumScale > 1)
    //                            xWriter.WriteAttributeString("minscale", (theme.MinimumScale / dpm).ToString());
    //                        if (theme.MaximumScale > 1)
    //                            xWriter.WriteAttributeString("maxscale", (theme.MaximumScale / dpm).ToString());

    //                        if (tocElement != null)
    //                        {
    //                            xWriter.WriteAttributeString("aliasname", tocElement.name);
    //                            string group = "";
    //                            ITOCElement parentElement = tocElement;
    //                            while ((parentElement = parentElement.ParentGroup) != null)
    //                            {
    //                                group = parentElement.name + ((group != "") ? "/" + group : "");
    //                            }
    //                            xWriter.WriteAttributeString("group", group);
    //                        }
    //                        xWriter.WriteEndElement(); // LAYERINFO
    //                    }
    //                }
    //            }
    //        }
    //        xWriter.WriteEndElement();
    //        xWriter.WriteEndDocument();
    //        xWriter.Flush();

    //        ms.Position = 0;
    //        StreamReader sr = new StreamReader(ms);
    //        axl.Append(sr.ReadToEnd());
    //        sr.Close();
    //        ms.Close();
    //        xWriter.Close();

    //        return axl.ToString();
    //    }

    //    public string Renderer(IFeatureLayer layer)
    //    {
    //        if (layer == null) return "";

    //        StringBuilder sb = new StringBuilder();
    //        string fRenderer = ObjectFromAXLFactory.ConvertToAXL(layer.FeatureRenderer);
    //        string lRenderer = ObjectFromAXLFactory.ConvertToAXL(layer.LabelRenderer);

    //        if (!fRenderer.Equals(String.Empty) && !lRenderer.Equals(String.Empty))
    //        {
    //            sb.Append("<GROUPRENDERER>\n");
    //            sb.Append(fRenderer);
    //            sb.Append(lRenderer);
    //            sb.Append("</GROUPRENDERER>\n");
    //        }
    //        else if (!fRenderer.Equals(String.Empty))
    //        {
    //            sb.Append(fRenderer);
    //        }
    //        else if (lRenderer.Equals(String.Empty))
    //        {
    //            sb.Append(lRenderer);
    //        }

    //        return "";
    //    }

    //    public string IMAGE(string outputPath, string outputUrl, string Title, IEnvelope envelope)
    //    {
    //        StringBuilder axl = new StringBuilder();
    //        MemoryStream ms = new MemoryStream();
    //        XmlTextWriter xWriter = new XmlTextWriter(ms, Encoding.UTF8);

    //        xWriter.WriteStartDocument();
    //        xWriter.WriteStartElement(_rootElement);
    //        xWriter.WriteAttributeString("version", _version);

    //        xWriter.WriteStartElement("RESPONSE");
    //        xWriter.WriteStartElement("IMAGE");
    //        ENVELOPE(xWriter, envelope);
    //        xWriter.WriteStartElement("OUTPUT");
    //        xWriter.WriteAttributeString("file", outputPath + @"\" + Title);
    //        xWriter.WriteAttributeString("url", outputUrl + "/" + Title);
    //        xWriter.WriteEndElement();  // OUTPUT

    //        xWriter.WriteEndElement();
    //        xWriter.WriteEndDocument();
    //        xWriter.Flush();

    //        ms.Position = 0;
    //        StreamReader sr = new StreamReader(ms);
    //        axl.Append(sr.ReadToEnd());
    //        sr.Close();
    //        ms.Close();
    //        xWriter.Close();

    //        return axl.ToString();
    //    }

    //    static public void ENVELOPE(XmlTextWriter xWriter, IEnvelope envelope)
    //    {
    //        if (xWriter == null || envelope == null) return;
    //        xWriter.WriteStartElement("ENVELOPE");
    //        xWriter.WriteAttributeString("minx", envelope.minx.ToString());
    //        xWriter.WriteAttributeString("miny", envelope.miny.ToString());
    //        xWriter.WriteAttributeString("maxx", envelope.maxx.ToString());
    //        xWriter.WriteAttributeString("maxy", envelope.maxy.ToString());
    //        xWriter.WriteEndElement();
    //    }
    //}

    public class ArcXMLGeometry
    {
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        /*
        static public IGeometry AXL2Geometry(string axl)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<g>" + axl + "</g>");

                AggregateGeometry aGeom = new AggregateGeometry();
                IPoint point;

                foreach (XmlNode node in doc.ChildNodes[0].ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "POINT":
                            point = new Point(
                                Convert.ToDouble(node.Attributes["x"].Value.Replace(".", ",")),
                                Convert.ToDouble(node.Attributes["y"].Value.Replace(".", ",")));
                            aGeom.AddGeometry(point);
                            break;
                        case "MULTIPOINT":
                            IMultiPoint mpoint = new MultiPoint();
                            foreach (XmlNode pointnode in node.SelectNodes("POINT"))
                            {
                                point = new Point(
                                    Convert.ToDouble(pointnode.Attributes["x"].Value.Replace(".", ",")),
                                    Convert.ToDouble(pointnode.Attributes["y"].Value.Replace(".", ",")));
                                mpoint.AddPoint(point);
                            }
                            aGeom.AddGeometry(mpoint);
                            break;
                        case "ENVELOPE":
                            IEnvelope envelope = new Envelope(
                                Convert.ToDouble(node.Attributes["minx"].Value.Replace(".", ",")),
                                Convert.ToDouble(node.Attributes["miny"].Value.Replace(".", ",")),
                                Convert.ToDouble(node.Attributes["maxx"].Value.Replace(".", ",")),
                                Convert.ToDouble(node.Attributes["maxy"].Value.Replace(".", ",")));
                            aGeom.AddGeometry(envelope);
                            break;
                        case "POLYLINE":
                            IPolyline pline = new Polyline();
                            foreach (XmlNode pathnode in node.SelectNodes("PATH"))
                            {
                                IPath path = new gView.Framework.Geometry.Path();
                                foreach (XmlNode pointnode in pathnode.SelectNodes("POINT"))
                                {
                                    point = new Point(
                                        Convert.ToDouble(pointnode.Attributes["x"].Value.Replace(".", ",")),
                                        Convert.ToDouble(pointnode.Attributes["y"].Value.Replace(".", ",")));
                                    path.AddPoint(point);
                                }
                                pline.AddPath(path);
                            }
                            aGeom.AddGeometry(pline);
                            break;
                        case "POLYGON":
                            IPolygon pgon = new Polygon();
                            foreach (XmlNode ringnode in node.SelectNodes("RING"))
                            {
                                IRing ring = new gView.Framework.Geometry.Ring();
                                foreach (XmlNode pointnode in ringnode.SelectNodes("POINT"))
                                {
                                    point = new Point(
                                        Convert.ToDouble(pointnode.Attributes["x"].Value.Replace(".", ",")),
                                        Convert.ToDouble(pointnode.Attributes["y"].Value.Replace(".", ",")));
                                    ring.AddPoint(point);
                                }
                                pgon.AddRing(ring);
                            }
                            foreach (XmlNode ringnode in node.SelectNodes("HOLE"))
                            {
                                IRing ring = new gView.Framework.Geometry.Ring();
                                foreach (XmlNode pointnode in ringnode.SelectNodes("POINT"))
                                {
                                    point = new Point(
                                        Convert.ToDouble(pointnode.Attributes["x"].Value.Replace(".", ",")),
                                        Convert.ToDouble(pointnode.Attributes["y"].Value.Replace(".", ",")));
                                    ring.AddPoint(point);
                                }
                                pgon.AddRing(ring);
                            }
                            aGeom.AddGeometry(pgon);
                            break;
                    }
                }

                if (aGeom.GeometryCount == 0)
                    return null;
                if (aGeom.GeometryCount == 1)
                    return aGeom[0];

                return aGeom;
            }
            catch
            {
                return null;
            }
        }
         * */

        static public IGeometry AXL2Geometry(string axl)
        {
            return AXL2Geometry(axl, true);
        }

        static private void AddSrs(XmlNode node, IGeometry geometry)
        {
            if (node == null || node.Attributes["srs"] == null)
                return;
            int srs;
            if (int.TryParse(node.Attributes["srs"].Value, out srs) && srs > 0)
            {
                geometry.Srs = srs;
            }
        }

        static public IGeometry AXL2Geometry(string axl, bool ignoreBuffer)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<g>" + axl + "</g>");

                AggregateGeometry aGeom = new AggregateGeometry();
                IPoint point;

                XmlNode bufferNode = null;
                foreach (XmlNode node in doc.ChildNodes[0].ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "BUFFER":
                            if (!ignoreBuffer) bufferNode = node;
                            break;
                        case "POINT":
                            point = PointFromAxl(node);
                            AddSrs(node, point);
                            aGeom.AddGeometry(point);
                            break;
                        case "MULTIPOINT":
                            IMultiPoint mpoint = new MultiPoint();
                            AddSrs(node, mpoint);
                            foreach (XmlNode pointnode in node.SelectNodes("POINT"))
                            {
                                point = PointFromAxl(pointnode);
                                mpoint.AddPoint(point);
                            }
                            if (mpoint.PointCount == 0)
                                PointsFromCoords(mpoint, node.SelectSingleNode("COORDS"));
                            aGeom.AddGeometry(mpoint);
                            break;
                        case "ENVELOPE":
                            IEnvelope envelope = new Envelope(
                                Convert.ToDouble(node.Attributes["minx"].Value.Replace(".", ",")),
                                Convert.ToDouble(node.Attributes["miny"].Value.Replace(".", ",")),
                                Convert.ToDouble(node.Attributes["maxx"].Value.Replace(".", ",")),
                                Convert.ToDouble(node.Attributes["maxy"].Value.Replace(".", ",")));
                            AddSrs(node, envelope);
                            aGeom.AddGeometry(envelope);
                            break;
                        case "POLYLINE":
                            IPolyline pline = new Polyline();
                            AddSrs(node, pline);
                            foreach (XmlNode pathnode in node.SelectNodes("PATH"))
                            {
                                IPath path = new gView.Framework.Geometry.Path();
                                foreach (XmlNode pointnode in pathnode.SelectNodes("POINT"))
                                {
                                    point = PointFromAxl(pointnode);
                                    path.AddPoint(point);
                                }
                                foreach (XmlNode coordnode in pathnode.SelectNodes("COORDS"))
                                {
                                    PointsFromCoords(path, coordnode);
                                }
                                pline.AddPath(path);
                            }
                            aGeom.AddGeometry(pline);
                            break;
                        case "POLYGON":
                            IPolygon pgon = new Polygon();
                            AddSrs(node, pgon);
                            foreach (XmlNode ringnode in node.SelectNodes("RING"))
                            {
                                IRing ring = new gView.Framework.Geometry.Ring();
                                foreach (XmlNode pointnode in ringnode.SelectNodes("POINT"))
                                {
                                    point = PointFromAxl(pointnode);
                                    ring.AddPoint(point);
                                }
                                foreach (XmlNode coordnode in ringnode.SelectNodes("COORDS"))
                                {
                                    PointsFromCoords(ring, coordnode);
                                }
                                pgon.AddRing(ring);
                                foreach (XmlNode holenode in ringnode.SelectNodes("HOLE"))
                                {
                                    IRing hole = new gView.Framework.Geometry.Ring();
                                    foreach (XmlNode pointnode in holenode.SelectNodes("POINT"))
                                    {
                                        point = PointFromAxl(pointnode);
                                        hole.AddPoint(point);
                                    }
                                    foreach (XmlNode coordnode in holenode.SelectNodes("COORDS"))
                                    {
                                        PointsFromCoords(hole, coordnode);
                                    }
                                    pgon.AddRing(hole);
                                }
                            }
                            foreach (XmlNode ringnode in node.SelectNodes("HOLE"))
                            {
                                IRing ring = new gView.Framework.Geometry.Ring();
                                foreach (XmlNode pointnode in ringnode.SelectNodes("POINT"))
                                {
                                    point = PointFromAxl(pointnode);
                                    ring.AddPoint(point);
                                }
                                foreach (XmlNode coordnode in ringnode.SelectNodes("COORDS"))
                                {
                                    PointsFromCoords(ring, coordnode);
                                }
                                pgon.AddRing(ring);
                            }
                            aGeom.AddGeometry(pgon);
                            break;
                    }
                }

                if (aGeom.GeometryCount == 0)
                    return null;

                if (!ignoreBuffer &&
                    bufferNode != null && bufferNode.Attributes["distance"] != null)
                {
                    try
                    {
                        double dist = Convert.ToDouble(bufferNode.Attributes["distance"].Value);
                        IPolygon polygon = aGeom.Buffer(dist);
                        return polygon;
                    }
                    catch { }
                }

                if (aGeom.GeometryCount == 1)
                    return aGeom[0];

                return aGeom;
            }
            catch
            {
                return null;
            }
        }

        static private string SrsAttribute(IGeometry geom)
        {
            if (geom == null || geom.Srs == null || geom.Srs <= 0)
                return String.Empty;

            return " srs=\"" + geom.Srs.ToString() + "\" ";
        }
        static public string Geometry2AXL(IGeometry geom)
        {
            if (geom == null) return "";
            StringBuilder axl = new StringBuilder();

            if (geom is IEnvelope)
            {
                IEnvelope env = (IEnvelope)geom;
                axl.Append("<ENVELOPE ");
                axl.Append("minx=\"" + env.minx.ToString() + "\" ");
                axl.Append("miny=\"" + env.miny.ToString() + "\" ");
                axl.Append("maxx=\"" + env.maxx.ToString() + "\" ");
                axl.Append("maxy=\"" + env.maxy.ToString() + "\" ");
                axl.Append(SrsAttribute(geom));
                axl.Append("/>");
            }
            else if (geom is IPoint)
            {
                IPoint point = (IPoint)geom;
                axl.Append("<POINT x=\"" + point.X.ToString() + "\" y=\"" + point.Y + "\"" + SrsAttribute(geom) + "/>");
            }
            else if (geom is IPolyline)
            {
                IPolyline line = (IPolyline)geom;
                axl.Append("<POLYLINE" + SrsAttribute(geom) + ">");
                for (int i = 0; i < line.PathCount; i++)
                {
                    IPath path = line[i];
                    axl.Append("<PATH>");
                    for (int p = 0; p < path.PointCount; p++)
                    {
                        IPoint point = path[p];
                        axl.Append("<POINT x=\"" + point.X.ToString() + "\" y=\"" + point.Y + "\" />");
                    }
                    axl.Append("</PATH>");
                }
                axl.Append("</POLYLINE>");
            }
            else if (geom is IPolygon)
            {
                IPolygon poly = (IPolygon)geom;
                axl.Append("<POLYGON" + SrsAttribute(geom) + ">");
                for (int i = 0; i < poly.RingCount; i++)
                {
                    IRing ring = poly[i];
                    axl.Append("<RING>");
                    for (int p = 0; p < ring.PointCount; p++)
                    {
                        IPoint point = ring[p];
                        axl.Append("<POINT x=\"" + point.X.ToString() + "\" y=\"" + point.Y + "\" />");
                    }
                    // Ringe im AXL immer schließen...
                    if (ring.PointCount > 3 && !ring[0].Equals(ring[ring.PointCount - 1], 0.0))
                    {
                        axl.Append("<POINT x=\"" + ring[0].X.ToString() + "\" y=\"" + ring[0].Y + "\" />");
                    }
                    axl.Append("</RING>");
                }
                axl.Append("</POLYGON>");
            }
            else if (geom is IMultiPoint)
            {
                IMultiPoint mPoint = (IMultiPoint)geom;
                axl.Append("<MULTIPOINT" + SrsAttribute(geom) + ">");
                for (int i = 0; i < mPoint.PointCount; i++)
                {
                    IPoint point = mPoint[i];
                    axl.Append("<POINT x=\"" + point.X.ToString() + "\" y=\"" + point.Y + "\" />");
                }
                axl.Append("</MULTIPOINT>");
            }
            else if (geom is IAggregateGeometry)
            {
                for (int i = 0; i < ((IAggregateGeometry)geom).GeometryCount; i++)
                {
                    axl.Append(Geometry2AXL(((IAggregateGeometry)geom)[i]));
                }
            }
            return axl.ToString();
        }

        private static IPoint PointFromAxl(XmlNode node)
        {
            IPoint point = null;

            if (node.Attributes["x"] != null && node.Attributes["y"] != null)
            {
                point = new Point(
                    double.Parse(node.Attributes["x"].Value.Replace(",", "."), NumberStyles.Any, _nhi),
                    double.Parse(node.Attributes["y"].Value.Replace(",", "."), NumberStyles.Any, _nhi));
            }
            else if (node.Attributes["coords"] != null)
            {
                string[] xy = RemoveDoubleSpaces(node.Attributes["coords"].Value).Split(' ');
                if (xy.Length == 2)
                {
                    double x, y;
                    if (double.TryParse(xy[0].Replace(",", "."), NumberStyles.Any, _nhi, out x) &&
                        double.TryParse(xy[1].Replace(",", "."), NumberStyles.Any, _nhi, out y))
                    {
                        point = new Point(x, y);
                    }
                }
            }

            return point;
        }
        private static void PointsFromCoords(IPointCollection pColl, XmlNode node)
        {
            if (pColl == null || node == null) return;

            string[] coords = node.InnerText.Split(';');
            foreach (string coord in coords)
            {
                string[] xy = RemoveDoubleSpaces(coord).Split(' ');
                if (xy.Length == 2)
                {
                    double x, y;
                    if (double.TryParse(xy[0].Replace(",", "."), NumberStyles.Any, _nhi, out x) &&
                        double.TryParse(xy[1].Replace(",", "."), NumberStyles.Any, _nhi, out y))
                    {
                        pColl.AddPoint(new Point(x, y));
                    }
                }
            }
        }
        private static string RemoveDoubleSpaces(string str)
        {
            while (str.Contains("  "))
                str = str.Replace("  ", " ");

            return str;
        }

        static internal string shortName(string fieldname)
        {
            int pos = 0;
            string[] fieldnames = fieldname.Split(';');
            fieldname = "";
            for (int i = 0; i < fieldnames.Length; i++)
            {
                while ((pos = fieldnames[i].IndexOf(".")) != -1)
                {
                    fieldnames[i] = fieldnames[i].Substring(pos + 1, fieldnames[i].Length - pos - 1);
                }
                if (fieldname != "") fieldname += ";";
                fieldname += fieldnames[i];
            }

            return fieldname;
        }

        static public ISpatialReference AXL2SpatialReference(string coordSysNode)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(coordSysNode);

                return AXL2SpatialReference(doc.ChildNodes[0]);
            }
            catch
            {
            }
            return null;
        }
        static public ISpatialReference AXL2SpatialReference(XmlNode coordSysNode)
        {
            ISpatialReference sRef = null;
            if (coordSysNode != null)
            {
                try
                {
                    if (coordSysNode.Attributes["id"] != null && coordSysNode.Attributes["id"].Value != "0")
                    {
                        sRef = gView.Framework.Geometry.SpatialReference.FromID("epsg:" + coordSysNode.Attributes["id"].Value);
                    }
                    else if (coordSysNode.Attributes["string"] != null)
                    {
                        sRef = gView.Framework.Geometry.SpatialReference.FromWKT(coordSysNode.Attributes["string"].Value);
                    }

                    if (sRef != null && coordSysNode.Attributes["datumtransformid"] != null)
                    {
                        GeodeticDatum datum = Const.FromID(coordSysNode.Attributes["datumtransformid"].Value);
                        if (datum != null) sRef.Datum = datum;
                    }
                    else if (sRef != null && coordSysNode.Attributes["datumid"] != null)
                    {
                        GeodeticDatum datum = Const.FromID(coordSysNode.Attributes["datumid"].Value);
                        if (datum != null) sRef.Datum = datum;
                    }
                    else if (sRef != null && coordSysNode.Attributes["datumtransformstring"] != null)
                    {
                        GeodeticDatum datum = GeodeticDatum.FromESRIWKT(coordSysNode.Attributes["datumtransformstring"].Value);
                        if (datum != null) sRef.Datum = datum;
                    }
                    else if (sRef != null && coordSysNode.Attributes["datumstring"] != null)
                    {
                        GeodeticDatum datum = GeodeticDatum.FromESRIWKT(coordSysNode.Attributes["datumstring"].Value);
                        if (datum != null) sRef.Datum = datum;
                    }
                }
                catch { }
            }
            return sRef;
        }
    }

    public class CBF
    {
        static public string CBF2AXL(string cbf)
        {
            // Test Header
            if (cbf.Length < 4 ||
                (cbf[0] != 0x05 || cbf[1] != 0x18 || cbf[2] != 0x01 || cbf[3] != 0x0)) return cbf;

            FileStream fs = new FileStream(@"C:\binaryStream.hex", FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);

            MemoryStream ms = new MemoryStream();
            for (int i = 0; i < cbf.Length; i++)
            {
                ms.WriteByte((byte)cbf[i]);
                bw.Write((byte)cbf[i]);
            }
            ms.Position = 0;

            BinaryReader br = new BinaryReader(ms);

            Header header = new Header();
            header.Read(br);

            List<AttributeFieldDescription> fields = new List<AttributeFieldDescription>();
            while (true)
            {
                AttributeFieldDescription field = new AttributeFieldDescription();
                if (!field.Read(br) || field.name == String.Empty)
                    break;

                fields.Add(field);
            }

            Shape shape = new Shape();
            shape.Read(br);
            return String.Empty;
        }

        #region Helper
        static private uint SwapWord(uint word)
        {
            uint b1 = word & 0x000000ff;
            uint b2 = (word & 0x0000ff00) >> 8;
            uint b3 = (word & 0x00ff0000) >> 16;
            uint b4 = (word & 0xff000000) >> 24;

            return (b1 << 24) + (b2 << 16) + (b3 << 8) + b4;
        }
        static private short SwapShort(short s)
        {
            int b1 = (int)s & 0x00ff;
            int b2 = ((int)s & 0xff00) >> 8;

            return (short)((b1 << 8) + b2);
        }
        #endregion

        #region HelperClasses
        private class Header
        {
            public uint hid;
            public short s1, s2;
            public byte b1, b2, b3, b4;

            public void Read(BinaryReader br)
            {
                hid = (uint)br.ReadInt32();
                s1 = CBF.SwapShort(br.ReadInt16());
                s2 = CBF.SwapShort(br.ReadInt16());

                br.BaseStream.Position = 4;
                b1 = br.ReadByte();
                b2 = br.ReadByte();
                b3 = br.ReadByte();
                b4 = br.ReadByte();
            }
        }
        private class AttributeFieldDescription
        {
            public short type, size;
            public byte precision;
            public string name;
            byte[] footer = { 0x08, 0x43, 0x41, 0xc3, 0x79, 0x37, 0xe0, 0x80, 0x00 };

            public bool Read(BinaryReader br)
            {
                long position = br.BaseStream.Position;
                bool isfooter = true;
                for (int i = 0; i < 8; i++)
                {
                    if (!br.ReadByte().Equals(footer[i]))
                    {
                        isfooter = false;
                        break;
                    }
                }
                if (isfooter) return false;
                br.BaseStream.Position = position;

                type = CBF.SwapShort(br.ReadInt16());
                precision = br.ReadByte();
                if (type != -99 && type != -98 && type != 0)
                    size = CBF.SwapShort(br.ReadInt16());

                name = String.Empty;
                byte b;
                while ((b = br.ReadByte()) != 0)
                {
                    name += (char)b;
                }
                return true;
            }
        }
        private class Shape
        {
            public byte type;
            public int parts;
            public IGeometry geometry = null;

            public void Read(BinaryReader br)
            {
                type = br.ReadByte();
                parts = (int)CBF.SwapWord(br.ReadUInt32());

                switch (type)
                {
                    case 1:
                        break;
                    case 2:
                        ReadMultiLine(br);
                        break;
                    case 3:
                        break;
                }
            }
            private void ReadMultiLine(BinaryReader br)
            {
                geometry = new Polyline();
                for (int i = 0; i < parts; i++)
                {
                    gView.Framework.Geometry.Path path = new gView.Framework.Geometry.Path();

                    int vertices = (int)CBF.SwapWord(br.ReadUInt32());
                    double x = 0.0, y = 0.0;
                    for (int v = 0; v < vertices; v++)
                    {
                        x += (double)br.ReadDecimal();
                        y += (double)br.ReadDecimal();

                        path.AddPoint(new Point(x, y));
                    }
                    ((Polyline)geometry).AddPath(path);
                }
            }
        }
        #endregion
    }
}
