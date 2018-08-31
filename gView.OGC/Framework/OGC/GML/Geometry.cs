using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using gView.Framework.Geometry;

namespace gView.Framework.OGC.GML
{
    public class GeometryTranslator
    {
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        #region Geometry from GML
        public static IGeometry GML2Geometry(string gml, GmlVersion gmlVersion)
        {
            try
            {
                gml = gml.Replace("<gml:", "<").Replace("</gml:", "</");
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(gml);

                XmlNode geomNode = doc.ChildNodes[0];

                ISpatialReference sRef = null;
                if ((int)gmlVersion > 2 && geomNode.Attributes["srsName"] != null)
                    sRef = SpatialReference.FromID(geomNode.Attributes["srsName"].Value);

                switch (geomNode.Name)
                {
                    case "Point":
                        return GML2Point(geomNode, sRef);
                    case "pointProperty":
                        return GML2Point(geomNode.SelectSingleNode("Point"), sRef);
                    case "Box":
                    case "Envelope":
                        return GML2Envelope(geomNode, sRef);
                    case "LineString":
                        IPath path = GML2Path(geomNode, sRef);
                        if (path == null) return null;
                        Polyline polyline = new Polyline();
                        polyline.AddPath(path);
                        return polyline;
                    case "MultiLineString":
                        return GML2Polyline(geomNode, sRef);
                    case "curveProperty":
                        return GML2Polyline(geomNode, sRef);
                    case "Polygon":
                        return GML2Polygon(geomNode, sRef);
                    case "MultiPolygon":
                        AggregateGeometry aGeom1 = new AggregateGeometry();
                        foreach (XmlNode polygonNode in geomNode.SelectNodes("polygonMember/Polygon"))
                        {
                            IPolygon polygon = GML2Polygon(polygonNode, sRef);
                            if (polygon != null) aGeom1.AddGeometry(polygon);
                        }
                        if (aGeom1.GeometryCount == 0) return null;
                        IPolygon mpolygon1 = (IPolygon)aGeom1[0];
                        for (int i = 1; i < aGeom1.GeometryCount; i++)
                        {
                            IPolygon p = (IPolygon)aGeom1[i];
                            for (int r = 0; r < p.RingCount; r++)
                                mpolygon1.AddRing(p[r]);
                        }
                        return mpolygon1;
                    case "surfaceProperty":
                        AggregateGeometry aGeom2 = new AggregateGeometry();
                        foreach (XmlNode polygonNode in geomNode.SelectNodes("Surface/patches/PolygonPatch"))
                        {
                            IPolygon polygon = GML2Polygon(polygonNode, sRef);
                            if (polygon != null) aGeom2.AddGeometry(polygon);
                        }
                        if (aGeom2.GeometryCount == 0) return null;
                        IPolygon mpolygon2 = (IPolygon)aGeom2[0];
                        for (int i = 1; i < aGeom2.GeometryCount; i++)
                        {
                            IPolygon p = (IPolygon)aGeom2[i];
                            for (int r = 0; r < p.RingCount; r++)
                                mpolygon2.AddRing(p[r]);
                        }
                        return mpolygon2;
                    default:
                        return null;
                }
                return null;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return null;
            }
        }

        private static IPoint GML2Point(XmlNode pointNode, ISpatialReference sRef)
        {
            if (pointNode == null) return null;

            XmlNode coordinatesNode = pointNode.SelectSingleNode("coordinates");
            if (coordinatesNode != null)
            {

                string[] xy = coordinatesNode.InnerText.Split(',');
                if (xy.Length < 2) return null;

                IPoint p = Gml3Point(new Point(
                    double.Parse(xy[0], _nhi),
                    double.Parse(xy[1], _nhi)), sRef);

                return p;
            }
            XmlNode posNode = pointNode.SelectSingleNode("pos");
            if (posNode != null)
            {
                string[] xy = posNode.InnerText.Split(' ');
                if (xy.Length < 2) return null;

                IPoint p = Gml3Point(new Point(
                    double.Parse(xy[0], _nhi),
                    double.Parse(xy[1], _nhi)), sRef);

                return p;
            }
            XmlNode coordNode = pointNode.SelectSingleNode("coord");
            if (coordNode != null)
            {
                XmlNode x = coordNode.SelectSingleNode("X");
                XmlNode y = coordNode.SelectSingleNode("Y");
                if (x != null && y != null)
                    return Gml3Point(new Point(double.Parse(x.InnerText, _nhi), double.Parse(y.InnerText, _nhi)), sRef);
            }
            return null;
        }

