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

public class Item : OgcApiBaseEndpoint
{
    public Item()
        : base(Routes.Item, Handler)
    {
    }

    static private Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManager,
                [FromServices] MapServiceManager mapServiceManager,
                [FromServices] ILogger<Item> logger,
                string folder,
                string service,
                string collectionId,
                string featureId
            ) => HandleSecureAsync(httpContext, mapServiceManager, loginManager, logger, folder, service, OgcApiOperations.Item,
            async request =>
            {
                var item = await request.Service.GetItemAsync(collectionId, featureId, request.Parameters);

                return request.Format == OgcApiFormat.Html
                    ? Html(request, item.Collection.Title, HtmlRenderer.Item(item, request.Service), item.Crs)
                    : Bytes(item.ToGeoJson(), OgcApiMediaTypes.GeoJson, request, item.Crs);
            });
}
