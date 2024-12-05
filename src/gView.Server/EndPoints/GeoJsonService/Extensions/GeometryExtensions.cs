#nullable enable

using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.GeoJsonService.DTOs;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

static internal class GeometryExtensions
{
    static public BBox? ToBBox(this IEnvelope? envelope)
        => envelope is null
            ? null
            : new BBox()
            {
                MinX = envelope.MinX,
                MinY = envelope.MinY,
                MaxX = envelope.MaxX,
                MaxY = envelope.MaxY
            };

    static public IEnvelope? ToEnvelope(this BBox? bbox)
        => bbox is not null
            ? new Envelope(bbox.MinX, bbox.MinY, bbox.MaxX, bbox.MaxY)
            : null;
}
