using gView.Core.Framework.Exceptions;
using gView.Framework.system;
using gView.MapServer;
using gView.Server.AppCode;
using gView.Server.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.Controllers
{
    public class BrowseServicesController : BaseController
    {
        async public Task<IActionResult> Index(string folder)
        {
            folder = folder ?? String.Empty;

            return await SecureMethodHandler(async (identity) =>
            {
                try
                {
                    await InternetMapServer.ReloadServices(folder, true);
                }
                catch  // Folder not exists
                {
                    if(!String.IsNullOrWhiteSpace(folder))
                    {
                        return RedirectToAction("Index");
                    }
                }

                if (!String.IsNullOrWhiteSpace(folder))
                {
                    var folderService = InternetMapServer.MapServices
                        .Where(f => f.Type == MapServiceType.Folder && String.IsNullOrWhiteSpace(f.Folder) && folder.Equals(f.Name, StringComparison.InvariantCultureIgnoreCase))
                        .FirstOrDefault();

                    if (folderService == null || !await folderService.HasAnyAccess(identity))
                    {
                        return RedirectToAction("Index");
                    }
                }

                List<string> folders = new List<string>();
                foreach (var f in InternetMapServer.MapServices.Where(s => s.Type == MapServiceType.Folder && s.Folder == folder))
                {
                    if (await f.HasAnyAccess(identity))
                    {
                        folders.Add(f.Name);
                    }
                }

                List<IMapService> services = new List<IMapService>();
                foreach (var s in InternetMapServer.MapServices)
                {
                    try
                    {
                        if (s.Type != MapServiceType.Folder &&
                            s.Folder == folder &&
                            (await s.GetSettingsAsync()).Status == MapServiceStatus.Running &&
                             await s.HasAnyAccess(identity) &&
                             await IsAccessAllowed(identity, s))
                        {
                            services.Add(s);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }



                var model = new BrowseServicesIndexModel()
                {
                    Folder = folder,
                    Folders = folders.ToArray(),
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
                {
                    throw new Exception("Unknown service: " + id);
                }

                if (!await mapService.HasAnyAccess(identity) && !await IsAccessAllowed(identity, mapService))
                {
                    throw new NotAuthorizedException();
                }

                List<IServiceRequestInterpreter> interpreters = new List<IServiceRequestInterpreter>();
                foreach (var interpreterType in InternetMapServer.Interpreters)
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
                    MapService = InternetMapServer.MapServices.Where(s => s.Name == id.ServiceName() && s.Folder == id.FolderName()).FirstOrDefault(),
                    Interpreters = interpreters
                });
            });

        }

        #region Helper

        async override protected Task<IActionResult> SecureMethodHandler(Func<Identity, Task<IActionResult>> func, Func<Exception, IActionResult> onException = null)
        {
            if (onException == null)
            {
                onException = (e) =>
                {
                    throw e;
                };
            }

            return await base.SecureMethodHandler(func, onException: onException);
        }

        async private Task<bool> IsAccessAllowed(IIdentity identity, IMapService service)
        {
            var accessType = await service.GetAccessTypes(identity);

            return (accessType.HasFlag(AccessTypes.Map) || accessType.HasFlag(AccessTypes.Query) || accessType.HasFlag(AccessTypes.Edit));
        }

        #endregion
    }
}