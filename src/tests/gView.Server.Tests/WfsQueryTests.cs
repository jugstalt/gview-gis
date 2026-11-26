using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.MapServer;
using gView.Framework.Geometry;
using gView.Framework.OGC.GML;
using gView.Interoperability.Extensions.Features;
using gView.Interoperability.OGC;
using gView.Interoperability.OGC.Request.WFS;
using System.Xml;
using System.Xml.Linq;

namespace gView.Server.Tests;

/// <summary>
/// WFS filters (Filter 1.x / FES 2.0 → CQL2 → SQL), resource ids, sorting, property selection, paging and counting
/// </summary>
public class WfsQueryTests
{
    private const string Fes = "xmlns:fes=\"http://www.opengis.net/fes/2.0\" xmlns:gml=\"http://www.opengis.net/gml/3.2\" xmlns:gv=\"http://www.gViewGIS.com/server\"";
    private const string Ogc = "xmlns:ogc=\"http://www.opengis.net/ogc\" xmlns:gml=\"http://www.opengis.net/gml\"";

    #region Filter

    [Fact]
    public void Fes20_ValueReferenceWithPrefix_EqualTo()
    {
        var filter = Where($"<fes:Filter {Fes}><fes:PropertyIsEqualTo><fes:ValueReference>gv:NAME</fes:ValueReference><fes:Literal>Linz</fes:Literal></fes:PropertyIsEqualTo></fes:Filter>");

        Assert.Equal("NAME = 'Linz'", filter.WhereClause);
    }

    [Fact]
    public void Fes20_PathValueReference_IsResolved()
    {
        var filter = Where($"<fes:Filter {Fes}><fes:PropertyIsGreaterThan><fes:ValueReference>gv:c0/gv:VALUE</fes:ValueReference><fes:Literal>10.5</fes:Literal></fes:PropertyIsGreaterThan></fes:Filter>");

        Assert.Equal("VALUE > 10.5", filter.WhereClause);
    }

    [Fact]
    public void Filter11_LiteralIsEscaped_NoSqlInjection()
    {
        var filter = Where($"<ogc:Filter {Ogc}><ogc:PropertyIsEqualTo><ogc:PropertyName>NAME</ogc:PropertyName><ogc:Literal>x' OR 1=1 --</ogc:Literal></ogc:PropertyIsEqualTo></ogc:Filter>");

        Assert.Equal("NAME = 'x'' OR 1=1 --'", filter.WhereClause);
    }

    [Fact]
    public void Filter_NumberLiteralForNumericField_MustBeANumber()
    {
        var ex = Assert.Throws<WfsException>(() => Where($"<ogc:Filter {Ogc}><ogc:PropertyIsEqualTo><ogc:PropertyName>VALUE</ogc:PropertyName><ogc:Literal>1 OR 1=1</ogc:Literal></ogc:PropertyIsEqualTo></ogc:Filter>"));

        Assert.Equal("InvalidParameterValue", ex.Code);
        Assert.Equal("filter", ex.Locator);
    }

    [Fact]
    public void Filter_UnknownProperty_InvalidParameterValue()
    {
        var ex = Assert.Throws<WfsException>(() => Where("<Filter><PropertyIsEqualTo><PropertyName>NAME;DROP</PropertyName><Literal>1</Literal></PropertyIsEqualTo></Filter>"));

        Assert.Equal("InvalidParameterValue", ex.Code);
    }

    [Fact]
    public void Filter_NoQueryableDateWithoutSqlDialect_IsRejected()
    {
        var ex = Assert.Throws<WfsException>(() => Where("<Filter><PropertyIsEqualTo><PropertyName>DATUM</PropertyName><Literal>2024-05-01</Literal></PropertyIsEqualTo></Filter>"));

        Assert.Contains("can't be used in filters", ex.Message);
    }

