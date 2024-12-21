using gView.Framework.Common;
using gView.Framework.Common.Reflection;
using gView.Framework.Core.MapServer;
using gView.GeoJsonService.DTOs;
using gView.Interoperability.GeoServices.Rest.Reflection;
using gView.Server.Services.Hosting;
using gView.Server.Services.MapServer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Server.AppCode.Extensions;

static internal class HtmlExtensions
{
    #region GeoServices REST

    static public string GeoServicesObjectToHtml(this object obj,
                HttpRequest request,
                UrlHelperService urlHelperService,
                MapServiceManager mapServerService)
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

        foreach (var serviceMethodAttribute in obj.GetType().GetCustomAttributes<ServiceMethodAttribute>(false))
        {
            var url = serviceMethodAttribute.Method;
            var target = String.Empty;

            if (serviceMethodAttribute.Method.StartsWith("http://") || serviceMethodAttribute.Method.StartsWith("https://"))
            {
                url = url.Replace("{onlineresource-url}", mapServerService.Options.OnlineResource);
                target = "_blank";
            }
            else
            {
                url = $"{mapServerService.Options.OnlineResource}{request.Path}/{url}";
            }

            sb.Append($"<a href='{url}' target='{target}' >{serviceMethodAttribute.Name}</a>");
        }

        sb.Append("<div class='code-block'>");
        sb.Append(ToYamlHtml(request, urlHelperService, obj));
        sb.Append("</div>");

        sb.Append("</div>");

