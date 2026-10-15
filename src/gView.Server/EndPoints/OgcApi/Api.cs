#nullable enable

using gView.Interoperability.OgcApi;
using gView.Interoperability.OgcApi.Html;
using gView.Interoperability.OgcApi.Request;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace gView.Server.EndPoints.OgcApi;

public class Api : OgcApiBaseEndpoint
{
    public Api()
        : base(Routes.Api, Handler)
    {
    }

    static private Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManager,
                [FromServices] MapServiceManager mapServiceManager,
                [FromServices] ILogger<Api> logger,
                string folder,
                string service
            ) => HandleSecureAsync(httpContext, mapServiceManager, loginManager, logger, folder, service, OgcApiOperations.Api,
            request =>
            {
                return Task.FromResult(request.Format == OgcApiFormat.Html
                    ? Html(request, "API", HtmlRenderer.SwaggerUi($"{request.Service.ApiUrl}?f=json", request.Service.ServiceUrl))
                    : Json(request.Service.GetOpenApiDocument(), OgcApiMediaTypes.OpenApiJson));
            });
}
