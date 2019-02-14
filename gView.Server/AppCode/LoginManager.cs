using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    public class LoginManager
    {
        public LoginManager(string path)
        {
            this.LoginRootPath = path;
        }

        #region Private

        private string LoginRootPath { get; set; }

        private void CreateLogin(string path, string username, string password)
        {
            var hashedPassword = Crypto.Hash64(Crypto.Encrypt(password, Globals.MasterPassword));

            File.WriteAllText(path + "/" + username + ".lgn", hashedPassword);
        }

        private AuthToken AuthToken(string path, string username, string password, int expireMiniutes=30)
        {
            var fi = new FileInfo(path + "/" + username + ".lgn");
            if(fi.Exists)
            {
                var hashedPassword = Crypto.Hash64(Crypto.Encrypt(password, Globals.MasterPassword));
                if(hashedPassword==File.ReadAllText(fi.FullName))
                {
                    return new AuthToken(username, new DateTimeOffset(DateTime.UtcNow.Ticks, new TimeSpan(0, 30, 0)));
                }
            }

            return null;
        }

        #endregion

        #region Manager Logins

        public AuthToken GetManagerAuthToken(string username, string password, int exipreMinutes = 30, bool createIfFirst = false)
        {
            var di = new DirectoryInfo(LoginRootPath + "/manage");
            if(createIfFirst && di.GetFiles().Count()==0)
            {
                CreateLogin(di.FullName, username, password);
            }

            return AuthToken(di.FullName, username, password, exipreMinutes);
        }

        #endregion
    }
}
