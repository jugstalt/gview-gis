#nullable enable

using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.GeoJsonService.Extensions;
using gView.Framework.Geometry;
using gView.Framework.Geometry.GeoProcessing;
using gView.GeoJsonService.DTOs;
using System;
using System.Linq;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

/// <summary>How the geometries of a result are written (query, identify, find)</summary>
/// <param name="MaxAllowableOffset">generalize lines and polygons: vertices closer than this (in output units) are dropped</param>
/// <param name="Precision">round the coordinates to this number of decimal places</param>
internal sealed record GeometryOptions(double? MaxAllowableOffset = null, int? Precision = null)
{
    public static readonly GeometryOptions None = new();
}

internal static class FeatureExtensions
{
    /// <summary>
    /// A feature of a query or identify as GeoJSON: geometry (none, geometry, its bbox or its centroid), bbox, the
    /// properties and the object id as oid
    /// </summary>
    static public gView.GeoJsonService.DTOs.Feature ToGeoJsonFeature(this IFeature feature, string? objectIdFieldName,
                                                                     GeometryResult returnGeometry, GeometryOptions? options = null)
    {
        var shape = feature.Shape.Shaped(returnGeometry, options ?? GeometryOptions.None);

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

    static private IGeometry? Shaped(this IGeometry? shape, GeometryResult returnGeometry, GeometryOptions options)
    {
        if (shape is null || returnGeometry == GeometryResult.None)
        {
            return null;
        }

        if (returnGeometry == GeometryResult.BBox)
        {
            return shape.Envelope;
        }

        if (returnGeometry == GeometryResult.Centroid)
        {
            shape = shape.Centroid();
        }
        else if (options.MaxAllowableOffset is > 0 && shape is IPolyline or IPolygon)
        {
            shape = Algorithm.Generalize(shape, options.MaxAllowableOffset.Value) ?? shape;
        }

        if (options.Precision is int digits && shape is not null)
        {
            shape.RoundCoordinates(Math.Clamp(digits, 0, 15));
        }

        return shape;
    }

    /// <summary>A point that stands for the geometry: inside a polygon, at half the length of a line, else the center of the envelope</summary>
    static private IGeometry? Centroid(this IGeometry shape)
    {
        if (shape is IPoint)
        {
            return shape;
        }
        if (shape is IPolygon polygon && Algorithm.PolygonLabelPoints(polygon) is { PointCount: > 0 } labelPoints)
        {
            return new Point(labelPoints[0].X, labelPoints[0].Y);
        }
        if (shape is IPolyline polyline && polyline.PathCount > 0 && polyline[0].PointCount > 1)
        {
            var path = polyline[0];
            double length = 0;
            for (int i = 1; i < path.PointCount; i++)
            {
                length += Distance(path[i - 1], path[i]);
            }

            double half = length / 2, walked = 0;
            for (int i = 1; i < path.PointCount; i++)
            {
                double segment = Distance(path[i - 1], path[i]);
                if (walked + segment >= half && segment > 0)
                {
                    double t = (half - walked) / segment;
                    return new Point(path[i - 1].X + t * (path[i].X - path[i - 1].X), path[i - 1].Y + t * (path[i].Y - path[i - 1].Y));
                }
                walked += segment;
            }
        }

        var envelope = shape.Envelope;
        return envelope is null ? null : new Point((envelope.MinX + envelope.MaxX) / 2, (envelope.MinY + envelope.MaxY) / 2);
    }

    static private double Distance(IPoint a, IPoint b) => Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

    static private void RoundCoordinates(this IGeometry shape, int digits)
    {
        void Round(IPoint point)
        {
            point.X = Math.Round(point.X, digits);
            point.Y = Math.Round(point.Y, digits);
        }

        switch (shape)
        {
            case IPoint point:
                Round(point);
                break;
            case IPolygon polygon:
                for (int r = 0; r < polygon.RingCount; r++)
                {
                    foreach (var point in polygon[r]) Round(point);
                }
                break;
            case IPolyline polyline:
                for (int p = 0; p < polyline.PathCount; p++)
                {
                    foreach (var point in polyline[p]) Round(point);
                }
                break;
            case IPointCollection points:
                foreach (var point in points) Round(point);
                break;
        }
    }
}