        private static void CoordinatesToPointCollection(XmlNode coordinates, IPointCollection pColl, ISpatialReference sRef)
        {
            CoordinatesToPointCollection(coordinates, pColl, ' ', ',', sRef);
        }
        private static void CoordinatesToPointCollection(XmlNode coordinates, IPointCollection pColl, char pointSplitter, char coordSplitter, ISpatialReference sRef)
        {
            if (coordinates == null) return;

            string[] coords = coordinates.InnerText.Split(pointSplitter);

            foreach (string coord in coords)
            {
                string[] xy = coord.Split(coordSplitter);
                if (xy.Length < 2) return;

                pColl.AddPoint(Gml3Point(new Point(
                    double.Parse(xy[0], _nhi),
                    double.Parse(xy[1], _nhi)), sRef));
            }
        }

        private static void PosListToPointCollection(XmlNode posList, IPointCollection pColl, ISpatialReference sRef)
        {
            PosListToPointCollection(posList, pColl, ' ', sRef);
        }
        private static void PosListToPointCollection(XmlNode posList, IPointCollection pColl, char splitter, ISpatialReference sRef)
        {
            if (posList == null) return;

            string[] coords = posList.InnerText.Split(splitter);

            if (coords.Length % 2 != 0) return;
            for (int i = 0; i < coords.Length - 1; i += 2)
            {
                pColl.AddPoint(Gml3Point(new Point(
                    double.Parse(coords[i], _nhi),
                    double.Parse(coords[i + 1], _nhi)), sRef));
            }
        }

        private static IPath GML2Path(XmlNode lineStringNode, ISpatialReference sRef)
        {
            if (lineStringNode == null) return null;

            XmlNode coordNode = lineStringNode.SelectSingleNode("coordinates");
            if (coordNode != null)
            {
                Path path = new Path();
                CoordinatesToPointCollection(coordNode, path, sRef);
                return path;
            }
            XmlNode posNode = lineStringNode.SelectSingleNode("posList");
            if (posNode != null)
            {
                Path path = new Path();
                PosListToPointCollection(posNode, path, sRef);
                return path;
            }
            return null;
        }

        private static IPolyline GML2Polyline(XmlNode multiLineStringNode, ISpatialReference sRef)
        {
            if (multiLineStringNode == null) return null;

            Polyline polyline = new Polyline();
            if (multiLineStringNode.Name == "curveProperty")
            {
                foreach (XmlNode lineStringNode in multiLineStringNode.SelectNodes("LineString"))
                {
                    IPath path = GML2Path(lineStringNode, sRef);
                    if (path != null)
                        polyline.AddPath(path);
                }
            }
            else
            {
                foreach (XmlNode lineStringNode in multiLineStringNode.SelectNodes("lineStringMember/LineString"))
                {
                    IPath path = GML2Path(lineStringNode, sRef);
                    if (path != null)
                        polyline.AddPath(path);
                }
            }
            return polyline;
        }

        private static IPolygon GML2Polygon(XmlNode polygonNode, ISpatialReference sRef)
        {
            if (polygonNode == null) return null;

            Polygon polygon = new Polygon();
            if (polygonNode.Name == "Polygon")
            {
                foreach (XmlNode coordintes in polygonNode.SelectNodes("outerBoundaryIs/LinearRing/coordinates"))
                {
                    Ring ring = new Ring();
                    CoordinatesToPointCollection(coordintes, ring, sRef);
                    if (ring.PointCount > 0)
                        polygon.AddRing(ring);
                }
                foreach (XmlNode coordintes in polygonNode.SelectNodes("innerBoundaryIs/LinearRing/coordinates"))
                {
                    Hole hole = new Hole();
                    CoordinatesToPointCollection(coordintes, hole, sRef);
                    if (hole.PointCount > 0)
                        polygon.AddRing(hole);
                }
            }
            else if (polygonNode.Name == "PolygonPatch")
            {
                foreach (XmlNode coordintes in polygonNode.SelectNodes("exterior/LinearRing/posList"))
                {
                    Ring ring = new Ring();
                    PosListToPointCollection(coordintes, ring, sRef);
                    if (ring.PointCount > 0)
                        polygon.AddRing(ring);
                }
                foreach (XmlNode coordintes in polygonNode.SelectNodes("interior/LinearRing/posList"))
                {
                    Hole hole = new Hole();
                    PosListToPointCollection(coordintes, hole, sRef);
                    if (hole.PointCount > 0)
                        polygon.AddRing(hole);
                }
            }
            return polygon;
        }