    [Fact]
    public void Filter_LikeWithWildcardsAndEscape()
    {
        var filter = Where($"<ogc:Filter {Ogc}><ogc:PropertyIsLike wildCard=\"*\" singleChar=\".\" escapeChar=\"!\"><ogc:PropertyName>NAME</ogc:PropertyName><ogc:Literal>L*n.!*%</ogc:Literal></ogc:PropertyIsLike></ogc:Filter>");

        Assert.Equal("NAME LIKE 'L%n_*\\%' ESCAPE '\\'", filter.WhereClause);
    }

    [Theory]
    [InlineData("abc", "*", "?", "\\", "abc")]
    [InlineData("a*b?c", "*", "?", "\\", "a%b_c")]
    [InlineData("100%_", "*", "?", "\\", "100\\%\\_")]
    [InlineData("a\\*b", "*", "?", "\\", "a*b")]
    [InlineData("a%%b", "%%", "#", "!", "a%b")]
    public void ToCqlLikePattern(string pattern, string wildCard, string singleChar, string escapeChar, string expected)
        => Assert.Equal(expected, FesFilterParser.ToCqlLikePattern(pattern, wildCard, singleChar, escapeChar));

    [Fact]
    public void Filter_MatchCaseFalse_UpperOnBothSides()
    {
        var filter = Where("<Filter><PropertyIsEqualTo matchCase=\"false\"><PropertyName>NAME</PropertyName><Literal>linz</Literal></PropertyIsEqualTo></Filter>");

        Assert.Equal("UPPER(NAME) = UPPER('linz')", filter.WhereClause);
    }

    [Fact]
    public void Filter_AndOrNotBetweenNull()
    {
        var filter = Where($"""
            <fes:Filter {Fes}>
              <fes:Or>
                <fes:PropertyIsBetween><fes:ValueReference>VALUE</fes:ValueReference><fes:LowerBoundary><fes:Literal>1</fes:Literal></fes:LowerBoundary><fes:UpperBoundary><fes:Literal>5</fes:Literal></fes:UpperBoundary></fes:PropertyIsBetween>
                <fes:Not><fes:PropertyIsNull><fes:ValueReference>REMARK</fes:ValueReference></fes:PropertyIsNull></fes:Not>
              </fes:Or>
            </fes:Filter>
            """);

        Assert.Equal("(VALUE BETWEEN 1 AND 5 OR NOT (REMARK IS NULL))", filter.WhereClause);
    }

    [Fact]
    public void Filter_BBoxInTopLevelAnd_SpatialFilterAndWhere()
    {
        var filter = Where($"""
            <fes:Filter {Fes}>
              <fes:And>
                <fes:PropertyIsEqualTo><fes:ValueReference>NAME</fes:ValueReference><fes:Literal>Linz</fes:Literal></fes:PropertyIsEqualTo>
                <fes:BBOX><fes:ValueReference>SHAPE</fes:ValueReference>
                  <gml:Envelope srsName="urn:ogc:def:crs:EPSG::4326"><gml:lowerCorner>48 16</gml:lowerCorner><gml:upperCorner>49 17</gml:upperCorner></gml:Envelope>
                </fes:BBOX>
              </fes:And>
            </fes:Filter>
            """);

        var spatial = Assert.IsAssignableFrom<ISpatialFilter>(filter);
        Assert.Equal("NAME = 'Linz'", filter.WhereClause);
        Assert.Equal(spatialRelation.SpatialRelationIntersects, spatial.SpatialRelation);
        Assert.Equal(4326, spatial.FilterSpatialReference.EpsgCode);
        Assert.Equal(16, ((IEnvelope)spatial.Geometry).MinX);  // urn: lat/lon => swapped
        Assert.Equal(48, ((IEnvelope)spatial.Geometry).MinY);
    }

