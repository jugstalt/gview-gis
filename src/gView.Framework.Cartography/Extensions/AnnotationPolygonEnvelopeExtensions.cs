using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Geometry;

namespace gView.Framework.Cartography.Extensions;

static internal class AnnotationPolygonEnvelopeExtensions
{
    static public IEnvelope ToEnvelope(this AnnotationPolygonEnvelope env)
        => new Envelope(env.MinX, env.MinY, env.MaxX, env.MaxY);
}
