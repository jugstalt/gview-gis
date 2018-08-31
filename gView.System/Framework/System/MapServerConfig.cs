using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Microsoft.Win32;
using System.Xml;

namespace gView.Framework.system
{
    static public class MapServerConfig
    {
        static private string _defaultOutputPath = String.Empty,
            _defaultOutputUrl = String.Empty,
            _defaultTileCachePath = String.Empty,
            _defaultOnlineResource = String.Empty;
        static private int _removeInterval = 20;
        static private bool _loaded = false;
        static private ServerConfig[] _servers = new ServerConfig[0];

        static public void Load()
        {
            _loaded = true;

            try
            {
                _servers = new ServerConfig[0];

                XmlDocument doc = new XmlDocument();
                doc.Load(SystemVariables.CommonApplicationData + @"\mapServer\MapServerConfig.xml");

                XmlNode globalsNode = doc.SelectSingleNode("mapserver/globals");
                _defaultOutputUrl = GetAttributeValue(globalsNode, "outputurl", "http://localhost/output");
                _defaultOutputPath = GetAttributeValue(globalsNode, "outputpath", @"C:\output");
                _defaultTileCachePath = GetAttributeValue(globalsNode, "tilecachepath", @"C:\output\tiles");
                _defaultOnlineResource = GetAttributeValue(globalsNode, "onlineresource", "http://localhost:{port}");
                _removeInterval = int.Parse(GetAttributeValue(globalsNode, "removeinterval", "20"));

                foreach (XmlNode serverNode in doc.SelectNodes("mapserver/servers/server[@port]"))
                {
                    ServerConfig serverConfig = new ServerConfig();
                    serverConfig.Port = int.Parse(serverNode.Attributes["port"].Value);
                    serverConfig.OutputUrl = GetAttributeValue(serverNode, "outputurl", _defaultOutputUrl);
                    serverConfig.OutputPath = GetAttributeValue(serverNode, "outputpath", _defaultOutputPath);
                    serverConfig.TileCachePath = GetAttributeValue(serverNode, "tilecachpath", _defaultTileCachePath);
                    serverConfig.OnlineResource = GetAttributeValue(serverNode, "onlineresource", _defaultOnlineResource.Replace("{port}", serverConfig.Port.ToString()));

                    foreach (XmlNode instanceNode in serverNode.SelectNodes("instances/instance[@port]"))
                    {
                        ServerConfig.InstanceConfig instanceConfig = new ServerConfig.InstanceConfig();
                        instanceConfig.Port = int.Parse(instanceNode.Attributes["port"].Value);
                        instanceConfig.MaxThreads = int.Parse(GetAttributeValue(instanceNode, "threads", "4"));
                        instanceConfig.MaxQueueLength = int.Parse(GetAttributeValue(instanceNode, "queuelength", "100"));
                        Array.Resize<ServerConfig.InstanceConfig>(ref serverConfig.Instances, serverConfig.Instances.Length + 1);
                        serverConfig.Instances[serverConfig.Instances.Length - 1] = instanceConfig;
                    }
                    Array.Resize<ServerConfig>(ref _servers, _servers.Length + 1);
                    _servers[_servers.Length - 1] = serverConfig;
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                _loaded = false;
            }
        }

        static private string GetAttributeValue(XmlNode node, string attrName, string defValue)
        {
            if (node == null || node.Attributes[attrName] == null)
                return defValue;

            return node.Attributes[attrName].Value;
        }

        static public string DefaultOutputPath
        {
            get
            {
                if (!_loaded) Load();
                return _defaultOutputPath;
            }
            set
            {
                _defaultOutputPath = value;
            }
        }
        static public string DefaultOutputUrl
        {
            get
            {
                if (!_loaded) Load();
                return _defaultOutputUrl;
            }
            set
            {
                _defaultOutputUrl = value;
            }
        }
        static public int RemoveInterval
        {
            get
            {
                if (!_loaded) Load();
                return _removeInterval;
            }
            set
            {
                _removeInterval = value;
            }
        }
        static public string DefaultTileCachePath
        {
            get
            {
                if (!_loaded) Load();
                return _defaultTileCachePath;
            }
            set
            {
                _defaultTileCachePath = value;
            }
        }
        static public string DefaultOnlineResource
        {
            get
            {
                if (!_loaded) Load();
                return _defaultOnlineResource;
            }
            set
            {
                _defaultOnlineResource = value;
            }
        }

        static public void Commit()
        {
            if (!_loaded) Load();
        }

        public class ServerConfig
        {
            public int Port = 8001;
            public string OutputPath = String.Empty;
            public string OutputUrl = String.Empty;
            public string OnlineResource = String.Empty;
            public string TileCachePath = String.Empty;
            public InstanceConfig[] Instances = new InstanceConfig[0];

            public class InstanceConfig
            {
                public int Port = 8011;
                public int MaxThreads = 5;
                public int MaxQueueLength = 100;
            }
        }
        static public int ServerCount
        {
            get
            {
                if (!_loaded) Load();

                return _servers.Length;
            }
        }
        static public ServerConfig Server(int serverNumber)
        {
            if (!_loaded) Load();

            if (serverNumber < 0 || serverNumber >= ServerCount) return null;

            return _servers[serverNumber];
        }

        static public ServerConfig ServerByPort(int port)
        {
            if (!_loaded) Load();

            for (int i = 0; i < _servers.Length; i++)
            {
                if (_servers[i].Port == port)
                    return _servers[i];
            }
            return null;
        }

        static public ServerConfig ServerByInstancePort(int port)
        {
            if (!_loaded) Load();

            for (int s = 0; s < _servers.Length; s++)
            {
                for (int i = 0; i < _servers[s].Instances.Length; i++)
                {
                    if (_servers[s].Instances[i].Port == port)
                        return _servers[s];
                }
            }
            return null;
        }

        static public ServerConfig.InstanceConfig InstanceByInstancePort(int port)
        {
            if (!_loaded) Load();

            for (int s = 0; s < _servers.Length; s++)
            {
                for (int i = 0; i < _servers[s].Instances.Length; i++)
                {
                    if (_servers[s].Instances[i].Port == port)
                        return _servers[s].Instances[i];
                }
            }
            return null;
        }
    }
}
