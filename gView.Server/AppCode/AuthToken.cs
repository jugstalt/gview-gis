using gView.Framework.Security;
using gView.Framework.system;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    public class AuthToken
    {
        public AuthToken()
        {
            
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

        #region Overrides

        public override string ToString()
        {
            //return Crypto.Encrypt(this.Username + "," + (int)this.AuthType + "," + Expire.ToString(), Globals.MasterPassword, resultType: Crypto.ResultType.Hex);
            return SecureCrypto.EncryptToken(LoginManager.GetCertificate(), Guid.NewGuid().ToString("N") + "|" + this.Username + "|" + (int)this.AuthType + "|" + Expire.ToString(), resultType: SecureCrypto.ResultType.Base62);
        }

        #endregion

        #region Static Members

        static public AuthToken FromString(string token)
        {
            string authToken = SecureCrypto.DecryptToken(LoginManager.GetCertificate(), token); //Crypto.Decrypt(token, Globals.MasterPassword);

            var at = authToken.Split('|');
            return new AuthToken()
            {
                Username = at[1],
                AuthType = (AuthTypes)int.Parse(at[2]),
                Expire = long.Parse(at[3])
            };
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
