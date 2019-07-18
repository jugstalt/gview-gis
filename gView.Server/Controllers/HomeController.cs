using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using gView.Server.Models;
using gView.Server.AppCode;
using gView.Framework.system;
using gView.MapServer;

namespace gView.Server.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            if(!String.IsNullOrWhiteSpace(Request.Query["user"]) && !String.IsNullOrWhiteSpace(Request.Query["portal_token"]))
            {
                string username = Request.Query["user"];
                string password = "";

                var loginManager = new LoginManager(Globals.LoginManagerRootPath);
                var authToken = loginManager.GetAuthToken(username, password);

                if(authToken!=null)
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
    }
}
