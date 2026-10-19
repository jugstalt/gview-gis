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
/// Part 4: DELETE .../items/{featureId} => 204
/// </summary>
public class DeleteItem : OgcApiBaseEndpoint
{
    public DeleteItem()
        : base(Routes.Item, Handler, "DELETE")
    {
    }

    static private Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManager,
                [FromServices] MapServiceManager mapServiceManager,
                [FromServices] ILogger<DeleteItem> logger,
                string folder,
                string service,
                string collectionId,
                string featureId
            ) => HandleSecureAsync(httpContext, mapServiceManager, loginManager, logger, folder, service, OgcApiOperations.DeleteItem,
            async request =>
            {
                await request.Service.DeleteItemAsync(collectionId, featureId);

                return Results.NoContent();
            },
            negotiateFormat: false);
}
