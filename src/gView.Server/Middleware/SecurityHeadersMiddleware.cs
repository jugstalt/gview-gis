using gView.Server.AppCode;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace gView.Server.Middleware;

/// <summary>
/// Security headers: <c>nosniff</c> and a referrer policy for every response,
/// no framing and a content security policy for the admin and login pages
/// (<see cref="AdminPaths"/>). Map services get no frame/CSP headers, they are
/// embedded by other applications.
/// </summary>
public class SecurityHeadersMiddleware
{
    /// <summary>
    /// Inline scripts/styles are still allowed: the admin views and manage.js use
    /// inline script blocks, onclick handlers and style attributes. Monaco (service
    /// editor) needs blob workers and data fonts.
    /// </summary>
    public const string AdminContentSecurityPolicy =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: blob:; " +
        "font-src 'self' data:; " +
        "worker-src 'self' blob:; " +
        "connect-src 'self'; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "frame-ancestors 'none'";

    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers.XContentTypeOptions = "nosniff";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        if (AdminPaths.IsAdminPath(context.Request.Path))
        {
            headers.XFrameOptions = "DENY";
            headers.ContentSecurityPolicy = AdminContentSecurityPolicy;
        }

        return _next(context);
    }
}
