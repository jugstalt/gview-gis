using gView.Cmd.MxlUtil.Lib.Utilities.Aprx;
using gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Symbology;
using gView.Framework.Data;
using gView.Framework.Symbology;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Tests for <see cref="AprxMapConverter"/>'s feature-renderer conversion: simple, unique-value
/// (single and multi-field), class-breaks (falls back to a simple renderer, since gView has no
/// direct equivalent), and the rotation visual variable.
/// </summary>
public class RendererConversionTests
{
    private static FeatureLayer ConvertSingleLayer(
        CimRenderer renderer, out List<string> warnings)
    {
        var w = new List<string>();
        warnings = w;
        var converter = new AprxMapConverter(warn: w.Add);
        var cimLayer = Cim.FeatureLayer(name: "L", featureTable: Cim.FeatureTable(), renderer: renderer);
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);
        return (FeatureLayer)map.MapElements[0];
    }

    [Fact]
    public void SimpleRenderer_ConvertsSymbolAndLabel()
    {
        var cimRenderer = Cim.SimpleRenderer(
            Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(255, 0, 0))),
            label: "Aktiv");

        var layer = ConvertSingleLayer(cimRenderer, out _);

        var renderer = Assert.IsType<SimpleRenderer>(layer.FeatureRenderer);
        var legendItem = Assert.IsAssignableFrom<ILegendItem>(renderer.Symbol);
        Assert.Equal("Aktiv", legendItem.LegendLabel);
    }

    [Fact]
    public void UniqueValueRenderer_SingleField_BecomesValueMapRenderer()
    {
        var cimRenderer = Cim.UniqueValueRenderer(
            fields: ["TYP"],
            groups:
            [
                Cim.UniqueValueGroup(
                    Cim.UniqueValueClass(Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(255, 0, 0))), label: "Schieber", fieldValues: "Schieber"),
                    Cim.UniqueValueClass(Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(0, 255, 0))), label: "Ventil", fieldValues: "Ventil"))
            ]);

        var layer = ConvertSingleLayer(cimRenderer, out _);

        var renderer = Assert.IsType<ValueMapRenderer>(layer.FeatureRenderer);
        Assert.Equal("TYP", renderer.ValueField);
        Assert.NotNull(renderer["Schieber"]);
        Assert.NotNull(renderer["Ventil"]);
    }

    [Fact]
    public void UniqueValueRenderer_InvisibleClass_IsSkipped()
    {
        var cimRenderer = Cim.UniqueValueRenderer(
            fields: ["TYP"],
            groups:
            [
                Cim.UniqueValueGroup(
                    Cim.UniqueValueClass(Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(255, 0, 0))), visible: false, fieldValues: "Hidden"))
            ]);

        var layer = ConvertSingleLayer(cimRenderer, out _);

        var renderer = Assert.IsType<ValueMapRenderer>(layer.FeatureRenderer);
        Assert.Null(renderer["Hidden"]);
    }

    [Fact]
    public void UniqueValueRenderer_UseDefaultSymbol_SetsDefaultSymbolAndLabel()
    {
        var cimRenderer = Cim.UniqueValueRenderer(
            fields: ["TYP"],
            useDefaultSymbol: true,
            defaultSymbol: Cim.SymbolRef(Cim.PolygonSymbol(Cim.SolidFill(Cim.Gray(50)))),
            defaultLabel: "Sonstige");

        var layer = ConvertSingleLayer(cimRenderer, out _);

        var renderer = Assert.IsType<ValueMapRenderer>(layer.FeatureRenderer);
        Assert.NotNull(renderer.DefaultSymbol);
        Assert.Equal("Sonstige", ((ILegendItem)renderer.DefaultSymbol!).LegendLabel);
    }

    [Fact]
    public void UniqueValueRenderer_UseDefaultSymbolFalse_LeavesDefaultSymbolUnset()
    {
        var cimRenderer = Cim.UniqueValueRenderer(
            fields: ["TYP"],
            useDefaultSymbol: false,
            defaultSymbol: Cim.SymbolRef(Cim.PolygonSymbol(Cim.SolidFill(Cim.Gray(50)))));

        var layer = ConvertSingleLayer(cimRenderer, out _);

        var renderer = Assert.IsType<ValueMapRenderer>(layer.FeatureRenderer);
        Assert.Null(renderer.DefaultSymbol);
    }

    [Fact]
    public void UniqueValueRenderer_MultipleFields_BecomesManyValueMapRenderer()
    {
        var cimRenderer = Cim.UniqueValueRenderer(
            fields: ["TYP", "STATUS"],
            groups:
            [
                Cim.UniqueValueGroup(
                    Cim.UniqueValueClass(Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(255, 0, 0))), label: "A", fieldValues: ["Schieber", "Aktiv"]))
            ]);

        var layer = ConvertSingleLayer(cimRenderer, out _);

        var renderer = Assert.IsType<ManyValueMapRenderer>(layer.FeatureRenderer);
        Assert.Equal("TYP", renderer.ValueField1);
        Assert.Equal("STATUS", renderer.ValueField2);
        Assert.NotNull(renderer["Schieber|Aktiv"]);
    }

    [Fact]
    public void UniqueValueRenderer_ManyValue_EsriNullBecomesEmptyKeySegment()
    {
        var cimRenderer = Cim.UniqueValueRenderer(
            fields: ["TYP", "STATUS"],
            groups:
            [
                Cim.UniqueValueGroup(
                    Cim.UniqueValueClass(Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(255, 0, 0))), label: "A", fieldValues: ["Schieber", "<Null>"]))
            ]);

        var layer = ConvertSingleLayer(cimRenderer, out _);

        var renderer = Assert.IsType<ManyValueMapRenderer>(layer.FeatureRenderer);
        Assert.NotNull(renderer["Schieber|"]);
    }

    [Fact]
    public void ClassBreaksRenderer_FallsBackToSimpleRendererWithFirstBreaksSymbol()
    {
        var cimRenderer = Cim.ClassBreaksRenderer(
            Cim.ClassBreak(10, Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(255, 0, 0))), label: "0-10"),
            Cim.ClassBreak(20, Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(0, 255, 0))), label: "10-20"));

        var layer = ConvertSingleLayer(cimRenderer, out _);

        var renderer = Assert.IsType<SimpleRenderer>(layer.FeatureRenderer);
        Assert.NotNull(renderer.Symbol);
    }

    [Fact]
    public void UnsupportedRendererType_ProducesNullRendererAndWarning()
    {
        // The base CimRenderer type itself (no concrete subtype) isn't one of the three
        // supported renderer kinds.
        var layer = ConvertSingleLayer(new CimRenderer(), out var warnings);

        Assert.Null(layer.FeatureRenderer);
        Assert.Contains(warnings, w => w.Contains("could not be converted"));
    }

    // -----------------------------------------------------------------------
    // Rotation visual variable
    // -----------------------------------------------------------------------

    [Fact]
    public void SimpleRenderer_RotationVisualVariable_SimpleFieldReference_SetsSymbolRotation()
    {
        var cimRenderer = Cim.SimpleRenderer(
            Cim.PointSymbol(Cim.CharacterMarker(33)),
            visualVariables: [Cim.Rotation("[WINKEL]", "Arithmetic")]);

        var layer = ConvertSingleLayer(cimRenderer, out var warnings);

        var renderer = Assert.IsType<SimpleRenderer>(layer.FeatureRenderer);
        Assert.Equal("WINKEL", renderer.SymbolRotation.RotationFieldName);
        Assert.Equal(RotationType.ArithmeticMinus90, renderer.SymbolRotation.RotationType);
        Assert.Empty(warnings);
    }

    [Fact]
    public void SimpleRenderer_RotationVisualVariable_GeographicType_MapsToGeographicPlus90()
    {
        var cimRenderer = Cim.SimpleRenderer(
            Cim.PointSymbol(Cim.CharacterMarker(33)),
            visualVariables: [Cim.Rotation("[WINKEL]", "Geographic")]);

        var layer = ConvertSingleLayer(cimRenderer, out _);

        var renderer = Assert.IsType<SimpleRenderer>(layer.FeatureRenderer);
        Assert.Equal(RotationType.GeographicPlus90, renderer.SymbolRotation.RotationType);
    }

    [Fact]
    public void SimpleRenderer_RotationVisualVariable_ComplexExpression_IsIgnoredWithWarning()
    {
        var cimRenderer = Cim.SimpleRenderer(
            Cim.PointSymbol(Cim.CharacterMarker(33)),
            visualVariables: [Cim.Rotation("[WINKEL] + 90")]);

        var layer = ConvertSingleLayer(cimRenderer, out var warnings);

        var renderer = Assert.IsType<SimpleRenderer>(layer.FeatureRenderer);
        Assert.Equal("", renderer.SymbolRotation.RotationFieldName);
        Assert.Contains(warnings, w => w.Contains("rotation expression"));
    }

    [Fact]
    public void SimpleRenderer_NoRotationVisualVariable_LeavesRotationUnset()
    {
        var cimRenderer = Cim.SimpleRenderer(Cim.PointSymbol(Cim.CharacterMarker(33)));

        var layer = ConvertSingleLayer(cimRenderer, out var warnings);

        var renderer = Assert.IsType<SimpleRenderer>(layer.FeatureRenderer);
        Assert.Equal("", renderer.SymbolRotation.RotationFieldName);
        Assert.Empty(warnings);
    }
}
