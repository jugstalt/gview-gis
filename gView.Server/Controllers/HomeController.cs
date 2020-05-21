using gView.Server.AppCode;
using gView.Server.Models;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;

namespace gView.Server.Controllers
{
    public class HomeController : BaseController
    {
        private readonly InternetMapServerService _mapServerService;
        private readonly LoginManagerService _loginManagerService;

        public HomeController(
            InternetMapServerService mapServerService, 
            LoginManagerService loginManagerService,
            EncryptionCertificateService encryptionCertificateService)
            : base(loginManagerService, encryptionCertificateService)
        {
            _mapServerService = mapServerService;
            _loginManagerService = loginManagerService;
        }

        public IActionResult Index()
        {
            if (_mapServerService.Options.IsValid == false)
            {
                return RedirectToAction("ConfigInvalid");
            }

            var user = Globals.ExternalAuthService != null ?
                Globals.ExternalAuthService.Perform(this.Request) :
                null;

            if (!String.IsNullOrWhiteSpace(user))
            {
                var authToken = _loginManagerService.CreateUserAuthTokenWithoutPasswordCheck(user);
                if (authToken != null)
                {
                    base.SetAuthCookie(authToken);
                }

                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult ConfigInvalid()
        {
            return View();
        }
    }
}
