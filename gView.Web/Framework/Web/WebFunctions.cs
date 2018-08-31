using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Web;
using System.Xml;
using System.Net;
using System.IO;
using Microsoft.Win32;
using System.Security.Cryptography;
using gView.Framework.system;
using gView.Framework.IO;

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
                if (domain != "" && domain != null) credential.Domain = domain;
                proxy.Credentials = credential;
            }
            return proxy;
        }

        public static Bitmap DownloadImage(XmlNode output)
        {
            if (output.Attributes["file"] != null)
            {
                try
                {
                    FileInfo fi = new FileInfo(output.Attributes["file"].Value);
                    if (fi.Exists) return (Bitmap)Image.FromFile(fi.FullName);
                }
                catch (Exception ex) { LastErrorMessage = ex.Message; throw ex; }
            }
            if (output.Attributes["url"] != null)
            {
                return DownloadImage(
                    output.Attributes["url"].Value,
                    ProxySettings.Proxy(output.Attributes["url"].Value),
                    WebCredentials.FromXmlNode(output.SelectSingleNode("credentials")));
            }
            return null;
        }

        public static Bitmap DownloadImage(XmlNode output, IWebProxy proxy)
        {
            if (output.Attributes["file"] != null)
            {
                try
                {
                    FileInfo fi = new FileInfo(output.Attributes["file"].Value);
                    if (fi.Exists) return (Bitmap)Image.FromFile(fi.FullName);
                }
                catch (Exception ex) { LastErrorMessage = ex.Message; throw ex; }
            }
            if (output.Attributes["url"] != null)
            {
                return DownloadImage(
                    output.Attributes["url"].Value,
                    proxy,
                    WebCredentials.FromXmlNode(output.SelectSingleNode("credentials")));
            }
            return null;
        }
        public static Bitmap DownloadImage(string imageUrl)
        {
            return DownloadImage(imageUrl, ProxySettings.Proxy(imageUrl));
        }
        public static Bitmap DownloadImage(string imageUrl, string usr, string pwd)
        {
            return DownloadImage(imageUrl, ProxySettings.Proxy(imageUrl), null, usr, pwd);
        }
        public static Bitmap DownloadImage(string imageUrl, IWebProxy proxy)
        {
            return DownloadImage(imageUrl, proxy, null);
        }
        public static Bitmap DownloadImage(string imageUrl, IWebProxy proxy, ICredentials credentials)
        {
            return DownloadImage(imageUrl, proxy, credentials, String.Empty, String.Empty);
        }
        public static Bitmap DownloadImage(string imageUrl, IWebProxy proxy, ICredentials credentials, string usr,string pwd)
        {
            try
            {
                MemoryStream memStream = DownloadStream(imageUrl, proxy, credentials, usr, pwd);
                Bitmap bm = (Bitmap)Image.FromStream(memStream);
                memStream.Close();
                memStream.Dispose();

                return bm;
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                throw ex;
                //return null;
            }
        }

        public static MemoryStream DownloadStream(string url)
        {
            return DownloadStream(url, ProxySettings.Proxy(url));
        }
        public static MemoryStream DownloadStream(string url, string usr, string pwd)
        {
            return DownloadStream(url, ProxySettings.Proxy(url), null, usr, pwd);
        }
        public static MemoryStream DownloadStream(string url, IWebProxy proxy)
        {
            return DownloadStream(url, proxy, null);
        }
        public static MemoryStream DownloadStream(string url, IWebProxy proxy, ICredentials credentials)
        {
            return DownloadStream(url, proxy, credentials, String.Empty, String.Empty);
        }
        public static MemoryStream DownloadStream(string url, IWebProxy proxy, ICredentials credentials, string usr, string pwd)
        {
            try
            {
                HttpWebRequest wReq = (HttpWebRequest)HttpWebRequest.Create(url);
                wReq.Credentials = credentials;

                if (proxy != null) wReq.Proxy = proxy;
                AppendAuthentification(wReq, usr, pwd);

                //wReq.UserAgent = "Mozilla/5.0 (Windows NT 6.1; rv:15.0) Gecko/20100101 Firefox/15.0.1";
                wReq.UserAgent = ".NET Framework";

                HttpWebResponse wresp = (HttpWebResponse)wReq.GetResponse();

                int Bytes2Read = 3500000;
                Byte[] b = new Byte[Bytes2Read];

                DateTime t1 = DateTime.Now;
                Stream stream = wresp.GetResponseStream();

                MemoryStream memStream = new MemoryStream();

                while (Bytes2Read > 0)
                {
                    int len = stream.Read(b, 0, Bytes2Read);
                    if (len == 0) break;

                    memStream.Write(b, 0, len);
                }
                memStream.Position = 0;
                return memStream;
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                throw ex;
                //return null;
            }
        }

        public static string DownloadXml(string url)
        {
            return DownloadXml(url, String.Empty, String.Empty);
        }
        public static string DownloadXml(string url, string usr, string pwd)
        {
            return DownloadXml(url, ProxySettings.Proxy(url), usr, pwd);
        }
        public static string DownloadXml(string url, IWebProxy proxy)
        {
            return DownloadXml(url, proxy, String.Empty, String.Empty);
        }
        public static string DownloadXml(string url, IWebProxy proxy, string usr, string pwd)
        {
            try
            {
                HttpWebRequest wReq = (HttpWebRequest)HttpWebRequest.Create(url);

                if (proxy != null) wReq.Proxy = proxy;
                AppendAuthentification(wReq, usr, pwd);

                HttpWebResponse wresp = (HttpWebResponse)wReq.GetResponse();

                int Bytes2Read = 3500000;
                Byte[] b = new Byte[Bytes2Read];

                DateTime t1 = DateTime.Now;
                Stream stream = wresp.GetResponseStream();

                MemoryStream memStream = new MemoryStream();

                while (Bytes2Read > 0)
                {
                    int len = stream.Read(b, 0, Bytes2Read);
                    if (len == 0) break;

                    memStream.Write(b, 0, len);
                }
                memStream.Position = 0;
                string ret = Encoding.Default.GetString(memStream.GetBuffer()).Trim(' ', '\0'); ;
                memStream.Close();
                memStream.Dispose();

                return ret.Trim();
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                return "<Exception>" + ex.Message + "</Exception>";
            }
        }
        public static Bitmap DownloadImage(string imageUrl, byte[] postBytes)
        {
            return DownloadImage(imageUrl, postBytes, null);
        }
        public static Bitmap DownloadImage(string imageUrl, byte[] postBytes, IWebProxy proxy)
        {
            try
            {
                HttpWebRequest wReq = (HttpWebRequest)HttpWebRequest.Create(imageUrl);
                wReq.Method = "POST";

                if (proxy != null) wReq.Proxy = proxy;

                if (postBytes != null)
                {
                    wReq.ContentLength = postBytes.Length;

                    try
                    {
                        Stream postStream = wReq.GetRequestStream();
                        postStream.Write(postBytes, 0, postBytes.Length);
                        postStream.Flush();
                        postStream.Close();
                    }
                    catch (Exception e)
                    {
                        //log("ERROR@Connector.Sendrequest_ServletExec:\n"+e.Message,"");
                        LastErrorMessage = e.Message;
                        return null;
                    }
                }

                HttpWebResponse wresp = (HttpWebResponse)wReq.GetResponse();

                int Bytes2Read = 3500000;
                Byte[] b = new Byte[Bytes2Read];

                DateTime t1 = DateTime.Now;
                Stream stream = wresp.GetResponseStream();

                MemoryStream memStream = new MemoryStream();

                while (Bytes2Read > 0)
                {
                    int len = stream.Read(b, 0, Bytes2Read);
                    if (len == 0) break;

                    memStream.Write(b, 0, len);
                }
                memStream.Position = 0;
                Bitmap bm = (Bitmap)Image.FromStream(memStream);
                memStream.Close();
                memStream.Dispose();

                return bm;
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                return null;
            }
        }

        public static byte[] DownloadRaw(string url)
        {
            return DownloadRaw(url, null, null, null, String.Empty, String.Empty);
        }
        public static byte[] DownloadRaw(string url,string usr,string pwd)
        {
            return DownloadRaw(url, null, null, null, usr,pwd);
        }
        public static byte[] DownloadRaw(string url, byte[] postBytes, string usr, string pwd)
        {
            return DownloadRaw(url, postBytes, null, null, usr, pwd);
        }
        public static byte[] DownloadRaw(string url, IWebProxy proxy)
        {
            return DownloadRaw(url, null, proxy, null, String.Empty, String.Empty);
        }
        public static byte[] DownloadRaw(string url, IWebProxy proxy, ICredentials credentials)
        {
            return DownloadRaw(url, null, proxy, credentials, String.Empty, String.Empty);
        }
        public static byte[] DownloadRaw(string url, byte[] postBytes, IWebProxy proxy, ICredentials credentials, string usr, string pwd)
        {
            try
            {
                HttpWebRequest wReq = (HttpWebRequest)HttpWebRequest.Create(url);
                wReq.Credentials = credentials;

                if (proxy != null) wReq.Proxy = proxy;
                AppendAuthentification(wReq, usr, pwd);

                if (postBytes != null)
                {
                    wReq.Method = "POST";
                    wReq.ContentLength = postBytes.Length;

                    try
                    {
                        Stream postStream = wReq.GetRequestStream();
                        postStream.Write(postBytes, 0, postBytes.Length);
                        postStream.Flush();
                        postStream.Close();
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
                    wReq.ContentLength = 0;
                }

                HttpWebResponse wresp = (HttpWebResponse)wReq.GetResponse();

                int Bytes2Read = 3500000;
                Byte[] b = new Byte[Bytes2Read];

                DateTime t1 = DateTime.Now;
                Stream stream = wresp.GetResponseStream();

                MemoryStream memStream = new MemoryStream();

                while (Bytes2Read > 0)
                {
                    int len = stream.Read(b, 0, Bytes2Read);
                    if (len == 0) break;

                    memStream.Write(b, 0, len);
                }
                memStream.Position = 0;
                byte[] bytes = new byte[memStream.Length];
                memStream.Read(bytes, 0, (int)memStream.Length);

                return bytes;
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                throw ex;
                //return null;
            }
        }

        public static string HttpSendRequest(string url)
        {
            return HttpSendRequest(url, "GET", null, "", "");
        }
        public static string HttpSendRequest(string url, string methode, byte[] postBytes)
        {
            return HttpSendRequest(url, methode, postBytes, "", "");
        }
        public static string HttpSendRequest(string url, string methode, byte[] postBytes, string user, string password, int timeout=0)
        {
            return HttpSendRequest(url, methode, postBytes, user, password, Encoding.Default, timeout);
        }
        public static string HttpSendRequest(string url, string methode, byte[] postBytes, string user, string password, Encoding encoding, int timeout=0)
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
                    httpRequest.Timeout = timeout;

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
                catch (Exception ex)
                {
                    trys++;
                    if (trys > 5)
                    {
                        throw (ex);
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
                    if (len == 0) break;

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

        public static string AppendParametersToUrl(string url, string parameters)
        {
            string c = "?";
            if (url.EndsWith("?") || url.EndsWith("&"))
                c = "";
            else if (url.Contains("?"))
                c = "&";

            return url + c + parameters;
        }

        public static string RemoveDOCTYPE(string xml)
        {
            int pos = xml.IndexOf("<!DOCTYPE");
            if (pos != -1)
            {
                int o = 1, i;
                for (i = pos + 1; i < xml.Length; i++)
                {
                    if (xml[i] == '<')
                        o++;
                    else if (xml[i] == '>')
                    {
                        o--;
                        if (o == 0) break;
                    }
                }

                string s1 = xml.Substring(0, pos - 1);
                string s2 = xml.Substring(i + 1, xml.Length - i - 1);

                return s1 + s2;
            }

            return xml;
        }

        private static void AppendAuthentification(HttpWebRequest req, string usr, string pwd)
        {
            if (String.IsNullOrEmpty(usr.Trim()))
                return;

            string auth = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(usr + ":" + pwd));
            req.Headers.Add("Authorization", auth);
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
                if (!_loaded) ProxySettings.Load();
                return _useProxy;
            }
            set { _useProxy = value; }
        }

        static public string Server
        {
            get
            {
                if (!_loaded) ProxySettings.Load();
                return _server;
            }
            set { _server = value; }
        }

        static public int Port
        {
            get
            {
                if (!_loaded) ProxySettings.Load();
                return _port;
            }
            set { _port = value; }
        }

        static public string Exceptions
        {
            get
            {
                if (!_loaded) ProxySettings.Load();
                return _exceptions;
            }
            set { _exceptions = value; }
        }

        static public string Domain
        {
            get
            {
                if (!_loaded) ProxySettings.Load();
                return _domain;
            }
            set { _domain = value; }
        }

        static public string User
        {
            get
            {
                if (!_loaded) ProxySettings.Load();
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
                if (!stream.ReadStream(SystemVariables.CommonApplicationData + @"\options_webproxy.xml"))
                {
                    stream = new XmlStream("webproxy");
                    stream.Save("useproxy", (int)UseProxyType.none);
                    stream.Save("server", String.Empty);
                    stream.Save("port", 80);
                    stream.Save("exceptions", "localhost;127.0.0.1");
                    stream.Save("domain", String.Empty);
                    stream.Save("user", String.Empty);
                    stream.Save("password", String.Empty);
                    stream.WriteStream(SystemVariables.CommonApplicationData + @"\options_webproxy.xml");

                    stream = new XmlStream("webproxy");
                    stream.ReadStream(SystemVariables.CommonApplicationData + @"\options_webproxy.xml");
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
                stream.WriteStream(SystemVariables.CommonApplicationData + @"\options_webproxy.xml");

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
                        _proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
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
            if (!_loaded) ProxySettings.Load();

            if (_proxy == null) return null;

            string serverName = extractServername(url).ToLower();
            if (serverName.ToLower() == "localhost")
                return null;

            foreach (string iServer in _exceptions.Split(';'))
            {
                string pattern = iServer.ToLower();
                try
                {
                    if (Regex.IsMatch(serverName, pattern)) return null;
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

    public class WebCredentials
    {
        static public ICredentials FromXmlNode(XmlNode node)
        {
            if (node == null) return null;

            if (node.Attributes["default"] != null &&
                node.Attributes["default"].Value.ToLower() == "true") return CredentialCache.DefaultCredentials;
            if (node.Attributes["default"] != null &&
                node.Attributes["default"].Value.ToLower() == "net") return CredentialCache.DefaultNetworkCredentials;

            NetworkCredential credential = new NetworkCredential();
            if (node.Attributes["user"] != null)
                credential.UserName = node.Attributes["user"].Value;
            if (node.Attributes["pwd"] != null)
                credential.Password = node.Attributes["pwd"].Value;
            if (node.Attributes["domain"] != null)
                credential.Domain = node.Attributes["domain"].Value;

            return credential;
        }
    }
}
