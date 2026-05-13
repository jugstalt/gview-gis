using gView.Framework.Common;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Interoperability.GeoServices.Rest.DTOs.Features.Geometry;
using gView.Interoperability.GeoServices.Rest.DTOs.FeatureServer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Interoperability.GeoServices.Request.Extensions;

public static class Extensions
{
    static public int[] ToSize(this string val)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(val) || !val.Contains(","))
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
                bbox[0].ToDouble(),
                bbox[1].ToDouble(),
                bbox[2].ToDouble(),
                bbox[3].ToDouble()
            };
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Error parsing bbox parameter: " + val, ex);
        }
    }

    #region Geometry

    static public JsonGeometryDTO ToJsonGeometry(this IGeometry shape, bool addZ = false, bool addM = false)
    {
        if (shape is IPoint p1)
        {
            var jsonPoint = new JsonGeometryDTO()
            {
                HasZ = addZ ? true : null,
                HasM = addM ? true : null,

                X = p1.X,
                Y = p1.Y,
                Z = addZ ? p1.Z : null,
                M = addM ? p1.M : null
            };

            return jsonPoint;
        }

        if (shape is IMultiPoint multiPoint)
        {
            List<double?[]> points = new List<double?[]>();

            for (int p = 0, pointCount = multiPoint.PointCount; p < pointCount; p++)
            {
                var point = multiPoint[p];
                if (point != null)
                {
                    points.Add(
                        (addZ, addM) switch
                        {
                            (true, true) => new double?[] { point.X, point.Y, point.Z, point.M },
                            (true, false) => new double?[] { point.X, point.Y, point.Z },
                            (false, true) => new double?[] { point.X, point.Y, point.M },
                            _ => new double?[] { point.X, point.Y }
                        });
                }
            }

            var jsonMultipoint = new JsonGeometryDTO()
            {
                HasZ = addZ ? true : null,
                HasM = addM ? true : null,

                Points = points.ToArray()
            };

            return jsonMultipoint;
        }
        if (shape is IEnvelope)
        {
            return new JsonGeometryDTO()
            {
                XMin = ((IEnvelope)shape).MinX,
                YMin = ((IEnvelope)shape).MinY,
                XMax = ((IEnvelope)shape).MaxX,
                YMax = ((IEnvelope)shape).MaxY,
            };
        }
        if (shape is IPolyline polyline)
        {
            List<double?[,]> paths = new List<double?[,]>();
            for (int r = 0, pathCount = polyline.PathCount; r < pathCount; r++)
            {
                var path = polyline[r];
                if (path == null || path.PointCount == 0)
                {
                    continue;
                }

                double?[,] points = new double?[path.PointCount, 2 + (addZ ? 1 : 0) + (addM ? 1 : 0)];
                for (int p = 0, pointCount = path.PointCount; p < pointCount; p++)
                {
                    var point = path[p];
                    points[p, 0] = point.X;
                    points[p, 1] = point.Y;
                    if (addZ) points[p, 2] = point.Z;
                    if (addM) points[p, addZ ? 3 : 2] = point.M;
                }
                paths.Add(points);
            }

            var jsonPolyline = new JsonGeometryDTO()
            {
                HasZ = addZ ? true : null,
                HasM = addM ? true : null,

                Paths = paths.ToArray()
            };

            return jsonPolyline;
        }
        if (shape is IPolygon polygon)
        {
            List<double?[,]> rings = new List<double?[,]>();
            for (int r = 0, ringCount = polygon.RingCount; r < ringCount; r++)
            {
                var ring = polygon[r];
                if (ring == null || ring.PointCount == 0)
                {
                    continue;
                }

                double?[,] points = new double?[ring.PointCount, 2 + (addZ ? 1 : 0) + (addM ? 1 : 0)];
                for (int p = 0, pointCount = ring.PointCount; p < pointCount; p++)
                {
                    var point = ring[p];
                    points[p, 0] = point.X;
                    points[p, 1] = point.Y;
                    if (addZ) points[p, 2] = point.Z;
                    if (addM) points[p, addZ ? 3 : 2] = point.M;
                }
                rings.Add(points);
            }

            var jsonPolylgon = new JsonGeometryDTO()
            {
                HasZ = addZ ? true : null,
                HasM = addM ? true : null,

                Rings = rings.ToArray()
            };

            return jsonPolylgon;
        }
        return null;
    }

    static public IGeometry ToGeometry(this JsonGeometryDTO geometry)
    {
        if (geometry == null)
        {
            return null;
        }

        IGeometry shape = null;

        if (geometry.X.HasValue && geometry.Y.HasValue)
        {
            shape = new Point(geometry.X.Value, geometry.Y.Value).AddZM(geometry);
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
                    path.AddPoint(new Point(jsonPath[i, 0].Value, jsonPath[i, 1].Value).AddZM(jsonPath, i, geometry));
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
                    ring.AddPoint(new Point(jsonRing[i, 0].Value, jsonRing[i, 1].Value).AddZM(jsonRing, i, geometry));
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
                    multiPoint.AddPoint(new Point(point[0].Value, point[1].Value).AddZM(point, geometry));
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

    static public IEnumerable<JsonFeatureServerResponseDTO.JsonResponse> ToEditJsonResponse(this IEnumerable<int> objectIds, bool succeeded)
    {
        return objectIds.Select(objectId => new JsonFeatureServerResponseDTO.JsonResponse()
        {
            Success = succeeded,
            ObjectId = objectId
        });
    }

    #endregion

    #region PointExtension

    static private Point AddZM(this Point point, double?[] xyzm, JsonGeometryDTO geometry)
    {
        var index = 2;

        if (geometry.HasZ == true && index < xyzm.Length)
        {
            point.Z = xyzm[index++] ?? 0D;
        }
        if (geometry.HasM == true && index < xyzm.Length)
        {
            point.M = xyzm[index++] ?? 0D;
        }

        return point;
    }

    static private Point AddZM(this Point point, double?[,] xyzm, int pointIndex, JsonGeometryDTO geometry)
    {
        var index = 2;

        if (geometry.HasZ == true && index < xyzm.GetLength(1))
        {
            point.Z = xyzm[pointIndex, index++] ?? 0D;
        }
        if (geometry.HasM == true && index < xyzm.GetLength(1))
        {
            point.M = xyzm[pointIndex, index++] ?? 0D;
        }

        return point;
    }

    static private Point AddZM(this Point point, JsonGeometryDTO geometry)
    {
        if (geometry.HasZ == true)
        {
            point.Z = geometry.Z ?? 0D;
        }
        if (geometry.HasM == true)
        {
            point.M = geometry.M ?? 0D;
        }

        return point;
    }

    #endregion
}
