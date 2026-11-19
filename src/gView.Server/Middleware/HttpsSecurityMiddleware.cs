using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Threading.Tasks;

namespace gView.Server.Middleware;

/// <summary>
/// HSTS and the optional http → https redirect (see <see cref="SecurityOptions"/>).
/// Runs after the forwarded headers, so <c>Request.IsHttps</c> reflects what the
/// client used in front of a trusted proxy.
/// </summary>
public class HttpsSecurityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityOptions _options;

    public HttpsSecurityMiddleware(RequestDelegate next, SecurityOptions options)
    {
        _next = next;
        _options = options;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        bool isLocalhost = IsLocalhost(request.Host.Host);

        if (_options.HttpsRedirect && !request.IsHttps && !isLocalhost)
        {
            var url = UriHelper.BuildAbsolute("https", new HostString(request.Host.Host), request.PathBase, request.Path, request.QueryString);

            context.Response.StatusCode = StatusCodes.Status308PermanentRedirect;
            context.Response.Headers.Location = url;
            return Task.CompletedTask;
        }

        // browsers ignore HSTS on http responses, so with force-https it can be sent always:
        // behind a proxy without X-Forwarded-Proto the request looks like http here
        if ((request.IsHttps || _options.ForceHttps) && !isLocalhost && _options.HstsMaxAgeDays > 0)
        {
            context.Response.Headers.StrictTransportSecurity = $"max-age={(long)TimeSpan.FromDays(_options.HstsMaxAgeDays).TotalSeconds}";
        }

        return _next(context);
    }

    internal static bool IsLocalhost(string host)
        => String.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
        || host == "127.0.0.1"
        || host == "[::1]"
        || host == "::1";
}