    [Fact]
    public void Filter_WithinGml3Polygon_WithinRelationInRequestCrs()
    {
        var filter = Where($"""
            <ogc:Filter {Ogc}>
              <ogc:Within><ogc:PropertyName>SHAPE</ogc:PropertyName>
                <gml:Polygon><gml:exterior><gml:LinearRing><gml:posList>0 0 0 10 10 10 10 0 0 0</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>
              </ogc:Within>
            </ogc:Filter>
            """, srsName: "EPSG:3857", version: WfsVersion.V110);

        var spatial = Assert.IsAssignableFrom<ISpatialFilter>(filter);
        Assert.Equal(spatialRelation.SpatialRelationWithin, spatial.SpatialRelation);  // regression: was Contains
        Assert.Equal(3857, spatial.FilterSpatialReference.EpsgCode);
        Assert.IsAssignableFrom<IPolygon>(spatial.Geometry);
    }

    [Fact]
    public void Filter_SpatialInOr_NotSupported()
    {
        var ex = Assert.Throws<WfsException>(() => Where($"""
            <fes:Filter {Fes}><fes:Or>
              <fes:PropertyIsEqualTo><fes:ValueReference>NAME</fes:ValueReference><fes:Literal>Linz</fes:Literal></fes:PropertyIsEqualTo>
              <fes:BBOX><gml:Envelope><gml:lowerCorner>0 0</gml:lowerCorner><gml:upperCorner>1 1</gml:upperCorner></gml:Envelope></fes:BBOX>
            </fes:Or></fes:Filter>
            """));

        Assert.Equal("OptionNotSupported", ex.Code);
    }

    [Fact]
    public void Filter_TwoSpatialOperators_NotSupported()
    {
        var ex = Assert.Throws<WfsException>(() => Where($"""
            <fes:Filter {Fes}><fes:And>
              <fes:BBOX><gml:Envelope><gml:lowerCorner>0 0</gml:lowerCorner><gml:upperCorner>1 1</gml:upperCorner></gml:Envelope></fes:BBOX>
              <fes:BBOX><gml:Envelope><gml:lowerCorner>0 0</gml:lowerCorner><gml:upperCorner>2 2</gml:upperCorner></gml:Envelope></fes:BBOX>
            </fes:And></fes:Filter>
            """));

        Assert.Equal("OptionNotSupported", ex.Code);
    }

    [Fact]
    public void Filter_TemporalOperator_NotSupported()
    {
        var ex = Assert.Throws<WfsException>(() => Where($"<fes:Filter {Fes}><fes:After><fes:ValueReference>DATUM</fes:ValueReference><fes:Literal>2024-01-01</fes:Literal></fes:After></fes:Filter>"));

        Assert.Equal("OptionNotSupported", ex.Code);
    }

    [Fact]
    public void Fes20_ResourceIds_OfThisFeatureType()
    {
        var filter = Where($"<fes:Filter {Fes}><fes:ResourceId rid=\"c0.7\"/><fes:ResourceId rid=\"c1.8\"/><fes:ResourceId rid=\"c0.9\"/></fes:Filter>");

        Assert.Equal("OBJECTID IN (7,9)", filter.WhereClause);
    }

    [Theory]
    [InlineData("<ogc:GmlObjectId gml:id=\"c0.12\"/>")]
    [InlineData("<ogc:FeatureId fid=\"c0.12\"/>")]
    public void Filter11_GmlObjectIdAndFeatureId(string id)
    {
        var filter = Where($"<ogc:Filter {Ogc}>{id}</ogc:Filter>");

        Assert.Equal("OBJECTID IN (12)", filter.WhereClause);
    }

    [Fact]
    public void Kvp_ResourceIdOfOtherType_NoFeatures()
    {
        var parameters = WfsRequestParameters.Parse("REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=c0&RESOURCEID=c1.7");
        var filter = new WfsQuery(null, parameters, WfsVersion.V200, null).CreateFilter(WfsTestData.Collection());

        Assert.Equal("1=0", filter.WhereClause);
    }

