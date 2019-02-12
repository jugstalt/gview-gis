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

namespace gView.Server.Controllers
{
    public class GeoServicesRestController : Controller
    {
        private const double Version = 10.61;
        //private const string DefaultFolder = "default";

        public int JsonExportResponse { get; private set; }

        public IActionResult Index()
        {
            return RedirectToAction("Services");
        }

        public IActionResult Services()
        {
            return Result(new JsonServices()
            {
                CurrentVersion = Version,
                Folders = InternetMapServer.mapServices
                    .Where(s => !String.IsNullOrWhiteSpace(s.Folder))
                    .Select(s => s.Folder).Distinct()
                    .ToArray(),
                Services = InternetMapServer.mapServices
                    .Where(s => String.IsNullOrWhiteSpace(s.Folder))
                    .Select(s => new AgsServices()
                    {
                        Name = s.Name,
                        Type = "MapServer"
                    })
                    .ToArray()
            });
        }

        public IActionResult Folder(string id)
        {
            try
            {
                //if (id != DefaultFolder)
                //    throw new Exception("Unknown folder: " + id);

                return Result(new JsonServices()
                {
                    CurrentVersion = 10.61,
                    Services = InternetMapServer.mapServices
                        .Where(s=>s.Folder==id)
                        .Select(s => new AgsServices()
                        {
                            Name = s.Name,
                            Type = "MapServer"
                        })
                    .ToArray(),
                    Folders = new string[0]
                });
            }
            catch (Exception ex)
            {
                return Json(new JsonError()
                {
                    error = new JsonError.Error() { code = 999, message = ex.Message }
                });
            }
        }

