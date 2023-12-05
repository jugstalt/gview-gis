using gView.Framework.Common.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.Web.Authorization
{
    public class WebAuthorizationCredentials
    {
        const string AuthTypeDefault = "Basic";
        const string GrantTypeDefault = "client_credentials";
        const string ScopeDefault = "openid";

        private static ConcurrentDictionary<string, Token> _globalTokenCache = new ConcurrentDictionary<string, Token>();

        public WebAuthorizationCredentials(
            string username,
            string password,
            AuthorizationType authType = AuthorizationType.Basic,
            string accessTokenServiceUrl = "",
            string grantType = "",
            string scope = "")
        {
            Username = username;
            Password = password;
            AuthType = authType;
            AccessTokenTokenServiceUrl = accessTokenServiceUrl;
            GrantType = grantType.OrTake(GrantTypeDefault);
            Scope = scope.OrTake(ScopeDefault);
        }

        public WebAuthorizationCredentials(string connectionString)
        {
            Username = connectionString.ExtractConnectionStringParameter("wa-user");
            Password = connectionString.ExtractConnectionStringParameter("wa-pwd");

            if (Enum.TryParse(
                string.IsNullOrWhiteSpace(connectionString.ExtractConnectionStringParameter("wa-authtype")) ?
                AuthTypeDefault :
                connectionString.ExtractConnectionStringParameter("wa-authtype"), out AuthorizationType authType))
            {
                AuthType = authType;
            }
            else
            {
                AuthType = AuthorizationType.Basic;
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

                if (!string.IsNullOrWhiteSpace(Username))
                {
                    sb.AddConnectionStringParameter("wa-user", Username);
                }

                if (!string.IsNullOrWhiteSpace(Password))
                {
                    sb.AddConnectionStringParameter("wa-pwd", Password);
                }

                if (!string.IsNullOrWhiteSpace(Username) ||
                    !AuthType.ToString().Trim().Equals(AuthTypeDefault, StringComparison.InvariantCultureIgnoreCase))
                {
                    sb.AddConnectionStringParameter("wa-authtype", AuthType.ToString());
                }

                if (!string.IsNullOrWhiteSpace(AccessTokenTokenServiceUrl))
                {
                    sb.AddConnectionStringParameter("wa-access-token-service", AccessTokenTokenServiceUrl);
                }

                if (!string.IsNullOrWhiteSpace(AccessTokenTokenServiceUrl) ||
                    !GrantTypeDefault.Equals(GrantType?.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    sb.AddConnectionStringParameter($"wa-granttype", GrantType);
                }

                if (!string.IsNullOrWhiteSpace(AccessTokenTokenServiceUrl) ||
                    !ScopeDefault.Equals(Scope?.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    sb.AddConnectionStringParameter($"wa-scope", Scope);
                }

                return sb.ToString();
            }
        }

        public string Username { get; set; }
        public string Password { get; set; }

        public AuthorizationType AuthType { get; set; }

        public string AccessTokenTokenServiceUrl { get; set; }

        public string GrantType { get; set; }
        public string Scope { get; set; }

        async public Task AddAuthorizationHeaders(HttpClient httpClient)
        {
            if (string.IsNullOrEmpty(Username))
            {
                return;
            }

            try
            {
                Token token = null;
                _globalTokenCache.TryGetValue(CacheKey, out token);

                if (token == null || token.IsExpired())
                {
                    if (AuthType == AuthorizationType.Basic)
                    {
                        #region Basic Authentication

                        _globalTokenCache[CacheKey] = token = new Token()
                        {
                            AuthToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password}")),
                            Expires = DateTime.Now.AddDays(365)
                        };

                        #endregion
                    }
                    if (AuthType == AuthorizationType.Bearer)
                    {
                        if (!string.IsNullOrEmpty(AccessTokenTokenServiceUrl))
                        {
                            #region OAuth2

                            var authServiceClient = httpClient; // use same...
                            //using (var authServiceClient = new HttpClient())
                            {
                                authServiceClient.DefaultRequestHeaders.Authorization =
                                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password}")));
                                //authServiceClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");

                                var nvc = new List<KeyValuePair<string, string>>();
                                nvc.Add(new KeyValuePair<string, string>("grant_type", GrantType));
                                nvc.Add(new KeyValuePair<string, string>("scope", Scope));
                                var client = new HttpClient();

                                using (var req = new HttpRequestMessage(HttpMethod.Post, AccessTokenTokenServiceUrl) { Content = new FormUrlEncodedContent(nvc) })
                                {
                                    var httpResponseMessage = await authServiceClient.SendAsync(req);
                                    if (httpResponseMessage.IsSuccessStatusCode)
                                    {
                                        var tokenResonseString = await httpResponseMessage.Content.ReadAsStringAsync();
                                        var accessToken = JsonConvert.DeserializeObject<AccessTokenResponse>(tokenResonseString);

                                        if (!string.IsNullOrEmpty(accessToken.error))
                                        {
                                            throw new Exception(RemoveSecrets($"AccessToken POST request returns error: {accessToken.error} - {accessToken.error_description ?? string.Empty}"));
                                        }

                                        _globalTokenCache[CacheKey] = token = new Token()
                                        {
                                            AuthToken = accessToken.access_token, //Convert.ToBase64String(Encoding.UTF8.GetBytes(accessToken.access_token)),
                                            Expires = DateTime.Now.AddSeconds(accessToken.expires_in)
                                        };
                                    }
                                    else
                                    {
                                        throw new Exception($"AccessToken POST request returns statuscode {httpResponseMessage.StatusCode}");
                                    }
                                }
                            }

                            #endregion
                        }
                        else
                        {
                            #region Username => access_token

                            _globalTokenCache[CacheKey] = token = new Token()
                            {
                                AuthToken = Username,
                                Expires = DateTime.Now.AddDays(365)
                            };

                            #endregion
                        }
                    }
                }

                if (token != null)
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(AuthType.ToString(),
                        token.AuthToken);
                }
            }
            catch (Exception ex)
            {
                _globalTokenCache[CacheKey] = null;
                throw new Exception("WebAuthoriation Exception", ex);
            }
        }

        private string CacheKey => $"{AccessTokenTokenServiceUrl}:{AuthType.ToString()}:{Username}";

        private string RemoveSecrets(string msg)
        {
            if (!string.IsNullOrEmpty(Username))
            {
                msg = msg.Replace(Username, $"{Username.Substring(0, Math.Min(Username.Length, 2))}******");
            }

            if (!string.IsNullOrEmpty(Password))
            {
                msg = msg.Replace(Username, $"********");
            }

            if (!string.IsNullOrEmpty(AccessTokenTokenServiceUrl))
            {
                msg = msg.Replace(AccessTokenTokenServiceUrl, $"{AccessTokenTokenServiceUrl.Substring(0, Math.Min(AccessTokenTokenServiceUrl.Length, 24))}******");
            }

            return msg;
        }

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

            public string error { get; set; }
            public string error_description { get; set; }
        }

        #endregion
    }
}
