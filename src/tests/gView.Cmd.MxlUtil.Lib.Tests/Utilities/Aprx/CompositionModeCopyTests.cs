using gView.Cmd.MxlUtil.Lib.Utilities.Aprx;
using gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Data;
using gView.Framework.Core.Symbology;
using gView.Framework.Data;
using gView.Framework.Symbology;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Tests for opting a transparent layer into <see cref="FeatureLayerCompositionMode.Copy"/>
/// instead of the default of baking its aprx "transparency" into every symbol color. Baking it
/// into colors produces a visibly darker seam wherever the layer's own features overlap
/// themselves (e.g. many crossing semi-transparent line symbols at the same transparency) - not
/// present in ArcGIS Pro, which always composites a layer once and applies its transparency to
/// the whole result. Reported against a real "Kabeltrassenstreifen" corridor layer.
/// </summary>
public class CompositionModeCopyTests
{
    private static FeatureLayer ConvertLayer(
        string name,
        double transparency,
        IEnumerable<string>? compositionModeCopyLayerPatterns = null)
    {
        var converter = new AprxMapConverter(compositionModeCopyLayerPatterns: compositionModeCopyLayerPatterns);
        var cimLayer = Cim.FeatureLayer(
            name: name,
            featureTable: Cim.FeatureTable(),
            renderer: Cim.SimpleRenderer(Cim.LineSymbol(Cim.SolidStroke(Cim.Rgb(255, 0, 0)))),
            transparency: transparency);
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);
        return (FeatureLayer)map.MapElements[0];
    }

    private static byte PenColorAlpha(FeatureLayer layer)
    {
        var renderer = (SimpleRenderer)layer.FeatureRenderer!;
        return ((IPenColor)renderer.Symbol!).PenColor.A;
    }

    [Fact]
    public void NoPatternsConfigured_BakesTransparencyIntoColorAsBefore()
    {
        var layer = ConvertLayer("Strom- und DL-Kabeltrassenstreifen", transparency: 60);

        Assert.Equal(FeatureLayerCompositionMode.Over, layer.CompositionMode);
        // 60% transparency -> 40% of 255 opacity remaining, baked straight into the color.
        Assert.Equal((byte)Math.Round(255 * 0.4), PenColorAlpha(layer));
    }

    [Fact]
    public void NameMatchesPattern_UsesCompositionModeCopyAndLeavesColorOpaque()
    {
        var layer = ConvertLayer(
            "Strom- und DL-Kabeltrassenstreifen",
            transparency: 60,
            compositionModeCopyLayerPatterns: ["*streifen*"]);

        Assert.Equal(FeatureLayerCompositionMode.Copy, layer.CompositionMode);
        Assert.Equal(60f, layer.CompositionModeCopyTransparency);
        // Color stays at its own full alpha - the transparency is applied once, to the whole
        // composited layer, not per color.
        Assert.Equal((byte)255, PenColorAlpha(layer));
    }

    [Fact]
    public void PatternMatchIsCaseInsensitive()
    {
        var layer = ConvertLayer(
            "Kabeltrassenstreifen",
            transparency: 60,
            compositionModeCopyLayerPatterns: ["*STREIFEN*"]);

        Assert.Equal(FeatureLayerCompositionMode.Copy, layer.CompositionMode);
    }

    [Fact]
    public void NameDoesNotMatchAnyPattern_FallsBackToBakedTransparency()
    {
        var layer = ConvertLayer(
            "NS-Leitungen",
            transparency: 60,
            compositionModeCopyLayerPatterns: ["*streifen*", "*Kabeltrasse*"]);

        Assert.Equal(FeatureLayerCompositionMode.Over, layer.CompositionMode);
        Assert.Equal((byte)Math.Round(255 * 0.4), PenColorAlpha(layer));
    }

    [Fact]
    public void ZeroTransparency_PatternMatchIsANoOp()
    {
        var layer = ConvertLayer(
            "Strom-Kabeltrassenstreifen",
            transparency: 0,
            compositionModeCopyLayerPatterns: ["*streifen*"]);

        Assert.Equal(FeatureLayerCompositionMode.Over, layer.CompositionMode);
        Assert.Equal((byte)255, PenColorAlpha(layer));
    }

    [Fact]
    public void MultiplePatterns_MatchesAnyOfThem()
    {
        var layer = ConvertLayer(
            "Fernwaerme-Trassenband",
            transparency: 50,
            compositionModeCopyLayerPatterns: ["*streifen*", "*trassenband*"]);

        Assert.Equal(FeatureLayerCompositionMode.Copy, layer.CompositionMode);
        Assert.Equal(50f, layer.CompositionModeCopyTransparency);
    }
}
