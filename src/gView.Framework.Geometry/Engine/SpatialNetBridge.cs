using gView.Framework.Core.Geometry;
using SpatialNET;
using System;

namespace gView.Framework.Geometry.Engine
{
    /// <summary>
    /// The single place where gView calls SpatialNET (ADR 0002).
    /// </summary>
    /// <remarks>
    /// SpatialNET is always called in its static form (<c>Overlays.Union(a, b)</c>), never as an
    /// extension method: gView's classes have instance members of the same name (<c>Polygon.Union</c>,
    /// <c>Polygon.Area</c>), which always win over extension methods.
    /// </remarks>
    internal static class SpatialNetBridge
    {
        private static readonly GViewGeometryFactory Factory = GViewGeometryFactory.Instance;

        public static IGeometry Intersection(IGeometry a, IGeometry b)
        {
            return ToGView(Overlays.Intersection(ToSpatial(a), ToSpatial(b), factory: Factory));
        }

        public static IGeometry Difference(IGeometry a, IGeometry b)
        {
            return ToGView(Overlays.Difference(ToSpatial(a), ToSpatial(b), factory: Factory));
        }

        public static IGeometry SymDifference(IGeometry a, IGeometry b)
        {
            return ToGView(Overlays.SymDifference(ToSpatial(a), ToSpatial(b), factory: Factory));
        }

        public static IGeometry Union(IGeometry a, IGeometry b)
        {
            return ToGView(Overlays.Union(ToSpatial(a), ToSpatial(b), factory: Factory));
        }

        /// <summary>
        /// Passes gView geometries through and copies foreign implementations into gView classes.
        /// </summary>
        /// <exception cref="NotSupportedException">Envelopes, aggregate geometries and unknown types.</exception>
        internal static IGeometry2D ToSpatial(IGeometry geometry)
        {
            switch (geometry)
            {
                case null:
                    throw new ArgumentNullException(nameof(geometry));
                case IEnvelope:
                case IAggregateGeometry:
                    throw new NotSupportedException($"{geometry.GetType().Name} is not supported by SpatialNET.");
                case IGeometry2D spatial:
                    return spatial;
                case IPoint point:
                    return new Point(point) { Srs = point.Srs };
                case IMultiPoint multiPoint:
                    return new MultiPoint(multiPoint) { Srs = multiPoint.Srs };
                case IPolyline polyline:
                    return CopyPolyline(polyline);
                case IPolygon polygon:
                    return new Polygon(polygon) { Srs = polygon.Srs };
                default:
                    throw new NotSupportedException($"{geometry.GetType().Name} is not supported by SpatialNET.");
            }
        }

        // Results always come from GViewGeometryFactory, so they are gView geometries
        private static IGeometry ToGView(IGeometry2D result) => (IGeometry)result;

        private static Polyline CopyPolyline(IPolyline polyline)
        {
            var copy = new Polyline() { Srs = polyline.Srs };
            for (int i = 0; i < polyline.PathCount; i++)
            {
                if (polyline[i] != null)
                {
                    copy.AddPath(new Path(polyline[i]));
                }
            }

            return copy;
        }
    }
}
