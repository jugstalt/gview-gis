#nullable enable

using System.Net;

namespace gView.Server.EndPoints;

/// <summary>
/// The page that shows an OpenAPI description (GeoJson services, OGC API) with Scalar, served locally from
/// wwwroot/lib/scalar (no CDN)
/// </summary>
internal static class ApiReferencePage
{
    public const string ScalarVersion = "1.84.0";

    /// <param name="specUrl">the url of the OpenAPI JSON</param>
    /// <param name="libUrl">the url of wwwroot/lib (with the path base of the server)</param>
    public static string Html(string title, string specUrl, string libUrl)
        => $$"""
            <!doctype html>
            <html>
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
              <title>{{WebUtility.HtmlEncode(title)}}</title>
            </head>
            <body>
              <div id="app"></div>
              <script src="{{WebUtility.HtmlEncode(libUrl)}}/scalar/standalone.js?{{ScalarVersion}}"></script>
              <script>
                Scalar.createApiReference('#app', { url: '{{JsString(specUrl)}}', hideClientButton: true, withDefaultFonts: false, showDeveloperTools: 'never', telemetry: false });
              </script>
            </body>
            </html>
            """;

    private static string JsString(string text) => text.Replace("\\", "\\\\").Replace("'", "\\'").Replace("<", "\\u003c");
}
