using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.IO;
using gView.Framework.UI;
using System;
using System.Linq;

namespace gView.Framework.system
{
    static public class Extensions
    {
        //static public T NonblockingResult<T>(this Task task)
        //{
        //    var nonBlockingTask = Task.Run(() => task);
        //    nonBlockingTask.Wait();

        //    var property=task.GetType().GetProperty("Result");
        //    if (property != null)
        //        return (T)property.GetValue(task);

        //    return default(T);
        //}

        static public string ExtractConnectionStringParameter(this string connectionString, string parameterName)
        {
            parameterName = parameterName.ToLower();

            foreach (var p in connectionString.Split(';'))
            {
                if (p.ToLower().StartsWith(parameterName + "="))
                {
                    return p.Substring(parameterName.Length + 1);
                }
            }

            return String.Empty;
        }

        #region Numbers

        //static private IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        //static public string ToDoubleString(this double d)
        //{
        //    return d.ToString(_nhi);
        //}

        #endregion

        #region Url

        static public string UrlRemoveEndingSlashes(this string url)
        {
            if (!String.IsNullOrEmpty(url))
            {
                url = url.Trim();

                while (url.EndsWith("/"))
                {
                    url = url.Substring(0, url.Length - 1);
                }
            }

            return url;
        }

        static public string UrlRemoveBeginningSlashes(this string url)
        {
            if (!String.IsNullOrEmpty(url))
            {
                url = url.Trim();

                while (url.StartsWith("/"))
                {
                    url = url.Substring(1);
                }
            }

            return url;
        }

        static public string UrlToConfigId(this string url)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(url))
                {
                    var uri = new Uri(url);
                    url = uri.Host;

                    if (uri.Port != 80 && uri.Port != 443)
                    {
                        url += "__" + uri.Port;
                    }
                }
            }
            catch
            {
                return "invalid_server_url";
            }

            return url;
        }

        static public string UrlAppendParameters(this string url, string parameters)
        {
            if (!String.IsNullOrEmpty(url))
            {
                while (url.EndsWith("&") || url.EndsWith("?"))
                {
                    url = url.Substring(0, url.Length - 1);
                }

                url += (url.Contains("?") ? "&" : "?") + parameters;
            }

            return url;
        }

        static public string UrlAppendPath(this string url, string path)
        {
            path = path?.UrlRemoveBeginningSlashes()?.UrlRemoveEndingSlashes();

            if (!String.IsNullOrEmpty(url) && !String.IsNullOrWhiteSpace(path))
            {
                url = $"{url.UrlRemoveEndingSlashes()}/{path}";
            }

            return url;
        }

        #endregion

        #region IMapDocument

        static public IMap MapFromDataset(this IMapDocument doc, IDataset dataset)
        {
            if (dataset == null)
            {
                return null;
            }

            return doc?.Maps?
                    .Where(m => m.Datasets != null && m.Datasets.Contains(dataset))
                    .FirstOrDefault();
        }

        static public IMap MapFromLayer(this IMapDocument doc, ILayer layer)
        {
            if (doc?.Maps == null)
            {
                return null;
            }

            foreach (var map in doc.Maps)
            {
                if (map?.TOC?.Elements == null)
                {
                    continue;
                }

                foreach (var element in map.TOC.Elements)
                {
                    if (element.Layers.Contains(layer))
                    {
                        return map;
                    }
                }
            }

            return null;
        }

        static public void TemporaryRestore(this IMapDocument doc)
        {
            if(doc is IPersistableTemporaryRestore)
            {
                ((IPersistableTemporaryRestore)doc).TemporaryRestore();
            }
        }

        #endregion
    }
}
