using gView.Framework.Common;
using gView.Framework.Core.Common;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.MapServer;
using gView.Server.AppCode;
using gView.Server.AppCode.Extensions;
using gView.Server.Models;
using gView.Server.Services.Hosting;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace gView.Server.Controllers;

public class BrowseServicesController : BaseController
{
    private readonly MapServiceManager _mapServerService;
    private readonly MapServiceDeploymentManager _mapServerDeployService;
    private readonly UrlHelperService _urlHelperService;
    private readonly LoginManager _loginManagerService;

    public BrowseServicesController(
        MapServiceManager mapServerService,
        MapServiceDeploymentManager mapServerDeployService,
        UrlHelperService urlHelperService,
        LoginManager loginManagerService,
        EncryptionCertificateService encryptionCertificateService)
        : base(mapServerService, loginManagerService, encryptionCertificateService)
    {
        _mapServerService = mapServerService;
        _mapServerDeployService = mapServerDeployService;
        _urlHelperService = urlHelperService;
        _loginManagerService = loginManagerService;
    }

    async public Task<IActionResult> Index(string folder, string serviceName = "", string errorMessage = "",
                                           bool openPublish = false, bool openCreate = false)
    {
        folder = folder ?? String.Empty;

        return await SecureMethodHandler(async (identity) =>
        {
            try
            {
                _mapServerService.ReloadServices(folder, true);
            }
            catch  // Folder not exists
            {
                if (!String.IsNullOrWhiteSpace(folder))
                {
                    return RedirectToAction("Index");
                }
            }

            var authToken = _loginManagerService.GetAuthToken(this.Request);

            bool isPublisher = authToken.AuthType == AuthToken.AuthTypes.Manage;
            bool isManager = authToken.AuthType == AuthToken.AuthTypes.Manage;

            if (!String.IsNullOrWhiteSpace(folder))
            {
                var folderService = _mapServerService.MapServices
                    .Where(f => f.Type == MapServiceType.Folder && String.IsNullOrWhiteSpace(f.Folder) && folder.Equals(f.Name, StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault();

                if (folderService == null || !await folderService.HasAnyAccess(identity))
                {
                    return RedirectToAction("Index");
                }

                isPublisher |= (await folderService.HasPublishAccess(identity));
            }

            List<string> folders = new List<string>();
            foreach (var f in _mapServerService.MapServices.Where(s => s.Type == MapServiceType.Folder && s.Folder == folder))
            {
                if (await f.HasAnyAccess(identity))
                {
                    folders.Add(f.Name);
                }
            }

            List<IMapService> services = new List<IMapService>();
            foreach (var s in _mapServerService.MapServices)
            {
                if (s.Type != MapServiceType.Folder &&
                    s.Folder == folder &&
                    (await s.GetSettingsAsync()).IsRunningOrIdle() &&
                     await s.HasAnyAccess(identity) &&
                     await IsAccessAllowed(identity, s))
                {
                    services.Add(s);
                }
            }

            ViewData["open-publish"] = openPublish;
            ViewData["open-create"] = openCreate;

            var model = new BrowseServicesIndexModel()
            {
                IsPublisher = isPublisher,
                IsManager = isManager,
                Folder = folder,
                Folders = folders.ToArray(),
                Services = services.ToArray(),

                ServiceName = serviceName,
                Message = errorMessage
            };

            return View("Index", model);
        });
    }

    async public Task<IActionResult> ServiceCapabilities(string id)
    {
        return await SecureMethodHandler(async (identity) =>
        {
            var mapService = _mapServerService.Instance.GetMapService(id.ServiceName(), id.FolderName());
            if (mapService == null)
            {
                throw new Exception("Unknown service: " + id);
            }

            if (!await mapService.HasAnyAccess(identity) && !await IsAccessAllowed(identity, mapService))
            {
                throw new NotAuthorizedException();
            }

            List<IServiceRequestInterpreter> interpreters = new List<IServiceRequestInterpreter>();
            foreach (var interpreterType in _mapServerService.Interpreters)
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
                Server = _urlHelperService.AppRootUrl(this.Request),
                OnlineResource = Request.Scheme + "://" + Request.Host + "/ogc?",
                MapService = _mapServerService.MapServices.Where(s => s.Name == id.ServiceName() && s.Folder == id.FolderName()).FirstOrDefault(),
                Interpreters = interpreters
            });
        });

    }

