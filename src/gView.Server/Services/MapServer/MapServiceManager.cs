using gView.Framework.Core.MapServer;
using gView.Framework.IO;
using gView.Framework.Common;
using gView.Server.AppCode;
using gView.Server.Services.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using gView.Framework.Core.Carto;
using gView.Framework.Cartography;

namespace gView.Server.Services.MapServer
{
    public class MapServiceManager
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly MapServiceAccessService _accessService;

        public MapServiceManager(
            IServiceProvider serviceProvider,
            MapServiceAccessService accessService,
            IOptions<MapServerManagerOptions> optionsMonitor,
            ILogger<MapServiceManager> logger = null)
        {
            _serviceProvider = serviceProvider;
            _accessService = accessService;
            Options = optionsMonitor.Value;
            _logger = logger ?? new ConsoleLogger<MapServiceManager>();

            if (Options.IsValid)
            {
                foreach (string createDirectroy in new string[] {
                    Options.ServicesPath,
                    Options.LoginManagerRootPath,
                    $"{ Options.LoginManagerRootPath }/manage",
                    $"{ Options.LoginManagerRootPath }/token",
                    Options.LoggingRootPath
                })
                {
                    if (!new DirectoryInfo(createDirectroy).Exists)
                    {
                        new DirectoryInfo(createDirectroy).Create();
                    }
                }

                AddServices(String.Empty);

                var pluginMananger = new PlugInManager();
                Interpreters = pluginMananger.GetPlugins(typeof(IServiceRequestInterpreter))
                    .OrderByDescending(t => ((IServiceRequestInterpreter)Activator.CreateInstance(t)).Priority)
                    .ToArray();

                TaskQueue = new TaskQueue<IServiceRequestContext>(Options.TaskQueue_MaxThreads, Options.TaskQueue_QueueLength);
            } 
            else
            {
                logger.Log(LogLevel.Error, "mapserver configuration is invalid (mapserver.json)");
            }
        }

        // Singleton
        private MapServerInstance _instance;
        public MapServerInstance Instance
        {
            get
            {
                if (_instance == null)
                {
                    var msds = (MapServiceDeploymentManager)_serviceProvider.GetService(typeof(MapServiceDeploymentManager));
                    var logger = (MapServicesEventLogger)_serviceProvider.GetService(typeof(MapServicesEventLogger));

                    _instance = new MapServerInstance(this, msds, _accessService, logger, Options.Port);
                }

                return _instance;
            }
        }

        public readonly MapServerManagerOptions Options;
        public readonly TaskQueue<IServiceRequestContext> TaskQueue;
        public readonly Type[] Interpreters;
        public ConcurrentBag<IMapService> MapServices = new ConcurrentBag<IMapService>();

