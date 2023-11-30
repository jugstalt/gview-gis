using gView.Framework.Core.system;
using gView.Framework.Web;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml;


// wsdl /namespace:gView.MapServer.Connector /protocol:soap /out:MapServerProxy.cs /language:cs http://localhost:8001/MapServer?wsdl
namespace gView.Server.Connector
{
    public class ServerConnection : IErrorMessage
    {
        private string _url = String.Empty;

        public ServerConnection(string url)
        {
            _url = url;
            if (!_url.ToLower().StartsWith("http://") && !_url.ToLower().StartsWith("https://"))
            {
                _url = "http://" + _url;
            }

            this.Timeout = 0;
        }

        #region Static

        static public string ServerUrl(string server, int port)
        {
            if (port > 0 && port != 80 && port != 443)
            {
                server += ":" + port;
            }

            return server;
        }

        #endregion

        public int Timeout { get; set; }

        public string Send(string service, string request, string InterpreterGUID)
        {
            return Send(service, request, InterpreterGUID, String.Empty, String.Empty);
        }
        public string Send(string service, string request, string InterpreterGUID, string user, string pwd)
        {
            string ret = WebFunctions.HttpSendRequest(_url + "/MapRequest/" + InterpreterGUID + "/" + service, "POST",
                Encoding.UTF8.GetBytes(request), user, pwd, this.Timeout * 1000);

            return Encoding.UTF8.GetString(Encoding.Default.GetBytes(ret));
        }

        public List<MapService> Services(string user, string password)
        {
            List<MapService> services = new List<MapService>();
            DateTime td = DateTime.Now;

            string axl = String.Empty;
            axl = WebFunctions.HttpSendRequest(_url + "/catalog", "POST",
                Encoding.UTF8.GetBytes("<GETCLIENTSERVICES/>"), user, password, this.Timeout * 1000);

            TimeSpan ts = DateTime.Now - td;
            int millisec = ts.Milliseconds;

            if (axl == "")
            {
                return services;
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(axl);
            foreach (XmlNode mapService in doc.SelectNodes("//SERVICE[@name]"))
            {
                MapService.MapServiceType type = MapService.MapServiceType.MXL;
                if (mapService.Attributes["servicetype"] != null)
                {
                    switch (mapService.Attributes["servicetype"].Value.ToLower())
                    {
                        case "mxl":
                            type = MapService.MapServiceType.MXL;
                            break;
                        case "svc":
                            type = MapService.MapServiceType.SVC;
                            break;
                        case "gdi":
                            type = MapService.MapServiceType.GDI;
                            break;
                    }
                }
                services.Add(new MapService(mapService.Attributes["name"].Value, type));
            }
            return services;
        }
        public string QueryMetadata(string serivce)
        {
            return QueryMetadata(serivce, String.Empty, String.Empty);
        }
        public string QueryMetadata(string service, string user, string password)
        {
            try
            {
                return WebFunctions.HttpSendRequest(_url + "/getmetadata/" + service, "GET",
                    null, user, password, this.Timeout * 1000);
            }
            catch (Exception ex)
            {
                return "ERROR:" + ex.Message;
            }
        }

        public bool UploadMetadata(string service, string metadata)
        {
            return UploadMetadata(service, metadata, String.Empty, String.Empty);
        }
        public bool UploadMetadata(string service, string metadata, string user, string password)
        {
            try
            {
                return WebFunctions.HttpSendRequest(_url + "/setmetadata/" + service, "POST",
                    Encoding.UTF8.GetBytes(metadata), user, password, this.Timeout * 1000).ToLower().Trim() == "true";
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                return false;
            }
        }

        public bool AddMap(string name, string mxl, string usr, string pwd)
        {
            string ret = WebFunctions.HttpSendRequest(_url + "/addmap/" + name, "POST", Encoding.UTF8.GetBytes(mxl), usr, pwd, this.Timeout * 1000);
            if (ret.ToString().ToLower().Trim() != "true")
            {
                LastErrorMessage = ret;
                return false;
            }
            return true;
        }
        public bool RemoveMap(string name, string usr, string pwd)
        {
            string ret = WebFunctions.HttpSendRequest(_url + "/removemap/" + name, "GET", null, usr, pwd, this.Timeout * 1000);
            if (ret.ToString().ToLower().Trim() != "true")
            {
                LastErrorMessage = ret;
                return false;
            }
            return true;
        }

        public string LastErrorMessage { get; set; }

        public class MapService
        {
            public enum MapServiceType { MXL = 0, SVC = 1, GDI = 2 }
            private string _name;
            private MapServiceType _type;

            internal MapService(string name, MapServiceType type)
            {
                _name = name;
                _type = type;
            }

            public string Name { get { return _name; } }
            public MapServiceType Type
            {
                get { return _type; }
            }
        }

        public void Dispose()
        {
        }
    }
}
