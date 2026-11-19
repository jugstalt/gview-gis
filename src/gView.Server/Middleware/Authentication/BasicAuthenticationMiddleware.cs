using gView.Server.AppCode;
using gView.Server.AppCode.Extensions;
using gView.Server.Extensions;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
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

    public async Task InvokeAsync(HttpContext context, LoginManager loginManager)
    {
        if (context.User.ApplyAuthenticationMiddleware())
        {
            if (!String.IsNullOrEmpty(context.Request.Headers["Authorization"]))
            {
                var userPwd = context.Request.Headers["Authorization"].ToString().FromAuthorizationHeader();

                // throttled like every other password check (see LoginThrottleService)
                var authToken = String.IsNullOrEmpty(userPwd.username)
                    ? null
                    : loginManager.GetAuthToken(userPwd.username, userPwd.password);

                if (authToken is not null)
                {
                    context.User = authToken.ToClaimsPrincipal();
                }
            }
        }

        await _next(context);
    }
}
