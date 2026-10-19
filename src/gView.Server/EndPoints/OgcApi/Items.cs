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

namespace gView.Server.EndPoints.OgcApi;

public class Items : OgcApiBaseEndpoint
{
    public Items()
        : base(Routes.Items, Handler)
    {
    }

    static private Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManager,
                [FromServices] MapServiceManager mapServiceManager,
                [FromServices] ILogger<Items> logger,
                string folder,
                string service,
                string collectionId
            ) => HandleSecureAsync(httpContext, mapServiceManager, loginManager, logger, folder, service, OgcApiOperations.Items,
            async request =>
            {
                var items = await request.Service.GetItemsAsync(collectionId, request.Parameters);

                return request.Format == OgcApiFormat.Html
                    ? Html(request, items.Collection.Title, HtmlRenderer.Items(items, request.Service), items.Crs)
                    : Bytes(items.ToGeoJson(), OgcApiMediaTypes.GeoJson, request, items.Crs);
            });
}
