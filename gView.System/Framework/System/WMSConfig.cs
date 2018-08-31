using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Xml;
using gView.MapServer;

namespace gView.Framework.system
{
    public class WMSConfig
    {
        static private string _onlineResource = "http://localhost/gViewPortal/wms.aspx?";
        static private string _srs = "epsg:4326";
        static private string _imageFormat = "image/png;image/jpeg";
        static private string _getFeatureInfoFormat = "text/plain;text/html;gml;text/xml";

        static private bool _loaded = false;

        static public void Load()
        {
            _loaded = true;

            try
            {
                //RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\gViewGisOS\WMS", false);
                //if (key == null)
                //{
                //    key = Registry.LocalMachine.CreateSubKey(@"Software\gViewGisOS\WMS", RegistryKeyPermissionCheck.ReadWriteSubTree);

                //    key.SetValue("OnlineResource", OnlineResource);
                //    key.SetValue("SRS", SRS);
                //    key.SetValue("ImageFormats", ImageFormats);
                //    key.SetValue("GetFeatureInfoFormats", GetFeatureInfoFormats);
                //}

                //OnlineResource = (string)key.GetValue("OnlineResource", "");
                //SRS = (string)key.GetValue("SRS", "");
                //ImageFormats = (string)key.GetValue("ImageFormats", "");
                //GetFeatureInfoFormats = (string)key.GetValue("GetFeatureInfoFormats", "");
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                _loaded = false;
            }
        }

        static public string OnlineResource
        {
            get
            {
                if (!_loaded) Load();
                return _onlineResource;
            }
            set { _onlineResource = value; }
        }
        static public string SRS
        {
            get
            {
                if (!_loaded) Load();
                return _srs;
            }
            set { _srs = value; }
        }
        static public string ImageFormats
        {
            get
            {
                if (!_loaded) Load();
                return _imageFormat;
            }
            set { _imageFormat = value; }
        }
        static public string GetFeatureInfoFormats
        {
            get
            {
                if (!_loaded) Load();
                return _getFeatureInfoFormat;
            }
            set { _getFeatureInfoFormat = value; }
        }

        static public void Commit()
        {
            if (!_loaded) Load();

            try
            {
                //RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\gViewGisOS\WMS", true);
                
                //key.SetValue("OnlineResource", OnlineResource);
                //key.SetValue("SRS", SRS);
                //key.SetValue("ImageFormats", ImageFormats);
                //key.SetValue("GetFeatureInfoFormats", GetFeatureInfoFormats);

                //key.Close();
                //Load();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                _loaded = false;
            }
        }
    }

    /*
    public class MapServerConnectors
    {
        static Dictionary<Guid, Connector> _connectors = new Dictionary<Guid, Connector>();
        static private bool _loaded = false;

        static public void Load()
        {
            PlugInManager compMan = new PlugInManager();
            if (compMan.ServiceRequestInterpreter == null) return;

            _loaded = true;
            _connectors.Clear();

            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\gViewGisOS\MapServerConnectors", false);
            if (key == null)
            {
                key = Registry.LocalMachine.CreateSubKey(@"Software\gViewGisOS\MapServerConnectors", RegistryKeyPermissionCheck.ReadWriteSubTree);
            }
            key.Close();

            foreach (XmlNode compNode in compMan.ServiceRequestInterpreter)
            {
                IServiceRequestInterpreter interpreter = compMan.CreateInstance(compNode) as IServiceRequestInterpreter;
                if (interpreter == null) continue;

                _connectors.Add(PlugInManager.PlugInID(interpreter), new Connector(interpreter));
            }
        }

        static public string OnlineResource(Guid guid)
        {
            if (!_loaded) Load();

            Connector connector;
            if (_connectors.TryGetValue(guid, out connector))
            {
                return connector.OnlineResource;
            }

            return String.Empty;
        }
        static public void SetOnlineResource(Guid guid, string connectorUrl)
        {
            foreach (Connector connector in Connectors)
            {
                if (connector.GUID.Equals(guid))
                {
                    connector.OnlineResource = connectorUrl;
                    return;
                }
            }
        }
        static public List<Connector> Connectors
        {
            get
            {
                if (!_loaded) Load();

                List<Connector> connectors = new List<Connector>();
                foreach (Guid guid in _connectors.Keys)
                {
                    connectors.Add(_connectors[guid]);
                }
                return connectors;
            }
        }

        static public void Commit()
        {
            if (!_loaded) Load();

            foreach (Guid guid in _connectors.Keys)
            {
                _connectors[guid].Commit();
            }

            Load();
        }

        #region HelperClass
        public class Connector
        {
            private string _OnlineResource = String.Empty;
            private Guid _guid;
            private string _className = String.Empty;

            internal Connector(IServiceRequestInterpreter interpreter)
            {
                _guid = PlugInManager.PlugInID(interpreter);
                _className = interpreter.ToString();

                Load();
            }

            private void Load()
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\gViewGisOS\MapServerConnectors\" + _guid.ToString(), false);
                if (key == null)
                {
                    key = Registry.LocalMachine.CreateSubKey(@"Software\gViewGisOS\MapServerConnectors\" + _guid.ToString(), RegistryKeyPermissionCheck.ReadWriteSubTree);
                }

                bool found = false;
                foreach (string subKeyName in key.GetValueNames())
                {
                    if (subKeyName == "OnlineResource")
                    {
                        found = true;
                        break;
                    }
                }
                key.Close();

                if (!found)
                {
                    switch (_guid.ToString().ToUpper())
                    {
                        case "C4892F49-446C-4E22-BCC7-76F033F1F03B":  // WMS
                        case "391FA941-3E31-456E-8A3A-703E07962BA6":  // WFS
                            this.OnlineResource = "http://localhost/gViewConnector/ogc.aspx";
                            break;
                        case "F2179EEB-DD01-433F-8A9F-4560131F1BFE":
                        case "1242DC9C-EF07-48AF-98F9-16D90082B888":  // WMS
                            this.OnlineResource = "http://localhost/gViewConnector/ogc2.aspx";
                            break;
                        case "BB294D9C-A184-4129-9555-398AA70284BC":
                            this.OnlineResource = "http://localhost/gViewConnector/axl.aspx";
                            break;
                        case "DC58F436-9E1E-444B-8E09-EA2D1E0F5C30":
                            this.OnlineResource = "http://localhost/gViewConnector/axl2.aspx";
                            break;
                        default:
                            this.OnlineResource = "";
                            break;
                    }
                    Commit();
                }

                key = Registry.LocalMachine.OpenSubKey(@"Software\gViewGisOS\MapServerConnectors\" + _guid.ToString(), false);
                _OnlineResource = (string)key.GetValue("OnlineResource","");
                key.Close();
            }

            public Guid GUID { get { return _guid; } }
            public string TypeName { get { return _className; } }
            public string OnlineResource
            {
                get
                {
                    return _OnlineResource;
                }
                set
                {
                    _OnlineResource = value;
                }
            }

            public void Commit()
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\gViewGisOS\MapServerConnectors\" + _guid.ToString(), true);
                key.SetValue("OnlineResource", _OnlineResource);
                key.SetValue("TypeName", _className);
                key.Close();
            }
        }
        #endregion
    }
     * */
}
