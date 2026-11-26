using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.OGC.GML;
using gView.Interoperability.Extensions.Features;
using gView.Interoperability.OGC;
using gView.Interoperability.OGC.Request.WFS;
using System.Xml.Linq;
using DataFeature = gView.Framework.Core.Data.Feature;

namespace gView.Server.Tests;

/// <summary>
/// WFS 2.0.0/1.1.0/1.0.0: request parsing, type names, CRS and axis order, GML output, exception reports
/// </summary>
public class WfsTests
{
    private static readonly XNamespace Wfs2 = WfsVersions.Wfs2Namespace;
    private static readonly XNamespace Wfs1 = WfsVersions.Wfs1Namespace;
    private static readonly XNamespace Gv = WfsVersions.FeatureTypeNamespace;
    private static readonly XNamespace Gml = GmlGeometryWriter.Gml2Namespace;
    private static readonly XNamespace Gml32 = GmlGeometryWriter.Gml32Namespace;

    #region Request parameters

    [Fact]
    public void Kvp_UrlDecodesValues_FilterWithEqualsSign()
    {
        string filter = "<Filter><PropertyIsEqualTo><PropertyName>NAME</PropertyName><Literal>a=b &amp; c</Literal></PropertyIsEqualTo></Filter>";
        var parameters = WfsRequestParameters.Parse(
            "service=WFS&version=2.0.0&request=GetFeature&typeNames=gv%3Ac0&FILTER=" + Uri.EscapeDataString(filter));

        Assert.Equal("GetFeature", parameters.Request);
        Assert.Equal(["gv:c0"], parameters.TypeNames);
        Assert.Equal("a=b & c", parameters.Filter!.InnerText.Replace("NAME", ""));
    }

    [Fact]
    public void Kvp_TypeNameList_MaxFeaturesAndSrsName()
    {
        var parameters = WfsRequestParameters.Parse("SERVICE=WFS&VERSION=1.1.0&REQUEST=GetFeature&TYPENAME=c0,c1&MAXFEATURES=10&SRSNAME=urn:ogc:def:crs:EPSG::4326&STARTINDEX=5");

        Assert.Equal(["c0", "c1"], parameters.TypeNames);
        Assert.Equal(10, parameters.Count);
        Assert.Equal(5, parameters.StartIndex);
        Assert.Equal("urn:ogc:def:crs:EPSG::4326", parameters.SrsName);
    }

    [Fact]
    public void Kvp_BBoxAndFilter_AreMutuallyExclusive()
    {
        var ex = Assert.Throws<WfsException>(() => WfsRequestParameters.Parse(
            "REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=c0&BBOX=0,0,1,1&FILTER=%3CFilter%2F%3E"));

        Assert.Equal("InvalidParameterValue", ex.Code);
    }

    [Fact]
    public void Kvp_InvalidCount_Throws()
    {
        var ex = Assert.Throws<WfsException>(() => WfsRequestParameters.Parse("REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=c0&COUNT=abc"));

        Assert.Equal("InvalidParameterValue", ex.Code);
        Assert.Equal("count", ex.Locator);
    }

    [Fact]
    public void Xml_GetFeature_QueryTypeNamesSrsNameFilter()
    {
        var parameters = WfsRequestParameters.Parse("""
            <wfs:GetFeature service="WFS" version="2.0.0" count="5" xmlns:wfs="http://www.opengis.net/wfs/2.0" xmlns:fes="http://www.opengis.net/fes/2.0">
              <wfs:Query typeNames="gv:c0" srsName="urn:ogc:def:crs:EPSG::3857">
                <fes:Filter><fes:ResourceId rid="c0.1"/></fes:Filter>
              </wfs:Query>
            </wfs:GetFeature>
            """);

        Assert.True(parameters.IsXmlRequest);
        Assert.Equal("GetFeature", parameters.Request);
        Assert.Equal("2.0.0", parameters.VersionString);
        Assert.Equal(5, parameters.Count);
        Assert.Equal(["gv:c0"], parameters.TypeNames);
        Assert.Equal("urn:ogc:def:crs:EPSG::3857", parameters.SrsName);
        Assert.Equal("Filter", parameters.Filter!.LocalName);
    }

