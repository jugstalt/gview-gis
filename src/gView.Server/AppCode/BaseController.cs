using gView.Framework.Common;
using gView.Server.Extensions;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    public class BaseController : Controller
    {
        private readonly MapServiceManager _mapServerService;
        private readonly LoginManager _loginManagerService;
        private readonly EncryptionCertificateService _encryptionCertificate;

        public BaseController(
            MapServiceManager mapServerService,
            LoginManager loginManagerService,
            EncryptionCertificateService encryptionCertificate)
        {
            _mapServerService = mapServerService;
            _loginManagerService = loginManagerService;
            _encryptionCertificate = encryptionCertificate;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            this.ActionStartTime = DateTime.UtcNow;

            base.OnActionExecuting(context);
        }

        protected DateTime? ActionStartTime = null;

        #region Security

        protected void SetAuthCookie(AuthToken authToken)
            => AuthCookie.Append(this.Response, _encryptionCertificate.ToToken(authToken));

        protected void RemoveAuthCookie()
            => AuthCookie.Remove(this.Response);

        #endregion

        #region ETAG

        public IActionResult NotModified()
        {
            Response.StatusCode = 304;
            return Content(String.Empty);
        }

        protected bool HasIfNonMatch
        {
            get
            {
                return (string)this.Request.Headers["If-None-Match"] != null;
            }
        }

        public bool IfMatch()
        {
            try
            {
                if (HasIfNonMatch == false)
                {
                    return false;
                }

                var etag = long.Parse(this.Request.Headers["If-None-Match"]);

                DateTime etagTime = new DateTime(etag, DateTimeKind.Utc);
                if (DateTime.UtcNow > etagTime)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void AppendEtag(DateTime expires)
        {
            this.Response.Headers.Append("ETag", expires.Ticks.ToString());
            this.Response.Headers.Append("Last-Modified", DateTime.UtcNow.ToString("R"));
            this.Response.Headers.Append("Expires", expires.ToString("R"));
            this.Response.Headers.Append("Cache-Control", "private, max-age=" + (int)(new TimeSpan(24, 0, 0)).TotalSeconds);
        }

        #endregion

        async virtual protected Task<IActionResult> SecureMethodHandler(Func<Identity, Task<IActionResult>> func,
                                                                        Func<Exception, IActionResult> onException = null)
        {
            if (_mapServerService.Options.IsValid == false)
            {
                return RedirectToAction("ConfigInvalid", "Home");
            }
            try
            {
                var authToken = _loginManagerService.GetAuthToken(this.Request);
                var identity = new Identity(authToken.Username, authToken.IsManageUser);

                return await func(identity);
            }
            catch (Exception ex)
            {
                if (onException != null)
                {
                    return onException(ex);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
