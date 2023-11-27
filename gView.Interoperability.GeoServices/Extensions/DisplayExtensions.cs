using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;

namespace gView.Interoperability.GeoServices.Extensions
{
    static class DisplayExtensions
    {
        static public void Image2World(this IDisplay display, IGeometry geometry)
        {
            if (geometry is IEnvelope)
            {
                IEnvelope envelope = (IEnvelope)geometry;

                double minX = envelope.minx, minY = envelope.miny, maxX = envelope.maxx, maxY = envelope.maxy;
                display.Image2World(ref minX, ref minY);
                display.Image2World(ref maxX, ref maxY);

                envelope.minx = minX;
                envelope.miny = minY;
                envelope.maxx = maxX;
                envelope.maxy = maxY;
            }
            else
            {
                var pointCollection = gView.Framework.SpatialAlgorithms.Algorithm.GeometryPoints(geometry, false);
                for (int i = 0, to = pointCollection.PointCount; i < to; i++)
                {
                    var point = pointCollection[i];

                    double x = point.X, y = point.Y;
                    display.Image2World(ref x, ref y);
                    point.X = x;
                    point.Y = y;
                }
            }
        }
    }
}
