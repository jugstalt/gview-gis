using gView.Framework.Core.Exceptions;
using gView.Server.AppCode;
using gView.Server.AppCode.Extensions;
using gView.Server.Extensions;
using gView.Server.Services.MapServer;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace gView.Server.Middleware.Authentication;

public class UrlTokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly MapServiceManager _mapServerService;

    public UrlTokenAuthenticationMiddleware(
                RequestDelegate next,
                MapServiceManager mapServerService
        )
    {
        _next = next;
        _mapServerService = mapServerService;
    }

    public async Task InvokeAsync(HttpContext context)
        => await AuthenticateAsync(context, _next, _mapServerService.Options.LoginManagerRootPath);

    /// <summary>
    /// geoservices(token-name~secret)/rest/...: the url token logs in as its token user.
    /// A wrong or unknown url token is an invalid token, not an anonymous request.
    /// </summary>
    internal static async Task AuthenticateAsync(HttpContext context, RequestDelegate next, string loginManagerRootPath)
    {
        if (context.User.ApplyAuthenticationMiddleware())
        {
            var urlToken = context.Request.GetGeoServicesUrlToken();

            if (!String.IsNullOrEmpty(urlToken))
            {
                if (!urlToken.IsValueUrlToken())
                {
                    throw new InvalidTokenException();
                }

                var path = Path.Combine(loginManagerRootPath, "token");
                var authToken = AuthToken.Create(path, urlToken.NameOfUrlToken(), urlToken, AuthToken.AuthTypes.Tokenuser)
                    ?? throw new InvalidTokenException();

                context.User = authToken.ToClaimsPrincipal();
            }
        }

        await next(context);
    }
}
