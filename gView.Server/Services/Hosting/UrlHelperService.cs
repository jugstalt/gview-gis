using gView.Server.Services.MapServer;
using Microsoft.AspNetCore.Http;

namespace gView.Server.Services.Hosting
{
    public class UrlHelperService
    {
        public MapServiceManager _mapServerSercice;

        public UrlHelperService(MapServiceManager mapServerService)
        {
            _mapServerSercice = mapServerService;
        }

        public string AppRootUrl(HttpRequest request)
        {
            //string url = String.Format("{0}{1}{2}", UrlScheme(request, httpSchema), request..Authority, urlHelper.UrlContent("~"));
            //if (url.EndsWith("/"))
            //    return url.Substring(0, url.Length - 1);

            //return url;

            string scheme = _mapServerSercice.Options.ForceHttps == true ? "https" : request.Scheme;

            return $"{scheme}://{request.Host}{request.PathBase}";
        }
    }
}
