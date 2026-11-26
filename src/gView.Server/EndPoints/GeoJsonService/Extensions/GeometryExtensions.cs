#nullable enable

using System;
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
                ?? throw new MapServerException($"Can't create spatial reference system '{(crs.Properties.TryGetValue("name", out var name) ? name : crs.Type)}' (EPSG:code or code)")
            : DefaultSpatialReference;

    /// <summary>
    /// Coordinates of a request in a geographic CRS: a latitude beyond ±90° (e.g. projected coordinates with
    /// crs EPSG:4326) can't be projected and ended in an internal error (500); now it is a 400. Longitudes beyond ±180°
    /// stay allowed (maps across the date line).
    /// </summary>
    static public T CheckLatitudes<T>(this T geometry, ISpatialReference? sRef, string parameter) where T : IGeometry?
    {
        if (geometry?.Envelope is IEnvelope envelope && sRef?.SpatialParameters?.IsGeographic == true)
        {
            if (!double.IsFinite(envelope.MinY) || !double.IsFinite(envelope.MaxY) || envelope.MinY < -90 || envelope.MaxY > 90)
            {
                throw new MapServerException(
                    FormattableString.Invariant($"{parameter}: latitude out of range ({envelope.MinY:R} .. {envelope.MaxY:R}, allowed -90 .. 90 in {sRef.Name}). Projected coordinates need their crs."),
                    400);
            }
        }
        return geometry;
    }

    public static spatialRelation ToSpatialRelation(this SpatialOperator spatialOperator)
        => spatialOperator switch
        {
            SpatialOperator.Contains => spatialRelation.SpatialRelationContains,
            SpatialOperator.Within => spatialRelation.SpatialRelationWithin,
            SpatialOperator.IntersectsBBox => spatialRelation.SpatialRelationEnvelopeIntersects,
            _ => spatialRelation.SpatialRelationIntersects
        };

}
