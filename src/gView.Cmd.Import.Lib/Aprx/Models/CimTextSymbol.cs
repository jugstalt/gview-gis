using System.Text.Json.Serialization;

namespace gView.Cmd.Import.Aprx.Models;

/// <summary>
/// CIM text symbol used for labeling.
/// Extends <see cref="CimSymbol"/> and is therefore handled by the same polymorphic deserializer.
/// </summary>
internal class CimTextSymbol : CimSymbol
{
    [JsonPropertyName("fontFamilyName")]
    public string? FontFamilyName { get; set; }

    [JsonPropertyName("fontStyleName")]
    public string? FontStyleName { get; set; }

    [JsonPropertyName("height")]
    public double Height { get; set; }

    [JsonPropertyName("horizontalAlignment")]
    public string? HorizontalAlignment { get; set; }

    [JsonPropertyName("verticalAlignment")]
    public string? VerticalAlignment { get; set; }

    [JsonPropertyName("fontEncoding")]
    public string? FontEncoding { get; set; }

    /// <summary>Text fill colour (foreground / font colour).</summary>
    [JsonPropertyName("symbol")]
    public CimSymbolReference? TextFillSymbol { get; set; }

    [JsonPropertyName("haloSymbol")]
    public CimSymbol? HaloSymbol { get; set; }

    [JsonPropertyName("haloSize")]
    public double HaloSize { get; set; }
}
