#nullable enable

using gView.Framework.Core.Exceptions;
using gView.Server.AppCode;
using gView.Server.AppCode.Extensions;
using gView.Server.Extensions;
using gView.Server.Services.Security;
using gView.Server.Services.Security.TwoFactor;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace gView.Server.Middleware.Authentication;

/// <summary>
/// Login by the auth cookie of the form logins. The token in the cookie must be
/// unexpired and carry the current security stamp of its login (see
/// <see cref="LoginStamp"/>), otherwise the cookie is removed and the request
/// is anonymous. Admin logins are renewed while the admin is active
/// (security:manage:session-hours). With security:manage:two-factor = required,
/// admin sessions of users without a second factor are no longer accepted
/// (sessions from before the switch): they log in again and set it up.
/// </summary>
public class CookieAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly EncryptionCertificateService _encryptionCertService;
    private readonly SecurityOptions _securityOptions;

    public CookieAuthenticationMiddleware(
                RequestDelegate next,
                EncryptionCertificateService encryptionCertService,
                SecurityOptions securityOptions
        )
    {
        _next = next;
        _encryptionCertService = encryptionCertService;
        _securityOptions = securityOptions;
    }

    public async Task InvokeAsync(HttpContext context, LoginManager loginManager, TwoFactorService twoFactor)
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
                catch (Exception ex) when (ex is System.Security.Cryptography.CryptographicException or InvalidTokenException)
                {
                    authToken = null;
                }

                if (authToken is null || authToken.IsExpired || !loginManager.HasCurrentStamp(authToken)
                    || (authToken.IsManageUser && authToken.Stamp is not null && !twoFactor.IsSessionAllowed(authToken.Username)))
                {
                    AuthCookie.Remove(context.Response);
                }
                else
                {
                    authToken = Renew(context, authToken);
                    context.User = authToken.ToClaimsPrincipal();
                }
            }
        }

        await _next(context);
    }

    /// <summary>
    /// Sliding admin session: when less than half of the session is left, the cookie
    /// gets a new token (same user and stamp) that is valid for a full session again.
    /// </summary>
    private AuthToken Renew(HttpContext context, AuthToken authToken)
    {
        if (!authToken.IsManageUser)
        {
            return authToken;
        }

        var session = TimeSpan.FromHours(_securityOptions.ManageSessionHours);
        var left = TimeSpan.FromTicks(authToken.Expire - DateTime.UtcNow.Ticks);

        if (left > session / 2)
        {
            return authToken;
        }

        var renewed = new AuthToken(authToken.Username, authToken.AuthType, session) { Stamp = authToken.Stamp };
        AuthCookie.Append(context.Response, _encryptionCertService.ToToken(renewed));

        return renewed;
    }
}
