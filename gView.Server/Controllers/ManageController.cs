using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using gView.Server.AppCode;
using gView.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace gView.Server.Controllers
{
    public class ManageController : BaseController
    {
        public IActionResult Index()
        {
            var authToken = base.GetAuthToken();
            if (authToken.IsAnonymous)
                return RedirectToAction("Login");

            return View();
        }

        #region Login/Security

        [HttpGet]
        public IActionResult Login()
        {
            return View(new ManageLoginModel());
        }
        

        #endregion
    }
}