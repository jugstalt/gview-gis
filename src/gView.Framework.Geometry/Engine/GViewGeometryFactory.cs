using gView.Framework.Core.Geometry;
using SpatialNET;
using System;

namespace gView.Framework.Geometry.Engine
{
    /// <summary>
    /// Creates SpatialNET results as gView geometries (ADR 0002). Stateless and therefore thread-safe;
    /// passed explicitly to every SpatialNET call, never set as <see cref="GeometryFactory.Default"/>.
    /// </summary>
    internal sealed class GViewGeometryFactory : IGeometryFactory
    {
        public static readonly GViewGeometryFactory Instance = new GViewGeometryFactory();

        private GViewGeometryFactory() { }

        // M is not carried (SupportsM keeps its default false)
        public bool SupportsZ => true;

        public IPoint2D CreatePoint(Coordinate coordinate, IGeometry2D source)
        {
            return WithSrs(ToPoint(coordinate), source);
        }

        public IMultiPoint2D CreateMultiPoint(ReadOnlySpan<Coordinate> coordinates, IGeometry2D source)
        {
            return WithSrs(Fill(new MultiPoint(), coordinates), source);
        }

        public IPolyline2D CreatePolyline(ReadOnlySpan<ReadOnlyMemory<Coordinate>> paths, IGeometry2D source)
        {
            var polyline = new Polyline();
            foreach (var path in paths)
            {
                polyline.AddPath(Fill(new Path(), path.Span));
            }

            return WithSrs(polyline, source);
        }

        public IPolygon2D CreatePolygon(ReadOnlySpan<PolygonPart> parts, IGeometry2D source)
        {
            // Parts are already valid: shells become rings, holes become typed holes, no VerifyHoles() needed
            var polygon = new Polygon();
            foreach (var part in parts)
            {
                polygon.AddRing(Fill(new Ring(), part.Shell.Span));
                foreach (var hole in part.Holes.Span)
                {
                    polygon.AddRing(Fill(new Hole(), hole.Span));
                }
            }

            return WithSrs(polygon, source);
        }

        private static Point ToPoint(Coordinate coordinate)
        {
            return double.IsNaN(coordinate.Z)
                ? new Point(coordinate.X, coordinate.Y)
                : new Point(coordinate.X, coordinate.Y, coordinate.Z);
        }

        private static T Fill<T>(T collection, ReadOnlySpan<Coordinate> coordinates)
            where T : PointCollection
        {
            foreach (var coordinate in coordinates)
            {
                collection.AddPoint(ToPoint(coordinate));
            }

            return collection;
        }

        private static T WithSrs<T>(T geometry, IGeometry2D source)
            where T : IGeometry
        {
            if (source is IGeometry sourceGeometry)
            {
                geometry.Srs = sourceGeometry.Srs;
            }

            return geometry;
        }
    }
}
