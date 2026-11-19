using gView.Framework.Core.Exceptions;
using gView.Framework.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;

namespace gView.Server.Services.Security;

/// <summary>
/// The admin (manage) logins, as far as <see cref="AdminSetupService"/> needs them
/// (implemented by <see cref="LoginManager"/>).
/// </summary>
public interface IManagerLoginStore
{
    bool HasManagerLogin();
    void CreateFirstManagerLogin(string username, string password);
}

/// <summary>
/// Creates the first admin (manage) login. A first login is never created just by
/// logging in. Instead:
/// <list type="bullet">
/// <item>the environment variables / config values <c>GVIEW_ADMIN_USER</c> and
/// <c>GVIEW_ADMIN_PASSWORD</c> create it at startup, or</item>
/// <item>the server writes a one-time setup code to the log, and the manage login
/// page asks for it (together with username and password) to create the admin.</item>
/// </list>
/// The setup code lives in this process only; a restart creates a new one.
/// </summary>
public class AdminSetupService
{
    public const string AdminUserKey = "GVIEW_ADMIN_USER";
    public const string AdminPasswordKey = "GVIEW_ADMIN_PASSWORD";

    private const string SetupCodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";  // no 0/O, 1/I
    private const int SetupCodeLength = 12;
    private const string SetupThrottleKey = "#setup";

    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminSetupService> _logger;
    private readonly LoginThrottleService _loginThrottle;
    private readonly object _lock = new();
    private string _setupCode;

    public AdminSetupService(IConfiguration configuration, LoginThrottleService loginThrottle, ILogger<AdminSetupService> logger)
    {
        _configuration = configuration;
        _loginThrottle = loginThrottle;
        _logger = logger;
    }

    /// <summary>
    /// Called at startup: creates the admin from the environment, or prints a setup code.
    /// </summary>
    public void Initialize(IManagerLoginStore loginManager)
    {
        lock (_lock)
        {
            if (loginManager.HasManagerLogin())
            {
                return;
            }

            var username = _configuration[AdminUserKey];
            var password = _configuration[AdminPasswordKey];

            if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
            {
                try
                {
                    loginManager.CreateFirstManagerLogin(username.Trim(), password.Trim());
                    _logger.LogWarning("Admin login {username} created from {key}", username.Trim(), AdminUserKey);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Admin login from {key} not created: {message}", AdminUserKey, ex.Message);
                }
            }

            EnsureSetupCode();
        }
    }

    /// <summary>
    /// True, if no admin exists yet: the login page must ask for the setup code.
    /// </summary>
    public bool IsSetupRequired(IManagerLoginStore loginManager)
    {
        lock (_lock)
        {
            if (loginManager.HasManagerLogin())
            {
                _setupCode = null;
                return false;
            }

            EnsureSetupCode();
            return true;
        }
    }

    /// <summary>
    /// Creates the first admin, if the setup code is correct.
    /// Throws a <see cref="MapServerException"/> with a message for the user otherwise.
    /// Wrong codes are throttled per client IP like failed logins.
    /// </summary>
    public void CreateFirstAdmin(IManagerLoginStore loginManager, string clientIp, string setupCode, string username, string password, string passwordRepeat)
    {
        lock (_lock)
        {
            if (loginManager.HasManagerLogin())
            {
                throw new MapServerException("An admin login already exists");
            }

            if (_loginThrottle.IsBlocked(clientIp, SetupThrottleKey))
            {
                throw new MapServerException("Invalid setup code");
            }

            if (_setupCode is null
                || !SecureCrypto.FixedTimeEquals(NormalizeCode(setupCode), _setupCode))
            {
                _loginThrottle.Failed(clientIp, SetupThrottleKey);
                _logger.LogWarning("Invalid admin setup code from {ip}", clientIp);

                throw new MapServerException("Invalid setup code");
            }

            if (password != passwordRepeat)
            {
                throw new MapServerException("Passwords do not match");
            }

            loginManager.CreateFirstManagerLogin(username, password);
            _setupCode = null;

            _logger.LogWarning("Admin login {username} created with the setup code", username);
        }
    }

    /// <summary>For tests: the current setup code (null if none).</summary>
    internal string CurrentSetupCode => _setupCode;

    #region Helper

    private void EnsureSetupCode()
    {
        if (_setupCode is not null)
        {
            return;
        }

        var chars = new char[SetupCodeLength];
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = SetupCodeChars[RandomNumberGenerator.GetInt32(SetupCodeChars.Length)];
        }
        _setupCode = new string(chars);

        _logger.LogWarning(
            "No admin login exists. Open /manage/login and use this setup code to create the admin: {setupCode} (or set {userKey} and {passwordKey})",
            $"{_setupCode[..4]}-{_setupCode[4..8]}-{_setupCode[8..]}", AdminUserKey, AdminPasswordKey);
    }

    private static string NormalizeCode(string code)
        => (code ?? String.Empty).Replace("-", "").Replace(" ", "").Trim().ToUpperInvariant();

    #endregion
}
