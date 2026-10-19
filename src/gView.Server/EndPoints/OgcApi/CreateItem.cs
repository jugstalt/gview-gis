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
/// Part 4: POST .../items => 201 Created + Location
/// </summary>
public class CreateItem : OgcApiBaseEndpoint
{
    public CreateItem()
        : base(Routes.Items, Handler, "POST")
    {
    }

    static private Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManager,
                [FromServices] MapServiceManager mapServiceManager,
                [FromServices] ILogger<CreateItem> logger,
                string folder,
                string service,
                string collectionId
            ) => HandleSecureAsync(httpContext, mapServiceManager, loginManager, logger, folder, service, OgcApiOperations.CreateItem,
            async request =>
            {
                var id = await request.Service.CreateItemAsync(
                    collectionId,
                    await request.ReadBodyAsync(),
                    httpContext.Request.Headers["Content-Crs"].ToString());

                return id.HasValue
                    ? Results.Created(request.Service.ItemUrl(collectionId, id.Value), null)
                    : Results.StatusCode(201);
            },
            negotiateFormat: false);
}