        private static IEnvelope GML2Envelope(XmlNode envNode, ISpatialReference sRef)
        {
            if (envNode == null) return null;

            PointCollection pColl = new PointCollection();

            XmlNode coordinates = envNode.SelectSingleNode("coordinates");
            if (coordinates != null)
            {
                //<gml:Box srsName="epsg:31467">
                //<gml:coordinates>3517721.548714,5522386.484305 3548757.845502,5556110.794487</gml:coordinates>
                //</gml:Box>

                CoordinatesToPointCollection(coordinates, pColl, sRef);
            }
            else
            {
                //<gml:Envelope srsName="_FME_0" srsDimension="2">
                //<gml:lowerCorner>-58461.5054920442 209579.200729251</gml:lowerCorner>
                //<gml:upperCorner>-17656.9734282536 265744.953950753</gml:upperCorner>
                //</gml:Envelope>

                XmlNode lowerCorner = envNode.SelectSingleNode("lowerCorner");
                XmlNode upperCorner = envNode.SelectSingleNode("upperCorner");

                CoordinatesToPointCollection(lowerCorner, pColl, '|', ' ', sRef);  // | dummy Seperator
                CoordinatesToPointCollection(upperCorner, pColl, '|', ' ', sRef);
            }

            if (pColl.PointCount == 2)
            {
                return new Envelope(pColl[0].X, pColl[0].Y, pColl[1].X, pColl[1].Y);
            }
            else
            {
                return null;
            }
        }

        public static IPoint Gml3Point(IPoint p, ISpatialReference sRef)
        {
            if (sRef == null)
                return p;

            double X = p.X, Y = p.Y;
            switch (sRef.Gml3AxisX)
            {
                case AxisDirection.North:
                    p.X = Y;
                    break;
                case AxisDirection.South:
                    p.X = -Y;
                    break;
                case AxisDirection.West:
                    p.X = -X;
                    break;
                case AxisDirection.East:
                    p.X = X;
                    break;
            }
            switch (sRef.Gml3AxisY)
            {
                case AxisDirection.North:
                    p.Y = Y;
                    break;
                case AxisDirection.South:
                    p.Y = -Y;
                    break;
                case AxisDirection.West:
                    p.Y = -X;
                    break;
                case AxisDirection.East:
                    p.Y = X;
                    break;
            }
            return p;
        }
        #endregion

