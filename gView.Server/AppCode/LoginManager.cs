using gView.Framework.Security;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    public class LoginManager
    {
        static X509Certificate2 _cert = null;

        public static X509Certificate2 GetCertificate(string name = "crypto0")
        {
            if (_cert == null)
                new LoginManager(Globals.LoginManagerRootPath).ReloadCert(name);

            return _cert;
        }

        public LoginManager(string path)
        {
            this.LoginRootPath = path;
        }

        #region Private

        private string LoginRootPath { get; set; }

        private void CreateLogin(string path, string username, string password)
        {
            var hashedPassword = SecureCrypto.Hash64(password, username);

            File.WriteAllText(path + "/" + username + ".lgn", hashedPassword);
        }

        private AuthToken AuthToken(string path, string username, string password, AuthToken.AuthTypes authType, int expireMiniutes=30)
        {
            var fi = new FileInfo(path + "/" + username + ".lgn");
            if(fi.Exists)
            {
                if (SecureCrypto.VerifyPassword(password, File.ReadAllText(fi.FullName), username))
                {
                    return new AuthToken(username, authType, new DateTimeOffset(DateTime.UtcNow.Ticks, new TimeSpan(0, 30, 0)));
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
                CreateCert("crypto0");
            }

            return AuthToken(di.FullName, username, password, AppCode.AuthToken.AuthTypes.Manage, exipreMinutes);
        }

        #endregion

        #region Token Logins

        public void CreateTokenLogin(string username, string password)
        {
            var di = new DirectoryInfo(LoginRootPath + "/token");
            var fi = new FileInfo(di.FullName + "/" + username + ".lgn");
            if (fi.Exists)
                throw new Exception("User '" + username + "' already exists");

            var hashedPassword = SecureCrypto.Hash64(password, username);
            File.WriteAllText(fi.FullName, hashedPassword);
        }

        public void ChangeTokenUserPassword(string username, string newPassword)
        {
            var di = new DirectoryInfo(LoginRootPath + "/token");
            var fi = new FileInfo(di.FullName + "/" + username + ".lgn");
            if (!fi.Exists)
                throw new Exception("User '" + username + "' do not exists");

            var hashedPassword = SecureCrypto.Hash64(newPassword, username);
            File.WriteAllText(fi.FullName, hashedPassword);
        }
 
        public IEnumerable<string> GetTokenUsernames()
        {
           
            var di = new DirectoryInfo(LoginRootPath + "/token");
            return di.GetFiles("*.lgn").Select(f => f.Name.Substring(0, f.Name.Length - f.Extension.Length));
        }

        public AuthToken GetAuthToken(string username, string password, int expireMinutes=30)
        {
            var di = new DirectoryInfo(LoginRootPath + "/token");
            return AuthToken(di.FullName, username, password, AppCode.AuthToken.AuthTypes.Tokenuser, expireMinutes);
        }

        #endregion

        #region Certificate

        private void CreateCert(string name)
        {
            var ecdsa = RSA.Create(); // generate asymmetric key pair
            var req = new CertificateRequest("cn=gview", ecdsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));

            // Create PFX (PKCS #12) with private key
            File.WriteAllBytes(this.LoginRootPath + "/" + name + ".pfx", cert.Export(X509ContentType.Pfx, "P@55w0rd"));

            // Create Base 64 encoded CER (public key only)
            //File.WriteAllText(path + "/" + name + ".cer",
            //    "-----BEGIN CERTIFICATE-----\r\n"
            //    + Convert.ToBase64String(cert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks)
            //    + "\r\n-----END CERTIFICATE-----");
        }

        private void ReloadCert(string name)
        {
            FileInfo fi = new FileInfo(this.LoginRootPath + "/" + name + ".pfx");
            if (!fi.Exists)
                CreateCert(name);

            _cert = new X509Certificate2(fi.FullName, "P@55w0rd");
            var privateKey = _cert.GetRSAPrivateKey();
            var pubkey = _cert.GetPublicKey();

        }

        #endregion
    }
}
