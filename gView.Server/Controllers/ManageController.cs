using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using gView.Interoperability.GeoServices.Rest.Json;
using gView.MapServer;
using gView.Server.AppCode;
using gView.Server.Models;
using Microsoft.AspNetCore.Mvc;
using static gView.Interoperability.GeoServices.Rest.Json.JsonServices;

namespace gView.Server.Controllers
{
    public class ManageController : BaseController
    {
        public IActionResult Index()
        {
            var authToken = base.GetAuthToken();
            if (authToken.IsAnonymous)
                return RedirectToAction("Login");

            return View();
        }

        #region Login

        [HttpGet]
        public IActionResult Login()
        {
            return View(new ManageLoginModel());
        }

        [HttpPost]
        public IActionResult Login(ManageLoginModel model)
        {
            try
            {
                var loginManager = new LoginManager(Globals.LoginManagerRootPath);
                var authToken = loginManager.GetManagerAuthToken(model.Username, model.Password, createIfFirst: true);

                if (authToken == null)
                    throw new Exception("Unknown user or password");

                base.SetAuthCookie(authToken);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return View(model);
            }
        }

        #endregion

        #region Services

        public IActionResult Folders()
        {
            return SecureApiCall(() =>
            {
                return Json(new
                {
                    success = true,
                    folders = InternetMapServer.mapServices.OrderBy(s => s.Folder).Select(s => s.Folder).Distinct()
                });
            });
        }

        public IActionResult Services(string folder)
        {
            return SecureApiCall(() =>
            {
                return Json(new
                {
                    success = true,
                    services = InternetMapServer.mapServices
                        .Where(s => s.Folder?.ToLower() == folder?.ToLower() || (String.IsNullOrEmpty(folder) && String.IsNullOrEmpty(s.Folder)))
                        .Select(async s => MapService2Json(s, await s.GetSettingsAsync()))
                        .Select(t => t.Result)
                        .ToArray()
                });
            });
        }

        async public Task<IActionResult> SetServiceStatus(string service, string status)
        {
            return await SecureApiCall(async () =>
            {
                var mapService = InternetMapServer.mapServices.Where(s => s.Fullname == service).FirstOrDefault();
                if (mapService == null)
                    throw new MapServerException("Unknown service: " + service);

                var settings = await mapService.GetSettingsAsync();
                switch (status.ToLower())
                {
                    case "running":
                        if (settings.Status != MapServiceStatus.Running)
                        {
                            // start
                            settings.Status = MapServiceStatus.Running;
                            await mapService.SaveSettingsAsync();
                            // reload
                            await InternetMapServer.Instance.GetServiceMap(service.ServiceName(), service.FolderName());
                        }
                        break;
                    case "stopped":
                        settings.Status = MapServiceStatus.Stopped;
                        InternetMapServer.MapDocument.RemoveMap(mapService.Fullname);
                        await mapService.SaveSettingsAsync();
                        break;
                    case "refresh":
                        settings.RefreshService = DateTime.UtcNow;

                        // start
                        settings.Status = MapServiceStatus.Running;
                        await mapService.SaveSettingsAsync();
                        // reload
                        await InternetMapServer.Instance.GetServiceMap(service.ServiceName(), service.FolderName());
                        break;
                }

                return Json(new
                {
                    success = true,
                    service = MapService2Json(mapService, settings)
                });
            });
        }

        public IActionResult ErrorLogs(string service, string last="0")
        {
            return SecureApiCall(() =>
            {
                var errorsResult = Logger.ErrorLogs(service, Framework.system.loggingMethod.error, long.Parse(last));

                return Json(new
                {
                    errors = errorsResult.errors,
                    ticks = errorsResult.ticks > 0 ? errorsResult.ticks.ToString() : null
                });
            });
        }

        #endregion

        #region Security

        public IActionResult TokenUsers()
        {
            return SecureApiCall(() =>
            {
                var loginManager = new LoginManager(Globals.LoginManagerRootPath);
                return Json(new { users = loginManager.GetTokenUsernames() });
            });
        }

        [HttpPost]
        public IActionResult CreateTokenUser(CreateTokenUserModel model)
        {
            return SecureApiCall(() =>
            {
                model.NewUsername = model.NewUsername?.Trim() ?? String.Empty;
                model.NewPassword = model.NewPassword?.Trim() ?? String.Empty;

                if (model.NewUsername.Length < 5)
                    throw new Exception("Username: min length 5 chars!");

                if (model.NewPassword.Length < 8)
                    throw new Exception("Password: min length 8 chars");

                var loginManager = new LoginManager(Globals.LoginManagerRootPath);
                loginManager.CreateTokenLogin(model.NewUsername, model.NewPassword);

                return Json(new { success = true });
            });
        }

        [HttpPost]
        public IActionResult ChangeTokenUserPassword(ChangeTokenUserPasswordModel model)
        {
            return SecureApiCall(() =>
            {
                model.Username = model.Username?.Trim() ?? String.Empty;
                model.NewPassword = model.NewPassword?.Trim() ?? String.Empty;

                if (model.NewPassword.Length < 8)
                    throw new Exception("Password: min length 8 chars");

                var loginManager = new LoginManager(Globals.LoginManagerRootPath);
                loginManager.ChangeTokenUserPassword(model.Username, model.NewPassword);

                return Json(new { success = true });
            });
        }

        #endregion

        #region Helper

        private IActionResult SecureApiCall(Func<IActionResult> func)
        {
            try
            {
                var authToken = base.GetAuthToken();
                if (authToken.IsAnonymous)
                    throw new Exception("Not allowed");

                return func.Invoke();
            }
            catch (NotAuthorizedException)
            {
                return Json(new { success = true, error = "not authorized" });
            }
            catch(MapServerException mse)
            {
                return Json(new { success = true, error = mse.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, error = "unknown error" });
            }
        }

        async private Task<IActionResult> SecureApiCall(Func<Task<IActionResult>> func)
        {
            try
            {
                var authToken = base.GetAuthToken();
                if (authToken.IsAnonymous)
                    throw new Exception("Not allowed");

                return await func.Invoke();
            }
            catch (NotAuthorizedException)
            {
                return Json(new { success = true, error = "not authorized" });
            }
            catch (MapServerException mse)
            {
                return Json(new { success = true, error = mse.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, error = "unknown error" });
            }
        }

        private object MapService2Json(IMapService mapService, IMapServiceSettings settings)
        {
            return new
            {
                name = mapService.Name,
                folder = mapService.Folder,
                status = (settings?.Status ?? MapServer.MapServiceStatus.Running).ToString().ToLower(),
                hasSecurity = settings?.AccessRules != null && settings.AccessRules.Length > 0,
                runningSince = settings?.Status == MapServiceStatus.Running && mapService.RunningSinceUtc.HasValue ?
                    mapService.RunningSinceUtc.Value.ToShortDateString() + " " + mapService.RunningSinceUtc.Value.ToLongTimeString() + " (UTC)" :
                    String.Empty,
                hasErrors = Logger.LogFileExists(mapService.Fullname, Framework.system.loggingMethod.error)
            };
        }

        #endregion
    }
}