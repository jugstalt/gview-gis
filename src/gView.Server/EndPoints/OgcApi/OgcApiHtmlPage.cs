#nullable enable

using gView.Framework.Common;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace gView.Server.EndPoints.OgcApi;

/// <summary>
/// Page frame for the OGC API HTML encoding. Minimal API endpoints can't render the MVC layout
/// (_Layout.cshtml generates urls with the legacy MVC router), so this frame uses the same
/// stylesheet (and color mode) as the server pages.
/// </summary>
internal static class OgcApiHtmlPage
{
    public static string Render(HttpContext httpContext, string title, string body)
    {
        string pathBase = httpContext.Request.PathBase.Value?.TrimEnd('/') ?? "";
        string version = SystemInfo.Version.ToString();

        return $$"""
            <!DOCTYPE html>
            <html lang="en" data-theme="light" data-theme-pref="system">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>{{WebUtility.HtmlEncode(title)}} · gView OGC API</title>
                <script>
                    (function () {
                        var pref = 'system';
                        try { pref = localStorage.getItem('gview-server-theme') || 'system'; } catch (e) { }
                        var dark = pref === 'dark' || (pref === 'system' && window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches);
                        document.documentElement.setAttribute('data-theme', dark ? 'dark' : 'light');
                        document.documentElement.setAttribute('data-theme-pref', pref);
                    })();
                </script>
                <link rel="stylesheet" href="{{pathBase}}/css/site.css?{{version}}" />
            </head>
            <body>
                <div class="app app-plain">
                    <div class="app-main">
                        <header class="app-top">
                            <a class="app-brand" href="{{pathBase}}/"><span class="ico ico-map"></span><span>gView<b>Server</b></span><em>v{{SystemInfo.Version.Major}}</em></a>
                            <nav class="app-crumb"><span class="ico ico-chevron"></span><a href="{{pathBase}}/ogcapi?f=html">OGC API</a></nav>
                        </header>
                        <main class="app-content ogcapi-body">
                            {{body}}
                        </main>
                        <footer class="app-foot"><span class="grow"></span><span>Version {{version}}</span></footer>
                    </div>
                </div>
            </body>
            </html>
            """;
    }
}
