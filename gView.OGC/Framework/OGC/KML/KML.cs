using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.Data;
using gView.Framework.Geometry;
using System.IO;
using System.Xml.Serialization;

namespace gView.Framework.OGC.KML
{
    public class KML
    {
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        static public Stream ToKml(IFeatureCursor cursor)
        {
            return ToKml(cursor, int.MaxValue);
        }
        static public Stream ToKml(IFeatureCursor cursor, int maxFeatures)
        {
            KmlType kml = new KmlType();

            DocumentType document=new DocumentType();
            kml.Item = document;

            int counter = 0;
            IFeature feature = null;
            List<PlacemarkType> placemarks = new List<PlacemarkType>();
            while ((feature = cursor.NextFeature) != null)
            {
                if (feature.Shape == null)
                    continue;

                PlacemarkType placemark = new PlacemarkType();
                placemark.Item_ = ToKml(feature.Shape);

                placemarks.Add(placemark);
                counter++;
                if (counter >= maxFeatures)
                    break;
            }

            document.Items_ = placemarks.ToArray();

            #region Serialize
            System.Xml.Serialization.XmlSerializerNamespaces namespaces = new System.Xml.Serialization.XmlSerializerNamespaces();

            //namespaces.Add("wps", "http://www.opengis.net/wps/1.0.0");
            //namespaces.Add("ows", "http://www.opengis.net/ows/1.1");
            //namespaces.Add("xlink", "http://www.w3.org/1999/xlink");
            //namespaces.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            MemoryStream ms = new MemoryStream();
            XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(kml.GetType());
            ser.Serialize(ms, kml, namespaces);
            ms.Position = 0;
            return ms;

            //byte[] bytes = new byte[ms.Length];
            //ms.Position = 0;
            //ms.Read(bytes, 0, (int)ms.Length);

            //return System.Text.Encoding.UTF8.GetString(bytes);
            #endregion
        }

        static public AbstractGeometryType ToKml(IGeometry geometry)
        {
            if (geometry is IPoint)
            {
                PointType point = new PointType();
                point.coordinates = ((IPoint)geometry).X.ToString(_nhi) + "," + ((IPoint)geometry).Y.ToString(_nhi);
                return point;
            }
            else if (geometry is IMultiPoint)
            {
                
            }
            else if (geometry is IPolyline)
            {
                IPolyline pLine = (IPolyline)geometry;
                if (pLine.PathCount == 1)
                {
                    LineStringType line = new LineStringType();
                    line.coordinates = ToKmlCoordinates(pLine[0]);
                    return line;
                }
                else if (pLine.PathCount > 1)
                {
                    MultiGeometryType mGeom = new MultiGeometryType();
                    mGeom.Items = new AbstractGeometryType[pLine.PathCount];
                    for (int i = 0, to = pLine.PathCount; i < to; i++)
                    {
                        LineStringType line = new LineStringType();
                        line.coordinates = ToKmlCoordinates(pLine[i]);
                        mGeom.Items[i] = line;
                    }
                }
            }
            else if (geometry is IPolygon)
            {
                //IPolygon poly = (IPolygon)geometry;
                //PolygonType polygon = new PolygonType();

                //polygon.o
            }
            return null;
        }

        static public string ToKmlCoordinates(IPointCollection points)
        {
            StringBuilder sb = new StringBuilder();

            for(int i=0,to=points.PointCount;i<to;i++)
            {
                IPoint point = points[i];
                if (sb.Length > 0) sb.Append(" ");
                sb.Append(point.X.ToString(_nhi));
                sb.Append(",");
                sb.Append(point.Y.ToString(_nhi));
            }
            return sb.ToString();
        }
    }
}
