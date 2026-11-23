using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Pipe.Abstraction;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using G = gView.Framework.Geometry;

namespace gView.Pipe.Plugins.Gis;

/// <summary>
/// gView features and geometries to GeoJSON items and back (gview-pipe ADR 0044):
/// one item is one feature, <c>{ type, id, geometry, properties, srs }</c>. The geometry stays in the
/// coordinate system named by <c>srs</c> (an EPSG code), not WGS 84 as RFC 7946 would have it. 2D only.
/// </summary>
static internal class GisGeoJson
{
    #region Feature → item

    static public JsonObject ToItem(IFeature feature, IFeatureClass featureClass, int? srs)
    {
        var properties = new JsonObject();
        foreach (var field in feature.Fields)
        {
            if (field.Name.Equals(featureClass.ShapeFieldName, StringComparison.OrdinalIgnoreCase)
                || field.Name.Equals(featureClass.IDFieldName, StringComparison.OrdinalIgnoreCase)
                || featureClass.FindField(field.Name) is null   // internal columns, e.g. FDB_NID of a gView FDB
                || field.Value is byte[])   // blobs are left out
            {
                continue;
            }

            properties[field.Name] = ToJson(field.Value);
        }

        return new JsonObject
        {
            ["type"] = "Feature",
            ["id"] = feature.OID,
            ["geometry"] = feature.Shape is null ? null : ToGeoJson(feature.Shape),
            ["properties"] = properties,
            ["srs"] = srs
        };
    }

    static public JsonNode? ToJson(object? value) => value switch
    {
        null or DBNull => null,
        string s => s,
        bool b => b,
        byte or short or int or long or sbyte or ushort or uint => JsonValue.Create(Convert.ToInt64(value, CultureInfo.InvariantCulture)),
        ulong ul => JsonValue.Create((decimal)ul),
        decimal d => d,
        float or double => JsonValue.Create(Convert.ToDouble(value, CultureInfo.InvariantCulture)),
        DateTime dt => dt.ToString("O"),
        DateTimeOffset dto => dto.ToString("O"),
        DateOnly date => date.ToString("O"),
        TimeOnly time => time.ToString("O"),
        TimeSpan span => span.ToString("c"),
        Guid guid => guid.ToString(),
        _ => Convert.ToString(value, CultureInfo.InvariantCulture)
    };

    static public JsonObject? ToGeoJson(IGeometry geometry)
    {
        switch (geometry)
        {
            case IPoint point:
                return Geometry("Point", Coordinates(point));
            case IMultiPoint multiPoint:
                return Geometry("MultiPoint", Coordinates((IPointCollection)multiPoint));
            case IPolyline polyline when polyline.PathCount == 1:
                return Geometry("LineString", Coordinates(polyline[0]));
            case IPolyline polyline when polyline.PathCount > 1:
                return Geometry("MultiLineString", new JsonArray(Enumerable.Range(0, polyline.PathCount).Select(i => (JsonNode?)Coordinates(polyline[i])).ToArray()));
            case IPolygon polygon when polygon.RingCount > 0:
                var parts = polygon.OuterRings()
                    .Select(outer => (JsonNode?)new JsonArray(new[] { outer }.Concat<IRing>(polygon.InnerRings(outer)).Select(ring => (JsonNode?)ClosedRing(ring)).ToArray()))
                    .ToArray();
                return parts.Length == 1
                    ? Geometry("Polygon", (JsonArray)parts[0]!)
                    : Geometry("MultiPolygon", new JsonArray(parts));
            case IEnvelope envelope:
                return ToGeoJson(new G.Polygon(new G.Ring(new List<IPoint>
                {
                    new G.Point(envelope.MinX, envelope.MinY), new G.Point(envelope.MaxX, envelope.MinY),
                    new G.Point(envelope.MaxX, envelope.MaxY), new G.Point(envelope.MinX, envelope.MaxY)
                })));
            case IAggregateGeometry aggregate:
                return new JsonObject
                {
                    ["type"] = "GeometryCollection",
                    ["geometries"] = new JsonArray(aggregate.Select(g => (JsonNode?)ToGeoJson(g)).Where(g => g is not null).ToArray())
                };
            default:
                return null;   // empty polylines and polygons
        }
    }

    static private JsonObject Geometry(string type, JsonArray coordinates)
        => new() { ["type"] = type, ["coordinates"] = coordinates };

    static private JsonArray Coordinates(IPoint point) => new(point.X, point.Y);

    static private JsonArray Coordinates(IPointCollection points)
        => new(Enumerable.Range(0, points.PointCount).Select(i => (JsonNode?)Coordinates(points[i])).ToArray());

    // GeoJSON rings repeat their first point at the end
    static private JsonArray ClosedRing(IRing ring)
    {
        var coordinates = Coordinates(ring);
        if (ring.PointCount > 0 && (ring[0].X != ring[ring.PointCount - 1].X || ring[0].Y != ring[ring.PointCount - 1].Y))
        {
            coordinates.Add(Coordinates(ring[0]));
        }
        return coordinates;
    }

