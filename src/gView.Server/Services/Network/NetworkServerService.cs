#nullable enable

using gView.Framework.Core.Carto;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.MapServer;
using gView.Framework.IO;
using gView.Framework.NetworkModel.Build;
using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Module;
using gView.Framework.NetworkModel.Store;
using gView.Server.AppCode;
using gView.Server.Services.MapServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace gView.Server.Services.Network;

/// <summary>Settings of the section "Network" in appsettings.</summary>
public class NetworkServerOptions
{
    /// <summary>A search result with more nodes (and violations) is cut and marked exceededLimit.</summary>
    public int MaxResultNodes { get; set; } = 50000;

    /// <summary>A search taking longer answers with an error.</summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>How many searches run at the same time; others wait.</summary>
    public int MaxParallel { get; set; } = 2;

    /// <summary>Shards kept per network; 0 for no limit.</summary>
    public int MaxLoadedShards { get; set; } = 512;

    /// <summary>A network not searched for this long is dropped from memory.</summary>
    public int IdleMinutes { get; set; } = 30;
}

/// <summary>The state of a build started from the admin page.</summary>
public class NetworkBuildJob
{
    public string Service { get; init; } = "";
    public DateTime Started { get; init; } = DateTime.Now;
    public DateTime? Finished { get; set; }
    public bool Running => Finished is null;
    public bool? Succeeded { get; set; }
    public string? Error { get; set; }

    private readonly List<string> _log = [];

    public IReadOnlyList<string> Log
    {
        get { lock (_log) { return _log.ToList(); } }
    }

    internal void AddLine(string line)
    {
        lock (_log) { _log.Add($"{DateTime.Now:HH:mm:ss} {line}"); }
    }
}

/// <summary>
/// The networks of the map services: where they are (NetworkModule of the mxl, directory next to it), the shared
/// graphs, the limit of parallel searches and builds started from the admin page (one per service at a time).
/// </summary>
public class NetworkServerService
{
    private readonly MapServiceManager _mapServiceManager;
    private readonly ILogger<NetworkServerService> _logger;
    private readonly ConcurrentDictionary<string, NetworkBuildJob> _jobs = new(StringComparer.OrdinalIgnoreCase);

    public NetworkServerService(MapServiceManager mapServiceManager, IConfiguration configuration, ILogger<NetworkServerService> logger)
    {
        _mapServiceManager = mapServiceManager;
        _logger = logger;
        configuration.GetSection("Network").Bind(Options);
        Searches = new SemaphoreSlim(Math.Max(1, Options.MaxParallel));
        NetworkGraphCache.Shared.MaxLoadedShards = Options.MaxLoadedShards;
        NetworkGraphCache.Shared.IdleTimeout = Options.IdleMinutes > 0 ? TimeSpan.FromMinutes(Options.IdleMinutes) : null;
    }

    public NetworkServerOptions Options { get; } = new();

    /// <summary>Limits the searches running at the same time.</summary>
    public SemaphoreSlim Searches { get; }

    private readonly ConcurrentDictionary<string, (DateTime Stamp, NetworkConfig? Config)> _configs = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The map document (mxl) of a service. A service added by publishing knows only its name, so the path is taken
    /// from the services folder then: {ServicesPath}/{folder/}{name}.mxl.
    /// </summary>
    public string? MapDocumentPath(IMapService mapService)
    {
        if (mapService.Type != MapServiceType.MXL && mapService.Type != MapServiceType.GDI)
        {
            return null;
        }
        if (mapService is MapService { Filename: var filename } && !string.IsNullOrEmpty(filename) && File.Exists(filename))
        {
            return Path.GetFullPath(filename);
        }
        var servicesPath = _mapServiceManager.Options.ServicesPath;
        if (string.IsNullOrEmpty(servicesPath))
        {
            return null;
        }
        var path = Path.GetFullPath(Path.Combine(servicesPath, $"{mapService.Fullname}.mxl"));
        return File.Exists(path) ? path : null;
    }

