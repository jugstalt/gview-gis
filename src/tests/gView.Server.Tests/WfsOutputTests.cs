using gView.Framework.OGC.GML;
using gView.Interoperability.OGC;
using gView.Interoperability.OGC.Request.WFS;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace gView.Server.Tests;

/// <summary>
/// WFS GeoJSON output and the stored query GetFeatureById
/// </summary>
public class WfsOutputTests
{
    #region GeoJSON

    [Theory]
    [InlineData("application/json", true)]
    [InlineData("application/geo+json", true)]
    [InlineData("json", true)]
    [InlineData("GEOJSON", true)]
    [InlineData("application/json; subtype=geojson", true)]
    [InlineData("text/xml; subtype=gml/3.1.1", false)]
    [InlineData(null, false)]
    public void IsGeoJsonOutputFormat(string? outputFormat, bool expected)
        => Assert.Equal(expected, WfsVersions.IsGeoJsonOutputFormat(outputFormat));

    [Fact]
    public async Task GeoJson_WebMercator_XYWithCrsMemberAndResourceIds()
    {
        var root = await GeoJson("urn:ogc:def:crs:EPSG::3857", featureCount: 2);

        Assert.Equal("FeatureCollection", root.GetProperty("type").GetString());
        Assert.Equal(2, root.GetProperty("numberReturned").GetInt32());
        Assert.Equal("urn:ogc:def:crs:EPSG::3857", root.GetProperty("crs").GetProperty("properties").GetProperty("name").GetString());

        var feature = root.GetProperty("features")[0];
        Assert.Equal("c0.7", feature.GetProperty("id").GetString());
        Assert.Equal("Linz & Co", feature.GetProperty("properties").GetProperty("NAME").GetString());
        var coordinates = feature.GetProperty("geometry").GetProperty("coordinates");
        Assert.InRange(coordinates[0].GetDouble(), 1_800_000, 1_900_000);  // x first
    }

    [Fact]
    public async Task GeoJson_UrnWgs84_StaysLonLatWithoutCrsMember()
    {
        // GML would be lat/lon for the URN, GeoJSON is always x/y
        var root = await GeoJson("urn:ogc:def:crs:EPSG::4326");

        Assert.False(root.TryGetProperty("crs", out _));
        var coordinates = root.GetProperty("features")[0].GetProperty("geometry").GetProperty("coordinates");
        Assert.InRange(coordinates[0].GetDouble(), 16, 17);
        Assert.InRange(coordinates[1].GetDouble(), 47.5, 49);
    }

    [Fact]
    public void GeoJson_Hits_NumberMatchedNoFeatures()
    {
        var result = new WfsQueryResult() { NumberMatched = 12 };
        var root = Parse(WfsGeoJsonWriter.Write(result, hits: true, null, DateTime.UtcNow, null, null));

        Assert.Equal(12, root.GetProperty("numberMatched").GetInt32());
        Assert.Equal(0, root.GetProperty("numberReturned").GetInt32());
        Assert.Equal(0, root.GetProperty("features").GetArrayLength());
    }

    [Fact]
    public void GeoJson_NextLink()
    {
        var root = Parse(WfsGeoJsonWriter.Write(new WfsQueryResult(), false, null, DateTime.UtcNow, "https://x/next", null));

        var link = root.GetProperty("links")[0];
        Assert.Equal("next", link.GetProperty("rel").GetString());
        Assert.Equal("https://x/next", link.GetProperty("href").GetString());
    }

    #endregion

    #region GetFeatureById

    [Fact]
    public void Kvp_StoredQuery_IdParameter()
    {
        var parameters = WfsRequestParameters.Parse("SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&STOREDQUERY_ID=urn:ogc:def:query:OGC-WFS::GetFeatureById&ID=c0.7");

        Assert.True(WfsStoredQueries.IsGetFeatureById(parameters.StoredQueryId));
        Assert.Equal("c0.7", parameters.StoredQueryParameters["id"]);
    }

    [Fact]
    public void Xml_StoredQuery_Parameter()
    {
        var parameters = WfsRequestParameters.Parse("""
            <wfs:GetFeature service="WFS" version="2.0.0" xmlns:wfs="http://www.opengis.net/wfs/2.0">
              <wfs:StoredQuery id="urn:ogc:def:query:OGC-WFS::GetFeatureById"><wfs:Parameter name="ID">c0.9</wfs:Parameter></wfs:StoredQuery>
            </wfs:GetFeature>
            """);

        Assert.Equal(WfsStoredQueries.GetFeatureById, parameters.StoredQueryId);
        Assert.Equal("c0.9", parameters.StoredQueryParameters["ID"]);
    }

    [Fact]
    public void GetFeatureById_FilterSelectsTheObjectId()
    {
        var parameters = WfsRequestParameters.Parse("VERSION=2.0.0&REQUEST=GetFeature&STOREDQUERY_ID=GetFeatureById&ID=c0.9").ForResourceId("c0.9");

        var filter = new WfsQuery(null, parameters, WfsVersion.V200, null).CreateFilter(WfsTestData.Collection());
        Assert.Equal("OBJECTID IN (9)", filter.WhereClause);
    }

