﻿using gView.Facilities.Abstraction;
using gView.Framework.Cartography;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.MapServer;
using gView.Framework.Core.UI;
using gView.Framework.Data.Metadata;
using gView.Framework.IO;
using gView.Server.AppCode;
using gView.Server.AppCode.Extensions;
using gView.Server.Services.Handlers;
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

namespace gView.Server.Services.MapServer;

public class MapServiceDeploymentManager
{
    private readonly MapServiceManager _mapServiceManager;
    private readonly AccessControlService _accessControl;
    private readonly IMessageQueueService _queue;
    private readonly ILogger<MapServiceDeploymentManager> _logger;

    // Singleton
    public readonly ServerMapDocument MapDocument;

    public MapServiceDeploymentManager(MapServiceManager mapServicerService,
                                       AccessControlService accessControlService,
                                       IMessageQueueService queueService,
                                       ILogger<MapServiceDeploymentManager> logger = null)
    {
        _mapServiceManager = mapServicerService;
        _accessControl = accessControlService;
        _queue = queueService;
        _logger = logger ?? new ConsoleLogger<MapServiceDeploymentManager>();

        MapDocument = new ServerMapDocument(_mapServiceManager);
    }

    async public Task<bool> AddMap(string mapName, string mapXml, string usr, string pwd)
    {
        await _accessControl.CheckPublishAccess(mapName.FolderName(), usr, pwd);

        if (await AddMap(mapName, mapXml))
        {
            await FireReloadMapMessage(mapName);
            return true;
        }

        return false;
    }

    async public Task<bool> AddMap(string mapName, string mapXml, IIdentity identity)
    {
        await _accessControl.CheckPublishAccess(mapName.FolderName(), identity);

        if (await AddMap(mapName, mapXml))
        {
            await FireReloadMapMessage(mapName);
            return true;
        }

        return false;
    }

    async public Task<bool> RemoveMap(string mapName, string usr, string pwd)
    {
        await _accessControl.CheckPublishAccess(mapName.FolderName(), usr, pwd);

        if (RemoveMap(mapName))
        {
            await FireRemoveMapMessage(mapName);
            return true;
        }

        return false;
    }

    async public Task<bool> RemoveMap(string mapName, IIdentity identity)
    {
        await _accessControl.CheckPublishAccess(mapName.FolderName(), identity);

        if (RemoveMap(mapName))
        {
            await FireRemoveMapMessage(mapName);
            return true;
        }

        return false;
    }

    async public Task<bool> ReloadMap(string mapName, string usr, string pwd)
    {
        await _accessControl.CheckPublishAccess(mapName.FolderName(), usr, pwd);

        if (await ReloadMap(mapName))
        {
            await FireReloadMapMessage(mapName);
            return true;
        }

        return false;
    }

    async public Task<string> GetMetadata(string mapName, string usr, string pwd)
    {
        await _accessControl.CheckPublishAccess(mapName.FolderName(), usr, pwd);

        if (!await ReloadMap(mapName))
        {
            return String.Empty;
        }

        //if (IMS.mapServer == null || IMS.mapServer[mapName] == null)
        //    return String.Empty;

        FileInfo fi = new FileInfo((_mapServiceManager.Options.ServicesPath + @"/" + mapName + ".meta").ToPlatformPath());
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
        await _accessControl.CheckPublishAccess(mapName.FolderName(), usr, pwd);

        FileInfo fi = new FileInfo(_mapServiceManager.Options.ServicesPath + @"/" + mapName + ".meta");

        StringReader sr = new StringReader(metadata);
        XmlStream xmlStream = new XmlStream("");
        xmlStream.ReadStream(sr);
        xmlStream.WriteStream(fi.FullName);

        if (await ReloadMap(mapName))
        {
            await FireReloadMapMessage(mapName);
            return true;
        }

        return false;
    }

