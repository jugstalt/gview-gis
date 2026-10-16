using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using System.Text.Json;

namespace gView.Interoperability.OgcApi.GeoJson;

/// <summary>
/// A GeoJSON feature from a request body (Part 4 create/replace/update)
/// </summary>
internal sealed class GeoJsonFeature
{
    /// <summary>
    /// The body contains a "geometry" member (it may be null)
    /// </summary>
    public bool HasGeometry { get; init; }
    public IGeometry? Geometry { get; init; }

    /// <summary>
    /// The "properties" member, null if the body has no properties
    /// </summary>
    public IReadOnlyDictionary<string, JsonElement>? Properties { get; init; }
}

/// <summary>
/// Reads RFC 7946 GeoJSON features into gView geometries
/// </summary>
internal static class GeoJsonReader
{
    public static GeoJsonFeature ReadFeature(string json, bool swapAxes)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw OgcApiException.BadRequest($"Invalid GeoJSON: {ex.Message}");
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("type", out var type)
                || type.ValueKind != JsonValueKind.String
                || type.GetString() != "Feature")
            {
                throw OgcApiException.BadRequest("The body must be a GeoJSON Feature");
            }

            bool hasGeometry = root.TryGetProperty("geometry", out var geometry);

            Dictionary<string, JsonElement>? properties = null;
            if (root.TryGetProperty("properties", out var propertiesElement))
            {
                if (propertiesElement.ValueKind == JsonValueKind.Object)
                {
                    properties = propertiesElement
                        .EnumerateObject()
                        .ToDictionary(p => p.Name, p => p.Value.Clone());
                }
                else if (propertiesElement.ValueKind != JsonValueKind.Null)
                {
                    throw OgcApiException.BadRequest("properties must be an object");
                }
            }

            return new GeoJsonFeature()
            {
                HasGeometry = hasGeometry,
                Geometry = hasGeometry ? ReadGeometry(geometry, swapAxes) : null,
                Properties = properties
            };
        }
    }

    public static IGeometry? ReadGeometry(JsonElement geometry, bool swapAxes)
    {
        if (geometry.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (geometry.ValueKind != JsonValueKind.Object
            || !geometry.TryGetProperty("type", out var typeElement)
            || typeElement.ValueKind != JsonValueKind.String)
        {
            throw OgcApiException.BadRequest("Invalid GeoJSON geometry");
        }

        string type = typeElement.GetString()!;
        if (!geometry.TryGetProperty("coordinates", out var coordinates) || coordinates.ValueKind != JsonValueKind.Array)
        {
            throw OgcApiException.BadRequest($"Invalid GeoJSON {type}: coordinates missing");
        }

        switch (type)
        {
            case "Point":
                return ReadPoint(coordinates, swapAxes);

            case "MultiPoint":
                var multiPoint = new MultiPoint();
                foreach (var position in coordinates.EnumerateArray())
                {
                    multiPoint.AddPoint(ReadPoint(position, swapAxes));
                }
                return multiPoint;

            case "LineString":
                var line = new Polyline();
                line.AddPath(ReadPath(coordinates, swapAxes));
                return line;

            case "MultiLineString":
                var multiLine = new Polyline();
                foreach (var path in coordinates.EnumerateArray())
                {
                    multiLine.AddPath(ReadPath(path, swapAxes));
                }
                return multiLine;

            case "Polygon":
                var polygon = new Polygon();
                AddRings(polygon, coordinates, swapAxes);
                return polygon;

            case "MultiPolygon":
                var multiPolygon = new Polygon();
                foreach (var part in coordinates.EnumerateArray())
                {
                    AddRings(multiPolygon, part, swapAxes);
                }
                return multiPolygon;

            default:
                throw OgcApiException.BadRequest($"Unsupported GeoJSON geometry type '{type}'");
        }
    }

    #region Helper

    private static Point ReadPoint(JsonElement position, bool swapAxes)
    {
        if (position.ValueKind != JsonValueKind.Array || position.GetArrayLength() < 2
            || position[0].ValueKind != JsonValueKind.Number || position[1].ValueKind != JsonValueKind.Number)
        {
            throw OgcApiException.BadRequest("Invalid GeoJSON position");
        }

        double a = position[0].GetDouble(), b = position[1].GetDouble();
        return swapAxes ? new Point(b, a) : new Point(a, b);
    }

    private static gView.Framework.Geometry.Path ReadPath(JsonElement positions, bool swapAxes)
    {
        if (positions.ValueKind != JsonValueKind.Array)
        {
            throw OgcApiException.BadRequest("Invalid GeoJSON coordinates");
        }

        var path = new gView.Framework.Geometry.Path();
        foreach (var position in positions.EnumerateArray())
        {
            path.AddPoint(ReadPoint(position, swapAxes));
        }
        return path;
    }

    private static void AddRings(Polygon polygon, JsonElement rings, bool swapAxes)
    {
        if (rings.ValueKind != JsonValueKind.Array)
        {
            throw OgcApiException.BadRequest("Invalid GeoJSON coordinates");
        }

        foreach (var positions in rings.EnumerateArray())
        {
            var ring = new Ring();
            foreach (var position in positions.EnumerateArray())
            {
                ring.AddPoint(ReadPoint(position, swapAxes));
            }
            polygon.AddRing(ring);
        }
    }

    #endregion
}
