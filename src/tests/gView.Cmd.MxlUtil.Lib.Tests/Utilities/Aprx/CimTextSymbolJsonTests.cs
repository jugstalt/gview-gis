using System.Text.Json;
using gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Tests <see cref="CimTextSymbol"/>'s JSON deserialization directly against the real shape
/// ArcGIS Pro exports, rather than building the object graph by hand (as
/// <see cref="LabelRendererConversionTests"/>'s font-color test does, which never exercises
/// JSON parsing). Found via a real aprx: <c>CimTextSymbol.TextFillSymbol</c> was typed as
/// <c>CimSymbolReference?</c> (a "type"/"symbol" envelope), but the real JSON puts the
/// CIMPolygonSymbol directly under "symbol" - the same shape "haloSymbol" already used. That
/// mismatch meant <c>TextFillSymbol.Symbol</c> was always null, so the label's own text always
/// silently fell back to black - even though the halo (a differently-shaped, correctly-typed
/// property) converted with its real color.
/// </summary>
public class CimTextSymbolJsonTests
{
    // Trimmed down from a real aprx's label class textSymbol - keeps only what's relevant here,
    // but preserves the exact "symbol"/"haloSymbol" shapes as ArcGIS Pro actually exports them.
    private const string RealWorldTextSymbolJson = """
        {
          "type": "CIMTextSymbol",
          "fontFamilyName": "Arial",
          "fontStyleName": "Regular",
          "height": 8,
          "haloSize": 3,
          "haloSymbol": {
            "type": "CIMPolygonSymbol",
            "symbolLayers": [
              {
                "type": "CIMSolidFill",
                "enable": true,
                "color": { "type": "CIMRGBColor", "values": [255, 170, 0, 100] }
              }
            ]
          },
          "symbol": {
            "type": "CIMPolygonSymbol",
            "symbolLayers": [
              {
                "type": "CIMSolidFill",
                "enable": true,
                "color": { "type": "CIMRGBColor", "values": [0, 77, 168, 100] }
              }
            ]
          }
        }
        """;

    [Fact]
    public void TextFillSymbol_RealWorldShape_DeserializesAsPolygonSymbolDirectly()
    {
        var textSymbol = JsonSerializer.Deserialize<CimSymbol>(RealWorldTextSymbolJson);

        var cimText = Assert.IsType<CimTextSymbol>(textSymbol);
        var fillPoly = Assert.IsType<CimPolygonSymbol>(cimText.TextFillSymbol);
        var fill = Assert.IsType<CimSolidFill>(Assert.Single(fillPoly.SymbolLayers!));
        var color = Assert.IsType<CimRgbColor>(fill.Color);
        Assert.Equal(0, color.R);
        Assert.Equal(77, color.G);
        Assert.Equal(168, color.B);
    }

    [Fact]
    public void HaloSymbol_RealWorldShape_StillDeserializesCorrectly()
    {
        // Regression guard: HaloSymbol was already correctly typed - make sure it still is.
        var textSymbol = JsonSerializer.Deserialize<CimSymbol>(RealWorldTextSymbolJson);

        var cimText = Assert.IsType<CimTextSymbol>(textSymbol);
        var haloPoly = Assert.IsType<CimPolygonSymbol>(cimText.HaloSymbol);
        var fill = Assert.IsType<CimSolidFill>(Assert.Single(haloPoly.SymbolLayers!));
        var color = Assert.IsType<CimRgbColor>(fill.Color);
        Assert.Equal(255, color.R);
        Assert.Equal(170, color.G);
        Assert.Equal(0, color.B);
    }
}
