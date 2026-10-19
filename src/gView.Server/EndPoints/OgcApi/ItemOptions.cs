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

public class ItemOptions : OgcApiBaseEndpoint
{
    public ItemOptions()
        : base(Routes.Item, Handler, "OPTIONS")
    {
    }

    static private Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManager,
                [FromServices] MapServiceManager mapServiceManager,
                [FromServices] ILogger<ItemOptions> logger,
                string folder,
                string service,
                string collectionId,
                string featureId
            ) => HandleSecureAsync(httpContext, mapServiceManager, loginManager, logger, folder, service, OgcApiOperations.Options,
            request =>
            {
                request.Service.GetFeatureCollectionLayer(collectionId);  // 404 for unknown collections

                var methods = new List<string> { "GET", "OPTIONS" };
                var statements = request.AccessTypes.HasFlag(AccessTypes.Edit)
                    ? request.Service.EditStatements(collectionId)
                    : EditStatements.NONE;

                if (statements.HasFlag(EditStatements.UPDATE))
                {
                    methods.Add("PUT");
                    methods.Add("PATCH");
                }
                if (statements.HasFlag(EditStatements.DELETE))
                {
                    methods.Add("DELETE");
                }

                return Task.FromResult(ItemsOptions.AllowResult(httpContext, methods));
            },
            negotiateFormat: false);
}
