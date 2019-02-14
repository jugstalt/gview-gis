using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using gView.Interoperability.GeoServices.Rest.Json;
using gView.Server.AppCode;
using gView.Server.Models;
using Microsoft.AspNetCore.Mvc;
using static gView.Interoperability.GeoServices.Rest.Json.JsonServices;

namespace gView.Server.Controllers
{
    public class ManageController : BaseController
    {
        public IActionResult Index()
        {
            var authToken = base.GetAuthToken();
            if (authToken.IsAnonymous)
                return RedirectToAction("Login");

            return View(new JsonServices()
            {
                CurrentVersion=GeoServicesRestController.Version,
                Folders = InternetMapServer.mapServices
                    .Where(s => !String.IsNullOrWhiteSpace(s.Folder))
                    .Select(s => s.Folder).Distinct()
                    .ToArray(),
                Services = InternetMapServer.mapServices
                    .Where(s => String.IsNullOrWhiteSpace(s.Folder))
                    .Select(s => new AgsServices()
                    {
                        Name = s.Name,
                        Type = "MapServer"
                    })
                    .ToArray()
            });
        }

        #region Login/Security

        [HttpGet]
        public IActionResult Login()
        {
            return View(new ManageLoginModel());
        }
        
        [HttpPost]
        public IActionResult Login(ManageLoginModel model)
        {
            try
            {
                var loginManager = new LoginManager(Globals.LoginManagerRootPath);
                var authToken = loginManager.GetManagerAuthToken(model.Username, model.Password, createIfFirst: true);

                if (authToken == null)
                    throw new Exception("Unknown user or password");

                base.SetAuthCookie(authToken);

                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return View(model);
            }
        }

        #endregion
    }
}