using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Carto.Core.Extensions;

static public class GeometryExtensions
{
    static public IEnvelope? ToProjectedEnvelope(this IGeometry? geometry,
                                                 ICartoApplicationScopeService scope,
                                                 ISpatialReference sRef,
                                                 int clickPixelTolerance = 10)
    {
        if(geometry is null)
        {
            return null;
        }

        geometry = scope.GeoTransformer.FromWGS84(
                        geometry switch
                        {
                            IPoint p => new Point(p),  // clone the original geometry
                            _ => new Envelope(geometry.Envelope),
                        }, sRef, scope.Document?.Map?.Display);

        if (geometry is IPoint point)
        {
            double tol = clickPixelTolerance * scope.DisplayService.ScaleDominator / (scope.DisplayService.Dpi / 0.0254);  // [m]
            if (scope.DisplayService.SpatialReference?.SpatialParameters?.IsGeographic == true)
            {
                tol = (180.0 * tol / Math.PI) / 6370000.0;
            }

            geometry = new Envelope(
                point.X - tol, point.Y - tol,
                point.X + tol, point.Y + tol);
        }

        return geometry?.Envelope;
    }
}
