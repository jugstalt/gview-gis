using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            return await SecureMethodHandler((identity) =>
            {
                InternetMapServer.ReloadServices(folder, true);

                var model = new BrowseServicesIndexModel()
                {
                    Folder = folder,
                    Folders = InternetMapServer.mapServices
                        .Where(s => s.Type == MapServiceType.Folder && s.Folder == folder)
                        .Select(s => s.Name).Distinct()
                        .ToArray(),
                    Services = InternetMapServer.mapServices
                        .Where(s =>
                        {
                            return
                                s.Type != MapServiceType.Folder &&
                                s.Folder == folder &&
                                (s.GetSettingsAsync().Result).Status == MapServiceStatus.Running &&
                                s.HasAnyAccess(identity).Result;
                        })
                        .ToArray()
                };

                return Task.FromResult<IActionResult>(View(model));
            });
        }

        public IActionResult ServiceCapabilities(string id)
        {
            return View(new BrowseServicesServiceModel()
            {
                Server = InternetMapServer.AppRootUrl(this.Request),
                OnlineResource = Request.Scheme + "://" + Request.Host + "/ogc?",
                MapService = InternetMapServer.mapServices.Where(s => s.Name == id.ServiceName() && s.Folder == id.FolderName()).FirstOrDefault(),
                Interpreters = InternetMapServer.Interpreters.Select(i => new PlugInManager().CreateInstance<IServiceRequestInterpreter>(i))
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