#nullable enable

using gView.Framework.Common.Extensions;
using gView.Framework.Core.MapServer;
using gView.GeoJsonService.DTOs;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace gView.Server.EndPoints.GeoJsonService;

/// <summary>
/// .../api: the OpenAPI description of the service (only the operations the user may use); ?f=html shows it with Scalar
/// </summary>
public class GetApi : BaseApiEndpoint
{
    public GetApi()
        : base(
                [
                    $"{Routes.Base}/{Routes.GetServices}/{{folder}}/{{service}}/{Routes.Api}",
                    $"{Routes.Base}/{Routes.GetServices}/{{service}}/{Routes.Api}"
                ],
                Handler
            )
    {
    }

    private static Delegate Handler => (
            HttpContext httpContext,
            [FromServices] LoginManager loginManagerService,
            [FromServices] MapServiceManager mapServerService,
            [FromServices] ILogger<GetApi> logger,
            string folder = "",
            string service = "") => HandleSecureAsync<GetServiceCapabilitiesRequest>(
                    httpContext, mapServerService, loginManagerService, logger,
                    folder, service,
                    async (serviceRequestContext, mapService, identity, _) =>
     {
         string path = String.IsNullOrEmpty(folder) ? service : $"{folder}/{service}";
         string serviceUrl = $"{serviceRequestContext.ServiceRequest.OnlineResource.OrTake(mapServerService.Options.OnlineResource)}/{Routes.Base}/{Routes.GetServices}/{path}";

         if ("html".Equals(httpContext.Request.Query["f"].ToString(), StringComparison.OrdinalIgnoreCase))
         {
             // the page loads the JSON from the same server (relative to the page, keeps the token in the query)
             var token = httpContext.Request.Query["token"].ToString();
             var specUrl = $"{Routes.Api}?f=json{(String.IsNullOrEmpty(token) ? "" : $"&token={Uri.EscapeDataString(token)}")}";
             return Results.Content(
                 EndPoints.ApiReferencePage.Html($"{mapService.Name} – GeoJson Service", specUrl, $"{httpContext.Request.PathBase.Value?.TrimEnd('/')}/lib"),
                 "text/html", Encoding.UTF8);
         }

         var accessTypes = await mapService.GetAccessTypes(identity);
         return Results.Text(GeoJsonServiceOpenApi.Build(serviceUrl, mapService.Name, accessTypes).ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }),
                             "application/vnd.oai.openapi+json;version=3.1", Encoding.UTF8);
     });
}
