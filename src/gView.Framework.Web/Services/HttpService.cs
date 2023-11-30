using gView.Framework.Web.Abstraction;
using gView.Framework.Web.Exceptions;
using gView.Framework.Web.Extensions;
using gView.Framework.Web.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace gView.Framework.Web.Services
{
    public class HttpService : IHttpService
    {
        static private HttpClient _staticClient = null;

        //private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpServiceOptions _options;

        private HttpService()
        {
            //_httpClientFactory = null;
            if (_staticClient == null)
            {
                _staticClient = new HttpClient();
            }

            _options = new HttpServiceOptions();
        }

        //public HttpService(IHttpClientFactory httpClientFactory,
        //                   IOptionsMonitor<HttpServiceOptions> optionsMonitor)
        //{
        //    _httpClientFactory = httpClientFactory;
        //    _options = optionsMonitor?.CurrentValue ?? new HttpServiceOptions();
        //}

        #region Get

        async public Task<byte[]> GetDataAsync(string url,
                                               RequestAuthorization authorization = null,
                                               int timeOutSeconds = 20)
        {
            url = CheckUrl(url);

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                // ToDo: brauch man, wenn man Google Tiles downloade möchte...
                //request.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("Mozilla/5.0 (Windows NT 6.1; WOW64; rv:34.0) Gecko/20100101 Firefox/34.0"));

                request.AddAuthentication(authorization);

                var cts = new CancellationTokenSource(timeOutSeconds * 1000);
                HttpResponseMessage responseMessage = null;
                try
                {

                    if (authorization?.ClientCerticate == null)
                    {
                        var client = Create(url);
                        responseMessage = await client.SendAsync(request, cts.Token);
                    }
                    else
                    {
                        using (var clientHandler = new HttpClientHandler())
                        {
                            clientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
                            clientHandler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                            clientHandler.ClientCertificates.Add(authorization.ClientCerticate);

                            using (var client = new HttpClient(clientHandler))
                            {
                                responseMessage = await client.SendAsync(request, cts.Token);
                            }
                        }
                    }
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var bytes = await responseMessage.Content.ReadAsByteArrayAsync();

                        return bytes;
                    }
                    else
                    {
                        throw new HttpServiceException($"Request returned Statuscode {responseMessage.StatusCode}");
                    }
                }
                catch /*(TaskCanceledException ex)*/
                {
                    //if (ex.CancellationToken == cts.Token)
                    {
                        throw new System.Exception("The http operation is canceled (timed out)!");
                    }
                }
                finally
                {
                    if (responseMessage != null)
                    {
                        responseMessage.Dispose();
                    }
                }
            }
        }

        async public Task<string> GetStringAsync(string url,
                                                 RequestAuthorization authorization = null,
                                                 int timeOutSeconds = 20,
                                                 Encoding encoding = null)
        {
            var bytes = await GetDataAsync(url, authorization, timeOutSeconds);

            return EncodeBytes(bytes, encoding);
        }

        #endregion

        #region Post

        async public Task<string> PostFormUrlEncodedStringAsync(string url,
                                                                string postData,
                                                                RequestAuthorization authorization = null,
                                                                int timeOutSeconds = 20)
        {
            url = CheckUrl(url);

            using (var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded")
            })
            {
                request.AddAuthentication(authorization);

                var cts = new CancellationTokenSource(timeOutSeconds * 1000);
                HttpResponseMessage responseMessage = null;

                try
                {
                    var client = Create(url);
                    responseMessage = await client.SendAsync(request, cts.Token);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var bytes = await responseMessage.Content.ReadAsByteArrayAsync();

                        return Encoding.UTF8.GetString(bytes);
                    }
                    else
                    {
                        throw new HttpServiceException($"Request returned Statuscode {responseMessage.StatusCode}");
                    }
                }
                catch /*(TaskCanceledException ex)*/
                {
                    //if (ex.CancellationToken == cts.Token)
                    {
                        throw new System.Exception("The http operation is canceled (timed out)!");
                    }
                }
                finally
                {
                    if (responseMessage != null)
                    {
                        responseMessage.Dispose();
                    }
                }
            }
        }

        async public Task<byte[]> PostFormUrlEncodedAsync(string url,
                                                          byte[] postData,
                                                          RequestAuthorization authorization = null,
                                                          int timeOutSeconds = 20)
        {
            var dataString = Encoding.UTF8.GetString(postData);

            url = CheckUrl(url);

            using (var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(dataString, Encoding.UTF8, "application/x-www-form-urlencoded")
            })
            {
                request.AddAuthentication(authorization);

                var cts = new CancellationTokenSource(timeOutSeconds * 1000);
                HttpResponseMessage responseMessage = null;

                try
                {
                    var client = Create(url);
                    responseMessage = await client.SendAsync(request, cts.Token);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var bytes = await responseMessage.Content.ReadAsByteArrayAsync();

                        return bytes;
                    }
                    else
                    {
                        throw new HttpServiceException($"Request returned Statuscode {responseMessage.StatusCode}");
                    }
                }
                catch /*(TaskCanceledException ex)*/
                {
                    //if (ex.CancellationToken == cts.Token)
                    {
                        throw new System.Exception("The http operation is canceled (timed out)!");
                    }
                }
                finally
                {
                    if (responseMessage != null)
                    {
                        responseMessage.Dispose();
                    }
                }
            }
        }

        async public Task<byte[]> PostDataAsync(string url,
                                                byte[] postData,
                                                RequestAuthorization authorization = null,
                                                int timeOutSeconds = 20)
        {
            url = CheckUrl(url);

            using (var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new ByteArrayContent(postData, 0, postData.Length)
            })
            {
                request.AddAuthentication(authorization);

                var cts = new CancellationTokenSource(timeOutSeconds * 1000);
                HttpResponseMessage responseMessage = null;

                try
                {
                    var client = Create(url);
                    responseMessage = await client.SendAsync(request, cts.Token);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var bytes = await responseMessage.Content.ReadAsByteArrayAsync();

                        return bytes;
                    }
                    else
                    {
                        throw new HttpServiceException($"Request returned Statuscode {responseMessage.StatusCode}");
                    }
                }
                catch /*(TaskCanceledException ex)*/
                {
                    //if (ex.CancellationToken == cts.Token)
                    {
                        throw new System.Exception("The http operation is canceled (timed out)!");
                    }
                }
                finally
                {
                    if (responseMessage != null)
                    {
                        responseMessage.Dispose();
                    }
                }
            }
        }

        async public Task<string> PostJsonAsync(string url,
                                                string json,
                                                RequestAuthorization authorization = null,
                                                int timeOutSeconds = 20)
        {
            url = CheckUrl(url);

            using (var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            })
            {
                request.AddAuthentication(authorization);

                var cts = new CancellationTokenSource(timeOutSeconds * 1000);
                HttpResponseMessage responseMessage = null;

                try
                {
                    var client = Create(url);
                    responseMessage = await client.SendAsync(request, cts.Token);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var response = await responseMessage.Content.ReadAsStringAsync();

                        return response;
                    }
                    else
                    {
                        throw new HttpServiceException($"Request returned Statuscode {responseMessage.StatusCode}");
                    }
                }
                catch /*(TaskCanceledException ex)*/
                {
                    //if (ex.CancellationToken == cts.Token)
                    {
                        throw new System.Exception("The http operation is canceled (timed out)!");
                    }
                }
                finally
                {
                    if (responseMessage != null)
                    {
                        responseMessage.Dispose();
                    }
                }
            }
        }

        async public Task<string> PostXmlAsync(string url,
                                               string xml,
                                               RequestAuthorization authorization = null,
                                               int timeOutSeconds = 20,
                                               Encoding encoding = null)
        {
            var xmlData = await PostDataAsync(url, (encoding ?? Encoding.UTF8).GetBytes(xml), authorization, timeOutSeconds);

            return EncodeBytes(xmlData, encoding);
        }

        #endregion

        #region Proxy

        public WebProxy GetProxy(string server)
        {
            if (_options.UseProxy && !IgnorProxy(server))
            {
                return _options.WebProxyInstance;
            }

            return null;
        }

        #endregion

        #region Url

        public string AppendParametersToUrl(string url, string parameters)
        {
            string c = "?";
            if (url.EndsWith("?") || url.EndsWith("&"))
            {
                c = "";
            }
            else if (url.Contains("?"))
            {
                c = "&";
            }

            return url + c + parameters;
        }

        public string ApplyUrlRedirection(string url)
        {
            if (_options?.UrlRedirections == null || _options.UrlRedirections.Count == 0 || String.IsNullOrEmpty(url))
            {
                return url;
            }

            var lowerCaseUrl = url.ToLower();

            foreach (string from in _options.UrlRedirections.Keys)
            {
                if (String.IsNullOrEmpty(from) ||
                    String.IsNullOrEmpty(_options.UrlRedirections[from]))
                {
                    continue;
                }

                if (lowerCaseUrl.IndexOf(from) >= 0)
                {
                    url = lowerCaseUrl.Replace(from, _options.UrlRedirections[from]);
                }
            }

            return url;
        }

        #region Legacy

        public bool Legacy_AlwaysDownloadFrom(string filename)  // From good old ArcIMS ...
        {
            var alwaysdownloadfrom = _options?.Legacy_AlwaysDownloadFrom;

            if (alwaysdownloadfrom == null)
            {
                return false;
            }

            filename = filename.ToLower();
            foreach (string f in alwaysdownloadfrom)
            {
                if (f == "*")
                {
                    return true;
                }

                string pattern = f.ToLower();
                if (Regex.IsMatch(filename, f))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #endregion

        public HttpClient Create(string url)
        {
            //if (_httpClientFactory == null)
            {
                return _staticClient;
            }

            //if (_options.UseProxy && !IgnorProxy(url))
            //{
            //    return _httpClientFactory.CreateClient(_options.DefaultProxyClientName);
            //}

            //return _httpClientFactory.CreateClient(_options.DefaultClientName);
        }

        #region Helper

        private string CheckUrl(string url)
        {
            if (url.StartsWith("//"))
            {
                url = $"{(_options.ForceHttps ? "https:" : "http:")}{url}";
            }

            return url;
        }



        private bool IgnorProxy(string server)
        {
            if (_options.IgnoreProxyServers == null)
            {
                return false;
            }

            server = server.ToLower();
            if (server.StartsWith("#") || server.StartsWith("$") || server.StartsWith("~") || server.StartsWith("&"))
            {
                server = server.Substring(1, server.Length - 1);
            }

            if (server.StartsWith("http://"))
            {
                server = server.Substring(7, server.Length - 7);
            }

            if (server.StartsWith("https://"))
            {
                server = server.Substring(8, server.Length - 8);
            }

            foreach (string iServer in _options.IgnoreProxyServers)
            {
                string pattern = iServer.ToLower();
                if (Regex.IsMatch(server, pattern))
                {
                    return true;
                }

                if (server.StartsWith(pattern))
                {
                    return true;
                }
            }
            return false;
        }

        private static string EncodeBytes(byte[] bytes, Encoding encoding)
        {
            string result;

            if (encoding == null)
            {
                encoding = Encoding.UTF8;
                result = encoding.GetString(bytes).Trim(' ', '\0').Trim();

                #region Xml Encoding

                try
                {
                    if (result.StartsWith("<?xml "))
                    {
                        int index = result.IndexOf(" encoding=");
                        if (index != -1)
                        {
                            int index2 = result.IndexOf(result[index + 10], index + 11);
                            if (index2 != -1)
                            {

                                string encodingString = result.Substring(index + 11, index2 - index - 11);
                                if (encodingString.ToLower() != "utf-8" && encodingString.ToLower() != "utf8")
                                {
                                    encoding = Encoding.GetEncoding(encodingString);
                                    if (encoding != null)
                                    {
                                        result = encoding.GetString(bytes).Trim(' ', '\0').Trim();
                                    }
                                    else
                                    {
                                        encoding = Encoding.UTF8;
                                    }
                                }

                            }
                        }
                    }
                }
                catch { }

                #endregion
            }
            else
            {
                result = encoding.GetString(bytes).Trim(' ', '\0').Trim();
            }

            return result;
        }

        #endregion

        #region Static

        static public IHttpService CreateInstance()
        {
            return new HttpService();
        }

        #endregion
    }
}
