using gView.Framework.Core.Exceptions;
using gView.Server.AppCode;
using gView.Server.Models.Manage;
using gView.Server.Services.Security;
using gView.Server.Services.Security.TwoFactor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace gView.Server.Controllers;

/// <summary>
/// Two-factor login (TOTP) of the admin form login, see <see cref="TwoFactorService"/>.
/// Password → pending cookie (no rights) → code (or setup in required mode) → auth cookie.
/// </summary>
public partial class ManageController
{
    #region Login: second step

    [HttpGet]
    public IActionResult LoginCode()
    {
        var pending = ReadPending();
        if (pending is null || pending.Step != TwoFactorStep.Code)
        {
            return RedirectToAction("Login");
        }

        return View(new ManageLoginCodeModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult LoginCode(ManageLoginCodeModel model)
    {
        var pending = ReadPending();
        if (pending is null || pending.Step != TwoFactorStep.Code)
        {
            return LoginAgain("Your login has expired. Please log in again.");
        }

        var username = pending.Username;

        if (_loginManager.IsManagerLoginBlocked(username))
        {
            RemovePendingCookie();
            return LoginAgain("Too many failed logins. Please try again later.");
        }

        var result = _twoFactor.Verify(username, model.Code, _loginManager.ClientIp);

        if (!result.IsValid)
        {
            _loginManager.ManagerLoginFailed(username);

            if (_twoFactor.PendingFailed(pending))
            {
                RemovePendingCookie();
                return LoginAgain("Too many wrong codes. Please log in again.");
            }

            return View(new ManageLoginCodeModel()
            {
                ErrorMessage = result.Status == TwoFactorVerifyStatus.Replay
                    ? "This code was already used. Wait for the next code."
                    : "Wrong code"
            });
        }

        _twoFactor.PendingSucceeded(pending);
        _loginManager.ManagerLoginSucceeded(username);
        RemovePendingCookie();

        var authToken = _loginManager.CreateManagerAuthTokenAfterSecondFactor(username, ManageSessionMinutes);
        if (authToken is null)
        {
            return LoginAgain("Your login has expired. Please log in again.");
        }

        base.SetAuthCookie(authToken);

        // few recovery codes left: show the two-factor page with the hint
        return result.Status == TwoFactorVerifyStatus.ValidRecoveryCode && result.RemainingRecoveryCodes <= RecoveryCodes.LowWarning
            ? RedirectToAction("TwoFactor")
            : RedirectToAction("Index");
    }

    #endregion

    #region Two-factor page

    [HttpGet]
    public IActionResult TwoFactor()
    {
        var user = TwoFactorUser();
        if (user is null)
        {
            return RedirectToAction("Login");
        }
        if (_twoFactor.Mode == TwoFactorMode.Off)
        {
            return RedirectToAction("Index");
        }

        return View(TwoFactorModel(user.Value.username, user.Value.pendingSetup, withSetup: true));
    }

    /// <summary>
    /// Activates (or replaces) the second factor with the code of the new authenticator.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TwoFactorConfirm(ManageTwoFactorModel input)
    {
        var user = TwoFactorUser();
        if (user is null || _twoFactor.Mode == TwoFactorMode.Off)
        {
            return RedirectToAction("Login");
        }

        var (username, pendingSetup) = user.Value;

        try
        {
            var recoveryCodes = _twoFactor.ConfirmSetup(username, input.SetupToken, input.Code, _loginManager.ClientIp);

            if (pendingSetup)
            {
                var pending = ReadPending();
                if (pending is not null)
                {
                    _twoFactor.PendingSucceeded(pending);
                }
                _loginManager.ManagerLoginSucceeded(username);
                RemovePendingCookie();
            }

            // the stamp is new: the cookie of this session too
            var authToken = _loginManager.CreateManagerAuthTokenAfterSecondFactor(username, ManageSessionMinutes);
            if (authToken is not null)
            {
                base.SetAuthCookie(authToken);
            }

            var model = TwoFactorModel(username, pendingSetup: false, withSetup: false);
            model.RecoveryCodes = recoveryCodes;
            model.SuccessMessage = "Two-factor login is active. All other sessions of this admin have been ended.";

            return View("TwoFactor", model);
        }
        catch (MapServerException ex)
        {
            var model = TwoFactorModel(username, pendingSetup, withSetup: false);
            model.Setup = _twoFactor.BeginSetup(username, Request.Host.Host, input.SetupToken);
            model.ErrorMessage = ex.Message;

            return View("TwoFactor", model);
        }
    }

    /// <summary>
    /// New device: after a current code (or recovery code) the page shows a new QR code.
    /// The old authenticator stays valid until the new one is confirmed.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TwoFactorReplace(ManageTwoFactorModel input)
        => TwoFactorAction(input, requiresCode: true, allowRecoveryCode: true, (username, model) =>
        {
            model.Setup = _twoFactor.BeginSetup(username, Request.Host.Host);
        });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TwoFactorRecoveryCodes(ManageTwoFactorModel input)
        => TwoFactorAction(input, requiresCode: false, allowRecoveryCode: false, (username, model) =>
        {
            model.RecoveryCodes = _twoFactor.RegenerateRecoveryCodes(username, input.Code, _loginManager.ClientIp);
            model.RemainingRecoveryCodes = model.RecoveryCodes.Length;
            model.SuccessMessage = "New recovery codes created. The old ones are no longer valid.";
        });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TwoFactorDisable(ManageTwoFactorModel input)
        => TwoFactorAction(input, requiresCode: false, allowRecoveryCode: false, (username, model) =>
        {
            if (_twoFactor.Mode != TwoFactorMode.Optional)
            {
                throw new MapServerException("Two-factor login is required on this server");
            }
            if (String.IsNullOrEmpty(input.Password)
                || _loginManager.GetManagerAuthToken(username, input.Password.Trim(), completesLogin: false) is null)
            {
                throw new MapServerException("Wrong password");
            }

            _twoFactor.RequireCode(username, input.Code, _loginManager.ClientIp, allowRecoveryCode: true);
            _twoFactor.Disable(username, _loginManager.ClientIp);

            var authToken = _loginManager.CreateManagerAuthTokenAfterSecondFactor(username, ManageSessionMinutes);
            if (authToken is not null)
            {
                base.SetAuthCookie(authToken);
            }

            model.Enabled = false;
            model.RemainingRecoveryCodes = 0;
            model.SuccessMessage = "Two-factor login is disabled. All other sessions of this admin have been ended.";
        });

    #endregion

    #region Helper

    private int ManageSessionMinutes
        => 60 * HttpContext.RequestServices.GetRequiredService<SecurityOptions>().ManageSessionHours;

    /// <summary>
    /// An action of a logged in admin with a second factor.
    /// </summary>
    private IActionResult TwoFactorAction(ManageTwoFactorModel input, bool requiresCode, bool allowRecoveryCode,
                                          Action<string, ManageTwoFactorModel> action)
    {
        var user = TwoFactorUser();
        if (user is null || user.Value.pendingSetup || _twoFactor.Mode == TwoFactorMode.Off)
        {
            return RedirectToAction("Login");
        }

        var username = user.Value.username;
        var model = TwoFactorModel(username, pendingSetup: false, withSetup: false);

        if (!model.Enabled)
        {
            return RedirectToAction("TwoFactor");
        }

        try
        {
            if (requiresCode)
            {
                _twoFactor.RequireCode(username, input.Code, _loginManager.ClientIp, allowRecoveryCode);
            }

            action(username, model);
        }
        catch (MapServerException ex)
        {
            _loginManager.ManagerLoginFailed(username);  // wrong codes/passwords count like failed logins
            model.ErrorMessage = ex.Message;
        }

        return View("TwoFactor", model);
    }

    private ManageTwoFactorModel TwoFactorModel(string username, bool pendingSetup, bool withSetup)
    {
        bool enabled = _twoFactor.IsEnabled(username);

        return new ManageTwoFactorModel()
        {
            Username = username,
            Mode = _twoFactor.Mode,
            IsPendingSetup = pendingSetup,
            Enabled = enabled,
            RemainingRecoveryCodes = enabled ? _twoFactor.RemainingRecoveryCodes(username) : 0,
            Setup = withSetup && !enabled ? _twoFactor.BeginSetup(username, Request.Host.Host) : null
        };
    }

    /// <summary>
    /// The admin of the two-factor page: logged in with the form login (not OIDC),
    /// or right after the password, when the setup is required.
    /// </summary>
    private (string username, bool pendingSetup)? TwoFactorUser()
    {
        try
        {
            var authToken = _loginManager.GetAuthToken(this.Request);
            if (authToken.IsManageUser && authToken.Stamp is not null)
            {
                return (authToken.Username, false);
            }
        }
        catch (InvalidTokenException) { }

        var pending = ReadPending();
        if (pending is not null && pending.Step == TwoFactorStep.Setup)
        {
            return (pending.Username, true);
        }

        return null;
    }

    /// <summary>
    /// The pending login of this browser, if it is valid and the stamp of the login
    /// did not change since the password (password change, sign out everywhere, reset).
    /// </summary>
    private TwoFactorPending ReadPending()
    {
        var pending = _twoFactor.ReadPendingToken(Request.Cookies[TwoFactorKeys.PendingCookieName]);
        if (pending is null)
        {
            return null;
        }

        var stamp = _loginManager.CurrentManagerStamp(pending.Username);

        return stamp is not null && Framework.Security.SecureCrypto.FixedTimeEquals(stamp, pending.Stamp)
            ? pending
            : null;
    }

    private void SetPendingCookie(string token)
        => Response.Cookies.Append(TwoFactorKeys.PendingCookieName, token, new CookieOptions()
        {
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = TwoFactorService.PendingLifetime
        });

    private void RemovePendingCookie()
    {
        if (Request.Cookies.ContainsKey(TwoFactorKeys.PendingCookieName))
        {
            Response.Cookies.Delete(TwoFactorKeys.PendingCookieName);
        }
    }

    private IActionResult LoginAgain(string message)
        => View("Login", new ManageLoginModel()
        {
            ErrorMessage = message,
            SetupRequired = _adminSetup.IsSetupRequired(_loginManager)
        });

    #endregion
}
