#nullable enable

using gView.Framework.Core.MapServer;
using gView.Framework.Editor.Core;
using gView.Interoperability.OgcApi.Request;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Server.EndPoints.OgcApi;

/// <summary>
/// QGIS asks with OPTIONS which methods are allowed (Part 4). The edit methods are only listed,
/// if the layer allows the statement and the user has the edit right.
/// CORS preflights are answered by the CORS middleware before.
/// </summary>
public class ItemsOptions : OgcApiBaseEndpoint
{
    public ItemsOptions()
        : base(Routes.Items, Handler, "OPTIONS")
    {
    }

    static private Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManager,
                [FromServices] MapServiceManager mapServiceManager,
                [FromServices] ILogger<ItemsOptions> logger,
                string folder,
                string service,
                string collectionId
            ) => HandleSecureAsync(httpContext, mapServiceManager, loginManager, logger, folder, service, OgcApiOperations.Options,
            request =>
            {
                request.Service.GetFeatureCollectionLayer(collectionId);  // 404 for unknown collections

                var methods = new List<string> { "GET", "OPTIONS" };
                var statements = request.AccessTypes.HasFlag(AccessTypes.Edit)
                    ? request.Service.EditStatements(collectionId)
                    : EditStatements.NONE;

                if (statements.HasFlag(EditStatements.INSERT))
                {
                    methods.Add("POST");
                }

                return Task.FromResult(AllowResult(httpContext, methods));
            },
            negotiateFormat: false);

    static internal IResult AllowResult(HttpContext httpContext, IEnumerable<string> methods)
    {
        httpContext.Response.Headers.Allow = String.Join(", ", methods);
        return Results.Ok();
    }
}
