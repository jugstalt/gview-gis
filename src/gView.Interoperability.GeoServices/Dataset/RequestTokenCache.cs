using gView.Framework.Web;
using gView.Interoperability.GeoServices.Extensions;
using gView.Interoperability.GeoServices.Rest.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;

namespace gView.Interoperability.GeoServices.Dataset
{
    public class RequestTokenCache
    {
        private static readonly object _refreshTokenLocker = new object();
        private static readonly ConcurrentDictionary<string, string> _tokenParams = new ConcurrentDictionary<string, string>();

        static public int TicketExpiration = 60;

        static public Task<string> RefreshTokenAsync(string serviceUrl, string user, string password, string currentToken = "")
        {
            string currentParameter = currentToken;
            //lock (_refreshTokenLocker)
            {
                string dictKey = serviceUrl + "/" + user;

                if (_tokenParams.ContainsKey(dictKey) && _tokenParams[dictKey] != currentParameter)
                {
                    return Task.FromResult(_tokenParams[dictKey]);
                }
                else
                {
                    int pos = serviceUrl.ToLower().IndexOf("/rest/");
                    string tokenServiceUrl = serviceUrl.Substring(0, pos) + "/tokens/generateToken";

                    //string tokenParams = $"request=gettoken&username={ user }&password={ password.UrlEncodePassword() }&expiration={ RequestTokenCache.TicketExpiration }&f=json";
                    string tokenParams = $"request=gettoken&username={user}&password={password.UrlEncodePassword()}&f=json";

                    string tokenResponse = String.Empty;
                    while (true)
                    {
                        try
                        {
                            tokenResponse = WebFunctions.HttpSendRequest($"{tokenServiceUrl}?{tokenParams}");
                            break;
                        }
                        catch (WebException we)
                        {
                            if (we.Message.Contains("(502)") && tokenServiceUrl.StartsWith("http://"))
                            {
                                tokenServiceUrl = "https:" + tokenServiceUrl.Substring(5);
                                continue;
                            }
                            throw;
                        }
                    }
                    if (tokenResponse.Contains("\"error\":"))
                    {
                        JsonError error = JsonConvert.DeserializeObject<JsonError>(tokenResponse);
                        throw new Exception($"GetToken-Error:{error.Error?.Code}\n{error.Error?.Message}\n{error.Error?.Details?.ToString()}");
                    }
                    else
                    {
                        JsonSecurityToken jsonToken = JsonConvert.DeserializeObject<JsonSecurityToken>(tokenResponse);
                        if (jsonToken.token != null)
                        {
                            _tokenParams.TryAdd(dictKey, jsonToken.token);
                            return Task.FromResult(jsonToken.token);
                        }
                    }
                }
            }

            return Task.FromResult(String.Empty);
        }
    }
}
