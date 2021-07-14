using gView.Core.Framework.Exceptions;
using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.UI;
using gView.MapServer;
using gView.Server.AppCode;
using gView.Server.AppCode.Extensions;
using gView.Server.Services.Logging;
using gView.Server.Services.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Server.Services.MapServer
{
    public class MapServiceDeploymentManager
    {
        private readonly MapServiceManager _mapServerService;
        private readonly AccessControlService _accessControlService;
        private readonly ILogger _logger;

        // Singleton
        public readonly ServerMapDocument MapDocument;

        public MapServiceDeploymentManager(MapServiceManager mapServicerService,
                                           AccessControlService accessControlService,
                                           ILogger<MapServiceDeploymentManager> logger=null)
        {
            _mapServerService = mapServicerService;
            _accessControlService = accessControlService;
            _logger = logger ?? new ConsoleLogger<MapServiceDeploymentManager>();

            MapDocument= new ServerMapDocument(_mapServerService);
        }

        async public Task<bool> AddMap(string mapName, string mapXml, string usr, string pwd)
        {
            await _accessControlService.CheckPublishAccess(mapName.FolderName(), usr, pwd);
            return await AddMap(mapName, mapXml);
        }

        async public Task<bool> AddMap(string mapName, string mapXml, IIdentity identity)
        {
            await _accessControlService.CheckPublishAccess(mapName.FolderName(), identity);
            return await AddMap(mapName, mapXml);
        }

        async public Task<bool> RemoveMap(string mapName, string usr, string pwd)
        {
            await _accessControlService.CheckPublishAccess(mapName.FolderName(), usr, pwd);
            return RemoveMap(mapName);
        }

        async public Task<bool> RemoveMap(string mapName, IIdentity identity)
        {
            await _accessControlService.CheckPublishAccess(mapName.FolderName(), identity);
            return RemoveMap(mapName);
        }

        async public Task<bool> ReloadMap(string mapName, string usr, string pwd)
        {
            await _accessControlService.CheckPublishAccess(mapName.FolderName(), usr, pwd);
            return await ReloadMap(mapName);
        }

        async public Task<string> GetMetadata(string mapName, string usr, string pwd)
        {
            await _accessControlService.CheckPublishAccess(mapName.FolderName(), usr, pwd);

            if (!await ReloadMap(mapName, usr, pwd))
            {
                return String.Empty;
            }

            //if (IMS.mapServer == null || IMS.mapServer[mapName] == null)
            //    return String.Empty;

            FileInfo fi = new FileInfo((_mapServerService.Options.ServicesPath + @"/" + mapName + ".meta").ToPlatformPath());
            if (!fi.Exists)
            {
                return String.Empty;
            }

            using (StreamReader sr = new StreamReader(fi.FullName.ToPlatformPath()))
            {
                string ret = sr.ReadToEnd();
                sr.Close();
                return ret;
            }
        }
        async public Task<bool> SetMetadata(string mapName, string metadata, string usr, string pwd)
        {
            await _accessControlService.CheckPublishAccess(mapName.FolderName(), usr, pwd);

            FileInfo fi = new FileInfo(_mapServerService.Options.ServicesPath + @"/" + mapName + ".meta");

            StringReader sr = new StringReader(metadata);
            XmlStream xmlStream = new XmlStream("");
            xmlStream.ReadStream(sr);
            xmlStream.WriteStream(fi.FullName);

            return await ReloadMap(mapName, usr, pwd);
        }

        async public Task<IMap> LoadMap(string name)
        {
            IMap map = null;
            try
            {
                DirectoryInfo di = new DirectoryInfo(_mapServerService.Options.ServicesPath);
                if (!di.Exists)
                {
                    di.Create();
                }

                FileInfo fi = new FileInfo(_mapServerService.Options.ServicesPath + @"/" + name + ".mxl");
                if (fi.Exists)
                {
                    ServerMapDocument doc = new ServerMapDocument(_mapServerService);
                    await doc.LoadMapDocumentAsync(fi.FullName);

                    if (doc.Maps.Count() == 1)
                    {
                        map = doc.Maps.First();
                        if (map.Name != name &&
                            name.Contains("/") &&
                            !map.Name.StartsWith(name.FolderName() + "/")) // Folder?
                        {
                            map.Name = name.Split('/')[0] + "/" + map.Name;
                        }

                        await ApplyMetadata(map as Map);

                        if (map.HasErrorMessages || !MapDocument.AddMap(map))
                        {
                            return null;
                        }

                        MapDocument.SetMapModules(map, doc.GetMapModules(map));

                        var mapService = _mapServerService.MapServices.Where(s => s.Fullname == map.Name).FirstOrDefault();
                        if (mapService != null)
                        {
                            mapService.ServiceRefreshed();
                        }


                        return map;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Map {name}: LoadConfig - { ex.Message }");
                _mapServerService?.Instance?.LogAsync(name, "LoadMap.Exception", loggingMethod.error, ex.Message);
            }
            finally
            {
                if (map != null && map.HasErrorMessages)
                {
                    foreach (var errorMessage in map.ErrorMessages)
                    {
                        _logger.LogWarning($"{ map.Name }: LoadMap - { errorMessage} ");
                        _mapServerService?.Instance?.LogAsync(map.Name, "LoadMap.MapErrors", loggingMethod.error, errorMessage);
                    }
                }
            }

            return null;
        }


        #region Helper

        async private Task<bool> AddMap(string mapName, string mapXml)
        {
            string folder = mapName.FolderName();
            if (!String.IsNullOrWhiteSpace(folder))
            {
                if (!Directory.Exists((_mapServerService.Options.ServicesPath + "/" + folder).ToPlatformPath()))
                {
                    throw new MapServerException("Folder not exists");
                }
            }

            if (String.IsNullOrEmpty(mapXml))
            {
                return await ReloadMap(mapName);
            }

            if ((await _mapServerService.Instance.Maps(null)).Count() >= _mapServerService.Instance.MaxServices)
            {
                // Überprüfen, ob schon eine Service mit gleiche Namen gibt...
                // wenn ja, ist es nur einem Refresh eines bestehenden Services
                bool found = false;
                foreach (IMapService existingMap in await _mapServerService.Instance.Maps(null))
                {
                    if (existingMap.Name == mapName)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    //return false;
                    throw new MapServerException("Unknown error");
                }
            }

            XmlStream xmlStream = new XmlStream("MapDocument");

            using (StringReader sr = new StringReader(mapXml))
            {
                if (!xmlStream.ReadStream(sr, String.Empty))  // invariant culture
                {
                    throw new MapServerException("Unable to read MapXML");
                }
            }

            ServerMapDocument mapDocument = new ServerMapDocument(_mapServerService);
            await mapDocument.LoadAsync(xmlStream);

            if (mapDocument.Maps.Count() == 0)
            {
                throw new MapServerException("No maps found in document");
            }

            var map = mapDocument.Maps.First() as Map;
            //Map map = new Map();
            //map.Load(xmlStream);
            map.Name = mapName;

            StringBuilder errors = new StringBuilder();
            bool hasErrors = false;

            foreach (var dataset in map.Datasets)
            {
                if (!String.IsNullOrWhiteSpace(dataset.LastErrorMessage))
                {
                    errors.Append("Dataset " + dataset.GetType().ToString() + Environment.NewLine);
                    errors.Append(dataset.LastErrorMessage + Environment.NewLine + Environment.NewLine);
                    hasErrors = true;
                }
            }

            if (map.HasErrorMessages)
            {
                //errors.Append("Map Errors/Warnings:" + Environment.NewLine);
                foreach (var errorMessage in map.ErrorMessages)
                {
                    errors.Append(errorMessage + Environment.NewLine);
                    hasErrors |= errorMessage.ToLower().StartsWith("warning:") == false;  // Warnings should not throw an exception
                }
            }
            if (map.LastException != null)
            {
                errors.Append("Map Exception:" + Environment.NewLine);
                errors.Append(map.LastException.Message?.ToString());
                hasErrors = true;
            }

            foreach (IFeatureLayer featureLayer in map.MapElements.Where(e => e is IFeatureLayer))
            {
                if (featureLayer.Class is IFeatureClass && ((IFeatureClass)featureLayer.Class).SpatialReference == null)
                {
                    if (map.LayerDefaultSpatialReference != null)
                    {
                        errors.Append($"Warning: { featureLayer.Title } has no spatial reference. Map default '{ map.LayerDefaultSpatialReference.EpsgCode }' will used for this layer." + Environment.NewLine);
                    }
                    else
                    {
                        errors.Append($"Error: { featureLayer.Title } has no spatial reference. Fix this or at least set a default spatial reference for this map in the carto app" + Environment.NewLine);
                    }
                }
            }

            if (hasErrors)
            {
                throw new MapServerException("Errors: " + Environment.NewLine + errors.ToString());
            }

            XmlStream pluginStream = new XmlStream("Moduls");
            using (StringReader sr = new StringReader(mapXml))
            {
                if (!xmlStream.ReadStream(sr))
                {
                    throw new MapServerException("Unable to read MapXML (Moduls)");
                }
            }

            ModulesPersists modules = new ModulesPersists(map);
            modules.Load(pluginStream);

            //foreach (IMap m in ListOperations<IMap>.Clone(_doc.Maps))
            //{
            //    if (m.Name == map.Name) _doc.RemoveMap(m);
            //}

            //if (!_doc.AddMap(map)) return false;
            _mapServerService.AddMapService(mapName, MapServiceType.MXL);

            await SaveConfig(mapDocument);

            var result = await ReloadMap(mapName);

            if (errors.Length > 0)  // Warnings
            {
                throw new MapServerException("Warnings: " + Environment.NewLine + errors.ToString());
            }

            return result;
        }

        private bool RemoveMap(string mapName)
        {
            var mapService = _mapServerService.GetMapService(mapName);
            if (mapService != null)
            {
                _mapServerService.MapServices = new ConcurrentBag<IMapService>(_mapServerService.MapServices.Except(new[] { mapService }));
            }
            MapDocument.RemoveMap(mapName);
            RemoveConfig(mapName);

            _mapServerService.ReloadServices(mapName.FolderName(), true);

            return true;
        }

        async public Task SaveConfig(ServerMapDocument mapDocument)
        {
            try
            {
                if (MapDocument == null)
                {
                    return;
                }

                var map = mapDocument?.Maps.First() as Map;
                if (map == null)
                {
                    throw new MapServerException("Mapdocument don't contain a map");
                }

                XmlStream stream = new XmlStream("MapServer");
                stream.Save("MapDocument", mapDocument);

                stream.WriteStream(_mapServerService.Options.ServicesPath + "/" + map.Name + ".mxl");

                if (map is Map)
                {
                    await ApplyMetadata(map);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Map { mapDocument?.Maps?.First()?.Name }: LoadConfig - { ex.Message }");
            }
        }

        private bool RemoveConfig(string mapName)
        {
            try
            {
                FileInfo fi = new FileInfo(_mapServerService.Options.ServicesPath + "/" + mapName + ".mxl");
                if (fi.Exists)
                {
                    fi.Delete();
                }

                fi = new FileInfo(_mapServerService.Options.ServicesPath + "/" + mapName + ".svc");
                if (fi.Exists)
                {
                    fi.Delete();
                }

                fi = new FileInfo(_mapServerService.Options.ServicesPath + "/" + mapName + ".meta");
                if (fi.Exists)
                {
                    fi.Delete();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($" {mapName }: RemoveConfig - { ex.Message }");
                return false;
            }
        }

        async private Task<bool> ReloadMap(string mapName)
        {
            if (MapDocument == null)
            {
                return false;
            }

            MapDocument.RemoveMap(mapName);
            return await LoadMap(mapName) != null;
        }

        async private Task ApplyMetadata(Map map)
        {
            try
            {
                if (map == null)
                {
                    return;
                }

                FileInfo fi = new FileInfo(_mapServerService.Options.ServicesPath + @"/" + map.Name + ".meta");

                IEnumerable<IMapApplicationModule> modules = null;
                if (MapDocument is IMapDocumentModules)
                {
                    modules = ((IMapDocumentModules)MapDocument).GetMapModules(map);
                }

                IServiceMap sMap = await ServiceMap.CreateAsync(map, _mapServerService.Instance, modules);
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
                await sMap.WriteMetadata(xmlStream);

                if (map is Metadata)
                {
                    await map.SetMetadataProviders(await sMap.GetMetadataProviders(), map, true);
                    await map.UpdateMetadataProviders();
                }

                // Overriding: no good idea -> problem, if multiple instances do this -> killing the metadata file!!!
                //fi.Refresh();
                //if (!fi.Exists)
                //{
                //    xmlStream.WriteStream(fi.FullName);
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError($"Map { map.Name }: ApplyMetadata - { ex.Message }");
            }
        }

        #endregion
    }
}