    [Fact]
    public void Xml_Invalid_OperationParsingFailed()
    {
        var ex = Assert.Throws<WfsException>(() => WfsRequestParameters.Parse("<wfs:GetFeature"));

        Assert.Equal("OperationParsingFailed", ex.Code);
    }

    #endregion

    #region Versions

    [Theory]
    [InlineData("1.1.0,1.0.0", null, WfsVersion.V110)]
    [InlineData("", "1.0.0", WfsVersion.V100)]
    [InlineData("", null, WfsVersion.V200)]
    [InlineData("", "9.9.9", WfsVersion.V200)]
    public void GetCapabilities_NegotiatesVersion(string acceptVersions, string? version, WfsVersion expected)
    {
        string request = "REQUEST=GetCapabilities&SERVICE=WFS"
            + (acceptVersions.Length > 0 ? "&ACCEPTVERSIONS=" + acceptVersions : "")
            + (version is not null ? "&VERSION=" + version : "");

        Assert.Equal(expected, WFSRequest.NegotiateVersion(WfsRequestParameters.Parse(request)));
    }

    [Fact]
    public void GetCapabilities_NoAcceptedVersion_VersionNegotiationFailed()
    {
        var ex = Assert.Throws<WfsException>(() => WFSRequest.NegotiateVersion(WfsRequestParameters.Parse("REQUEST=GetCapabilities&ACCEPTVERSIONS=3.0.0")));

        Assert.Equal("VersionNegotiationFailed", ex.Code);
    }

    [Fact]
    public void GetFeature_WithoutVersion_MissingParameterValue()
    {
        var ex = Assert.Throws<WfsException>(() => WFSRequest.RequestVersion(WfsRequestParameters.Parse("REQUEST=GetFeature&TYPENAMES=c0")));

        Assert.Equal("MissingParameterValue", ex.Code);
        Assert.Equal("version", ex.Locator);
    }

    #endregion

    #region Type names

    [Theory]
    [InlineData("c0")]
    [InlineData("gv:c0")]
    [InlineData("wfs:c0")]  // as advertised by the capabilities of former gView versions
    [InlineData("C0")]
    [InlineData("{http://www.gViewGIS.com/server}c0")]
    public void FeatureTypes_FindsTypeName(string typeName)
    {
        var featureTypes = new WfsFeatureTypes([Collection()]);

        Assert.NotNull(featureTypes.Find(typeName));
    }

    [Fact]
    public void FeatureTypes_UnknownTypeName_InvalidParameterValue()
    {
        var featureTypes = new WfsFeatureTypes([Collection()]);

        var ex = Assert.Throws<WfsException>(() => featureTypes.Resolve(["gv:c99"], "typeNames", allIfEmpty: false));
        Assert.Equal("InvalidParameterValue", ex.Code);
        Assert.Equal("typeNames", ex.Locator);
    }

    [Fact]
    public void FeatureTypes_QualifiedNameInGvNamespace()
        => Assert.Equal("gv:c0", WfsFeatureTypes.QualifiedName(Collection()));

    #endregion

    #region CRS and axis order

    [Theory]
    [InlineData(WfsVersion.V100, "urn:ogc:def:crs:EPSG::4326", false)]  // 1.0.0 is always x/y
    [InlineData(WfsVersion.V110, "EPSG:4326", false)]                    // short form: x/y
    [InlineData(WfsVersion.V110, "urn:ogc:def:crs:EPSG::4326", true)]   // URN: authority order (lat/lon)
    [InlineData(WfsVersion.V200, "urn:x-ogc:def:crs:EPSG:4326", true)]
    [InlineData(WfsVersion.V200, "http://www.opengis.net/def/crs/EPSG/0/31256", true)]
    [InlineData(WfsVersion.V200, "urn:ogc:def:crs:EPSG::3857", false)]  // east/north
    [InlineData(WfsVersion.V200, "http://www.opengis.net/gml/srs/epsg.xml#4326", false)]
    [InlineData(WfsVersion.V200, "urn:ogc:def:crs:OGC:1.3:CRS84", false)]
    public void SwapAxes_FollowsNotationRule(WfsVersion version, string srsName, bool expected)
    {
        var crs = WFSRequest.ParseCrs(srsName, "srsName");

        Assert.Equal(expected, version.SwapAxes(crs, crs.ToSpatialReference()));
    }