    [Fact]
    public void Filter_LayerFilterQueryIsCombined()
    {
        var collection = WfsTestData.Collection();
        collection.FeatureLayer!.FilterQuery = new gView.Framework.Data.Filters.QueryFilter() { WhereClause = "VALUE > 0" };

        var parameters = WfsRequestParameters.Parse("REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=c0&RESOURCEID=c0.7");
        var filter = new WfsQuery(null, parameters, WfsVersion.V200, null).CreateFilter(collection);

        Assert.Equal("(VALUE > 0) AND (OBJECTID IN (7))", filter.WhereClause);
    }

    #endregion

    #region Sort, properties

    [Theory]
    [InlineData("SORTBY=NAME%20DESC,VALUE", "NAME DESC,VALUE,OBJECTID")]
    [InlineData("SORTBY=gv:NAME+A", "NAME,OBJECTID")]          // WFS 1.1.0 +A/+D (+ is a space)
    [InlineData("SORTBY=OBJECTID%20D", "OBJECTID DESC")]
    public void Kvp_SortBy_OrderByWithIdAsLastKey(string sortBy, string expected)
    {
        var parameters = WfsRequestParameters.Parse($"REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=c0&{sortBy}");
        var filter = new WfsQuery(null, parameters, WfsVersion.V200, null).CreateFilter(WfsTestData.Collection());

        Assert.Equal(expected, filter.OrderBy);
    }

    [Fact]
    public void Kvp_SortByUnknownProperty_InvalidParameterValue()
    {
        var parameters = WfsRequestParameters.Parse("REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=c0&SORTBY=NAME;DROP");

        var ex = Assert.Throws<WfsException>(() => new WfsQuery(null, parameters, WfsVersion.V200, null).CreateFilter(WfsTestData.Collection()));
        Assert.Equal("sortBy", ex.Locator);
    }

    [Fact]
    public void Xml_SortBy_ValueReferenceAndOrder()
    {
        var parameters = WfsRequestParameters.Parse($"""
            <wfs:GetFeature service="WFS" version="2.0.0" xmlns:wfs="http://www.opengis.net/wfs/2.0" {Fes}>
              <wfs:Query typeNames="gv:c0">
                <fes:SortBy><fes:SortProperty><fes:ValueReference>VALUE</fes:ValueReference><fes:SortOrder>DESC</fes:SortOrder></fes:SortProperty></fes:SortBy>
              </wfs:Query>
            </wfs:GetFeature>
            """);
        var filter = new WfsQuery(null, parameters, WfsVersion.V200, null).CreateFilter(WfsTestData.Collection());

        Assert.Equal("VALUE DESC,OBJECTID", filter.OrderBy);
    }

    [Fact]
    public async Task PropertyName_OnlySelectedPropertiesWithoutGeometry()
    {
        var xml = await GetFeatureXml("REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=c0&PROPERTYNAME=NAME");

        var feature = xml.Element(XNamespace.Get(WfsVersions.Wfs2Namespace) + "member")!.Elements().Single();
        XNamespace gv = WfsVersions.FeatureTypeNamespace;
        Assert.NotNull(feature.Element(gv + "NAME"));
        Assert.Null(feature.Element(gv + "VALUE"));
        Assert.Null(feature.Element(gv + "SHAPE"));
    }

    [Fact]
    public void PropertyName_Unknown_InvalidParameterValue()
    {
        var parameters = WfsRequestParameters.Parse("REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=c0&PROPERTYNAME=FOO");

        var ex = Assert.Throws<WfsException>(() => new WfsQuery(null, parameters, WfsVersion.V200, null).SelectedProperties(WfsTestData.Collection()));
        Assert.Equal("propertyName", ex.Locator);
    }

    #endregion

    #region Paging, counting

    [Fact]
    public async Task Paging_FirstPage_HasMoreAndNumberMatched()
    {
        var result = await Execute("STARTINDEX=0", count: 2, countMatched: true, [WfsTestData.Collection(0, 5)]);

        Assert.Equal(2, result.NumberReturned);
        Assert.Equal(5, result.NumberMatched);
        Assert.True(result.HasMore);
        Assert.Equal([7L, 8L], Oids(result));
    }

