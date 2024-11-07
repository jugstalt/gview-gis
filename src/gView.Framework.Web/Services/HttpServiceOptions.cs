using System;
using System.Collections.Generic;
using System.Net;

namespace gView.Framework.Web.Services
{
    public class HttpServiceOptions
    {
        public string DefaultClientName { get; set; }
        public string DefaultProxyClientName { get; set; }
        public bool UseProxy => !String.IsNullOrEmpty(DefaultProxyClientName);
        public string[] IgnoreProxyServers { get; set; }
        public bool ForceHttps { get; set; }
        public WebProxy WebProxyInstance { get; set; }

        public Dictionary<string, string> UrlRedirections { get; set; }

        public string[] Legacy_AlwaysDownloadFrom { get; set; }
    }
}
