using gView.Framework.Core.MapServer;
using gView.Framework.OGC.KML;
using System.Text;

namespace gView.Server.EndPoints.GeoJsonService;

internal class Routes
{
    public const string Base = "geojsonservice/v1";

    public const string GetInfo = "info";
    public const string GetServices = "services";
    public const string GetToken = "token";
    public const string GetServiceCapabilities = "capabilities";
    public const string GetMap = "map";
    public const string GetLegend = "legend";
    public const string GetFeatures = "query";
    public const string EditFeatures = "features";
}

internal class RouteBuilder
{
    private readonly StringBuilder _sb;

    public RouteBuilder()
    {
        _sb = new();
        _sb.Append(Routes.Base);
    }

    public string Build() => _sb.ToString();

    public RouteBuilder UseCapabilitesRoute(IMapService mapService)
    {
        _sb.Append($"/{Routes.GetServices}/{mapService.Fullname}/{Routes.GetServiceCapabilities}");

        return this;
    }

    public RouteBuilder AddCrs(string crsName)
    {
        _sb.Append(_sb.ToString().Contains("?") ? "&" : "?");
        _sb.Append($"crs={crsName}");

        return this;
    }
}