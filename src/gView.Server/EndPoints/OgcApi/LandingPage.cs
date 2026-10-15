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

public class LandingPage : OgcApiBaseEndpoint
{
    public LandingPage()
        : base(Routes.Service, Handler)
    {
    }

    static private Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManager,
                [FromServices] MapServiceManager mapServiceManager,
                [FromServices] ILogger<LandingPage> logger,
                string folder,
                string service
            ) => HandleSecureAsync(httpContext, mapServiceManager, loginManager, logger, folder, service, OgcApiOperations.LandingPage,
            request =>
            {
                var landingPage = request.Service.GetLandingPage();

                return Task.FromResult(request.Format == OgcApiFormat.Html
                    ? Html(request, landingPage.Title ?? "", HtmlRenderer.LandingPage(landingPage))
                    : Json(landingPage));
            });
}
