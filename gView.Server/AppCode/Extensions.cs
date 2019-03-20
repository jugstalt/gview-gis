using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    static public class Extensions
    {
        static public string ServiceName(this string id)
        {
            if (id.Contains("@"))
                return id.Split('@')[1].Trim();
            else if (id.Contains("/"))
                return id.Split('/')[1].Trim();

            return id;
        }

        static public string FolderName(this string id)
        {
            if (id.Contains("@"))
                return id.Split('@')[0].Trim();
            else if (id.Contains("/"))
                return id.Split('/')[0].Trim();

            return String.Empty;
        }

        static public string CombineUri(this string baseUrl, string urlPath)
        {
            while (baseUrl.EndsWith("/"))
                baseUrl = baseUrl.Substring(0, baseUrl.Length - 1);
            while (urlPath.StartsWith("/"))
                urlPath = urlPath.Substring(1);

            return baseUrl + "/" + urlPath;
            //return new Uri(new Uri(baseUrl), urlPath).ToString();
        }

        static public string ToValidUri(this string uri)
        {
            int pos1 = uri.IndexOf("://") < 0 ? 0 : uri.IndexOf("://") + 3;
            int pos2 = uri.IndexOf("?") < 0 ? uri.Length : uri.IndexOf("?"), pos;

            while ((pos = uri.LastIndexOf("//")) > 0)
            {
                if (pos > pos1 && pos < pos2)
                {
                    uri = uri.Substring(0, pos) + uri.Substring(pos + 1);
                    pos2 = pos2--;
                }
                else
                {
                    break;
                }
            }

            return uri;
        }
    }
}
