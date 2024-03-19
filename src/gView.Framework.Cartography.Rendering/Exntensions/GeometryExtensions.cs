using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.Cartography.Rendering.Exntensions;

static internal class GeometryExtensions
{
    static public IGeometry Elevate(this IGeometry geometry, IDisplay display, double elevation)
    {
        var Z = display.MapScale;
        if (Z <= elevation) return null;

        var center = display.Envelope.Center;

        if (geometry is IPoint point)
        {
            return ElevatePoint(point, center, Z, elevation);
        }
        else if (geometry is IMultiPoint multiPoint)
        {
            var elevatedMultiPoint = new MultiPoint();

            for (int p = 0; p < multiPoint.PointCount; p++)
            {
                elevatedMultiPoint.AddPoint(ElevatePoint(multiPoint[p], center, Z, elevation));
            }

            return elevatedMultiPoint;
        }
        else if(geometry is IPolyline polyline)
        {
            var elevatedPolyline = new Polyline();

            for (int r = 0; r < polyline.PathCount; r++)
            {
                var path = polyline[r];

                var elevatedPath = new Path();
                elevatedPolyline.AddPath(elevatedPath);

                for (var p = 0; p < path.PointCount; p++)
                {
                    elevatedPath.AddPoint(ElevatePoint(path[p], center, Z, elevation));
                }
            }

            return elevatedPolyline;
        }
        if (geometry is IPolygon polygon)
        {
            var elevatedPolygon = new Polygon();

            for (int r = 0; r < polygon.RingCount; r++)
            {
                var ring = polygon[r];

                var elevatedRing = new Ring();
                elevatedPolygon.AddRing(elevatedRing);

                for (var p = 0; p < ring.PointCount; p++)
                {
                    elevatedRing.AddPoint(ElevatePoint(ring[p], center, Z, elevation));
                }
            }

            return elevatedPolygon;
        }

        return null;
    }

    static private IPoint ElevatePoint(IPoint point, IPoint center, double Z, double elevation)
    {
        if (point == null) return null;

        double dx = point.X - center.X;
        double dy = point.Y - center.Y;

        double d = Math.Sqrt(dx * dx + dy * dy);
        double t = d / Z * elevation;

        double rx = dx / d, ry = dy / d;

        return new Point(point.X + rx * t, point.Y + ry * t);
    }

}
