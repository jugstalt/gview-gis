#nullable enable

using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.GeoJsonService.Extensions;
using gView.GeoJsonService.DTOs;
using System;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

internal static class FeatureExtensions
{
    /// <summary>
    /// A feature of a query or identify as GeoJSON: geometry (none, geometry or its bbox), bbox, the properties
    /// and the object id as oid
    /// </summary>
    static public gView.GeoJsonService.DTOs.Feature ToGeoJsonFeature(this IFeature feature, string? objectIdFieldName, GeometryResult returnGeometry)
    {
        var shape = returnGeometry switch
        {
            GeometryResult.None => null,
            GeometryResult.BBox => feature.Shape?.Envelope,
            _ => feature.Shape
        };

        var geoJsonFeature = new gView.GeoJsonService.DTOs.Feature()
        {
            Geometry = shape?.ToGeoJsonGeometry(),
            BBox = shape?.Envelope.ToBBox()
        };

        foreach (var field in feature.Fields)
        {
            if (field.Name == objectIdFieldName)
            {
                geoJsonFeature.Oid = field.Value;
            }

            geoJsonFeature.Properties[field.Name] = field.Value == DBNull.Value ? null : field.Value;
        }

        return geoJsonFeature;
    }
}
