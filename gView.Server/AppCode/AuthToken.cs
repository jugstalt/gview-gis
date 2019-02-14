using gView.Framework.system;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    public class AuthToken
    {
        public AuthToken()
        {

        }

        public AuthToken(string username, DateTimeOffset expires)
        {
            this.Username = username;
            this.Expire = (DateTime.UtcNow.AddTicks(expires.Ticks)).Ticks;
        }

        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public long Expire { get; set; }

        public bool IsAnonymous => String.IsNullOrWhiteSpace(this.Username);
        public bool IsExpired => DateTime.UtcNow.Ticks > Expire;

        #region Overrides

        public override string ToString()
        {
            return Crypto.Encrypt(this.Username + "," + Expire.ToString(), Globals.MasterPassword);
        }

        #endregion

        #region Static Members

        static public AuthToken FromString(string token)
        {
            string authToken = Crypto.Decrypt(token, Globals.MasterPassword);
            return new AuthToken()
            {
                Username = authToken.Split(',')[0],
                Expire = long.Parse(authToken.Split(',')[1])
            };
        }

        #endregion
    }
}
