using gView.Framework.Core.Geometry;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis.Tests;

public class GeoJsonTests
{
    [Theory]
    [InlineData("""{"type":"Point","coordinates":[1,2]}""")]
    [InlineData("""{"type":"MultiPoint","coordinates":[[1,2],[3,4]]}""")]
    [InlineData("""{"type":"LineString","coordinates":[[0,0],[1,1],[2,0]]}""")]
    [InlineData("""{"type":"MultiLineString","coordinates":[[[0,0],[1,1]],[[5,5],[6,6]]]}""")]
    [InlineData("""{"type":"Polygon","coordinates":[[[0,0],[10,0],[10,10],[0,10],[0,0]],[[2,2],[2,4],[4,4],[4,2],[2,2]]]}""")]
    [InlineData("""{"type":"MultiPolygon","coordinates":[[[[0,0],[10,0],[10,10],[0,10],[0,0]]],[[[20,0],[30,0],[30,10],[20,10],[20,0]]]]}""")]
    public void Geometry_survives_the_way_through_gview(string geoJson)
    {
        var geometry = GisGeoJson.ToGeometry(JsonNode.Parse(geoJson));

        Assert.Equal(geoJson, GisGeoJson.ToGeoJson(geometry!)!.ToJsonString());
    }

    [Fact]
    public void Hole_stays_with_its_outer_ring_of_a_multipolygon()
    {
        var geoJson = """{"type":"MultiPolygon","coordinates":[[[[0,0],[10,0],[10,10],[0,10],[0,0]],[[2,2],[2,4],[4,4],[4,2],[2,2]]],[[[20,0],[30,0],[30,10],[20,10],[20,0]]]]}""";

        var polygon = (IPolygon)GisGeoJson.ToGeometry(JsonNode.Parse(geoJson))!;

        Assert.Equal(2, polygon.OuterRingCount);
        Assert.Equal(geoJson, GisGeoJson.ToGeoJson(polygon)!.ToJsonString());
    }

    [Fact]
    public void Feature_is_taken_as_its_geometry()
        => Assert.IsAssignableFrom<IPoint>(GisGeoJson.ToGeometry(JsonNode.Parse("""{"type":"Feature","geometry":{"type":"Point","coordinates":[1,2]},"properties":{}}""")));

    [Theory]
    [InlineData("42", "42")]
    [InlineData("1.5", "1.5")]
    [InlineData("\"O'Brien\"", "'O''Brien'")]
    public void Key_values_become_literals(string json, string literal)
        => Assert.Equal(literal, GisGeoJson.ToSqlLiteral(JsonNode.Parse(json)));

    [Theory]
    [InlineData("31256", 31256)]
    [InlineData("\"31256\"", 31256)]
    [InlineData("\"EPSG:4326\"", 4326)]
    [InlineData("0", null)]
    [InlineData("\"\"", null)]
    [InlineData("null", null)]
    public void Srs_values_become_epsg_codes(string json, int? expected)
        => Assert.Equal(expected, GisSpatialReferences.ToEpsg(JsonNode.Parse(json)));

    [Fact]
    public void Geometry_is_projected_between_epsg_codes()
    {
        var point = (IPoint)GisSpatialReferences.Project(new gView.Framework.Geometry.Point(15.4395, 47.0707), 4326, 3857);

        Assert.Equal(1718717.3, point.X, 1);   // 6378137 m · 15.4395° in radians
        Assert.InRange(point.Y, 5_953_000, 5_954_500);   // 6378137 m · ln(tan(45° + 47.0707°/2))
    }

    [Theory]
    [InlineData("true")]
    [InlineData("{\"a\":1}")]
    [InlineData("\"a\\nb\"")]
    public void Other_key_values_are_refused(string json)
        => Assert.Throws<gView.Pipe.Abstraction.PipeException>(() => GisGeoJson.ToSqlLiteral(JsonNode.Parse(json)));
}
