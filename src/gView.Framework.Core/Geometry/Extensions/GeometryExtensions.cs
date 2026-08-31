#nullable enable

namespace gView.Framework.Core.Geometry.Extensions
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

        /// <summary>
        /// The length of a line feature (sum of all path lengths) or the perimeter of a polygon
        /// feature (sum of all ring lengths, exterior and interior/holes) - <c>0</c> for any other
        /// geometry type or <see langword="null"/>. Planar (Euclidean, in the geometry's own
        /// native/unprojected coordinate units) - not geodesic.
        /// </summary>
        static public double GetLength(this IGeometry geometry)
        {
            switch (geometry)
            {
                case IPolyline polyline:
                    return polyline.Length; // already sums every path

                case IPolygon polygon:
                    double perimeter = 0.0;
                    foreach (IRing ring in polygon.Rings)
                    {
                        if (ring != null)
                        {
                            perimeter += ring.Length;
                        }
                    }
                    return perimeter;

                default:
                    return 0.0;
            }
        }

        /// <summary>
        /// The area of a polygon feature (holes already subtracted, see <see cref="IPolygon.Area"/>)
        /// - <c>0</c> for any other geometry type or <see langword="null"/>.
        /// </summary>
        static public double GetArea(this IGeometry geometry)
        {
            return geometry is IPolygon polygon ? polygon.Area : 0.0;
        }
    }
}
