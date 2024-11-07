using gView.Blazor.Core.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace gView.Blazor.Core.Services;

public class MapControlCrsService
{
    private readonly ConcurrentDictionary<string, MapControlCrsModel> _crsDict = new();
    private readonly MapControlCrsServiceOptions _options;

    public MapControlCrsService(IOptions<MapControlCrsServiceOptions> options)
    {
        _options = options.Value;

        string rootPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!,
            "misc",
            "mapcontrol",
            "crs");

        foreach (var filename in new DirectoryInfo(rootPath).GetFiles("*.json"))
        {
            try
            {
                var json = File.ReadAllText(filename.FullName);
                var crs = JsonSerializer.Deserialize<MapControlCrsModel>(json,
                    new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (crs is not null)
                {
                    _crsDict.TryAdd(
                        filename.Name.Substring(0, filename.Name.Length - filename.Extension.Length),
                        crs);
                }
            }
            catch { }
        }
    }

    public MapControlCrsModel GetDefaultOrAny()
    {
        if (_crsDict.ContainsKey(_options.Default))
        {
            return _crsDict[_options.Default];
        }

        if (_crsDict.Count == 0)
        {
            throw new ArgumentException("No map control crs defined");
        }

        return _crsDict[_crsDict.Keys.First()];
    }
}
