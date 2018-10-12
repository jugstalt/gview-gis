using gView.Framework.Geometry;
using gView.Framework.system;
using gView.Interoperability.ArcGisServer.Rest.Json.Features.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Request
{
    static class Extensions
    {
        static public int[] ToSize(this string val)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(val) || !val.Contains(","))
                    return new int[] { 400, 400 };

                var size = val.Split(',');
                return new int[] { int.Parse(size[0]), int.Parse(size[1]) };
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error parsing size parameter: " + val, ex);
            }
        }

        static public double[] ToBBox(this string val)
        {
            try
            {
                var bbox = val.Split(',');
                return new double[] {
                    NumberConverter.ToDouble(bbox[0]),
                    NumberConverter.ToDouble(bbox[1]),
                    NumberConverter.ToDouble(bbox[2]),
                    NumberConverter.ToDouble(bbox[3])
                };
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error parsing bbox parameter: " + val, ex);
            }
        }

        #region Geometry

        static public JsonGeometry ToJsonGeometry(this IGeometry shape)
        {
            if (shape is IPoint)
            {
                var point = (IPoint)shape;

                var jsonPoint = new JsonGeometry()
                {
                    X = ((Point)shape).X,
                    Y = ((IPoint)shape).Y
                };

                return jsonPoint;
            }
            if (shape is IPolyline)
            {
                var polyline = (IPolyline)shape;

                List<double[,]> rings = new List<double[,]>();
                for (int r = 0, ringCount = polyline.PathCount; r < ringCount; r++)
                {
                    var path = polyline[r];
                    if (path == null || path.PointCount == 0)
                        continue;

                    double[,] points = new double[path.PointCount, 2];
                    for (int p = 0, pointCount = path.PointCount; p < pointCount; p++)
                    {
                        var point = path[p];
                        points[p, 0] = point.X;
                        points[p, 1] = point.Y;
                    }
                    rings.Add(points);
                }

                var jsonPolyline = new JsonGeometry()
                {
                    Rings = rings.ToArray()
                };

                return jsonPolyline;
            }
            if (shape is IPolygon)
            {
                var polygon = (IPolygon)shape;

                List<double[,]> rings = new List<double[,]>();
                for (int r = 0, ringCount = polygon.RingCount; r < ringCount; r++)
                {
                    var ring = polygon[r];
                    if (ring == null || ring.PointCount == 0)
                        continue;

                    double[,] points = new double[ring.PointCount, 2];
                    for (int p = 0, pointCount = ring.PointCount; p < pointCount; p++)
                    {
                        var point = ring[p];
                        points[p, 0] = point.X;
                        points[p, 1] = point.Y;
                    }
                    rings.Add(points);
                }

                var jsonPolylgon = new JsonGeometry()
                {
                    Rings = rings.ToArray()
                };

                return jsonPolylgon;
            }
            return null;
        }

        #endregion
    }
}