    [Fact]
    public void ParseCrs_EchoesNotation()
    {
        Assert.Equal("urn:ogc:def:crs:EPSG::4326", WFSRequest.ParseCrs("urn:x-ogc:def:crs:EPSG:4326", "srsName").Name);
        Assert.Equal("EPSG:31256", WFSRequest.ParseCrs("epsg:31256", "srsName").Name);
    }

    [Theory]
    [InlineData("foo:bar")]
    [InlineData("EPSG:999999")]  // regression: unknown EPSG codes caused an internal error
    public void ParseCrs_Unknown_InvalidParameterValue(string value)
    {
        var ex = Assert.Throws<WfsException>(() => WFSRequest.ParseCrs(value, "srsName"));

        Assert.Equal("InvalidParameterValue", ex.Code);
    }

    [Fact]
    public void BBox_UrnWgs84_IsLatLon()
    {
        var condition = WfsQuery.BBoxCondition("48,14,49,15,urn:ogc:def:crs:EPSG::4326", WfsVersion.V200, null);

        var envelope = (IEnvelope)condition.Geometry;
        Assert.Equal(14, envelope.MinX);
        Assert.Equal(48, envelope.MinY);
        Assert.Equal(4326, condition.SpatialReference!.EpsgCode);
    }

    [Fact]
    public void BBox_ShortEpsg_IsXY()
    {
        var condition = WfsQuery.BBoxCondition("14,48,15,49,EPSG:4326", WfsVersion.V110, null);

        Assert.Equal(14, ((IEnvelope)condition.Geometry).MinX);
    }

    [Fact]
    public void BBox_WithoutCrs_UsesRequestCrs()
    {
        var crs = WFSRequest.ParseCrs("urn:ogc:def:crs:EPSG::3857", "srsName");
        var condition = WfsQuery.BBoxCondition("1,2,3,4", WfsVersion.V200, crs);

        Assert.Equal(3857, condition.SpatialReference!.EpsgCode);
        Assert.Equal(1, ((IEnvelope)condition.Geometry).MinX);
    }

    #endregion

    #region GetFeature output

    [Fact]
    public async Task GetFeature200_WebMercator_GeometryInRequestCrs()
    {
        // regression: the geometries were written in the CRS of the data (epsg:31256)
        var xml = await GetFeature(WfsVersion.V200, GmlFormat.Gml32, "urn:ogc:def:crs:EPSG::3857");

        Assert.Equal(Wfs2 + "FeatureCollection", xml.Name);
        Assert.Equal("1", xml.Attribute("numberReturned")!.Value);

        var feature = xml.Element(Wfs2 + "member")!.Element(Gv + "c0")!;
        Assert.Equal("c0.7", feature.Attribute(Gml32 + "id")!.Value);

        var point = feature.Element(Gv + "SHAPE")!.Element(Gml32 + "Point")!;
        Assert.Equal("urn:ogc:def:crs:EPSG::3857", point.Attribute("srsName")!.Value);
        var (x, y) = Pos(point.Element(Gml32 + "pos")!.Value);
        Assert.InRange(x, 1_800_000, 1_900_000);  // ~16.5° E
        Assert.InRange(y, 6_100_000, 6_200_000);  // ~48.3° N
    }

    [Fact]
    public async Task GetFeature110_UrnWgs84_LatLon()
    {
        var xml = await GetFeature(WfsVersion.V110, GmlFormat.Gml31, "urn:ogc:def:crs:EPSG::4326");

        var point = xml.Element(Gml + "featureMember")!.Element(Gv + "c0")!.Element(Gv + "SHAPE")!.Element(Gml + "Point")!;
        var (first, second) = Pos(point.Element(Gml + "pos")!.Value);
        Assert.InRange(first, 47.5, 49);   // lat first
        Assert.InRange(second, 16, 17);
    }

