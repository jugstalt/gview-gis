#nullable enable

using gView.Interoperability.OgcApi.Request;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace gView.Server.EndPoints.OgcApi;

/// <summary>
/// Part 4: PUT .../items/{featureId} => 204
/// </summary>
public class ReplaceItem : OgcApiBaseEndpoint
{
    public ReplaceItem()
        : base(Routes.Item, Handler, "PUT")
    {
    }

    static private Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManager,
                [FromServices] MapServiceManager mapServiceManager,
                [FromServices] ILogger<ReplaceItem> logger,
                string folder,
                string service,
                string collectionId,
                string featureId
            ) => HandleSecureAsync(httpContext, mapServiceManager, loginManager, logger, folder, service, OgcApiOperations.ReplaceItem,
            async request =>
            {
                await request.Service.ReplaceItemAsync(
                    collectionId,
                    featureId,
                    await request.ReadBodyAsync(),
                    httpContext.Request.Headers["Content-Crs"].ToString());

                return Results.NoContent();
            },
            negotiateFormat: false);
}
