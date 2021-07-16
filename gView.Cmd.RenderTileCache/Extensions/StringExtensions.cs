using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Cmd.RenderTileCache.Extensions
{
    static class StringExtensions
    {
        static public string ToWmtsUrl(this string server, string service, string request="GetCapabilities")
        {
            if (service.Contains("/"))
                service = service.Replace("/", "@");

            return $"{ server }/ogc/{ service }?service=WMTS&version=1.0.0&request={ request }";
        }
    }
}
