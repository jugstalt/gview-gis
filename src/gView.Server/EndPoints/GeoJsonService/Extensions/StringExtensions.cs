#nullable enable

using gView.Framework.Common;
using gView.GeoJsonService;
using gView.GeoJsonService.DTOs;
using System;
using System.Linq;
using System.Text.Json;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

internal static class StringExtensions
{
    static public object? ParseToObject(this string? parameterValue, Type targetType)
    {
        parameterValue = parameterValue?.Trim();
        if (String.IsNullOrEmpty(parameterValue))
        {
            return null;
        }

        return targetType switch
        {
            // bbox = 12.34,34.22,...
            Type t when t == typeof(double[]) && !parameterValue.StartsWith("[")
                => parameterValue.Split(',').Select(n => n.ToDouble()).ToArray(),

            // crs=epsg:4326
            Type t when t == typeof(CoordinateReferenceSystem) && !parameterValue.StartsWith("{")
                => CoordinateReferenceSystem.CreateByName(parameterValue),

            // parse json
            _ => JsonSerializer.Deserialize(parameterValue, targetType, GeoJsonSerializer.JsonDeserializerOptions)
        };
    }
}
