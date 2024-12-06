using gView.Server.AppCode;
using gView.Server.AppCode.Extensions;
using gView.Server.Models;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.Middleware.Authentication;

public class OidcAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AuthConfigModel _authConfig;

    public OidcAuthenticationMiddleware(
                RequestDelegate next,
                IConfiguration configuration
        )
    {
        _next = next;

        _authConfig = new AuthConfigModel();
        configuration.Bind("Authentication", _authConfig);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.ApplyAuthenticationMiddleware())
        {
            if (!String.IsNullOrEmpty(_authConfig?.RequiredManageRole)
                && context.User.GetRoles().Contains(_authConfig.RequiredManageRole))
            {
                var authToken = new AuthToken(
                            context.User.Identity.Name,
                            AuthToken.AuthTypes.Manage,
                            DateTime.UtcNow.AddDays(1).Ticks
                       );

                context.User = authToken.ToClaimsPrincipal();
            }
        }

        await _next(context);
    }
}