    [HttpPost]
    async public Task<IActionResult> DeleteService(string folder, string service)
    {
        folder = folder ?? String.Empty;

        return await SecureMethodHandler(async (identity) =>
        {
            try
            {
                if (!String.IsNullOrEmpty(folder))
                {
                    service = folder + "/" + service;
                }

                bool ret = await _mapServerDeployService.RemoveMap(service, identity);

                return Json(new AdminMapServerResponse());
            }
            catch (MapServerException mse)
            {
                return Json(new AdminMapServerResponse(false, mse.Message));
            }
            catch (Exception)
            {
                return Json(new AdminMapServerResponse(false, "Unknown error"));
            }
        });
    }

    [HttpPost]
    async public Task<IActionResult> AddService(string service, string folder)
    {
        folder = (folder ?? String.Empty).ToLower();

        return await SecureMethodHandler(async (identity) =>
        {
            try
            {
                if (!service.IsValidServiceName())
                {
                    throw new MapServerException("service name is invalid");
                }
                service = service.ToLower();

                if (!String.IsNullOrEmpty(folder))
                {
                    service = $"{folder}/{service}";
                }

                if (Request.Form.Files.Count == 0)
                {
                    throw new MapServerException("No file uploaded");
                }

                var file = Request.Form.Files[0];
                byte[] buffer = new byte[file.Length];
                await file.OpenReadStream().ReadAsync(buffer, 0, buffer.Length);

                string mapXml = String.Empty;

                foreach (var encoding in new Encoding[]{
                            Encoding.UTF8,
                            Encoding.Unicode,
                            Encoding.UTF32,
                            //Encoding.UTF7,
                            Encoding.Default
                        })
                {
                    try
                    {
                        string xml = encoding.GetString(buffer);

                        int index = xml.IndexOf("<");
                        if (index < 0)
                        {
                            continue;
                        }

                        // Cut leading bytes -> often strange charakters that are not XML conform
                        xml = xml.Substring(index);

                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(xml);
                        var mapDocumentNode = doc.SelectSingleNode("//MapDocument");

                        mapXml = mapDocumentNode.OuterXml;
                        break;
                    }
                    catch (Exception ex)
                    {
                        string xmlError = ex.Message;
                    }
                }

                if (String.IsNullOrWhiteSpace(mapXml))
                {
                    throw new MapServerException("Can't read xml");
                }

                bool ret = await _mapServerDeployService.AddMap(service, mapXml, identity);
                if (ret == false)
                {
                    throw new Exception("unable to add service");
                }

                if (UseJsonResponse())
                {
                    return Json(new AdminMapServerResponse());
                }

                return await Index(folder);
            }
            catch (MapServerException mse)
            {
                if (UseJsonResponse())
                {
                    return Json(new AdminMapServerResponse(false, mse.Message));
                }
                else
                {
                    return await Index(folder, String.Empty, mse.Message);
                }
            }
            catch (Exception ex)
            {
                if (UseJsonResponse())
                {
                    return Json(new AdminMapServerResponse(false, ex.Message));
                }
                else
                {
                    return await Index(folder, String.Empty, "Unknown error: " + ex.Message);
                }
            }
        });
    }

    [HttpPost]
    async public Task<IActionResult> CreateFolder(string newFolder, string folder)
    {
        folder = folder ?? String.Empty;

        return await SecureMethodHandler(async (identity) =>
        {
            try
            {
                if (!identity.IsAdministrator)
                {
                    throw new MapServerException("Not allowed");
                }
                if (!newFolder.IsValidFolderName())
                {
                    throw new MapServerException($"Foldername {newFolder} is invalid");
                }

                var di = new DirectoryInfo($"{_mapServerService.Options.ServicesPath}/{newFolder.ToLower()}");
                if (di.Exists)
                {
                    throw new MapServerException($"Folder {newFolder} already exists");
                }

                di.Create();
                return await Index(folder);
            }
            catch (MapServerException mse)
            {
                return await Index(folder, String.Empty, mse.Message);
            }
            catch (Exception)
            {
                return await Index(folder, String.Empty, "Unknown error");
            }
        });
    }

    #region Helper

    async override protected Task<IActionResult> SecureMethodHandler(Func<Identity, Task<IActionResult>> func, Func<Exception, IActionResult> onException = null)
    {
        if (onException == null)
        {
            onException = (e) =>
            {
                if (e is InvalidTokenException)
                {
                    base.RemoveAuthCookie();
                }
                else
                {
                    throw e;
                }

                return RedirectToAction("Index", "Home");
            };
        }

        return await base.SecureMethodHandler(func, onException: onException);
    }

    async private Task<bool> IsAccessAllowed(IIdentity identity, IMapService service)
    {
        var accessType = await service.GetAccessTypes(identity);

        return (accessType.HasFlag(AccessTypes.Map) || accessType.HasFlag(AccessTypes.Query) || accessType.HasFlag(AccessTypes.Edit));
    }

    private bool UseJsonResponse()
    {
        return this.Request.Query["f"] == "json";
    }

    #endregion
}