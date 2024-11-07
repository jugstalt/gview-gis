using gView.Framework.Core.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace gView.Framework.Geometry.Extensions;
static public class EnvelopeExtensions
{
    static public IEnvelope ToUnion(this IEnumerable<IEnvelope> envelopes)
    {
        envelopes = envelopes?.Where(e => e is not null).ToArray();

        if (envelopes?.Any() != true)
        {
            return null;
        }

        var result = new Envelope(envelopes.First());

        foreach (var envelope in envelopes.Skip(1))
        {
            result.Union(envelope);
        }

        return result;
    }
}
