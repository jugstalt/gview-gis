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

            return View();
        }

        #region Login

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

        #region Security

        public IActionResult TokenUsers()
        {
            var authToken = base.GetAuthToken();
            if (authToken.IsAnonymous)
                return RedirectToAction("Login");

            var loginManager = new LoginManager(Globals.LoginManagerRootPath);
            return Json(new { users = loginManager.GetTokenUsernames() });
        }

        [HttpPost]
        public IActionResult CreateTokenUser(CreateTokenUserModel model)
        {
            try
            {
                var authToken = base.GetAuthToken();
                if (authToken.IsAnonymous)
                    throw new Exception("Not authorized");

                model.Username = model.Username?.Trim() ?? String.Empty;
                model.Password = model.Password?.Trim() ?? String.Empty;

                if (model.Username.Length < 5)
                    throw new Exception("Username: min length 5 chars!");

                if (model.Password.Length < 8)
                    throw new Exception("Password: min length 8 chars");

                var loginManager = new LoginManager(Globals.LoginManagerRootPath);
                loginManager.CreateTokenLogin(model.Username, model.Password);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, success = false });
            }
        }

        [HttpPost]
        public IActionResult ChangeTokenUserPassword(ChangeTokenUserPasswordModel model)
        {
            try
            {
                var authToken = base.GetAuthToken();
                if (authToken.IsAnonymous)
                    throw new Exception("Not authorized");

                model.Username = model.Username?.Trim() ?? String.Empty;
                model.NewPassword = model.NewPassword?.Trim() ?? String.Empty;

                if (model.NewPassword.Length < 8)
                    throw new Exception("Password: min length 8 chars");

                var loginManager = new LoginManager(Globals.LoginManagerRootPath);
                loginManager.ChangeTokenUserPassword(model.Username, model.NewPassword);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, success = false });
            }
        }

        #endregion
    }
}