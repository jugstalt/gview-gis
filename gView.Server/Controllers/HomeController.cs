using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using gView.Server.Models;
using gView.Server.AppCode;
using System.Linq;
using gView.Framework.system;
using gView.MapServer;

namespace gView.Server.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(new HomeIndexModel()
            {
                Services = InternetMapServer.Instance.Maps
            });
        }

        public IActionResult Service(string id)
        {
 
            return View(new HomeServiceModel()
            {
                Server = InternetMapServer.AppRootUrl(this.Request), 
                OnlineResource= Request.Scheme + "://" + Request.Host+"/ogc?",
                ServiceMap = InternetMapServer.Instance[id],
                Interpreters = InternetMapServer.Interpreters.Select(i => new PlugInManager().CreateInstance<IServiceRequestInterpreter>(i))
            });
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
