using SpatialNET;
using SpatialNET.IO;
using SpatialNET.Imp;
using System.Globalization;
using System.Text.Json;

namespace gView.Framework.NetworkModel.Query;

public enum NetworkAreaKind
{
    Box,
    Polygon,
    Shards
}

/// <summary>
/// The part of a network a search or a validation is restricted to: a box, polygons or shards (stations).
/// <see cref="Parse"/> takes "xmin,ymin,xmax,ymax", GeoJSON (Polygon, MultiPolygon, Feature, FeatureCollection),
/// Esri JSON (rings or an envelope), WKT (POLYGON, MULTIPOLYGON); GeoJSON and WKT are read with SpatialNET.IO or "shard:key1,key2".
/// Coordinates are in the spatial reference of the network (<see cref="Transform"/> projects them).
/// </summary>
public sealed class NetworkArea
{
    private NetworkArea(NetworkAreaKind kind, IReadOnlyList<IReadOnlyList<Point[]>> polygons, IReadOnlyList<string> shardKeys)
    {
        Kind = kind;
        Polygons = polygons;
        ShardKeys = shardKeys;
        if (polygons.Count > 0)
        {
            var points = polygons.SelectMany(p => p).SelectMany(r => r).ToList();
            (MinX, MinY, MaxX, MaxY) = (points.Min(p => p.X), points.Min(p => p.Y), points.Max(p => p.X), points.Max(p => p.Y));
        }
    }

    public readonly record struct Point(double X, double Y);

    public NetworkAreaKind Kind { get; }

    /// <summary>Box and Polygon: the polygons, each a list of rings (outer ring and holes, even-odd); a box is one polygon.</summary>
    public IReadOnlyList<IReadOnlyList<Point[]>> Polygons { get; }

    /// <summary>Shards: the shard keys (e.g. the station numbers).</summary>
    public IReadOnlyList<string> ShardKeys { get; }

    public double MinX { get; }
    public double MinY { get; }
    public double MaxX { get; }
    public double MaxY { get; }

    /// <summary>For the messages, e.g. "polygon (2 part(s))", "box 10,20,30,40", "shard 4711, 4712".</summary>
    public string Description => Kind switch
    {
        NetworkAreaKind.Box => $"box {Format(MinX)},{Format(MinY)},{Format(MaxX)},{Format(MaxY)}",
        NetworkAreaKind.Polygon => $"polygon ({Polygons.Count} part(s), {Polygons.Sum(p => p.Count)} ring(s))",
        _ => $"shard {string.Join(", ", ShardKeys)}"
    };

    public static NetworkArea Box(double minX, double minY, double maxX, double maxY)
    {
        if (!(maxX >= minX) || !(maxY >= minY))
        {
            throw new ArgumentException("area: the box needs xmin <= xmax and ymin <= ymax.");
        }
        return new NetworkArea(NetworkAreaKind.Box, [[[new(minX, minY), new(maxX, minY), new(maxX, maxY), new(minX, maxY), new(minX, minY)]]], []);
    }

    public static NetworkArea FromPolygons(IEnumerable<IEnumerable<IEnumerable<Point>>> polygons)
    {
        var list = polygons
            .Select(p => (IReadOnlyList<Point[]>)p.Select(r => r.ToArray()).Where(r => r.Length >= 3).ToList())
            .Where(p => p.Count > 0)
            .ToList();
        return list.Count == 0
            ? throw new ArgumentException("area: the polygon has no ring with at least 3 points.")
            : new NetworkArea(NetworkAreaKind.Polygon, list, []);
    }

    public static NetworkArea FromShards(IEnumerable<string> keys)
    {
        var list = keys.Select(k => k.Trim()).Where(k => k.Length > 0).Distinct().ToList();
        return list.Count == 0 ? throw new ArgumentException("area: shard: needs at least one key.") : new NetworkArea(NetworkAreaKind.Shards, [], list);
    }

