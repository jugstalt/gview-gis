using System.Text.Json.Serialization;

namespace gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;

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

    /// <summary>
    /// Text fill colour (foreground / font colour). Unlike most other CIM symbol references,
    /// this is the symbol (typically CIMPolygonSymbol) directly - not wrapped in a
    /// CIMSymbolReference "type"/"symbol" envelope, the same shape <see cref="HaloSymbol"/>
    /// already uses.
    /// </summary>
    [JsonPropertyName("symbol")]
    public CimSymbol? TextFillSymbol { get; set; }

    [JsonPropertyName("haloSymbol")]
    public CimSymbol? HaloSymbol { get; set; }

    [JsonPropertyName("haloSize")]
    public double HaloSize { get; set; }

    /// <summary>
    /// Background/"mask" drawn behind the text - ArcGIS Pro's Text Symbol -> Callout, e.g. a
    /// white rounded box behind a label so it stays readable over busy geometry. Only
    /// CIMBalloonCallout's plain background fill is convertible (see
    /// <see cref="AprxMapConverter"/>) - other callout types (e.g. leader-line callouts) have
    /// no gView equivalent.
    /// </summary>
    [JsonPropertyName("callout")]
    public CimCallout? Callout { get; set; }
}

/// <summary>
/// A text symbol's callout - see <see cref="CimTextSymbol.Callout"/>. Not polymorphic: only
/// "type" and the background symbol are read, regardless of callout type, so the converter can
/// check <see cref="Type"/> itself and warn on unsupported ones.
/// </summary>
internal class CimCallout
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>Symbol (typically a CIMPolygonSymbol) drawn behind the text.</summary>
    [JsonPropertyName("backgroundSymbol")]
    public CimSymbol? BackgroundSymbol { get; set; }

    /// <summary>
    /// Padding (in points) between the text and the background box's edge - what makes
    /// ArcGIS Pro's balloon box noticeably larger than the bare text, unlike gView's
    /// pixel-tight <see cref="BlockoutTextSymbol"/> box before this was read. Real exports
    /// carry a "type": "CIMTextMargin" alongside left/right/top/bottom - not read here, this
    /// isn't polymorphic (there's nothing else to distinguish it from).
    /// </summary>
    [JsonPropertyName("margin")]
    public CimMargin? Margin { get; set; }
}

/// <summary>Padding around a callout's text - see <see cref="CimCallout.Margin"/>.</summary>
internal class CimMargin
{
    [JsonPropertyName("left")]
    public double Left { get; set; }

    [JsonPropertyName("right")]
    public double Right { get; set; }

    [JsonPropertyName("top")]
    public double Top { get; set; }

    [JsonPropertyName("bottom")]
    public double Bottom { get; set; }
}
