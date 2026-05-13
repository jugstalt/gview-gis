using gView.Framework.Core.Geometry;
using gView.Framework.Geometry.GeoProcessing;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.OGC.WKT
{
    public class WKT
    {
        public readonly static System.Globalization.NumberFormatInfo _nhi = new System.Globalization.CultureInfo("en-US", false).NumberFormat;

        public static string ToWKT(IGeometry geometry, bool addZ = false, bool addM = false)
        {
            StringBuilder sb = new StringBuilder();
            if (geometry is IPoint)
            {
                sb.Append("POINT(");
                AppendPoint(sb, (IPoint)geometry, addZ, addM);
                sb.Append(")");
            }
            else if (geometry is IMultiPoint)
            {
                sb.Append("MULTIPOINT(");
                bool first = true;
                for (int i = 0; i < ((IMultiPoint)geometry).PointCount; i++)
                {
                    IPoint mPoint = ((IMultiPoint)geometry)[i];
                    if (mPoint == null)
                    {
                        continue;
                    }

                    if (!first)
                    {
                        sb.Append(",");
                    }

                    sb.Append("(");
                    AppendPoint(sb, mPoint, addZ, addM);
                    sb.Append(")");
                    first = false;
                }
                sb.Append(")");
            }
            else if (geometry is IPolyline polyline)
            {
                switch(polyline.PathCount)
                {
                    case 0:
                        sb.Append("MULTILINESTRING EMPTY");
                        break;
                    case 1:
                        sb.Append("LINESTRING");
                        AppendPointCollection(sb, polyline[0], addZ, addM);
                        break;
                    default:
                        sb.Append("MULTILINESTRING(");
                        AppendPolyline(sb, (IPolyline)geometry, addZ, addM);
                        sb.Append(")");
                        break;
                }
                
            }
            else if (geometry is IPolygon)
            {
                switch (((IPolygon)geometry).RingCount)
                {
                    case 0:
                        sb.Append("MULTIPOLYGON EMPTY");
                        break;
                    case 1:
                        sb.Append("POLYGON(");
                        AppendPolygon(sb, (IPolygon)geometry, addZ, addM);
                        sb.Append(")");
                        break;
                    default:
                        List<IPolygon> polygons = Algorithm.SplitPolygonToDonutsAndPolygons((IPolygon)geometry);
                        if (polygons == null)
                        {
                            sb.Append("MULTIPOLYGON EMPTY");
                            break;
                        }
                        if (polygons.Count == 1)
                        {
                            sb.Append("POLYGON(");
                            AppendPolygon(sb, polygons[0], addZ, addM);
                            sb.Append(")");
                        }
                        else
                        {
                            sb.Append("MULTIPOLYGON(");
                            bool first = true;

                            var equal = polygons[1].Equals(polygons[1]);

                            foreach (IPolygon mPoly in polygons)
                            {
                                if (!first)
                                {
                                    sb.Append(",");
                                }

                                sb.Append("(");
                                AppendPolygon(sb, mPoly, addZ, addM);
                                sb.Append(")");

                                first = false;
                            }
                            sb.Append(")");
                        }
                        break;
                }
            }
            else if (geometry is IAggregateGeometry)
            {
                sb.Append("GEOMETRYCOLLECTION(");
                bool first = true;
                for (int i = 0; i < ((IAggregateGeometry)geometry).GeometryCount; i++)
                {
                    IGeometry aGeom = ((IAggregateGeometry)geometry)[i];
                    if (aGeom == null)
                    {
                        continue;
                    }

                    if (!first)
                    {
                        sb.Append(",");
                    }

                    sb.Append(ToWKT(aGeom));
                    first = false;
                }
                sb.Append(")");
            }
            else if (geometry is IEnvelope)
            {
                IEnvelope env = (IEnvelope)geometry;
                sb.Append("POLYGON((");
                sb.Append(env.MinX.ToString(_nhi) + " ");
                sb.Append(env.MinY.ToString(_nhi) + ",");

                sb.Append(env.MaxX.ToString(_nhi) + " ");
                sb.Append(env.MinY.ToString(_nhi) + ",");

                sb.Append(env.MaxX.ToString(_nhi) + " ");
                sb.Append(env.MaxY.ToString(_nhi) + ",");

                sb.Append(env.MinX.ToString(_nhi) + " ");
                sb.Append(env.MaxY.ToString(_nhi) + ",");

                sb.Append(env.MinX.ToString(_nhi) + " ");
                sb.Append(env.MinY.ToString(_nhi) + "))");
            }
            return sb.ToString();
        }

        private static void AppendPoint(StringBuilder sb, IPoint point, bool addZ, bool addM)
        {
            if (point == null)
            {
                return;
            }

            sb.Append($"{point.X.ToString(_nhi)} {point.Y.ToString(_nhi)}");
            
            if(addZ || addM)
            {
                sb.Append(
                    (addZ, addM) switch
                    {
                        (true, false) => $" {point.Z.ToString(_nhi)}",
                        (true, true) => $" {point.Z.ToString(_nhi)} {point.M.ToString(_nhi)}",
                        (false, true) => $" NULL {point.M.ToString(_nhi)}",
                        _ => ""
                    }
                    );
            }
        }

        private static void AppendPointCollection(StringBuilder sb, IPointCollection pColl, bool addZ, bool addM)
        {
            if (pColl == null || pColl.PointCount == 0)
            {
                return;
            }

            sb.Append("(");
            bool first = true;
            for (int i = 0; i < pColl.PointCount; i++)
            {
                IPoint p = pColl[i];
                if (p != null)
                {
                    if (!first)
                    {
                        sb.Append(",");
                    }

                    AppendPoint(sb, p, addZ, addM);
                    first = false;
                }
            }
            sb.Append(")");
        }

        private static void AppendPolyline(StringBuilder sb, IPolyline pLine, bool addZ, bool addM)
        {
            if (pLine == null || pLine.PathCount == 0)
            {
                return;
            }

            bool first = true;
            for (int i = 0; i < pLine.PathCount; i++)
            {
                IPath p = pLine[i];
                if (p != null && p.PointCount > 1)
                {
                    if (!first)
                    {
                        sb.Append(",");
                    }

                    AppendPointCollection(sb, p, addZ, addM);
                    first = false;
                }
            }
        }

        private static void AppendPolygon(StringBuilder sb, IPolygon poly, bool addZ, bool addM)
        {
            if (poly == null || poly.RingCount == 0)
            {
                return;
            }

            bool first = true;
            for (int i = 0; i < poly.RingCount; i++)
            {
                IRing r = poly[i];
                if (r != null && r.PointCount > 2)
                {
                    if (!first)
                    {
                        sb.Append(",");
                    }

                    r.ClosePath();
                    AppendPointCollection(sb, r, addZ, addM);
                    first = false;
                }
            }
        }
    }
}
