using Microsoft.IdentityModel.Tokens;
using System;

namespace gView.Server.Extensions
{
    static class AuthorizationExtension
    {
        static public (string username, string password) FromAuthorizationHeader(this string authorizationHeader)
        {
            if (!String.IsNullOrEmpty(authorizationHeader))
            {
                if (authorizationHeader.StartsWith("Basic "))
                {
                    var auth = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(authorizationHeader.Substring(6)));

                    return (username: auth.Substring(0, auth.IndexOf(":")),
                            password: auth.Substring(auth.IndexOf(":") + 1));

                }
            }

            return (String.Empty, String.Empty);
        }
    }
}
