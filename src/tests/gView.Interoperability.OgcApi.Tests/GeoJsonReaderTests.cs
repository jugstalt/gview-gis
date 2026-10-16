using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Interoperability.OgcApi.GeoJson;
using System.Text.Json;

namespace gView.Interoperability.OgcApi.Tests;

public class GeoJsonReaderTests
{
    [Fact]
    public void ReadFeature_PointWithProperties()
    {
        var feature = GeoJsonReader.ReadFeature("""
            {"type":"Feature","id":5,"geometry":{"type":"Point","coordinates":[15.5,47.1]},"properties":{"name":"Graz","ew":1}}
            """, swapAxes: false);

        Assert.True(feature.HasGeometry);
        var point = Assert.IsAssignableFrom<IPoint>(feature.Geometry);
        Assert.Equal(15.5, point.X);
        Assert.Equal(47.1, point.Y);
        Assert.Equal("Graz", feature.Properties!["name"].GetString());
    }

    [Fact]
    public void ReadFeature_SwapAxes_SwapsCoordinates()
    {
        var feature = GeoJsonReader.ReadFeature("""
            {"type":"Feature","geometry":{"type":"Point","coordinates":[47.1,15.5]},"properties":{}}
            """, swapAxes: true);

        var point = Assert.IsAssignableFrom<IPoint>(feature.Geometry);
        Assert.Equal(15.5, point.X);
        Assert.Equal(47.1, point.Y);
    }

    [Fact]
    public void ReadFeature_WithoutGeometryMember_HasGeometryFalse()
    {
        var feature = GeoJsonReader.ReadFeature("""{"type":"Feature","properties":{"a":1}}""", false);

        Assert.False(feature.HasGeometry);
        Assert.Null(feature.Geometry);
    }

    [Fact]
    public void ReadFeature_NullGeometry_HasGeometryTrue()
    {
        var feature = GeoJsonReader.ReadFeature("""{"type":"Feature","geometry":null,"properties":null}""", false);

        Assert.True(feature.HasGeometry);
        Assert.Null(feature.Geometry);
        Assert.Null(feature.Properties);
    }

    [Theory]
    [InlineData("""{"type":"LineString","coordinates":[[0,0],[1,1]]}""", typeof(IPolyline), 1)]
    [InlineData("""{"type":"MultiLineString","coordinates":[[[0,0],[1,1]],[[2,2],[3,3]]]}""", typeof(IPolyline), 2)]
    [InlineData("""{"type":"Polygon","coordinates":[[[0,0],[10,0],[10,10],[0,0]],[[2,2],[3,2],[3,3],[2,2]]]}""", typeof(IPolygon), 2)]
    [InlineData("""{"type":"MultiPolygon","coordinates":[[[[0,0],[1,0],[1,1],[0,0]]],[[[5,5],[6,5],[6,6],[5,5]]]]}""", typeof(IPolygon), 2)]
    [InlineData("""{"type":"MultiPoint","coordinates":[[0,0],[1,1],[2,2]]}""", typeof(IMultiPoint), 3)]
    public void ReadGeometry_Types(string geometryJson, Type expectedType, int expectedParts)
    {
        using var doc = JsonDocument.Parse(geometryJson);

        var geometry = GeoJsonReader.ReadGeometry(doc.RootElement, false);

        Assert.IsAssignableFrom(expectedType, geometry);
        int parts = geometry switch
        {
            IPolyline polyline => polyline.PathCount,
            IPolygon polygon => polygon.RingCount,
            IMultiPoint multiPoint => multiPoint.PointCount,
            _ => -1
        };
        Assert.Equal(expectedParts, parts);
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("""{"type":"FeatureCollection","features":[]}""")]
    [InlineData("""{"type":"Feature","geometry":{"type":"GeometryCollection","geometries":[]}}""")]
    [InlineData("""{"type":"Feature","geometry":{"type":"Point","coordinates":["a","b"]}}""")]
    [InlineData("""{"type":"Feature","properties":[1,2]}""")]
    public void ReadFeature_Invalid_ThrowsBadRequest(string json)
    {
        var ex = Assert.Throws<OgcApiException>(() => GeoJsonReader.ReadFeature(json, false));

        Assert.Equal(400, ex.StatusCode);
    }

    private static object? ToFieldValue(FieldType type, string json)
    {
        using var doc = JsonDocument.Parse(json);
        return OgcApiFeatureService.ToFieldValue(new Field("f", type), doc.RootElement);
    }

    [Fact]
    public void ToFieldValue_ConvertsByFieldType()
    {
        Assert.Equal(42, ToFieldValue(FieldType.integer, "42"));
        Assert.Equal(42L, ToFieldValue(FieldType.biginteger, "\"42\""));
        Assert.Equal(1, ToFieldValue(FieldType.integer, "true"));
        Assert.Equal(0, ToFieldValue(FieldType.integer, "false"));
        Assert.Equal(1.5, ToFieldValue(FieldType.Double, "1.5"));
        Assert.Equal(true, ToFieldValue(FieldType.boolean, "true"));
        Assert.Equal("x", ToFieldValue(FieldType.String, "\"x\""));
        Assert.Equal("12", ToFieldValue(FieldType.String, "12"));
        Assert.Null(ToFieldValue(FieldType.integer, "null"));
        Assert.Equal(new DateTime(2024, 5, 17), ToFieldValue(FieldType.Date, "\"2024-05-17\""));
        Assert.Equal(new DateTime(2024, 5, 17, 8, 30, 0, DateTimeKind.Utc), ToFieldValue(FieldType.Date, "\"2024-05-17T10:30:00+02:00\""));
    }

    [Theory]
    [InlineData(FieldType.integer, "\"abc\"")]
    [InlineData(FieldType.integer, "1.5")]
    [InlineData(FieldType.Double, "\"x\"")]
    [InlineData(FieldType.Date, "\"yesterday\"")]
    public void ToFieldValue_InvalidValue_ThrowsBadRequest(FieldType type, string json)
    {
        var ex = Assert.Throws<OgcApiException>(() => ToFieldValue(type, json));

        Assert.Equal(400, ex.StatusCode);
    }
}
