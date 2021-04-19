using gView.Core.Framework.Exceptions;
using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.system;
using gView.Interoperability.GeoServices.Request;
using gView.Interoperability.GeoServices.Rest.Json;
using gView.Interoperability.GeoServices.Rest.Json.Features;
using gView.Interoperability.GeoServices.Rest.Json.FeatureServer;
using gView.Interoperability.GeoServices.Rest.Json.Legend;
using gView.Interoperability.GeoServices.Rest.Json.Renderers.SimpleRenderers;
using gView.Interoperability.GeoServices.Rest.Json.Request;
using gView.Interoperability.GeoServices.Rest.Json.Response;
using gView.Interoperability.GeoServices.Rest.Reflection;
using gView.MapServer;
using gView.Server.AppCode;
using gView.Server.Extensions;
using gView.Server.Services.Hosting;
using gView.Server.Services.Logging;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static gView.Interoperability.GeoServices.Rest.Json.JsonServices;

namespace gView.Server.Controllers
{
    public class GeoServicesRestController : BaseController
    {
        private readonly MapServiceManager _mapServerService;
        private readonly UrlHelperService _urlHelperService;
        private readonly LoginManager _loginManagerService;
        private readonly EncryptionCertificateService _encryptionCertificate;
        private readonly PerformanceLoggerService _performanceLogger;

        public GeoServicesRestController(
            MapServiceManager mapServerService,
            UrlHelperService urlHelperService,
            LoginManager loginManagerService,
            EncryptionCertificateService encryptionCertificate,
            PerformanceLoggerService performanceLogger)
            : base(mapServerService, loginManagerService, encryptionCertificate)
        {
            _mapServerService = mapServerService;
            _urlHelperService = urlHelperService;
            _loginManagerService = loginManagerService;
            _encryptionCertificate = encryptionCertificate;
            _performanceLogger = performanceLogger;
        }

        public const double Version = 10.61;
        //private const string DefaultFolder = "default";

        public int JsonExportResponse { get; private set; }

        public IActionResult Index()
        {
            return RedirectToAction("Services");
        }

        public Task<IActionResult> Services()
        {
            return Folder(String.Empty);
        }

        async public Task<IActionResult> Folder(string id)
        {
            return await SecureMethodHandler(async (identity) =>
            {
                _mapServerService.ReloadServices(id, true);

                if (!String.IsNullOrWhiteSpace(id))
                {
                    var folderService = _mapServerService.MapServices
                        .Where(f => f.Type == MapServiceType.Folder && String.IsNullOrWhiteSpace(f.Folder) && id.Equals(f.Name, StringComparison.InvariantCultureIgnoreCase))
                        .FirstOrDefault();

                    if (folderService == null || !await folderService.HasAnyAccess(identity))
                    {
                        throw new Exception("Unknown folder or forbidden");
                    }
                }

                List<string> folders = new List<string>();
                foreach (var f in _mapServerService.MapServices.Where(s => s.Type == MapServiceType.Folder && s.Folder == id))
                {
                    if (await f.HasAnyAccess(identity))
                    {
                        folders.Add(f.Name);
                    }
                }

                List<AgsService> services = new List<AgsService>();
                foreach (var s in _mapServerService.MapServices)
                {
                    if (s.Type != MapServiceType.Folder &&
                       s.Folder == id &&
                       (await s.GetSettingsAsync()).Status == MapServiceStatus.Running &&
                        await s.HasAnyAccess(identity))
                    {
                        services.AddRange(await AgsServices(identity, s));
                    }
                }

                return Result(new JsonServices()
                {
                    CurrentVersion = Version,
                    Folders = folders.ToArray(),
                    Services = services.ToArray()
                });
            });
        }

