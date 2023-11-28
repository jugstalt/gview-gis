using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Framework.Geometry.GeoProcessing.Clipper;

namespace gView.Framework.Geometry.GeoProcessing
{
    public class Clip
    {
        public static IGeometry PerformClip(IGeometry clipper, IGeometry clippee)
        {
            if (clipper == null || clippee == null)
            {
                return null;
            }

            if (clipper is IEnvelope)
            {
                return ClipEnvelope(new Envelope(clipper.Envelope), clippee);
            }
            if (clipper is IPolygon)
            {
                //return ClipPolygon((IPolygon)clipper, clippee);
            }
            return null;
        }

        private static IGeometry ClipEnvelope(Envelope envelope, IGeometry clippee)
        {
            if (envelope == null || clippee == null)
            {
                return null;
            }

            if (double.IsInfinity(envelope.Width) ||
               double.IsInfinity(envelope.Height))
            {
                return clippee;
            }

            IEnvelope geomEnv = clippee.Envelope;

            if (!envelope.Intersects(geomEnv))
            {
                return null;
            }

            if (geomEnv.minx >= envelope.minx && geomEnv.maxx <= envelope.maxx &&
                geomEnv.miny >= envelope.miny && geomEnv.maxy <= envelope.maxy)
            {
                // Full included...
                return clippee;
            }

            if (clippee is IMultiPoint)
            {
                // Point ist schon durch den oberen Test enthalten...
                IMultiPoint multipoint = (IMultiPoint)clippee;
                MultiPoint newMultiPoint = new MultiPoint();

                for (int i = 0; i < multipoint.PointCount; i++)
                {
                    IPoint point = ClipPoint2Envelope(envelope, multipoint[i]);
                    if (point != null)
                    {
                        newMultiPoint.AddPoint(point);
                    }
                }
                return newMultiPoint;
            }
            if (clippee is IPolyline)
            {
                return ClipPolyline2Envelope(envelope, (IPolyline)clippee);
            }
            if (clippee is IPolygon)
            {
                //GeomPolygon clipperGeom = new GeomPolygon(envelope);
                //GeomPolygon clippeeGeom = new GeomPolygon((IPolygon)clippee);

                //GeomPolygon result = clippeeGeom.Clip(ClipOperation.Intersection, clipperGeom);
                //int x = result.NofContours;
                //return result.ToPolygon();
                return ((IPolygon)clippee).Clip(envelope);
            }
            return null;
        }

        private static IPoint ClipPoint2Envelope(IEnvelope envelope, IPoint point)
        {
            if (point.X >= envelope.minx && point.X <= envelope.maxx &&
                point.Y >= envelope.miny && point.Y <= envelope.maxy)
            {
                return new Point(point.X, point.Y);
            }
            return null;
        }

        //private static IGeometry ClipPolygon(IPolygon clipper, IGeometry clippee)
        //{
        //    if (clippee is IPolygon)
        //    {
        //        if (clipper is Polygon)
        //            ((Polygon)clipper).VerifyHoles();
        //        if (clippee is Polygon)
        //            ((Polygon)clippee).VerifyHoles();

        //        //if (SpatialAlgorithms.Algorithm.Contains(clippee, clipper))
        //        //    return clipper.Clone() as IGeometry;

        //        GeomPolygon clipperPolygon = new GeomPolygon(clipper);
        //        GeomPolygon clippeePolygon = new GeomPolygon((IPolygon)clippee);

        //        GeomPolygon res = clippeePolygon.Clip(ClipOperation.Intersection, clipperPolygon);
        //        return res.ToPolygon();
        //    }
        //    if (clippee is IPoint)
        //    {
        //        if (Algorithm.Jordan(clipper, ((IPoint)clippee).X, ((IPoint)clippee).Y))
        //            return clippee;
        //        else
        //            return null;
        //    }
        //    return null;
        //}

