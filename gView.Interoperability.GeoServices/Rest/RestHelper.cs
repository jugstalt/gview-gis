using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system;
using System;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest
{
    public class RestHelper
    {
        public static FieldType FType(string parsedFieldTypeString)
        {
            switch (parsedFieldTypeString)
            {
                case "esriFieldTypeBlob":
                    return FieldType.binary;
                case "esriFieldTypeDate":
                    return FieldType.Date;
                case "esriFieldTypeDouble":
                    return FieldType.Double;
                case "esriFieldTypeGeometry":
                    return FieldType.Shape;
                case "esriFieldTypeGlobalID":
                    return FieldType.String;
                case "esriFieldTypeGUID":
                    return FieldType.guid;
                case "esriFieldTypeInteger":
                    return FieldType.integer;
                case "esriFieldTypeOID":
                    return FieldType.ID;
                case "esriFieldTypeRaster":
                    return FieldType.binary;
                case "esriFieldTypeSingle":
                    return FieldType.Float;
                case "esriFieldTypeSmallInteger":
                    return FieldType.smallinteger;
                case "esriFieldTypeString":
                    return FieldType.String;
                case "esriFieldTypeXML":
                    return FieldType.String;
            }
            return FieldType.unknown;
        }

        public static string ConvertGeometryToJson(IGeometry geometry,
                                                   int spatialReferenceId,
                                                   bool hasZ = false,
                                                   bool hasM = false)
        {
            string geometryType = GetGeometryTypeString(geometry);
            switch (geometryType)
            {
                case "esriGeometryPoint":
                    return Convert2DPointToJsonString((IPoint)geometry, spatialReferenceId, hasZ, hasM);
                case "esriGeometryMultiPoint":
                    return Convert2DMultiPointToJsonString((IMultiPoint)geometry, spatialReferenceId, hasZ, hasM);
                case "esriGeometryPolyline":
                    return Convert2DPolylineToJsonString((IPolyline)geometry, spatialReferenceId, hasZ, hasM);
                case "esriGeometryPolygon":
                    return Convert2DPolygonToJsonString((IPolygon)geometry, spatialReferenceId, hasZ, hasM);
                case "esriGeometryEnvelope":
                    return Convert2DEnvelopeToJsonString((IEnvelope)geometry, spatialReferenceId);
                default:
                    throw new NotImplementedException("Passed in GeometryType is not implemented yet.");
            }
        }

        public static string GetGeometryTypeString(IGeometry geometry)
        {
            if (geometry is IPoint)
            {
                return "esriGeometryPoint";
            }
            else if (geometry is IMultiPoint)
            {
                return "esriGeometryMultiPoint";
            }
            else if (geometry is IPolyline)
            {
                return "esriGeometryPolyline";
            }
            else if (geometry is IPolygon)
            {
                return "esriGeometryPolygon";
            }
            else if (geometry is IEnvelope)
            {
                return "esriGeometryEnvelope";
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static string Convert2DPointToJsonString(IPoint point, int spatialReferenceId, bool hasZ, bool hasM)
        {
            if (hasZ == false && hasM == false)
            {
                return
                    @"{""x"":" + point.X.ToDoubleString() +
                    @",""y"":" + point.Y.ToDoubleString() +
                    @",""spatialReference"":{""wkid"":" + spatialReferenceId + "}" +
                    "}";
            }

            StringBuilder sb = new StringBuilder();

            sb.Append(@"{""x"":" + point.X.ToDoubleString() +
                      @",""y"":" + point.Y.ToDoubleString());

            if (hasZ)
            {
                sb.Append(@",""z"":" + point.Z.ToDoubleString());
            }
            if (hasM)
            {
                double mv = point is PointM && ((PointM)point).M != null ? Convert.ToDouble(((PointM)point).M) : 0.0D;
                sb.Append(@",""m"":" + ((double)mv).ToDoubleString());
            }

            sb.Append(@",""spatialReference"":{""wkid"":" + spatialReferenceId + "}}");

            return sb.ToString();
        }

        private static string Convert2DEnvelopeToJsonString(IEnvelope envelope, int spatialReferenceId)
        {
            return
            @"{""xmin"":" + envelope.minx.ToDoubleString() +
            @",""ymin"":" + envelope.miny.ToDoubleString() +
            @",""xmax"":" + envelope.maxx.ToDoubleString() +
            @",""ymax"":" + envelope.maxy.ToDoubleString() +
            @",""spatialReference"":{""wkid"":" + spatialReferenceId + "}" +
            "}";
        }

        private static string Convert2DMultiPointToJsonString(IMultiPoint multiPoint, int spatialReferenceId, bool hasZ, bool hasM)
        {
            PointCollection pointCollection;
            int pathCount = multiPoint.PointCount;
            PointCollection[] pointCollectionArray = new PointCollection[pathCount];
            for (int pathIndex = 0; pathIndex < pathCount; pathIndex++)
            {
                IPoint point = multiPoint[pathIndex];
                //int pointCount = point.PointCount;

                pointCollection = new PointCollection();

                pointCollection.AddPoint(new Point(point.X, point.Y));

                pointCollectionArray[pathIndex] = pointCollection;
            }

            return Convert2DPointCollectionToJsonString(pointCollectionArray, PointCollectionParent.points, spatialReferenceId, hasZ, hasM);
        }

        private enum PointCollectionParent
        {
            paths,
            rings,
            points
        }
        private static string Convert2DPolylineToJsonString(IPolyline polyline, int spatialReferenceId, bool hasZ, bool hasM)
        {
            PointCollection pointCollection;
            int pathCount = polyline.PathCount;
            PointCollection[] pointCollectionArray = new PointCollection[pathCount];
            for (int pathIndex = 0; pathIndex < pathCount; pathIndex++)
            {
                IPath path = polyline[pathIndex];
                int pointCount = path.PointCount;

                pointCollection = new PointCollection();

                for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
                {
                    pointCollection.AddPoint(Clone(path[pointIndex]));
                }
                pointCollectionArray[pathIndex] = pointCollection;
            }

            return Convert2DPointCollectionToJsonString(pointCollectionArray, PointCollectionParent.paths, spatialReferenceId, hasZ, hasM);
        }
        private static string Convert2DPolygonToJsonString(IPolygon polygon, int spatialReferenceId, bool hasZ, bool hasM)
        {
            PointCollection pointCollection;
            int ringCount = polygon.RingCount;
            PointCollection[] pointCollectionArray = new PointCollection[ringCount];
            for (int ringIndex = 0; ringIndex < ringCount; ringIndex++)
            {
                IRing ring = polygon[ringIndex];
                int pointCount = ring.PointCount;

                pointCollection = new PointCollection();

                for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
                {
                    pointCollection.AddPoint(Clone(ring[pointIndex]));
                }
                pointCollectionArray[ringIndex] = pointCollection;
            }

            return Convert2DPointCollectionToJsonString(pointCollectionArray, PointCollectionParent.rings, spatialReferenceId, hasZ, hasM);
        }

        private static string Convert2DPointCollectionToJsonString(PointCollection[] pointCollectionArray, PointCollectionParent parent, int spatialReferenceId, bool hasZ, bool hasM)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(@"{");
            if (hasZ == true)
            {
                sb.Append(@"""hasZ"":true,");
            }

            if (hasM == true)
            {
                sb.Append(@"""hasM"":true,");
            }

            sb.Append(@"""" + parent + @""":[");

            for (int arrayIndex = 0; arrayIndex < pointCollectionArray.Length; arrayIndex++)
            {
                if (arrayIndex > 0)
                {
                    sb.Append(",");
                }

                if (parent.ToString() != "points")
                {
                    sb.Append("[");
                }

                for (int pointIndex = 0, to = pointCollectionArray[arrayIndex].PointCount; pointIndex < to; pointIndex++)
                {
                    if (pointIndex > 0)
                    {
                        sb.Append(",");
                    }

                    StringBuilder zm = new StringBuilder();
                    if (hasZ)
                    {
                        zm.Append("," + pointCollectionArray[arrayIndex][pointIndex].Z.ToDoubleString());
                    }
                    if (hasM)
                    {
                        double m = 0D;
                        try
                        {
                            if (pointCollectionArray[arrayIndex][pointIndex] is PointM)
                            {
                                m = Convert.ToDouble(((PointM)pointCollectionArray[arrayIndex][pointIndex]).M);
                            }
                        }
                        catch { }

                        zm.Append("," + m.ToDoubleString());
                    }

                    sb.Append(
                    "[" +
                        pointCollectionArray[arrayIndex][pointIndex].X.ToDoubleString() +
                        "," +
                        pointCollectionArray[arrayIndex][pointIndex].Y.ToDoubleString() +
                        zm.ToString() +
                    "]");
                }

                if (parent.ToString() != "points")
                {
                    sb.Append("]");
                }
            }

            sb.Append(@"],""spatialReference"":{""wkid"":" + spatialReferenceId + "}}");
            return sb.ToString();
        }

        #region Helper

        static private IPoint Clone(IPoint point)
        {
            if (point is PointM3)
            {
                return new PointM3(point, ((PointM3)point).M, ((PointM3)point).M2, ((PointM3)point).M3);
            }
            else if (point is PointM2)
            {
                return new PointM2(point, ((PointM2)point).M, ((PointM2)point).M2);
            }
            else if (point is PointM)
            {
                return new PointM(point, ((PointM)point).M);
            }

            return new Point(point);
        }

        #endregion
    }
}
