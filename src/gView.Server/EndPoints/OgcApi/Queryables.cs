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

public class Queryables : OgcApiBaseEndpoint
{
    public Queryables()
        : base(Routes.Queryables, Handler)
    {
    }

    static private Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManager,
                [FromServices] MapServiceManager mapServiceManager,
                [FromServices] ILogger<Queryables> logger,
                string folder,
                string service,
                string collectionId
            ) => HandleSecureAsync(httpContext, mapServiceManager, loginManager, logger, folder, service, OgcApiOperations.Queryables,
            request =>
            {
                var queryables = request.Service.GetQueryables(collectionId);

                return Task.FromResult(request.Format == OgcApiFormat.Html
                    ? Html(request, "Queryables", HtmlRenderer.Queryables(queryables, request.Service.ServiceUrl, collectionId))
                    : Json(queryables, OgcApiMediaTypes.SchemaJson));
            });
}
