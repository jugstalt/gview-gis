using gView.Framework.IO;
using gView.Framework.Common;
using gView.Framework.Web.Abstraction;
using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;
using System.Text.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace gView.Framework.Web
{
    public class WebFunctions
    {
        public static string LastErrorMessage = String.Empty;

        public static IWebProxy Proxy(string host, int port)
        {
            return Proxy(host, port, "", "", "");
        }

        public static IWebProxy Proxy(string server, int port, string user, string password, string domain)
        {
            IWebProxy proxy = new WebProxy(server, port);

            if (user != "" && user != null && password != null)
            {
                NetworkCredential credential = new NetworkCredential(user, password);
                if (domain != "" && domain != null)
                {
                    credential.Domain = domain;
                }

                proxy.Credentials = credential;
            }
            return proxy;
        }

        async public static Task<IBitmap> DownloadImage(IHttpService http, XmlNode output)
        {
            if (output.Attributes["file"] != null)
            {
                try
                {
                    FileInfo fi = new FileInfo(output.Attributes["file"].Value);
                    if (fi.Exists)
                    {
                        return Current.Engine.CreateBitmap(fi.FullName);
                    }
                }
                catch (Exception ex) { LastErrorMessage = ex.Message; throw; }
            }
            if (output.Attributes["url"] != null)
            {
                var webCredentials = WebCredentials.FromXmlNode(output);
                var imageData = await http.GetDataAsync(
                    output.Attributes["url"].Value,
                    webCredentials != null
                        ? new Models.RequestAuthorization() { Username = webCredentials.UserName, Password = webCredentials.Password }
                        : null);

                if (imageData != null)
                {
                    using (var ms = new MemoryStream(imageData))
                    {
                        return Current.Engine.CreateBitmap(ms);
                    }
                }
            }
            return null;
        }

        public static string HttpSendRequest(string url)
        {
            return HttpSendRequest(url, "GET", null, "", "");
        }
        public static string HttpSendRequest(string url, string methode, byte[] postBytes)
        {
            return HttpSendRequest(url, methode, postBytes, "", "");
        }
        public static string HttpSendRequest(string url, string methode, byte[] postBytes, string user, string password, int timeout = 0)
        {
            return HttpSendRequest(url, methode, postBytes, user, password, Encoding.Default, timeout);
        }
        public static string HttpSendRequest(string url, string methode, byte[] postBytes, string user, string password, Encoding encoding, int timeout = 0)
        {
            HttpWebResponse httpResponse;
            int trys = 0;
            while (true)
            {
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);

                if (!user.Equals(String.Empty) || !password.Equals(string.Empty))
                {
                    string auth = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(user + ":" + password));
                    httpRequest.Headers.Add("Authorization", auth);
                }

                //HttpWReq.Timeout = timeout;
                httpRequest.Method = methode;

                if (timeout > 0)
                {
                    httpRequest.Timeout = timeout;
                }

                //ProxySettings settings = new ProxySettings();
                //HttpWReq.Proxy = settings.Proxy(url);
                httpRequest.Proxy = ProxySettings.Proxy(url);

                if (postBytes != null)
                {
                    httpRequest.ContentLength = postBytes.Length;

                    try
                    {
                        Stream stream = httpRequest.GetRequestStream();
                        stream.Write(postBytes, 0, postBytes.Length);
                        stream.Flush();
                        stream.Close();
                    }
                    catch (Exception e)
                    {
                        //log("ERROR@Connector.Sendrequest_ServletExec:\n"+e.Message,"");
                        LastErrorMessage = e.Message;
                        return null;
                    }
                }
                else
                {
                    httpRequest.ContentLength = 0;
                }

                try
                {
                    httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    break;
                }
                catch (Exception)
                {
                    trys++;
                    if (trys > 5)
                    {
                        throw;
                    }
                }
            }

            using (Stream stream = httpResponse.GetResponseStream())
            {
                int Bytes2Read = 3500000;
                Byte[] b = new Byte[Bytes2Read];

                MemoryStream memStream = new MemoryStream();
                while (Bytes2Read > 0)
                {
                    int len = stream.Read(b, 0, Bytes2Read);
                    if (len == 0)
                    {
                        break;
                    }

                    memStream.Write(b, 0, len);
                }

                memStream.Position = 0;
                string s = encoding.GetString(memStream.GetBuffer()).Trim(' ', '\0');
                memStream.Close();
                memStream.Dispose();

                return s;
            }
            /*
            using (StreamReader sr = new StreamReader(HttpWResp.GetResponseStream()))
            {
                string s;
                System.IO.StringReader stgr = new System.IO.StringReader(sr.ReadToEnd());
                s = stgr.ReadToEnd();

                return s;
            }
             * */
        }
    }

    public class ProxySettings
    {
        public enum UseProxyType { defaultProxy = 0, none = 1, use = 2 }
        static private UseProxyType _useProxy = UseProxyType.defaultProxy;
        static private bool _loaded = false;
        static private string _server, _domain, _user, _password, _exceptions;
        static private int _port;
        static private IWebProxy _proxy = null;

        static public string LastErrorMessage = String.Empty;

        static public UseProxyType UseProxy
        {
            get
            {
                if (!_loaded)
                {
                    ProxySettings.Load();
                }

                return _useProxy;
            }
            set { _useProxy = value; }
        }

        static public string Server
        {
            get
            {
                if (!_loaded)
                {
                    ProxySettings.Load();
                }

                return _server;
            }
            set { _server = value; }
        }

        static public int Port
        {
            get
            {
                if (!_loaded)
                {
                    ProxySettings.Load();
                }

                return _port;
            }
            set { _port = value; }
        }

        static public string Exceptions
        {
            get
            {
                if (!_loaded)
                {
                    ProxySettings.Load();
                }

                return _exceptions;
            }
            set { _exceptions = value; }
        }

        static public string Domain
        {
            get
            {
                if (!_loaded)
                {
                    ProxySettings.Load();
                }

                return _domain;
            }
            set { _domain = value; }
        }

        static public string User
        {
            get
            {
                if (!_loaded)
                {
                    ProxySettings.Load();
                }

                return _user;
            }
            set
            {
                _user = value;
            }
        }

        static public string Password
        {
            set { EncodePassword(value); }
        }

        static public void Load()
        {
            try
            {
                _loaded = true;

                XmlStream stream = new XmlStream("webproxy");
                if (!stream.ReadStream(SystemVariables.CommonApplicationData + @"/options_webproxy.xml"))
                {
                    stream = new XmlStream("webproxy");
                    stream.Save("useproxy", (int)UseProxyType.none);
                    stream.Save("server", String.Empty);
                    stream.Save("port", 80);
                    stream.Save("exceptions", "localhost;127.0.0.1");
                    stream.Save("domain", String.Empty);
                    stream.Save("user", String.Empty);
                    stream.Save("password", String.Empty);
                    stream.WriteStream(SystemVariables.CommonApplicationData + @"/options_webproxy.xml");

                    stream = new XmlStream("webproxy");
                    stream.ReadStream(SystemVariables.CommonApplicationData + @"/options_webproxy.xml");
                }

                _useProxy = (UseProxyType)stream.Load("useproxy");
                _server = (string)stream.Load("server", "");
                _port = (int)stream.Load("port", 80);
                _exceptions = (string)stream.Load("exceptions", "");
                _domain = (string)stream.Load("domain", "");
                _user = (string)stream.Load("user", "");
                _password = (string)stream.Load("password", "");

                ProxySettings.LoadProxy();
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                _useProxy = UseProxyType.defaultProxy;
            }
        }

        static public bool Commit()
        {
            try
            {
                XmlStream stream = new XmlStream("webproxy");
                stream.Save("useproxy", (int)_useProxy);
                stream.Save("server", _server);
                stream.Save("port", _port);
                stream.Save("exceptions", _exceptions);
                stream.Save("domain", _domain);
                stream.Save("user", _user);
                stream.Save("password", _password);
                stream.WriteStream(SystemVariables.CommonApplicationData + @"/options_webproxy.xml");

                ProxySettings.LoadProxy();
                return true;
            }
            catch
            {
                return false;
            }
        }

        static private void LoadProxy()
        {
            switch (_useProxy)
            {
                case UseProxyType.defaultProxy:
                    _proxy = WebRequest.GetSystemWebProxy();
                    if (_proxy != null)
                    {
                        _proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                    }

                    break;
                case UseProxyType.none:
                    _proxy = null;
                    break;
                case UseProxyType.use:
                    if (!String.IsNullOrEmpty(_server))
                    {
                        if (!_user.Equals(string.Empty) && !_password.Equals(string.Empty))
                        {
                            _proxy = WebFunctions.Proxy(_server, _port, _user, DecodePassword(), _domain);
                        }
                        else
                        {
                            _proxy = WebFunctions.Proxy(_server, _port);
                        }
                    }
                    else
                    {
                        _proxy = null;
                    }
                    break;
            }
        }

        static public IWebProxy Proxy(string url)
        {
            if (!_loaded)
            {
                ProxySettings.Load();
            }

            if (_proxy == null)
            {
                return null;
            }

            string serverName = extractServername(url).ToLower();
            if (serverName.ToLower() == "localhost")
            {
                return null;
            }

            foreach (string iServer in _exceptions.Split(';'))
            {
                string pattern = iServer.ToLower();
                try
                {
                    if (Regex.IsMatch(serverName, pattern))
                    {
                        return null;
                    }
                }
                catch { }
            }

            return _proxy;
        }

        static private string extractServername(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                return uri.Host;
            }
            catch { return url; }
        }

        static private string DecodePassword()
        {
            //return Encoding.ASCII.GetString(Convert.FromBase64String(_password));
            return Crypto.Decrypt(_password, "195885bd-740b-4111-b8e8-68e3a42dec06");
        }
        static private void EncodePassword(string password)
        {
            //_password = Convert.ToBase64String(Encoding.ASCII.GetBytes(password));
            _password = Crypto.Encrypt(password, "195885bd-740b-4111-b8e8-68e3a42dec06");
        }
    }

    internal class WebCredentials
    {
        public string Domain { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }

        static public WebCredentials FromXmlNode(XmlNode node)
        {
            if (node == null)
            {
                return null;
            }

            var credential = new WebCredentials();
            if (node.Attributes["user"] != null)
            {
                credential.UserName = node.Attributes["user"].Value;
            }

            if (node.Attributes["pwd"] != null)
            {
                credential.Password = node.Attributes["pwd"].Value;
            }

            if (node.Attributes["domain"] != null)
            {
                credential.Domain = node.Attributes["domain"].Value;
            }

            return credential;
        }
    }
}
