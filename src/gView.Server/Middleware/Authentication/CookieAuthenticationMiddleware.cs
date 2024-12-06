#nullable enable

using gView.Server.AppCode;
using gView.Server.AppCode.Extensions;
using gView.Server.Extensions;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace gView.Server.Middleware.Authentication;

public class CookieAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly EncryptionCertificateService _encryptionCertService;

    public CookieAuthenticationMiddleware(
                RequestDelegate next,
                EncryptionCertificateService encryptionCertService
        )
    {
        _next = next;
        _encryptionCertService = encryptionCertService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.ApplyAuthenticationMiddleware())
        {
            string? cookie = context.Request.Cookies[Globals.AuthCookieName];
            if (!String.IsNullOrWhiteSpace(cookie))
            {
                AuthToken? authToken;
                try
                {
                    authToken = _encryptionCertService.FromToken(cookie);
                }
                catch (System.Security.Cryptography.CryptographicException)
                {
                    authToken = AuthToken.Anonymous;
                }

                if (authToken is not null)
                {
                    context.User = authToken.ToClaimsPrincipal();
                }
            }
        }

        await _next(context);
    }
}
