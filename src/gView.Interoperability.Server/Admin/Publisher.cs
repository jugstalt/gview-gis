#nullable enable

using gView.Framework.IO;
using gView.Interoperability.GeoServices.Rest.DTOs;
using gView.Server.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace gView.Interoperability.Server.Admin;
public class Publisher
{
    private static HttpClient HttpClient = null!;

    private readonly string _server;
    private readonly string _client;
    private readonly string _secret;

    public Publisher(string server, string client, string secret)
    {
        _server = server;
        _client = client;
        _secret = secret;

        if (HttpClient is null) { HttpClient = new HttpClient(); }
    }

    async private Task<string> GetToken()
    {
        #region Get Access Token

        if (!String.IsNullOrEmpty(_client) && !String.IsNullOrEmpty(_secret))
        {
            var tokenUrl = $"{_server}/geoservices/tokens/generateToken";
            var tokenParams = $"request=gettoken&username={_client}&password={_secret}&expiration=1&f=json";

            using (var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = new StringContent(tokenParams, Encoding.UTF8, "application/x-www-form-urlencoded")
            })
            {
                var tokenResponse = await HttpClient.SendAsync(tokenRequest);

                if (tokenResponse.IsSuccessStatusCode)
                {
                    var tokenResponseString = await tokenResponse.Content.ReadAsStringAsync();

                    if (tokenResponseString.Contains("\"error\":"))
                    {
                        JsonErrorDTO error = JsonSerializer.Deserialize<JsonErrorDTO>(tokenResponseString)!;
                        throw new Exception($"GetToken-Error: {error.Error?.Code}\n{error.Error?.Message}\n{error.Error?.Details}");
                    }
                    else
                    {
                        JsonSecurityTokenDTO? jsonToken = JsonSerializer.Deserialize<JsonSecurityTokenDTO>(tokenResponseString);
                        if (jsonToken?.Token != null)
                        {
                            return jsonToken.Token;
                        }
                    }
                }
                else
                {
                    throw new Exception($"Token request returned Statuscode {tokenResponse.StatusCode}");
                }
            }
        }

        return String.Empty;

        #endregion
    }

    async public Task<string[]> GetFolders()
    {
        var token = await GetToken();
        var url = $"{_server}/geoservices/rest/services?f=json&token={token}";

        var response = await HttpClient.GetAsync(url);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        var services = JsonSerializer.Deserialize<JsonServicesDTO>(jsonResponse);

        return services?.Folders ?? [];
    }

    async public Task<string[]> GetServiceNames(string folder)
    {
        var token = await GetToken();
        var url = $"{_server}/geoservices/rest/services{(String.IsNullOrEmpty(folder) ? "" : $"/{folder}")}?f=json&token={token}";

        var response = await HttpClient.GetAsync(url);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        var services = JsonSerializer.Deserialize<JsonServicesDTO>(jsonResponse);

        return services?.Services?.Select(s =>  s.ServiceName).Distinct().ToArray() ?? [];
    }

    public Task<bool> Publish(string folder, string service, string mxl)
        => Publish(folder, service, XmlStream.DefaultEncoding.GetBytes(mxl));

    async public Task<bool> Publish(string folder, string service, byte[] mxlData)
    {
        var token = await GetToken();
        var url = $"{_server}/BrowseServices/PublishService?service={service}&folder={folder}&token={token}&f=json";

        var requestContent = new MultipartFormDataContent();
        var mxlContent = new ByteArrayContent(mxlData);
        mxlContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml");

        requestContent.Add(mxlContent, "file", $"{service}.mxl");

        var response = await HttpClient.PostAsync(url, requestContent);
        
        if(!response.IsSuccessStatusCode)
        {
            throw new Exception($"Publish response with status code {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        }


        var mapServerResponse = JsonSerializer.Deserialize<AdminMapServerResponse>(await response.Content.ReadAsStringAsync())!;
        if (mapServerResponse.Success == false)
        {
            throw new Exception($"Error on publishing service:{Environment.NewLine}{mapServerResponse.Message}");
        }

        return true;
    }
}
