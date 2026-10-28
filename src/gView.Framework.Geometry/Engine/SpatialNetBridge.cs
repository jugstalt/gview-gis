using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using SpatialNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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

        #region Merge

        /// <summary>
        /// The SpatialNET variant of gView's polygon merge: all polygons in one <c>UnionAll</c>, each read
        /// by even-odd nesting (independent of ring orientation). gView's conventions: no polygons give
        /// <c>null</c>, a single polygon is returned unchanged.
        /// </summary>
        internal static IPolygon MergePolygons(IList<IPolygon> polygons)
        {
            var parts = polygons?.Where(p => p != null).ToList();
            if (parts == null || parts.Count == 0)
            {
                return null;
            }

            if (parts.Count == 1)
            {
                return parts[0];
            }

            return (IPolygon)Overlays.UnionAll(
                parts.Select(p => (IPolygon2D)ToSpatial(p)), factory: Factory);
        }

        #endregion

        #region Clip

        /// <summary>
        /// The SpatialNET variant of Clipper's polygon clip: the part of <paramref name="polygon"/> inside
        /// <paramref name="envelope"/>, an empty polygon if they do not meet (as Clipper gives).
        /// </summary>
        internal static IPolygon ClipPolygon(IPolygon polygon, IEnvelope envelope)
        {
            var rectangle = new Envelope2D(envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY);

            return (IPolygon)Overlays.Clip((IPolygon2D)ToSpatial(polygon), rectangle, factory: Factory);
        }

        #endregion

        #region Buffer

        // 16 segments per quarter circle: about 5.6° per step, close to gView's own 0.1 rad.
        // An angle works in degrees and metres alike, unlike an arc tolerance in map units.
        private static readonly BufferOptions BufferSettings = new BufferOptions { QuadrantSegments = 16 };

        /// <summary>
        /// The SpatialNET variant of <c>ITopologicalOperation.Buffer</c>, with gView's conventions:
        /// points and lines with a distance ≤ 0 give <c>null</c>, a polygon with distance 0 is returned itself,
        /// and a polygon shrunk away gives an empty polygon.
        /// </summary>
        internal static IPolygon Buffer(IGeometry geometry, double distance)
        {
            if (geometry is IPolygon polygon)
            {
                if (distance == 0.0)
                {
                    return polygon;
                }
            }
            else if (distance <= 0.0)
            {
                return null;
            }

            return (IPolygon)Buffering.Buffer(ToSpatial(geometry), distance, BufferSettings, Factory);
        }

        #endregion

        #region Spatial relations

        // Filter geometries are prepared once and reused for every feature of a cursor. The entry lives as long as
        // the filter geometry object. A prepared geometry is a snapshot: changing the filter geometry in place
        // after the first check is not seen.
        private static readonly ConditionalWeakTable<IGeometry, PreparedGeometry> PreparedFilters = new();

        /// <summary>
        /// The SpatialNET variant of <c>SpatialRelation.Check</c>: whether <paramref name="geometry"/> passes a
        /// spatial filter with <paramref name="relation"/> and <paramref name="filterGeometry"/>.
        /// </summary>
        /// <returns>
        /// <c>null</c> if SpatialNET does not decide it: aggregate geometries, degenerate envelopes (width or height 0)
        /// and the envelope relations. The caller then uses gView's own implementation.
        /// </returns>
        internal static bool? Check(spatialRelation relation, IGeometry filterGeometry, IGeometry geometry)
        {
            if (!CanCheck(filterGeometry) || !CanCheck(geometry))
            {
                return null;
            }

            var candidate = ToSpatial(geometry);
            switch (relation)
            {
                case spatialRelation.SpatialRelationIntersects:
                    return PreparedFilter(filterGeometry).Intersects(candidate);
                case spatialRelation.SpatialRelationContains:
                    // the filter geometry contains the feature
                    return PreparedFilter(filterGeometry).Contains(candidate);
                case spatialRelation.SpatialRelationWithin:
                    // the filter geometry lies within the feature (as gView's Algorithm.Contains(feature, filter))
                    return PreparedFilter(filterGeometry).Within(candidate);
                default:
                    // The envelope relations stay classic (render path, box tests); SpatialRelation.Check
                    // does not even call the bridge for them
                    return null;
            }
        }

        private static PreparedGeometry PreparedFilter(IGeometry filterGeometry)
        {
            return PreparedFilters.GetValue(filterGeometry, g => Preparation.Prepare(ToSpatial(g)));
        }

        private static bool CanCheck(IGeometry geometry)
        {
            switch (geometry)
            {
                case IEnvelope envelope:
                    return envelope.Width > 0 && envelope.Height > 0;
                case IPoint:
                case IMultiPoint:
                case IPolyline:
                case IPolygon:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        /// <summary>
        /// Passes gView geometries through and copies foreign implementations into gView classes.
        /// Envelopes become rectangular polygons.
        /// </summary>
        /// <exception cref="NotSupportedException">Aggregate geometries and unknown types.</exception>
        internal static IGeometry2D ToSpatial(IGeometry geometry)
        {
            switch (geometry)
            {
                case null:
                    throw new ArgumentNullException(nameof(geometry));
                case IEnvelope envelope:
                    return EnvelopeToPolygon(envelope);
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

        private static Polygon EnvelopeToPolygon(IEnvelope envelope)
        {
            var ring = new Ring(new List<IPoint>
            {
                new Point(envelope.MinX, envelope.MinY), new Point(envelope.MaxX, envelope.MinY),
                new Point(envelope.MaxX, envelope.MaxY), new Point(envelope.MinX, envelope.MaxY),
            });

            return new Polygon(ring) { Srs = envelope.Srs };
        }

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
