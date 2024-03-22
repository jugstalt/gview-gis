using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.DataSources.VectorTileCache.Json.GLStyles;

public class GLStyleLayerLayout
{
    [JsonPropertyName("visibility")]
    public string Visibility { get; set; } = "visible";

    #region Text

    [JsonPropertyName("text-font")]
    public string[] TextFont { get; set; }

    [JsonPropertyName("text-field")]
    public string TextFieldExpression { get; set; }

    [JsonPropertyName("text-line-height")]
    public float TextLineHeight { get; set; }

    [JsonPropertyName("text-padding")]
    public float TextPadding { get; set; }

    [JsonPropertyName("text-allow-overlap")]
    public bool TextAllowOverlap { get; set; }

    [JsonPropertyName("text-ignore-placement")]
    public bool TextIgnorePlacement { get; set; }

    [JsonPropertyName("text-optional")]
    public bool TextOptional { get; set; }

    [JsonPropertyName(GLStyleProperties.TextOffset)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? TextOffset { get; set; }

    [JsonPropertyName(GLStyleProperties.TextAnchor)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? TextAnchor { get; set; }

    [JsonPropertyName(GLStyleProperties.TextSize)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? TextSize { get; set; }

    #endregion

    #region Symbol

    [JsonPropertyName(GLStyleProperties.SymbolPlacement)]
    public JsonElement? SymbolPlacement { get; set; }

    [JsonPropertyName(GLStyleProperties.SymbolSpacing)]
    public JsonElement? SymbolSpacing { get; set; }

    #endregion

    #region Icon

    [JsonPropertyName("icon-pitch-alignment")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string IconPitchAlignment { get; set; }

    [JsonPropertyName(GLStyleProperties.IconImage)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? IconImage { get; set; }

    [JsonPropertyName(GLStyleProperties.IconSize)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? IconSize { get; set; }

    [JsonPropertyName(GLStyleProperties.IconIgnorePlacement)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? IconIgnorePlacement { get; set; }

    [JsonPropertyName(GLStyleProperties.IconAllowOverlap)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? IconAllowOverlap { get; set; }

    #endregion
}