    [Fact]
    public async Task Paging_LastPage_NoMore()
    {
        var result = await Execute("STARTINDEX=4", count: 2, countMatched: false, [WfsTestData.Collection(0, 5)]);

        Assert.Equal([11L], Oids(result));
        Assert.False(result.HasMore);
        Assert.Null(result.NumberMatched);
    }

    [Fact]
    public async Task Paging_AcrossFeatureTypes()
    {
        var result = await Execute("STARTINDEX=4", count: 3, countMatched: false, [WfsTestData.Collection(0, 5), WfsTestData.Collection(1, 5)]);

        Assert.Equal(2, result.FeatureSets.Count);
        Assert.Equal("c0", result.FeatureSets[0].Collection.Id);
        Assert.Equal([11L], result.FeatureSets[0].Features.Select(f => Convert.ToInt64(f.Id)));
        Assert.Equal([7L, 8L], result.FeatureSets[1].Features.Select(f => Convert.ToInt64(f.Id)));
        Assert.True(result.HasMore);
        Assert.Equal(10, result.NumberMatched);  // counted for the start index across types
    }

    [Fact]
    public async Task Hits_OnlyCounts()
    {
        var result = await Execute("", count: 2, countMatched: false, [WfsTestData.Collection(0, 5), WfsTestData.Collection(1, 3)], hitsOnly: true);

        Assert.Equal(8, result.NumberMatched);
        Assert.Empty(result.FeatureSets);
    }

    [Fact]
    public void Writer200_NumberMatchedAndLinks()
    {
        var result = new WfsQueryResult() { NumberMatched = 5, HasMore = true };
        var bytes = new WfsFeatureWriter(WfsVersion.V200, GmlFormat.Gml32, null, false)
            .Write(result, false, null, DateTime.UtcNow, "https://x/next", "https://x/prev");

        var root = XElement.Parse(System.Text.Encoding.UTF8.GetString(bytes));
        Assert.Equal("5", root.Attribute("numberMatched")!.Value);
        Assert.Equal("0", root.Attribute("numberReturned")!.Value);
        Assert.Equal("https://x/next", root.Attribute("next")!.Value);
        Assert.Equal("https://x/prev", root.Attribute("previous")!.Value);
    }

    [Fact]
    public void Writer110_Hits_NumberOfFeatures()
    {
        var result = new WfsQueryResult() { NumberMatched = 42 };
        var bytes = new WfsFeatureWriter(WfsVersion.V110, GmlFormat.Gml31, null, false)
            .Write(result, true, null, DateTime.UtcNow, null, null);

        Assert.Equal("42", XElement.Parse(System.Text.Encoding.UTF8.GetString(bytes)).Attribute("numberOfFeatures")!.Value);
    }

    [Fact]
    public void PageUrl_ReplacesStartIndexAndCount()
    {
        var parameters = WfsRequestParameters.Parse("SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&TYPENAMES=gv%3Ac0&COUNT=10&STARTINDEX=0&FILTER=%3CFilter%2F%3E");
        var serviceRequest = new ServiceRequest("svc", "", "") { OnlineResource = "https://gview.example.com/ogc/svc" };

        string url = WFSRequest.PageUrl(serviceRequest, parameters, 10, 10)!;

        Assert.Equal("https://gview.example.com/ogc/svc?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&TYPENAMES=gv%3Ac0&FILTER=%3CFilter%2F%3E&STARTINDEX=10&COUNT=10", url);
    }

    [Fact]
    public void PageUrl_XmlRequest_NoLink()
    {
        var parameters = WfsRequestParameters.Parse("<wfs:GetFeature version=\"2.0.0\" xmlns:wfs=\"http://www.opengis.net/wfs/2.0\"><wfs:Query typeNames=\"c0\"/></wfs:GetFeature>");

        Assert.Null(WFSRequest.PageUrl(new ServiceRequest("svc", "", ""), parameters, 10, 10));
    }

