using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.UI;
using gView.MapServer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace gView.Server.AppCode
{
    class InternetMapServer
    {
        static public ServerMapDocument MapDocument = new ServerMapDocument();
        static public ThreadQueue<IServiceRequestContext> ThreadQueue = null;
        static internal string ServicesPath = String.Empty;
        static internal string OutputPath = String.Empty;
        static internal string OutputUrl = String.Empty;
        static internal string TileCachePath = String.Empty;
        static internal string OnlineResource = String.Empty;
        static internal List<Type> Interpreters = new List<Type>();
        static internal License myLicense = null;
        static internal List<IMapService> mapServices = new List<IMapService>();
        static internal MapServerInstance Instance = null;
        static internal Acl acl = null;

        static public void Init(string rootPath, int port = 80)
        {
            var mapServerConfig = JsonConvert.DeserializeObject<MapServerConfig>(File.ReadAllText(rootPath + "/_config/mapserver.json"));

            OutputPath = mapServerConfig.OuputPath.ToPlattformPath();
            OutputUrl = mapServerConfig.OutputUrl;
            OnlineResource = mapServerConfig.OnlineResourceUrl;

            Instance = new MapServerInstance(port);

            ServicesPath = mapServerConfig.ServiceFolder;
            foreach (var mapFileInfo in new DirectoryInfo(ServicesPath.ToPlattformPath()).GetFiles("*.mxl"))
            {
                try
                {
                    MapService service = new MapService(mapFileInfo.FullName, MapServiceType.MXL);
                    mapServices.Add(service);
                    Console.WriteLine("service " + service.Name + " added");
                }
                catch (Exception ex)
                {
                    Logger.Log(loggingMethod.error, "LoadConfig - " + mapFileInfo.Name + ": " + ex.Message);
                }
            }

            var pluginMananger = new PlugInManager();
            foreach (Type interpreterType in pluginMananger.GetPlugins(typeof(IServiceRequestInterpreter)))
            {
                Interpreters.Add(interpreterType);
            }

            ThreadQueue = new ThreadQueue<IServiceRequestContext>(Globals.MaxThreads, Globals.QueueLength);
        }

        internal static void LoadConfigAsync()
        {
            Thread thread = new Thread(new ThreadStart(LoadConfig));
            thread.Start();
        }
        internal static void LoadConfig()
        {
            try
            {
                if (MapDocument == null) return;

                DirectoryInfo di = new DirectoryInfo(ServicesPath);
                if (!di.Exists) di.Create();

                acl = new Acl(new FileInfo(ServicesPath + @"\acl.xml"));

                foreach (FileInfo fi in di.GetFiles("*.mxl"))
                {
                    try
                    {
                        if (mapServices.Count < Instance.MaxServices)
                        {
                            MapService service = new MapService(fi.FullName, MapServiceType.MXL);
                            mapServices.Add(service);
                            Console.WriteLine("service " + service.Name + " added");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(loggingMethod.error, "LoadConfig - " + fi.Name + ": " + ex.Message);
                    }
                }
                foreach (FileInfo fi in di.GetFiles("*.svc"))
                {
                    try
                    {
                        if (mapServices.Count < Instance.MaxServices)
                        {
                            MapService service = new MapService(fi.FullName, MapServiceType.SVC);
                            mapServices.Add(service);
                            Console.WriteLine("service " + service.Name + " added");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(loggingMethod.error, "LoadConfig - " + fi.Name + ": " + ex.Message);
                    }
                }

                try
                {
                    // Config Datei laden...
                    FileInfo fi = new FileInfo(ServicesPath + @"\config.xml");
                    if (fi.Exists)
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(fi.FullName);

                        #region onstart - alias
                        foreach (XmlNode serviceNode in doc.SelectNodes("MapServer/onstart/alias/services/service[@alias]"))
                        {
                            string serviceName = serviceNode.InnerText;
                            MapService ms = new MapServiceAlias(
                                serviceNode.Attributes["alias"].Value,
                                serviceName.Contains(",") ? MapServiceType.GDI : MapServiceType.SVC,
                                serviceName);
                            mapServices.Add(ms);
                        }
                        #endregion

                        #region onstart - load

                        foreach (XmlNode serviceNode in doc.SelectNodes("MapServer/onstart/load/services/service"))
                        {
                            ServiceRequest serviceRequest = new ServiceRequest(
                                serviceNode.InnerText, String.Empty);

                            ServiceRequestContext context = new ServiceRequestContext(
                                Instance,
                                null,
                                serviceRequest);

                            IServiceMap sm = Instance[context];

                            /*
                            // Initalisierung...?!
                            sm.Display.iWidth = sm.Display.iHeight = 50;
                            IEnvelope env = null;
                            foreach (IDatasetElement dsElement in sm.MapElements)
                            {
                                if (dsElement != null && dsElement.Class is IFeatureClass)
                                {
                                    if (env == null)
                                        env = new Envelope(((IFeatureClass)dsElement.Class).Envelope);
                                    else
                                        env.Union(((IFeatureClass)dsElement.Class).Envelope);
                                }
                            }
                            sm.Display.ZoomTo(env);
                            sm.Render();
                             * */
                        }
                        #endregion

                        Console.WriteLine("config.xml loaded...");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(loggingMethod.error, "LoadConfig - config.xml: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(loggingMethod.error, "LoadConfig: " + ex.Message);
            }
        }

        internal static IMap LoadMap(string name, IServiceRequestContext context)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(ServicesPath);
                if (!di.Exists) di.Create();

                FileInfo fi = new FileInfo(ServicesPath + @"\" + name + ".mxl");
                if (fi.Exists)
                {
                    ServerMapDocument doc = new ServerMapDocument();
                    doc.LoadMapDocument(fi.FullName);

                    if (doc.Maps.Count == 1)
                    {
                        ApplyMetadata(doc.Maps[0] as Map);
                        if (!MapDocument.AddMap(doc.Maps[0]))
                            return null;

                        return doc.Maps[0];
                    }
                    return null;
                }
                fi = new FileInfo(ServicesPath + @"\" + name + ".svc");
                if (fi.Exists)
                {
                    XmlStream stream = new XmlStream("");
                    stream.ReadStream(fi.FullName);
                    IServiceableDataset sds = stream.Load("IServiceableDataset", null) as IServiceableDataset;
                    if (sds != null && sds.Datasets != null)
                    {
                        Map map = new Map();
                        map.Name = name;

                        foreach (IDataset dataset in sds.Datasets)
                        {
                            if (dataset is IRequestDependentDataset)
                            {
                                if (!((IRequestDependentDataset)dataset).Open(context)) return null;
                            }
                            else
                            {
                                if (!dataset.Open()) return null;
                            }
                            //map.AddDataset(dataset, 0);

                            foreach (IDatasetElement element in dataset.Elements)
                            {
                                if (element == null) continue;
                                ILayer layer = LayerFactory.Create(element.Class, element as ILayer);
                                if (layer == null) continue;

                                map.AddLayer(layer);

                                if (element.Class is IWebServiceClass)
                                {
                                    if (map.SpatialReference == null)
                                        map.SpatialReference = ((IWebServiceClass)element.Class).SpatialReference;

                                    foreach (IWebServiceTheme theme in ((IWebServiceClass)element.Class).Themes)
                                    {
                                        map.SetNewLayerID(theme);
                                    }
                                }
                                else if (element.Class is IFeatureClass && map.SpatialReference == null)
                                {
                                    map.SpatialReference = ((IFeatureClass)element.Class).SpatialReference;
                                }
                                else if (element.Class is IRasterClass && map.SpatialReference == null)
                                {
                                    map.SpatialReference = ((IRasterClass)element.Class).SpatialReference;
                                }
                            }
                        }
                        ApplyMetadata(map);

                        if (!MapDocument.AddMap(map))
                            return null;
                        return map;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(loggingMethod.error, "LoadConfig: " + ex.Message);
            }

            return null;
        }
        private static void ApplyMetadata(Map map)
        {
            try
            {
                if (map == null) return;
                FileInfo fi = new FileInfo(ServicesPath + @"\" + map.Name + ".meta");

                IEnumerable<IMapApplicationModule> modules = null;
                if (InternetMapServer.MapDocument is IMapDocumentModules)
                {
                    modules = ((IMapDocumentModules)InternetMapServer.MapDocument).GetMapModules(map);
                }

                IServiceMap sMap = new ServiceMap(map, Instance, modules);
                XmlStream xmlStream;
                // 1. Bestehende Metadaten auf sds anwenden
                if (fi.Exists)
                {
                    xmlStream = new XmlStream("");
                    xmlStream.ReadStream(fi.FullName);
                    sMap.ReadMetadata(xmlStream);
                }
                // 2. Metadaten neu schreiben...
                xmlStream = new XmlStream("Metadata");
                sMap.WriteMetadata(xmlStream);

                if (map is Metadata)
                    ((Metadata)map).Providers = sMap.Providers;

                // Overriding: no good idea -> problem, if multiple instances do this -> killing the metadata file!!!
                fi.Refresh();
                if (!fi.Exists)
                {
                    xmlStream.WriteStream(fi.FullName);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(loggingMethod.error, "LoadConfig: " + ex.Message);
            }
        }
        static public void SaveConfig(IMap map)
        {
            try
            {
                if (MapDocument == null) return;

                ServerMapDocument doc = new ServerMapDocument();
                if (!doc.AddMap(map))
                    return;

                XmlStream stream = new XmlStream("MapServer");
                stream.Save("MapDocument", doc);

                stream.WriteStream(ServicesPath + @"\" + map.Name + ".mxl");

                if (map is Map)
                    ApplyMetadata((Map)map);
            }
            catch (Exception ex)
            {
                Logger.Log(loggingMethod.error, "LoadConfig: " + ex.Message);
            }
        }

        static public void SaveServiceableDataset(IServiceableDataset sds, string name)
        {
            try
            {
                if (sds != null)
                {
                    XmlStream stream = new XmlStream("MapServer");
                    stream.Save("IServiceableDataset", sds);

                    stream.WriteStream(ServicesPath + @"\" + name + ".svc");

                    if (sds is IMetadata)
                    {
                        stream = new XmlStream("Metadata");
                        ((IMetadata)sds).WriteMetadata(stream);
                        stream.WriteStream(ServicesPath + @"\" + name + ".svc.meta");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(loggingMethod.error, "LoadConfig: " + ex.Message);
            }
        }

        static public void SaveServiceCollection(string name, XmlStream stream)
        {
            stream.WriteStream(ServicesPath + @"\" + name + ".scl");
        }

        static public bool RemoveConfig(string mapName)
        {
            try
            {
                FileInfo fi = new FileInfo(ServicesPath + @"\" + mapName + ".mxl");
                if (fi.Exists) fi.Delete();
                fi = new FileInfo(ServicesPath + @"\" + mapName + ".svc");
                if (fi.Exists) fi.Delete();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(loggingMethod.error, "LoadConfig: " + ex.Message);
                return false;
            }
        }

        static internal void mapDocument_MapAdded(IMap map)
        {
            try
            {
                Console.WriteLine("Map Added:" + map.Name);

                foreach (IDatasetElement element in map.MapElements)
                {
                    Console.WriteLine("     >> " + element.Title + " added");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(loggingMethod.error, "LoadConfig: " + ex.Message);
            }
        }
        static internal void mapDocument_MapDeleted(IMap map)
        {
            try
            {
                Console.WriteLine("Map Removed: " + map.Name);
            }
            catch (Exception ex)
            {
                Logger.Log(loggingMethod.error, "LoadConfig: " + ex.Message);
            }
        }

        static internal IServiceRequestInterpreter GetInterpreter(Type type)
        {
            var interpreter = new PlugInManager().CreateInstance<IServiceRequestInterpreter>(type);
            if (interpreter == null)
                throw new Exception("Can't intialize interperter");

            interpreter.OnCreate(Instance);
            return interpreter;
        }

        static internal IServiceRequestInterpreter GetInterpreter(Guid guid)
        {
            var interpreter = new PlugInManager().CreateInstance(guid) as IServiceRequestInterpreter;
            if (interpreter == null)
                throw new Exception("Can't intialize interperter");

            interpreter.OnCreate(Instance);
            return interpreter;
        }

        #region Manage

        static public bool AddMap(string mapName, string MapXML, string usr, string pwd)
        {
            if (String.IsNullOrEmpty(MapXML))
            {
                return ReloadMap(mapName, usr, pwd);
            }

            if (InternetMapServer.acl != null && !InternetMapServer.acl.HasAccess(Identity.FromFormattedString(usr), pwd, "admin_addmap"))
                return false;

            if (InternetMapServer.Instance.Maps.Count >= InternetMapServer.Instance.MaxServices)
            {
                // Überprüfen, ob schon eine Service mit gleiche Namen gibt...
                // wenn ja, ist es nur einem Refresh eines bestehenden Services
                bool found = false;
                foreach (IMapService map in InternetMapServer.Instance.Maps)
                {
                    if (map.Name == mapName)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return false;
            }

            if (MapXML.IndexOf("<IServiceableDataset") == 0)
            {
                XmlStream xmlStream = new XmlStream("");

                StringReader sr = new StringReader("<root>" + MapXML + "</root>");
                if (!xmlStream.ReadStream(sr)) return false;

                IServiceableDataset sds = xmlStream.Load("IServiceableDataset", null) as IServiceableDataset;
                if (sds != null && sds.Datasets != null)
                {
                    InternetMapServer.SaveServiceableDataset(sds, mapName);
                    AddMapService(mapName, MapServiceType.SVC);
                }
            }
            else if (MapXML.IndexOf("<ServiceCollection") == 0)
            {
                XmlStream stream = new XmlStream("");

                StringReader sr = new StringReader(MapXML);
                if (!stream.ReadStream(sr)) return false;

                InternetMapServer.SaveServiceCollection(mapName, stream);
            }
            else  // Map
            {
                XmlStream xmlStream = new XmlStream("IMap");

                using (StringReader sr = new StringReader(MapXML))
                    if (!xmlStream.ReadStream(sr)) return false;

                Map map = new Map();
                map.Load(xmlStream);
                map.Name = mapName;

                XmlStream pluginStream = new XmlStream("Moduls");
                using (StringReader sr = new StringReader(MapXML))
                    if (!xmlStream.ReadStream(sr)) return false;

                ModulesPersists modules = new ModulesPersists(map);
                modules.Load(pluginStream);

                //foreach (IMap m in ListOperations<IMap>.Clone(_doc.Maps))
                //{
                //    if (m.Name == map.Name) _doc.RemoveMap(m);
                //}

                //if (!_doc.AddMap(map)) return false;
                AddMapService(mapName, MapServiceType.MXL);

                InternetMapServer.SaveConfig(map);
            }

            return true;
        }
        static public bool RemoveMap(string mapName, string usr, string pwd)
        {
            if (InternetMapServer.acl != null && !InternetMapServer.acl.HasAccess(Identity.FromFormattedString(usr), pwd, "admin_removemap"))
                return false;

            bool found = false;

            foreach (IMapService service in ListOperations<IMapService>.Clone(InternetMapServer.mapServices))
            {
                if (service.Name == mapName)
                {
                    InternetMapServer.mapServices.Remove(service);
                    found = true;
                }
            }

            foreach (IMapService m in ListOperations<IMapService>.Clone(InternetMapServer.Instance.Maps))
            {
                if (m.Name == mapName)
                {
                    //_doc.RemoveMap(m);
                    found = true;
                }
            }
            InternetMapServer.RemoveConfig(mapName);

            return found;
        }

        static public bool ReloadMap(string mapName, string usr, string pwd)
        {
            if (InternetMapServer.acl != null && !InternetMapServer.acl.HasAccess(Identity.FromFormattedString(usr), pwd, mapName))
                return false;

            // ToDo: needfull?

            /*
            if (_doc == null) return false;

            // Remove map(s) (GDI) from Document
            List<IMap> maps = new List<IMap>();
            foreach (IMap m in _doc.Maps)
            {
                if (mapName.Contains(","))   // wenn ',' -> nur GDI Service suchen
                {
                    if (mapName == m.Name)
                    {
                        maps.Add(m);
                        break;
                    }
                }
                else   // sonst alle Services (incl GDI) suchen und entfernen
                {
                    foreach (string name in m.Name.Split(','))
                    {
                        if (mapName == name)
                        {
                            maps.Add(m);
                            break;
                        }
                    }
                }
            }

            if (maps.Count == 0)
            {
                // Reload map...
                IServiceMap smap = IMS.mapServer[mapName];
            }
            else
            {
                foreach (IMap m in maps)
                {
                    _doc.RemoveMap(m);
                    // Reload map(s) (GDI)...
                    IServiceMap smap = IMS.mapServer[m.Name];
                }
            }

    */
            return true;
        }

        static public string GetMetadata(string mapName, string usr, string pwd)
        {
            if (InternetMapServer.acl != null && !InternetMapServer.acl.HasAccess(Identity.FromFormattedString(usr), pwd, "admin_metadata_get"))
                return "ERROR: Not Authorized!";

            if (!ReloadMap(mapName, usr, pwd)) return String.Empty;

            //if (IMS.mapServer == null || IMS.mapServer[mapName] == null)
            //    return String.Empty;

            FileInfo fi = new FileInfo((InternetMapServer.ServicesPath + @"/" + mapName + ".meta").ToPlattformPath());
            if (!fi.Exists) return String.Empty;

            using (StreamReader sr = new StreamReader(fi.FullName.ToPlattformPath()))
            {
                string ret = sr.ReadToEnd();
                sr.Close();
                return ret;
            }
        }
        static public bool SetMetadata(string mapName, string metadata, string usr, string pwd)
        {
            if (InternetMapServer.acl != null && !InternetMapServer.acl.HasAccess(Identity.FromFormattedString(usr), pwd, "admin_metadata_set"))
                return false;

            FileInfo fi = new FileInfo(InternetMapServer.ServicesPath + @"/" + mapName + ".meta");

            StringReader sr = new StringReader(metadata);
            XmlStream xmlStream = new XmlStream("");
            xmlStream.ReadStream(sr);
            xmlStream.WriteStream(fi.FullName);

            return ReloadMap(mapName, usr, pwd);
        }

        static private void AddMapService(string mapName, MapServiceType type)
        {
            foreach (IMapService service in InternetMapServer.mapServices)
            {
                if (service.Name == mapName)
                    return;
            }
            InternetMapServer.mapServices.Add(new MapService(mapName, type));
        }

        #endregion

        #region Http

        static public string AppRootUrl(HttpRequest request)
        {
            //string url = String.Format("{0}{1}{2}", UrlScheme(request, httpSchema), request..Authority, urlHelper.UrlContent("~"));
            //if (url.EndsWith("/"))
            //    return url.Substring(0, url.Length - 1);

            //return url;

            return $"{request.Scheme}://{request.Host}{request.PathBase}";
        }

        #endregion

        #region  ToDo: Diese Konfiguration sollte dann eigentlich im Init() passieren ... Komentar derweil noch nicht löschen

        //static internal int Port
        //{
        //    get { return _port; }
        //    set
        //    {
        //        _port = value;
        //        MapServerConfig.ServerConfig serverConfig = MapServerConfig.ServerByInstancePort(_port);
        //        ServicesPath = gView.Framework.system.SystemVariables.MyCommonApplicationData + @"\mapServer\Services\" + serverConfig.Port;

        //        try
        //        {
        //            MapServerConfig.ServerConfig.InstanceConfig InstanceConfig = MapServerConfig.InstanceByInstancePort(value);
        //            if (serverConfig != null && InstanceConfig != null)
        //            {
        //                OutputPath = serverConfig.OutputPath.Trim();
        //                OutputUrl = serverConfig.OutputUrl.Trim();
        //                TileCachePath = serverConfig.TileCachePath.Trim();

        //                Globals.MaxThreads = InstanceConfig.MaxThreads;
        //                Globals.QueueLength = InstanceConfig.MaxQueueLength;

        //                Logger.Log(loggingMethod.request, "Output Path: '" + OutputPath + "'");
        //                Logger.Log(loggingMethod.request, "Output Url : '" + OutputUrl + "'");
        //            }
        //            ThreadQueue = new ThreadQueue<IServiceRequestContext>(Globals.MaxThreads, Globals.QueueLength);
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.Log(loggingMethod.error, "IMS.Port(set): " + ex.Message + "\r\n" + ex.StackTrace);
        //        }
        //    }
        //}

        #endregion
    }
}
