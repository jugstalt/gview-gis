namespace gView.Framework.Geometry.Extensions
{
    static public class GeometryExtensions
    {
        static public GeometryType ToGeometryType(this IGeometry geometry)
        {
            if (geometry is IPoint)
            {
                return GeometryType.Point;
            }

            if (geometry is IMultiPoint)
            {
                return GeometryType.Multipoint;
            }

            if (geometry is IPolyline)
            {
                return GeometryType.Polyline;
            }

            if (geometry is IPolygon)
            {
                return GeometryType.Polygon;
            }

            if (geometry is IEnvelope)
            {
                return GeometryType.Envelope;
            }

            if (geometry is IAggregateGeometry)
            {
                return GeometryType.Aggregate;
            }

            return GeometryType.Unknown;
        }
    }
}
