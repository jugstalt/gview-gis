#nullable enable

using gView.Endpoints.Abstractions;
using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.MapServer;
using gView.Framework.Data.Extensions;
using gView.Interoperability.OgcApi;
using gView.Interoperability.OgcApi.Maps;
using gView.Interoperability.OgcApi.Models;
using gView.Interoperability.OgcApi.Request;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace gView.Server.EndPoints.OgcApi;

public enum OgcApiFormat
{
    Json,
    Html
}

/// <summary>
/// A request for one OGC API resource of a map service
/// </summary>
public class OgcApiRequest
{
    public OgcApiRequest(HttpContext httpContext, OgcApiFeatureService service, OgcApiFormat format, AccessTypes accessTypes)
    {
        HttpContext = httpContext;
        Service = service;
        Format = format;
        AccessTypes = accessTypes;
        Parameters = httpContext.Request.Query
            .ToDictionary(q => q.Key, q => q.Value.ToString());
    }

    public HttpContext HttpContext { get; }
    public OgcApiFeatureService Service { get; }
    public OgcApiFormat Format { get; }
    public IReadOnlyDictionary<string, string> Parameters { get; }

    /// <summary>
    /// The access types (map, query, edit) of the user for this service
    /// </summary>
    public AccessTypes AccessTypes { get; }

    /// <summary>
    /// Reads the GeoJSON body of a Part 4 request. Other content types (e.g. form data, which the
    /// token middleware would consume) are rejected with 415.
    /// </summary>
    public async Task<string> ReadBodyAsync()
    {
        string contentType = HttpContext.Request.ContentType ?? "";
        if (!contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
        {
            throw new OgcApiException(415, "UnsupportedMediaType", $"Content-Type '{contentType}' is not supported, use {OgcApiMediaTypes.GeoJson}");
        }

        using var reader = new System.IO.StreamReader(HttpContext.Request.Body);
        return await reader.ReadToEndAsync();
    }
}

public class OgcApiBaseEndpoint : IApiEndpoint
{
    private static readonly OgcApiRequestInterpreter RequestInterpreter = new OgcApiRequestInterpreter();

    static internal readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    private readonly string _route;
    private readonly Delegate _delegate;
    private readonly string _httpMethod;

    protected OgcApiBaseEndpoint(string route, Delegate @delegate, string httpMethod = "GET")
    {
        _route = route;
        _delegate = @delegate;
        _httpMethod = httpMethod;
    }

    public void Register(IEndpointRouteBuilder app)
    {
        app.MapMethods(_route, [_httpMethod], _delegate);
    }

    async static protected Task<IResult> HandleSecureAsync(
                HttpContext httpContext,
                MapServiceManager mapServiceManager,
                LoginManager loginManager,
                ILogger logger,
                string folder,
                string service,
                string operation,
                Func<OgcApiRequest, Task<IResult>> action,
                bool negotiateFormat = true)
    {
        try
        {
            // image (map) and edit resources don't negotiate json/html
            var format = negotiateFormat ? GetFormat(httpContext.Request) : OgcApiFormat.Json;

            if (folder == Routes.RootFolder)
            {
                folder = "";
            }

            var authToken = loginManager.GetAuthToken(httpContext.Request);
            var identity = new Identity(authToken.Username, authToken.IsManageUser);

            var serviceRequest = new ServiceRequest(service, folder, "")
            {
                Method = operation,
                Identity = identity,
                OnlineResource = mapServiceManager.Options.OnlineResource,
                OutputUrl = mapServiceManager.Options.OutputUrl
            };

            var mapService = mapServiceManager.Instance.GetMapService(service, folder)
                ?? throw OgcApiException.NotFound($"Unknown service '{service}'");

            var serviceRequestContext = await ServiceRequestContext.TryCreate(
                mapServiceManager.Instance,
                RequestInterpreter,
                serviceRequest,
                checkSecurity: false);

            await mapService.CheckAccess(serviceRequestContext);  // may also set the folder's online resource
            var accessTypes = await mapService.GetAccessTypes(identity);

            IResult? result = null;
            Exception? error = null;

            await mapServiceManager.TaskQueue.AwaitRequest(async (context) =>
            {
                try
                {
                    using var serviceMap = await mapServiceManager.Instance.GetServiceMapAsync(mapService)
                        ?? throw new OgcApiException(503, "ServiceUnavailable", $"Can't load service '{service}'");

                    var ogcApiService = new OgcApiFeatureService(
                        serviceMap,
                        ServiceUrl(httpContext, serviceRequest.OnlineResource, folder, service));

                    result = await action(new OgcApiRequest(httpContext, ogcApiService, format, accessTypes));
                }
                catch (Exception ex)
                {
                    error = ex;  // the task queue swallows exceptions
                }
            }, serviceRequestContext);

            if (error is not null)
            {
                ExceptionDispatchInfo.Throw(error);
            }

            return result ?? throw new Exception("Request was not processed");
        }
        catch (Exception ex)
        {
            return ErrorResult(httpContext, logger, ex);
        }
    }

