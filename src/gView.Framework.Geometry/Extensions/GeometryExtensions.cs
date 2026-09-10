#nullable enable

using gView.Framework.Core.Geometry;

namespace gView.Framework.Geometry.Extensions;

public static class GeometryExtensions
{
    extension(IGeometry? geometry)
    {
        public bool IsNullOrEmptyGeometry()
            => geometry switch
            {
                null => true,
                // NULL Geoemtry for points is sumetimes stored as GEOMETRYCOLLECTION EMPTY (WKT)
                IAggregateGeometry agg when agg.GeometryCount == 0 => true,
                // also do the other types
                IPolyline polyline when polyline.PathCount == 0 => true,
                IPolygon polygon when polygon.RingCount == 0 => true,
                IMultiPoint multiPoint when multiPoint.PointCount == 0 => true,
                _ => false,
            };
    }
}
