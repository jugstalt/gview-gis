using gView.Framework.Core.Exceptions;
using gView.Framework.Core.UI;
using System;
using System.Linq;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

internal static class EnumerableExtensions
{
    static private string[] IdStrings = Enumerable.Range(0, 1000).Select(i => i.ToString()).ToArray();
    static private string[] ExcludeIdStrings = Enumerable.Range(0, 1000).Select(i => $"-{i}").ToArray();
    static private string[] IncludeIdStrings = Enumerable.Range(0, 1000).Select(i => $"+{i}").ToArray();

    public static MapLayerVisibility LayerOrParentIsInArray(this string[] layerIds, ITocElement tocElement)
    {
        while (tocElement != null)
        {
            foreach (var layer in tocElement.Layers ?? [])
            {
                var visibility = layerIds.ContainsId(layer.ID);
                if (visibility != MapLayerVisibility.Invisible)
                {
                    return visibility;
                }
            }

            tocElement = tocElement.ParentGroup;
        }

        return MapLayerVisibility.Invisible;
    }

    public static MapLayerVisibility ContainsId(this string[] layerIds, int layerId)
    {
        var id = layerId >= 0 && layerId < IdStrings.Length
                    ? IdStrings[layerId]
                    : layerId.ToString();

        if (layerIds.Contains(id)) return MapLayerVisibility.Visible;

        id = layerId >= 0 && layerId < IdStrings.Length
                    ? ExcludeIdStrings[layerId]
                    : $"-{layerId}";

        if (layerIds.Contains(id)) return MapLayerVisibility.Exclude;

        id = layerId >= 0 && layerId < IdStrings.Length
                    ? IncludeIdStrings[layerId]
                    : $"+{layerId}";

        if (layerIds.Contains(id)) return MapLayerVisibility.Include;

        return MapLayerVisibility.Invisible;
    }

    public static MapLayerVisibilityPattern GetVisibilityPattern(this string[] layerIds)
    {
        if (layerIds is null || layerIds.Length == 0)
        {
            return MapLayerVisibilityPattern.Defaults;
        }

        var countIncludeExcludes = layerIds.Count(l => l.StartsWith("-") || l.StartsWith("+"));

        if (countIncludeExcludes == 0) // no include/exclude layers
        {
            return MapLayerVisibilityPattern.Normal;
        }

        if (countIncludeExcludes == layerIds.Length)  // all are include or exclude
        {
            return MapLayerVisibilityPattern.IncludeExclude;
        }

        throw new MapServerException($"Invalid layer ids: dont mix include/exclude ids with normal ids");
    }
}
