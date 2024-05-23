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

        public bool IsAnonymous => String.IsNullOrWhiteSpace(this.Username);
        public bool IsManageUser => this.IsAnonymous == false && this.AuthType == AuthTypes.Manage;
        public bool IsTokenUser => this.IsAnonymous == false && this.AuthType == AuthTypes.Tokenuser;
        public bool IsExpired => !IsAnonymous && DateTime.UtcNow.Ticks > Expire;

        #region Static Members

        private static AuthToken _anonymous = new AuthToken();
        public static AuthToken Anonymous => _anonymous;

        public static AuthToken Create(string path, string username, string password, AuthToken.AuthTypes authType, int expireMiniutes = 30)
        {
            var fi = new FileInfo(Path.Combine(path, $"{username}.lgn"));

            if (fi.Exists)
            {
                expireMiniutes = expireMiniutes <= 0 ? 30 : expireMiniutes;

                if (username.UserNameIsUrlToken())
                {
                    if (password == File.ReadAllText(fi.FullName))
                    {
                        return new AuthToken(username, authType, new TimeSpan(0, expireMiniutes, 0));
                    }
                }
                else
                {
                    if (SecureCrypto.VerifyPassword(password, File.ReadAllText(fi.FullName), username))
                    {
                        return new AuthToken(username, authType, new TimeSpan(0, expireMiniutes, 0));
                    }
                }
            }

            return null;
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
