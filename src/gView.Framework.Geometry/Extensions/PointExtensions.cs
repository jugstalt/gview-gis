using gView.Framework.Core.Geometry;
using System;

namespace gView.Framework.Geometry.Extensions;
static public class PointExtensions
{
    static public Point WebMercatorToGeographic(this IPoint point)
    => new Point(
            point.X / 20037508.34 * 180.0,
            180 / Math.PI * (2.0 * Math.Atan(
                Math.Exp(point.Y / 20037508.34 * Math.PI))
            - Math.PI / 2));
    
}
