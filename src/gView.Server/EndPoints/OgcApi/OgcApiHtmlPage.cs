#nullable enable

using gView.Framework.Common;
using gView.Server.AppCode;
using gView.Server.Extensions;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace gView.Server.EndPoints.OgcApi;

/// <summary>
/// Page frame for the OGC API HTML encoding. Minimal API endpoints can't render the MVC layout
/// (_Layout.cshtml generates urls with the legacy MVC router), so this frame rebuilds the same
/// shell (sidebar, top bar, color mode) with the stylesheet of the server pages.
/// </summary>
internal static class OgcApiHtmlPage
{
    public static string Render(HttpContext httpContext, string title, string body)
    {
        string pathBase = httpContext.Request.PathBase.Value?.TrimEnd('/') ?? "";
        string version = SystemInfo.Version.ToString();

        string username = "";
        bool isManageUser = false;
        var loginManager = httpContext.RequestServices.GetService<LoginManager>();
        if (loginManager is not null)
        {
            username = loginManager.LoginUsername(httpContext.Request);
            isManageUser = loginManager.IsManageUser(httpContext.Request);
        }

        string manageLink = httpContext.Request.IsFromAdminNetwork() && (isManageUser || (Globals.AllowFormsLogin && string.IsNullOrWhiteSpace(username)))
            ? $"""<a href="{pathBase}/manage"><span class="ico ico-shield"></span><span>Manage</span></a>"""
            : "";

        string userLinks = !string.IsNullOrWhiteSpace(username)
            ? $"""
                <div class="app-user" title="Logged in"><span class="ico ico-users"></span><span>{WebUtility.HtmlEncode(username)}</span></div>
                <a href="{pathBase}/token/logout"><span class="ico ico-logout"></span><span>Logout</span></a>
                """
            : Globals.AllowFormsLogin
                ? $"""<a href="{pathBase}/token/login"><span class="ico ico-login"></span><span>Login</span></a>"""
                : "";

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
                    function gviewSetTheme(pref) {
                        try { localStorage.setItem('gview-server-theme', pref); } catch (e) { }
                        var dark = pref === 'dark' || (pref === 'system' && window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches);
                        document.documentElement.setAttribute('data-theme', dark ? 'dark' : 'light');
                        document.documentElement.setAttribute('data-theme-pref', pref);
                    }
                </script>
                <link rel="stylesheet" href="{{pathBase}}/css/site.css?{{version}}" />
            </head>
            <body>
                <div class="app">
                    <aside class="app-side">
                        <a class="app-brand" href="{{pathBase}}/">
                            <span class="ico ico-map"></span>
                            <span>gView<b>Server</b></span>
                            <em>v{{SystemInfo.Version.Major}}</em>
                        </a>
                        <nav class="app-nav">
                            <a href="{{pathBase}}/"><span class="ico ico-home"></span><span>Home</span></a>
                            <div class="app-nav-label">Services</div>
                            <a href="{{pathBase}}/browseservices"><span class="ico ico-layers"></span><span>Browse Services</span></a>
                            <a href="{{pathBase}}/geoservices/rest/services"><span class="ico ico-code"></span><span>GeoServices (REST)</span></a>
                            <a class="active" href="{{pathBase}}/ogcapi?f=html"><span class="ico ico-plug"></span><span>OGC API</span></a>
                            {{manageLink}}
                        </nav>
                        <nav class="app-nav bottom">
                            {{userLinks}}
                            <a href="{{pathBase}}/home/about"><span class="ico ico-info"></span><span>About</span></a>
                        </nav>
                    </aside>
                    <div class="app-main">
                        <header class="app-top">
                            <nav class="app-crumb" aria-label="Breadcrumb">
                                <a href="{{pathBase}}/">gView Server</a>
                                <span class="ico ico-chevron"></span>
                                <span>OGC API</span>
                            </nav>
                            <div class="theme-switch" role="group" aria-label="Color mode">
                                <button type="button" data-theme-set="light" title="Light" onclick="gviewSetTheme('light')"><span class="ico ico-sun"></span></button>
                                <button type="button" data-theme-set="system" title="System" onclick="gviewSetTheme('system')"><span class="ico ico-monitor"></span></button>
                                <button type="button" data-theme-set="dark" title="Dark" onclick="gviewSetTheme('dark')"><span class="ico ico-moon"></span></button>
                            </div>
                        </header>
                        <main class="app-content ogcapi-body">
                            {{body}}
                        </main>
                        <footer class="app-foot"><span class="grow"></span><span>Version {{version}}</span></footer>
                    </div>
                </div>
                <script>
                    (function () {
                        var input = document.getElementById('ogc-filter');
                        if (!input) return;
                        input.addEventListener('input', function () {
                            var q = input.value.toLowerCase();
                            document.querySelectorAll('table.table tr[data-filter]').forEach(function (tr) {
                                tr.style.display = tr.getAttribute('data-filter').indexOf(q) >= 0 ? '' : 'none';
                            });
                        });
                    })();
                </script>
            </body>
            </html>
            """;
    }
}
