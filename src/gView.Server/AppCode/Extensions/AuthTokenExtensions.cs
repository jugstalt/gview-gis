using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace gView.Server.AppCode.Extensions;

static public class AuthTokenExtensions
{
    private const string StopAuthenticationPropagationClaimType = "stop_auth_propagation";
    private const string AuthTypeClaimType = "auth_type";
    private const string ExpiresClaimType = "expires_in";
    private const string StampClaimType = "security_stamp";

    static public ClaimsPrincipal ToClaimsPrincipal(
                this AuthToken authToken,
                bool stopAuthenicationMiddlewarePropagation = true
            )
    {
        List<Claim> claims = new List<Claim>();

        // stable over token renewals (expires_in changes): antiforgery tokens are bound to it
        claims.Add(new Claim(ClaimTypes.NameIdentifier, $"{authToken.AuthType}:{authToken.Username}"));
        claims.Add(new Claim(AuthTypeClaimType, authToken.AuthType.ToString()));
        claims.Add(new Claim(ExpiresClaimType, authToken.Expire.ToString()));
        if (!String.IsNullOrEmpty(authToken.Stamp))
        {
            claims.Add(new Claim(StampClaimType, authToken.Stamp));
        }

        if (stopAuthenicationMiddlewarePropagation)
        {
            claims.Add(new Claim(StopAuthenticationPropagationClaimType, "1"));
        }

        var claimsIdentity = new ClaimsIdentity(new Identity(authToken), claims);
        var claimsPricipal = new ClaimsPrincipal(claimsIdentity);

        return claimsPricipal;
    }

    static public AuthToken ToAuthToken(this ClaimsPrincipal claimsPrincipal)
    {
        if (claimsPrincipal?.Identity?.IsAuthenticated == false
            || String.IsNullOrEmpty(claimsPrincipal?.Identity?.Name)
            )
        {
            return AuthToken.Anonymous;
        }

        try
        {
            return new AuthToken(
                claimsPrincipal.Identity.Name,
                Enum.Parse<AuthToken.AuthTypes>(claimsPrincipal.Claims.First(c => c.Type == AuthTypeClaimType).Value),
                long.Parse(claimsPrincipal.Claims.First(c => c.Type == ExpiresClaimType).Value)
            )
            {
                Stamp = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == StampClaimType)?.Value
            };
        }
        catch
        {
            return AuthToken.Anonymous;
        }
    }

    static public bool ApplyAuthenticationMiddleware(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal == null ||
               !(claimsPrincipal.Identity.IsAuthenticated == true && claimsPrincipal.StopAuthenticationPropagation());
    }

    /// <summary>
    /// The test map of a service: admins only for now (manage user from the admin network).
    /// The one place to change when it is opened to everyone with access to the service.
    /// </summary>
    static public bool CanUseTestMap(this AuthToken authToken, bool isFromAdminNetwork)
        => isFromAdminNetwork && authToken?.IsManageUser == true;

    static public bool StopAuthenticationPropagation(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal?
                    .Claims?
                    .Where(c => c.Type == StopAuthenticationPropagationClaimType)
                    .FirstOrDefault()?
                    .Value == "1";
    }

    #region Helper Class

    private class Identity : IIdentity
    {
        public Identity(AuthToken authToken)
        {
            this.Name = authToken?.Username;
            _isAuthenticated = authToken != null && !authToken.IsAnonymous;

            if (_isAuthenticated)
            {
                _authenticationType = "AuthenticationTypes.Federation";
            }
        }

        private readonly string _authenticationType;
        public string AuthenticationType => _authenticationType;

        private readonly bool _isAuthenticated = false;
        public bool IsAuthenticated => _isAuthenticated;

        public string Name { get; }
    }

    #endregion
}
