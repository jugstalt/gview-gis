using gView.Server.AppCode;
using gView.Server.Models;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System;

namespace gView.Server.Controllers
{
    public class TokenController : BaseController
    {
        private readonly LoginManager _loginMananger;

        public TokenController(
            MapServiceManager mapServiceMananger,
            LoginManager loginManager,
            EncryptionCertificateService encryptionCertificateService)
            : base(mapServiceMananger, loginManager, encryptionCertificateService)
        {
            _loginMananger = loginManager;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (Globals.AllowFormsLogin == false)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new TokenLoginModel());
        }

        [HttpPost]
        public IActionResult Login(TokenLoginModel model)
        {
            try
            {
                if (Globals.AllowFormsLogin == false)
                {
                    return RedirectToAction("Index", "Home");
                }

                var authToken = _loginMananger.GetAuthToken(model.Username, model.Password);

                if (authToken == null)
                {
                    throw new Exception("Unknown user or password");
                }

                base.SetAuthCookie(authToken);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            base.RemoveAuthCookie();

            return RedirectToAction("Index", "Home");
        }
    }
}