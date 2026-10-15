#nullable enable

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

public class Collections : OgcApiBaseEndpoint
{
    public Collections()
        : base(Routes.Collections, Handler)
    {
    }

    static private Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManager,
                [FromServices] MapServiceManager mapServiceManager,
                [FromServices] ILogger<Collections> logger,
                string folder,
                string service
            ) => HandleSecureAsync(httpContext, mapServiceManager, loginManager, logger, folder, service, OgcApiOperations.Collections,
            request =>
            {
                var collections = request.Service.GetCollections();

                return Task.FromResult(request.Format == OgcApiFormat.Html
                    ? Html(request, "Collections", HtmlRenderer.Collections(collections, request.Service.ServiceUrl))
                    : Json(collections));
            });
}
