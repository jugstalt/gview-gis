using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gView.Framework.Data;
using gView.Interoperability.GeoServices.Rest.Json;
using gView.Server.AppCode;
using Microsoft.AspNetCore.Mvc;
using static gView.Interoperability.GeoServices.Rest.Json.JsonServices;
using System.Reflection;
using Newtonsoft.Json;
using gView.Framework.system;
using gView.MapServer;
using gView.Interoperability.GeoServices.Request;
using System.Collections.Specialized;
using gView.Interoperability.GeoServices.Rest.Json.Request;
using Microsoft.Extensions.Primitives;
using gView.Interoperability.GeoServices.Rest.Json.Response;
using gView.Framework.Carto;
using gView.Interoperability.GeoServices.Rest.Json.Legend;
using gView.Interoperability.GeoServices.Rest.Json.FeatureServer;
using Newtonsoft.Json.Serialization;
using gView.Interoperability.GeoServices.Rest.Reflection;
using gView.Interoperability.GeoServices.Rest.Json.Renderers.SimpleRenderers;
using gView.Core.Framework.Exceptions;

namespace gView.Server.Controllers
{
    public class GeoServicesRestController : BaseController
    {
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
            return await SecureMethodHandler((identity) =>
            {
                InternetMapServer.ReloadServices(id, true);

                return Task.FromResult(Result(new JsonServices()
                {
                    CurrentVersion = Version,
                    Folders = InternetMapServer.mapServices
                        .Where(s => s.Type == MapServiceType.Folder && s.Folder == id)
                        .Select(s => s.Name).Distinct()
                        .ToArray(),
                    Services = InternetMapServer.mapServices
                        .Where(s =>
                        {
                            return
                                s.Type != MapServiceType.Folder &&
                                s.Folder == id &&
                                (s.GetSettingsAsync().Result).Status == MapServiceStatus.Running &&
                                s.HasAnyAccess(identity).Result;
                        })
                        .SelectMany((s) => AgsServices(identity, s).Result)
                        .ToArray()
                }));
            });
        }

