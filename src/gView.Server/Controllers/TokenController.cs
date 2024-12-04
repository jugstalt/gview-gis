using gView.Server.AppCode;
using gView.Server.Models;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace gView.Server.Controllers;

public class TokenController : BaseController
{
    private readonly LoginManager _loginMananger;
    private readonly IConfiguration _configuration;

    public TokenController(
        MapServiceManager mapServiceMananger,
        LoginManager loginManager,
        IConfiguration configuration,
        EncryptionCertificateService encryptionCertificateService)
        : base(mapServiceMananger, loginManager, encryptionCertificateService)
    {
        _loginMananger = loginManager;
        _configuration = configuration;
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
    [ValidateAntiForgeryToken]
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

    async public Task<IActionResult> Logout()
    {
        var authConfig = new AuthConfigModel();
        _configuration.Bind("Authentication", authConfig);

        if ("oidc".Equals(authConfig.Type, StringComparison.OrdinalIgnoreCase))
        {
            await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        base.RemoveAuthCookie();

        return RedirectToAction("Index", "Home");
    }
}