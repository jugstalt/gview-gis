using gView.Core.Framework.Exceptions;
using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system;
using gView.Framework.UI;
using gView.MapServer;
using System;
using System.Collections.Generic;
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
                {
                    throw new MapServerException("Fatal error: map(server) document missing");
                }

                folder = folder ?? String.Empty;

                object locker = null;
                lock (_lockThis)
                {
                    if (!_lockers.ContainsKey(name))
                    {
                        _lockers.Add(name, new object());
                    }

                    locker = _lockers[name];
                }

                IMapService mapService = GetMapService(name, folder);
                if (mapService == null || (await mapService.GetSettingsAsync()).Status != MapServiceStatus.Running)
                {
                    throw new MapServerException("unknown service: " + name);
                }

                if (await mapService.RefreshRequired())
                {
                    InternetMapServer.MapDocument.RemoveMap(mapService.Fullname);
                }

                //lock (locker)
                {
                    string alias = name;

                    if (mapService is MapServiceAlias)
                    {
                        name = ((MapServiceAlias)mapService).ServiceName;
                    }

                    if (!String.IsNullOrWhiteSpace(folder))
                    {
                        name = folder + "/" + name;
                    }

                    return await FindServiceMap(name, alias, context);
                }
            }
            catch (MapServerException mse)
            {
                throw mse;
            }
            catch (Exception ex)
            {
                await LogAsync(ToMapName(name, folder), "MapServer.Map", loggingMethod.error, ex.Message + "\n" + ex.StackTrace);
                throw new MapServerException("unknown error");
            }
        }

        #region IMapServer Member

        async public Task<IEnumerable<IMapService>> Maps(IIdentity identity)
        {
            try
            {
                if (identity == null)
                {
                    return InternetMapServer.mapServices.ToArray();
                }

                List<IMapService> services = new List<IMapService>();
                foreach (var service in InternetMapServer.mapServices)
                {
                    if (await service.HasAnyAccess(identity))
                    {
                        services.Add(service);
                    }
                }

                return services;
                //return  await
                //    InternetMapServer.mapServices
                //    .Where(async s => true == await s.HasAnyAccess(identity));
            }
            catch (MapServerException mse)
            {
                throw mse;
            }
            catch (Exception ex)
            {
                await LogAsync(String.Empty, "MapServer.Map", loggingMethod.error, ex.Message + "\n" + ex.StackTrace);
                //return new List<IMapService>();
                throw new MapServerException("unknown error");
            }
        }

        async public Task<IServiceMap> GetServiceMapAsync(string name, string folder)
        {
            return await this.Map(name, folder, null);
        }
        async public Task<IServiceMap> GetServiceMapAsync(IMapService service)
        {
            if (service == null)
            {
                return null;
            }

            return await GetServiceMapAsync(service.Name, service.Folder);
        }
        async public Task<IServiceMap> GetServiceMapAsync(IServiceRequestContext context)
        {
            try
            {
                if (context == null || context.ServiceRequest == null)
                {
                    return null;
                }

                IServiceMap map = await this.Map(context.ServiceRequest.Service, context.ServiceRequest.Folder, context);
                if (map is ServiceMap)
                {
                    ((ServiceMap)map).SetRequestContext(context);
                }

                return map;
            }
            catch (MapServerException mse)
            {
                throw mse;
            }
            catch (Exception ex)
            {
                await LogAsync(ToMapName(context?.ServiceRequest?.Service, context?.ServiceRequest?.Folder), "MapServer.Map", loggingMethod.error, ex.Message + "\n" + ex.StackTrace);
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
        async public Task LogAsync(string mapName, string header, loggingMethod method, string msg)
        {
            if (!LoggingEnabled(method))
            {
                return;
            }

            switch (method)
            {
                case loggingMethod.error:
                    if (!String.IsNullOrEmpty(header))
                    {
                        msg = header + "\n" + msg;
                    }

                    await Logger.LogAsync(mapName, method, msg);
                    break;
                case loggingMethod.request:
                    if (!String.IsNullOrEmpty(header))
                    {
                        msg = header + " - " + msg;
                    }

                    await Logger.LogAsync(mapName, method, msg);
                    break;
                case loggingMethod.request_detail:
                    if (!String.IsNullOrEmpty(header))
                    {
                        msg = header + "\n" + msg;
                    }

                    await Logger.LogAsync(mapName, method, msg);
                    break;
                case loggingMethod.request_detail_pro:
                    if (!String.IsNullOrEmpty(header))
                    {
                        header = header.Replace(".", "_");
                    }

                    await Logger.LogAsync(mapName, method, msg);
                    break;
            }
        }

        async public Task LogAsync(IServiceRequestContext context, string header, loggingMethod method, string msg)
        {
            if (!LoggingEnabled(method))
            {
                return;
            }

            await LogAsync(ToMapName(context?.ServiceRequest?.Service, context?.ServiceRequest?.Folder), header, method, msg);
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

        //public bool CheckAccess(IIdentity identity, string service)
        //{
        //    if (InternetMapServer.acl == null) return true;
        //    return InternetMapServer.acl.HasAccess(identity, null, service);
        //}

        public int FeatureQueryLimit => 1000;

        #endregion

        //private bool ServiceExists(string name)
        //{
        //    foreach (IMapService service in InternetMapServer.mapServices)
        //    {
        //        if (service == null) continue;
        //        if (service.Name == name) return true;
        //    }
        //    return false;
        //}

        async private Task<IServiceMap> FindServiceMap(string name, string alias, IServiceRequestContext context)
        {
            Map map = await FindMap(name, context);
            if (map != null)
            {
                IEnumerable<IMapApplicationModule> modules = null;
                if (InternetMapServer.MapDocument is IMapDocumentModules)
                {
                    modules = ((IMapDocumentModules)InternetMapServer.MapDocument).GetMapModules(map);
                }
                return await ServiceMap.CreateAsync(map, this, modules);
            }

            if (name.Contains(",") /* && _serverLicType == ServerLicType.gdi*/)
            {
                Map newMap = null;

                string[] names = name.Split(',');

                #region Alias Service auflösen...
                StringBuilder sb = new StringBuilder();
                foreach (string n in names)
                {
                    IMapService ms = GetMapService(n.ServiceName(), n.FolderName());
                    if (ms == null)
                    {
                        return null;
                    }

                    if (sb.Length > 0)
                    {
                        sb.Append(",");
                    }

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
                                tocElement.Layers == null)
                            {
                                continue;
                            }

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
                    {
                        return null;
                    }

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

                    return await ServiceMap.CreateAsync(newMap, this, null);
                }
            }

            return null;
        }

        public IMapService GetMapService(string name, string folder)
        {
            foreach (IMapService ms in InternetMapServer.mapServices)
            {
                if (ms == null)
                {
                    continue;
                }

                if (ms.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) &&
                    ms.Folder.Equals(folder ?? String.Empty, StringComparison.CurrentCultureIgnoreCase))
                {
                    return ms;
                }
            }

            return InternetMapServer.TryAddService(name, folder);
        }

        async private Task<Map> FindMap(string name, IServiceRequestContext context)
        {
            foreach (IMap map in InternetMapServer.MapDocument.Maps)
            {
                if (map.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && map is Map)
                {
                    return (Map)map;
                }
            }

            if (name.Contains(","))
            {
                return null;
            }

            IMap m = await InternetMapServer.LoadMap(name);
            if (m is Map)
            {
                return (Map)m;
            }

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

        private string ToMapName(string name, string folder)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                return String.Empty;
            }

            if (!String.IsNullOrWhiteSpace(folder))
            {
                name = folder + "/" + name;
            }

            return name;
        }
    }
}