    #endregion

    #region GML geometry reader

    [Fact]
    public void GmlReader_Gml2Box()
    {
        var geometry = ReadGml("<gml:Box xmlns:gml=\"http://www.opengis.net/gml\"><gml:coordinates>1,2 3,4</gml:coordinates></gml:Box>", swapAxes: false);

        var envelope = Assert.IsAssignableFrom<IEnvelope>(geometry);
        Assert.Equal((1d, 2d, 3d, 4d), (envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY));
    }

    [Fact]
    public void GmlReader_Gml32MultiSurfaceWithHole_Swapped()
    {
        var geometry = ReadGml("""
            <gml:MultiSurface xmlns:gml="http://www.opengis.net/gml/3.2" gml:id="m1">
              <gml:surfaceMember><gml:Polygon gml:id="p1">
                <gml:exterior><gml:LinearRing><gml:posList>0 0 10 0 10 10 0 10 0 0</gml:posList></gml:LinearRing></gml:exterior>
                <gml:interior><gml:LinearRing><gml:posList>2 2 2 8 8 8 8 2 2 2</gml:posList></gml:LinearRing></gml:interior>
              </gml:Polygon></gml:surfaceMember>
            </gml:MultiSurface>
            """, swapAxes: true);

        var polygon = Assert.IsAssignableFrom<IPolygon>(geometry);
        Assert.Equal(2, polygon.RingCount);
        Assert.IsAssignableFrom<IHole>(polygon[1]);
        Assert.Equal(10, polygon[0][1].Y);  // "10 0" swapped => x=0, y=10
    }

    [Fact]
    public void GmlReader_Point3D_PosWithSrsDimension()
    {
        var point = Assert.IsAssignableFrom<IPoint>(ReadGml("<gml:Point xmlns:gml=\"http://www.opengis.net/gml\" srsDimension=\"3\"><gml:pos>1 2 3</gml:pos></gml:Point>", false));

        Assert.Equal((1d, 2d), (point.X, point.Y));
    }

    #endregion

    #region Helper

    private static IQueryFilter Where(string filterXml, string? srsName = null, WfsVersion version = WfsVersion.V200)
    {
        string query = "REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=c0&FILTER=" + Uri.EscapeDataString(filterXml);
        var parameters = WfsRequestParameters.Parse(query);
        var crs = srsName is null ? null : WFSRequest.ParseCrs(srsName, "srsName");

        return new WfsQuery(null, parameters, version, crs).CreateFilter(WfsTestData.Collection());
    }

    private static Task<WfsQueryResult> Execute(string kvp, int count, bool countMatched, OgcCollectionLayer[] collections, bool hitsOnly = false)
    {
        var parameters = WfsRequestParameters.Parse($"REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=c0&{kvp}");
        return new WfsQuery(null, parameters, WfsVersion.V200, null).ExecuteAsync(collections, count, countMatched, hitsOnly);
    }

    private static long[] Oids(WfsQueryResult result)
        => result.FeatureSets.SelectMany(s => s.Features).Select(f => Convert.ToInt64(f.Id)).ToArray();

    private static async Task<XElement> GetFeatureXml(string kvp)
    {
        var parameters = WfsRequestParameters.Parse(kvp);
        var collection = WfsTestData.Collection();
        var query = new WfsQuery(null, parameters, WfsVersion.V200, null);
        var result = await query.ExecuteAsync([collection], 10, false, false);

        var bytes = new WfsFeatureWriter(WfsVersion.V200, GmlFormat.Gml32, null, false).Write(result, false, null, DateTime.UtcNow, null, null);
        return XElement.Parse(System.Text.Encoding.UTF8.GetString(bytes));
    }

    private static IGeometry ReadGml(string xml, bool swapAxes)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);
        return GmlGeometryReader.Read(doc.DocumentElement!, swapAxes);
    }

    #endregion
}
