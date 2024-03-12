using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.DataSources.VectorTileCache.Json.Styles;

public class StyleLayerPaint
{
    #region Line 

    [JsonPropertyName("line-color")]
    public JsonElement LineColor { get; set; }

    [JsonPropertyName("line-width")]
    public JsonElement LineWidth { get; set; }

    [JsonPropertyName("line-opacity")]
    public JsonElement LineOpacity { get; set; }

    #endregion

    #region Fill

    [JsonPropertyName("fill-antialias")]
    public JsonElement FillAntiAliasing { get; set; }

    [JsonPropertyName("fill-color")]
    public JsonElement FillColor { get; set; }

    [JsonPropertyName("fill-opacity")]
    public JsonElement FillOpacity { get; set; }

    [JsonPropertyName("fill-outline-color")]
    public JsonElement FillOutlineColor { get; set; }

    #endregion

    #region Text

    [JsonPropertyName("text-halo-width")]
    public JsonElement TextHaloWidth { get; set; }

    [JsonPropertyName("text-halo-color")]
    public JsonElement TextHaloColor { get; set; }

    [JsonPropertyName("text-color")]
    public JsonElement TextColor { get; set; }

    #endregion

    #region Raster

    [JsonPropertyName("raster-brightness-min")]
    public JsonElement RasterBrightnessMin { get; set; }

    #endregion
}
