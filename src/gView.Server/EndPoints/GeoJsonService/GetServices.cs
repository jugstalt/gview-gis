using gView.Framework.Core.Exceptions;
using gView.Framework.Core.MapServer;
using gView.GeoJsonService.DTOs;
using gView.Server.AppCode.Extensions;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Server.EndPoints.GeoJsonService;

public class GetServices : BaseApiEndpoint
{
    public GetServices()
        : base(
                $"{Routes.Base}/{Routes.GetServices}/{{folder?}}",
                Handler
            )
    {
    }

    static private Delegate Handler =>
        (
            HttpContext httpContext,
            [FromServices] LoginManager loginManagerService,
            [FromServices] MapServiceManager mapServerService,
            string folder = ""
        ) => HandleSecureAsync(httpContext, mapServerService, loginManagerService, "", "", async (_, identity) =>
        {
            mapServerService.ReloadServices(folder, true);

            if (!String.IsNullOrWhiteSpace(folder))
            {
                var folderService = mapServerService.MapServices
                    .Where(f => f.Type == MapServiceType.Folder && String.IsNullOrWhiteSpace(f.Folder) && folder.Equals(f.Name, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();

                if (folderService == null)
                {
                    throw new MapServerException("Unknown folder");
                }
                if (await folderService.HasAnyAccess(identity) == false)
                {
                    throw new MapServerAuthException("folder forbidden");
                }
            }

            List<string> folders = new List<string>();
            foreach (var f in mapServerService.MapServices.Where(s => s.Type == MapServiceType.Folder && s.Folder == folder))
            {
                if (await f.HasAnyAccess(identity))
                {
                    folders.Add(f.Name);
                }
            }

            List<string> services = new List<string>();
            foreach (var service in mapServerService.MapServices)
            {
                if (service.Type != MapServiceType.Folder &&
                    service.Folder == folder &&
                   (await service.GetSettingsAsync()).IsRunningOrIdle() &&
                    await service.HasAnyAccess(identity))
                {
                    services.Add(service.Name);
                }
            }

            return new GetServicesResponse()
            {
                Folders = folders,
                Services = services
            };
        });
}
