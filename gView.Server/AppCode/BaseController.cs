using gView.Framework.system;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            this.ActionStartTime = DateTime.UtcNow;

            base.OnActionExecuting(context);
        }

        protected DateTime? ActionStartTime = null;

        #region Security

        const string AuthCookieName = "gview5-auth-token";

        protected AuthToken GetAuthToken()
        {
            AuthToken authToken = null;

            try
            {
                #region From Token

                string token = this.Request.Query["token"];
                if (!String.IsNullOrEmpty(token))
                {
                    return authToken = AuthToken.FromString(token);
                }

                #endregion

                #region From Cookie

                string cookie = this.Request.Cookies[AuthCookieName];
                if (!String.IsNullOrWhiteSpace(cookie))
                {
                    return authToken = AuthToken.FromString(cookie);
                }

                #endregion

                return authToken = new AuthToken()
                {
                    Username = String.Empty
                };
            }
            finally
            {
                if (authToken.IsExpired)
                    throw new InvalidTokenException();
            }
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
