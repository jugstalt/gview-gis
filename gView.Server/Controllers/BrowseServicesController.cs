using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using gView.Core.Framework.Exceptions;
using gView.Framework.system;
using gView.MapServer;
using gView.Server.AppCode;
using gView.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace gView.Server.Controllers
{
    public class BrowseServicesController : BaseController
    {
        async public Task<IActionResult> Index(string folder)
        {
            folder = folder ?? String.Empty;

            return await SecureMethodHandler(async (identity) =>
            {
                await InternetMapServer.ReloadServices(folder, true);

                List<IMapService> services = new List<IMapService>();
                foreach(var s in InternetMapServer.mapServices)
                {
                    if (s.Type != MapServiceType.Folder &&
                        s.Folder == folder &&
                        (await s.GetSettingsAsync()).Status == MapServiceStatus.Running &&
                         await s.HasAnyAccess(identity))
                    {
                        services.Add(s);
                    }
                }

                var model = new BrowseServicesIndexModel()
                {
                    Folder = folder,
                    Folders = InternetMapServer.mapServices
                        .Where(s => s.Type == MapServiceType.Folder && s.Folder == folder)
                        .Select(s => s.Name).Distinct()
                        .OrderBy(s=>s)
                        .ToArray(),
                    Services = services.ToArray()
                };

                return View(model);
            });
        }

        async public Task<IActionResult> ServiceCapabilities(string id)
        {
            return await SecureMethodHandler(async (identity) =>
            {
                var mapService = InternetMapServer.Instance.GetMapService(id.ServiceName(), id.FolderName());
                if (mapService == null)
                    throw new Exception("Unknown service: " + id);

                if (!await mapService.HasAnyAccess(identity))
                    throw new NotAuthorizedException();

                List<IServiceRequestInterpreter> interpreters = new List<IServiceRequestInterpreter>();
                foreach(var interpreterType in InternetMapServer.Interpreters)
                {
                    try
                    {
                        var interpreter = new PlugInManager().CreateInstance<IServiceRequestInterpreter>(interpreterType);
                        await mapService.CheckAccess(identity, interpreter);
                        interpreters.Add(interpreter);
                    }
                    catch { }
                }

                return View(new BrowseServicesServiceModel()
                {
                    Server = InternetMapServer.AppRootUrl(this.Request),
                    OnlineResource = Request.Scheme + "://" + Request.Host + "/ogc?",
                    MapService = InternetMapServer.mapServices.Where(s => s.Name == id.ServiceName() && s.Folder == id.FolderName()).FirstOrDefault(),
                    Interpreters = interpreters
                });
            });
            
        }

        #region Helper

        async override protected Task<IActionResult> SecureMethodHandler(Func<Identity, Task<IActionResult>> func, Func<Exception, IActionResult> onException = null)
        {
            if(onException==null)
            {
                onException = (e) =>
                {
                    throw e;
                };
            }

            return await base.SecureMethodHandler(func, onException: onException);
        }

        #endregion
    }
}