    [Fact]
    public async Task GetFeature110_ShortEpsgWgs84_LonLat()
    {
        var xml = await GetFeature(WfsVersion.V110, GmlFormat.Gml31, "EPSG:4326");

        var point = xml.Element(Gml + "featureMember")!.Element(Gv + "c0")!.Element(Gv + "SHAPE")!.Element(Gml + "Point")!;
        Assert.Equal("EPSG:4326", point.Attribute("srsName")!.Value);
        var (first, _) = Pos(point.Element(Gml + "pos")!.Value);
        Assert.InRange(first, 16, 17);   // lon first
    }

    [Fact]
    public async Task GetFeature100_Gml2_FidCoordinatesAndBoundedBy()
    {
        var xml = await GetFeature(WfsVersion.V100, GmlFormat.Gml2, "EPSG:31256");

        Assert.Equal(Wfs1 + "FeatureCollection", xml.Name);
        Assert.NotNull(xml.Element(Gml + "boundedBy"));
        var feature = xml.Element(Gml + "featureMember")!.Element(Gv + "c0")!;
        Assert.Equal("c0.7", feature.Attribute("fid")!.Value);
        Assert.Equal("10000,355000", feature.Element(Gv + "SHAPE")!.Element(Gml + "Point")!.Element(Gml + "coordinates")!.Value);
    }

    [Fact]
    public async Task GetFeature_PropertiesInvariantIsoNullOmitted()
    {
        var xml = await GetFeature(WfsVersion.V200, GmlFormat.Gml32, "urn:ogc:def:crs:EPSG::31256");

        var feature = xml.Element(Wfs2 + "member")!.Element(Gv + "c0")!;
        Assert.Equal("Linz & Co", feature.Element(Gv + "NAME")!.Value);
        Assert.Equal("1234.5", feature.Element(Gv + "VALUE")!.Value);
        Assert.Equal("2024-05-01T12:30:00", feature.Element(Gv + "DATUM")!.Value);
        Assert.Null(feature.Element(Gv + "REMARK"));      // null => omitted
        Assert.Null(feature.Element(Gv + "OBJECTID"));    // the id is the gml:id
        Assert.Null(feature.Element(Gml32 + "boundedBy")); // no boundedBy per feature in GML 3.2
    }

    #endregion

    #region DescribeFeatureType

    [Fact]
    public void DescribeFeatureType_Gml32_ElementsAsInTheFeatures()
    {
        var xsd = XElement.Parse(System.Text.Encoding.UTF8.GetString(WfsSchemaWriter.Write([Collection()], GmlFormat.Gml32)));
        XNamespace xs = WfsVersions.XsdNamespace;

        var element = xsd.Elements(xs + "element").Single();
        Assert.Equal("c0", element.Attribute("name")!.Value);
        Assert.Equal("gml:AbstractFeature", element.Attribute("substitutionGroup")!.Value);

        var properties = xsd.Descendants(xs + "sequence").Single().Elements(xs + "element")
            .ToDictionary(e => e.Attribute("name")!.Value, e => e.Attribute("type")!.Value);
        Assert.Equal("gml:PointPropertyType", properties["SHAPE"]);
        Assert.Equal("xsd:string", properties["NAME"]);
        Assert.Equal("xsd:double", properties["VALUE"]);
        Assert.Equal("xsd:dateTime", properties["DATUM"]);
        Assert.False(properties.ContainsKey("OBJECTID"));
    }

    [Fact]
    public void DescribeFeatureType_Gml2_FeatureSubstitutionGroup()
    {
        var xsd = XElement.Parse(System.Text.Encoding.UTF8.GetString(WfsSchemaWriter.Write([Collection()], GmlFormat.Gml2)));
        XNamespace xs = WfsVersions.XsdNamespace;

        Assert.Equal("gml:_Feature", xsd.Elements(xs + "element").Single().Attribute("substitutionGroup")!.Value);
        Assert.Contains("2.1.2", xsd.Element(xs + "import")!.Attribute("schemaLocation")!.Value);
    }

