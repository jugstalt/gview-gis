using gView.Framework.Core.Exceptions;
using gView.Interoperability.GeoServices.Rest.DTOs;
using gView.Server.AppCode.Extensions;
using gView.Server.Extensions;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace gView.Server.Middleware.Authentication;

public class TokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly EncryptionCertificateService _encryptionCertService;

    public TokenAuthenticationMiddleware(
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
            string token = context.Request?.GetGeoservicesToken();

            if (!String.IsNullOrEmpty(token))
            {
                var authToken = _encryptionCertService.FromToken(token);

                context.User = authToken.ToClaimsPricipal();
            }
        }

        await _next(context);
    }
}
