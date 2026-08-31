using gView.Cmd.MxlUtil.Lib.Utilities.Aprx;
using gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Data;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Tests for <see cref="AprxMapConverter"/>'s <see cref="AdvancedLabelRenderer"/> wiring: a label
/// class whose expression wraps every branch's output in ArcGIS Pro's per-branch
/// <c>&lt;CLR red=.. green=.. blue=..&gt;</c> tag produces an <see cref="AdvancedLabelRenderer"/>
/// with a populated <see cref="AdvancedLabelRenderer.ColorExpression"/> instead of the usual plain
/// <see cref="SimpleLabelRenderer"/> - every other label class (the overwhelming majority) must
/// keep producing a plain <see cref="SimpleLabelRenderer"/> exactly as before this feature existed.
/// </summary>
public class AdvancedLabelRendererConversionTests
{
    private static (FeatureLayer Layer, List<string> Warnings, List<string> Infos) ConvertLabeledLayer(CimLabelClass labelClass)
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

        var converted = converter.Convert(result);
        return ((FeatureLayer)converted.MapElements[0], warnings, infos);
    }

    private const string GasleitungenExpression = """
        Function FindLabel ([SA4],[SA1],[SA2])
        If [SA1] ="N" AND [SA2] ="HA" Then
          FindLabel = "<CLR red='255' green='0' blue='0'>" &[SA4]&"</CLR>"
        ElseIf [SA1] ="M" AND [SA2] ="HA" Then
          FindLabel = "<CLR red='56' green='168' blue='0'>" &[SA4]&"</CLR>"
        End If
        End Function
        """;

    [Fact]
    public void ClrTaggedExpression_ProducesAdvancedLabelRendererWithColorExpression()
    {
        var (layer, warnings, infos) = ConvertLabeledLayer(Cim.LabelClass(expression: GasleitungenExpression));

        var renderer = Assert.IsType<AdvancedLabelRenderer>(layer.LabelRenderer);
        Assert.True(renderer.UseExpression);
        Assert.DoesNotContain("<CLR", renderer.LabelExpression, StringComparison.OrdinalIgnoreCase);
        Assert.False(string.IsNullOrEmpty(renderer.ColorExpression));
        Assert.Contains("rgb(", renderer.ColorExpression);
        Assert.Empty(warnings);
        Assert.Contains(infos, i => i.Contains("<CLR") && i.Contains("color expression"));
    }

    [Fact]
    public void PlainExpression_NoClrTag_StillProducesPlainSimpleLabelRenderer()
    {
        var (layer, _, _) = ConvertLabeledLayer(Cim.LabelClass(expression: "[NAME]"));

        // Assert.IsType checks the *exact* type - this fails if a plain field reference ever
        // started producing an AdvancedLabelRenderer unnecessarily.
        Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
    }

    [Fact]
    public void ConvertibleExpression_NoClrTag_StillProducesPlainSimpleLabelRenderer()
    {
        var (layer, _, _) = ConvertLabeledLayer(Cim.LabelClass(expression: "\"Leerrohr vorh.\""));

        Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
    }

    [Fact]
    public void FntTagOnly_NoClrTag_StillProducesPlainSimpleLabelRenderer()
    {
        // <FNT> is stripped like before, but never produces a ColorExpression - must not trigger
        // AdvancedLabelRenderer either.
        const string source = """
            Function FindLabel ([TEXT])
            FindLabel = "<FNT size='8'>" & [TEXT] & "</FNT>"
            End Function
            """;
        var (layer, _, _) = ConvertLabeledLayer(Cim.LabelClass(expression: source));

        Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
    }

    [Fact]
    public void ClrTaggedExpression_TextColorEqualsHaloColor_NoLongerWarns()
    {
        // Same "text color equals halo color" setup as
        // LabelRendererConversionTests.TextSymbol_TextColorEqualsHaloColor_FallsBackToContrastingColor,
        // but this time the real per-feature color IS recovered dynamically - the "this converter
        // does not evaluate <CLR> tags" warning would now be stale/misleading and must not fire.
        var (layer, warnings, _) = ConvertLabeledLayer(Cim.LabelClass(
            expression: GasleitungenExpression,
            textSymbol: Cim.SymbolRef(Cim.TextSymbol(
                height: 10,
                haloSize: 1,
                textFillSymbol: Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(240, 240, 240))),
                haloSymbol: Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(240, 240, 240)))))));

        Assert.IsType<AdvancedLabelRenderer>(layer.LabelRenderer);
        Assert.DoesNotContain(warnings, w => w.Contains("halo"));
    }
}
