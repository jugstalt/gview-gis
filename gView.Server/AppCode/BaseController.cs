using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    public class BaseController : Controller
    {
        #region Security

        const string AuthCookieName = "gview5-auth-token";

        protected AuthToken GetAuthToken()
        {
            #region From Token

            string token = this.Request.Query["token"];
            if(!String.IsNullOrEmpty(token))
            {
                return AuthToken.FromString(token);
            }

            #endregion

            #region From Cookie

            string cookie = this.Request.Cookies[AuthCookieName];
            if(!String.IsNullOrWhiteSpace(cookie))
            {
                return AuthToken.FromString(cookie);
            }

            #endregion

            return new AuthToken()
            {
                Username = String.Empty
            };
        }

        protected void SetAuthCookie(AuthToken authToken)
        {
            var cookieOptions = new CookieOptions()
            {
                Expires = null,
                Secure = true,
                HttpOnly = true
            };

            this.Response.Cookies.Append(AuthCookieName, authToken.ToString(), cookieOptions);
        }

        protected void RemoveAuthCookie()
        {
            this.Response.Cookies.Delete(AuthCookieName);
        }

        #endregion
    }
}
