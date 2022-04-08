using System;

namespace gView.Server.AppCode
{
    public class AuthToken
    {
        public AuthToken()
        {
            this.AuthType = AuthTypes.Unknown;
        }

        public AuthToken(string username, AuthTypes authType, DateTimeOffset expires)
        {
            this.Username = username;
            this.AuthType = authType;
            this.Expire = (DateTime.UtcNow.AddTicks(expires.Ticks)).Ticks;
        }

        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public long Expire { get; set; }
        public AuthTypes AuthType { get; set; }

        public bool IsAnonymous => String.IsNullOrWhiteSpace(this.Username);
        public bool IsManageUser => this.IsAnonymous == false && this.AuthType == AuthTypes.Manage;
        public bool IsTokenUser => this.IsAnonymous == false && this.AuthType == AuthTypes.Tokenuser;
        public bool IsExpired => !IsAnonymous && DateTime.UtcNow.Ticks > Expire;

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
