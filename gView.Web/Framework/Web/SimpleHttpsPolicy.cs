using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace gView.Framework.Web
{
    //public class SimpleHttpsPolicy : ICertificatePolicy
    //{
    //    #region ICertificatePolicy Member

    //    public bool CheckValidationResult(ServicePoint srvPoint, System.Security.Cryptography.X509Certificates.X509Certificate certificate, WebRequest request, int certificateProblem)
    //    {
    //        return true;
    //    }

    //    #endregion

    //    public static bool CheckPolicy(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
    //    {
    //        return true;
    //    }
    //}

    public class Certification
    {
        public static FileInfo CertificateFile(string filename)
        {
            try
            {
                filename = filename.Trim();

                if (String.IsNullOrEmpty(filename))
                    return null;

                FileInfo fi = new FileInfo(filename);
                if (fi.Exists)
                    return fi;
            }
            catch { }
            return null;
        }

        public static X509Certificate X509Certificate(string filename, string pwd)
        {
            try
            {
                FileInfo fi = CertificateFile(filename);
                if (fi == null)
                    return null;

                //return new X509Certificate2(@"C:\Inetpub\wwwroot\etc\cer\extern_geoland.p12", "NuekomJGrY");
                if (fi.Extension.ToLower() == ".p12" || fi.Extension.ToLower() == ".pfx")
                {
                    return new X509Certificate2(fi.FullName, pwd);
                }
                return X509Certificate2.CreateFromCertFile(fi.FullName);
            }
            catch { }
            return null;
        }
    }
}