    /// <summary>The network config stored in the service's map document (read from the mxl, cached until it changes); null for none.</summary>
    public NetworkConfig? Config(IMapService mapService)
    {
        if (MapDocumentPath(mapService) is not string mxl)
        {
            return null;
        }
        var stamp = File.GetLastWriteTimeUtc(mxl);
        if (_configs.TryGetValue(mxl, out var cached) && cached.Stamp == stamp)
        {
            return cached.Config;
        }

        NetworkConfig? config = null;
        try
        {
            config = NetworkModuleReader.Read(null, reader =>
            {
                var stream = new XmlStream("");
                stream.ReadStream(mxl);
                stream.Load("MapDocument", null, reader);
            })?.Config;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Can't read the network config of {mxl}: {message}", mxl, ex.Message);
        }
        _configs[mxl] = (stamp, config);
        return config;
    }

    /// <summary>
    /// The network directory of a service, or null when its map has no network configured:
    /// {NetworksPath}/{folder}/{service}.network. The "directory" of the config is for Carto and gView.Cmd only.
    /// </summary>
    public string? NetworkDirectory(IMapService mapService)
        => Config(mapService) is not null ? NetworkDirectoryPath(_mapServiceManager.Options, mapService.Fullname) : null;

    /// <summary>{NetworksPath}/{folder}/{service}.network for a service name like "folder/service".</summary>
    public static string? NetworkDirectoryPath(MapServerManagerOptions options, string serviceFullname)
        => string.IsNullOrEmpty(options?.NetworksPath) || string.IsNullOrWhiteSpace(serviceFullname)
            ? null
            : Path.GetFullPath(Path.Combine(options.NetworksPath, $"{serviceFullname.Trim('/')}.network"));

    /// <summary>The metadata of the built network, or null.</summary>
    public static NetworkMetadata? Metadata(string? directory)
        => IsBuilt(directory) ? new FileNetworkStore(directory!).ReadMetadata() : null;

    public static bool IsBuilt(string? directory)
        => directory is not null && File.Exists(Path.Combine(directory, FileNetworkStore.MetadataFile));

    public NetworkBuildJob? Job(IMapService mapService) => _jobs.GetValueOrDefault(mapService.Fullname);

    /// <summary>Starts a build of the service's network in the background; null when one is running already.</summary>
    public NetworkBuildJob? StartBuild(IMapService mapService)
    {
        var mxl = MapDocumentPath(mapService) ?? throw new MapServerException("The service has no map document.");

        var job = new NetworkBuildJob { Service = mapService.Fullname };
        while (true)
        {
            if (_jobs.TryGetValue(mapService.Fullname, out var current))
            {
                if (current.Running)
                {
                    return null;
                }
                if (!_jobs.TryUpdate(mapService.Fullname, job, current))
                {
                    continue;
                }
            }
            else if (!_jobs.TryAdd(mapService.Fullname, job))
            {
                continue;
            }
            break;
        }

        var directory = NetworkDirectory(mapService) ?? throw new MapServerException("No networks folder configured.");
        _ = Task.Run(() => RunBuild(job, mxl, directory));
        return job;
    }

    private async Task RunBuild(NetworkBuildJob job, string mxl, string directory)
    {
        try
        {
            // a fresh map, so the build does not share map objects with running requests
            var document = new ServerMapDocument(_mapServiceManager);
            if (!await document.LoadMapDocumentAsync(mxl) || document.Maps.FirstOrDefault() is not IMap map)
            {
                throw new InvalidDataException($"Can't load {mxl}");
            }
            var module = document.GetMapModules(map).OfType<NetworkModule>().FirstOrDefault(m => m.IsConfigured)
                ?? throw new InvalidDataException("The map has no network configured.");

            Directory.CreateDirectory(Path.GetDirectoryName(directory)!);
            job.AddLine($"Network directory: {directory}");
            job.Succeeded = await NetworkBuildRunner.RunAsync(map, module.Config, directory, false, job.AddLine);
            NetworkGraphCache.Shared.Invalidate(directory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Network build of {service} failed", job.Service);
            job.Error = ex.Message;
            job.AddLine($"Error: {ex.Message}");
            job.Succeeded = false;
        }
        finally
        {
            job.Finished = DateTime.Now;
        }
    }
}
