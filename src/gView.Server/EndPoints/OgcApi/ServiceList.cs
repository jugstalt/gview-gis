#nullable enable

using gView.Endpoints.Abstractions;
using gView.Framework.Common;
using gView.Framework.Core.MapServer;
using gView.Interoperability.OgcApi;
using gView.Interoperability.OgcApi.Html;
using gView.Interoperability.OgcApi.Models;
using OgcModels = gView.Interoperability.OgcApi.Models;
using gView.Server.AppCode.Extensions;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace gView.Server.EndPoints.OgcApi;

/// <summary>
/// /ogcapi: links to the landing pages of all services the user has access to (not an OGC resource)
/// </summary>
public class ServiceList : IApiEndpoint
{
    public void Register(IEndpointRouteBuilder app)
    {
        app.MapGet(Routes.Base, Handler);
    }

    static private Delegate Handler => async (
                HttpContext httpContext,
                [FromServices] LoginManager loginManager,
                [FromServices] MapServiceManager mapServiceManager,
                [FromServices] ILogger<ServiceList> logger
            ) =>
    {
        try
        {
            var format = OgcApiBaseEndpoint.GetFormat(httpContext.Request);
            var authToken = loginManager.GetAuthToken(httpContext.Request);
            var identity = new Identity(authToken.Username, authToken.IsManageUser);
            string baseUrl = $"{OgcApiBaseEndpoint.BaseUrl(httpContext, mapServiceManager.Options.OnlineResource)}/{Routes.Base}";

            mapServiceManager.ReloadServices("", true);

            var serviceList = new OgcModels.ServiceList()
            {
                Links =
                [
                    new Link($"{baseUrl}?f=json", OgcApiRelations.Self, OgcApiMediaTypes.Json, "This document"),
                    new Link($"{baseUrl}?f=html", OgcApiRelations.Alternate, OgcApiMediaTypes.Html, "This document as HTML")
                ]
            };

            var folders = mapServiceManager.MapServices
                .Where(s => s.Type == MapServiceType.Folder && String.IsNullOrEmpty(s.Folder))
                .ToList();

            var accessibleFolders = new System.Collections.Generic.List<string>() { "" };
            foreach (var folder in folders)
            {
                if (await folder.HasAnyAccess(identity))
                {
                    mapServiceManager.ReloadServices(folder.Name, true);
                    accessibleFolders.Add(folder.Name);
                }
            }

            foreach (var service in mapServiceManager.MapServices
                                        .Where(s => s.Type != MapServiceType.Folder)
                                        .Where(s => accessibleFolders.Contains(s.Folder ?? ""))
                                        .OrderBy(s => s.Folder)
                                        .ThenBy(s => s.Name))
            {
                if (!(await service.GetSettingsAsync()).IsRunningOrIdle()
                    || !await service.HasAnyAccess(identity))
                {
                    continue;
                }

                string serviceUrl = OgcApiBaseEndpoint.ServiceUrl(httpContext, mapServiceManager.Options.OnlineResource, service.Folder ?? "", service.Name);
                serviceList.Services.Add(new ServiceListItem()
                {
                    Name = service.Name,
                    Folder = String.IsNullOrEmpty(service.Folder) ? null : service.Folder,
                    Links =
                    [
                        new Link($"{serviceUrl}?f=json", "item", OgcApiMediaTypes.Json, service.Name),
                        new Link($"{serviceUrl}?f=html", "item", OgcApiMediaTypes.Html, service.Name)
                    ]
                });
            }

            return format == OgcApiFormat.Html
                ? Results.Content(OgcApiHtmlPage.Render(httpContext, serviceList.Title, HtmlRenderer.ServiceList(serviceList)), "text/html", Encoding.UTF8)
                : Results.Text(JsonSerializer.Serialize(serviceList, OgcApiBaseEndpoint.JsonOptions),
                               OgcApiMediaTypes.Json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            return OgcApiBaseEndpoint.ErrorResult(httpContext, logger, ex);
        }
    };
}
