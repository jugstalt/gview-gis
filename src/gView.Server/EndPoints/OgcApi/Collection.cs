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

public class Collection : OgcApiBaseEndpoint
{
    public Collection()
        : base(Routes.Collection, Handler)
    {
    }

    static private Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManager,
                [FromServices] MapServiceManager mapServiceManager,
                [FromServices] ILogger<Collection> logger,
                string folder,
                string service,
                string collectionId
            ) => HandleSecureAsync(httpContext, mapServiceManager, loginManager, logger, folder, service, OgcApiOperations.Collection,
            request =>
            {
                var collection = request.Service.GetCollection(collectionId);

                return Task.FromResult(request.Format == OgcApiFormat.Html
                    ? Html(request, collection.Title ?? collection.Id, HtmlRenderer.Collection(collection, request.Service.ServiceUrl))
                    : Json(collection));
            });
}
