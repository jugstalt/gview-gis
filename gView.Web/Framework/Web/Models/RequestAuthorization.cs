using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace gView.Framework.Web.Models
{
    public class RequestAuthorization
    {
        public RequestAuthorization(string authType = "Basic", string urlTokenParameterName = "token")
        {
            this.AuthType = authType;
            this.UrlTokenParameterName = urlTokenParameterName;
        }

        public string AuthType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AccessToken { get; set; }

        public string UrlTokenParameterName { get; set; }
        public string UrlToken { get; set; }

        public X509Certificate ClientCerticate { get; set; }

        public ICredentials Credentials { get; set; }
    }
}
