#nullable enable

using gView.Framework.Core.Data;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

internal static class LayerExtensions
{
    /// <summary>The layer type of capabilities, layers and legend: GroupLayer, FeatureLayer, RasterLayer or Layer</summary>
    public static string LayerTypeName(this ILayer layer) => layer switch
    {
        IGroupLayer => "GroupLayer",
        IFeatureLayer => "FeatureLayer",
        _ when layer.Class is IRasterClass => "RasterLayer",
        _ => "Layer"
    };
}
