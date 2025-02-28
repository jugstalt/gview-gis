#nullable enable

using gView.Framework.Core.Data;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Framework.OGC.WFS.Version_1_1_0;
using gView.GeoJsonService.DTOs;
using Microsoft.SqlServer.Management.SqlParser.Metadata;

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

    private static ISpatialReference DefaultSpatialReference => SpatialReference.FromID("epsg:4326");
    static public ISpatialReference ToSpatialReferenceOrDefault(this CoordinateReferenceSystem? crs)
        => crs is not null
            ? SpatialReference.FromID(crs.ToSpatialReferenceName())
                ?? throw new MapServerException($"Can't create spatial reference system {crs.ToSpatialReferenceName()}")
            : DefaultSpatialReference;

    public static spatialRelation ToSpatialRelation(this SpatialOperator spatialOperator)
        => spatialOperator switch
        {
            SpatialOperator.Contains => spatialRelation.SpatialRelationContains,
            SpatialOperator.Within => spatialRelation.SpatialRelationWithin,
            SpatialOperator.IntersectsBBox => spatialRelation.SpatialRelationEnvelopeIntersects,
            _ => spatialRelation.SpatialRelationIntersects
        };

}
