using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace gView.Framework.Security
{
    public class X509CertificateWrapper
    {
        public X509CertificateWrapper(X509Certificate2 cert)
        {
            Certificate = cert;

            // Generate some Password for eg Token Encryption now to improve performace
            var hash = new SHA1Managed().ComputeHash(cert.GetPublicKey());
            hash = cert.GetRSAPrivateKey().SignHash(hash, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

            var password = new byte[128];
            var salt = new byte[8];
            var g1 = new byte[16];

            Buffer.BlockCopy(hash, 0, password, 0, password.Length);
            Buffer.BlockCopy(hash, password.Length, salt, 0, salt.Length);
            Buffer.BlockCopy(hash, password.Length + salt.Length, g1, 0, g1.Length);

            this.AESPassword = password;
            this.AESSalt = salt;
            this.AESG1 = g1;
        }

        public byte[] AESPassword { get; private set; }
        public byte[] AESSalt { get; private set; }
        public byte[] AESG1 { get; private set; }

        public X509Certificate2 Certificate { get; private set; }
    }
}
