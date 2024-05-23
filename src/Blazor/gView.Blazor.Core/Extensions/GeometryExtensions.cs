using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;

namespace gView.Blazor.Core.Extensions;

static public class GeometryExtensions
{
    static public IEnvelope? TryUnion(this IEnvelope? envelope, IEnvelope? candidate)
    {
        if (candidate is null)
        {
            return envelope;
        }

        if (envelope is null)
        {
            return candidate;
        }

        var union = new Envelope(envelope);
        union.Union(candidate);

        return union;
    }

    static public IEnvelope ToMinSize(this IEnvelope envelope, double minWidth = 2e-4, double minHeight = 2e-4)
    {
        var center = envelope.Center;

        if (envelope.Width < minWidth)
        {
            envelope.MinX = center.X - minWidth/2.0;
            envelope.MaxX = center.X + minWidth/2.0;
        }
        if (envelope.Height < minHeight)
        {
            envelope.MinY = center.Y - minHeight/2.0;
            envelope.MaxY = center.Y + minHeight/2.0;
        }

        return envelope;
    }
}
