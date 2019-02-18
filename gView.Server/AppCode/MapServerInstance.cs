using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.UI;
using gView.MapServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    public class MapServerInstance : IMapServer
    {
        public enum ServerLicType { Unknown = 0, Private = 1, Professional = 2, Express = 3 };
        private IMapDocument _doc;
        private bool _log_requests, _log_request_details, _log_errors;
        private object _lockThis = new object();
        private Dictionary<string, object> _lockers = new Dictionary<string, object>();
        //private int _maxGDIServers = int.MaxValue;
        private int _maxServices = int.MaxValue;
            
        public MapServerInstance(int port)
        {
            _log_requests = Globals.log_requests;
            _log_request_details = Globals.log_request_details;
            _log_errors = Globals.log_errors;
        }

        async private Task<IServiceMap> Map(string name, string folder, IServiceRequestContext context)
        {
            try
            {
                if (InternetMapServer.MapDocument == null)
                    throw new MapServerException("Fatal error: map(server) document missing");

                folder = folder ?? String.Empty;

                object locker = null;
                lock (_lockThis)
                {
                    if (!_lockers.ContainsKey(name))
                        _lockers.Add(name, new object());
                    locker = _lockers[name];
                }

                IMapService mapService = FindMapService(name, folder);
                if (mapService == null || (await mapService.GetSettingsAsync()).Status != MapServiceStatus.Running)
                    throw new MapServerException("unknown service: " + name);

                if (await mapService.RefreshRequired())
                {
                    InternetMapServer.MapDocument.RemoveMap(mapService.Fullname);
                }

                lock (locker)
                {
                    string alias = name;

                    if (mapService is MapServiceAlias)
                    {
                        name = ((MapServiceAlias)mapService).ServiceName;
                    }

                    if (!String.IsNullOrWhiteSpace(folder))
                        name = folder + "/" + name;

                    return FindServiceMap(name, alias, context).Result;
                }
            }
            catch(MapServerException mse)
            {
                throw mse;
            }
            catch (Exception ex)
            {
                Log("MapServer.Map", loggingMethod.error, ex.Message + "\n" + ex.StackTrace);
                throw new MapServerException("unknown error");
            }
        }

        #region IMapServer Member

        public List<IMapService> Maps
        {
            get
            {
                try
                {
                    return ListOperations<IMapService>.Clone(InternetMapServer.mapServices);
                }
                catch (MapServerException mse)
                {
                    throw mse;
                }
                catch (Exception ex)
                {
                    Log("MapServer.Map", loggingMethod.error, ex.Message + "\n" + ex.StackTrace);
                    //return new List<IMapService>();
                    throw new MapServerException("unknown error");
                }
            }
        }

        async public Task<IServiceMap> GetServiceMap(string name, string folder)
        {
            return await this.Map(name, folder, null);
        }
        async public Task<IServiceMap> GetServiceMap(IMapService service)
        {
                if (service == null)
                    return null;
                return await GetServiceMap(service.Name, service.Folder);
        }
        async public Task<IServiceMap> GetServiceMap(IServiceRequestContext context)
        {
            try
            {
                if (context == null || context.ServiceRequest == null) return null;
                IServiceMap map = await this.Map(context.ServiceRequest.Service, context.ServiceRequest.Folder, context);
                if (map is ServiceMap)
                    ((ServiceMap)map).SetRequestContext(context);

                return map;
            }
            catch(MapServerException mse)
            {
                throw mse;
            }
            catch (Exception ex)
            {
                Log("MapServer.Map", loggingMethod.error, ex.Message + "\n" + ex.StackTrace);
                throw new MapServerException("unknown error");
            }
        }

        public bool LoggingEnabled(loggingMethod method)
        {
            switch (method)
            {
                case loggingMethod.error:
                    return _log_errors;
                case loggingMethod.request:
                    return _log_requests;
                case loggingMethod.request_detail:
                case loggingMethod.request_detail_pro:
                    return _log_request_details;
            }

            return false;
        }
        public void Log(string header, loggingMethod method, string msg)
        {
            switch (method)
            {
                case loggingMethod.error:
                    if (_log_errors)
                    {
                        if (!String.IsNullOrEmpty(header))
                            msg = header + "\n" + msg;
                        Logger.Log(method,  msg);
                    }
                    break;
                case loggingMethod.request:
                    if (_log_requests)
                    {
                        if (!String.IsNullOrEmpty(header))
                            msg = header + " - " + msg;
                        Logger.Log(method,  msg);
                    }
                    break;
                case loggingMethod.request_detail:
                    if (_log_request_details)
                    {
                        if (!String.IsNullOrEmpty(header))
                            msg = header + "\n" + msg;
                        Logger.Log(method,  msg);
                    }
                    break;
                case loggingMethod.request_detail_pro:
                    if (_log_request_details)
                    {
                        if (!String.IsNullOrEmpty(header))
                            header = header.Replace(".", "_");
                        Logger.Log(method, msg);
                    }
                    break;
            }
        }

        public string OutputUrl
        {
            get
            {
                return Globals.OutputUrl;
            }
        }

        public string OutputPath
        {
            get
            {
                return Globals.OutputPath;
            }
        }

        public string TileCachePath
        {
            get
            {
                return Globals.TileCachePath;
            }
        }

        public bool CheckAccess(IIdentity identity, string service)
        {
            if (InternetMapServer.acl == null) return true;
            return InternetMapServer.acl.HasAccess(identity, null, service);
        }

        #endregion

        private bool ServiceExists(string name)
        {
            foreach (IMapService service in InternetMapServer.mapServices)
            {
                if (service == null) continue;
                if (service.Name == name) return true;
            }
            return false;
        }

        async private Task<IServiceMap> FindServiceMap(string name, string alias, IServiceRequestContext context)
        {
            Map map = await FindMap(name, context);
            if (map != null)
            {
                IEnumerable<IMapApplicationModule> modules = null;
                if(InternetMapServer.MapDocument is IMapDocumentModules)
                {
                    modules = ((IMapDocumentModules)InternetMapServer.MapDocument).GetMapModules(map);
                }
                return new ServiceMap(map, this, modules);
            }

            if (name.Contains(",") /* && _serverLicType == ServerLicType.gdi*/)
            {
                Map newMap = null;

                string[] names = name.Split(',');

                #region Alias Service auflösen...
                StringBuilder sb = new StringBuilder();
                foreach (string n in names)
                {
                    IMapService ms = FindMapService(n.ServiceName(), n.FolderName());
                    if (ms == null) return null;

                    if (sb.Length > 0) sb.Append(",");
                    if (ms is MapServiceAlias)
                    {
                        sb.Append(((MapServiceAlias)ms).ServiceName);
                    }
                    else
                    {
                        sb.Append(ms.Name);
                    }
                }
                names = sb.ToString().Split(',');
                #endregion
                Array.Reverse(names);

                //if (names.Length > _maxGDIServices)
                //{
                //    return null;
                //}

                foreach (string n in names)
                {
                    Map m1 = await FindMap(n, context);
                    if (m1.Name == n)
                    {
                        if (newMap == null)
                        {
                            newMap = new Map(m1, true);
                        }
                        else
                        {
                            newMap.Append(m1, true);

                            // SpatialReference von am weitesten unten liegenden Karte übernehmen
                            // ist geschackssache...
                            if (m1.Display != null && m1.Display.SpatialReference != null)
                            {
                                newMap.Display.SpatialReference =
                                    m1.Display.SpatialReference.Clone() as ISpatialReference;
                            }
                        }
                    }
                }
                if (newMap != null)
                {
                    // alle webServiceThemes im TOC vereinigen...
                    if (newMap.TOC != null)
                    {
                        foreach (ITOCElement tocElement in newMap.TOC.Elements)
                        {
                            if (tocElement == null ||
                                tocElement.Layers == null) continue;

                            foreach (ILayer layer in tocElement.Layers)
                            {
                                if (layer is IWebServiceLayer)
                                {
                                    newMap.TOC.RenameElement(tocElement, newMap.Name + "_WebThemes");
                                    break;
                                }
                            }
                        }
                    }
                    newMap.Name = alias;
                    if (!InternetMapServer.MapDocument.AddMap(newMap))
                        return null;

                    bool found = false;
                    foreach (IMapService ms in InternetMapServer.mapServices)
                    {
                        if (ms != null &&
                            ms.Name == alias && ms.Type == MapServiceType.GDI)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        InternetMapServer.mapServices.Add(new MapService(name, String.Empty, MapServiceType.GDI));
                    }

                    return new ServiceMap(newMap, this, null);
                }
            }

            return null;
        }

        private IMapService FindMapService(string name, string folder)
        {
            foreach (IMapService ms in InternetMapServer.mapServices)
            {
                if (ms == null) continue;
                if (ms.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) &&
                    ms.Folder.Equals(folder ?? String.Empty, StringComparison.CurrentCultureIgnoreCase)) return ms;
            }
            return null;
        }

        async private Task<Map> FindMap(string name, IServiceRequestContext context)
        {
            foreach (IMap map in InternetMapServer.MapDocument.Maps)
            {
                if (map.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && map is Map)
                    return (Map)map;
            }

            if (name.Contains(",")) return null;

            IMap m = await InternetMapServer.LoadMap(name, context);
            if (m is Map)
                return (Map)m;

            return null;
        }

        //internal int MaxGDIServers
        //{
        //    get { return _maxGDIServers; }
        //}

        internal int MaxServices
        {
            get { return _maxServices; }
        }
    }
}