        async public Task<IActionResult> Service(string id, string folder="")
        {
            return await SecureMethodHandler(async (identity) =>
            {
                var mapService = InternetMapServer.Instance.GetMapService(id, folder);
                if(mapService==null)
                    throw new Exception("Unknown service: " + id);

                await mapService.CheckAccess(identity, InternetMapServer.GetInterpreter(typeof(GeoServicesRestInterperter)));

                var map = await InternetMapServer.Instance.GetServiceMapAsync(id, folder);
                if (map == null)
                    throw new Exception("unable to create map: " + id);

                gView.Framework.Geometry.Envelope fullExtent = null;

                return Result(new JsonMapService()
                {
                    CurrentVersion = 10.61,
                    Layers = map.MapElements.Select(e =>
                    {
                        var tocElement = map.TOC.GetTOCElement(e as ILayer);

                        if (e.Class is IFeatureClass && ((IFeatureClass)e.Class).Envelope != null)
                        {
                            if (fullExtent == null)
                                fullExtent = new Framework.Geometry.Envelope(((IFeatureClass)e.Class).Envelope);
                            else
                                fullExtent.Union(((IFeatureClass)e.Class).Envelope);
                        }

                        return new JsonMapService.Layer()
                        {
                            Id = e.ID,
                            Name = tocElement != null ? tocElement.Name : e.Title,
                            DefaultVisibility = tocElement != null ? tocElement.LayerVisible : true,
                            MaxScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MinimumScale > 1 ? tocElement.Layers[0].MinimumScale : 0, 0) : 0,
                            MinScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MaximumScale > 1 ? tocElement.Layers[0].MaximumScale : 0, 0) : 0,
                        };
                    }).ToArray(),
                    FullExtent = new JsonMapService.Extent()
                    {
                        XMin = fullExtent != null ? fullExtent.minx : 0D,
                        YMin = fullExtent != null ? fullExtent.miny : 0D,
                        XMax = fullExtent != null ? fullExtent.maxx : 0D,
                        YMax = fullExtent != null ? fullExtent.maxy : 0D,
                        SpatialReference = new JsonMapService.SpatialReference(0)
                    }
                });
            });
        }

        async public Task<IActionResult> ServiceLayers(string id, string folder = "")
        {
            return await SecureMethodHandler(async (identity) =>
            {
                var mapService = InternetMapServer.Instance.GetMapService(id, folder);
                if (mapService == null)
                    throw new Exception("Unknown service: " + id);

                await mapService.CheckAccess(identity, InternetMapServer.GetInterpreter(typeof(GeoServicesRestInterperter)));

                var map = await InternetMapServer.Instance.GetServiceMapAsync(id, folder);
                if (map == null)
                    throw new Exception("unable to create map: " + id);

                var jsonLayers = new JsonLayers();
                jsonLayers.Layers = map.MapElements
                    .Where(e => map.TOC.GetTOCElement(e as ILayer) != null)  // Just show layer in Toc
                    .Select(e => JsonLayer(map, e.ID))
                    .ToArray();

                return Result(jsonLayers);
            });
        }

        async public Task<IActionResult> ServiceLayer(string id, int layerId, string folder = "")
        {
            return await SecureMethodHandler(async (identity) =>
            {
                var mapService = InternetMapServer.Instance.GetMapService(id, folder);
                if (mapService == null)
                    throw new Exception("Unknown service: " + id);

                await mapService.CheckAccess(identity, InternetMapServer.GetInterpreter(typeof(GeoServicesRestInterperter)));

                var map = await InternetMapServer.Instance.GetServiceMapAsync(id, folder);
                if (map == null)
                    throw new Exception("unable to create map: " + id);

                var jsonLayers = new JsonLayers();
                return Result(JsonLayer(map, layerId));
            });
        }

        #region MapServer

        async public Task<IActionResult> ExportMap(string id, string folder = "")
        {
            return await SecureMethodHandler(async (identity) =>
            {
                var interpreter = InternetMapServer.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                JsonExportMap exportMap = Deserialize<JsonExportMap>(
                    Request.HasFormContentType ?
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, JsonConvert.SerializeObject(exportMap))
                {
                    OnlineResource = InternetMapServer.OnlineResource,
                    Method = "export",
                    Identity = identity
                };

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                    InternetMapServer.Instance,
                    interpreter,
                    serviceRequest);

                string format = ResultFormat();
                if (String.IsNullOrWhiteSpace(format))
                {
                    return FormResult(exportMap);
                }

                await InternetMapServer.TaskQueue.AwaitRequest(interpreter.Request, context);

                #endregion

                if (serviceRequest.Succeeded)
                {
                    return Result(JsonConvert.DeserializeObject<JsonExportResponse>(serviceRequest.Response));
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
                var interpreter = InternetMapServer.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                JsonQueryLayer queryLayer = Deserialize<JsonQueryLayer>(
                    Request.HasFormContentType ?
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);
                queryLayer.LayerId = layerId;

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, JsonConvert.SerializeObject(queryLayer))
                {
                    OnlineResource = InternetMapServer.OnlineResource,
                    Method = "query",
                    Identity = identity
                };

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                    InternetMapServer.Instance,
                    interpreter,
                    serviceRequest);

                string format = ResultFormat();
                if (String.IsNullOrWhiteSpace(format))
                {
                    return FormResult(queryLayer);
                }

                await InternetMapServer.TaskQueue.AwaitRequest(interpreter.Request, context);

                #endregion

                if (serviceRequest.Succeeded)
                {
                    if (queryLayer.ReturnCountOnly == true)
                    {
                        return Result(JsonConvert.DeserializeObject<JsonFeatureCountResponse>(serviceRequest.Response));
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
                var interpreter = InternetMapServer.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, String.Empty)
                {
                    OnlineResource = InternetMapServer.OnlineResource,
                    Method = "legend",
                    Identity = identity
                };

                #endregion



                #region Queue & Wait

                IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                    InternetMapServer.Instance,
                    interpreter,
                    serviceRequest);

                await InternetMapServer.TaskQueue.AwaitRequest(interpreter.Request, context);

                #endregion

                return Result(JsonConvert.DeserializeObject<LegendResponse>(serviceRequest.Response));
            });
        }

        #endregion

        #region FeatureServer

        async public Task<IActionResult> FeatureServerService(string id, string folder="")
        {
            return await SecureMethodHandler(async (identity) =>
            {
                var mapService = InternetMapServer.Instance.GetMapService(id, folder);
                if (mapService == null)
                    throw new Exception("Unknown service: " + id);

                await mapService.CheckAccess(identity, InternetMapServer.GetInterpreter(typeof(GeoServicesRestInterperter)));

                var map = await InternetMapServer.Instance.GetServiceMapAsync(id, folder);
                if (map == null)
                    throw new Exception("unable to create map: " + id);

                gView.Framework.Geometry.Envelope fullExtent = null;
                return Result(new JsonFeatureService()
                {
                    CurrentVersion = 10.61,
                    Layers = map.MapElements.Select(e =>
                    {
                        var tocElement = map.TOC.GetTOCElement(e as ILayer);

                        if (e.Class is IFeatureClass && ((IFeatureClass)e.Class).Envelope != null)
                        {
                            if (fullExtent == null)
                                fullExtent = new Framework.Geometry.Envelope(((IFeatureClass)e.Class).Envelope);
                            else
                                fullExtent.Union(((IFeatureClass)e.Class).Envelope);
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
                        SpatialReference = new JsonMapService.SpatialReference(0)
                    }
                });
            });
        }

        async public Task<IActionResult> FeatureServerQuery(string id, int layerId, string folder = "")
        {
            return await SecureMethodHandler(async (identity) =>
            {
                var interpreter = InternetMapServer.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                JsonQueryLayer queryLayer = Deserialize<JsonQueryLayer>(
                    Request.HasFormContentType ?
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);
                queryLayer.LayerId = layerId;

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, JsonConvert.SerializeObject(queryLayer))
                {
                    OnlineResource = InternetMapServer.OnlineResource,
                    Method = "featureserver_query",
                    Identity = identity
                };

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                    InternetMapServer.Instance,
                    interpreter,
                    serviceRequest);

                string format = ResultFormat();
                if (String.IsNullOrWhiteSpace(format))
                {
                    return FormResult(queryLayer);
                }

                await InternetMapServer.TaskQueue.AwaitRequest(interpreter.Request, context);

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
                var interpreter = InternetMapServer.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                JsonFeatureServerUpdateRequest editRequest = Deserialize<JsonFeatureServerUpdateRequest>(
                    Request.HasFormContentType ?
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);
                editRequest.LayerId = layerId;

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, JsonConvert.SerializeObject(editRequest))
                {
                    OnlineResource = InternetMapServer.OnlineResource,
                    Method = "featureserver_addfeatures",
                    Identity = identity
                };

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                    InternetMapServer.Instance,
                    interpreter,
                    serviceRequest);

                string format = ResultFormat();
                if (String.IsNullOrWhiteSpace(format))
                {
                    return FormResult(editRequest);
                }

                await InternetMapServer.TaskQueue.AwaitRequest(interpreter.Request, context);

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
                var interpreter = InternetMapServer.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                JsonFeatureServerUpdateRequest editRequest = Deserialize<JsonFeatureServerUpdateRequest>(
                    Request.HasFormContentType ?
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);
                editRequest.LayerId = layerId;

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, JsonConvert.SerializeObject(editRequest))
                {
                    OnlineResource = InternetMapServer.OnlineResource,
                    Method = "featureserver_updatefeatures",
                    Identity = identity
                };

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                    InternetMapServer.Instance,
                    interpreter,
                    serviceRequest);

                string format = ResultFormat();
                if (String.IsNullOrWhiteSpace(format))
                {
                    return FormResult(editRequest);
                }

                await InternetMapServer.TaskQueue.AwaitRequest(interpreter.Request, context);

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
                var interpreter = InternetMapServer.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                JsonFeatureServerDeleteRequest editRequest = Deserialize<JsonFeatureServerDeleteRequest>(
                    Request.HasFormContentType ?
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);
                editRequest.LayerId = layerId;

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, JsonConvert.SerializeObject(editRequest))
                {
                    OnlineResource = InternetMapServer.OnlineResource,
                    Method = "featureserver_deletefeatures",
                    Identity = identity
                };

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                    InternetMapServer.Instance,
                    interpreter,
                    serviceRequest);

                string format = ResultFormat();
                if (String.IsNullOrWhiteSpace(format))
                {
                    return FormResult(editRequest);
                }

                await InternetMapServer.TaskQueue.AwaitRequest(interpreter.Request, context);

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
                var mapService = InternetMapServer.Instance.GetMapService(id, folder);
                if (mapService == null)
                    throw new Exception("Unknown service: " + id);

                await mapService.CheckAccess(identity, InternetMapServer.GetInterpreter(typeof(GeoServicesRestInterperter)));

                var map = await InternetMapServer.Instance.GetServiceMapAsync(id, folder);
                if (map == null)
                    throw new Exception("unable to create map: " + id);

                var jsonLayers = new JsonLayers();
                return Result(JsonFeatureServerLayer(map, layerId));
            });
        }

        #endregion

        #region Security

        public Task<IActionResult> GenerateToken(string request, string username, string password, int expiration=60)
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
                        throw new Exception("username required");
                    if (String.IsNullOrWhiteSpace(password))
                        throw new Exception("password required");

                    var loginManager = new LoginManager(Globals.LoginManagerRootPath);
                    var authToken = loginManager.GetAuthToken(username, password, expireMinutes: expiration);
                    if (authToken == null)
                        throw new Exception("unknown username or password");

                    return Task.FromResult(Result(new JsonSecurityToken()
                    {
                        token = authToken.ToString(),
                        expires = Convert.ToInt64((new DateTime(authToken.Expire, DateTimeKind.Utc)-new DateTime(1970,1,1,0,0,0, DateTimeKind.Utc)).TotalMilliseconds)
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
                throw new Exception("Unknown layer: " + layerId);

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

                return new JsonLayer()
                {
                    CurrentVersion = Version,
                    Id = groupLayer.ID,
                    Name = groupLayer.Title,
                    DefaultVisibility = groupLayer.Visible,
                    MaxScale = Math.Max(groupLayer.MinimumScale > 1 ? groupLayer.MinimumScale : 0, 0),
                    MinScale = Math.Max(groupLayer.MaximumScale > 1 ? groupLayer.MaximumScale : 0, 0),
                    Type = type,
                    ParentLayer = parentLayer,
                    SubLayers = groupLayer.ChildLayer != null ?
                        groupLayer.ChildLayer.Where(l => map.TOC.GetTOCElement(l as ILayer) != null).Select(l =>
                            {
                                var childTocElement = map.TOC.GetTOCElement(l as ILayer);

                                return new JsonLayerLink()
                                {
                                    Id = l.ID,
                                    Name = childTocElement.Name
                                };
                            }).ToArray() :
                        new JsonLayerLink[0]
                };
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
                if (datasetElement.Class is IFeatureClass && ((IFeatureClass)datasetElement.Class).Envelope != null)
                {
                    extent = new JsonExtent()
                    {
                        // DoTo: SpatialReference
                        Xmin = ((IFeatureClass)datasetElement.Class).Envelope.minx,
                        Ymin = ((IFeatureClass)datasetElement.Class).Envelope.miny,
                        Xmax = ((IFeatureClass)datasetElement.Class).Envelope.maxx,
                        Ymax = ((IFeatureClass)datasetElement.Class).Envelope.maxy
                    };
                }

                string type = "Feature Layer";
                if (datasetElement.Class is IRasterClass)
                    type = "Raster Layer";

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
                result.GeometryType = datasetElement.Class is IFeatureClass ? Interoperability.GeoServices.Rest.Json.JsonLayer.ToGeometryType(((IFeatureClass)datasetElement.Class).GeometryType).ToString() : EsriGeometryType.esriGeometryNull.ToString();

                return result;
            }
        }

        private JsonLayer JsonFeatureServerLayer(IServiceMap map, int layerId)
        {
            return JsonLayer(map, layerId, new JsonFeatureServerLayer());
        }

        #endregion

        private IActionResult Result(object obj)
        {
            if (base.ActionStartTime.HasValue && obj is JsonStopWatch)
                ((JsonStopWatch)obj).DurationMilliseconds = (DateTime.UtcNow - base.ActionStartTime.Value).TotalMilliseconds;

            string format = ResultFormat();

            if (format == "json")
                return Json(obj);
            else if (format == "pjson")
                return Json(obj, new Newtonsoft.Json.JsonSerializerSettings()
                {
                    Formatting = Newtonsoft.Json.Formatting.Indented
                });

            #region ToHtml

            AddNavigation();
            ViewData["htmlbody"] = ToHtml(obj);
            return View("_htmlbody");

            #endregion
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
                requestPath = requestPath.Substring(1);

            string path = InternetMapServer.AppRootUrl(this.Request);
            string[] requestPathItems = requestPath.Split('/');
            string[] serverTypes = new string[] { "mapserver", "featureserver" };

            Dictionary<string, string> menuItems = new Dictionary<string, string>();
            bool usePath = false;
            for (int i = 0,to=requestPathItems.Length; i < to; i++)
            {
                var item = requestPathItems[i];
                path += "/" + item;

                if (i < to - 1 && serverTypes.Contains(requestPathItems[i+1].ToLower()))
                {
                    continue;
                }
                else if (i > 0 && serverTypes.Contains(item.ToLower()))
                {
                    item = requestPathItems[i - 1] + " (" + item + ")";
                }

                if (usePath)
                    menuItems.Add(item, path);

                if (item.ToLower() == "rest")
                    usePath = true;
            }

            ViewData["mainMenuItems"] = "_mainMenuGeoServicesPartial";
            ViewData["menuItems"] = menuItems;
        }

        private string ResultFormat()
        {
            if(!String.IsNullOrWhiteSpace(Request.Query["f"]))
            {
                return Request.Query["f"].ToString().ToLower();
            }
            if(Request.HasFormContentType && !String.IsNullOrWhiteSpace(Request.Form["f"].ToString().ToLower()))
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
                serviceTypes.Add("MapServer");
            if (accessTypes.HasFlag(AccessTypes.Edit) /* || accessTypes.HasFlag(AccessTypes.Query)*/)
                serviceTypes.Add("FeatureServer");

            return serviceTypes;
        }

        async private Task<IEnumerable<AgsService>> AgsServices(Identity identity, IMapService mapService)
        {
            return (await ServiceTypes(identity, mapService))
                        .Select(s => new AgsService()
                        {
                            Name = mapService.Name,
                            Type = s
                        });
        }

        #region Html

        private string ToHtml(object obj)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<div class='html-body container'>");

            var typeString = obj.GetType().ToString();
            if (typeString.Contains("."))
                typeString = typeString.Substring(typeString.LastIndexOf(".") + 1);
            if (typeString.StartsWith("Json"))
                typeString = typeString.Substring(4);

            sb.Append("<h3>" + typeString + " (YAML):</h3>");

            sb.Append("<div class='code-block'>");
            sb.Append(ToYamlHtml(obj));
            sb.Append("</div>");

            foreach(var serviceMethodAttribute in obj.GetType().GetCustomAttributes<ServiceMethodAttribute>(false))
            {
                sb.Append("<a href='" + InternetMapServer.OnlineResource + this.Request.Path + "/" + serviceMethodAttribute.Method + "'>" + serviceMethodAttribute.Name + "</a><br/>");
            }

            sb.Append("</div>");

            return sb.ToString();
        }

        private string ToYamlHtml(object obj, int spaces=0, bool isArray=false)
        {
            if (obj == null)
                return String.Empty;

            var type = obj.GetType();

            StringBuilder sb = new StringBuilder();
            sb.Append("<div class='yaml-code'>");

            bool isFirst = true;

            foreach(var propertyInfo in SortYamlProperties(type.GetProperties()))
            {
                if (propertyInfo.GetValue(obj) == null)
                    continue;

                var jsonPropertyAttribute = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
                if (jsonPropertyAttribute == null)
                    continue;

                var linkAttribute = propertyInfo.GetCustomAttribute<HtmlLinkAttribute>();

                bool newLine = !(propertyInfo.PropertyType.IsValueType || propertyInfo.PropertyType == typeof(string));
                if (newLine == true)
                {
                    if (propertyInfo.PropertyType.IsArray && (propertyInfo.GetValue(obj) == null || ((Array)propertyInfo.GetValue(obj)).Length == 0))
                        newLine = false;
                }

                string spacesValue = HtmlYamlSpaces(spaces);
                if(isArray && isFirst)
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
                                if(i==0)
                                {
                                    sb.Append("[");
                                } else
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

            for(int i=0;i<spaces;i++)
            {
                sb.Append("&nbsp;");
            }

            return sb.ToString();
        }

        private string HtmlYamlValue(HtmlLinkAttribute htmlLink, object val, object instance)
        {
            string valString = val?.ToString() ?? String.Empty;

            if(val is double || val is float)
            {
                valString = valString.Replace(",", ".");
            }

            if (htmlLink == null)
                return valString;

            

            string link = htmlLink.LinkTemplate.Replace("{url}", InternetMapServer.AppRootUrl(this.Request) + "/" + Request.Path).Replace("{0}", valString);

            if (instance != null && link.Contains("{") && link.Contains("}"))  // Replace instance properties
            {
                foreach (var propertyInfo in instance.GetType().GetProperties())
                {
                    var placeHolder = "{" + propertyInfo.Name + "}";
                    if (link.Contains(placeHolder)) {
                        object propertyValue = propertyInfo.GetValue(instance);
                        link = link.Replace(placeHolder, propertyValue != null ? propertyValue.ToString() : "");
                    }
                }
            }

            return "<a href='" + link + "'>" + valString + "</a>";
        }

        private IEnumerable<PropertyInfo> SortYamlProperties(IEnumerable<PropertyInfo> propertyInfos)
        {
            List<PropertyInfo> result = new List<PropertyInfo>();

            List<string> orderItems = new List<string>(new string[] { "name", "id" });

            foreach(var orderItem in orderItems)
            {
                var propertyInfo = propertyInfos.Where(p => p.Name.ToLower() == orderItem).FirstOrDefault();
                if (propertyInfo != null)
                    result.Add(propertyInfo);
            }

            foreach(var propertyInfo in propertyInfos)
            {
                if (orderItems.Contains(propertyInfo.Name.ToLower()))
                    continue;

                result.Add(propertyInfo);
            }

            return result;
        }

        private string ToHtmlForm(object obj)
        {
            if (obj == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder();
            
            sb.Append("<div class='html-body'>");

            sb.Append("<form>");

            sb.Append("<table>");

            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                var jsonPropertyAttribute = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
                if (jsonPropertyAttribute == null)
                    continue;

                var inputAttribute = propertyInfo.GetCustomAttribute<FormInputAttribute>();
                if (inputAttribute != null && inputAttribute.InputType == FormInputAttribute.InputTypes.Ignore)
                    continue;

                var inputType = inputAttribute?.InputType ?? FormInputAttribute.InputTypes.Text;

                if (propertyInfo.GetMethod.IsPublic && propertyInfo.SetMethod.IsPublic)
                {
                    sb.Append("<tr>");
                    sb.Append("<td>");
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
                            default:
                                sb.Append("<input name='" + jsonPropertyAttribute.PropertyName + "' value='" + (propertyInfo.GetValue(obj)?.ToString() ?? String.Empty) + "'>");
                                break;
                        }
                    }
                    sb.Append("</td>");
                    sb.Append("</tr>");
                }
            }

            sb.Append("<tr>");
            sb.Append("<td>");
            sb.Append("<span>Format:</span>");
            sb.Append("</td><td>");
            sb.Append("<select name='f'><option value='pjson'>JSON</option></select>");
            sb.Append("</td>");
            sb.Append("</tr>");

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

            foreach(var propertyInfo in typeof(T).GetProperties())
            {
                if (propertyInfo.SetMethod == null)
                    continue;

                var jsonPropertyAttribute = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
                if (jsonPropertyAttribute == null)
                    continue;

                string key = jsonPropertyAttribute.PropertyName ?? propertyInfo.Name;
                var keyValuePair = nv.Where(k => k.Key == key).FirstOrDefault();
                if (keyValuePair.Key == null)
                {
                    key = "&" + key;
                    keyValuePair = nv.Where(k => k.Key == key).FirstOrDefault();   // Sometimes the keyvalue-key starts with an & ??
                }

                if (keyValuePair.Key == key)
                {
                    var val = keyValuePair.Value.ToString();

                    if(propertyInfo.PropertyType==typeof(double))
                    {
                        if (!String.IsNullOrWhiteSpace(val))
                            propertyInfo.SetValue(instance, NumberConverter.ToDouble(keyValuePair.Value.ToString()));
                    }
                    else if(propertyInfo.PropertyType==typeof(float))
                    {
                        if (!String.IsNullOrWhiteSpace(val))
                            propertyInfo.SetValue(instance, NumberConverter.ToFloat(keyValuePair.Value.ToString()));
                    }
                    else if(propertyInfo.PropertyType==typeof(System.Int16))
                    {
                        if (!String.IsNullOrWhiteSpace(val))
                            propertyInfo.SetValue(instance, Convert.ToInt16(val));
                    }
                    else if (propertyInfo.PropertyType == typeof(System.Int32))
                    {
                        if (!String.IsNullOrWhiteSpace(val))
                            propertyInfo.SetValue(instance, Convert.ToInt32(val));
                    }
                    else if (propertyInfo.PropertyType == typeof(System.Int64))
                    {
                        if (!String.IsNullOrWhiteSpace(val))
                            propertyInfo.SetValue(instance, Convert.ToInt64(val));
                    }
                    else if(propertyInfo.PropertyType==typeof(System.String))
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

        async override protected Task<IActionResult> SecureMethodHandler(Func<Identity, Task<IActionResult>> func, Func<Exception, IActionResult> onException=null)
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