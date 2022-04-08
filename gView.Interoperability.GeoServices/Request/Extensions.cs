using gView.Framework.Geometry;
using gView.Framework.system;
using gView.Interoperability.GeoServices.Rest.Json.Features.Geometry;
using gView.Interoperability.GeoServices.Rest.Json.FeatureServer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Interoperability.GeoServices.Request
{
    static class Extensions
    {
        static public int[] ToSize(this string val)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(val) || !val.Contains(","))
                {
                    return new int[] { 400, 400 };
                }

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
            if (shape is IMultiPoint)
            {
                var multiPoint = (IMultiPoint)shape;

                List<double?[]> points = new List<double?[]>();

                for (int p = 0, pointCount = multiPoint.PointCount; p < pointCount; p++)
                {
                    var point = multiPoint[p];
                    if (point != null)
                    {
                        points.Add(new double?[] { point.X, point.Y });
                    }
                }

                var jsonMultipoint = new JsonGeometry()
                {
                    Points = points.ToArray()
                };

                return jsonMultipoint;
            }
            if (shape is IEnvelope)
            {
                return new JsonGeometry()
                {
                    XMin = ((IEnvelope)shape).minx,
                    YMin = ((IEnvelope)shape).miny,
                    XMax = ((IEnvelope)shape).maxx,
                    YMax = ((IEnvelope)shape).maxy,
                };
            }
            if (shape is IPolyline)
            {
                var polyline = (IPolyline)shape;

                List<double?[,]> paths = new List<double?[,]>();
                for (int r = 0, pathCount = polyline.PathCount; r < pathCount; r++)
                {
                    var path = polyline[r];
                    if (path == null || path.PointCount == 0)
                    {
                        continue;
                    }

                    double?[,] points = new double?[path.PointCount, 2];
                    for (int p = 0, pointCount = path.PointCount; p < pointCount; p++)
                    {
                        var point = path[p];
                        points[p, 0] = point.X;
                        points[p, 1] = point.Y;
                    }
                    paths.Add(points);
                }

                var jsonPolyline = new JsonGeometry()
                {
                    Paths = paths.ToArray()
                };

                return jsonPolyline;
            }
            if (shape is IPolygon)
            {
                var polygon = (IPolygon)shape;

                List<double?[,]> rings = new List<double?[,]>();
                for (int r = 0, ringCount = polygon.RingCount; r < ringCount; r++)
                {
                    var ring = polygon[r];
                    if (ring == null || ring.PointCount == 0)
                    {
                        continue;
                    }

                    double?[,] points = new double?[ring.PointCount, 2];
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

        static public IGeometry ToGeometry(this JsonGeometry geometry)
        {
            if (geometry == null)
            {
                return null;
            }

            IGeometry shape = null;

            if (geometry.X.HasValue && geometry.Y.HasValue)
            {
                shape = new Point(geometry.X.Value, geometry.Y.Value);
            }
            else if (geometry.XMin.HasValue && geometry.YMin.HasValue && geometry.XMax.HasValue && geometry.YMax.HasValue)
            {
                shape = new Envelope(geometry.XMin.Value, geometry.YMin.Value, geometry.XMax.Value, geometry.YMax.Value);
            }
            else if (geometry.Paths != null && geometry.Paths.Length > 0)
            {
                var polyline = new Polyline();

                for (int p = 0, pathCount = geometry.Paths.Length; p < pathCount; p++)
                {
                    var jsonPath = geometry.Paths[p];
                    if (jsonPath.Length < 1)
                    {
                        continue;
                    }

                    var path = new Path();
                    for (int i = 0, pointCount = jsonPath.GetLength(0); i < pointCount; i++)
                    {
                        path.AddPoint(new Point(jsonPath[i, 0].Value, jsonPath[i, 1].Value));
                    }
                    polyline.AddPath(path);
                }

                shape = polyline;
            }
            else if (geometry.Rings != null && geometry.Rings.Length > 0)
            {
                var polygon = new Polygon();

                for (int p = 0, ringCount = geometry.Rings.Length; p < ringCount; p++)
                {
                    var jsonRing = geometry.Rings[p];
                    if (jsonRing.Length < 1)
                    {
                        continue;
                    }

                    var ring = new Ring();
                    for (int i = 0, pointCount = jsonRing.GetLength(0); i < pointCount; i++)
                    {
                        ring.AddPoint(new Point(jsonRing[i, 0].Value, jsonRing[i, 1].Value));
                    }
                    polygon.AddRing(ring);
                }

                shape = polygon;
            }
            else if (geometry.Points != null && geometry.Points.Length > 0)
            {
                var multiPoint = new MultiPoint();

                for (int p = 0, pointCount = geometry.Points.Length; p < pointCount; p++)
                {
                    var point = geometry.Points[p];
                    if (point != null && point.Length >= 2)
                    {
                        multiPoint.AddPoint(new Point(point[0].Value, point[1].Value));
                    }
                }

                shape = multiPoint;
            }

            if (shape != null && geometry.SpatialReference != null && geometry.SpatialReference.Wkid > 0)
            {
                shape.Srs = geometry.SpatialReference.Wkid;
            }

            return shape;
        }

        #endregion

        #region EditResponse

        static public IEnumerable<JsonFeatureServerResponse.JsonResponse> ToEditJsonResponse(this IEnumerable<int> objectIds, bool succeeded)
        {
            return objectIds.Select(objectId => new JsonFeatureServerResponse.JsonResponse()
            {
                Success = succeeded,
                ObjectId = objectId
            });
        }

        #endregion
    }
}
