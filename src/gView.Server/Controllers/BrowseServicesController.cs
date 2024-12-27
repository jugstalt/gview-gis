using gView.Framework.Common;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.MapServer;
using gView.GeoJsonService;
using gView.GeoJsonService.DTOs;
using gView.Server.AppCode;
using gView.Server.AppCode.Extensions;
using gView.Server.EndPoints.GeoJsonService;
using gView.Server.Models;
using gView.Server.Services.Hosting;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public BrowseServicesController(
        MapServiceManager mapServerService,
        MapServiceDeploymentManager mapServerDeployService,
        UrlHelperService urlHelperService,
        LoginManager loginManagerService,
        EncryptionCertificateService encryptionCertificateService,
        IConfiguration configuration,
        HttpClient httpClient)
        : base(mapServerService, loginManagerService, encryptionCertificateService)
    {
        _mapServerService = mapServerService;
        _mapServerDeployService = mapServerDeployService;
        _urlHelperService = urlHelperService;
        _loginManagerService = loginManagerService;
        _configuration = configuration;
        _httpClient = httpClient;
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

    #region GeoJson Service

    public Task<IActionResult> ServiceCapabilities(string id) => SecureMethodHandler(async (identity) =>
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

        var url = AppendPathToBaseUrl(
                new RouteBuilder()
                .UseCapabilitesRoute(mapService)
                .Build()
            );
        var httpResponse = (await _httpClient.GetAsync(url)).EnsureSuccessStatusCode();
        var jsonResponse = await httpResponse.Content.ReadAsStringAsync();

        var response = GeoJsonSerializer.DeserializeResponse(jsonResponse);

        ViewData["htmlbody"] = response.GeoJsonObjectToHtml(mapService);
        return View("_htmlbody");
    });

    [HttpGet]
    public Task<IActionResult> GeoJsonRequest(string id, string request) => SecureMethodHandler(async (identity) =>
    {
        if (String.IsNullOrEmpty(request))
        {
            throw new Exception("Invalid request");
        }

        var mapService = _mapServerService.Instance.GetMapService(id.ServiceName(), id.FolderName());
        if (mapService == null)
        {
            throw new Exception("Unknown service: " + id);
        }

        if (!await mapService.HasAnyAccess(identity) && !await IsAccessAllowed(identity, mapService))
        {
            throw new NotAuthorizedException();
        }

        using var serviceMap = await _mapServerService.Instance.GetServiceMapAsync(mapService);
        var bounds = serviceMap.FullExtent();

        var requestParts = request.Split("/");
        (string command, int? layerId) = requestParts switch
        {
        [_, _] => (requestParts[0].ToLowerInvariant(), int.Parse(requestParts[1])),
            _ => (requestParts[0].ToLowerInvariant(), (int?)null)
        };

        var capabilitiesUrl = AppendPathToBaseUrl(
                new RouteBuilder()
                .UseCapabilitesRoute(mapService)
                .Build()
            );
        var httpCapabilitiesResponse = (await _httpClient.GetAsync(capabilitiesUrl)).EnsureSuccessStatusCode();
        var jsonCapabilitiesResponse = await httpCapabilitiesResponse.Content.ReadAsStringAsync();
        var capabilitiesResponse = GeoJsonSerializer.DeserializeResponse(jsonCapabilitiesResponse) as GetServiceCapabilitiesResponse;

        var supportedRequests = capabilitiesResponse.SupportedRequests.Where(r => r.Name == command).FirstOrDefault();
        if (supportedRequests is null)
        {
            throw new Exception($"Command {command} is not supported in this service");
        }

        object obj = command switch
        {
            "map" => new GetMapRequest()
            {
                CRS = CoordinateReferenceSystem.CreateByName(serviceMap.Display.SpatialReference.Name),
                BBox = BBox.FromArray([
                    bounds.MinX,
                    bounds.MinY,
                    bounds.MaxX,
                    bounds.MaxY ]),
                Width = 800,
                Height = 600,
                Dpi = 96,
                Format = "png",
                Transparent = true
            },
            "legend" => new GetLegendRequest(),
            "query" => new GetFeaturesRequest()
            {
                Command = QueryCommand.Select,
                OutFields = ["*"],
                OutCRS = CoordinateReferenceSystem.CreateByName(serviceMap.Display.SpatialReference.Name),
                Limit = 10
            },
            "feature" => new EditFeaturesRequest(),
            _ => throw new Exception($"Invalid request: {request}")
        };

        ViewData["htmlBody"] = obj.GeoJsonObjectToInputForm(
                                    id,
                                    request,
                                    layerId.HasValue
                                        ? serviceMap.MapElements
                                                    .Where(l => l.ID == layerId)
                                                    .Select(l => serviceMap.TOC.GetTOCElement(l as ILayer))
                                                    .FirstOrDefault()
                                                    .Name
                                        : "",
                                    supportedRequests.HttpMethods
                                    );
        return View("_htmlbody");
    });

    [HttpPost]
    public Task<IActionResult> GeoJsonRequest() => SecureMethodHandler(async (identity) =>
    {
        string id = Request.Form["_id"];
        string request = Request.Form["_request"];
        string httpMethod = Request.Form["_method"].ToString().ToUpperInvariant();

        var mapService = _mapServerService.Instance.GetMapService(id.ServiceName(), id.FolderName());
        if (mapService == null)
        {
            throw new Exception("Unknown service: " + id);
        }

        if (!await mapService.HasAnyAccess(identity) && !await IsAccessAllowed(identity, mapService))
        {
            throw new NotAuthorizedException();
        }

        return httpMethod switch
        {
            "GET" => await SendGeoJsonGetRequst(mapService, request),
            _ => await SendGeoJsonRequest(mapService, request)
        };
    });

    async private Task<IActionResult> SendGeoJsonGetRequst(IMapService mapService, string request)
    {
        StringBuilder sb = new();
        foreach (var formKey in Request.Form.Keys.Where(k => !k.StartsWith("_")))
        {
            var value = Request.Form[formKey];
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            if (sb.Length > 0)
            {
                sb.Append("&");
            }

            sb.Append($"{formKey}={value}");
        }

        var requestUrl = $"{new RouteBuilder().UseServiceRootRoute(mapService).Build()}/{request}";

        var httpRequestResponse = (await _httpClient.GetAsync($"{AppendPathToBaseUrl(requestUrl)}?{sb}")).EnsureSuccessStatusCode();

        var contentType = httpRequestResponse.Content.Headers.ContentType.MediaType;

        if (contentType == "application/json")
        {
            var jsonRequestResponse = await httpRequestResponse.Content.ReadAsStringAsync();
            var requestResponse = GeoJsonSerializer.DeserializeResponse(jsonRequestResponse);

            ViewData["htmlbody"] = requestResponse.GeoJsonObjectToHtml(
                    mapService,
                    requestUrl: $"{_configuration["onlineresource-url"]}/{requestUrl}?{sb}"
               );
            return View("_htmlbody");

        }

        ViewData["content-type"] = contentType;
        ViewData["data"] = await httpRequestResponse.Content.ReadAsByteArrayAsync();
        return View("_binary");
    }

    async private Task<IActionResult> SendGeoJsonRequest(IMapService mapService, string request)
    {
        var form = Request.Form;

        BaseRequest requestBodyObject = request.Split("/").First().ToLowerInvariant() switch
        {
            "map" => new GetMapRequest()
            {
                Layers = form.AsArrayOrDefault<string>("Layers"),
                CRS = form.AsObjectOrDefault("CRS", (value) => CoordinateReferenceSystem.CreateByName(value)),
                BBox = form.AsObject("BBox", (value) => BBox.FromArray(value.Split(',').Select(n => n.ToDouble()).ToArray())),
                Width = form.Parse<int>("Width"),
                Height = form.Parse<int>("Height"),
                Dpi = form.ParseOrDefault<int?>("Dpi"),
                Rotation = form.ParseOrDefault<double?>("Rotation"),
                Format = form["Format"],
                Transparent = form.IsTrue("Transparent"),
                ResponseFormat = form.AsEnumValue<MapReponseFormat>("ResponseFormat")
            },
            "legend" => new GetLegendRequest()
            {
                Width = form.Parse<int>("Width"),
                Height = form.Parse<int>("Height"),
                Dpi = form.ParseOrDefault<int?>("Dpi"),
            },
            "query" => new GetFeaturesRequest()
            {
                Command = form.AsEnumValue<QueryCommand>("Command"),
                OutFields = form.AsArray<string>("OutFields"),
                OutCRS = form.AsObjectOrDefault("OutCRS", (value) => CoordinateReferenceSystem.CreateByName(value)),

                ReturnGeometry = form.AsEnumValue<GeometryResult>("ReturnGeometry"),

                OrderByFields = form.AsArrayOrDefault<string>("OrderByFields"),
                ObjectIds = form.AsArrayOrDefault<string>("ObjectIds"),

                Limit = form.ParseOrDefault<int?>("Limit"),
                Offset = form.ParseOrDefault<int?>("Offset")
            },
            "feature" => new EditFeaturesRequest(),
            _ => throw new Exception($"Invalid request: {request}")
        };

        var requestBody = GeoJsonSerializer.Serialize(requestBodyObject);

        var requestUrl = $"{new RouteBuilder().UseServiceRootRoute(mapService).Build()}/{request}";

        var httpRequestResponse = (await _httpClient.PostAsync($"{AppendPathToBaseUrl(requestUrl)}",
            new StringContent(requestBody))).EnsureSuccessStatusCode();

        var contentType = httpRequestResponse.Content.Headers.ContentType.MediaType;

        if (contentType == "application/json")
        {
            var jsonRequestResponse = await httpRequestResponse.Content.ReadAsStringAsync();
            var requestResponse = GeoJsonSerializer.DeserializeResponse(jsonRequestResponse);

            ViewData["htmlbody"] = requestResponse.GeoJsonObjectToHtml(
            mapService,
                    requestUrl: $"{_configuration["onlineresource-url"]}/{requestUrl}",
                    requestBody: requestBody
               );
            return View("_htmlbody");

        }

        ViewData["content-type"] = contentType;
        ViewData["data"] = await httpRequestResponse.Content.ReadAsByteArrayAsync();
        return View("_binary");
    }

    #endregion

    public Task<IActionResult> ServiceInterfaces(string id) => SecureMethodHandler(async (identity) =>
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
    [ValidateAntiForgeryToken]
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
    [ValidateAntiForgeryToken]
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

    private string AppendPathToBaseUrl(string path)
        => (_configuration["onlineresource-url-internal"] ??
           _configuration["onlineresource-url"]) + "/" + path;

    async override protected Task<IActionResult> SecureMethodHandler(Func<Identity, Task<IActionResult>> func, Func<Exception, IActionResult> onException = null)
    {
        if (onException == null)
        {
            onException = (e) =>
            {
                if (e is InvalidTokenException)
                {
                    base.RemoveAuthCookie();
                    return RedirectToAction("Index", "Home");
                }

                //throw e;
                ViewData["errorMessage"] = e.Message;
                return View("_errorMessage");
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