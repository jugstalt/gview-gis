﻿using gView.Framework.Core.MapServer;
using gView.Framework.OGC.KML;
using gView.Server.AppCode;
using Microsoft.AspNetCore.Http;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
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
        UseServiceRootRoute(mapService);
        _sb.Append($"/{Routes.GetServiceCapabilities}");

        return this;
    }

    public RouteBuilder UseServiceRootRoute(IMapService mapService)
    {
        _sb.Append($"/{Routes.GetServices}/{mapService.Fullname}");

        return this;
    }

    public RouteBuilder AddCrs(string crsName)
    {
        _sb.Append(_sb.ToString().Contains("?") ? "&" : "?");
        _sb.Append($"crs={crsName}");

        return this;
    }

    public RouteBuilder AppendUrlPath(string path)
    {
        _sb.Append($"/{path}");

        return this;
    }

    public RouteBuilder AppendQueryString(string queryString)
    {
        _sb.Append(_sb.ToString().Contains("?") ? "&" : "?");
        _sb.Append(queryString);

        return this;
    }
}