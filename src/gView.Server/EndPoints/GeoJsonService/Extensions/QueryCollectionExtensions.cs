#nullable enable

using gView.Framework.Common;
using gView.Framework.Core.Exceptions;
using gView.GeoJsonService.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Text.Json;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

internal static class QueryCollectionExtensions
{
    /// <summary>
    /// A query from the url (GET .../query/{layerId}), case-insensitive:
    /// command, outFields (default *), returnGeometry, outCRS, orderBy (or orderByFields), objectIds,
    /// where (SQL), filter + filter-lang (cql2-text, default, or cql2-json), bbox (minx,miny,maxx,maxy) + bbox-crs,
    /// spatialOperator, limit, offset. Lists are comma separated, a CRS is EPSG:code or the code.
    /// </summary>
    static public GetFeaturesRequest ToGetFeaturesRequest(this IQueryCollection query)
    {
        string? Value(params string[] names)
            => names.Select(name => query[name].ToString()).FirstOrDefault(value => !String.IsNullOrWhiteSpace(value))?.Trim();

        string[]? List(params string[] names)
            => Value(names)?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        T EnumValue<T>(string name, T defaultValue) where T : struct, Enum
            => Value(name) is string text
                ? Enum.TryParse<T>(text, true, out var value) && Enum.IsDefined(value)
                    ? value
                    : throw new MapServerException($"Invalid value for the parameter {name}: {text} ({String.Join(", ", Enum.GetNames<T>())})")
                : defaultValue;

        int? Integer(string name)
            => Value(name) is string text
                ? int.TryParse(text, out var value) && value >= 0
                    ? value
                    : throw new MapServerException($"Invalid value for the parameter {name}: {text}")
                : null;

        var request = new GetFeaturesRequest()
        {
            Command = EnumValue("command", QueryCommand.Select),
            OutFields = List("outFields") ?? ["*"],
            ReturnGeometry = EnumValue("returnGeometry", GeometryResult.None),
            OutCRS = Crs(Value("outCRS", "out-crs"), "outCRS"),
            OrderByFields = List("orderBy", "orderByFields"),
            ObjectIds = List("objectIds"),
            Limit = Integer("limit"),
            Offset = Integer("offset"),
        };

        var where = Value("where");
        var filter = Value("filter");
        if (where is not null || filter is not null)
        {
            request.Filter = new AttributeFilter()
            {
                WhereClause = where,
                Cql2 = filter is null
                    ? null
                    : (Value("filter-lang")?.ToLowerInvariant() ?? "cql2-text") switch
                    {
                        "cql2-text" => filter,
                        "cql2-json" => JsonElementOf(filter),
                        var lang => throw new MapServerException($"Unsupported filter-lang '{lang}': cql2-text or cql2-json")
                    }
            };
        }

        if (List("bbox") is string[] bbox)
        {
            if (bbox.Length != 4 || bbox.Any(n => !double.TryParse(n, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _)))
            {
                throw new MapServerException($"Invalid bbox: {Value("bbox")} (minx,miny,maxx,maxy)");
            }
            request.SpatialFilter = new SpatialFilter()
            {
                BBox = BBox.FromArray(bbox.Select(n => n.ToDouble()).ToArray()),
                CRS = Crs(Value("bbox-crs", "bboxCRS"), "bbox-crs"),
                Operator = EnumValue("spatialOperator", SpatialOperator.Intersects)
            };
        }

        return request;
    }

    private static CoordinateReferenceSystem? Crs(string? value, string name)
        => value switch
        {
            null => null,
            _ when int.TryParse(value, out var code) => CoordinateReferenceSystem.CreateByName($"EPSG:{code}"),
            _ when value.StartsWith("EPSG:", StringComparison.OrdinalIgnoreCase) && int.TryParse(value[5..], out var code)
                => CoordinateReferenceSystem.CreateByName($"EPSG:{code}"),
            _ => throw new MapServerException($"Invalid value for the parameter {name}: {value} (EPSG:code or code)")
        };

    private static JsonElement JsonElementOf(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.Clone();
        }
        catch (JsonException ex)
        {
            throw new MapServerException($"Invalid CQL2-JSON filter: {ex.Message}");
        }
    }
}