        private static IPolyline ClipPolyline2Envelope(IEnvelope envelope, IPolyline line)
        {
            Polyline newLine = new Polyline();

            for (int pathIndex = 0; pathIndex < line.PathCount; pathIndex++)
            {
                IPath path = line[pathIndex];
                if (path.PointCount < 2)
                {
                    continue;
                }

                IPoint p1 = path[0];
                Path newPath = null;
                for (int i = 1; i < path.PointCount; i++)
                {
                    IPoint p2 = path[i];
                    LineClipType type;
                    IPoint[] points = LiamBarsky(envelope, p1, p2, out type);

                    switch (type)
                    {
                        case LineClipType.inside:
                        case LineClipType.entering:
                            if (newPath == null)
                            {
                                newPath = new Path();
                                newPath.AddPoint(points[0]);
                            }
                            newPath.AddPoint(points[1]);
                            break;
                        case LineClipType.outside:
                            if (newPath != null)
                            {
                                newLine.AddPath(newPath);
                                newPath = null;
                            }
                            break;
                        case LineClipType.leaving:
                            if (newPath == null)
                            {
                                newPath = new Path();
                                newPath.AddPoint(points[0]);
                            }
                            newPath.AddPoint(points[1]);
                            newLine.AddPath(newPath);
                            newPath = null;
                            break;
                    }

                    p1 = p2;
                }
                if (newPath != null)
                {
                    newLine.AddPath(newPath);
                }
            }

            if (newLine.PathCount > 0)
            {
                return newLine;
            }

            return null;
        }
        private enum LineClipType { entering, leaving, outside, inside }
        private static IPoint[] LiamBarsky(IEnvelope envelope, IPoint p1, IPoint p2, out LineClipType type)
        {
            IPoint[] points = new Point[2];

            int code1 = CalculateLRBT(envelope, p1);
            int code2 = CalculateLRBT(envelope, p2);

            type = LineClipType.inside;
            if (code1 == 0 && code2 == 0)
            {
                points[0] = p1;
                points[1] = p2;
                return points;
            }
            if ((code1 & code2) != 0)
            {
                type = LineClipType.outside;
                return null;
            }

            double[] pdiff = { p2.X - p1.X, p2.Y - p1.Y };
            double[] p0 = { p1.X, p1.Y };
            double tpe = 1e10; //0.0;
            double tpl = 1e10; //1.0;

            for (int edge = 0; edge < 4; edge++)
            {
                double[] N = new double[2];
                double[] pe = new double[2];
                switch (edge)
                {
                    case 0:
                        N[0] = -1.0; N[1] = 0.0;
                        pe[0] = envelope.minx; pe[1] = 0.0;
                        break;
                    case 1:
                        N[0] = 0.0; N[1] = -1.0;
                        pe[0] = 0.0; pe[1] = envelope.miny;
                        break;
                    case 2:
                        N[0] = 1.0; N[1] = 0.0;
                        pe[0] = envelope.maxx; pe[1] = 0.0;
                        break;
                    case 3:
                        N[0] = 0.0; N[1] = 1.0;
                        pe[0] = 0.0; pe[1] = envelope.maxy;
                        break;
                }

                double t = SolveLineFactorT(N, pe, p0, pdiff, out type);
                if (t >= 0 && t <= 1.0)
                {
                    if (type == LineClipType.entering && (t > tpe || tpe == 1e10))
                    {
                        tpe = t;
                    }
                    else if (type == LineClipType.leaving && (t < tpl || tpl == 1e10))
                    {
                        tpl = t;
                    }
                }
            }

            if (tpe == 1e10 && tpl == 1e10)
            {
                type = LineClipType.outside;
                return null;
            }

            double tpe_ = (tpe != 1e10) ? tpe : 0.0;
            double tpl_ = (tpl != 1e10) ? tpl : 1.0;

            points[0] = new Point(p1.X + tpe_ * pdiff[0], p1.Y + tpe_ * pdiff[1]);
            points[1] = new Point(p1.X + tpl_ * pdiff[0], p1.Y + tpl_ * pdiff[1]);

            code1 = CalculateLRBT(envelope, points[0]);
            code2 = CalculateLRBT(envelope, points[0]);

            if (code1 != 0 || code2 != 0)
            {
                type = LineClipType.outside;
                return null;
            }

            if (tpl_ < 1.0)
            {
                type = LineClipType.leaving;
                return points;
            }
            else if (tpe_ > 0.0)
            {
                type = LineClipType.entering;
                return points;
            }
            return null;
        }

        private static double SolveLineFactorT(double[] N, double[] pe, double[] p0, double[] pdiff, out LineClipType type)
        {
            double dominator = N[0] * pdiff[0] + N[1] * pdiff[1];

            if (dominator < 0.0)
            {
                type = LineClipType.entering;
            }
            else
            {
                type = LineClipType.leaving;
            }

            if (dominator == 0.0)
            {
                return 1e10; // paralell lines...
            }

            return (N[0] * (pe[0] - p0[0]) + N[1] * (pe[1] - p0[1])) / dominator;
        }
        // Code from Cohen-Sutherland 
        private static int CalculateLRBT(IEnvelope env, IPoint p)
        {
            //   1001   0001   0101
            //   1000   0000   0100
            //   1010   0010   0110

            int code = 0;
            if (p.X < env.minx)
            {
                code = code | 0x1000;
            }

            if (p.X > env.maxx)
            {
                code = code | 0x0100;
            }

            if (p.Y < env.miny)
            {
                code = code | 0x0010;
            }

            if (p.Y > env.maxy)
            {
                code = code | 0x0001;
            }

            return code;
        }

        //public static System.Drawing.Drawing2D.GraphicsPath PerformClip(System.Drawing.Rectangle rect, System.Drawing.Drawing2D.GraphicsPath clippeePath)
        //{
        //    System.Drawing.Drawing2D.GraphicsPath clipperPath = new System.Drawing.Drawing2D.GraphicsPath();
        //    clipperPath.AddPolygon(new System.Drawing.PointF[]{
        //        new System.Drawing.PointF(rect.Left,rect.Bottom),
        //        new System.Drawing.PointF(rect.Right,rect.Bottom),
        //        new System.Drawing.PointF(rect.Right,rect.Top),
        //        new System.Drawing.PointF(rect.Left,rect.Top)}
        //        );

        //    GeomPolygon clipper = new GeomPolygon(clipperPath);
        //    GeomPolygon clippee = new GeomPolygon(clippeePath);

        //    GeomPolygon result = clipper.Clip(ClipOperation.Intersection, clippee);
        //    return result.ToGraphicsPath();
        //}
    }
}
