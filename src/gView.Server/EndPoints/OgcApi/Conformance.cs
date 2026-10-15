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

public class Conformance : OgcApiBaseEndpoint
{
    public Conformance()
        : base(Routes.Conformance, Handler)
    {
    }

    static private Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManager,
                [FromServices] MapServiceManager mapServiceManager,
                [FromServices] ILogger<Conformance> logger,
                string folder,
                string service
            ) => HandleSecureAsync(httpContext, mapServiceManager, loginManager, logger, folder, service, OgcApiOperations.Conformance,
            request =>
            {
                var conformance = request.Service.GetConformance();

                return Task.FromResult(request.Format == OgcApiFormat.Html
                    ? Html(request, "Conformance", HtmlRenderer.Conformance(conformance, request.Service.ServiceUrl))
                    : Json(conformance));
            });
}