    [Fact]
    public void SingleFeature_Gml32_FeatureIsTheRoot()
    {
        var collection = WfsTestData.Collection();
        var feature = new gView.Interoperability.Extensions.Features.OgcFeature() { Id = 7, Geometry = new gView.Framework.Geometry.Point(1, 2) };

        var xml = XElement.Parse(Encoding.UTF8.GetString(
            new WfsFeatureWriter(WfsVersion.V200, GmlFormat.Gml32, "urn:ogc:def:crs:EPSG::3857", false).WriteSingleFeature(collection, feature)));

        Assert.Equal(XNamespace.Get(WfsVersions.FeatureTypeNamespace) + "c0", xml.Name);
        Assert.Equal("c0.7", xml.Attribute(XNamespace.Get(GmlGeometryWriter.Gml32Namespace) + "id")!.Value);
    }

    [Fact]
    public void SingleFeature_GeoJson_IsAFeature()
    {
        var feature = new gView.Interoperability.Extensions.Features.OgcFeature() { Id = 7, Geometry = new gView.Framework.Geometry.Point(1, 2) };

        var root = Parse(WfsGeoJsonWriter.WriteFeature(WfsTestData.Collection(), feature, null));

        Assert.Equal("Feature", root.GetProperty("type").GetString());
        Assert.Equal("c0.7", root.GetProperty("id").GetString());
    }

    [Fact]
    public void ListStoredQueries_GetFeatureByIdWithFeatureTypes()
    {
        var xml = XElement.Parse(Encoding.UTF8.GetString(WfsStoredQueries.WriteList(["gv:c0", "gv:c1"])));
        XNamespace wfs = WfsVersions.Wfs2Namespace;

        var storedQuery = xml.Element(wfs + "StoredQuery")!;
        Assert.Equal(WfsStoredQueries.GetFeatureById, storedQuery.Attribute("id")!.Value);
        Assert.Equal(["gv:c0", "gv:c1"], storedQuery.Elements(wfs + "ReturnFeatureType").Select(e => e.Value));
    }

    [Fact]
    public void DescribeStoredQueries_IdParameter()
    {
        var xml = XElement.Parse(Encoding.UTF8.GetString(WfsStoredQueries.WriteDescriptions([WfsStoredQueries.GetFeatureById])));
        XNamespace wfs = WfsVersions.Wfs2Namespace;

        var parameter = xml.Element(wfs + "StoredQueryDescription")!.Element(wfs + "Parameter")!;
        Assert.Equal("ID", parameter.Attribute("name")!.Value);
    }

    [Fact]
    public void DescribeStoredQueries_Unknown_InvalidParameterValue()
    {
        var ex = Assert.Throws<WfsException>(() => WfsStoredQueries.WriteDescriptions(["urn:my:query"]));

        Assert.Equal("InvalidParameterValue", ex.Code);
    }

    [Fact]
    public void Gml_XLinkPrefixDeclared_ForSchemaValidatingClients()
    {
        // the GML property types have a default xlink:type attribute: .NET validation failed without the prefix
        var collection = WfsTestData.Collection();
        var feature = new gView.Interoperability.Extensions.Features.OgcFeature() { Id = 7, Geometry = new gView.Framework.Geometry.Point(1, 2) };

        var single = Encoding.UTF8.GetString(new WfsFeatureWriter(WfsVersion.V200, GmlFormat.Gml32, null, false).WriteSingleFeature(collection, feature));
        var result = new WfsQueryResult();
        result.FeatureSets.Add(new WfsFeatureSet(collection, [feature]));
        var collectionXml = Encoding.UTF8.GetString(new WfsFeatureWriter(WfsVersion.V110, GmlFormat.Gml31, null, false).Write(result, false, null, DateTime.UtcNow, null, null));

        Assert.Contains("xmlns:xlink=\"http://www.w3.org/1999/xlink\"", single);
        Assert.Contains("xmlns:xlink=\"http://www.w3.org/1999/xlink\"", collectionXml);
    }

    [Fact]
    public void ExceptionReport110_LanguageAttribute()
    {
        // OWS 1.0 ExceptionReport has a language attribute, no xml:lang
        var xml = XElement.Parse(Encoding.UTF8.GetString(WfsExceptionReport.Write(WfsVersion.V110, "InvalidParameterValue", "x", "typeName")));

        Assert.Equal("en", xml.Attribute("language")!.Value);
        Assert.Null(xml.Attribute(XNamespace.Xml + "lang"));
    }

    [Fact]
    public void NotFound_Is404()
        => Assert.Equal(404, new WfsException("NotFound", "Feature c0.1 not found", "ID").HttpStatusCode);

    #endregion

    #region Helper

    private static async Task<JsonElement> GeoJson(string srsName, int featureCount = 1)
    {
        var crs = WFSRequest.ParseCrs(srsName, "srsName");
        var parameters = WfsRequestParameters.Parse("REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=c0&OUTPUTFORMAT=application/json");
        var result = await new WfsQuery(null, parameters, WfsVersion.V200, crs)
            .ExecuteAsync([WfsTestData.Collection(0, featureCount)], 10, false, false);

        return Parse(WfsGeoJsonWriter.Write(result, false, crs, DateTime.UtcNow, null, null));
    }

    private static JsonElement Parse(byte[] json) => JsonDocument.Parse(json).RootElement;

    #endregion
}
