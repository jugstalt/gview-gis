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

namespace gView.Server.Controllers
{
    public class RestController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Services");
        }

        public IActionResult Services()
        {
            return Result(new JsonServices()
            {
                CurrentVersion = 10.61,
                Folders = new string[] { "gview" },
                Services = new AgsServices[0]
            });
        }

        public IActionResult Folder(string id)
        {
            try
            {
                if (id != "gview")
                    throw new Exception("Unknown folder: " + id);

                List<AgsServices> agsServices = new List<AgsServices>();
                foreach (var service in InternetMapServer.mapServices)
                {
                    agsServices.Add(new AgsServices()
                    {
                        Name = service.Name,
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
                if (folder != "gview")
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
                            Name = e.Title,
                            DefaultVisiblity = tocElement != null ? tocElement.LayerVisible : true,
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

        #region Helper

        private IActionResult Result(object obj)
        {
            if (Request.Query["f"] == "json")
                return Json(obj);
            else if (Request.Query["f"] == "pjson")
                return Json(obj, new Newtonsoft.Json.JsonSerializerSettings()
                {
                    Formatting = Newtonsoft.Json.Formatting.Indented
                });

            #region ToHtml

            ViewData["htmlbody"] = ToHtml(obj);
            return View("_htmlbody");

            #endregion
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

        #endregion
    }
}