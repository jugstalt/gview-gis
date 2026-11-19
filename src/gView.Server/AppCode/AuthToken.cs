using gView.Framework.Security;
using gView.Server.Extensions;
using System;
using System.IO;

namespace gView.Server.AppCode
{
    public class AuthToken
    {
        public AuthToken()
        {
            this.Username = string.Empty;
            this.AuthType = AuthTypes.Unknown;
        }

        public AuthToken(string username, AuthTypes authType, TimeSpan expiresIn)
        {
            this.Username = username;
            this.AuthType = authType;
            this.Expire = DateTime.UtcNow.Ticks + expiresIn.Ticks;
        }

        public AuthToken(string username, AuthTypes authType, long expiresTicksUtc)
        {
            this.Username = username;
            this.AuthType = authType;
            this.Expire = expiresTicksUtc;
        }

        public string Username { get; }
        public long Expire { get; }
        public AuthTypes AuthType { get; }

        /// <summary>
        /// The security stamp of the login at login time (see <see cref="LoginStamp"/>).
        /// Null for tokens that are not issued from a login file (OIDC, url tokens).
        /// </summary>
        public string Stamp { get; init; }

        public bool IsAnonymous => String.IsNullOrWhiteSpace(this.Username);
        public bool IsManageUser => this.IsAnonymous == false && this.AuthType == AuthTypes.Manage;
        public bool IsTokenUser => this.IsAnonymous == false && this.AuthType == AuthTypes.Tokenuser;
        public bool IsExpired => !IsAnonymous && DateTime.UtcNow.Ticks > Expire;

        #region Static Members

        private static AuthToken _anonymous = new AuthToken();
        public static AuthToken Anonymous => _anonymous;

        public static AuthToken Create(string path, string username, string password, AuthToken.AuthTypes authType, int expireMiniutes = 30)
        {
            var fi = username?.ToLoginFile(path);

            if (fi is null || !fi.Exists || String.IsNullOrEmpty(password))
            {
                return null;
            }

            expireMiniutes = expireMiniutes <= 0 ? 30 : expireMiniutes;
            var stored = File.ReadAllText(fi.FullName);

            if (username.UserNameIsUrlToken())
            {
                if (SecureCrypto.FixedTimeEquals(password, stored))
                {
                    return new AuthToken(username, authType, new TimeSpan(0, expireMiniutes, 0)) { Stamp = LoginStamp.Compute(fi) };
                }

                return null;
            }

            if (PasswordVerificationCache.IsVerified(fi.FullName, password, stored)
                || SecureCrypto.VerifyPassword(password, stored, username, out bool needsRehash) && Rehash(fi, password, needsRehash))
            {
                PasswordVerificationCache.SetVerified(fi.FullName, password, File.ReadAllText(fi.FullName));
                return new AuthToken(username, authType, new TimeSpan(0, expireMiniutes, 0)) { Stamp = LoginStamp.Compute(fi) };
            }

            return null;
        }

        /// <summary>
        /// Replaces a legacy (SHA256) hash with the current PBKDF2 hash after a successful login.
        /// Always returns true: a failed rewrite must not fail the login.
        /// </summary>
        private static bool Rehash(FileInfo fi, string password, bool needsRehash)
        {
            if (needsRehash)
            {
                try
                {
                    File.WriteAllText(fi.FullName, SecureCrypto.HashPassword(password));
                    LoginStamp.Invalidate(fi);
                }
                catch { }
            }

            return true;
        }


        #endregion

        #region Classes / Enums

        public enum AuthTypes
        {
            Unknown = 0,
            Tokenuser = 1,
            Manage = 2
        }

        #endregion
    }
}