        async public Task<IActionResult> Service(string id, string folder = "")
        {
            return await SecureMethodHandler(async (identity) =>
            {
                var mapService = _mapServerService.Instance.GetMapService(id, folder);
                if (mapService == null)
                {
                    throw new Exception("Unknown service: " + id);
                }

                await mapService.CheckAccess(identity, _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter)));

                using (var map = await _mapServerService.Instance.GetServiceMapAsync(id, folder))
                {
                    if (map == null)
                    {
                        throw new Exception("unable to create map: " + id);
                    }

                    gView.Framework.Geometry.IEnvelope fullExtent = map.FullExtent();
                    var spatialReference = map.Display.SpatialReference;
                    int epsgCode = spatialReference != null ? spatialReference.EpsgCode : 0;

                    return Result(new JsonMapService()
                    {
                        CurrentVersion = 10.61,
                        MapName = String.IsNullOrWhiteSpace(map.Title) ?
                            (map.Name.Contains("/") ? map.Name.Substring(map.Name.LastIndexOf("/") + 1) : map.Name) :
                            map.Title,
                        CopyrightText = map.GetLayerCopyrightText(Map.MapCopyrightTextId),
                        ServiceDescription = map.GetLayerDescription(Map.MapDescriptionId),
                        Layers = map.MapElements
                            .Where(e =>
                            {
                                var tocElement = map.TOC.GetTOCElement(e as ILayer);

                                return tocElement == null ? false : tocElement.IsHidden() == false;
                            })
                            .Select(e =>
                            {
                                var tocElement = map.TOC.GetTOCElement(e as ILayer);

                                int parentLayerId =
                                    (e is IFeatureLayer && ((IFeatureLayer)e).GroupLayer != null) ?
                                    ((IFeatureLayer)e).GroupLayer.ID :
                                    -1;

                                return new JsonMapService.Layer()
                                {
                                    Id = e.ID,
                                    ParentLayerId = parentLayerId,
                                    Name = tocElement != null ? tocElement.Name : e.Title,
                                    DefaultVisibility = tocElement != null ? tocElement.LayerVisible : true,
                                    MaxScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MinimumScale > 1 ? tocElement.Layers[0].MinimumScale : 0, 0) : 0,
                                    MinScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MaximumScale > 1 ? tocElement.Layers[0].MaximumScale : 0, 0) : 0,
                                };
                            })
                            .ToArray(),
                        SpatialReferenceInstance = epsgCode > 0 ? new JsonMapService.SpatialReference(epsgCode) : null,
                        FullExtent = new JsonMapService.Extent()
                        {
                            XMin = fullExtent != null ? fullExtent.minx : 0D,
                            YMin = fullExtent != null ? fullExtent.miny : 0D,
                            XMax = fullExtent != null ? fullExtent.maxx : 0D,
                            YMax = fullExtent != null ? fullExtent.maxy : 0D,
                            SpatialReference = new JsonMapService.SpatialReference(epsgCode)
                        }
                    });
                }
            });
        }

        async public Task<IActionResult> ServiceLayers(string id, string folder = "")
        {
            return await SecureMethodHandler(async (identity) =>
            {
                var mapService = _mapServerService.Instance.GetMapService(id, folder);
                if (mapService == null)
                {
                    throw new Exception("Unknown service: " + id);
                }

                await mapService.CheckAccess(identity, _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter)));

                using (var map = await _mapServerService.Instance.GetServiceMapAsync(id, folder))
                {
                    if (map == null)
                    {
                        throw new Exception("unable to create map: " + id);
                    }

                    var jsonLayers = new JsonLayers();
                    jsonLayers.Layers = map.MapElements
                        .Where(e => {   // Just show layer in Toc (and not hidden)
                            var tocElement = map.TOC.GetTOCElement(e as ILayer);

                            return tocElement == null ? false : tocElement.IsHidden() == false;
                            })
                        .Select(e => JsonLayer(map, e.ID))
                        .ToArray();

                    return Result(jsonLayers);
                }
            });
        }

        async public Task<IActionResult> ServiceLayer(string id, int layerId, string folder = "")
        {
            return await SecureMethodHandler(async (identity) =>
            {
                var mapService = _mapServerService.Instance.GetMapService(id, folder);
                if (mapService == null)
                {
                    throw new Exception("Unknown service: " + id);
                }

                await mapService.CheckAccess(identity, _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter)));

                var map = await _mapServerService.Instance.GetServiceMapAsync(id, folder);
                if (map == null)
                {
                    throw new Exception("unable to create map: " + id);
                }

                var jsonLayers = new JsonLayers();
                return Result(JsonLayer(map, layerId));
            });
        }

        #region MapServer

        async public Task<IActionResult> ExportMap(string id, string folder = "")
        {

            return await SecureMethodHandler(async (identity) =>
            {
                var interpreter = _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                JsonExportMap exportMap = Deserialize<JsonExportMap>(
                    Request.HasFormContentType ?
                    Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, JsonConvert.SerializeObject(exportMap))
                {
                    OnlineResource = _mapServerService.Options.OnlineResource,
                    OutputUrl = _mapServerService.Options.OutputUrl,
                    Method = "export",
                    Identity = identity
                };

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                    _mapServerService.Instance,
                    interpreter,
                    serviceRequest);

                string format = ResultFormat();
                if (String.IsNullOrWhiteSpace(format))
                {
                    using (var serviceMap = await context.CreateServiceMapInstance())
                    {
                        exportMap.InitForm(serviceMap);
                        return FormResult(exportMap);
                    }
                }

                await _mapServerService.TaskQueue.AwaitRequest(interpreter.Request, context);

                #endregion

                if (serviceRequest.Succeeded)
                {
                    if (Request.Query["f"] == "image" && serviceRequest.Response.StartsWith("base64:"))
                    {
                        return Base64Result(serviceRequest.Response);
                    }
                    else
                    {
                        return Result(JsonConvert.DeserializeObject<JsonExportResponse>(serviceRequest.Response), folder, id, "ExportMap");
                    }
                }
                else
                {
                    return Result(JsonConvert.DeserializeObject<JsonError>(serviceRequest.Response));
                }
            });
        }

        async public Task<IActionResult> Query(string id, int layerId, string folder = "")
        {
            return await SecureMethodHandler(async (identity) =>
            {
                var interpreter = _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                JsonQueryLayer queryLayer = Deserialize<JsonQueryLayer>(
                    Request.HasFormContentType ?
                    Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);
                queryLayer.LayerId = layerId;

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, JsonConvert.SerializeObject(queryLayer))
                {
                    OnlineResource = _mapServerService.Options.OnlineResource,
                    OutputUrl = _mapServerService.Options.OutputUrl,
                    Method = "query",
                    Identity = identity
                };

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                    _mapServerService.Instance,
                    interpreter,
                    serviceRequest);

                string format = ResultFormat();
                if (String.IsNullOrWhiteSpace(format))
                {
                    return FormResult(queryLayer);
                }

                await _mapServerService.TaskQueue.AwaitRequest(interpreter.Request, context);

                #endregion

                if (serviceRequest.Succeeded)
                {
                    if (queryLayer.ReturnCountOnly == true)
                    {
                        return Result(JsonConvert.DeserializeObject<JsonFeatureCountResponse>(serviceRequest.Response), folder, id, "Query");
                    }
                    else
                    {
                        var featureResponse = JsonConvert.DeserializeObject<JsonFeatureResponse>(serviceRequest.Response);
                        return Result(featureResponse);
                    }
                }
                else
                {
                    return Result(JsonConvert.DeserializeObject<JsonError>(serviceRequest.Response));
                }
            });
        }

        async public Task<IActionResult> Legend(string id, string folder = "")
        {
            return await SecureMethodHandler(async (identity) =>
            {
                var interpreter = _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, String.Empty)
                {
                    OnlineResource = _mapServerService.Options.OnlineResource,
                    OutputUrl = _mapServerService.Options.OutputUrl,
                    Method = "legend",
                    Identity = identity
                };

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                    _mapServerService.Instance,
                    interpreter,
                    serviceRequest);

                await _mapServerService.TaskQueue.AwaitRequest(interpreter.Request, context);

                #endregion

                return Result(JsonConvert.DeserializeObject<LegendResponse>(serviceRequest.Response), folder, id, "Legend");
            });
        }

        async public Task<IActionResult> Identify(string id, string folder="")
        {
            return await SecureMethodHandler(async (identity) =>
            {
                var interpreter = _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                JsonIdentify identify = Deserialize<JsonIdentify>(
                    Request.HasFormContentType ?
                    Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, JsonConvert.SerializeObject(identify))
                {
                    OnlineResource = _mapServerService.Options.OnlineResource,
                    OutputUrl = _mapServerService.Options.OutputUrl,
                    Method = "identify",
                    Identity = identity
                };

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                    _mapServerService.Instance,
                    interpreter,
                    serviceRequest);

                string format = ResultFormat();
                if (String.IsNullOrWhiteSpace(format))
                {
                    using (var serviceMap = await context.CreateServiceMapInstance())
                    {
                        identify.InitForm(serviceMap);
                        return FormResult(identify);
                    }
                }

                await _mapServerService.TaskQueue.AwaitRequest(interpreter.Request, context);

                #endregion

                if (serviceRequest.Succeeded)
                {
                    return Result(JsonConvert.DeserializeObject<JsonIdentifyResponse>(serviceRequest.Response), folder, id, "Identify");
                }
                else
                {
                    return Result(JsonConvert.DeserializeObject<JsonError>(serviceRequest.Response));
                }
                
            });
        }

        #endregion

        #region FeatureServer

        async public Task<IActionResult> FeatureServerService(string id, string folder = "")
        {
            return await SecureMethodHandler(async (identity) =>
            {
                var mapService = _mapServerService.Instance.GetMapService(id, folder);
                if (mapService == null)
                {
                    throw new Exception("Unknown service: " + id);
                }

                await mapService.CheckAccess(identity, _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter)));

                var map = await _mapServerService.Instance.GetServiceMapAsync(id, folder);
                if (map == null)
                {
                    throw new Exception("unable to create map: " + id);
                }

                gView.Framework.Geometry.Envelope fullExtent = null;
                int epsg = 0;

                return Result(new JsonFeatureService()
                {
                    CurrentVersion = 10.61,
                    Layers = map.MapElements
                    .Where(e => e.Class is IFeatureClass)
                    .Select(e =>
                    {
                        var tocElement = map.TOC.GetTOCElement(e as ILayer);
                        IFeatureClass fc = (IFeatureClass)e.Class;

                        if (fc.Envelope != null)
                        {
                            if (fullExtent == null)
                            {
                                fullExtent = new Framework.Geometry.Envelope(fc.Envelope);
                            }
                            else
                            {
                                fullExtent.Union(fc.Envelope);
                            }  
                        }

                        if (epsg == 0 && fc.SpatialReference != null && fc.SpatialReference.Name.ToLower().StartsWith("epsg:"))
                        {
                            int.TryParse(fc.SpatialReference.Name.Substring(5), out epsg);
                        }

                        return new JsonIdName()
                        {
                            Id = e.ID,
                            Name = tocElement != null ? tocElement.Name : e.Title
                        };
                    }).ToArray(),
                    FullExtent = new JsonMapService.Extent()
                    {
                        XMin = fullExtent != null ? fullExtent.minx : 0D,
                        YMin = fullExtent != null ? fullExtent.miny : 0D,
                        XMax = fullExtent != null ? fullExtent.maxx : 0D,
                        YMax = fullExtent != null ? fullExtent.maxy : 0D,
                        SpatialReference = new JsonMapService.SpatialReference(epsg)
                    }
                });
            });
        }

        async public Task<IActionResult> FeatureServerQuery(string id, int layerId, string folder = "")
        {
            return await SecureMethodHandler(async (identity) =>
            {
                var interpreter = _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                JsonQueryLayer queryLayer = Deserialize<JsonQueryLayer>(
                    Request.HasFormContentType ?
                    Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);
                queryLayer.LayerId = layerId;

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, JsonConvert.SerializeObject(queryLayer))
                {
                    OnlineResource = _mapServerService.Options.OnlineResource,
                    OutputUrl = _mapServerService.Options.OutputUrl,
                    Method = "featureserver_query",
                    Identity = identity
                };

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                    _mapServerService.Instance,
                    interpreter,
                    serviceRequest);

                string format = ResultFormat();
                if (String.IsNullOrWhiteSpace(format))
                {
                    return FormResult(queryLayer);
                }

                await _mapServerService.TaskQueue.AwaitRequest(interpreter.Request, context);

                #endregion

                if (serviceRequest.Succeeded)
                {
                    return Result(JsonConvert.DeserializeObject<JsonFeatureServiceQueryResponse>(serviceRequest.Response));
                }
                else
                {
                    return Result(JsonConvert.DeserializeObject<JsonError>(serviceRequest.Response));
                }
            });
        }

        async public Task<IActionResult> FeatureServerAddFeatures(string id, int layerId, string folder = "")
        {
            return await SecureMethodHandler(async (identity) =>
            {
                var interpreter = _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                JsonFeatureServerUpdateRequest editRequest = Deserialize<JsonFeatureServerUpdateRequest>(
                    Request.HasFormContentType ?
                    Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);
                editRequest.LayerId = layerId;

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, JsonConvert.SerializeObject(editRequest))
                {
                    OnlineResource = _mapServerService.Options.OnlineResource,
                    OutputUrl = _mapServerService.Options.OutputUrl,
                    Method = "featureserver_addfeatures",
                    Identity = identity
                };

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                    _mapServerService.Instance,
                    interpreter,
                    serviceRequest);

                string format = ResultFormat();
                if (String.IsNullOrWhiteSpace(format))
                {
                    return FormResult(editRequest);
                }

                await _mapServerService.TaskQueue.AwaitRequest(interpreter.Request, context);

                #endregion

                return Result(JsonConvert.DeserializeObject<JsonFeatureServerResponse>(serviceRequest.Response));
            },
            onException: (ex) =>
            {
                return Result(new JsonFeatureServerResponse()
                {
                    AddResults = new JsonFeatureServerResponse.JsonResponse[]
                    {
                        new JsonFeatureServerResponse.JsonResponse()
                        {
                            Success=false,
                            Error=new JsonFeatureServerResponse.JsonError()
                            {
                                Code=999,
                                Description=ex.Message
                            }
                        }
                    }
                });
            });
        }

        async public Task<IActionResult> FeatureServerUpdateFeatures(string id, int layerId, string folder = "")
        {
            return await SecureMethodHandler(async (identity) =>
            {
                var interpreter = _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                JsonFeatureServerUpdateRequest editRequest = Deserialize<JsonFeatureServerUpdateRequest>(
                    Request.HasFormContentType ?
                    Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);
                editRequest.LayerId = layerId;

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, JsonConvert.SerializeObject(editRequest))
                {
                    OnlineResource = _mapServerService.Options.OnlineResource,
                    OutputUrl = _mapServerService.Options.OutputUrl,
                    Method = "featureserver_updatefeatures",
                    Identity = identity
                };

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                    _mapServerService.Instance,
                    interpreter,
                    serviceRequest);

                string format = ResultFormat();
                if (String.IsNullOrWhiteSpace(format))
                {
                    return FormResult(editRequest);
                }

                await _mapServerService.TaskQueue.AwaitRequest(interpreter.Request, context);

                #endregion

                return Result(JsonConvert.DeserializeObject<JsonFeatureServerResponse>(serviceRequest.Response));
            },
            onException: (ex) =>
            {
                return Result(new JsonFeatureServerResponse()
                {
                    UpdateResults = new JsonFeatureServerResponse.JsonResponse[]
                    {
                        new JsonFeatureServerResponse.JsonResponse()
                        {
                            Success=false,
                            Error=new JsonFeatureServerResponse.JsonError()
                            {
                                Code=999,
                                Description=ex.Message
                            }
                        }
                    }
                });
            });
        }

        async public Task<IActionResult> FeatureServerDeleteFeatures(string id, int layerId, string folder = "")
        {
            return await SecureMethodHandler(async (identity) =>
            {
                var interpreter = _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                JsonFeatureServerDeleteRequest editRequest = Deserialize<JsonFeatureServerDeleteRequest>(
                    Request.HasFormContentType ?
                    Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);
                editRequest.LayerId = layerId;

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, JsonConvert.SerializeObject(editRequest))
                {
                    OnlineResource = _mapServerService.Options.OnlineResource,
                    OutputUrl = _mapServerService.Options.OutputUrl,
                    Method = "featureserver_deletefeatures",
                    Identity = identity
                };

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                    _mapServerService.Instance,
                    interpreter,
                    serviceRequest);

                string format = ResultFormat();
                if (String.IsNullOrWhiteSpace(format))
                {
                    return FormResult(editRequest);
                }

                await _mapServerService.TaskQueue.AwaitRequest(interpreter.Request, context);

                #endregion

                return Result(JsonConvert.DeserializeObject<JsonFeatureServerResponse>(serviceRequest.Response));
            },
            onException: (ex) =>
            {
                return Result(new JsonFeatureServerResponse()
                {
                    DeleteResults = new JsonFeatureServerResponse.JsonResponse[]
                                    {
                        new JsonFeatureServerResponse.JsonResponse()
                        {
                            Success=false,
                            Error=new JsonFeatureServerResponse.JsonError()
                            {
                                Code=999,
                                Description=ex.Message
                            }
                        }
                    }
                });
            });
        }

        async public Task<IActionResult> FeatureServerLayer(string id, int layerId, string folder = "")
        {
            return await SecureMethodHandler(async (identity) =>
            {
                var mapService = _mapServerService.Instance.GetMapService(id, folder);
                if (mapService == null)
                {
                    throw new Exception("Unknown service: " + id);
                }

                await mapService.CheckAccess(identity, _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter)));

                var map = await _mapServerService.Instance.GetServiceMapAsync(id, folder);
                if (map == null)
                {
                    throw new Exception("unable to create map: " + id);
                }

                var jsonLayers = new JsonLayers();
                return Result(JsonFeatureServerLayer(map, layerId));
            });
        }

        #endregion

        #region Security

        public Task<IActionResult> GenerateToken(string request, string username, string password, int expiration = 60)
        {
            try
            {
                // https://developers.arcgis.com/rest/users-groups-and-items/generate-token.htm

                string format = ResultFormat();
                if (String.IsNullOrWhiteSpace(format))
                {
                    return Task.FromResult(FormResult(new JsonGenerateToken()));
                }

                if (request?.ToLower() == "gettoken")
                {
                    if (String.IsNullOrWhiteSpace(username))
                    {
                        throw new Exception("username required");
                    }

                    if (String.IsNullOrWhiteSpace(password))
                    {
                        throw new Exception("password required");
                    }

                    var authToken = _loginManagerService.GetAuthToken(username, password, expireMinutes: expiration);
                    if (authToken == null)
                    {
                        throw new Exception("unknown username or password");
                    }

                    return Task.FromResult(Result(new JsonSecurityToken()
                    {
                        token = _encryptionCertificate.ToToken(authToken),
                        expires = Convert.ToInt64((new DateTime(authToken.Expire, DateTimeKind.Utc) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds)
                    }));
                }
                else
                {
                    throw new Exception("unkown request: " + request);
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(Result(new JsonError()
                {
                    Error = new JsonError.ErrorDef() { Code = 400, Message = ex.Message }
                }));
            }
        }

        #endregion

        #region Helper

        #region Json

        private JsonLayer JsonLayer(IServiceMap map, int layerId, JsonLayer result = null)
        {
            var datasetElement = map.MapElements.Where(e => e.ID == layerId).FirstOrDefault();
            if (datasetElement == null)
            {
                throw new Exception("Unknown layer: " + layerId);
            }

            var tocElement = map.TOC.GetTOCElement(datasetElement as ILayer);

            JsonLayerLink parentLayer = null;
            if (datasetElement is ILayer && ((ILayer)datasetElement).GroupLayer != null)
            {
                parentLayer = new JsonLayerLink()
                {
                    Id = ((ILayer)datasetElement).GroupLayer.ID,
                    Name = ((ILayer)datasetElement).GroupLayer.Title
                };
            }

            if (datasetElement is GroupLayer && datasetElement.Class == null)  // GroupLayer
            {
                var groupLayer = (GroupLayer)datasetElement;
                string type = "Group Layer";
                var childLayers = groupLayer.ChildLayer != null ?
                        groupLayer.ChildLayer.Where(l => map.TOC.GetTOCElement(l as ILayer) != null).Select(l =>
                        {
                            var childTocElement = map.TOC.GetTOCElement(l as ILayer);

                            return new JsonLayerLink()
                            {
                                Id = l.ID,
                                Name = childTocElement.Name
                            };
                        }).ToArray() :
                        new JsonLayerLink[0];

                if (groupLayer.MapServerStyle == MapServerGrouplayerStyle.Checkbox)
                {
                    type = "Feature Layer";
                    childLayers = null;
                }


                var jsonGroupLayer = new JsonLayer()
                {
                    CurrentVersion = Version,
                    Id = groupLayer.ID,
                    Name = groupLayer.Title,
                    DefaultVisibility = groupLayer.Visible,
                    MaxScale = Math.Max(groupLayer.MinimumScale > 1 ? groupLayer.MinimumScale : 0, 0),
                    MinScale = Math.Max(groupLayer.MaximumScale > 1 ? groupLayer.MaximumScale : 0, 0),
                    Type = type,
                    ParentLayer = parentLayer,
                    SubLayers = childLayers
                };

                if (jsonGroupLayer.SubLayers != null)
                {
                    JsonSpatialReference spatialReference = new JsonSpatialReference(map.Display.SpatialReference != null ? map.Display.SpatialReference.EpsgCode : 0);
                    JsonExtent extent = null;
                    foreach (var subLayer in jsonGroupLayer.SubLayers)
                    {
                        var featureClass = map.MapElements.Where(e => e.ID == subLayer.Id && e.Class is IFeatureClass).Select(l => (IFeatureClass)l.Class).FirstOrDefault();

                        if (featureClass != null)
                        {
                            int epsgCode = featureClass.SpatialReference != null ? featureClass.SpatialReference.EpsgCode : 0;
                            if (epsgCode == spatialReference.Wkid || epsgCode == 0)
                            {
                                var envelope = featureClass.Envelope;
                                if (envelope != null)
                                {
                                    if (extent == null)
                                    {
                                        extent = new JsonExtent()
                                        {
                                            Xmin = featureClass.Envelope.minx,
                                            Ymin = featureClass.Envelope.miny,
                                            Xmax = featureClass.Envelope.maxx,
                                            Ymax = featureClass.Envelope.maxy,
                                            SpatialReference = spatialReference
                                        };
                                    }
                                    else
                                    {
                                        extent.Xmin = Math.Min(extent.Xmin, featureClass.Envelope.minx);
                                        extent.Ymin = Math.Min(extent.Ymin, featureClass.Envelope.miny);
                                        extent.Xmax = Math.Min(extent.Xmax, featureClass.Envelope.maxx);
                                        extent.Ymax = Math.Min(extent.Ymax, featureClass.Envelope.maxy);
                                    }
                                }
                            }
                        }
                    }
                    jsonGroupLayer.Extent = extent;

                    jsonGroupLayer.Description = map.GetLayerDescription(layerId);
                    jsonGroupLayer.CopyrightText = map.GetLayerCopyrightText(layerId);
                }
                return jsonGroupLayer;
            }
            else // Featurelayer, Rasterlayer
            {
                JsonField[] fields = new JsonField[0];
                if (datasetElement.Class is ITableClass)
                {
                    fields = ((ITableClass)datasetElement.Class).Fields.ToEnumerable().Select(f =>
                    {
                        return new JsonField()
                        {
                            Name = f.name,
                            Alias = f.aliasname,
                            Type = JsonField.ToType(f.type).ToString()
                        };
                    }).ToArray();
                }

                JsonExtent extent = null;
                var spatialReference = (datasetElement.Class is IFeatureClass ? ((IFeatureClass)datasetElement.Class).SpatialReference : null) ?? (map.LayerDefaultSpatialReference ?? map.Display.SpatialReference);
                int epsgCode = spatialReference != null ? spatialReference.EpsgCode : 0;

                if (datasetElement.Class is IFeatureClass && ((IFeatureClass)datasetElement.Class).Envelope != null)
                {
                    extent = new JsonExtent()
                    {
                        // DoTo: SpatialReference
                        Xmin = ((IFeatureClass)datasetElement.Class).Envelope.minx,
                        Ymin = ((IFeatureClass)datasetElement.Class).Envelope.miny,
                        Xmax = ((IFeatureClass)datasetElement.Class).Envelope.maxx,
                        Ymax = ((IFeatureClass)datasetElement.Class).Envelope.maxy,
                        SpatialReference = new JsonSpatialReference(epsgCode)
                    };
                }

                string type = "Feature Layer";
                if (datasetElement.Class is IRasterClass)
                {
                    type = "Raster Layer";
                }

                JsonDrawingInfo drawingInfo = null;
                if (datasetElement is IFeatureLayer)
                {
                    var featureLayer = (IFeatureLayer)datasetElement;

                    drawingInfo = new JsonDrawingInfo()
                    {
                        Renderer = JsonRenderer.FromFeatureRenderer(featureLayer.FeatureRenderer)
                    };
                }

                result = result ?? new JsonLayer();

                var geometryType = datasetElement.Class is IFeatureClass ?
                    ((IFeatureClass)datasetElement.Class).GeometryType :
                    Framework.Geometry.geometryType.Unknown;

                if (geometryType == Framework.Geometry.geometryType.Unknown && datasetElement is IFeatureLayer)   // if layer is SQL Spatial with undefined geometrytype...
                {
                    geometryType = ((IFeatureLayer)datasetElement).LayerGeometryType;                             // take the settings from layer-properties
                }

                result.CurrentVersion = Version;
                result.Id = datasetElement.ID;
                result.Name = tocElement != null ? tocElement.Name : datasetElement.Title;
                result.DefaultVisibility = tocElement != null ? tocElement.LayerVisible : true;
                result.MaxScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MinimumScale > 1 ? tocElement.Layers[0].MinimumScale : 0, 0) : 0;
                result.MinScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MaximumScale > 1 ? tocElement.Layers[0].MaximumScale : 0, 0) : 0;
                result.Fields = fields;
                result.Extent = extent;
                result.Type = type;
                result.ParentLayer = parentLayer;
                result.DrawingInfo = drawingInfo;
                result.GeometryType = datasetElement.Class is IFeatureClass ?
                    Interoperability.GeoServices.Rest.Json.JsonLayer.ToGeometryType(geometryType).ToString() :
                    EsriGeometryType.esriGeometryNull.ToString();

                result.Description = map.GetLayerDescription(layerId);
                result.CopyrightText = map.GetLayerCopyrightText(layerId);

                if(result is JsonFeatureServerLayer)
                {
                    var editorModule = map.GetModule<gView.Plugins.Modules.EditorModule>();
                    if(editorModule!=null)
                    {
                        var editLayer = editorModule.GetEditLayer(result.Id);
                        if (editLayer != null)
                        {
                            List<string> editOperations = new List<string>();
                            foreach (Framework.Editor.Core.EditStatements statement in Enum.GetValues(typeof(Framework.Editor.Core.EditStatements)))
                            {
                                if (statement != Framework.Editor.Core.EditStatements.NONE && editLayer.Statements.HasFlag(statement))
                                {
                                    editOperations.Add(statement.ToString());
                                }
                            }

                            if(editOperations.Count>0)
                            {
                                ((JsonFeatureServerLayer)result).IsEditable = true;
                                ((JsonFeatureServerLayer)result).EditOperations = editOperations.ToArray();
                            } 
                        }
                    }
                }

                return result;
            }
        }

        private JsonLayer JsonFeatureServerLayer(IServiceMap map, int layerId)
        {
            return JsonLayer(map, layerId, new JsonFeatureServerLayer());
        }

        #endregion

        private IActionResult Result(object obj, string folder = null, string id = null, string method = null)
        {
            if (base.ActionStartTime.HasValue && obj is JsonStopWatch)
            {
                ((JsonStopWatch)obj).DurationMilliseconds = (DateTime.UtcNow - base.ActionStartTime.Value).TotalMilliseconds;

                ((JsonStopWatch)obj).AddPerformanceLoggerItem(_performanceLogger,
                                                              folder, id, method);
            }

            string format = ResultFormat();

            if (format == "json")
            {
                return Json(obj);
            }
            else if (format == "pjson")
            {
                return Json(obj, new Newtonsoft.Json.JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented
                });
            }

            #region ToHtml

            AddNavigation();
            ViewData["htmlbody"] = ToHtml(obj);
            return View("_htmlbody");

            #endregion
        }

        private IActionResult Base64Result(string response)
        {
            byte[] data = null;
            string contentType = "";

            if (response.StartsWith("base64:"))
            {
                response = response.Substring("base64:".Length);
                if (response.Contains(":"))
                {
                    int pos = response.IndexOf(":");
                    contentType = response.Substring(0, pos);
                    response = response.Substring(pos + 1);
                }
                data = Convert.FromBase64String(response);
            }
            else
            {
                data = Encoding.UTF8.GetBytes(response);
            }
            return Result(data, contentType);
        }

        private IActionResult Result(byte[] data, string contentType)
        {
            //ViewData["content-type"] = contentType;
            //ViewData["data"] = data;

            //return View("_binary");
            return File(data, contentType);
        }

        public IActionResult FormResult(object obj)
        {
            AddNavigation();
            ViewData["htmlBody"] = ToHtmlForm(obj);
            return View("_htmlbody");
        }

        private void AddNavigation()
        {
            var requestPath = this.Request.Path.ToString();
            while (requestPath.StartsWith("/"))
            {
                requestPath = requestPath.Substring(1);
            }

            string path = _urlHelperService.AppRootUrl(this.Request);
            string[] requestPathItems = requestPath.Split('/');
            string[] serverTypes = new string[] { "mapserver", "featureserver" };

            Dictionary<string, string> menuItems = new Dictionary<string, string>();
            bool usePath = false;
            for (int i = 0, to = requestPathItems.Length; i < to; i++)
            {
                var item = requestPathItems[i];
                path += "/" + item;

                if (i < to - 1 && serverTypes.Contains(requestPathItems[i + 1].ToLower()))
                {
                    continue;
                }
                else if (i > 0 && serverTypes.Contains(item.ToLower()))
                {
                    item = requestPathItems[i - 1] + " (" + item + ")";
                }

                if (usePath)
                {
                    menuItems.Add(item, path);
                }

                if (item.ToLower() == "rest")
                {
                    usePath = true;
                }
            }

            ViewData["mainMenuItems"] = "_mainMenuGeoServicesPartial";
            ViewData["menuItems"] = menuItems;
        }

        private string ResultFormat()
        {
            if (!String.IsNullOrWhiteSpace(Request.Query["f"]))
            {
                return Request.Query["f"].ToString().ToLower();
            }
            if (Request.HasFormContentType && !String.IsNullOrWhiteSpace(Request.Form["f"].ToString().ToLower()))
            {
                return Request.Form["f"];
            }

            return String.Empty;
        }

        async private Task<IEnumerable<string>> ServiceTypes(IIdentity identity, IMapService mapService)
        {
            var accessTypes = await mapService.GetAccessTypes(identity);

            List<string> serviceTypes = new List<string>(2);
            if (accessTypes.HasFlag(AccessTypes.Map))
            {
                serviceTypes.Add("MapServer");
            }

            if (accessTypes.HasFlag(AccessTypes.Edit) /* || accessTypes.HasFlag(AccessTypes.Query)*/)
            {
                serviceTypes.Add("FeatureServer");
            }

            return serviceTypes;
        }

        async private Task<IEnumerable<AgsService>> AgsServices(Identity identity, IMapService mapService)
        {
            return (await ServiceTypes(identity, mapService))
                        .Select(s => new AgsService()
                        {
                            Name = (String.IsNullOrWhiteSpace(mapService.Folder) ? "" : mapService.Folder + "/") + mapService.Name,
                            Type = s
                        });
        }

        #region Html

        private string ToHtml(object obj)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<div class='html-body'>");

            var typeString = obj.GetType().ToString();
            if (typeString.Contains("."))
            {
                typeString = typeString.Substring(typeString.LastIndexOf(".") + 1);
            }

            if (typeString.StartsWith("Json"))
            {
                typeString = typeString.Substring(4);
            }

            sb.Append("<h3>" + typeString + " (YAML):</h3>");

            sb.Append("<div class='code-block'>");
            sb.Append(ToYamlHtml(obj));
            sb.Append("</div>");

            foreach (var serviceMethodAttribute in obj.GetType().GetCustomAttributes<ServiceMethodAttribute>(false))
            {
                sb.Append("<a href='" + _mapServerService.Options.OnlineResource + this.Request.Path + "/" + serviceMethodAttribute.Method + "'>" + serviceMethodAttribute.Name + "</a><br/>");
            }

            sb.Append("</div>");

            return sb.ToString();
        }

        private string ToYamlHtml(object obj, int spaces = 0, bool isArray = false)
        {
            if (obj == null)
            {
                return String.Empty;
            }

            var type = obj.GetType();

            StringBuilder sb = new StringBuilder();
            sb.Append("<div class='yaml-code'>");

            bool isFirst = true;

            foreach (var propertyInfo in SortYamlProperties(type.GetProperties()))
            {
                if (propertyInfo.GetValue(obj) == null)
                {
                    continue;
                }

                var jsonPropertyAttribute = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
                if (jsonPropertyAttribute == null)
                {
                    continue;
                }

                var linkAttribute = propertyInfo.GetCustomAttribute<HtmlLinkAttribute>();

                bool newLine = !(propertyInfo.PropertyType.IsValueType || propertyInfo.PropertyType == typeof(string));
                if (newLine == true)
                {
                    if (propertyInfo.PropertyType.IsArray && (propertyInfo.GetValue(obj) == null || ((Array)propertyInfo.GetValue(obj)).Length == 0))
                    {
                        newLine = false;
                    }
                }

                string spacesValue = HtmlYamlSpaces(spaces);
                if (isArray && isFirst)
                {
                    spacesValue += "-&nbsp;";
                    spaces += 2;
                }
                sb.Append("<div class='property-name" + (newLine ? " array" : "") + "'>" + spacesValue + propertyInfo.Name + ":&nbsp;</div>");
                sb.Append("<div class='property-value'>");

                if (propertyInfo.PropertyType.IsArray)
                {
                    var array = (Array)propertyInfo.GetValue(obj);
                    if (array == null)
                    {
                        sb.Append("null");
                    }
                    else if (array.Length == 0)
                    {
                        sb.Append("[]");
                    }
                    else
                    {
                        for (int i = 0; i < array.Length; i++)
                        {

                            var val = array.GetValue(i);
                            if (val == null)
                            {
                                sb.Append("null");
                            }
                            else if (val.GetType().IsValueType || val.GetType() == typeof(string))
                            {
                                if (i == 0)
                                {
                                    sb.Append("[");
                                }
                                else
                                {
                                    sb.Append(", ");
                                }
                                sb.Append(HtmlYamlValue(linkAttribute, val, obj));
                                if (i == array.Length - 1)
                                {
                                    sb.Append("]");
                                }
                            }
                            else
                            {
                                sb.Append(ToYamlHtml(val, spaces + 2, true));
                            }
                        }
                    }
                }
                else if (propertyInfo.PropertyType.IsValueType || propertyInfo.PropertyType == typeof(string))
                {
                    sb.Append(HtmlYamlValue(linkAttribute, propertyInfo.GetValue(obj), obj));
                }
                else
                {
                    sb.Append(ToYamlHtml(propertyInfo.GetValue(obj), spaces + 2));
                }
                sb.Append("</div>");
                sb.Append("<br/>");

                isFirst = false;
            }
            sb.Append("</div>");

            return sb.ToString();
        }

        private string HtmlYamlSpaces(int spaces)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < spaces; i++)
            {
                sb.Append("&nbsp;");
            }

            return sb.ToString();
        }

        private string HtmlYamlValue(HtmlLinkAttribute htmlLink, object val, object instance)
        {
            string valString = val?.ToString() ?? String.Empty;

            if (val is double || val is float)
            {
                valString = valString.Replace(",", ".");
            }

            if (htmlLink == null)
            {
                return valString;
            }

            string link = htmlLink.LinkTemplate
                .Replace("{url}", _urlHelperService.AppRootUrl(this.Request).CombineUri(Request.Path))
                .Replace("{0}", valString);

            if (instance != null && link.Contains("{") && link.Contains("}"))  // Replace instance properties
            {
                foreach (var propertyInfo in instance.GetType().GetProperties())
                {
                    var placeHolder = "{" + propertyInfo.Name + "}";
                    if (link.Contains(placeHolder))
                    {
                        object propertyValue = propertyInfo.GetValue(instance);
                        link = link.Replace(placeHolder, propertyValue != null ? propertyValue.ToString() : "");
                    }
                }
            }

            return "<a href='" + link.ToValidUri() + "'>" + valString + "</a>";
        }

        private IEnumerable<PropertyInfo> SortYamlProperties(IEnumerable<PropertyInfo> propertyInfos)
        {
            List<PropertyInfo> result = new List<PropertyInfo>();

            List<string> orderItems = new List<string>(new string[] { "name", "id" });

            foreach (var orderItem in orderItems)
            {
                var propertyInfo = propertyInfos.Where(p => p.Name.ToLower() == orderItem).FirstOrDefault();
                if (propertyInfo != null)
                {
                    result.Add(propertyInfo);
                }
            }

            foreach (var propertyInfo in propertyInfos)
            {
                if (orderItems.Contains(propertyInfo.Name.ToLower()))
                {
                    continue;
                }

                result.Add(propertyInfo);
            }

            return result;
        }

        private string ToHtmlForm(object obj)
        {
            if (obj == null)
            {
                return String.Empty;
            }

            StringBuilder sb = new StringBuilder();

            sb.Append("<div class='html-body'>");

            sb.Append("<form>");

            sb.Append("<table>");

            bool hasFormatAttribute = false;

            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                var jsonPropertyAttribute = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
                if (jsonPropertyAttribute == null)
                {
                    continue;
                }

                var inputAttribute = propertyInfo.GetCustomAttribute<FormInputAttribute>();
                if (inputAttribute != null && inputAttribute.InputType == FormInputAttribute.InputTypes.Ignore)
                {
                    continue;
                }

                var inputType = inputAttribute?.InputType ?? FormInputAttribute.InputTypes.Text;


                if (propertyInfo.GetMethod.IsPublic && propertyInfo.SetMethod.IsPublic)
                {
                    sb.Append("<tr>");
                    sb.Append("<td>");

                    if (jsonPropertyAttribute.PropertyName == "f")
                    {
                        hasFormatAttribute = true;
                    }

                    if (inputType != FormInputAttribute.InputTypes.Hidden)
                    {
                        sb.Append("<span>" + propertyInfo.Name + ":</span>");
                    }
                    sb.Append("</td><td class='input'>");

                    if (propertyInfo.PropertyType.Equals(typeof(bool)))
                    {
                        sb.Append("<select name='" + jsonPropertyAttribute.PropertyName + "' style='min-width:auto;'><option value='false'>False</option><option value='true'>True</option></select>");
                    }
                    else
                    {
                        switch (inputType)
                        {
                            case FormInputAttribute.InputTypes.TextBox:
                                sb.Append("<textarea rows='3' name='" + jsonPropertyAttribute.PropertyName + "'>" + (propertyInfo.GetValue(obj)?.ToString() ?? String.Empty) + "</textarea>");
                                break;
                            case FormInputAttribute.InputTypes.TextBox10:
                                sb.Append("<textarea rows='10' name='" + jsonPropertyAttribute.PropertyName + "'>" + (propertyInfo.GetValue(obj)?.ToString() ?? String.Empty) + "</textarea>");
                                break;
                            case FormInputAttribute.InputTypes.Hidden:
                                sb.Append("<input type='hidden' name='" + jsonPropertyAttribute.PropertyName + "' value='" + (propertyInfo.GetValue(obj)?.ToString() ?? String.Empty) + "'>");
                                break;
                            case FormInputAttribute.InputTypes.Password:
                                sb.Append("<input name='" + jsonPropertyAttribute.PropertyName + "' type='password' value='" + (propertyInfo.GetValue(obj)?.ToString() ?? String.Empty) + "'>");
                                break;
                            default:
                                if (inputAttribute?.Values != null && inputAttribute.Values.Count() > 0)
                                {
                                    sb.Append("<select name='" + jsonPropertyAttribute.PropertyName + "' style='min-width:auto;'>");
                                    foreach (var val in inputAttribute.Values)
                                    {
                                        sb.Append("<option value='" + val + "'>" + val + "</option>");
                                    }
                                    sb.Append("</select>");
                                }
                                else
                                {
                                    sb.Append("<input name='" + jsonPropertyAttribute.PropertyName + "' value='" + (propertyInfo.GetValue(obj)?.ToString() ?? String.Empty) + "'>");
                                }
                                break;
                        }
                    }
                    sb.Append("</td>");
                    sb.Append("</tr>");
                }
            }

            if (!hasFormatAttribute)
            {
                sb.Append("<tr>");
                sb.Append("<td>");
                sb.Append("<span>Format:</span>");
                sb.Append("</td><td>");
                sb.Append("<select name='f'><option value='pjson'>JSON</option></select>");
                sb.Append("</td>");
                sb.Append("</tr>");
            }

            sb.Append("<tr>");
            sb.Append("<td>");
            sb.Append("</td><td>");
            sb.Append("<button>Submit</button>");
            sb.Append("</td>");
            sb.Append("</tr>");

            sb.Append("</table>");



            sb.Append("</form>");

            sb.Append("</div>");

            return sb.ToString();
        }

        #endregion

        private T Deserialize<T>(IEnumerable<KeyValuePair<string, StringValues>> nv)
        {
            var instance = (T)Activator.CreateInstance(typeof(T));

            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                if (propertyInfo.SetMethod == null)
                {
                    continue;
                }

                var jsonPropertyAttribute = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
                if (jsonPropertyAttribute == null)
                {
                    continue;
                }

                string key = jsonPropertyAttribute.PropertyName ?? propertyInfo.Name;
                var keyValuePair = nv.Where(k => key.Equals(k.Key, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (keyValuePair.Key == null)
                {
                    key = "&" + key;
                    keyValuePair = nv.Where(k => k.Key == key).FirstOrDefault();   // Sometimes the keyvalue-key starts with an & ??
                }

                if (key.Equals(keyValuePair.Key, StringComparison.InvariantCultureIgnoreCase))
                {
                    var val = keyValuePair.Value.ToString();

                    if (propertyInfo.PropertyType == typeof(double))
                    {
                        if (!String.IsNullOrWhiteSpace(val))
                        {
                            propertyInfo.SetValue(instance, NumberConverter.ToDouble(keyValuePair.Value.ToString()));
                        }
                    }
                    else if (propertyInfo.PropertyType == typeof(float))
                    {
                        if (!String.IsNullOrWhiteSpace(val))
                        {
                            propertyInfo.SetValue(instance, NumberConverter.ToFloat(keyValuePair.Value.ToString()));
                        }
                    }
                    else if (propertyInfo.PropertyType == typeof(System.Int16))
                    {
                        if (!String.IsNullOrWhiteSpace(val))
                        {
                            propertyInfo.SetValue(instance, Convert.ToInt16(val));
                        }
                    }
                    else if (propertyInfo.PropertyType == typeof(System.Int32))
                    {
                        if (!String.IsNullOrWhiteSpace(val))
                        {
                            propertyInfo.SetValue(instance, Convert.ToInt32(val));
                        }
                    }
                    else if (propertyInfo.PropertyType == typeof(System.Int64))
                    {
                        if (!String.IsNullOrWhiteSpace(val))
                        {
                            propertyInfo.SetValue(instance, Convert.ToInt64(val));
                        }
                    }
                    else if (propertyInfo.PropertyType == typeof(System.String))
                    {
                        propertyInfo.SetValue(instance, val);
                    }
                    else
                    {
                        if ((val.Trim().StartsWith("{") && val.Trim().EndsWith("}")) ||
                            (val.Trim().StartsWith("[") && val.Trim().EndsWith("]")))
                        {
                            propertyInfo.SetValue(instance, JsonConvert.DeserializeObject(val, propertyInfo.PropertyType));
                        }
                        else
                        {
                            propertyInfo.SetValue(instance, Convert.ChangeType(keyValuePair.Value.ToString(), propertyInfo.PropertyType));
                        }
                    }
                }
            }

            return instance;
        }

        async override protected Task<IActionResult> SecureMethodHandler(Func<Identity, Task<IActionResult>> func, Func<Exception, IActionResult> onException = null)
        {
            if (onException == null)
            {
                onException = (e) =>
                {
                    try
                    {
                        throw e;
                    }
                    catch (NotAuthorizedException nae)
                    {
                        return Result(new JsonError()
                        {
                            Error = new JsonError.ErrorDef() { Code = 403, Message = nae.Message }
                        });
                    }
                    catch (TokenRequiredException tre)
                    {
                        return Result(new JsonError()
                        {
                            Error = new JsonError.ErrorDef() { Code = 499, Message = tre.Message }
                        });
                    }
                    catch (InvalidTokenException ite)
                    {
                        return Result(new JsonError()
                        {
                            Error = new JsonError.ErrorDef() { Code = 498, Message = ite.Message }
                        });
                    }
                    catch (MapServerException mse)
                    {
                        return Result(new JsonError()
                        {
                            Error = new JsonError.ErrorDef() { Code = 999, Message = mse.Message }
                        });
                    }
                    catch (Exception)
                    {
                        return Result(new JsonError()
                        {
                            Error = new JsonError.ErrorDef() { Code = 999, Message = "unknown error" }
                        });

                    }
                };
            }

            return await base.SecureMethodHandler(func, onException: onException);
        }

        #endregion
    }
}