        public IActionResult Service(string folder, string id)
        {
            try
            {
                //if (folder != DefaultFolder)
                //    throw new Exception("Unknown folder: " + folder);

                var map = InternetMapServer.Instance.GetService(id, folder);
                if (map == null)
                    throw new Exception("Unknown service: " + id);

                gView.Framework.Geometry.Envelope fullExtent = null;
                return Result(new JsonService()
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

                        return new JsonService.Layer()
                        {
                            Id = e.ID,
                            Name = tocElement != null ? tocElement.Name : e.Title,
                            DefaultVisibility = tocElement != null ? tocElement.LayerVisible : true,
                            MaxScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MinimumScale > 1 ? tocElement.Layers[0].MinimumScale : 0, 0) : 0,
                            MinScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MaximumScale > 1 ? tocElement.Layers[0].MaximumScale : 0, 0) : 0,
                        };
                    }).ToArray(),
                    FullExtent = new JsonService.Extent()
                    {
                        XMin = fullExtent != null ? fullExtent.minx : 0D,
                        YMin = fullExtent != null ? fullExtent.miny : 0D,
                        XMax = fullExtent != null ? fullExtent.maxx : 0D,
                        YMax = fullExtent != null ? fullExtent.maxy : 0D,
                        SpatialReference = new JsonService.SpatialReference(0)
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new JsonError()
                {
                    error = new JsonError.Error() { code = 999, message = ex.Message }
                });
            }
        }

        public IActionResult ServiceLayers(string folder, string id)
        {
            try
            {
                //if (folder != DefaultFolder)
                //    throw new Exception("Unknown folder: " + folder);

                var map = InternetMapServer.Instance.GetService(id, folder);
                if (map == null)
                    throw new Exception("Unknown service: " + id);

                var jsonLayers = new JsonLayers();
                jsonLayers.Layers = map.MapElements
                    .Where(e=>map.TOC.GetTOCElement(e as ILayer)!=null)  // Just show layer in Toc
                    .Select(e => Layer(map, e.ID))
                    .ToArray();

                return Result(jsonLayers);
            }
            catch (Exception ex)
            {
                return Json(new JsonError()
                {
                    error = new JsonError.Error() { code = 999, message = ex.Message }
                });
            }
        }

        public IActionResult ServiceLayer(string folder, string id, int layerId)
        {
            try
            {
                //if (folder != DefaultFolder)
                //    throw new Exception("Unknown folder: " + folder);

                var map = InternetMapServer.Instance.GetService(id, folder);
                if (map == null)
                    throw new Exception("Unknown service: " + id);

                var jsonLayers = new JsonLayers();
                return Result(Layer(map, layerId));
            }
            catch (Exception ex)
            {
                return Json(new JsonError()
                {
                    error = new JsonError.Error() { code = 999, message = ex.Message }
                });
            }
        }

        #region MapServer

        async public Task<IActionResult> ExportMap(string folder, string id)
        {
            try
            {
                //if (folder != DefaultFolder)
                //    throw new Exception("Unknown folder: " + folder);

                var interpreter = InternetMapServer.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                JsonExportMap exportMap = Deserialize<JsonExportMap>(
                    Request.HasFormContentType ?
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);

                string format = ResultFormat();
                if (String.IsNullOrWhiteSpace(format))
                {
                    return FormResult(exportMap);
                }

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, JsonConvert.SerializeObject(exportMap))
                {
                    OnlineResource = InternetMapServer.OnlineResource,
                    Method = "export"
                };

                #endregion

                #region Security

                Identity identity = Identity.FromFormattedString(String.Empty);
                identity.HashedPassword = String.Empty;
                serviceRequest.Identity = identity;

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = new ServiceRequestContext(
                    InternetMapServer.Instance,
                    interpreter,
                    serviceRequest);

                //InternetMapServer.ThreadQueue.AddQueuedThreadSync(interpreter.Request, context);

                //await interpreter.Request(context);
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
            }
            catch (Exception ex)
            {
                return Json(new JsonError()
                {
                    error = new JsonError.Error() { code = 999, message = ex.Message }
                });
            }
        }

        async public Task<IActionResult> Query(string folder, string id, int layerId)
        {
            try
            {
                //if (folder != DefaultFolder)
                //    throw new Exception("Unknown folder: " + folder);

                var interpreter = InternetMapServer.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                JsonQueryLayer queryLayer = Deserialize<JsonQueryLayer>(
                    Request.HasFormContentType ?
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);
                queryLayer.LayerId = layerId;

                string format = ResultFormat();
                if (String.IsNullOrWhiteSpace(format))
                {
                    return FormResult(queryLayer);
                }

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, JsonConvert.SerializeObject(queryLayer))
                {
                    OnlineResource = InternetMapServer.OnlineResource,
                    Method = "query"
                };

                #endregion

                #region Security

                Identity identity = Identity.FromFormattedString(String.Empty);
                identity.HashedPassword = String.Empty;
                serviceRequest.Identity = identity;

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = new ServiceRequestContext(
                    InternetMapServer.Instance,
                    interpreter,
                    serviceRequest);

                //InternetMapServer.ThreadQueue.AddQueuedThreadSync(interpreter.Request, context);

                //await interpreter.Request(context);
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
            }
            catch (Exception ex)
            {
                return Json(new JsonError()
                {
                    error = new JsonError.Error() { code = 999, message = ex.Message }
                });
            }
        }

        async public Task<IActionResult> Legend(string folder, string id)
        {
            try
            {
                //if (folder != DefaultFolder)
                //    throw new Exception("Unknown folder: " + folder);

                var interpreter = InternetMapServer.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, String.Empty)
                {
                    OnlineResource = InternetMapServer.OnlineResource,
                    Method = "legend"
                };

                #endregion

                #region Security

                Identity identity = Identity.FromFormattedString(String.Empty);
                identity.HashedPassword = String.Empty;
                serviceRequest.Identity = identity;

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = new ServiceRequestContext(
                    InternetMapServer.Instance,
                    interpreter,
                    serviceRequest);

                await InternetMapServer.TaskQueue.AwaitRequest(interpreter.Request, context);

                #endregion

                return Result(JsonConvert.DeserializeObject<LegendResponse>(serviceRequest.Response));
            }
            catch (Exception ex)
            {
                return Json(new JsonError()
                {
                    error = new JsonError.Error() { code = 999, message = ex.Message }
                });
            }
        }

        #endregion

        #region FeatureServer

        async public Task<IActionResult> FeatureServerQuery(string folder, string id, int layerId)
        {
            try
            {
                //if (folder != DefaultFolder)
                //    throw new Exception("Unknown folder: " + folder);

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
                    Method = "featureserver_query"
                };

                #endregion

                #region Security

                Identity identity = Identity.FromFormattedString(String.Empty);
                identity.HashedPassword = String.Empty;
                serviceRequest.Identity = identity;

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = new ServiceRequestContext(
                    InternetMapServer.Instance,
                    interpreter,
                    serviceRequest);

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
            }
            catch (Exception ex)
            {
                return Result(new JsonError()
                {
                    error = new JsonError.Error() { code = 999, message = ex.Message }
                });
            }
        }

        async public Task<IActionResult> FeatureServerAddFeatures(string folder, string id, int layerId)
        {
            try
            {
                //if (folder != DefaultFolder)
                //    throw new Exception("Unknown folder: " + folder);

                var interpreter = InternetMapServer.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                JsonFeatureServerEditRequest editRequest = Deserialize<JsonFeatureServerEditRequest>(
                    Request.HasFormContentType ?
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);
                editRequest.LayerId = layerId;

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, JsonConvert.SerializeObject(editRequest))
                {
                    OnlineResource = InternetMapServer.OnlineResource,
                    Method = "featureserver_addfeatures"
                };

                #endregion

                #region Security

                Identity identity = Identity.FromFormattedString(String.Empty);
                identity.HashedPassword = String.Empty;
                serviceRequest.Identity = identity;

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = new ServiceRequestContext(
                    InternetMapServer.Instance,
                    interpreter,
                    serviceRequest);

                await InternetMapServer.TaskQueue.AwaitRequest(interpreter.Request, context);

                #endregion

                return Result(JsonConvert.DeserializeObject<JsonFeatureServerResponse>(serviceRequest.Response));
            }
            catch (Exception ex)
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
            }
        }

        async public Task<IActionResult> FeatureServerUpdateFeatures(string folder, string id, int layerId)
        {
            try
            {
                //if (folder != DefaultFolder)
                //    throw new Exception("Unknown folder: " + folder);

                var interpreter = InternetMapServer.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                JsonFeatureServerEditRequest editRequest = Deserialize<JsonFeatureServerEditRequest>(
                    Request.HasFormContentType ?
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);
                editRequest.LayerId = layerId;

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, JsonConvert.SerializeObject(editRequest))
                {
                    OnlineResource = InternetMapServer.OnlineResource,
                    Method = "featureserver_updatefeatures"
                };

                #endregion

                #region Security

                Identity identity = Identity.FromFormattedString(String.Empty);
                identity.HashedPassword = String.Empty;
                serviceRequest.Identity = identity;

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = new ServiceRequestContext(
                    InternetMapServer.Instance,
                    interpreter,
                    serviceRequest);

                await InternetMapServer.TaskQueue.AwaitRequest(interpreter.Request, context);

                #endregion

                return Result(JsonConvert.DeserializeObject<JsonFeatureServerResponse>(serviceRequest.Response));
            }
            catch (Exception ex)
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
            }
        }

        async public Task<IActionResult> FeatureServerDeleteFeatures(string folder, string id, int layerId)
        {
            try
            {
                //if (folder != DefaultFolder)
                //    throw new Exception("Unknown folder: " + folder);

                var interpreter = InternetMapServer.GetInterpreter(typeof(GeoServicesRestInterperter));

                #region Request

                JsonFeatureServerEditRequest editRequest = Deserialize<JsonFeatureServerEditRequest>(
                    Request.HasFormContentType ?
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);
                editRequest.LayerId = layerId;

                ServiceRequest serviceRequest = new ServiceRequest(id, folder, JsonConvert.SerializeObject(editRequest))
                {
                    OnlineResource = InternetMapServer.OnlineResource,
                    Method = "featureserver_deletefeatures"
                };

                #endregion

                #region Security

                Identity identity = Identity.FromFormattedString(String.Empty);
                identity.HashedPassword = String.Empty;
                serviceRequest.Identity = identity;

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = new ServiceRequestContext(
                    InternetMapServer.Instance,
                    interpreter,
                    serviceRequest);

                await InternetMapServer.TaskQueue.AwaitRequest(interpreter.Request, context);

                #endregion

                return Result(JsonConvert.DeserializeObject<JsonFeatureServerResponse>(serviceRequest.Response));
            }
            catch (Exception ex)
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
            }
        }

        public IActionResult FeatureServiceLayer(string folder, string id, int layerId)
        {
            try
            {
                //if (folder != DefaultFolder)
                //    throw new Exception("Unknown folder: " + folder);

                var map = InternetMapServer.Instance.GetService(id, folder);
                if (map == null)
                    throw new Exception("Unknown service: " + id);

                var jsonLayers = new JsonLayers();
                return Result(Layer(map, layerId));
            }
            catch (Exception ex)
            {
                return Json(new JsonError()
                {
                    error = new JsonError.Error() { code = 999, message = ex.Message }
                });
            }
        }

        #endregion

        #region Helper

        #region Json

        private JsonLayer Layer(IServiceMap map, int layerId)
        {
            var datasetElement = map.MapElements.Where(e => e.ID == layerId).FirstOrDefault();
            if (datasetElement == null)
                throw new Exception("Unknown layer: " + layerId);

            var tocElement = map.TOC.GetTOCElement(datasetElement as ILayer);

            JsonLayerLink parentLayer = null;
            if(datasetElement is ILayer && ((ILayer)datasetElement).GroupLayer!=null)
            {
                parentLayer = new JsonLayerLink()
                {
                    Id = ((ILayer)datasetElement).GroupLayer.ID,
                    Name = ((ILayer)datasetElement).GroupLayer.Title
                };
            }

            if (datasetElement is GroupLayer && datasetElement.Class==null)  // GroupLayer
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
                if(datasetElement is IFeatureLayer)
                {
                    var featureLayer = (IFeatureLayer)datasetElement;

                    drawingInfo = new JsonDrawingInfo()
                    {
                        Renderer = JsonRenderer.FromFeatureRenderer(featureLayer.FeatureRenderer)
                    };
                }

                return new JsonLayer()
                {
                    CurrentVersion = Version,
                    Id = datasetElement.ID,
                    Name = tocElement != null ? tocElement.Name : datasetElement.Title,
                    DefaultVisibility = tocElement != null ? tocElement.LayerVisible : true,
                    MaxScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MinimumScale > 1 ? tocElement.Layers[0].MinimumScale : 0, 0) : 0,
                    MinScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MaximumScale > 1 ? tocElement.Layers[0].MaximumScale : 0, 0) : 0,
                    Fields = fields,
                    Extent = extent,
                    Type = type,
                    ParentLayer = parentLayer,
                    DrawingInfo = drawingInfo,
                    GeometryType = datasetElement.Class is IFeatureClass ? JsonLayer.ToGeometryType(((IFeatureClass)datasetElement.Class).GeometryType).ToString() : EsriGeometryType.esriGeometryNull.ToString()
                };
            }
        }

        #endregion

        private IActionResult Result(object obj)
        {
            string format = ResultFormat();

            if (format == "json")
                return Json(obj);
            else if (format == "pjson")
                return Json(obj, new Newtonsoft.Json.JsonSerializerSettings()
                {
                    Formatting = Newtonsoft.Json.Formatting.Indented
                });

            #region ToHtml

            ViewData["htmlbody"] = ToHtml(obj);
            return View("_htmlbody");

            #endregion
        }

        public IActionResult FormResult(object obj)
        {
            ViewData["htmlBody"] = ToHtmlForm(obj);
            return View("_htmlbody");
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

        #region Html

        private string ToHtml(object obj)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<div class='html-body'>");

            sb.Append("<h3>" + obj.GetType().ToString() + " (YAML):</h3>");

            sb.Append("<div class='code-block'>");
            sb.Append(ToYamlHtml(obj));
            sb.Append("</div>");

            foreach(var serviceMethodAttribute in obj.GetType().GetCustomAttributes<ServiceMethodAttribute>())
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

            foreach(var propertyInfo in type.GetProperties())
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
                                sb.Append(HtmlYamlValue(linkAttribute, val));
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
                    sb.Append(HtmlYamlValue(linkAttribute, propertyInfo.GetValue(obj)));
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

        private string HtmlYamlValue(HtmlLinkAttribute htmlLink, object val)
        {
            string valString = val?.ToString() ?? String.Empty;

            if(val is double || val is float)
            {
                valString = valString.Replace(",", ".");
            }

            if (htmlLink == null)
                return valString;

            

            string link = htmlLink.LinkTemplate.Replace("{url}", InternetMapServer.AppRootUrl(this.Request) + "/" + Request.Path).Replace("{0}", valString);
            return "<a href='" + link + "'>" + valString + "</a>";
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

                if (propertyInfo.GetMethod.IsPublic && propertyInfo.SetMethod.IsPublic)
                {
                    sb.Append("<tr>");
                    sb.Append("<td>");
                    sb.Append("<span>" + propertyInfo.Name + ":</span>");
                    sb.Append("</td><td class='input'>");

                    if (propertyInfo.PropertyType.Equals(typeof(bool)))
                    {
                        sb.Append("<select name='" + jsonPropertyAttribute.PropertyName + "'><option value='true'>True</option><option value='false'>False</option></select>");
                    }
                    else
                    {
                        sb.Append("<input name='" + jsonPropertyAttribute.PropertyName + "' value='" + (propertyInfo.GetValue(obj)?.ToString() ?? String.Empty) + "'>");
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

        #endregion
    }
}