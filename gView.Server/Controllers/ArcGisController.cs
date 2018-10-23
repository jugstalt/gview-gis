using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gView.Framework.Data;
using gView.Interoperability.ArcGisServer.Rest.Json;
using gView.Server.AppCode;
using Microsoft.AspNetCore.Mvc;
using static gView.Interoperability.ArcGisServer.Rest.Json.JsonServices;
using System.Reflection;
using Newtonsoft.Json;
using gView.Framework.system;
using gView.MapServer;
using gView.Interoperability.ArcGisServer.Request;
using System.Collections.Specialized;
using gView.Interoperability.ArcGisServer.Rest.Json.Request;
using Microsoft.Extensions.Primitives;
using gView.Interoperability.ArcGisServer.Rest.Json.Response;
using gView.Framework.Carto;

namespace gView.Server.Controllers
{
    public class ArcGisController : Controller
    {
        private const double Version = 10.61;
        private const string DefaultFolder = "default";

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
                Folders = new string[] { DefaultFolder },
                Services = new AgsServices[0]
            });
        }

        public IActionResult Folder(string id)
        {
            try
            {
                if (id != DefaultFolder)
                    throw new Exception("Unknown folder: " + id);

                List<AgsServices> agsServices = new List<AgsServices>();
                foreach (var service in InternetMapServer.mapServices)
                {
                    agsServices.Add(new AgsServices()
                    {
                        Name = DefaultFolder + "/" + service.Name,
                        Type = "MapServer"
                    });
                }

                return Result(new JsonServices()
                {
                    CurrentVersion = 10.61,
                    Services = agsServices.ToArray(),
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
                if (folder != DefaultFolder)
                    throw new Exception("Unknown folder: " + folder);

                var map = InternetMapServer.Instance[id];
                if (map == null)
                    throw new Exception("Unknown service: " + id);

                return Result(new JsonService()
                {
                    CurrentVersion = 10.61,
                    Layers = map.MapElements.Select(e =>
                    {
                        var tocElement = map.TOC.GetTOCElement(e as ILayer);
                        return new JsonService.Layer()
                        {
                            Id = e.ID,
                            Name = tocElement != null ? tocElement.Name : e.Title,
                            DefaultVisibility = tocElement != null ? tocElement.LayerVisible : true,
                            MinScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MinimumScale, 0) : 0,
                            MaxScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MaximumScale, 0) : 0,
                        };
                    }).ToArray(),
                    FullExtent = new JsonService.Extent()
                    {
                        XMin = map?.Display?.Envelope.minx ?? 0D,
                        YMin = map?.Display?.Envelope.miny ?? 0D,
                        XMax = map?.Display?.Envelope.maxx ?? 0D,
                        YMax = map?.Display?.Envelope.maxy ?? 0D,
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
                if (folder != DefaultFolder)
                    throw new Exception("Unknown folder: " + folder);

                var map = InternetMapServer.Instance[id];
                if (map == null)
                    throw new Exception("Unknown service: " + id);

                var jsonLayers = new JsonLayers();
                jsonLayers.Layers = map.MapElements.Select(e => Layer(map, e.ID)).ToArray();

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
                if (folder != DefaultFolder)
                    throw new Exception("Unknown folder: " + folder);

                var map = InternetMapServer.Instance[id];
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

        public IActionResult ExportMap(string folder, string id)
        {
            try
            {
                if (folder != DefaultFolder)
                    throw new Exception("Unknown folder: " + folder);

                var interpreter = InternetMapServer.GetInterpreter(typeof(ArcGisServerInterperter));

                #region Request

                JsonExportMap exportMap = Deserialize<JsonExportMap>(
                    Request.HasFormContentType ?
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);

                ServiceRequest serviceRequest = new ServiceRequest(id, JsonConvert.SerializeObject(exportMap))
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

                InternetMapServer.ThreadQueue.AddQueuedThreadSync(interpreter.Request, context);

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

        public IActionResult Query(string folder, string id, int layerId)
        {
            try
            {
                if (folder != DefaultFolder)
                    throw new Exception("Unknown folder: " + folder);

                var interpreter = InternetMapServer.GetInterpreter(typeof(ArcGisServerInterperter));

                #region Request

                JsonQueryLayer queryLayer = Deserialize<JsonQueryLayer>(
                    Request.HasFormContentType ?
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Form :
                    (IEnumerable<KeyValuePair<string, StringValues>>)Request.Query);
                queryLayer.LayerId = layerId;

                ServiceRequest serviceRequest = new ServiceRequest(id, JsonConvert.SerializeObject(queryLayer))
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

                InternetMapServer.ThreadQueue.AddQueuedThreadSync(interpreter.Request, context);

                #endregion

                if (serviceRequest.Succeeded)
                {
                    return Result(JsonConvert.DeserializeObject<JsonFeatureResponse>(serviceRequest.Response));
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

        #region Helper

        #region Json

        private JsonLayer Layer(IServiceMap map, int layerId)
        {
            var datasetElement = map.MapElements.Where(e => e.ID == layerId).FirstOrDefault();
            if (datasetElement == null)
                throw new Exception("Unknown layer: " + layerId);

            var tocElement = map.TOC.GetTOCElement(datasetElement as ILayer);

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

            return new JsonLayer()
            {
                CurrentVersion = Version,
                Id = datasetElement.ID,
                Name = tocElement != null ? tocElement.Name : datasetElement.Title,
                DefaultVisibility = tocElement != null ? tocElement.LayerVisible : true,
                MinScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MinimumScale, 0) : 0,
                MaxScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MaximumScale, 0) : 0,
                Fields = fields,
                Extent = extent,
                Type = type,
                GeometryType = datasetElement.Class is IFeatureClass ? JsonLayer.ToGeometryType(((IFeatureClass)datasetElement.Class).GeometryType).ToString() : EsriGeometryType.esriGeometryNull.ToString()
            };
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

        private string ToHtml(object obj)
        {
            if (obj == null)
                return String.Empty;

            var type = obj.GetType();

            StringBuilder sb = new StringBuilder();
            sb.Append("<ul class='property-list'>");
            foreach(var propertyInfo in type.GetProperties())
            {
                if (propertyInfo.GetValue(obj) == null)
                    continue;

                var jsonPropertyAttribute = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
                if (jsonPropertyAttribute == null)
                    continue;

                var linkAttribute = propertyInfo.GetCustomAttribute<HtmlLinkAttribute>();

                sb.Append("<li>");

                bool newLine = !(propertyInfo.PropertyType.IsValueType || propertyInfo.PropertyType == typeof(string));
                if (newLine == true)
                {
                    if (propertyInfo.PropertyType.IsArray && (propertyInfo.GetValue(obj) == null || ((Array)propertyInfo.GetValue(obj)).Length == 0))
                        newLine = false;
                }

                sb.Append("<div class='property-name" +  (newLine ? " array" : "") + "'>" + propertyInfo.Name + ":&nbsp;</div>");
                sb.Append("<div class='property-value'>");
                if(propertyInfo.PropertyType.IsArray)
                {
                    var array = (Array)propertyInfo.GetValue(obj);
                    if (array == null)
                    {
                        sb.Append("null");
                    }
                    else if(array.Length==0)
                    {
                        sb.Append("[]");
                    }
                    else
                    {
                        sb.Append("<ul class='property-array'>");
                        //sb.Append("<li>[</li>");
                        for (int i = 0; i < array.Length; i++)
                        {
                            if(i>0)
                            {
                                sb.Append("<li>,</li>");
                            }
                            sb.Append("<li class='array-value'>");
                            var val = array.GetValue(i);
                            if(val==null)
                            {
                                sb.Append("null");
                            }
                            else if (val.GetType().IsValueType || val.GetType() == typeof(string))
                            {
                                sb.Append(HtmlValue(linkAttribute, val.ToString()));
                            }
                            else
                            {
                                sb.Append(ToHtml(val));
                            }
                            sb.Append("</li>");
                        }
                        //sb.Append("<li>]</li>");
                        sb.Append("</ul>");
                    }
                }
                else if(propertyInfo.PropertyType.IsValueType || propertyInfo.PropertyType == typeof(string))
                {
                    sb.Append(HtmlValue(linkAttribute, propertyInfo.GetValue(obj)?.ToString() ?? String.Empty));
                }
                else
                {
                    sb.Append(ToHtml(propertyInfo.GetValue(obj)));
                }
                sb.Append("</div>");
                sb.Append("</li>");
            }
            sb.Append("</ul>");

            return sb.ToString();
        }

        private string HtmlValue(HtmlLinkAttribute htmlLink, string val)
        {
            if (htmlLink == null)
                return val;

            string link = htmlLink.LinkTemplate.Replace("{url}", Request.Scheme + "://" + Request.Host + "/" + Request.Path).Replace("{0}", val);
            return "<a href='" + link + "'>" + val + "</a>";
        }

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