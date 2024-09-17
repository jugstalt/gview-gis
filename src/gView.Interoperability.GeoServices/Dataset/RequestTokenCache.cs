using gView.Framework.Web;
using gView.Interoperability.GeoServices.Extensions;
using gView.Interoperability.GeoServices.Rest.DTOs;
using System.Text.Json;
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

        async static public Task<string> RefreshTokenAsync(string serviceUrl, string user, string password, string currentToken = "")
        {
            string currentParameter = currentToken;
            //lock (_refreshTokenLocker)
            {
                string dictKey = serviceUrl + "/" + user;

                if (_tokenParams.ContainsKey(dictKey) && _tokenParams[dictKey] != currentParameter)
                {
                    return _tokenParams[dictKey];
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
                            tokenResponse = await WebFunctions.HttpSendRequest($"{tokenServiceUrl}?{tokenParams}");
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
                        JsonErrorDTO error = JsonSerializer.Deserialize<JsonErrorDTO>(tokenResponse);
                        throw new Exception($"GetToken-Error:{error.Error?.Code}\n{error.Error?.Message}\n{error.Error?.Details?.ToString()}");
                    }
                    else
                    {
                        JsonSecurityTokenDTO jsonToken = JsonSerializer.Deserialize<JsonSecurityTokenDTO>(tokenResponse);
                        if (jsonToken.Token != null)
                        {
                            _tokenParams.TryAdd(dictKey, jsonToken.Token);
                            return jsonToken.Token;
                        }
                    }
                }
            }

            return String.Empty;
        }
    }
}