    #endregion

    #region GeoJSON → geometry

    static public IGeometry? ToGeometry(JsonNode? geoJson)
    {
        if (geoJson is null)
        {
            return null;
        }
        if (geoJson is not JsonObject json || json["type"]?.GetValue<string>() is not { } type)
        {
            throw new PipeException("A geometry must be a GeoJSON object with a 'type'");
        }
        // a whole feature: take its geometry
        if (type == "Feature")
        {
            return ToGeometry(json["geometry"]);
        }

        var coordinates = json["coordinates"];
        switch (type)
        {
            case "Point":
                return ToPoint(coordinates);
            case "MultiPoint":
                return new G.MultiPoint(ToPoints(coordinates));
            case "LineString":
                return new G.Polyline(new G.Path(ToPoints(coordinates)));
            case "MultiLineString":
                return new G.Polyline(Array(coordinates).Select(path => (IPath)new G.Path(ToPoints(path))).ToList());
            case "Polygon":
                return ToPolygon([coordinates]);
            case "MultiPolygon":
                return ToPolygon(Array(coordinates));
            case "GeometryCollection":
                var aggregate = new G.AggregateGeometry();
                foreach (var geometry in Array(json["geometries"]))
                {
                    if (ToGeometry(geometry) is { } part)
                    {
                        aggregate.AddGeometry(part);
                    }
                }
                return aggregate;
            default:
                throw new PipeException($"Unknown GeoJSON geometry type '{type}'");
        }
    }

    static private IPolygon ToPolygon(IEnumerable<JsonNode?> polygons)
    {
        var polygon = new G.Polygon();
        foreach (var rings in polygons)
        {
            var first = true;
            foreach (var ring in Array(rings))
            {
                var points = ToPoints(ring);
                polygon.AddRing(first ? new G.Ring(points) : new G.Hole(points));
                first = false;
            }
        }
        return polygon;
    }

    static private G.Point ToPoint(JsonNode? coordinates)
    {
        var values = Array(coordinates);
        if (values.Count < 2)
        {
            throw new PipeException("A GeoJSON position needs x and y");
        }
        return new G.Point(Number(values[0]), Number(values[1]));
    }

    static private List<IPoint> ToPoints(JsonNode? coordinates)
        => Array(coordinates).Select(position => (IPoint)ToPoint(position)).ToList();

    static private JsonArray Array(JsonNode? node)
        => node as JsonArray ?? throw new PipeException("GeoJSON coordinates must be arrays");

    static private double Number(JsonNode? node)
        => node is JsonValue value && value.GetValueKind() == JsonValueKind.Number
            ? double.Parse(value.ToJsonString(), CultureInfo.InvariantCulture)
            : throw new PipeException("GeoJSON coordinates must be numbers");

    #endregion

    #region Values for gView

    /// <summary>A JSON value as field value: numbers, text, true/false; objects and arrays as JSON text.</summary>
    static public object? ToFieldValue(JsonNode? value) => value switch
    {
        null => null,
        JsonValue v => v.GetValueKind() switch
        {
            JsonValueKind.String => v.GetValue<string>(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            // a value built in code holds int, double, … and parsed JSON a JsonElement: read numbers from their text
            JsonValueKind.Number when long.TryParse(v.ToJsonString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer) => integer,
            JsonValueKind.Number => double.Parse(v.ToJsonString(), CultureInfo.InvariantCulture),
            _ => v.ToJsonString()
        },
        _ => value.ToJsonString()
    };

    /// <summary>
    /// A value as SQL literal for a where clause built by the node (key and match filters).
    /// gView filters take no parameters, so only numbers and quoted text are allowed.
    /// </summary>
    static public string ToSqlLiteral(JsonNode? value) => (value is null or JsonValue ? ToFieldValue(value) : value) switch
    {
        long integer => integer.ToString(CultureInfo.InvariantCulture),
        double number when double.IsFinite(number) => number.ToString("R", CultureInfo.InvariantCulture),
        string text when !text.Any(char.IsControl) => $"'{text.Replace("'", "''")}'",
        string => throw new PipeException("A key value must not contain control characters"),
        null => throw new PipeException("A key value is empty"),
        _ => throw new PipeException($"A key value must be a number or text, not {value!.ToJsonString()}")
    };

    /// <summary>A field name for a where clause built by the node: letters, digits and underscores only.</summary>
    static public string ToSqlField(IFeatureClass featureClass, string name)
    {
        var field = featureClass.FindField(name.Trim())
            ?? throw new PipeException($"The feature class '{featureClass.Name}' has no field '{name}'");
        if (!field.name.All(c => char.IsLetterOrDigit(c) || c == '_'))
        {
            throw new PipeException($"The field name '{field.name}' can't be used as key");
        }
        return field.name;
    }

    #endregion
}
