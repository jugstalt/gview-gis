using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.DataSources.VectorTileCache.Json.Styles;

public class StyleLayerPaint
{
    #region Line 

    [JsonPropertyName(StyleProperties.LineColor)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? LineColor { get; set; }

    [JsonPropertyName(StyleProperties.LineWidth)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? LineWidth { get; set; }

    [JsonPropertyName(StyleProperties.LineOpacity)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? LineOpacity { get; set; }

    #endregion

    #region Fill

    [JsonPropertyName(StyleProperties.FillAntiAliasing)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? FillAntiAliasing { get; set; }

    [JsonPropertyName(StyleProperties.FillColor)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? FillColor { get; set; }

    [JsonPropertyName(StyleProperties.FillOpacity)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? FillOpacity { get; set; }

    [JsonPropertyName(StyleProperties.FillOutlineColor)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? FillOutlineColor { get; set; }

    [JsonPropertyName(StyleProperties.FillExtrusionHeight)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? FillExtrusionHeight { get; set; }

    [JsonPropertyName(StyleProperties.FillExtrusionColor)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? FillExtrusionColor { get; set; }

    #endregion

    #region Text

    [JsonPropertyName(StyleProperties.TextHaloWidth)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? TextHaloWidth { get; set; }

    [JsonPropertyName(StyleProperties.TextHaloColor)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? TextHaloColor { get; set; }

    [JsonPropertyName(StyleProperties.TextColor)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? TextColor { get; set; }

    #endregion

    #region Raster

    [JsonPropertyName(StyleProperties.RasterBrightnessMin)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? RasterBrightnessMin { get; set; }

    #endregion
}
