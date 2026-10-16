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
/// Part 4: PATCH .../items/{featureId} (JSON merge patch) => 204
/// </summary>
public class UpdateItem : OgcApiBaseEndpoint
{
    public UpdateItem()
        : base(Routes.Item, Handler, "PATCH")
    {
    }

    static private Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManager,
                [FromServices] MapServiceManager mapServiceManager,
                [FromServices] ILogger<UpdateItem> logger,
                string folder,
                string service,
                string collectionId,
                string featureId
            ) => HandleSecureAsync(httpContext, mapServiceManager, loginManager, logger, folder, service, OgcApiOperations.UpdateItem,
            async request =>
            {
                await request.Service.UpdateItemAsync(
                    collectionId,
                    featureId,
                    await request.ReadBodyAsync(),
                    httpContext.Request.Headers["Content-Crs"].ToString());

                return Results.NoContent();
            },
            negotiateFormat: false);
}
