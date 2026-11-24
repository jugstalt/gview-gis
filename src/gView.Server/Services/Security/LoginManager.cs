using gView.Framework.Core.Exceptions;
using gView.Framework.Security;
using gView.Framework.Security.Extensions;
using gView.Server.AppCode;
using gView.Server.AppCode.Extensions;
using gView.Server.Extensions;
using gView.Server.Services.MapServer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gView.Server.Services.Security
{
    public class LoginManager : IManagerLoginStore
    {
        private readonly MapServiceManager _mapServerService;
        private readonly MapServiceAccessService _mapServiceAccessService;
        private readonly EncryptionCertificateService _encryptionCertService;
        private readonly LoginThrottleService _loginThrottle;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _securityLog;

        public LoginManager(
                    MapServiceManager mapServerService,
                    MapServiceAccessService mapServiceAccessService,
                    EncryptionCertificateService encryptionCertService,
                    LoginThrottleService loginThrottle,
                    IHttpContextAccessor httpContextAccessor,
                    ILoggerFactory loggerFactory
            )
        {
            _mapServerService = mapServerService;
            _mapServiceAccessService = mapServiceAccessService;
            _encryptionCertService = encryptionCertService;
            _loginThrottle = loginThrottle;
            _httpContextAccessor = httpContextAccessor;
            _securityLog = loggerFactory.CreateLogger(SecurityLog.Category);
        }

        #region Manager Logins

        /// <param name="completesLogin">false, if a second factor follows: the failure counter
        /// of the user is only reset after the second factor (see <see cref="ManagerLoginSucceeded"/>)</param>
        public AuthToken GetManagerAuthToken(string username, string password, int exipreMinutes = 30, bool completesLogin = true)
        {
            var di = new DirectoryInfo(ManageFolder);
            username = username?.Trim().ToLower();  // admin logins are always created lowercase

            return Throttled(username, () => AuthToken.Create(di.FullName, username, password, AppCode.AuthToken.AuthTypes.Manage, exipreMinutes), completesLogin);
        }

        /// <summary>
        /// The folder of the admin logins (<c>&lt;user&gt;.lgn</c>, <c>.stamp</c>, <c>.2fa</c>).
        /// </summary>
        public string ManageFolder => Path.Combine(_mapServerService.Options.LoginManagerRootPath, "manage");

        /// <summary>
        /// Admin token after the second factor (password checked before), with the current stamp.
        /// </summary>
        public AuthToken CreateManagerAuthTokenAfterSecondFactor(string username, int expireMinutes)
            => CreateAuthTokenWithoutPasswordCheck(ManageFolder, username, AppCode.AuthToken.AuthTypes.Manage, expireMinutes);

        /// <summary>
        /// Is the admin login of the user (or the client IP) locked after too many failures?
        /// </summary>
        public bool IsManagerLoginBlocked(string username)
            => _loginThrottle.IsBlocked(ClientIp, username);

        /// <summary>
        /// A wrong second factor counts like a wrong password.
        /// </summary>
        public void ManagerLoginFailed(string username)
            => _loginThrottle.Failed(ClientIp, username);

        public void ManagerLoginSucceeded(string username)
            => _loginThrottle.Succeeded(username);

        /// <summary>
        /// The current security stamp of an admin login (null if the login does not exist).
        /// </summary>
        public string CurrentManagerStamp(string username)
            => LoginStamp.Current(ManageFolder, username);

        public string ClientIp => _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        /// <summary>
        /// Creates the first admin login. Only allowed while no admin exists
        /// (see <see cref="AdminSetupService"/>).
        /// </summary>
        public void CreateFirstManagerLogin(string username, string password)
        {
            if (HasManagerLogin())
            {
                throw new MapServerException("An admin login already exists");
            }

            var di = new DirectoryInfo(_mapServerService.Options.LoginManagerRootPath + "/manage");
            di.Create();

            CreateLogin(di.FullName, username?.Trim().ToLower(), password);
            _encryptionCertService.GetCertificate("crypto0");  // Create the Service if not exits
        }

        public bool HasManagerLogin()
        {
            var di = new DirectoryInfo(_mapServerService.Options.LoginManagerRootPath + "/manage");
            return di.Exists && di.GetFiles("*.lgn").Length > 0;
        }

        public IEnumerable<string> GetMangeUserNames()
        {
            if(!String.IsNullOrEmpty(_mapServiceAccessService.AdminAlias))
            {
                return [_mapServiceAccessService.AdminAlias];
            }

            var di = new DirectoryInfo(_mapServerService.Options.LoginManagerRootPath + "/manage");

            if (di.Exists)
            {
                return di.GetFiles("*.lgn").Select(f => f.Name.Substring(0, f.Name.Length - f.Extension.Length));
            }

            return new string[0];
        }

        #endregion

        #region Token Logins

        /// <summary>
        /// Listed instead of the secret part of a hashed url token: "token-name~(hidden)".
        /// </summary>
        public const string HiddenUrlTokenSuffix = "~(hidden)";

        public void CreateTokenLogin(string username, string password)
        {
            if (GetManageAndTokenUsernames().Where(u => username.Equals(u, StringComparison.InvariantCultureIgnoreCase)).Count() > 0)
            {
                throw new MapServerException("User '" + username + "' already exists");
            }

            var fi = TokenLoginFile(username);

            if (fi.Exists)
            {
                throw new MapServerException($"User '{username}' already exists");
            }

            fi.Directory.Create();
            CreateLogin(fi.Directory.FullName, username, password);
        }

        public void ChangeTokenUserPassword(string username, string newPassword)
        {
            newPassword.ValidatePassword();

            var fi = TokenLoginFile(username);

            if (!fi.Exists)
            {
                throw new MapServerException($"User '{username}' does not exists");
            }

            var hashedPassword = username.UserNameIsUrlToken()
                ? SecureCrypto.HashSecret(newPassword)  // url tokens: long random secrets
                : SecureCrypto.HashPassword(newPassword);

            File.WriteAllText(fi.FullName, hashedPassword);
            LoginStamp.Invalidate(fi);  // ends all sessions and tokens of the user
        }

        public void DeleteTokenLogin(string username)
        {
            var fi = TokenLoginFile(username);

            if (!fi.Exists)
            {
                throw new MapServerException($"User '{username}' does not exists");
            }

            fi.Delete();
            LoginStamp.Delete(fi);
        }

        public IEnumerable<string> GetTokenUsernames(bool fullUrlToken = true)
        {

            var di = new DirectoryInfo(_mapServerService.Options.LoginManagerRootPath + "/token");
            if (di.Exists)
            {
                return di.GetFiles("*.lgn")
                         .Select(f =>
                         {
                             var username = f.Name.Substring(0, f.Name.Length - f.Extension.Length);
                             if (!fullUrlToken || !username.UserNameIsUrlToken())
                             {
                                 return username;
                             }

                             // hashed url tokens are shown only once, when they are created
                             var stored = File.ReadAllText(f.FullName);
                             return stored.StartsWith(SecureCrypto.Sha256Prefix, StringComparison.Ordinal)
                                 ? $"{username}{HiddenUrlTokenSuffix}"
                                 : stored;
                         });
            }

            return new string[0];
        }

        public AuthToken GetAuthToken(string username, string password, int expireMinutes = 30)
        {
            var di = new DirectoryInfo(_mapServerService.Options.LoginManagerRootPath + "/token");
            return Throttled(username, () => AuthToken.Create(di.FullName, username, password, AppCode.AuthToken.AuthTypes.Tokenuser, expireMinutes));
        }

        public AuthToken CreateUserAuthTokenWithoutPasswordCheck(string username, int expireMinutes = 30)
        {
            var di = new DirectoryInfo(_mapServerService.Options.LoginManagerRootPath + "/token");
            return CreateAuthTokenWithoutPasswordCheck(di.FullName, username, AppCode.AuthToken.AuthTypes.Tokenuser, expireMinutes);
        }

        #endregion

        public IEnumerable<string> GetManageAndTokenUsernames()
        {
            List<string> names = new List<string>();

            names.AddRange(GetMangeUserNames());
            names.AddRange(GetTokenUsernames(false));

            return names;
        }

        #region Security Stamp

        /// <summary>
        /// Is the token still valid for its login: was it issued with the current
        /// security stamp (no password change, no "sign out everywhere" since)?
        /// Tokens without a login file behind them (OIDC) are not checked here.
        /// </summary>
        public bool HasCurrentStamp(AuthToken authToken)
        {
            if (authToken is null || authToken.IsAnonymous)
            {
                return true;
            }

            var current = LoginStamp.Current(LoginFolder(authToken.AuthType), authToken.Username);

            return current is not null && SecureCrypto.FixedTimeEquals(current, authToken.Stamp);
        }

        /// <summary>
        /// Ends every session and token of the user of this token.
        /// </summary>
        public void SignOutEverywhere(AuthToken authToken)
        {
            if (authToken is null || authToken.IsAnonymous)
            {
                return;
            }

            LoginStamp.Renew(LoginFolder(authToken.AuthType), authToken.Username);
            _securityLog.LogWarning("All sessions and tokens of {username} ended (sign out everywhere) from {ip}",
                authToken.Username, _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString());
        }

        private string LoginFolder(AuthToken.AuthTypes authType) => authType switch
        {
            AuthToken.AuthTypes.Manage => Path.Combine(_mapServerService.Options.LoginManagerRootPath, "manage"),
            AuthToken.AuthTypes.Tokenuser => Path.Combine(_mapServerService.Options.LoginManagerRootPath, "token"),
            _ => null
        };

        #endregion

        #region Request

        public string LoginUsername(HttpRequest request)
        {
            try
            {
                return LoginAuthToken(request).Username;
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        public bool IsManageUser(HttpRequest request)
        {
            try
            {
                var loginAuthToken = LoginAuthToken(request);
                return loginAuthToken != null &&
                       loginAuthToken.AuthType == AuthToken.AuthTypes.Manage;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public AuthToken LoginAuthToken(HttpRequest request)
        {
            AuthToken authToken = null;

            try
            {
                authToken = request.HttpContext.User.ToAuthToken();

                return authToken;
            }
            finally
            {
                if (authToken == null || authToken.IsExpired)
                {
                    throw new InvalidTokenException();
                }
            }
        }

        public AuthToken GetAuthToken(HttpRequest request)
        {
            return LoginAuthToken(request);
        }

        #endregion

        #region Helper

        /// <summary>
        /// Checks a password, unless the user or the client IP is locked after too many
        /// failures. A locked login fails like a wrong password.
        /// </summary>
        private AuthToken Throttled(string username, Func<AuthToken> login, bool completesLogin = true)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            var ip = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            if (_loginThrottle.IsBlocked(ip, username))
            {
                _securityLog.LogWarning("Login refused for user {username} from {ip}: locked after too many failed logins", username, ip);
                return null;
            }

            var authToken = login();

            if (authToken is null)
            {
                _securityLog.LogWarning("Failed login for user {username} from {ip}", username, ip);
                _loginThrottle.Failed(ip, username);
            }
            else if (completesLogin)
            {
                _loginThrottle.Succeeded(username);
            }

            return authToken;
        }

        private FileInfo TokenLoginFile(string username)
            => username.ToLoginFile(Path.Combine(_mapServerService.Options.LoginManagerRootPath, "token"))
               ?? throw new MapServerException("Invalid username");

        private AuthToken CreateAuthTokenWithoutPasswordCheck(string path, string username, AuthToken.AuthTypes authType, int expireMiniutes = 30)
        {
            var fi = username.ToLoginFile(path);

            if (fi?.Exists == true)
            {
                expireMiniutes = expireMiniutes <= 0 ? 30 : expireMiniutes;

                return new AuthToken(username, authType, new TimeSpan(0, expireMiniutes, 0)) { Stamp = LoginStamp.Compute(fi) };
            }

            return null;
        }

        private void CreateLogin(string path, string username, string password)
        {
            username.ValidateUsername();
            password.ValidatePassword();

            var hashedPassword = username.UserNameIsUrlToken()
                ? SecureCrypto.HashSecret(password)  // url tokens: long random secrets
                : SecureCrypto.HashPassword(password);

            File.WriteAllText(Path.Combine(path, $"{username}.lgn"), hashedPassword);
        }

        #endregion
    }
}
