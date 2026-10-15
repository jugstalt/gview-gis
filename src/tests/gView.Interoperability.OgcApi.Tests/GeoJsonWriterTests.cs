using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Interoperability.OgcApi.Features;
using gView.Interoperability.OgcApi.GeoJson;
using System.Text;
using System.Text.Json;
using GeoPath = gView.Framework.Geometry.Path;

namespace gView.Interoperability.OgcApi.Tests;

public class GeoJsonWriterTests
{
    private static string WriteGeometry(IGeometry? geometry, bool swapAxes = false)
    {
        using var ms = new MemoryStream();
        using (var writer = new Utf8JsonWriter(ms))
        {
            GeoJsonWriter.WriteGeometry(writer, geometry, swapAxes);
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static string WriteValue(object? value)
    {
        using var ms = new MemoryStream();
        using (var writer = new Utf8JsonWriter(ms))
        {
            GeoJsonWriter.WriteValue(writer, value);
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static Ring Square(double minX, double minY, double size)
        => new Ring(new List<IPoint>()
        {
            new Point(minX, minY),
            new Point(minX + size, minY),
            new Point(minX + size, minY + size),
            new Point(minX, minY + size)
        });

    [Fact]
    public void WriteGeometry_Point_WritesXY()
    {
        var json = WriteGeometry(new Point(15.5, 47.1));

        Assert.Equal("""{"type":"Point","coordinates":[15.5,47.1]}""", json);
    }

    [Fact]
    public void WriteGeometry_PointWithSwapAxes_WritesYX()
    {
        var json = WriteGeometry(new Point(15.5, 47.1), swapAxes: true);

        Assert.Equal("""{"type":"Point","coordinates":[47.1,15.5]}""", json);
    }

    [Fact]
    public void WriteGeometry_Null_WritesNull()
    {
        Assert.Equal("null", WriteGeometry(null));
    }

    [Fact]
    public void WriteGeometry_PolylineWithOnePath_WritesLineString()
    {
        var path = new GeoPath();
        path.AddPoint(new Point(0, 0));
        path.AddPoint(new Point(1, 1));
        var polyline = new Polyline();
        polyline.AddPath(path);

        var json = WriteGeometry(polyline);

        Assert.Equal("""{"type":"LineString","coordinates":[[0,0],[1,1]]}""", json);
    }

    [Fact]
    public void WriteGeometry_PolygonWithHole_WritesPolygonWithClosedRings()
    {
        var polygon = new Polygon();
        polygon.AddRing(Square(0, 0, 10));
        polygon.AddRing(Square(2, 2, 2));

        using var doc = JsonDocument.Parse(WriteGeometry(polygon));
        var root = doc.RootElement;

        Assert.Equal("Polygon", root.GetProperty("type").GetString());
        var rings = root.GetProperty("coordinates");
        Assert.Equal(2, rings.GetArrayLength());
        foreach (var ring in rings.EnumerateArray())
        {
            Assert.Equal(5, ring.GetArrayLength());  // closed
            Assert.Equal(ring[0].ToString(), ring[4].ToString());
        }
        Assert.Equal(0, rings[0][0][0].GetDouble());  // outer ring first
        Assert.Equal(2, rings[1][0][0].GetDouble());
    }

    [Fact]
    public void WriteGeometry_PolygonWithTwoOuterRings_WritesMultiPolygon()
    {
        var polygon = new Polygon();
        polygon.AddRing(Square(0, 0, 10));
        polygon.AddRing(Square(2, 2, 2));      // hole of the first ring
        polygon.AddRing(Square(100, 100, 5));  // second outer ring

        using var doc = JsonDocument.Parse(WriteGeometry(polygon));
        var root = doc.RootElement;

        Assert.Equal("MultiPolygon", root.GetProperty("type").GetString());
        var polygons = root.GetProperty("coordinates");
        Assert.Equal(2, polygons.GetArrayLength());

        var withHole = polygons.EnumerateArray().Single(p => p[0][0][0].GetDouble() == 0);
        var withoutHole = polygons.EnumerateArray().Single(p => p[0][0][0].GetDouble() == 100);
        Assert.Equal(2, withHole.GetArrayLength());
        Assert.Equal(1, withoutHole.GetArrayLength());
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData("text", "\"text\"")]
    [InlineData(42, "42")]
    [InlineData(1.5, "1.5")]
    [InlineData(true, "true")]
    [InlineData(double.NaN, "null")]
    public void WriteValue_SimpleValues_WritesJson(object? value, string expected)
    {
        Assert.Equal(expected, WriteValue(value));
    }

    [Fact]
    public void WriteValue_DBNull_WritesNull()
    {
        Assert.Equal("null", WriteValue(DBNull.Value));
    }

    [Fact]
    public void WriteValue_UtcDateTime_WritesIso8601WithZ()
    {
        var value = new DateTime(2024, 5, 17, 8, 30, 0, DateTimeKind.Utc);

        Assert.Equal("\"2024-05-17T08:30:00Z\"", WriteValue(value));
    }

    [Fact]
    public void WriteFeature_WithIdAndProperties_WritesGeoJsonFeature()
    {
        var feature = new OgcFeature() { Id = 7, Geometry = new Point(1, 2) };
        feature.Properties.Add(new("NAME", "Graz"));

        using var ms = new MemoryStream();
        using (var writer = new Utf8JsonWriter(ms))
        {
            GeoJsonWriter.WriteFeature(writer, feature, swapAxes: false);
        }

        Assert.Equal(
            """{"type":"Feature","id":7,"geometry":{"type":"Point","coordinates":[1,2]},"properties":{"NAME":"Graz"}}""",
            Encoding.UTF8.GetString(ms.ToArray()));
    }
}
