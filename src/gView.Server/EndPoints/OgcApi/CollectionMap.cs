#nullable enable

using gView.Interoperability.OgcApi.Request;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace gView.Server.EndPoints.OgcApi;

public class CollectionMap : OgcApiBaseEndpoint
{
    public CollectionMap()
        : base(Routes.CollectionMap, Handler)
    {
    }

    static private Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManager,
                [FromServices] MapServiceManager mapServiceManager,
                [FromServices] ILogger<CollectionMap> logger,
                string folder,
                string service,
                string collectionId
            ) => HandleSecureAsync(httpContext, mapServiceManager, loginManager, logger, folder, service, OgcApiOperations.Map,
            async request =>
            {
                var map = await request.Service.GetMapAsync(collectionId, request.Parameters, httpContext.Request.Headers.Accept.ToString());

                return Image(request, map);
            },
            negotiateFormat: false);
}
