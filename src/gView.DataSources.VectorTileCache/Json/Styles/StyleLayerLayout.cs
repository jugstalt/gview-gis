using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.DataSources.VectorTileCache.Json.Styles;

public class StyleLayerLayout
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

    [JsonPropertyName(StyleProperties.TextSize)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? TextSize { get; set; }

    #endregion

    #region Symbol

    [JsonPropertyName("symbol-placement")]
    public string SymbolPlacement { get; set; }

    [JsonPropertyName("symbol-spacing")]
    public float SymbolSpacing { get; set; }

    #endregion

    #region Icon

    [JsonPropertyName("icon-pitch-alignment")]
    public string IconPitchAlignment { get; set; }

    #endregion
}
