using gView.Cmd.MxlUtil.Lib.Utilities.Aprx;
using gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;
using gView.Framework.Data;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Tests for <see cref="AprxMapConverter"/>'s "invisible anchor" detection: a renderer whose
/// every color is fully transparent (alpha 0) - a common real-world pattern for label-only
/// layers (a point feature that exists purely to carry a label, with its own marker symbol
/// deliberately made invisible) - gets no FeatureRenderer at all, matching gView's own
/// "Render features for this layer" checkbox being unchecked, instead of a renderer that always
/// draws nothing. See PolygonSymbol_FullyTransparentFill_ProducesNoFeatureRendererAtAll in
/// SymbolConversionTests.cs for the simplest single-fill case.
/// </summary>
public class TransparentRendererTests
{
    private static FeatureLayer ConvertLayer(CimRenderer renderer)
    {
        var converter = new AprxMapConverter();
        var cimLayer = Cim.FeatureLayer(featureTable: Cim.FeatureTable(), renderer: renderer);
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);
        return (FeatureLayer)map.MapElements[0];
    }

    [Fact]
    public void LineSymbol_FullyTransparentStroke_ProducesNoFeatureRenderer()
    {
        var layer = ConvertLayer(Cim.SimpleRenderer(Cim.LineSymbol(Cim.SolidStroke(Cim.Rgb(0, 0, 0, 0)))));

        Assert.Null(layer.FeatureRenderer);
    }

    [Fact]
    public void PointSymbol_FullyTransparentCharacterMarker_ProducesNoFeatureRenderer()
    {
        // Matches the real-world case this was found from (e.g. NS-Kasten-Beschriftung): a
        // CIMCharacterMarker whose nested fill symbol's color has alpha 0.
        var layer = ConvertLayer(Cim.SimpleRenderer(Cim.PointSymbol(new CimCharacterMarker
        {
            CharacterIndex = 33,
            FontFamilyName = "ESRI Default Marker",
            Symbol = Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(212, 208, 200, 0)))
        })));

        Assert.Null(layer.FeatureRenderer);
    }

    [Fact]
    public void PolygonSymbol_MixOfTransparentAndOpaqueColors_StillRenders()
    {
        // Fill invisible, outline stroke visible - at least one color shows something, so the
        // renderer must still be attached.
        var layer = ConvertLayer(Cim.SimpleRenderer(Cim.PolygonSymbol(
            Cim.SolidFill(Cim.Rgb(255, 0, 0, 0)),
            Cim.SolidStroke(Cim.Rgb(0, 0, 0, 100)))));

        Assert.NotNull(layer.FeatureRenderer);
    }

    [Fact]
    public void UniqueValueRenderer_EveryClassFullyTransparent_ProducesNoFeatureRenderer()
    {
        var layer = ConvertLayer(Cim.UniqueValueRenderer(
            fields: ["CAT"],
            groups:
            [
                Cim.UniqueValueGroup(
                    Cim.UniqueValueClass(Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(255, 0, 0, 0))), fieldValues: "A"),
                    Cim.UniqueValueClass(Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(0, 255, 0, 0))), fieldValues: "B"))
            ]));

        Assert.Null(layer.FeatureRenderer);
    }

    [Fact]
    public void UniqueValueRenderer_OneClassOpaque_StillRenders()
    {
        // Even if most classes are invisible, a renderer that shows *something* for at least one
        // value must still be attached - it isn't uniformly an "invisible anchor" layer.
        var layer = ConvertLayer(Cim.UniqueValueRenderer(
            fields: ["CAT"],
            groups:
            [
                Cim.UniqueValueGroup(
                    Cim.UniqueValueClass(Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(255, 0, 0, 0))), fieldValues: "A"),
                    Cim.UniqueValueClass(Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(0, 255, 0, 100))), fieldValues: "B"))
            ]));

        Assert.NotNull(layer.FeatureRenderer);
    }

    [Fact]
    public void PictureMarker_IsNeverTreatedAsInvisible_EvenWithNoOtherColors()
    {
        // A picture-based symbol has no CIM color to inspect (referenced by URL) - conservatively
        // assume it might be visible rather than silently dropping a genuinely visible image
        // layer.
        var layer = ConvertLayer(Cim.SimpleRenderer(Cim.PointSymbol(new CimPictureMarker { Url = "marker.png" })));

        Assert.NotNull(layer.FeatureRenderer);
    }
}
