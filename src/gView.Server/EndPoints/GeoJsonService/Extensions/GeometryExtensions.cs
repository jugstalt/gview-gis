#nullable enable

using gView.Framework.Core.Exceptions;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

static internal class GeometryExtensions
{
    static public double[]? ToBBox(this IEnvelope? envelope)
        => envelope is null
            ? null
            : [envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY];

    static public IEnvelope ToEnvelope(this double[] bbox)
        => bbox is not null && bbox.Length == 4
            ? new Envelope(bbox[0], bbox[1], bbox[2], bbox[3])
            : throw new MapServerException("Invalid bbox");
}
