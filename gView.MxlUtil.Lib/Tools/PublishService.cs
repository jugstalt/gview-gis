using gView.Interoperability.GeoServices.Rest.Json;
using gView.MxlUtil.Lib.Abstraction;
using gView.MxlUtil.Lib.Exceptions;
using gView.Server.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

// Example
// PublishService -mxl C:\gview5\mxl\sqlite\basis_sqlite.mxl -server https://localhost:44331 -service test-publish/basis1 -client publish-123 -secret publish-123
//

namespace gView.MxlUtil.Lib.Tools
{
    public class PublishService : IMxlUtility
    {
        #region IMxlUtility

        public string Name => "PublishService";

        public string Description()
        {
            return
@"
PublishService
--------------

Publish Mxl as gView Map Service
";
        }

        public string HelpText()
        {
            return Description() +
@"
Required arguments:
-mxl <mxl-file>
-server <gview-server-url>
-service <servicename incl. folder ... folder/servicename>

Optional arguments:
-client <clientname>
-secret <clientsecret>
";
        }

        async public Task Run(string[] args)
        {
            string inFile = String.Empty;
            string server = String.Empty;
            string service = String.Empty;
            string folder = String.Empty;
            string client = String.Empty, secret = String.Empty, accessToken = String.Empty;

            for (int i = 1; i < args.Length - 1; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-mxl":
                        inFile = args[++i];
                        break;
                    case "-server":
                        server = args[++i];
                        break;
                    case "-service":
                        service = args[++i];
                        break;
                    case "-client":
                        client = args[++i];
                        break;
                    case "-secret":
                        secret = args[++i];
                        break;
                }
            }

            if (String.IsNullOrEmpty(inFile) ||
               String.IsNullOrEmpty(server) ||
               String.IsNullOrEmpty(service))
            {
                throw new IncompleteArgumentsException();
            }

            if (service.Contains("/"))
            {
                folder = service.Substring(0, service.IndexOf("/"));
                service = service.Substring(service.IndexOf("/") + 1);
            }

            try
            {
                #region Get Access Token

                if (!String.IsNullOrEmpty(client) && !String.IsNullOrEmpty(secret))
                {
                    var tokenUrl = $"{ server }/geoservices/tokens/generateToken";
                    var tokenParams = $"request=gettoken&username={ client }&password={ secret }&expiration=60&f=json";

                    using (var httpClient = new HttpClient())
                    using (var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
                    {
                        Content = new StringContent(tokenParams, Encoding.UTF8, "application/x-www-form-urlencoded")
                    })
                    {
                        var tokenResponse = await httpClient.SendAsync(tokenRequest);

                        if (tokenResponse.IsSuccessStatusCode)
                        {
                            var tokenResponseString = await tokenResponse.Content.ReadAsStringAsync();

                            if (tokenResponseString.Contains("\"error\":"))
                            {
                                JsonError error = JsonConvert.DeserializeObject<JsonError>(tokenResponseString);
                                throw new Exception($"GetToken-Error: { error.Error?.Code }\n{ error.Error?.Message }\n{ error.Error?.Details }");
                            }
                            else
                            {
                                JsonSecurityToken jsonToken = JsonConvert.DeserializeObject<JsonSecurityToken>(tokenResponseString);
                                if (jsonToken.token != null)
                                {
                                    accessToken = jsonToken.token;
                                    Console.WriteLine($"Successfull requested access token ({ client }): { accessToken.Substring(0, 5) }........");
                                }
                            }
                        }
                        else
                        {
                            throw new Exception($"Token request returned Statuscode { tokenResponse.StatusCode }");
                        }
                    }
                }

                #endregion

                var url = $"{ server }/BrowseServices/AddService?service={ service }&folder={ folder }&token={ accessToken }&f=json";
                using (var httpClient = new HttpClient())
                {
                    var requestContent = new MultipartFormDataContent();
                    var mxlContent = new ByteArrayContent(await File.ReadAllBytesAsync(inFile));
                    mxlContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml");

                    requestContent.Add(mxlContent, "file", new FileInfo(inFile).Name);

                    var response = await httpClient.PostAsync(url, requestContent);

                    var mapServerResponse = JsonConvert.DeserializeObject<AdminMapServerResponse>(await response.Content.ReadAsStringAsync());
                    if (mapServerResponse.Success == false)
                    {
                        throw new Exception($"Error on publishing service:{ Environment.NewLine }{ mapServerResponse.Message }");
                    }

                    Console.WriteLine($"Service { folder }/{ service } successfully published...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(ex.Message);
            }
        }

        #endregion
    }
}