    /// <summary>Reads an area; the format is recognized by its text (see <see cref="NetworkArea"/>).</summary>
    public static NetworkArea Parse(string text)
    {
        var value = text.Trim();
        if (value.Length == 0)
        {
            throw new ArgumentException("area is empty.");
        }
        if (value.StartsWith("shard:", StringComparison.OrdinalIgnoreCase))
        {
            return FromShards(value[6..].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }
        if (value.StartsWith('{'))
        {
            return ParseJson(value);
        }
        if (char.IsLetter(value[0]))
        {
            return FromGeometries(Read(() => Wkt.ReadAll(value, Factory), "WKT"));
        }
        var numbers = value.Split(',', StringSplitOptions.TrimEntries);
        if (numbers.Length == 4 && numbers.All(n => double.TryParse(n, NumberStyles.Float, CultureInfo.InvariantCulture, out _)))
        {
            var v = numbers.Select(n => double.Parse(n, NumberStyles.Float, CultureInfo.InvariantCulture)).ToArray();
            return Box(v[0], v[1], v[2], v[3]);
        }
        throw new ArgumentException("area: use xmin,ymin,xmax,ymax, GeoJSON or Esri JSON, WKT (POLYGON, MULTIPOLYGON) or shard:key1,key2.");
    }

    /// <summary>Projects the coordinates (e.g. from the spatial reference of a request into that of the network); a box becomes a polygon.</summary>
    public NetworkArea Transform(Func<double, double, (double X, double Y)> transform)
    {
        if (Kind == NetworkAreaKind.Shards)
        {
            return this;
        }
        return FromPolygons(Polygons.Select(p => p.Select(r => r.Select(pt =>
        {
            var (x, y) = transform(pt.X, pt.Y);
            return new Point(x, y);
        }))));
    }

    /// <summary>Whether the point is inside one of the polygons or at most <paramref name="tolerance"/> from a ring (on the border counts as inside).</summary>
    public bool Contains(double x, double y, double tolerance = 0)
    {
        if (Kind == NetworkAreaKind.Shards
            || x < MinX - tolerance || x > MaxX + tolerance || y < MinY - tolerance || y > MaxY + tolerance)
        {
            return false;
        }
        foreach (var polygon in Polygons)
        {
            bool inside = false;
            foreach (var ring in polygon)
            {
                for (int i = 0, j = ring.Length - 1; i < ring.Length; j = i++)
                {
                    var (a, b) = (ring[i], ring[j]);
                    if ((a.Y > y) != (b.Y > y) && x < (b.X - a.X) * (y - a.Y) / (b.Y - a.Y) + a.X)
                    {
                        inside = !inside;
                    }
                }
            }
            if (inside)
            {
                return true;
            }
        }
        if (tolerance > 0)
        {
            foreach (var ring in Polygons.SelectMany(p => p))
            {
                for (int i = 0, j = ring.Length - 1; i < ring.Length; j = i++)
                {
                    if (SegmentDistance(x, y, ring[j], ring[i]) <= tolerance)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private static double SegmentDistance(double x, double y, Point a, Point b)
    {
        double dx = b.X - a.X, dy = b.Y - a.Y;
        double t = dx == 0 && dy == 0 ? 0 : Math.Clamp(((x - a.X) * dx + (y - a.Y) * dy) / (dx * dx + dy * dy), 0, 1);
        double px = a.X + t * dx - x, py = a.Y + t * dy - y;
        return Math.Sqrt(px * px + py * py);
    }

    #region JSON

    private static NetworkArea ParseJson(string text)
    {
        using var document = JsonDocument.Parse(text);
        var root = document.RootElement;

        if (TryGet(root, "xmin", out var xmin) && TryGet(root, "ymin", out var ymin) && TryGet(root, "xmax", out var xmax) && TryGet(root, "ymax", out var ymax))
        {
            return Box(xmin.GetDouble(), ymin.GetDouble(), xmax.GetDouble(), ymax.GetDouble());
        }
        if (TryGet(root, "rings", out var rings))
        {
            // Esri: outer rings and holes in one list, even-odd
            return FromPolygons([rings.EnumerateArray().Select(Ring)]);
        }
        // GeoJSON: Polygon, MultiPolygon, Feature, FeatureCollection (each geometry is a polygon of its own)
        return FromGeometries(Read(() => GeoJson.ReadAll(text, Factory), "GeoJSON"));
    }

    private static List<Point> Ring(JsonElement ring)
        => ring.EnumerateArray().Select(p => new Point(p[0].GetDouble(), p[1].GetDouble())).ToList();

    private static JsonElement Get(JsonElement element, string name)
        => TryGet(element, name, out var value) ? value : throw new ArgumentException($"area: '{name}' is missing.");

    private static bool TryGet(JsonElement element, string name, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }
        value = default;
        return false;
    }

    #endregion

    #region SpatialNET geometries

    /// <summary>Passed to the readers instead of relying on GeometryFactory.Default (gView does not set it); 2D is enough for an area.</summary>
    private static readonly IGeometryFactory Factory = new ImpGeometryFactory(supportsZ: false, supportsM: false);

    /// <summary>One polygon per geometry; a MultiPolygon is one polygon with the rings of all its parts (they do not overlap, even-odd holds).</summary>
    private static NetworkArea FromGeometries(IEnumerable<IGeometry2D> geometries)
        => FromPolygons(geometries.Select(geometry => geometry is IPolygon2D polygon
            ? Enumerable.Range(0, polygon.RingCount).Select(r => Points(polygon[r]))
            : throw new ArgumentException($"area: a {geometry.GetType().Name} is no polygon; use Polygon or MultiPolygon.")));

    private static IEnumerable<Point> Points(IRing2D ring)
        => Enumerable.Range(0, ring.PointCount).Select(i => new Point(ring[i].X, ring[i].Y));

    /// <summary>The readers of SpatialNET.IO throw FormatException/NotSupportedException; the searches report ArgumentException.</summary>
    private static IReadOnlyList<IGeometry2D> Read(Func<IReadOnlyList<IGeometry2D>> read, string format)
    {
        try
        {
            return read();
        }
        catch (Exception ex) when (ex is FormatException or NotSupportedException)
        {
            throw new ArgumentException($"area: invalid {format}: {ex.Message}", ex);
        }
    }

    #endregion

    private static string Format(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}