    #endregion

    #region Content type

    [Theory]
    [InlineData(GmlFormat.Gml2)]
    [InlineData(GmlFormat.Gml31)]
    [InlineData(GmlFormat.Gml32)]
    public void ContentType_IsAValidMediaType(GmlFormat format)
    {
        // regression: ASP.NET rejected text/xml; subtype=gml/3.1.1 (unquoted / in a parameter value)
        var mediaType = Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse(WfsVersions.ContentType(format));

        Assert.NotNull(mediaType.MediaType.Value);
    }

    [Theory]
    [InlineData("text/xml; subtype=\"gml/3.1.1\"", WfsVersion.V110, GmlFormat.Gml31)]
    [InlineData("text/xml; subtype=gml/3.1.1", WfsVersion.V200, GmlFormat.Gml31)]
    [InlineData("GML2", WfsVersion.V100, GmlFormat.Gml2)]
    [InlineData("application/gml+xml; version=3.2", WfsVersion.V200, GmlFormat.Gml32)]
    [InlineData(null, WfsVersion.V200, GmlFormat.Gml32)]
    public void ParseGmlOutputFormat_QuotedAndUnquoted(string? outputFormat, WfsVersion version, GmlFormat expected)
        => Assert.Equal(expected, WfsVersions.ParseGmlOutputFormat(outputFormat, version));

    [Fact]
    public void ParseGmlOutputFormat_Gml32ForVersion100_NotSupported()
        => Assert.Null(WfsVersions.ParseGmlOutputFormat("application/gml+xml; version=3.2", WfsVersion.V100));

    #endregion

    #region Exception report

    [Fact]
    public void ExceptionReport200_Ows11WithLocator()
    {
        var ex = WfsException.InvalidParameterValue("typeNames", "Unknown feature type 'x'");
        var xml = XElement.Parse(System.Text.Encoding.UTF8.GetString(WfsExceptionReport.Write(WfsVersion.V200, ex.Code, ex.Message, ex.Locator)));
        XNamespace ows = WfsVersions.Ows11Namespace;

        Assert.Equal(ows + "ExceptionReport", xml.Name);
        var exception = xml.Element(ows + "Exception")!;
        Assert.Equal("InvalidParameterValue", exception.Attribute("exceptionCode")!.Value);
        Assert.Equal("typeNames", exception.Attribute("locator")!.Value);
        Assert.Equal(400, ex.HttpStatusCode);
    }

    [Fact]
    public void ExceptionReport100_ServiceExceptionReport()
    {
        var xml = XElement.Parse(System.Text.Encoding.UTF8.GetString(WfsExceptionReport.Write(WfsVersion.V100, "OperationNotSupported", "Operation Transaction is not supported", "Transaction")));

        Assert.Equal(XNamespace.Get(WfsVersions.OgcNamespace) + "ServiceExceptionReport", xml.Name);
        Assert.Equal(501, WfsException.OperationNotSupported("Transaction").HttpStatusCode);
    }

    #endregion

    #region Helper

    private static async Task<XElement> GetFeature(WfsVersion version, GmlFormat format, string srsName)
    {
        var collection = Collection();
        var crs = WFSRequest.ParseCrs(srsName, "srsName");
        var sref = crs.ToSpatialReference();

        var filter = new QueryFilter();
        var features = await FeatureReader.ReadListAsync(null, collection, filter, sref, 0, 10);

        var bytes = new WfsFeatureWriter(version, format, crs.Name, version.SwapAxes(crs, sref))
            .Write([new WfsFeatureSet(collection, features)], null, DateTime.UtcNow);

        return XElement.Parse(System.Text.Encoding.UTF8.GetString(bytes));
    }

    private static (double, double) Pos(string pos)
    {
        var parts = pos.Split(' ');
        return (double.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture),
                double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture));
    }

    private static OgcCollectionLayer Collection() => WfsTestData.Collection();

    #endregion
}
