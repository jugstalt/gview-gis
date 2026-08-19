using gView.Cmd.MxlUtil.Lib.Utilities.Aprx;
using gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Data;
using gView.Framework.Symbology;
using gView.GraphicsEngine;
using FontStyle = gView.GraphicsEngine.FontStyle;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Tests for <see cref="AprxMapConverter"/>'s label-renderer conversion: plain field references,
/// expressions handed off to <c>AprxLabelExpressionParser</c> (successful conversions logged via
/// "info", unsupported ones kept as-is with a "warn"), and text symbol building (font style,
/// color, halo).
/// </summary>
public class LabelRendererConversionTests
{
    private static (FeatureLayer Layer, List<string> Warnings, List<string> Infos) ConvertLabeledLayer(
        CimLabelClass labelClass)
    {
        var warnings = new List<string>();
        var infos = new List<string>();
        var converter = new AprxMapConverter(warn: warnings.Add, info: infos.Add);
        var cimLayer = Cim.FeatureLayer(
            name: "L",
            featureTable: Cim.FeatureTable(),
            labelVisibility: true,
            labelClasses: [labelClass]);
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);
        return ((FeatureLayer)map.MapElements[0], warnings, infos);
    }

    [Fact]
    public void LabelVisibilityFalse_NoLabelRendererIsCreated()
    {
        var warnings = new List<string>();
        var converter = new AprxMapConverter(warn: warnings.Add);
        var cimLayer = Cim.FeatureLayer(
            featureTable: Cim.FeatureTable(),
            labelVisibility: false,
            labelClasses: [Cim.LabelClass(expression: "[NAME]")]);
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);

        Assert.Null(((FeatureLayer)map.MapElements[0]).LabelRenderer);
    }

    [Fact]
    public void NoLabelClasses_NoLabelRendererIsCreated()
    {
        var converter = new AprxMapConverter();
        var cimLayer = Cim.FeatureLayer(featureTable: Cim.FeatureTable(), labelVisibility: true, labelClasses: []);
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);

        Assert.Null(((FeatureLayer)map.MapElements[0]).LabelRenderer);
    }

    [Fact]
    public void PlainFieldReference_SetsFieldNameDirectly_NoExpressionUsed()
    {
        var (layer, warnings, infos) = ConvertLabeledLayer(Cim.LabelClass(expression: "[STRNAME]"));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal("STRNAME", renderer.FieldName);
        Assert.False(renderer.UseExpression);
        Assert.Empty(warnings);
        Assert.Empty(infos);
    }

    [Fact]
    public void ConvertibleExpression_UsesConvertedExpressionAndLogsInfo()
    {
        var (layer, warnings, infos) = ConvertLabeledLayer(Cim.LabelClass(expression: "\"Leerrohr vorh.\""));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.True(renderer.UseExpression);
        Assert.Equal("Leerrohr vorh.", renderer.LabelExpression);
        Assert.Empty(warnings);
        Assert.Contains(infos, i => i.Contains("converted to"));
    }

    [Fact]
    public void UnconvertibleExpression_KeepsOriginalExpressionAndLogsWarning()
    {
        var (layer, warnings, infos) = ConvertLabeledLayer(Cim.LabelClass(expression: "Trim([NAME])"));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.True(renderer.UseExpression);
        Assert.Equal("Trim([NAME])", renderer.LabelExpression);
        Assert.Empty(infos);
        Assert.Contains(warnings, w => w.Contains("complex"));
    }

    [Fact]
    public void NoExpressionOrFieldNames_ProducesRendererWithoutExpression()
    {
        var (layer, warnings, infos) = ConvertLabeledLayer(Cim.LabelClass(expression: null, fieldNames: null));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.False(renderer.UseExpression);
        Assert.Empty(warnings);
        Assert.Empty(infos);
    }

    [Fact]
    public void MissingExpression_FallsBackToFirstFieldName_PlainNameNoBrackets()
    {
        // CIM's "fieldNames" entries are plain field names, never bracketed - unlike
        // "expression", which uses "[Field]" syntax. The fallback must still be recognized as a
        // simple field reference (FieldName set, no expression/warning), not misrouted into
        // AprxLabelExpressionParser as if it were a VB expression.
        var (layer, warnings, infos) = ConvertLabeledLayer(Cim.LabelClass(expression: null, fieldNames: ["STRNAME", "OTHER"]));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal("STRNAME", renderer.FieldName);
        Assert.False(renderer.UseExpression);
        Assert.Empty(warnings);
        Assert.Empty(infos);
    }

    [Fact]
    public void MissingExpression_BracketedFieldNameFallback_IsNotDoubleBracketed()
    {
        // Defensive: if a FieldNames entry were ever already bracketed, the converter must not
        // wrap it a second time (which would turn "[STRNAME]" into "[[STRNAME]]" and break the
        // plain-field-reference match).
        var (layer, _, _) = ConvertLabeledLayer(Cim.LabelClass(expression: null, fieldNames: ["[STRNAME]"]));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.False(renderer.UseExpression);
    }

    [Fact]
    public void OnlyFirstLabelClass_IsUsed()
    {
        var warnings = new List<string>();
        var converter = new AprxMapConverter(warn: warnings.Add);
        var cimLayer = Cim.FeatureLayer(
            featureTable: Cim.FeatureTable(),
            labelVisibility: true,
            labelClasses: [Cim.LabelClass(expression: "[FIRST]"), Cim.LabelClass(expression: "[SECOND]")]);
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);

        var renderer = Assert.IsType<SimpleLabelRenderer>(((FeatureLayer)map.MapElements[0]).LabelRenderer);
        Assert.Equal("FIRST", renderer.FieldName);
    }

    // -----------------------------------------------------------------------
    // Text symbol building
    // -----------------------------------------------------------------------

    [Fact]
    public void TextSymbol_NoHalo_ProducesSimpleTextSymbol()
    {
        var (layer, _, _) = ConvertLabeledLayer(Cim.LabelClass(
            expression: "[NAME]",
            textSymbol: Cim.SymbolRef(Cim.TextSymbol(height: 12, fontFamilyName: "Arial", haloSize: 0))));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.IsType<SimpleTextSymbol>(renderer.TextSymbol);
        Assert.Equal(12f, renderer.TextSymbol!.Font.Size);
    }

    [Fact]
    public void TextSymbol_WithHalo_ProducesGlowingTextSymbol()
    {
        var (layer, _, _) = ConvertLabeledLayer(Cim.LabelClass(
            expression: "[NAME]",
            textSymbol: Cim.SymbolRef(Cim.TextSymbol(
                height: 10,
                haloSize: 1.5,
                haloSymbol: Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(255, 255, 255)))))));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        var glow = Assert.IsType<GlowingTextSymbol>(renderer.TextSymbol);
        Assert.Equal(255, glow.GlowingColor.R);
        Assert.Equal(2, glow.GlowingWidth); // Math.Round(1.5) -> 2 (banker's rounding to even)
    }

    [Fact]
    public void TextSymbol_FontColor_ComesFromTextFillSymbol()
    {
        var (layer, _, _) = ConvertLabeledLayer(Cim.LabelClass(
            expression: "[NAME]",
            textSymbol: Cim.SymbolRef(Cim.TextSymbol(
                textFillSymbol: Cim.SymbolRef(Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(10, 20, 30))))))));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        var simple = Assert.IsType<SimpleTextSymbol>(renderer.TextSymbol);
        Assert.Equal(10, simple.Color.R);
        Assert.Equal(20, simple.Color.G);
        Assert.Equal(30, simple.Color.B);
    }

    [Fact]
    public void TextSymbol_NoTextFillSymbol_DefaultsToBlack()
    {
        var (layer, _, _) = ConvertLabeledLayer(Cim.LabelClass(
            expression: "[NAME]",
            textSymbol: Cim.SymbolRef(Cim.TextSymbol())));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        var simple = Assert.IsType<SimpleTextSymbol>(renderer.TextSymbol);
        Assert.Equal(ArgbColor.Black.R, simple.Color.R);
        Assert.Equal(ArgbColor.Black.G, simple.Color.G);
        Assert.Equal(ArgbColor.Black.B, simple.Color.B);
    }

    [Theory]
    [InlineData("Bold", FontStyle.Bold)]
    [InlineData("Italic", FontStyle.Italic)]
    [InlineData("Bold Italic", FontStyle.Bold | FontStyle.Italic)]
    [InlineData("Regular", FontStyle.Regular)]
    [InlineData(null, FontStyle.Regular)]
    public void TextSymbol_FontStyle_IsResolvedFromFontStyleName(string? fontStyleName, FontStyle expected)
    {
        var (layer, _, _) = ConvertLabeledLayer(Cim.LabelClass(
            expression: "[NAME]",
            textSymbol: Cim.SymbolRef(Cim.TextSymbol(fontStyleName: fontStyleName))));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal(expected, renderer.TextSymbol!.Font.Style);
    }

    [Fact]
    public void TextSymbol_MissingHeight_DefaultsToTenPoints()
    {
        var (layer, _, _) = ConvertLabeledLayer(Cim.LabelClass(
            expression: "[NAME]",
            textSymbol: Cim.SymbolRef(Cim.TextSymbol(height: 0))));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal(10f, renderer.TextSymbol!.Font.Size);
    }

    [Fact]
    public void NoTextSymbol_LeavesRendererDefaultTextSymbol()
    {
        var (layer, _, _) = ConvertLabeledLayer(Cim.LabelClass(expression: "[NAME]", textSymbol: null));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.NotNull(renderer.TextSymbol); // SimpleLabelRenderer's own default, untouched
    }
}
