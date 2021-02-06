using gView.Framework.system;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace gView.Web.Framework.Web.Authorization
{
    public class WebAuthorizationCredentials
    {
        const string AuthTypeDefault = "Basic";
        const string GrantTypeDefault = "client_credentials";
        const string ScopeDefault = "openid";

        public WebAuthorizationCredentials(
            string username,
            string password,
            AuthorizationType authType = AuthorizationType.Basic,
            string accessTokenServiceUrl = "", 
            string grantType = "",
            string scope = "")
        {
            this.Username = username;
            this.Password = password;
            this.AuthType = authType;
            this.AccessTokenTokenServiceUrl = accessTokenServiceUrl;
            this.GrantType = grantType.OrTake(GrantTypeDefault);
            this.Scope = scope.OrTake(ScopeDefault);
        }

        public WebAuthorizationCredentials(string connectionString)
        {
            this.Username = connectionString.ExtractConnectionStringParameter("wa-user");
            this.Password = connectionString.ExtractConnectionStringParameter("wa-pwd");

            if (Enum.TryParse<AuthorizationType>(
                String.IsNullOrWhiteSpace(connectionString.ExtractConnectionStringParameter("wa-authtype")) ?
                AuthTypeDefault :
                connectionString.ExtractConnectionStringParameter("wa-authtype"), out AuthorizationType authType))
            {
                this.AuthType = authType;
            } else
            {
                this.AuthType = AuthorizationType.Basic;
            }

            AccessTokenTokenServiceUrl = connectionString.ExtractConnectionStringParameter("wa-access-token-service");
            GrantType = connectionString.ExtractConnectionStringParameter("wa-granttype").OrTake(GrantTypeDefault);
            Scope = connectionString.ExtractConnectionStringParameter("wa-scope").OrTake(ScopeDefault);
        }

        public string ConnectionString
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (!String.IsNullOrWhiteSpace(this.Username))
                {
                    sb.AddConnectionStringParameter("wa-user", this.Username);
                }

                if (!String.IsNullOrWhiteSpace(this.Password))
                {
                    sb.AddConnectionStringParameter("wa-pwd", this.Password);
                }

                if (!String.IsNullOrWhiteSpace(this.Username) ||
                    !this.AuthType.ToString().Trim().Equals(AuthTypeDefault, StringComparison.InvariantCultureIgnoreCase))
                {
                    sb.AddConnectionStringParameter("wa-authtype", this.AuthType.ToString());
                }

                if (!String.IsNullOrWhiteSpace(this.AccessTokenTokenServiceUrl))
                {
                    sb.AddConnectionStringParameter("wa-access-token-service", AccessTokenTokenServiceUrl);
                }

                if (!String.IsNullOrWhiteSpace(this.AccessTokenTokenServiceUrl) ||
                    !GrantTypeDefault.Equals(this.GrantType?.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    sb.AddConnectionStringParameter($"wa-granttype", this.GrantType);
                }

                if (!String.IsNullOrWhiteSpace(this.AccessTokenTokenServiceUrl) ||
                    !ScopeDefault.Equals(this.Scope?.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    sb.AddConnectionStringParameter($"wa-scope", this.Scope);
                }

                return sb.ToString();
            }
        }

        public string Username { get; }
        public string Password { get; }

        public AuthorizationType AuthType { get; }

        public string AccessTokenTokenServiceUrl { get; }

        public string GrantType { get; }
        public string Scope { get; }

        async public Task AddAuthorizationHeaders(HttpClient httpClient)
        {
            if (String.IsNullOrEmpty(this.Username))
                return;

            if (AuthToken == null || AuthToken.IsExpired())
            {
                if (this.AuthType == AuthorizationType.Basic)
                {
                    #region Basic Authentication

                    AuthToken = new Token()
                    {
                        AuthToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ this.Username }:{ this.Password }")),
                        Expires = DateTime.Now.AddDays(365)
                    };

                    #endregion
                }
                if (this.AuthType == AuthorizationType.Bearer)
                {
                    if (!String.IsNullOrEmpty(AccessTokenTokenServiceUrl))
                    {
                        #region OAuth2

                        var authServiceClient = httpClient; // use same...
                        //using (var authServiceClient = new HttpClient())
                        //{
                            authServiceClient.DefaultRequestHeaders.Authorization =
                                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ this.Username }:{ this.Password }")));

                            StringBuilder content = new StringBuilder();
                            content.Append($"grant_type={ this.GrantType }");
                            content.Append($"&scope={ WebUtility.UrlEncode(this.Scope) }");

                            using (var postContent = new StringContent(content.ToString()))
                            {
                                var httpResponseMessage = await authServiceClient.PostAsync(AccessTokenTokenServiceUrl, postContent);
                                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                                {
                                    var tokenResonseString = await httpResponseMessage.Content.ReadAsStringAsync();

                                    var accessToken = JsonConvert.DeserializeObject<AccessTokenResponse>(tokenResonseString);

                                    AuthToken = new Token()
                                    {
                                        AuthToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(accessToken.access_token)),
                                        Expires = DateTime.Now.AddSeconds(accessToken.expires_in)
                                    };
                                }
                            }
                        //}

                        #endregion
                    }
                    else
                    {
                        #region Username => access_token

                        AuthToken = new Token()
                        {
                            AuthToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ this.Username }")),
                            Expires = DateTime.Now.AddDays(365)
                        };

                        #endregion
                    }
                }
            }

            if (AuthToken != null)
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(AuthType.ToString(),
                    AuthToken.AuthToken);
            }
        }

        private Token AuthToken { get; set; } 

        #region Classes

        class Token
        {
            public string AuthToken { get; set; }
            public DateTime Expires { get; set; }

            public bool IsExpired()
            {
                return DateTime.Now > Expires;
            }
        }


        class AccessTokenResponse
        {
            public string access_token { get; set; }
            public string scope { get; set; }
            public string id_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
        }

        #endregion
    }
}
