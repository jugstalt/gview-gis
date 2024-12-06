using gView.Framework.Core.Exceptions;
using gView.Server.AppCode.Extensions;
using gView.Server.Extensions;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;

namespace gView.Server.Middleware.Authentication;

public class JwtTokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtAccessTokenService _jwtTokenService;

    public JwtTokenAuthenticationMiddleware(
                RequestDelegate next,
                JwtAccessTokenService jwtTokenService
        )
    {
        _next = next;
        _jwtTokenService = jwtTokenService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.ApplyAuthenticationMiddleware())
        {
            string authHeader = context.Request.Headers.Authorization;

            if (authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();

                try
                {
                    context.User = _jwtTokenService.ValidateToken(token);
                }
                catch(SecurityTokenException ste)
                {
                    throw new MapServerException(ste.Message);
                }
            }
        }

        await _next(context);
    }
}
