using gView.Framework.Core.Exceptions;
using gView.Framework.Security;
using gView.Server.Services.MapServer;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace gView.Server.Services.Security
{
    public class EncryptionCertificateService
    {
        private readonly MapServiceManager _mapServerService;
        // Singleton
        private X509CertificateWrapper _cert = null;

        public EncryptionCertificateService(MapServiceManager mapServerService)
        {
            _mapServerService = mapServerService;
        }

        #region Certificate

        public X509CertificateWrapper GetCertificate(string name = "crypto0")
        {
            if (_cert == null)
            {
                ReloadCert(name);
            }

            return _cert;
        }

        private const string CertPassword = "gView-cert-P@55w0rd";

        private void CreateCert(string name)
        {
            var ecdsa = RSA.Create(); // generate asymmetric key pair
            var req = new CertificateRequest("cn=gview", ecdsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(100));

            // Create PFX (PKCS #12) with private key
            File.WriteAllBytes(_mapServerService.Options.LoginManagerRootPath + "/" + name + ".pfx", cert.Export(X509ContentType.Pfx, CertPassword));

            // Create Base 64 encoded CER (public key only)
            //File.WriteAllText(path + "/" + name + ".cer",
            //    "-----BEGIN CERTIFICATE-----\r\n"
            //    + Convert.ToBase64String(cert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks)
            //    + "\r\n-----END CERTIFICATE-----");
        }

        private void ReloadCert(string name, bool throwException = false)
        {
            try
            {
                FileInfo fi = new FileInfo(Path.Combine(_mapServerService.Options.LoginManagerRootPath, $"{name}.pfx"));
                if (!fi.Exists)
                {
                    CreateCert(name);
                }

                var pkcs12Cert = X509CertificateLoader.LoadPkcs12(
                    File.ReadAllBytes(fi.FullName), 
                    CertPassword,
                    X509KeyStorageFlags.MachineKeySet
                    | X509KeyStorageFlags.PersistKeySet
                    | X509KeyStorageFlags.Exportable
                    | X509KeyStorageFlags.UserKeySet
                    //| X509KeyStorageFlags.EphemeralKeySet
                    );     

                _cert = new X509CertificateWrapper(pkcs12Cert);

                //_cert = new X509CertificateWrapper(
                //    new X509Certificate2(fi.FullName, CertPassword,
                //               X509KeyStorageFlags.MachineKeySet
                //             | X509KeyStorageFlags.PersistKeySet
                //             | X509KeyStorageFlags.Exportable
                //             | X509KeyStorageFlags.UserKeySet));
            }
            catch (CryptographicException cx)
            {
                if (throwException == true)
                {
                    throw new MapServerException("gView internal Certificate: " + cx.Message);
                }

                CreateCert(name);
                ReloadCert(name, true);
            }
        }

        #endregion
    }
}
