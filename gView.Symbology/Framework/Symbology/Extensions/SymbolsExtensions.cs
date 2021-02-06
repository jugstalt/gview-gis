using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace gView.Symbology.Framework.Symbology.Extensions
{
    static class SymbolsExtensions
    {
        static public bool IntersectsOne(this IEnvelope envelope, IEnumerable<IEnvelope> envelopes)
        {
            if (envelope == null || envelopes == null)
                return false;

            return envelopes.Where(r => r.Intersects(envelope))
                             .FirstOrDefault() != null;
        }

        static public double MetersToDeg(this ISpatialReference sRef, double meters)
        {
            if (sRef != null &&
                sRef.SpatialParameters.IsGeographic)
            {
                meters = (180.0 * meters / Math.PI) / 6370000.0;
            }

            return meters;
        }
    }
}
