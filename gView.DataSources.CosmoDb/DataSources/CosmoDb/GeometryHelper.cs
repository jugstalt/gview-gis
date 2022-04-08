using gView.Framework.Geometry;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace gView.DataSources.CosmoDb
{
    static public class GeometryHelper
    {
        static public Microsoft.Azure.Documents.Spatial.Geometry ToAzureGeometry(this IGeometry shape)
        {
            if (shape is IPoint)
            {
                var point = (IPoint)shape;
                return new Microsoft.Azure.Documents.Spatial.Point(point.X, point.Y);
            }
            if (shape is IMultiPoint)
            {
                var mPoint = (IMultiPoint)shape;
                if (mPoint.PointCount != 1)
                {
                    throw new Exception("Can't convert Multipoints to Azure GeoJson");
                }

                return new Microsoft.Azure.Documents.Spatial.Point(mPoint[0].X, mPoint[1].Y);
            }
            if (shape is IPolyline)
            {
                var polyline = (IPolyline)shape;
                if (polyline.PathCount != 1)
                {
                    throw new Exception($"Can't convert Polylines with {polyline.PathCount} to Azure GeoJson");
                }

                List<Microsoft.Azure.Documents.Spatial.Position> positions = new List<Microsoft.Azure.Documents.Spatial.Position>();
                for (int p = 0, p_to = polyline[0].PointCount; p < p_to; p++)
                {
                    positions.Add(new Microsoft.Azure.Documents.Spatial.Position(polyline[0][p].X, polyline[0][p].Y));
                }

                var azureLinestring = new Microsoft.Azure.Documents.Spatial.LineString(positions);
                return azureLinestring;
            }
            if (shape is IPolygon)
            {
                var polygon = (IPolygon)shape;

                List<Microsoft.Azure.Documents.Spatial.LinearRing> azureRings = new List<Microsoft.Azure.Documents.Spatial.LinearRing>();
                for (int r = 0, r_to = polygon.RingCount; r < r_to; r++)
                {
                    var ring = polygon[r];
                    ring.Close();

                    List<Microsoft.Azure.Documents.Spatial.Position> positions = new List<Microsoft.Azure.Documents.Spatial.Position>();
                    for (int p = 0, p_to = ring.PointCount; p < p_to; p++)
                    {
                        positions.Add(new Microsoft.Azure.Documents.Spatial.Position(ring[p].X, ring[p].Y));
                    }

                    azureRings.Add(new Microsoft.Azure.Documents.Spatial.LinearRing(positions));
                }

                var azurePolygon = new Microsoft.Azure.Documents.Spatial.Polygon(azureRings);
                return azurePolygon;
            }

            return null;
        }

        static public IGeometry ToGeometry(this JObject jObject)
        {
            if (jObject != null)
            {
                var type = jObject["type"].Value<string>()?.ToLower();
                if (type == "point")
                {
                    var coordinates = (JArray)jObject["coordinates"];
                    return new Point(
                        coordinates[0].Value<double>(),
                        coordinates[1].Value<double>());
                }
                if (type == "polygon")
                {
                    var polygon = new Polygon();

                    var jsonRings = (JArray)jObject["coordinates"];
                    foreach (JArray jsonRing in jsonRings)
                    {
                        var ring = new Ring();
                        foreach (JArray jsonPoint in jsonRing)
                        {
                            ring.AddPoint(new Point(
                                jsonPoint[0].Value<double>(),
                                jsonPoint[1].Value<double>()));
                        }
                        if (ring.PointCount > 0)
                        {
                            polygon.AddRing(ring);
                        }
                    }

                    return polygon;
                }
            }
            return null;
        }
    }
}
