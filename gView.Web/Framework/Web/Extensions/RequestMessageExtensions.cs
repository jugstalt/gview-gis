using gView.Framework.Web.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace gView.Framework.Web.Extensions
{
    static public class RequestMessageExtensions
    {
        static public void AddAuthentication(this HttpRequestMessage requestMessage, RequestAuthorization authentication)
        {
            if (authentication != null)
            {
                string username = authentication.Username;
                string password = authentication.Password;
                string authType = authentication.AuthType;

                if (String.IsNullOrEmpty(username) && authentication.Credentials is NetworkCredential)
                {
                    if (String.IsNullOrEmpty(((NetworkCredential)authentication.Credentials).Domain))
                    {
                        username = ((NetworkCredential)authentication.Credentials).UserName;
                        password = ((NetworkCredential)authentication.Credentials).Password;

                        authType = "Basic"; // ??
                    }
                }

                if (!String.IsNullOrEmpty(username) &&
                    !String.IsNullOrEmpty(password) &&
                    authType?.ToLower() == "basic")
                {
                    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ username }:{ password }")));
                }
                else if (!String.IsNullOrEmpty(authentication.AccessToken) &&
                        authType?.ToLower() == "bearer")
                {
                    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authentication.AccessToken);
                }

                if (!String.IsNullOrEmpty(authentication.UrlToken))
                {
                    string tokenParameter = authentication.UrlToken.Contains("=") ?
                                                            authentication.UrlToken :
                                                            $"{authentication.UrlTokenParameterName}={authentication.UrlToken}";

                    var url = requestMessage.RequestUri.ToString();
                    url += (url.Contains("?") ? "&" : "?") + tokenParameter;

                    requestMessage.RequestUri = new Uri(url);
                }
            }
        }
    }
}
