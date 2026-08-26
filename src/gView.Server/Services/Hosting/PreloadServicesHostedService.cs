using gView.Framework.Core.MapServer;
using gView.Server.Services.MapServer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace gView.Server.Services.Hosting;

// Loads services matching the "preload-services" patterns (mapserver.json) into memory
// right after startup, so the first client request doesn't have to wait for it.
// Runs in the background - it does not delay the server startup itself.
public class PreloadServicesHostedService : BackgroundService
{
    private readonly MapServiceManager _mapServiceManager;
    private readonly MapServiceDeploymentManager _deploymentManager;
    private readonly ILogger<PreloadServicesHostedService> _logger;

    public PreloadServicesHostedService(
        MapServiceManager mapServiceManager,
        MapServiceDeploymentManager deploymentManager,
        ILogger<PreloadServicesHostedService> logger)
    {
        _mapServiceManager = mapServiceManager;
        _deploymentManager = deploymentManager;
        _logger = logger;
    }

    async protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var patterns = _mapServiceManager.Options?.PreloadServices;
        if (patterns == null || patterns.Length == 0)
        {
            return;
        }

        // MapServiceManager only discovers services on demand: at startup it only knows
        // about the root-level services/folders, and a folder's own content is loaded
        // only once ReloadServices(folder) is called for it (e.g. when that folder gets
        // browsed). Walk the whole folder tree here, level by level, so nested services
        // (e.g. "folder/sub/service") are known before we try to match/preload them.
        DiscoverAllServices(stoppingToken);

        var services = _mapServiceManager.MapServices
            .Where(s => s.Type == MapServiceType.MXL)
            .Where(s => patterns.Any(pattern => MatchesWildcard(s.Fullname, pattern)))
            .ToArray();

        foreach (var service in services)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                var map = await _deploymentManager.LoadMap(service.Fullname);
                if (map != null)
                {
                    _logger.LogInformation("Preloaded service '{service}'", service.Fullname);
                }
                else
                {
                    _logger.LogWarning("Preload of service '{service}' failed", service.Fullname);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Preload of service '{service}' failed", service.Fullname);
            }
        }
    }

    private void DiscoverAllServices(CancellationToken stoppingToken)
    {
        var processedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { String.Empty };
        var pendingFolders = new Queue<string>(
            _mapServiceManager.MapServices
                .Where(s => s.Type == MapServiceType.Folder)
                .Select(s => s.Fullname));

        while (pendingFolders.Count > 0 && !stoppingToken.IsCancellationRequested)
        {
            var folder = pendingFolders.Dequeue();
            if (!processedFolders.Add(folder))
            {
                continue;
            }

            _mapServiceManager.ReloadServices(folder, true);

            foreach (var subFolder in _mapServiceManager.MapServices
                .Where(s => s.Type == MapServiceType.Folder && folder.Equals(s.Folder, StringComparison.OrdinalIgnoreCase)))
            {
                if (!processedFolders.Contains(subFolder.Fullname))
                {
                    pendingFolders.Enqueue(subFolder.Fullname);
                }
            }
        }
    }

    private static bool MatchesWildcard(string name, string pattern)
    {
        if (String.IsNullOrWhiteSpace(name) || String.IsNullOrWhiteSpace(pattern))
        {
            return false;
        }

        var regexPattern = "^" + Regex.Escape(pattern.Trim())
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";

        return Regex.IsMatch(name, regexPattern, RegexOptions.IgnoreCase);
    }
}
