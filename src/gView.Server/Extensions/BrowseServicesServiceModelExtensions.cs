using gView.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Server.Extensions;

internal static class BrowseServicesServiceModelExtensions
{
    /// <summary>The interfaces the test map drives (v1), in the order of its selector</summary>
    private static readonly (string Key, string Title)[] TestMapInterfaceKeys =
    [
        ("wms", "WMS"),
        ("geoservices", "GeoServices REST"),
        ("geojsonservice", "GeoJson")
    ];

    /// <summary>
    /// Base urls of the interfaces of the service the test map can drive: the primary url of
    /// the interface without the query string (GeoJson: the service root instead of /capabilities).
    /// GeoServices only if the service is a MapServer for the user.
    /// </summary>
    public static IEnumerable<TestMapInterface> TestMapInterfaces(this BrowseServicesServiceModel model)
    {
        foreach (var (key, title) in TestMapInterfaceKeys)
        {
            var interpreter = model.Interpreters?.FirstOrDefault(i => i.IdentityName == key);
            var primary = interpreter?.Capabilities?.Capabilities?.FirstOrDefault(c => c.IsPrimary);
            if (primary is null)
            {
                continue;
            }

            if (key == "geoservices" && !model.GeoServiceTypes.Contains("MapServer"))
            {
                continue;
            }

            var url = model.ReplaceRequest(primary.RequestText);
            if (url.Contains('?'))
            {
                url = url.Substring(0, url.IndexOf('?'));
            }
            if (key == "geojsonservice" && url.EndsWith("/capabilities"))
            {
                url = url.Substring(0, url.Length - "/capabilities".Length);
            }

            yield return new TestMapInterface(key, title, url);
        }
    }

    public static string ReplaceRequest(this BrowseServicesServiceModel model, string request)
    {
        return request?
            .Replace("{server}", model.Server)
            .Replace("{onlineresource}", model.OnlineResource)
            .Replace("{service}", model.MapService.Name)
            .Replace("{folder}", model.MapService.Folder)
            .Replace("{folder|_}", String.IsNullOrWhiteSpace(model.MapService.Folder)  // OGC API: "_" is the root folder
                                            ? "_"
                                            : model.MapService.Folder)
            .Replace("{folder/service}", $"{(String.IsNullOrWhiteSpace(model.MapService.Folder)
                                            ? ""
                                            : $"{model.MapService.Folder}/")}{model.MapService.Name}")
            .Replace("{folder@service}", $"{(String.IsNullOrWhiteSpace(model.MapService.Folder)
                                            ? ""
                                            : $"{model.MapService.Folder}@")}{model.MapService.Name}");
    }
}