        private void AddServices(string folder)
        {

            foreach (var mapFileInfo in new DirectoryInfo((Options.ServicesPath + "/" + folder).ToPlatformPath()).GetFiles("*.mxl"))
            {
                string mapName = String.Empty;
                try
                {
                    if (TryAddService(mapFileInfo, folder) == null)
                    {
                        throw new Exception("unable to load servive: " + mapFileInfo.FullName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Service {mapName}: loadConfig - {mapFileInfo.Name}: {ex.Message}");
                }
            }

            #region Add Folders on same level

            foreach (var folderDirectory in new DirectoryInfo((Options.ServicesPath + "/" + folder).ToPlatformPath()).GetDirectories())
            {
                MapService folderService = new MapService(this, _accessService, folderDirectory.FullName, folder, MapServiceType.Folder);
                if (MapServices.Where(s => s.Fullname == folderService.Fullname && s.Type == folderService.Type).Count() == 0)
                {
                    MapServices.Add(folderService);
                    Console.WriteLine("folder " + folderService.Name + " added");
                }
            }

            #endregion
        }

        public void AddMapService(string mapName, MapServiceType type)
        {
            foreach (IMapService service in MapServices)
            {
                if (service.Fullname == mapName)
                {
                    return;
                }
            }
            string folder = String.Empty;
            if (mapName.Contains("/"))
            {
                if (mapName.Split('/').Length > 2)
                {
                    throw new Exception("Invalid map name: " + mapName);
                }
                folder = mapName.Split('/')[0];
                mapName = mapName.Split('/')[1];
            }
            MapServices.Add(new MapService(this, _accessService, mapName.Trim(), folder.Trim(), type));
        }

        private object _tryAddServiceLocker = new object();
        private IMapService TryAddService(FileInfo mapFileInfo, string folder)
        {
            lock (_tryAddServiceLocker)
            {
                if (!mapFileInfo.Exists)
                {
                    return null;
                }

                MapService mapService = new MapService(this, _accessService, mapFileInfo.FullName, folder, MapServiceType.MXL);
                if (MapServices.Where(s => s.Fullname == mapService.Fullname && s.Type == mapService.Type).Count() > 0)
                {
                    // allready exists
                    return MapServices.Where(s => s.Fullname == mapService.Fullname && s.Type == mapService.Type).FirstOrDefault();
                }

                if (!String.IsNullOrWhiteSpace(folder))
                {
                    #region Add Service Parent Folders

                    string folderName = String.Empty, parentFolder = String.Empty;
                    foreach (var subFolder in folder.Split('/'))
                    {
                        folderName += (folderName.Length > 0 ? "/" : "") + subFolder;
                        DirectoryInfo folderDirectory = new DirectoryInfo((Options.ServicesPath + "/" + folder).ToPlatformPath());
                        MapService folderService = new MapService(this, _accessService, folderDirectory.FullName, parentFolder, MapServiceType.Folder);

                        if (MapServices.Where(s => s.Fullname == folderService.Fullname && s.Type == folderService.Type).Count() == 0)
                        {
                            MapServices.Add(folderService);
                            Console.WriteLine("folder " + folderService.Name + " added");
                        }

                        parentFolder = folderName;
                    }

                    #endregion
                }

                MapServices.Add(mapService);
                Console.WriteLine("service " + mapService.Name + " added");

                return mapService;
            }
        }

        public IMapService TryAddService(string name, string folder)
        {
            var mapFileInfo = new FileInfo(Options.ServicesPath + (String.IsNullOrWhiteSpace(folder) ? "" : "/" + folder) + "/" + name + ".mxl");
            return TryAddService(mapFileInfo, folder);
        }

        private static object _reloadServicesLocker = new object();
        private static object _reloadServicesLockerKey = null;
        public void ReloadServices(string folder, bool forceRefresh = false)
        {
            lock (_reloadServicesLocker)
            {
                while (_reloadServicesLockerKey != null)
                {
                    Thread.Sleep(100);
                }
                _reloadServicesLockerKey = new object();
            }
            try
            {
                if (forceRefresh == true || MapServices.Where(s => s.Type != MapServiceType.Folder && s.Folder == folder).Count() == 0)
                {
                    AddServices(folder);
                }
            }
            finally
            {
                _reloadServicesLockerKey = null;
            }
        }

        public IServiceRequestInterpreter GetInterpreter(Type type)
        {
            var interpreter = new PlugInManager().CreateInstance<IServiceRequestInterpreter>(type);
            if (interpreter == null)
            {
                throw new Exception("Can't intialize interperter");
            }

            interpreter.OnCreate(Instance);
            return interpreter;
        }

        public IServiceRequestInterpreter GetInterpreter(Guid guid)
        {
            var interpreter = new PlugInManager().CreateInstance(guid) as IServiceRequestInterpreter;
            if (interpreter == null)
            {
                throw new Exception("Can't intialize interperter");
            }

            interpreter.OnCreate(Instance);
            return interpreter;
        }

        public IMapService GetMapService(string id)
        {
            var mapService = MapServices
                        .Where(f => f.Type == MapServiceType.MXL && id.Equals(f.Fullname, StringComparison.InvariantCultureIgnoreCase))
                        .FirstOrDefault();

            return mapService;
        }

        public IMapService GetFolderService(string id)
        {
            var folderService = MapServices
                        .Where(f => f.Type == MapServiceType.Folder && String.IsNullOrWhiteSpace(f.Folder) && id.Equals(f.Name, StringComparison.InvariantCultureIgnoreCase))
                        .FirstOrDefault();

            return folderService;
        }
    }
}