    #region Results

    protected static IResult Json(object value, string contentType = OgcApiMediaTypes.Json)
        => Results.Text(JsonSerializer.Serialize(value, value.GetType(), JsonOptions), contentType, Encoding.UTF8);

    protected static IResult Bytes(byte[] content, string contentType, OgcApiRequest request, CrsReference? contentCrs = null)
    {
        if (contentCrs is not null)
        {
            request.HttpContext.Response.Headers["Content-Crs"] = $"<{contentCrs.Uri}>";
        }
        return Results.Bytes(content, contentType);
    }

    protected static IResult Image(OgcApiRequest request, MapResult map)
    {
        request.HttpContext.Response.Headers["Content-Crs"] = $"<{map.Crs.Uri}>";
        request.HttpContext.Response.Headers["Content-Bbox"] = map.ContentBboxHeader;
        return Results.Bytes(map.Image, map.ContentType);
    }

    protected static IResult Html(OgcApiRequest request, string title, string body, CrsReference? contentCrs = null)
    {
        if (contentCrs is not null)
        {
            request.HttpContext.Response.Headers["Content-Crs"] = $"<{contentCrs.Uri}>";
        }
        return Results.Content(OgcApiHtmlPage.Render(request.HttpContext, title, body), "text/html", Encoding.UTF8);
    }

    #endregion

    #region Helper

    static internal OgcApiFormat GetFormat(HttpRequest request)
    {
        string? f = request.Query["f"];

        if (!String.IsNullOrEmpty(f))
        {
            return f.ToLowerInvariant() switch
            {
                "json" or "geojson" => OgcApiFormat.Json,
                "html" => OgcApiFormat.Html,
                _ => throw OgcApiException.BadRequest($"Unsupported format f={f}")
            };
        }

        string accept = request.Headers.Accept.ToString();
        return accept.Contains("text/html", StringComparison.OrdinalIgnoreCase)
            ? OgcApiFormat.Html
            : OgcApiFormat.Json;
    }

    static internal string BaseUrl(HttpContext httpContext, string? onlineResource)
    {
        if (!String.IsNullOrWhiteSpace(onlineResource))
        {
            return onlineResource.TrimEnd('/');
        }

        var request = httpContext.Request;
        return $"{request.Scheme}://{request.Host}{request.PathBase}".TrimEnd('/');
    }

    static internal string ServiceUrl(HttpContext httpContext, string? onlineResource, string folder, string service)
        => $"{BaseUrl(httpContext, onlineResource)}/{Routes.Base}/{Uri.EscapeDataString(String.IsNullOrEmpty(folder) ? Routes.RootFolder : folder)}/{Uri.EscapeDataString(service)}";

    static internal IResult ErrorResult(HttpContext httpContext, ILogger logger, Exception ex)
    {
        (int statusCode, string code, string message) = ex switch
        {
            OgcApiException oae => (oae.StatusCode, oae.Code, oae.Message),
            TokenRequiredException tre => (401, "Unauthorized", tre.Message),
            InvalidTokenException ite => (401, "Unauthorized", ite.Message),
            MapServerAuthException mae => (403, "Forbidden", mae.Message),
            SqlDangerousStatementExceptions dse => (400, "InvalidParameterValue", dse.Message),
            MapServerException mse => (mse.StatusCode ?? 400, "InvalidParameterValue", mse.Message),
            _ => (500, "ServerError", "internal server error")
        };

        if (statusCode == 401)
        {
            httpContext.Response.Headers.WWWAuthenticate = "Basic realm=\"gView Server\"";
        }

        if (statusCode >= 500)
        {
            logger.LogError(ex, "Handle OGC API Request: {message}", ex.Message);
        }
        else
        {
            logger.LogWarning("Handle OGC API Request: {message}", ex.Message);
        }

        return Results.Text(
            JsonSerializer.Serialize(new ExceptionResponse() { Code = code, Description = message }, JsonOptions),
            OgcApiMediaTypes.Json,
            Encoding.UTF8,
            statusCode);
    }

    #endregion
}
