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
            // command=statistics: statistics=count:OBJECTID:n,sum:EW (type:field[:alias]), groupBy=LAND, having (CQL2-Text)
            Statistics = List("statistics")?.Select(s => s.Split(':', StringSplitOptions.TrimEntries) switch
            {
                [var type, var field] => new StatisticRequest() { Type = type, Field = field },
                [var type, var field, var alias] => new StatisticRequest() { Type = type, Field = field, Alias = alias },
                _ => throw new MapServerException($"Invalid statistic '{s}': type:field[:alias], e.g. count:OBJECTID:n")
            }).ToArray(),
            GroupBy = List("groupBy"),
            Having = Value("having"),
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

    /// <summary>
    /// An identify from the url (GET .../identify), case-insensitive: geometry (x,y or a GeoJSON geometry), crs,
    /// layerOption (visible, top, all), layers, tolerance, mapExtent (minx,miny,maxx,maxy), imageWidth, imageHeight,
    /// dpi, returnGeometry, outCRS, limit
    /// </summary>
    static public IdentifyRequest ToIdentifyRequest(this IQueryCollection query)
    {
        string? Value(string name) => query[name].ToString() is string text && !String.IsNullOrWhiteSpace(text) ? text.Trim() : null;

        int? Integer(string name)
            => Value(name) is string text
                ? int.TryParse(text, out var value) && value >= 0
                    ? value
                    : throw new MapServerException($"Invalid value for the parameter {name}: {text}")
                : null;

        var request = new IdentifyRequest()
        {
            Geometry = Value("geometry") is string geometry ? GeometryOf(geometry) : null,
            CRS = Crs(Value("crs"), "crs"),
            LayerOption = Value("layerOption") ?? "visible",
            Layers = Value("layers")?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            Tolerance = Integer("tolerance") ?? 3,
            MapExtent = Value("mapExtent") is string extent ? BBoxOf(extent, "mapExtent") : null,
            ImageWidth = Integer("imageWidth") ?? 0,
            ImageHeight = Integer("imageHeight") ?? 0,
            Dpi = Integer("dpi"),
            OutCRS = Crs(Value("outCRS") ?? Value("out-crs"), "outCRS"),
            Limit = Integer("limit")
        };
        if (Value("returnGeometry") is string returnGeometry)
        {
            request.ReturnGeometry = Enum.TryParse<GeometryResult>(returnGeometry, true, out var value) && Enum.IsDefined(value)
                ? value
                : throw new MapServerException($"Invalid value for the parameter returnGeometry: {returnGeometry} ({String.Join(", ", Enum.GetNames<GeometryResult>())})");
        }

        return request;
    }

    /// <summary>x,y or a GeoJSON geometry</summary>
    private static Geometry GeometryOf(string text)
    {
        if (!text.StartsWith("{"))
        {
            var xy = text.Split(',', StringSplitOptions.TrimEntries);
            if (xy.Length != 2
                || !double.TryParse(xy[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var x)
                || !double.TryParse(xy[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var y))
            {
                throw new MapServerException($"Invalid geometry: {text} (x,y or a GeoJSON geometry)");
            }
            return new Geometry()
            {
                Type = GeometryType.Point,
                Coordinates = JsonElementOf($"[{x.ToString(System.Globalization.CultureInfo.InvariantCulture)},{y.ToString(System.Globalization.CultureInfo.InvariantCulture)}]")
            };
        }

        try
        {
            return gView.GeoJsonService.GeoJsonSerializer.Deserialize<Geometry>(text)
                ?? throw new MapServerException("Invalid geometry");
        }
        catch (JsonException ex)
        {
            throw new MapServerException($"Invalid geometry: {ex.Message}");
        }
    }

    private static BBox BBoxOf(string text, string name)
    {
        var parts = text.Split(',', StringSplitOptions.TrimEntries);
        var values = parts.Select(n => double.TryParse(n, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : double.NaN).ToArray();
        return values.Length == 4 && values.All(double.IsFinite)
            ? BBox.FromArray(values)
            : throw new MapServerException($"Invalid {name}: {text} (minx,miny,maxx,maxy)");
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
