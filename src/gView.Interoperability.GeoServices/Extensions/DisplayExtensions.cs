using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry.GeoProcessing;

namespace gView.Interoperability.GeoServices.Extensions
{
    static class DisplayExtensions
    {
        static public void Image2World(this IDisplay display, IGeometry geometry)
        {
            if (geometry is IEnvelope)
            {
                IEnvelope envelope = (IEnvelope)geometry;

                double minX = envelope.MinX, minY = envelope.MinY, maxX = envelope.MaxX, maxY = envelope.MaxY;
                display.Image2World(ref minX, ref minY);
                display.Image2World(ref maxX, ref maxY);

                envelope.MinX = minX;
                envelope.MinY = minY;
                envelope.MaxX = maxX;
                envelope.MaxY = maxY;
            }
            else
            {
                var pointCollection = Algorithm.GeometryPoints(geometry, false);
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
