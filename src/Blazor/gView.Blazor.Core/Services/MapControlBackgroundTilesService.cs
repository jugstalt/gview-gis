using gView.Blazor.Core.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace gView.Blazor.Core.Services;

public class MapControlBackgroundTilesService
{
    private readonly MapControlBackgroundTilesServiceOptions _options;
    private readonly ConcurrentDictionary<string, MapControlBackgroundTilesModel> _dict = new();

    public MapControlBackgroundTilesService(IOptions<MapControlBackgroundTilesServiceOptions> options)
    {
        _options = options.Value;

        string rootPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!,
            "misc",
            "mapcontrol",
            "tiles");

        foreach (var filename in new DirectoryInfo(rootPath).GetFiles("*.json"))
        {
            try
            {
                var json = File.ReadAllText(filename.FullName);
                var tileService = JsonSerializer.Deserialize<MapControlBackgroundTilesModel>(json,
                    new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (tileService is not null)
                {
                    _dict.TryAdd(
                        filename.Name.Substring(0, filename.Name.Length - filename.Extension.Length),
                        tileService);
                }
            }
            catch { }
        }
    }

    public MapControlBackgroundTilesModel GetDefaultOrAny()
    {
        if (_dict.ContainsKey(_options.Default))
        {
            return _dict[_options.Default];
        }

        if (_dict.Count == 0)
        {
            throw new ArgumentException("No background tile service defined");
        }

        return _dict[_dict.Keys.First()];
    }
}
