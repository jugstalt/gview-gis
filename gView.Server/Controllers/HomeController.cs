using gView.Server.AppCode;
using gView.Server.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;

namespace gView.Server.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            if (Globals.HasValidConfig == false)
            {
                return RedirectToAction("ConfigInvalid");
            }

            var user = Globals.ExternalAuthService != null ?
                Globals.ExternalAuthService.Perform(this.Request) :
                null;

            if (!String.IsNullOrWhiteSpace(user))
            {
                var loginManager = new LoginManager(Globals.LoginManagerRootPath);
                var authToken = loginManager.CreateUserAuthTokenWithoutPasswordCheck(user);
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
