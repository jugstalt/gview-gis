using gView.DataExplorer.Core.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.DataSources.RasterNet;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using gView.Framework.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.RasterNet.Cog;

/// <summary>The stored COG connections (name → URL) and the helpers of the dialogs.</summary>
internal static class CogConnections
{
    internal const string ConfigName = "rasternet_cog_connections";
    internal const string EncKey = "8E14903D-CAD9-425C-A9DD-EC8DEF30D00F";

    /// <summary>The connections, encrypted like the other web service connections (URLs may hold signatures).</summary>
    public static ConfigConnections Open(IExplorerObject explorerObject, IExplorerApplicationScopeService? scope = null)
        => ConfigConnections.Create(scope?.ConfigConnectionStorage ?? explorerObject.ConfigStorage(), ConfigName, EncKey);

    /// <summary>Opens the URL with RasterNet (one request for a COG) and describes the raster, or returns the error.</summary>
    public static async Task<CogUrlCheck> CheckAsync(string url)
    {
        if (!RasterNetSettings.IsUrl(url, out var uri))
        {
            return new CogUrlCheck(url, false, "not an absolute http(s) URL");
        }
        try
        {
            using var raster = await global::RasterNet.Raster.OpenAsync(uri, RasterNetSettings.HttpOptions);
            string crs = raster.Crs?.EpsgCode is { } epsg ? $", EPSG:{epsg}" : "";
            return new CogUrlCheck(url, true,
                $"{raster.Width} x {raster.Height}, {raster.Bands.Count} band(s) {raster.Bands[0].DataType}, {raster.Overviews.Count - 1} overview(s){crs}");
        }
        catch (Exception ex)
        {
            return new CogUrlCheck(url, false, ex.Message);
        }
    }

    /// <summary>
    /// A name for the URL that is not taken yet: the file name, or with the folder before it if that is taken
    /// (e.g. "S2B_33UXP_20240824_0_L2A/TCI.tif"), then with a counter.
    /// </summary>
    public static string UniqueName(string url, ICollection<string> taken)
    {
        var uri = new Uri(url);
        var segments = uri.Segments.Select(s => Uri.UnescapeDataString(s).Trim('/')).Where(s => s.Length > 0).ToArray();
        string file = segments.Length > 0 ? segments[^1] : uri.Host;
        var candidates = new List<string> { file };
        if (segments.Length > 1)
        {
            candidates.Add($"{segments[^2]}/{file}");
        }
        foreach (var candidate in candidates)
        {
            if (!taken.Contains(candidate))
            {
                return candidate;
            }
        }
        for (int i = 2; ; i++)
        {
            string candidate = $"{candidates[^1]} ({i})";
            if (!taken.Contains(candidate))
            {
                return candidate;
            }
        }
    }
}
