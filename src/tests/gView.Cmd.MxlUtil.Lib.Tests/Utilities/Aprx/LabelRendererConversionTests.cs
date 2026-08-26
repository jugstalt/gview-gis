using gView.Cmd.MxlUtil.Lib.Utilities.Aprx;
using gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Symbology;
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
        CimLabelClass labelClass, CimRenderer? renderer = null, CimMap? map = null)
    {
        var warnings = new List<string>();
        var infos = new List<string>();
        var converter = new AprxMapConverter(warn: warnings.Add, info: infos.Add);
        var cimLayer = Cim.FeatureLayer(
            name: "L",
            featureTable: Cim.FeatureTable(),
            renderer: renderer,
            labelVisibility: true,
            labelClasses: [labelClass]);
        var result = new AprxMapResult(map ?? Cim.Map(), [cimLayer]);

        var converted = converter.Convert(result);
        return ((FeatureLayer)converted.MapElements[0], warnings, infos);
    }

    private static CimRenderer PointRenderer() =>
        Cim.SimpleRenderer(Cim.PointSymbol(Cim.CharacterMarker(characterIndex: 65)));

    private static CimRenderer LineRenderer() =>
        Cim.SimpleRenderer(Cim.LineSymbol(Cim.SolidStroke(Cim.Rgb(0, 0, 0))));

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
        // haloSize is in points; converted to pixels @96dpi (×96/72) before rounding:
        // 1.5 * 96/72 = 2.0 -> 2.
        Assert.Equal(2, glow.GlowingWidth);
    }

    [Fact]
    public void TextSymbol_WithBalloonCallout_ProducesBlockoutTextSymbol()
    {
        // ArcGIS Pro's Text Symbol -> Callout ("mask"/background box behind the label, e.g. a
        // white box for readability over busy geometry) has no wrapper - the CIM directly
        // carries a CIMBalloonCallout with a backgroundSymbol.
        var (layer, warnings, _) = ConvertLabeledLayer(Cim.LabelClass(
            expression: "[NAME]",
            textSymbol: Cim.SymbolRef(Cim.TextSymbol(
                height: 10,
                callout: Cim.BalloonCallout(Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(240, 240, 240))))))));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        var blockout = Assert.IsType<BlockoutTextSymbol>(renderer.TextSymbol);
        Assert.Equal(240, blockout.ColorOutline.R);
        Assert.Equal(240, blockout.ColorOutline.G);
        Assert.Equal(240, blockout.ColorOutline.B);
        Assert.Empty(warnings);
    }

    [Fact]
    public void TextSymbol_WithBalloonCallout_TakesPriorityOverHalo()
    {
        // A label class can carry both haloSize>0 and a callout at once - gView can't combine
        // BlockoutTextSymbol and GlowingTextSymbol (both are mutually exclusive SimpleTextSymbol
        // subclasses), so the (more deliberately authored) background box wins.
        var (layer, _, _) = ConvertLabeledLayer(Cim.LabelClass(
            expression: "[NAME]",
            textSymbol: Cim.SymbolRef(Cim.TextSymbol(
                haloSize: 1.5,
                haloSymbol: Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(255, 255, 255))),
                callout: Cim.BalloonCallout(Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(240, 240, 240))))))));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.IsType<BlockoutTextSymbol>(renderer.TextSymbol);
    }

    [Fact]
    public void TextSymbol_UnsupportedCalloutType_WarnsAndFallsBackToPlainText()
    {
        var (layer, warnings, _) = ConvertLabeledLayer(Cim.LabelClass(
            expression: "[NAME]",
            textSymbol: Cim.SymbolRef(Cim.TextSymbol(
                callout: new CimCallout { Type = "CIMLeaderCallout" }))));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.IsType<SimpleTextSymbol>(renderer.TextSymbol);
        Assert.Contains(warnings, w => w.Contains("CIMLeaderCallout"));
    }

    [Fact]
    public void TextSymbol_FontColor_ComesFromTextFillSymbol()
    {
        var (layer, _, _) = ConvertLabeledLayer(Cim.LabelClass(
            expression: "[NAME]",
            textSymbol: Cim.SymbolRef(Cim.TextSymbol(
                textFillSymbol: Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(10, 20, 30)))))));

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

    // -----------------------------------------------------------------------
    // Point label placement (around-point zone priorities -> TextSymbolAlignment)
    // -----------------------------------------------------------------------

    [Fact]
    public void PointPlacement_MaplexZonePriorities_OrderAlignmentsByPriorityAscending()
    {
        // A full ranking (1 = best .. 8 = worst) locks in the zone -> TextSymbolAlignment
        // mapping derived from SimpleTextSymbol's point placement math.
        var (layer, _, _) = ConvertLabeledLayer(
            Cim.LabelClass(
                expression: "[NAME]",
                maplexLabelPlacementProperties: Cim.MaplexLabelPlacementProperties(
                    pointExternalZonePriorities: Cim.PointZonePriorities(
                        aboveLeft: 1, aboveCenter: 2, aboveRight: 3,
                        centerLeft: 4, centerRight: 5,
                        belowLeft: 6, belowCenter: 7, belowRight: 8))),
            renderer: PointRenderer());

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal(TextSymbolAlignment.rightAlignOver, renderer.TextSymbol!.TextSymbolAlignment);
        Assert.Equal(
            [
                TextSymbolAlignment.rightAlignOver, TextSymbolAlignment.Over, TextSymbolAlignment.leftAlignOver,
                TextSymbolAlignment.rightAlignCenter, TextSymbolAlignment.leftAlignCenter,
                TextSymbolAlignment.rightAlignUnder, TextSymbolAlignment.Under, TextSymbolAlignment.leftAlignUnder,
            ],
            renderer.TextSymbol!.SecondaryTextSymbolAlignments);
    }

    [Fact]
    public void PointPlacement_BestZoneIsBelowRight_PicksLeftAlignUnder()
    {
        var (layer, _, _) = ConvertLabeledLayer(
            Cim.LabelClass(
                expression: "[NAME]",
                maplexLabelPlacementProperties: Cim.MaplexLabelPlacementProperties(
                    pointExternalZonePriorities: Cim.PointZonePriorities(belowRight: 1, aboveLeft: 2))),
            renderer: PointRenderer());

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal(TextSymbolAlignment.leftAlignUnder, renderer.TextSymbol!.TextSymbolAlignment);
    }

    [Fact]
    public void PointPlacement_ZeroPriority_MeansZoneIsProhibited_NotBestChoice()
    {
        // Per ArcGIS Pro's docs (both engines): priority 1 is tried first, and a priority of
        // *0 blocks the zone entirely* - it must NOT be picked as "lowest number = best".
        // Regression case straight from a real aprx (FG-Kunde): belowLeft=0 (prohibited) must
        // lose to aboveRight=1 (genuinely ranked first), not win because 0 < 1.
        var (layer, _, _) = ConvertLabeledLayer(
            Cim.LabelClass(
                expression: "[NAME]",
                standardLabelPlacementProperties: Cim.StandardLabelPlacementProperties(
                    pointPlacementPriorities: Cim.PointZonePriorities(
                        aboveLeft: 3, aboveCenter: 2, aboveRight: 1,
                        centerLeft: 3, centerRight: 2,
                        belowLeft: 0, belowCenter: 3, belowRight: 3))),
            renderer: PointRenderer(),
            map: Cim.Map(generalPlacementProperties: Cim.GeneralPlacementProperties(maplex: false)));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal(TextSymbolAlignment.leftAlignOver, renderer.TextSymbol!.TextSymbolAlignment); // aboveRight
        Assert.DoesNotContain(TextSymbolAlignment.rightAlignUnder, renderer.TextSymbol!.SecondaryTextSymbolAlignments); // belowLeft never offered
    }

    [Fact]
    public void PointPlacement_AllZonesProhibited_LeavesDefaultAlignmentUntouched()
    {
        var (layer, _, _) = ConvertLabeledLayer(
            Cim.LabelClass(
                expression: "[NAME]",
                maplexLabelPlacementProperties: Cim.MaplexLabelPlacementProperties(
                    pointExternalZonePriorities: Cim.PointZonePriorities(
                        aboveLeft: 0, aboveCenter: 0, aboveRight: 0, centerLeft: 0,
                        centerRight: 0, belowLeft: 0, belowCenter: 0, belowRight: 0))),
            renderer: PointRenderer());

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal(TextSymbolAlignment.Center, renderer.TextSymbol!.TextSymbolAlignment);
        Assert.Null(renderer.TextSymbol!.SecondaryTextSymbolAlignments);
    }

    [Fact]
    public void PointPlacement_MapUsesStandardEngine_UsesStandardZonePrioritiesNotMaplex()
    {
        // Both blocks are always present in CIM regardless of which engine is active; the map's
        // generalPlacementProperties decides which one actually governs placement in ArcGIS Pro.
        var (layer, _, _) = ConvertLabeledLayer(
            Cim.LabelClass(
                expression: "[NAME]",
                standardLabelPlacementProperties: Cim.StandardLabelPlacementProperties(
                    pointPlacementPriorities: Cim.PointZonePriorities(aboveRight: 1, belowRight: 2)),
                maplexLabelPlacementProperties: Cim.MaplexLabelPlacementProperties(
                    pointExternalZonePriorities: Cim.PointZonePriorities(belowRight: 1, aboveRight: 2))),
            renderer: PointRenderer(),
            map: Cim.Map(generalPlacementProperties: Cim.GeneralPlacementProperties(maplex: false)));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        // Standard's ranking (aboveRight=1) must win, not Maplex's (which would pick belowRight).
        Assert.Equal(TextSymbolAlignment.leftAlignOver, renderer.TextSymbol!.TextSymbolAlignment);
    }

    [Fact]
    public void PointPlacement_MapUsesMaplexByDefault_WhenGeneralPlacementPropertiesMissing()
    {
        var (layer, _, _) = ConvertLabeledLayer(
            Cim.LabelClass(
                expression: "[NAME]",
                maplexLabelPlacementProperties: Cim.MaplexLabelPlacementProperties(
                    pointExternalZonePriorities: Cim.PointZonePriorities(belowRight: 1, aboveRight: 2))),
            renderer: PointRenderer(),
            map: Cim.Map(generalPlacementProperties: null));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal(TextSymbolAlignment.leftAlignUnder, renderer.TextSymbol!.TextSymbolAlignment);
    }

    [Fact]
    public void PointPlacement_NonPointRenderer_LeavesDefaultAlignmentUntouched()
    {
        var (layer, _, _) = ConvertLabeledLayer(
            Cim.LabelClass(
                expression: "[NAME]",
                maplexLabelPlacementProperties: Cim.MaplexLabelPlacementProperties(
                    pointExternalZonePriorities: Cim.PointZonePriorities(belowRight: 1))),
            renderer: null); // no point symbol -> not recognized as a point layer

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal(TextSymbolAlignment.Center, renderer.TextSymbol!.TextSymbolAlignment);
        Assert.Null(renderer.TextSymbol!.SecondaryTextSymbolAlignments);
    }

    [Fact]
    public void PointPlacement_MethodNotAroundPoint_LeavesDefaultAlignmentUntouched()
    {
        var (layer, _, _) = ConvertLabeledLayer(
            Cim.LabelClass(
                expression: "[NAME]",
                maplexLabelPlacementProperties: Cim.MaplexLabelPlacementProperties(
                    pointPlacementMethod: "CenteredOnPoint",
                    pointExternalZonePriorities: Cim.PointZonePriorities(belowRight: 1))),
            renderer: PointRenderer());

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal(TextSymbolAlignment.Center, renderer.TextSymbol!.TextSymbolAlignment);
        Assert.Null(renderer.TextSymbol!.SecondaryTextSymbolAlignments);
    }

    // -----------------------------------------------------------------------
    // Line label placement (above/on/below the line -> TextSymbolAlignment Over/Center/Under)
    // -----------------------------------------------------------------------

    [Fact]
    public void LinePlacement_AboveAndInLineAllowed_PrefersAboveButKeepsInLineAsFallback()
    {
        // Real-world case (FG-Autobemaßungslinie): both above and inLine (centered) are
        // allowed - ArcGIS Pro tries "above" first.
        var (layer, _, _) = ConvertLabeledLayer(
            Cim.LabelClass(
                expression: "[LAENGE]",
                standardLabelPlacementProperties: Cim.StandardLabelPlacementProperties(
                    lineLabelPosition: Cim.LineLabelPosition(above: true, inLine: true))),
            renderer: LineRenderer(),
            map: Cim.Map(generalPlacementProperties: Cim.GeneralPlacementProperties(maplex: false)));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal(TextSymbolAlignment.Over, renderer.TextSymbol!.TextSymbolAlignment);
        Assert.Equal(
            [TextSymbolAlignment.Over, TextSymbolAlignment.Center],
            renderer.TextSymbol!.SecondaryTextSymbolAlignments);
    }

    [Fact]
    public void LinePlacement_OnlyInLineAllowed_PicksCenter()
    {
        var (layer, _, _) = ConvertLabeledLayer(
            Cim.LabelClass(
                expression: "[LAENGE]",
                standardLabelPlacementProperties: Cim.StandardLabelPlacementProperties(
                    lineLabelPosition: Cim.LineLabelPosition(inLine: true))),
            renderer: LineRenderer(),
            map: Cim.Map(generalPlacementProperties: Cim.GeneralPlacementProperties(maplex: false)));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal(TextSymbolAlignment.Center, renderer.TextSymbol!.TextSymbolAlignment);
    }

    [Fact]
    public void LinePlacement_AllThreePositionsAllowed_OrderIsAboveThenInLineThenBelow()
    {
        var (layer, _, _) = ConvertLabeledLayer(
            Cim.LabelClass(
                expression: "[LAENGE]",
                standardLabelPlacementProperties: Cim.StandardLabelPlacementProperties(
                    lineLabelPosition: Cim.LineLabelPosition(above: true, inLine: true, below: true))),
            renderer: LineRenderer(),
            map: Cim.Map(generalPlacementProperties: Cim.GeneralPlacementProperties(maplex: false)));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal(
            [TextSymbolAlignment.Over, TextSymbolAlignment.Center, TextSymbolAlignment.Under],
            renderer.TextSymbol!.SecondaryTextSymbolAlignments);
    }

    [Fact]
    public void LinePlacement_MaplexEngineActive_IsNotApplied()
    {
        // The Standard engine's simple above/inLine/below toggles have no Maplex equivalent
        // captured here, so they must be ignored when Maplex is the active engine.
        var (layer, _, _) = ConvertLabeledLayer(
            Cim.LabelClass(
                expression: "[LAENGE]",
                standardLabelPlacementProperties: Cim.StandardLabelPlacementProperties(
                    lineLabelPosition: Cim.LineLabelPosition(above: true))),
            renderer: LineRenderer(),
            map: Cim.Map(generalPlacementProperties: Cim.GeneralPlacementProperties(maplex: true)));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal(TextSymbolAlignment.Center, renderer.TextSymbol!.TextSymbolAlignment); // untouched default
        Assert.Null(renderer.TextSymbol!.SecondaryTextSymbolAlignments);
    }

    [Fact]
    public void LinePlacement_NoPositionInfo_LeavesDefaultAlignmentUntouched()
    {
        var (layer, _, _) = ConvertLabeledLayer(
            Cim.LabelClass(expression: "[LAENGE]"),
            renderer: LineRenderer());

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal(TextSymbolAlignment.Center, renderer.TextSymbol!.TextSymbolAlignment);
        Assert.Null(renderer.TextSymbol!.SecondaryTextSymbolAlignments);
    }

    [Fact]
    public void NumLabelsOption_OneLabelPerName_MapsToOnPerName()
    {
        var (layer, _, _) = ConvertLabeledLayer(Cim.LabelClass(
            expression: "[NAME]",
            standardLabelPlacementProperties: Cim.StandardLabelPlacementProperties(numLabelsOption: "OneLabelPerName")));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal(SimpleLabelRenderer.RenderHowManyLabels.OnPerName, renderer.HowManyLabels);
    }

    [Fact]
    public void NumLabelsOption_OneLabelPerPart_MapsToOnPerPart()
    {
        var (layer, _, _) = ConvertLabeledLayer(Cim.LabelClass(
            expression: "[NAME]",
            standardLabelPlacementProperties: Cim.StandardLabelPlacementProperties(numLabelsOption: "OneLabelPerPart")));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal(SimpleLabelRenderer.RenderHowManyLabels.OnPerPart, renderer.HowManyLabels);
    }

    [Fact]
    public void NumLabelsOption_Missing_LeavesRendererDefault()
    {
        var (layer, _, _) = ConvertLabeledLayer(Cim.LabelClass(expression: "[NAME]"));

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal(SimpleLabelRenderer.RenderHowManyLabels.OnPerFeature, renderer.HowManyLabels); // SimpleLabelRenderer's own default
    }
}
