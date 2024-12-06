using gView.Server.AppCode;
using gView.Server.AppCode.Extensions;
using gView.Server.Extensions;
using gView.Server.Services.MapServer;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace gView.Server.Middleware.Authentication;

public class BasicAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly MapServiceManager _mapServerService;

    public BasicAuthenticationMiddleware(
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
            if (!String.IsNullOrEmpty(context.Request.Headers["Authorization"]))
            {
                var userPwd = context.Request.Headers["Authorization"].ToString().FromAuthorizationHeader();
                var path = Path.Combine(_mapServerService.Options.LoginManagerRootPath, "token");

                var authToken = AuthToken.Create(path, userPwd.username, userPwd.password, AuthToken.AuthTypes.Tokenuser);

                if (authToken is not null)
                {
                    context.User = authToken.ToClaimsPrincipal();
                }
            }
        }

        await _next(context);
    }
}
