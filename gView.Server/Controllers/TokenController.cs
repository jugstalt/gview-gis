using gView.Server.AppCode;
using gView.Server.Models;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System;

namespace gView.Server.Controllers
{
    public class TokenController : BaseController
    {
        private readonly LoginManagerService _loginManagerService;

        public TokenController(
            LoginManagerService loginManagerService,
            EncryptionCertificateService encryptionCertificateService)
            : base(loginManagerService, encryptionCertificateService)
        {
            _loginManagerService = loginManagerService;
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

                var authToken = _loginManagerService.GetAuthToken(model.Username, model.Password);

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