        #region GML from Geometry
        public static string Geometry2GML(IGeometry geometry, string srsName, GmlVersion version)
        {
            ISpatialReference sRef = (int)version > 2 ? SpatialReference.FromID(srsName) : null;

            if (geometry is IEnvelope)
            {
                return Envelope2GML(geometry as IEnvelope, srsName, sRef);
            }
            else if (geometry is IPoint)
            {
                return Point2GML(geometry as IPoint, srsName, sRef);
            }
            else if (geometry is IPolyline)
            {
                return Polyline2GML(geometry as IPolyline, srsName, sRef);
            }
            else if (geometry is IPolygon)
            {
                return Polygon2GML(geometry as IPolygon, srsName, sRef);
            }
            else if (geometry is IAggregateGeometry)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < ((IAggregateGeometry)geometry).GeometryCount; i++)
                {
                    sb.Append(Geometry2GML(((IAggregateGeometry)geometry)[i], srsName, version));
                }
                return sb.ToString();
            }
            return "";
        }

        private static string CoordinateString(IPoint point, ISpatialReference sRef)
        {
            if (sRef != null)
            {
                StringBuilder sb = new StringBuilder();
                switch (sRef.Gml3AxisX)
                {
                    case AxisDirection.North:
                    case AxisDirection.South: sb.Append(point.Y.ToString(_nhi)); break;
                    case AxisDirection.East:
                    case AxisDirection.West: sb.Append(point.X.ToString(_nhi)); break;
                }
                sb.Append(",");
                switch (sRef.Gml3AxisY)
                {
                    case AxisDirection.North:
                    case AxisDirection.South: sb.Append(point.Y.ToString(_nhi)); break;
                    case AxisDirection.East:
                    case AxisDirection.West: sb.Append(point.X.ToString(_nhi)); break;
                }
                return sb.ToString();
            }
            return point.X.ToString(_nhi) + "," + point.Y.ToString(_nhi);
        }
        private static string CoordinatesString(IPointCollection pColl, ISpatialReference sRef)
        {
            if (pColl == null) return String.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append(@"<gml:coordinates>");
            for (int i = 0; i < pColl.PointCount; i++)
            {
                if (i != 0) sb.Append(" ");
                sb.Append(CoordinateString(pColl[i], sRef));
            }
            sb.Append(@"</gml:coordinates>");
            return sb.ToString();
        }
        private static string Envelope2GML(IEnvelope envelope, string srsName, ISpatialReference sRef)
        {
            StringBuilder sb = new StringBuilder();
            //sb.Append(@"<gml:Envelope srsName=""" + srsName + @""">");
            //sb.Append(@"<gml:lowerCorner>" +
            //    envelope.minx.ToString(_nhi) + " " + envelope.miny.ToString(_nhi) + "</gml:lowerCorner>");
            //sb.Append(@"<gml:upperCorner>" +
            //    envelope.minx.ToString(_nhi) + " " + envelope.miny.ToString(_nhi) + "</gml:upperCorner>");
            //sb.Append(@"</gml:Envelope>");
            sb.Append(@"<gml:Box srsName=""" + srsName + @""">");
            sb.Append(@"<gml:coordinates>");
            sb.Append(CoordinateString(new Point(envelope.minx, envelope.miny), sRef) + " ");
            sb.Append(CoordinateString(new Point(envelope.maxx, envelope.maxy), sRef));
            sb.Append(@"</gml:coordinates>");
            sb.Append(@"</gml:Box>");
            return sb.ToString();
        }

        private static string Point2GML(IPoint point, string srsName, ISpatialReference sRef)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<gml:Point srsName=""" + srsName + @""">");
            sb.Append(@"<gml:coordinates>");
            sb.Append(CoordinateString(point, sRef));
            sb.Append(@"</gml:coordinates>");
            sb.Append(@"</gml:Point>");
            return sb.ToString();
        }

        private static string Path2GML(IPath path, string srsName, ISpatialReference sRef)
        {
            if (path == null || path.PointCount == 0) return String.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append(@"<gml:LineString");
            if (srsName != String.Empty)
                sb.Append(@" srsName=""" + srsName + @""">");
            else
                sb.Append(">");

            sb.Append(CoordinatesString(path, sRef));

            sb.Append(@"</gml:LineString>");
            return sb.ToString();
        }
        private static string Polyline2GML(IPolyline polyline, string srsName, ISpatialReference sRef)
        {
            if (polyline == null) return String.Empty;
            StringBuilder sb = new StringBuilder();

            if (polyline.PathCount == 1)
            {
                sb.Append(Path2GML(polyline[0], srsName, sRef));
            }
            else
            {
                sb.Append(@"<gml:MultiLineString srsName=""" + srsName + @""">");
                for (int i = 0; i < polyline.PathCount; i++)
                    sb.Append(Path2GML(polyline[i], String.Empty, sRef));

                sb.Append(@"</gml:MultiLineString>");
            }
            return sb.ToString();
        }
        private static string Ring2GML(IRing ring, ISpatialReference sRef)
        {
            if (ring == null || ring.PointCount == 0) return String.Empty;

            StringBuilder sb = new StringBuilder();
            if (ring is IHole)
                sb.Append(@"<gml:innerBoundaryIs>");
            else
                sb.Append(@"<gml:outerBoundaryIs>");

            sb.Append(@"<gml:LinearRing>");

            sb.Append(CoordinatesString(ring, sRef));

            sb.Append(@"</gml:LinearRing>");
            if (ring is IHole)
                sb.Append(@"</gml:innerBoundaryIs>");
            else
                sb.Append(@"</gml:outerBoundaryIs>");

            return sb.ToString();
        }
        private static string Polygon2GML(IPolygon polygon, string srsName, ISpatialReference sRef)
        {
            if (polygon == null) return String.Empty;
            StringBuilder sb = new StringBuilder();

            sb.Append(@"<gml:Polygon srsName=""" + srsName + @""">");
            for (int i = 0; i < polygon.RingCount; i++)
                sb.Append(Ring2GML(polygon[i], sRef));

            sb.Append(@"</gml:Polygon>");
            return sb.ToString();
        }
        #endregion
    }
}