        return sb.ToString();
    }

    #region Helper

    static private string ToYamlHtml(
            HttpRequest request,
            UrlHelperService urlHelperService,
            object obj, int spaces = 0, bool isArray = false)
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

            var jsonPropertyAttribute = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>();
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

            if (isArray || propertyInfo.PropertyType.IsArray)
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
                    List<object> arrayValues = new List<object>();
                    List<string> groupByValues = new List<string>();

                    for (int i = 0; i < array.Length; i++)
                    {
                        arrayValues.Add(array.GetValue(i));
                    }

                    var firstElement = arrayValues.Where(v => v != null).FirstOrDefault();
                    YamlGroupByAttribute groupByAttribute = null;
                    if (firstElement != null && !firstElement.GetType().IsValueType && arrayValues.Where(v => firstElement.GetType() == v?.GetType()).Count() == arrayValues.Count())
                    {
                        groupByAttribute = firstElement.GetType().GetCustomAttribute<YamlGroupByAttribute>();
                        if (!String.IsNullOrEmpty(groupByAttribute?.GroupByField))
                        {
                            groupByValues.AddRange(arrayValues.Select(v => v.GetType().GetProperty(groupByAttribute.GroupByField).GetValue(v).ToString())
                                                              .Distinct());
                        }
                    }

                    foreach (var groupBy in groupByAttribute != null ? groupByValues.ToArray() : new string[] { String.Empty })
                    {
                        int arrayIndex = 0;
                        foreach (var val in arrayValues)
                        {
                            if (val == null)
                            {
                                sb.Append("null");
                            }
                            else if (val.GetType().IsValueType || val.GetType() == typeof(string))
                            {
                                if (arrayIndex == 0)
                                {
                                    sb.Append("[");
                                }
                                else
                                {
                                    sb.Append(", ");
                                }
                                sb.Append(HtmlYamlValue(request, urlHelperService, linkAttribute, val, obj));
                                if (arrayIndex == array.Length - 1)
                                {
                                    sb.Append("]");
                                }
                            }
                            else
                            {
                                if (!String.IsNullOrEmpty(groupByAttribute?.GroupByField))
                                {
                                    if (groupBy != val.GetType().GetProperty(groupByAttribute.GroupByField).GetValue(val)?.ToString())
                                    {
                                        continue;
                                    }

                                    if (arrayIndex == 0)
                                    {
                                        sb.Append("<div class='yaml-comment'>");
                                        sb.Append($"<div>{HtmlYamlSpaces(spaces + 2)}#</div>");
                                        sb.Append($"<div>{HtmlYamlSpaces(spaces + 2)}# {groupByAttribute.GroupByField}: {groupBy}</div>");
                                        sb.Append($"<div>{HtmlYamlSpaces(spaces + 2)}#</div>");
                                        sb.Append("</div>");
                                    }
                                }

                                sb.Append(ToYamlHtml(request, urlHelperService, val, spaces + 2, true));
                            }
                            arrayIndex++;
                        }
                    }
                }
            }
            else if (propertyInfo.PropertyType.IsValueType || propertyInfo.PropertyType == typeof(string))
            {
                sb.Append(HtmlYamlValue(request, urlHelperService, linkAttribute, propertyInfo.GetValue(obj), obj));
            }
            else
            {
                sb.Append(ToYamlHtml(request, urlHelperService, propertyInfo.GetValue(obj), spaces + 2));
            }
            sb.Append("</div>");
            sb.Append("<br/>");

            isFirst = false;
        }
        sb.Append("</div>");

        return sb.ToString();
    }

    static private string HtmlYamlSpaces(int spaces)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < spaces; i++)
        {
            sb.Append("&nbsp;");
        }

        return sb.ToString();
    }

    static private string HtmlYamlValue(
            HttpRequest request,
            UrlHelperService urlHelperService,
            HtmlLinkAttribute htmlLink,
            object val,
            object instance)
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
            .Replace("{url}", urlHelperService.AppRootUrl(request).CombineUri(request.Path))
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

    static private IEnumerable<PropertyInfo> SortYamlProperties(IEnumerable<PropertyInfo> propertyInfos)
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

    #endregion

    static public string GeoServicesObjectToHtmlForm(this object obj)
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
            var jsonPropertyAttribute = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>();
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

                if (jsonPropertyAttribute.Name == "f")
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
                    sb.Append("<select name='" + jsonPropertyAttribute.Name + "' style='min-width:auto;'><option value='false'>False</option><option value='true'>True</option></select>");
                }
                else
                {
                    switch (inputType)
                    {
                        case FormInputAttribute.InputTypes.TextBox:
                            sb.Append("<textarea rows='3' name='" + jsonPropertyAttribute.Name + "'>" + (propertyInfo.GetValue(obj)?.ToString() ?? String.Empty) + "</textarea>");
                            break;
                        case FormInputAttribute.InputTypes.TextBox10:
                            sb.Append("<textarea rows='10' name='" + jsonPropertyAttribute.Name + "'>" + (propertyInfo.GetValue(obj)?.ToString() ?? String.Empty) + "</textarea>");
                            break;
                        case FormInputAttribute.InputTypes.Hidden:
                            sb.Append("<input type='hidden' name='" + jsonPropertyAttribute.Name + "' value='" + (propertyInfo.GetValue(obj)?.ToString() ?? String.Empty) + "'>");
                            break;
                        case FormInputAttribute.InputTypes.Password:
                            sb.Append("<input name='" + jsonPropertyAttribute.Name + "' type='password' value='" + (propertyInfo.GetValue(obj)?.ToString() ?? String.Empty) + "'>");
                            break;
                        default:
                            if (inputAttribute?.Values != null && inputAttribute.Values.Count() > 0)
                            {
                                sb.Append("<select name='" + jsonPropertyAttribute.Name + "' style='min-width:auto;'>");
                                foreach (var val in inputAttribute.Values)
                                {
                                    sb.Append("<option value='" + val + "'>" + val + "</option>");
                                }
                                sb.Append("</select>");
                            }
                            else
                            {
                                sb.Append("<input name='" + jsonPropertyAttribute.Name + "' value='" + (propertyInfo.GetValue(obj)?.ToString() ?? String.Empty) + "'>");
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

    #region GeoJson Services

    static public string GeoJsonObjectToHtml(this object obj, IMapService mapService)
    {
        var json = JsonSerializer.Serialize(obj,
            new JsonSerializerOptions()
            {
                Converters =
                {
                    new JsonStringEnumConverter(),
                },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true
            });

        StringBuilder sb = new StringBuilder();



        sb.Append("<div class='html-body'>");
        sb.Append("<h3>" + obj.GetType().Name + " (JSON):</h3>");

        if (obj is BaseRequest || obj is BaseResponse)
        {
            sb.Append("<p>GeoJson Service");
            sb.Append(Link("Specification", "https://docs.gviewonline.com/en/spec/geojson_service/index.html"));
            sb.Append("</p>");
        }

        if (obj is GetServiceCapabilitiesResponse capabilities)
        {
            sb.Append("<div>");
            foreach (var supportedRequest in capabilities.SupportedRequests ?? [])
            {
                if (supportedRequest.Url.IndexOf(mapService.Fullname) < 0) continue;

                var url = supportedRequest.Url.Substring(
                                supportedRequest.Url.LastIndexOf(mapService.Fullname) + mapService.Fullname.Length + 1);

                if (url.Contains("{layerId}"))
                {
                    sb.Append("<div>");
                    foreach (var layer in capabilities.Layers?
                                                      .Where(l => l.SuportedOperations?
                                                                   .Contains(supportedRequest.Name) == true) ?? [])
                    {
                        sb.Append(Link($"{supportedRequest.Name} {layer.Name}",
                                       $"./../GeoJsonRequest?id={mapService.Fullname}&request={url.Replace("{layerId}", layer.Id)}"));
                    }
                    sb.Append("</div>");
                }
                else
                {
                    sb.Append(Link(supportedRequest.Name, $"./../GeoJsonRequest?id={mapService.Fullname}&request={url}"));
                }
            }
            sb.Append("</div>");
        }

        sb.Append("<div class='code-block'>");
        sb.Append("<pre>");
        sb.Append(json);
        sb.Append("</pre>");
        sb.Append("</div>");
        sb.Append("</div>");

        return sb.ToString();
    }

    static public string GeoJsonObjectToInputForm(this object obj, string id, string request)
    {
        if (obj == null)
        {
            return String.Empty;
        }

        StringBuilder sb = new StringBuilder();

        sb.Append("<div class='html-body'>");
        sb.Append("<form method='post'>");

        sb.Append($"<input type='hidden' name='id' value='{id}' />");
        sb.Append($"<input type='hidden' name='request' value='{request}' />");

        sb.Append("<table>");

        foreach (var propertyInfo in obj.GetType().GetProperties())
        {
            if (propertyInfo.GetMethod.IsPublic && propertyInfo.SetMethod.IsPublic)
            {
                string name = propertyInfo.Name;
                object value = propertyInfo.GetValue(obj);

                if (name == "Type")
                {
                    sb.Append($"<tr><td></td><td><h2>{value}</h2></td></tr>");
                    continue;
                }

                if (value is BBox bbox)
                {
                    value = $"{bbox.MinX.ToDoubleString()},{bbox.MinY.ToDoubleString()},{bbox.MaxX.ToDoubleString()},{bbox.MaxY.ToDoubleString()}";
                }
                else if (value is CoordinateReferenceSystem crs)
                {
                    value = crs.ToSpatialReferenceName();
                }
                else if(value is string[] stringArray)
                {
                    value = String.Join(",", stringArray);
                }

                var inputType = FormInputAttribute.InputTypes.Text;

                sb.Append("<tr>");
                sb.Append("<td>");

                if (inputType != FormInputAttribute.InputTypes.Hidden)
                {
                    sb.Append($"<span>{propertyInfo.Name}:</span>");
                }
                sb.Append("</td><td class='input'>");

                if (propertyInfo.PropertyType.Equals(typeof(bool)))
                {
                    sb.Append($"<select name='{name}' style='min-width:auto;'><option value='false'>False</option><option value='true'>True</option></select>");
                }
                else
                {
                    switch (inputType)
                    {
                        case FormInputAttribute.InputTypes.TextBox:
                            sb.Append($"<textarea rows='3' name='{name}'>{(value?.ToString() ?? String.Empty)}</textarea>");
                            break;
                        case FormInputAttribute.InputTypes.TextBox10:
                            sb.Append($"<textarea rows='10' name='{name}'>{(value?.ToString() ?? String.Empty)}</textarea>");
                            break;
                        case FormInputAttribute.InputTypes.Hidden:
                            sb.Append($"<input type='hidden' name='{name}' value='{(value?.ToString() ?? String.Empty)}'>");
                            break;
                        case FormInputAttribute.InputTypes.Password:
                            sb.Append($"<input name='{name}' type='password' value='{(value?.ToString() ?? String.Empty)}'>");
                            break;
                        default:
                            if(propertyInfo.PropertyType.IsEnum)
                            {
                                sb.Append($"<select name='{name}' style='min-width:auto;'>");
                                foreach (var enumValue in Enum.GetValues(propertyInfo.PropertyType))
                                {
                                    sb.Append($"<option value='{(int)enumValue}'>{enumValue}</option>");
                                }
                                sb.Append("</select>");
                            }
                            else
                            {
                                sb.Append($"<input name='{name}' value='{(value?.ToString() ?? String.Empty)}'>");
                            }
                            break;
                    }
                }
                sb.Append("</td>");
                sb.Append("</tr>");
            }
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

    #region Helper

    static private string Link(string title, string url)
    {
        return $"<a target='_blank' href='{url}'>{title}</a>";
    }

    #endregion
}
