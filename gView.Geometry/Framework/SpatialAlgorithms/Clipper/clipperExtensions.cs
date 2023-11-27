using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using System.Collections.Generic;

namespace gView.Framework.SpatialAlgorithms.Clipper
{
    using ClipperPolygons = List<List<IntPoint>>;

    public static class ClipperExtensions
    {
        #region ToClipper

        static public ClipperPolygons ToClipperPolygons(this IPolygon polygon)
        {
            if (polygon == null)
            {
                return null;
            }

            Clipper c = new Clipper();
            ClipperPolygons polygons = new ClipperPolygons(), result = new ClipperPolygons();

            for (int r = 0, r_to = polygon.RingCount; r < r_to; r++)
            {
                var ring = polygon[r];
                ring.Close();

                var ringPoints = new List<IntPoint>();
                for (int p = 0, p_to = ring.PointCount; p < p_to; p++)
                {
                    ringPoints.Add(ring[p].ToClipperPointer());
                }
                polygons.Add(ringPoints);
            }

            c.AddPaths(polygons, PolyType.ptSubject, true);
            if (c.Execute(ClipType.ctUnion, result) == true)
            {
                return result;
            }

            return null;
        }

        static public int Acc = 1000;
        static public IntPoint ToClipperPointer(this IPoint point)
        {
            return new IntPoint((long)(point.X * Acc), (long)(point.Y * Acc));
        }

        static public ClipperPolygons Buffer(this ClipperPolygons polygons, double distance)
        {
            ClipperPolygons result = new ClipperPolygons();

            ClipperOffset co = new ClipperOffset();
            co.AddPaths(polygons, JoinType.jtRound, EndType.etClosedPolygon);
            co.Execute(ref result, distance * Acc);

            return result;
        }

        #endregion

        #region From Clipper

        static public Polygon ToPolygon(this ClipperPolygons clipperPolygons)
        {
            Polygon result = new Polygon();

            foreach (var clipperRing in clipperPolygons)
            {
                var ring = new Ring();

                foreach (var clipperPoint in clipperRing)
                {
                    ring.AddPoint(clipperPoint.ToPoint());
                }
                result.AddRing(ring);
            }

            return result;
        }

        static public Point ToPoint(this IntPoint clipperPoint)
        {
            return new Point((double)clipperPoint.X / Acc, (double)clipperPoint.Y / Acc);
        }

        #endregion

        #region Operations

        static public IPolygon Merge(this List<IPolygon> polygons)
        {
            Clipper c = new Clipper();

            foreach (var polygon in polygons)
            {
                c.AddPaths(polygon.ToClipperPolygons(), PolyType.ptSubject, true);
            }

            ClipperPolygons result = new ClipperPolygons();
            if (c.Execute(ClipType.ctUnion, result, PolyFillType.pftPositive) == true)
            {
                return result.ToPolygon();
            }

            return null;
        }

        static public IPolygon Clip(this IPolygon clippee, Envelope clipper)
        {
            var clippeePolygons = clippee.ToClipperPolygons();
            var clipperPolygons = clipper.ToPolygon().ToClipperPolygons();

            var c = new Clipper();
            c.AddPaths(clippeePolygons, PolyType.ptSubject, true);
            c.AddPaths(clipperPolygons, PolyType.ptClip, true);

            ClipperPolygons result = new ClipperPolygons();
            if (c.Execute(ClipType.ctIntersection, result) == true)
            {
                return result.ToPolygon();
            }

            return clippee;
        }

        #endregion
    }
}
