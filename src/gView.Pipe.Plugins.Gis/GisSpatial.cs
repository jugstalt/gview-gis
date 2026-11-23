using gView.Pipe.Abstraction;
using gView.Pipe.Abstraction.Data;
using SpatialNET;
using SpatialNET.Imp;
using SpatialNET.IO;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis;

/// <summary>
/// GeoJSON items for the spatial nodes, read and written with SpatialNET.IO; no gView geometry involved
/// (gView ADR 0004). The factory is passed explicitly: <c>SpatialNET.GeometryFactory.Default</c> belongs to the host.
/// </summary>
static internal class GisSpatial
{
    static public readonly IGeometryFactory Factory = ImpGeometryFactory.Instance;

    // geographic coordinate systems: distances in degrees make no sense
    static private readonly HashSet<int> GeographicSrs = [4326, 4258, 4269, 4267, 4283, 4312, 4314, 4230, 4618, 4674, 4937, 4979];

    /// <summary>The geometry of an item, or null when it has none.</summary>
    static public IGeometry2D? Geometry(PipeItem item) => Geometry(item.Json["geometry"]);

    static public IGeometry2D? Geometry(JsonNode? geoJson)
    {
        if (geoJson is null)
        {
            return null;
        }
        if (geoJson is JsonObject json && json["type"]?.ToString() == "GeometryCollection")
        {
            throw new PipeException("A GeometryCollection can't be used here; split it into features first");
        }

        try
        {
            var geometry = GeoJson.Read(geoJson.ToJsonString(), Factory);
            return IsEmpty(geometry) ? null : geometry;
        }
        catch (Exception ex) when (ex is FormatException or NotSupportedException or JsonException)
        {
            throw new PipeException($"Invalid GeoJSON geometry: {ex.Message}");
        }
    }

    static public JsonNode? ToJson(IGeometry2D? geometry)
        => geometry is null || IsEmpty(geometry) ? null : JsonNode.Parse(geometry.ToGeoJson());

    static public bool IsEmpty(IGeometry2D? geometry) => geometry switch
    {
        null => true,
        IPolygon2D polygon => polygon.RingCount == 0,
        IPolyline2D polyline => polyline.PathCount == 0,
        IMultiPoint2D points => points.PointCount == 0,
        _ => false
    };

    static public IPoint2D Point(Coordinate coordinate) => Factory.CreatePoint(coordinate, null);

    /// <summary>All vertices of a geometry, e.g. as sites of a Voronoi diagram or for a hull.</summary>
    static public IEnumerable<Coordinate> Coordinates(IGeometry2D geometry)
    {
        IEnumerable<IPointCollection2D> parts = geometry switch
        {
            IPolygon2D polygon => Enumerable.Range(0, polygon.RingCount).Select(i => (IPointCollection2D)polygon[i]),
            IPolyline2D polyline => Enumerable.Range(0, polyline.PathCount).Select(i => (IPointCollection2D)polyline[i]),
            IMultiPoint2D points => [points],
            _ => []
        };
        if (geometry is IPoint2D point)
        {
            yield return new Coordinate(point.X, point.Y);
        }
        foreach (var part in parts)
        {
            for (var i = 0; i < part.PointCount; i++)
            {
                yield return new Coordinate(part[i].X, part[i].Y);
            }
        }
    }

    static public IMultiPoint2D MultiPoint(IEnumerable<Coordinate> coordinates)
        => Factory.CreateMultiPoint(coordinates.Distinct().ToArray(), null);

    /// <summary>The srs of an item (EPSG code), or null.</summary>
    static public int? Srs(PipeItem item) => GisSpatialReferences.ToEpsg(item.Json["srs"]);

    /// <summary>The common srs of items; an error when they differ.</summary>
    static public int? CommonSrs(IEnumerable<PipeItem> items, string what = "The items")
    {
        var codes = items.Select(Srs).Where(s => s is not null).Distinct().ToList();
        return codes.Count switch
        {
            0 => null,
            1 => codes[0],
            _ => throw new PipeException($"{what} have different coordinate systems ({string.Join(", ", codes.Select(c => $"EPSG:{c}"))}); project them first, e.g. with Target SRS of Read Features")
        };
    }

    /// <summary>Stops operations with distances or areas on geographic coordinates (degrees).</summary>
    static public void RequireProjected(int? srs, string operation)
    {
        if (srs is { } code && GeographicSrs.Contains(code))
        {
            throw new PipeException($"{operation} needs a projected coordinate system, the items are in EPSG:{code} (degrees); read them with a Target SRS in meters, e.g. 31256 or 3857");
        }
    }

    static public double Number(JsonNode? value, string name)
    {
        var text = value?.ToString().Trim() ?? "";
        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) && double.IsFinite(number)
            ? number
            : throw new PipeException($"{name} must be a number, not '{text}'");
    }

    /// <summary>A feature item with a new geometry; the other members stay.</summary>
    static public PipeItem WithGeometry(PipeItem item, IGeometry2D? geometry)
    {
        var result = item.Clone();
        result.Json["geometry"] = ToJson(geometry);
        return result;
    }

    static public PipeItem Feature(IGeometry2D? geometry, JsonObject properties, int? srs)
        => new(new JsonObject
        {
            ["type"] = "Feature",
            ["geometry"] = ToJson(geometry),
            ["properties"] = properties,
            ["srs"] = srs
        });

    static public JsonObject Properties(PipeItem item)
        => item.Json["properties"] as JsonObject ?? new JsonObject();
}
