using Microsoft.AspNetCore.Http;

namespace gView.Server.AppCode;

/// <summary>
/// The auth cookie of form logins (admin and token users).
/// </summary>
internal static class AuthCookie
{
    public static CookieOptions Options() => new CookieOptions()
    {
        Expires = null,  // session cookie: the token inside expires
        Secure = true,
        HttpOnly = true,
        SameSite = SameSiteMode.Lax
    };

    public static void Append(HttpResponse response, string token)
        => response.Cookies.Append(Globals.AuthCookieName, token, Options());

    public static void Remove(HttpResponse response)
        => response.Cookies.Delete(Globals.AuthCookieName);
}