    async public Task<IMap> LoadMap(string name)
    {
        IMap map = null;
        try
        {
            DirectoryInfo di = new DirectoryInfo(_mapServiceManager.Options.ServicesPath);
            if (!di.Exists)
            {
                di.Create();
            }

            FileInfo fi = new FileInfo($"{_mapServiceManager.Options.ServicesPath}/{name}.mxl");
            if (fi.Exists)
            {
                ServerMapDocument doc = new ServerMapDocument(_mapServiceManager);
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

                    if (map.HasErrorMessages(_mapServiceManager.Options.CriticalErrorLevel)
                        || !MapDocument.AddMap(map))
                    {
                        return null;
                    }

                    MapDocument.SetMapModules(map, doc.GetMapModules(map));

                    var mapService = _mapServiceManager.MapServices.Where(s => s.Fullname == map.Name).FirstOrDefault();
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
            _logger.LogError($"Map {name}: LoadConfig - {ex.Message}");
            _mapServiceManager?.Instance?.LogAsync(name, "LoadMap.Exception", loggingMethod.error, ex.Message);
        }
        finally
        {
            if (map != null && map.HasErrorMessages(ErrorMessageLevel.Any))
            {
                foreach (var errorMessage in map.ErrorMessages(ErrorMessageLevel.Any))
                {
                    _logger.LogWarning($"{map.Name}: LoadMap - {errorMessage} ");
                    _mapServiceManager?.Instance?.LogAsync(map.Name, "LoadMap.MapErrors", loggingMethod.error, errorMessage);
                }
            }
        }

        return null;
    }

    public Map GetMapByName(string name)
    {
        foreach (IMap map in this.MapDocument.Maps)
        {
            if (map.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && map is Map)
            {
                return (Map)map;
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
            if (!Directory.Exists((_mapServiceManager.Options.ServicesPath + "/" + folder).ToPlatformPath()))
            {
                throw new MapServerException("Folder not exists");
            }
        }

        if (String.IsNullOrEmpty(mapXml))
        {
            return await ReloadMap(mapName);
        }

        if ((await _mapServiceManager.Instance.Maps(null)).Count() >= _mapServiceManager.Instance.MaxServices)
        {
            // Überprüfen, ob schon eine Service mit gleiche Namen gibt...
            // wenn ja, ist es nur einem Refresh eines bestehenden Services
            bool found = false;
            foreach (IMapService existingMap in await _mapServiceManager.Instance.Maps(null))
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

        ServerMapDocument mapDocument = new ServerMapDocument(_mapServiceManager);
        await mapDocument.LoadAsync(xmlStream);

        if (mapDocument.Maps.Count() == 0)
        {
            throw new MapServerException("No maps found in document");
        }

        var map = mapDocument.Maps.First() as Map;
        //Map map = new Map();
        //map.Load(xmlStream);

        if (mapDocument.Readonly && map.Name != mapName)
        {
            // Readonly maps: eg. VTC Print Services
            // the will not be touthed, only copied to services folder
            throw new Exception($"MapDocument is readonly. The name of the map ({map.Name}) must be equal with service name ({mapName})");
        }
        else
        {
            map.Name = mapName;
        }

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

        if (map.HasErrorMessages(ErrorMessageLevel.Any))
        {
            //errors.Append("Map Errors/Warnings:" + Environment.NewLine);
            foreach (var errorMessage in map.ErrorMessages(ErrorMessageLevel.Any))
            {
                errors.Append(errorMessage + Environment.NewLine);
            }
            hasErrors |= map.HasErrorMessages(_mapServiceManager.Options.CriticalErrorLevel);
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
                    errors.Append($"Warning: {featureLayer.Title} has no spatial reference. Map default '{map.LayerDefaultSpatialReference.EpsgCode}' will used for this layer." + Environment.NewLine);
                }
                else
                {
                    errors.Append($"Error: {featureLayer.Title} has no spatial reference. Fix this or at least set a default spatial reference for this map in the carto app" + Environment.NewLine);
                }
            }
        }

        if (hasErrors)
        {
            throw new MapServerException($"Critical Errors:{Environment.NewLine}{errors.ToString()}");
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
        _mapServiceManager.AddMapService(mapName, MapServiceType.MXL);

        if (mapDocument.Readonly)
        {
            // Readonly maps: eg. VTC Print Services
            // the will not be touthed, only copied to services folder
            await SaveConfig(map,
                $"""
                <?xml version="1.0" encoding="utf-16"?>
                <MapServer culture="">
                   {mapXml}
                </MapServer>
                """);
        }
        else
        {
            await SaveConfig(mapDocument);
        }

        var result = await ReloadMap(mapName);

        if (errors.Length > 0)  // Warnings
        {
            throw new MapServerException($"Warnings:{Environment.NewLine}{errors.ToString()}");
        }

        return result;
    }

    internal bool RemoveMap(string mapName)
    {
        var mapService = _mapServiceManager.GetMapService(mapName);
        if (mapService != null)
        {
            _mapServiceManager.MapServices = new ConcurrentBag<IMapService>(_mapServiceManager.MapServices.Except(new[] { mapService }));
        }
        MapDocument.RemoveMap(mapName);
        RemoveConfig(mapName);

        _mapServiceManager.ReloadServices(mapName.FolderName(), true);

        _logger.LogInformation("Removed map {mapName} successfully", mapName);

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

            stream.WriteStream($"{_mapServiceManager.Options.ServicesPath}/{map.Name}.mxl");

            await ApplyMetadata(map);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Map {mapDocument?.Maps?.First()?.Name}: LoadConfig - {ex.Message}");
        }
    }

    async public Task SaveConfig(IMap map, string mapXml)
    {
        try
        {
            await File.WriteAllTextAsync(
                        $"{_mapServiceManager.Options.ServicesPath}/{map.Name}.mxl",
                        mapXml,
                        Encoding.Unicode
                    );

            if (map is Map)
            {
                await ApplyMetadata((Map)map);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Map {map.Name}: LoadConfig - {ex.Message}");
        }
    }

    private bool RemoveConfig(string mapName)
    {
        try
        {
            FileInfo fi = new FileInfo(_mapServiceManager.Options.ServicesPath + "/" + mapName + ".mxl");
            if (fi.Exists)
            {
                fi.Delete();
            }

            fi = new FileInfo(_mapServiceManager.Options.ServicesPath + "/" + mapName + ".svc");
            if (fi.Exists)
            {
                fi.Delete();
            }

            fi = new FileInfo(_mapServiceManager.Options.ServicesPath + "/" + mapName + ".meta");
            if (fi.Exists)
            {
                fi.Delete();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($" {mapName}: RemoveConfig - {ex.Message}");
            return false;
        }
    }

    async internal Task<bool> ReloadMap(string mapName)
    {
        if (MapDocument == null)
        {
            return false;
        }

        MapDocument.RemoveMap(mapName);
        var result = await LoadMap(mapName) != null;

        if (result)
        {
            _logger.LogInformation("Reloaded map {mapName} successfully", mapName);
        }

        return result;
    }

    async private Task ApplyMetadata(Map map)
    {
        try
        {
            if (map == null)
            {
                return;
            }

            FileInfo fi = new FileInfo(_mapServiceManager.Options.ServicesPath + @"/" + map.Name + ".meta");

            IEnumerable<IMapApplicationModule> modules = null;
            if (MapDocument is IMapDocumentModules)
            {
                modules = ((IMapDocumentModules)MapDocument).GetMapModules(map);
            }

            IServiceMap sMap = await ServiceMap.CreateAsync(map, _mapServiceManager.Instance, modules, null);
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
            _logger.LogError($"Map {map.Name}: ApplyMetadata - {ex.Message}");
        }
    }

    async private Task FireReloadMapMessage(string mapName)
        => await _queue.EnqueueAsync(
                Facilities.Const.MessageQueuePrefix,
                [$"{ReloadMapMessageHandler.Name}:{mapName}"]
            );

    async private Task FireRemoveMapMessage(string mapName)
        => await _queue.EnqueueAsync(
                Facilities.Const.MessageQueuePrefix,
                [$"{RemoveMapMessageHandler.Name}:{mapName}"]
            );

    #endregion
}
