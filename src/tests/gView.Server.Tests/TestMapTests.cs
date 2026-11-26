using gView.Framework.Core.MapServer;
using gView.Framework.GeoJsonService.Request;
using gView.Interoperability.GeoServices.Request;
using gView.Interoperability.OGC;
using gView.Interoperability.OGC.Request.WMTS;
using gView.Interoperability.OgcApi.Request;
using gView.Server.AppCode;
using gView.Server.AppCode.Extensions;
using gView.Server.Extensions;
using gView.Server.Models;

namespace gView.Server.Tests;

/// <summary>The test map of a service in the server UI: access, interfaces, projections</summary>
public class TestMapTests
{
    private static BrowseServicesServiceModel ServiceModel(string folder, params string[] geoServiceTypes) => new()
    {
        Server = "https://gview.example.com",
        OnlineResource = "https://gview.example.com/ogc?",
        MapService = new MapService(null!, null!, Path.Combine(Path.GetTempPath(), "basis.mxl"), folder, MapServiceType.MXL),
        Interpreters = new IServiceRequestInterpreter[]
        {
            new GeoJsonServiceRequestInterpreter(),
            new WMSRequest(),
            new GeoServicesRestInterperter(),
            new OgcApiRequestInterpreter(),
            new WFSRequest(),
            new WMTSRequest()
        },
        GeoServiceTypes = new HashSet<string>(geoServiceTypes)
    };

    #region Access

    [Fact]
    public void CanUseTestMap_ManageUserFromAdminNetwork()
    {
        var token = new AuthToken("admin", AuthToken.AuthTypes.Manage, TimeSpan.FromMinutes(30));

        Assert.True(token.CanUseTestMap(isFromAdminNetwork: true));
    }

    [Fact]
    public void CanUseTestMap_NotOutsideTheAdminNetwork()
    {
        var token = new AuthToken("admin", AuthToken.AuthTypes.Manage, TimeSpan.FromMinutes(30));

        Assert.False(token.CanUseTestMap(isFromAdminNetwork: false));
    }

    [Fact]
    public void CanUseTestMap_NotForTokenUsersOrAnonymous()
    {
        var tokenUser = new AuthToken("user", AuthToken.AuthTypes.Tokenuser, TimeSpan.FromMinutes(30));

        Assert.False(tokenUser.CanUseTestMap(isFromAdminNetwork: true));
        Assert.False(AuthToken.Anonymous.CanUseTestMap(isFromAdminNetwork: true));
        Assert.False(((AuthToken)null!).CanUseTestMap(isFromAdminNetwork: true));
    }

    #endregion

    #region Interfaces

    [Fact]
    public void TestMapInterfaces_InSelectorOrderWithBaseUrls()
    {
        var interfaces = ServiceModel("webmerc", "MapServer").TestMapInterfaces().ToArray();

        Assert.Equal(new[] { "wms", "wmts", "wfs", "ogcapi", "geoservices", "geojsonservice" }, interfaces.Select(i => i.Key));
        Assert.Equal("https://gview.example.com/geoservices/rest/services/webmerc/basis/MapServer/WmsServer", interfaces[0].Url);
        Assert.Equal("https://gview.example.com/ogc/webmerc@basis", interfaces[1].Url);
        Assert.Equal("https://gview.example.com/ogc/webmerc@basis", interfaces[2].Url);
        Assert.Equal("https://gview.example.com/ogcapi/webmerc/basis", interfaces[3].Url);
        Assert.Equal("https://gview.example.com/geoservices/rest/services/webmerc/basis/MapServer", interfaces[4].Url);
        Assert.Equal("https://gview.example.com/geojsonservice/v1/services/webmerc/basis", interfaces[5].Url);
    }

    [Fact]
    public void TestMapInterfaces_RootFolder()
    {
        var interfaces = ServiceModel("", "MapServer").TestMapInterfaces().ToArray();

        Assert.Equal("https://gview.example.com/geojsonservice/v1/services/basis", interfaces.Single(i => i.Key == "geojsonservice").Url);
        Assert.Equal("https://gview.example.com/ogcapi/_/basis", interfaces.Single(i => i.Key == "ogcapi").Url);
        Assert.Equal("https://gview.example.com/ogc/basis", interfaces.Single(i => i.Key == "wmts").Url);
    }

    [Fact]
    public void TestMapInterfaces_GeoServicesOnlyForMapServers()
    {
        var interfaces = ServiceModel("webmerc", "FeatureServer").TestMapInterfaces();

        Assert.DoesNotContain(interfaces, i => i.Key == "geoservices");
    }

    [Fact]
    public void TestMapInterfaces_OnlyAvailableInterpreters()
    {
        var model = ServiceModel("webmerc", "MapServer");
        model.Interpreters = new IServiceRequestInterpreter[] { new GeoJsonServiceRequestInterpreter() };

        Assert.Equal(new[] { "geojsonservice" }, model.TestMapInterfaces().Select(i => i.Key));
    }

    #endregion

    #region Projections

    [Theory]
    [InlineData("31256,3857", new[] { 31256, 3857 })]
    [InlineData("EPSG:31256, epsg:4326,EPSG:31256", new[] { 31256, 4326 })]
    [InlineData("x,-1,0,3857;", new int[0])]
    [InlineData("", new int[0])]
    [InlineData(null, new int[0])]
    public void ParseEpsgCodes(string? codes, int[] expected)
    {
        Assert.Equal(expected, codes.ParseEpsgCodes());
    }

    [Fact]
    public void ParseEpsgCodes_Limited()
    {
        var codes = String.Join(",", Enumerable.Range(1, 100));

        Assert.Equal(SpatialReferenceExtensions.MaxTestMapProjections, codes.ParseEpsgCodes().Length);
    }

    [Fact]
    public void TestMapProjections_Proj4AndAxisOrder()
    {
        var projections = new[] { 3857, 4326, 999999 }.TestMapProjections();

        Assert.True(projections.ContainsKey("EPSG:3857"));
        Assert.Contains("+proj=merc", projections["EPSG:3857"].Proj4);
        Assert.Equal("enu", projections["EPSG:3857"].Axis);

        Assert.Contains("+proj=longlat", projections["EPSG:4326"].Proj4);
        Assert.Equal("neu", projections["EPSG:4326"].Axis);   // WMS 1.3.0: lat/lon

        Assert.False(projections.ContainsKey("EPSG:999999"));
    }

    #endregion
}
