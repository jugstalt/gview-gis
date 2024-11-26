#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.DataSources.VectorTileCache.Json.GLStyles;

public class GLStyleLayerPaint
{
    #region Line 

    [JsonPropertyName(GLStyleProperties.LineColor)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? LineColor { get; set; }

    [JsonPropertyName(GLStyleProperties.LineWidth)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? LineWidth { get; set; }

    [JsonPropertyName(GLStyleProperties.LineOpacity)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? LineOpacity { get; set; }

    [JsonPropertyName(GLStyleProperties.LineDashArray)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? LineDashArray { get; set; }

    #endregion

    #region Fill

    [JsonPropertyName(GLStyleProperties.FillAntiAliasing)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? FillAntiAliasing { get; set; }

    [JsonPropertyName(GLStyleProperties.FillColor)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? FillColor { get; set; }

    [JsonPropertyName(GLStyleProperties.FillOpacity)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? FillOpacity { get; set; }

    [JsonPropertyName(GLStyleProperties.FillOutlineColor)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? FillOutlineColor { get; set; }

    [JsonPropertyName(GLStyleProperties.FillOutlineWidth)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? FillOutlineWidth { get; set; }

    [JsonPropertyName(GLStyleProperties.FillOutlineDashArray)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? FillOutlineDashArray { get; set; }

    [JsonPropertyName(GLStyleProperties.FillOutlineOpacity)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? FillOutlineOpacity { get; set; }

    [JsonPropertyName(GLStyleProperties.FillExtrusionHeight)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? FillExtrusionHeight { get; set; }

    [JsonPropertyName(GLStyleProperties.FillExtrusionColor)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? FillExtrusionColor { get; set; }

    [JsonPropertyName(GLStyleProperties.RasterOpacity)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? RasterOpacity { get; set; }

    #endregion

    #region Text

    [JsonPropertyName(GLStyleProperties.TextHaloWidth)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? TextHaloWidth { get; set; }

    [JsonPropertyName(GLStyleProperties.TextHaloColor)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? TextHaloColor { get; set; }

    [JsonPropertyName(GLStyleProperties.TextColor)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? TextColor { get; set; }

    #endregion

    #region Raster

    [JsonPropertyName(GLStyleProperties.RasterBrightnessMin)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? RasterBrightnessMin { get; set; }

    #endregion
}
