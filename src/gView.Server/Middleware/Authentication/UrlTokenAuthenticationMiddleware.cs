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
    {
        if (context.User.ApplyAuthenticationMiddleware())
        {
            var urlToken = context.Request.GetGeoServicesUrlToken();

            if (!String.IsNullOrEmpty(urlToken))
            {
                var urlTokenName = urlToken.NameOfUrlToken();
                var path = Path.Combine(_mapServerService.Options.LoginManagerRootPath, "token");

                var authToken = AuthToken.Create(path, urlTokenName, urlToken, AuthToken.AuthTypes.Tokenuser);
            }
        }

        await _next(context);
    }
}
