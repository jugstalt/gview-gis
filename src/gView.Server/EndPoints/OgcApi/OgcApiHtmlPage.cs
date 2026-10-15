#nullable enable

using gView.Framework.Common;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace gView.Server.EndPoints.OgcApi;

/// <summary>
/// Page frame for the OGC API HTML encoding. Minimal API endpoints can't render the MVC layout
/// (_Layout.cshtml generates urls with the legacy MVC router), so this frame uses the same
/// stylesheets as the server pages.
/// </summary>
internal static class OgcApiHtmlPage
{
    public static string Render(HttpContext httpContext, string title, string body)
    {
        string pathBase = httpContext.Request.PathBase.Value?.TrimEnd('/') ?? "";
        string version = SystemInfo.Version.ToString();

        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>{{WebUtility.HtmlEncode(title)}} - gView OGC API</title>
                <link rel="stylesheet" href="{{pathBase}}/lib/bootstrap/dist/css/bootstrap.css" />
                <link rel="stylesheet" href="{{pathBase}}/css/site.css?{{version}}" />
                <style>
                    body { padding: 20px; }
                    .ogcapi-breadcrumb { margin-bottom: 10px; }
                    .ogcapi-header { font-size: 1.1em; margin-bottom: 20px; }
                    td, th { white-space: nowrap; }
                </style>
            </head>
            <body>
                <div class="ogcapi-header"><a href="{{pathBase}}/">gView Server</a> | <a href="{{pathBase}}/ogcapi?f=html">OGC API</a></div>
                {{body}}
            </body>
            </html>
            """;
    }
}
