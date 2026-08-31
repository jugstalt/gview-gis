using gView.Cmd.MxlUtil.Lib.Utilities.Aprx;
using gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Symbology;
using gView.Framework.Data;
using gView.Framework.Symbology;
using gView.GraphicsEngine;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Tests for <see cref="AprxMapConverter"/>'s symbol conversion (point/line/polygon,
/// character markers, hatch fills, dash patterns, unsupported-layer fallbacks) and color
/// conversion (RGB/CMYK/Gray/HSV, ESRI's 0-100 alpha).
/// </summary>
public class SymbolConversionTests
{
    private static ISymbol ConvertSimpleSymbol(
        CimSymbol symbol,
        out List<string> warnings,
        double transparency = 0,
        string? glyphCenteringCorrection = null)
    {
        var w = new List<string>();
        warnings = w;
        var converter = new AprxMapConverter(warn: w.Add, glyphCenteringCorrection: glyphCenteringCorrection);
        var cimLayer = Cim.FeatureLayer(
            featureTable: Cim.FeatureTable(),
            renderer: Cim.SimpleRenderer(symbol),
            transparency: transparency);
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);
        var renderer = (SimpleRenderer)((FeatureLayer)map.MapElements[0]).FeatureRenderer!;
        return renderer.Symbol!;
    }

    // -----------------------------------------------------------------------
    // Point symbols
    // -----------------------------------------------------------------------

    [Fact]
    public void PointSymbol_CharacterMarker_ProducesTrueTypeMarkerSymbol()
    {
        var symbol = ConvertSimpleSymbol(
            Cim.PointSymbol(Cim.CharacterMarker(characterIndex: 65, size: 12, color: Cim.Rgb(0, 0, 255))),
            out _);

        var marker = Assert.IsType<TrueTypeMarkerSymbol>(symbol);
        Assert.Equal((byte)65, marker.Charakter.Value);
        Assert.Equal(255, marker.Color.B);
    }

    [Fact]
    public void PointSymbol_CharacterMarkerColorFromNestedFillSymbol_WhenNoDirectColor()
    {
        var symbol = ConvertSimpleSymbol(
            Cim.PointSymbol(new CimCharacterMarker
            {
                CharacterIndex = 65,
                Symbol = Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(10, 20, 30)))
            }),
            out _);

        var marker = Assert.IsType<TrueTypeMarkerSymbol>(symbol);
        Assert.Equal(10, marker.Color.R);
        Assert.Equal(20, marker.Color.G);
        Assert.Equal(30, marker.Color.B);
    }

    [Fact]
    public void PointSymbol_NoMarkerLayers_FallsBackToSimplePointFromFillAndStroke()
    {
        var symbol = ConvertSimpleSymbol(
            Cim.PointSymbol(Cim.SolidFill(Cim.Rgb(255, 0, 0)), Cim.SolidStroke(Cim.Rgb(0, 0, 0), width: 2)),
            out _);

        var point = Assert.IsType<SimplePointSymbol>(symbol);
        Assert.Equal(255, point.FillColor.R);
        Assert.Equal(2f * 96f / 72f, point.PenWidth); // aprx width is in points, gView PenWidth in pixels @96dpi
    }

    [Fact]
    public void PointSymbol_DisabledLayer_IsSkipped()
    {
        var symbol = ConvertSimpleSymbol(
            Cim.PointSymbol(Cim.CharacterMarker(characterIndex: 65, enable: false)),
            out _);

        // The disabled character marker is skipped, falling back to a plain point (no fill/stroke -> defaults).
        Assert.IsType<SimplePointSymbol>(symbol);
    }

    [Fact]
    public void PointSymbol_MultipleMarkerLayers_ProducesSymbolCollectionInReverseOrder()
    {
        var symbol = ConvertSimpleSymbol(
            Cim.PointSymbol(
                Cim.CharacterMarker(characterIndex: 1),
                Cim.CharacterMarker(characterIndex: 2)),
            out _);

        var collection = Assert.IsType<SymbolCollection>(symbol);
        Assert.Equal(2, collection.Symbols.Count);
        // Source order is drawn bottom-to-top in CIM; the converter reverses so index 0 is the
        // *last* CIM layer (drawn last = visually on top).
        var first = Assert.IsType<TrueTypeMarkerSymbol>(collection.Symbols[0].Symbol);
        var second = Assert.IsType<TrueTypeMarkerSymbol>(collection.Symbols[1].Symbol);
        Assert.Equal((byte)2, first.Charakter.Value);
        Assert.Equal((byte)1, second.Charakter.Value);
    }

    // Character-marker offset tests use a *differential* assertion: every converted
    // TrueTypeMarkerSymbol also carries an automatic glyph-ink-centering correction (see
    // GetGlyphCenteringCorrectionFraction) whose exact value depends on the font actually
    // resolved on the machine running the test, so it can't be asserted directly. Comparing
    // against a same-font/char/size baseline conversion (with no anchorPoint/offset) cancels
    // that shared term out, leaving just the contribution under test.

    private static TrueTypeMarkerSymbol ConvertCharacterMarkerSymbol(CimCharacterMarker marker) =>
        Assert.IsType<TrueTypeMarkerSymbol>(ConvertSimpleSymbol(Cim.PointSymbol(marker), out _));

    [Fact]
    public void PointSymbol_CharacterMarkerWithoutAnchorOrOffset_IsDeterministic()
    {
        // No anchorPoint/offsetX/Y -> whatever offset ends up on the symbol is purely the
        // automatic glyph-centering correction, which must be stable for the same font/char.
        var marker1 = ConvertCharacterMarkerSymbol(Cim.CharacterMarker(characterIndex: 65));
        var marker2 = ConvertCharacterMarkerSymbol(Cim.CharacterMarker(characterIndex: 65));

        Assert.Equal(marker1.HorizontalOffset, marker2.HorizontalOffset);
        Assert.Equal(marker1.VerticalOffset, marker2.VerticalOffset);
    }

    [Fact]
    public void PointSymbol_CharacterMarkerRelativeAnchorPoint_ProducesScaledInverseOffset()
    {
        var baseline = ConvertCharacterMarkerSymbol(Cim.CharacterMarker(characterIndex: 65, size: 12));

        // A relative anchor point of (0.5, 0.5) on a size-12 marker (half-extent = 6pt) means
        // "place the point half-way to the top-right corner (3pt, 3pt in symbol space) at the
        // feature", which pulls the glyph itself down-left of the feature -> gView offset
        // (-3, 3) in screen (y-down) coordinates, on top of the (shared, cancelled-out) glyph
        // centering correction.
        var marker = ConvertCharacterMarkerSymbol(Cim.CharacterMarker(
            characterIndex: 65,
            size: 12,
            anchorPoint: Cim.Point2D(0.5, 0.5)));

        Assert.Equal(-3f, marker.HorizontalOffset - baseline.HorizontalOffset, 3);
        Assert.Equal(3f, marker.VerticalOffset - baseline.VerticalOffset, 3);
    }

    [Fact]
    public void PointSymbol_CharacterMarkerAbsoluteAnchorPoint_IsNotScaledBySize()
    {
        var baseline = ConvertCharacterMarkerSymbol(Cim.CharacterMarker(characterIndex: 65, size: 12));

        var marker = ConvertCharacterMarkerSymbol(Cim.CharacterMarker(
            characterIndex: 65,
            size: 12,
            anchorPoint: Cim.Point2D(3, -2),
            anchorPointUnits: "Absolute"));

        Assert.Equal(-3f, marker.HorizontalOffset - baseline.HorizontalOffset, 3);
        Assert.Equal(-2f, marker.VerticalOffset - baseline.VerticalOffset, 3);
    }

    [Fact]
    public void PointSymbol_CharacterMarkerOffsetXY_MapsDirectlyWithYFlipped()
    {
        var baseline = ConvertCharacterMarkerSymbol(Cim.CharacterMarker(characterIndex: 65));

        var marker = ConvertCharacterMarkerSymbol(Cim.CharacterMarker(characterIndex: 65, offsetX: 4, offsetY: 5));

        Assert.Equal(4f, marker.HorizontalOffset - baseline.HorizontalOffset, 3);
        Assert.Equal(-5f, marker.VerticalOffset - baseline.VerticalOffset, 3);
    }

    // -----------------------------------------------------------------------
    // glyph-centering-correction: a manual, per font+character override of the automatic
    // ink-centering correction above - there's no reliable automatic way to tell a font glyph
    // that legitimately needs its whole ink centered (e.g. "?"/"i"/"j" and their dot) apart from
    // one that bakes in a dominant shape plus an unrelated, deliberately off-center attached
    // label (some ArcGIS Pro dingbat fonts do this), and simply disabling the correction for such
    // a glyph would leave it at gView's plain font-metrics centering - the very thing the
    // automatic correction exists to fix - rather than actually centered. So this takes an exact,
    // user-measured replacement (at a given reference font size) instead of a heuristic - read
    // off directly as gView's own HorizontalOffset/VerticalOffset for some already-centered
    // instance of the glyph (e.g. nudged in gView.Carto's symbol editor), which means it
    // *replaces* a marker's own anchorPoint/offsetX/offsetY entirely rather than adding on top of
    // them (unlike the automatic correction) - the observed "centered" state at calibration time
    // already reflects whatever anchor/offset that instance needed.
    // -----------------------------------------------------------------------

    [Fact]
    public void PointSymbol_CharacterMarkerCenteringOverride_ScalesToMarkerSize()
    {
        // "At font size 36, this glyph is centered with offset (18, -9)" -> at half that marker
        // size (18), the override must scale down by the same factor (9, -4.5).
        var symbol = ConvertSimpleSymbol(
            Cim.PointSymbol(Cim.CharacterMarker(characterIndex: 65, fontFamilyName: "Arial", size: 18)),
            out _,
            glyphCenteringCorrection: "Arial:65(36,18,-9)");

        var marker = Assert.IsType<TrueTypeMarkerSymbol>(symbol);
        Assert.Equal(9f, marker.HorizontalOffset, 3);
        Assert.Equal(-4.5f, marker.VerticalOffset, 3);
    }

    [Fact]
    public void PointSymbol_CharacterMarkerCenteringOverride_ReplacesRatherThanAddsToAutomaticCorrection()
    {
        // At the override's own reference size, the resulting offset must be exactly the
        // override's X/Y - not that plus whatever the automatic ink-scan would have measured for
        // this font/char.
        var symbol = ConvertSimpleSymbol(
            Cim.PointSymbol(Cim.CharacterMarker(characterIndex: 65, fontFamilyName: "Arial", size: 36)),
            out _,
            glyphCenteringCorrection: "Arial:65(36,18,-9)");

        var marker = Assert.IsType<TrueTypeMarkerSymbol>(symbol);
        Assert.Equal(18f, marker.HorizontalOffset, 3);
        Assert.Equal(-9f, marker.VerticalOffset, 3);
    }

    [Fact]
    public void PointSymbol_CharacterMarkerCenteringOverride_ReplacesRatherThanAddsToOwnAnchorPoint()
    {
        // A marker with its own real anchorPoint (e.g. -1,-2 Absolute, matching the real
        // "NS-Muffe" case this feature was built for) must have that anchor *replaced*, not added
        // to, by the override - the override was calibrated by nudging a glyph to visually
        // centered, which already includes whatever offset was needed, so re-adding this marker's
        // own separate anchorPoint on top would double-count it.
        var symbol = ConvertSimpleSymbol(
            Cim.PointSymbol(Cim.CharacterMarker(
                characterIndex: 65,
                fontFamilyName: "Arial",
                size: 36,
                anchorPoint: Cim.Point2D(-1, -2),
                anchorPointUnits: "Absolute",
                offsetX: 5,
                offsetY: 7)),
            out _,
            glyphCenteringCorrection: "Arial:65(36,18,-9)");

        var marker = Assert.IsType<TrueTypeMarkerSymbol>(symbol);
        Assert.Equal(18f, marker.HorizontalOffset, 3);
        Assert.Equal(-9f, marker.VerticalOffset, 3);
    }

    [Fact]
    public void PointSymbol_CharacterMarkerCenteringOverride_IsFontAndCharacterSpecific()
    {
        // An override for "Arial:65" ('A') must not affect a different character on the same font.
        var baseline = ConvertSimpleSymbol(
            Cim.PointSymbol(Cim.CharacterMarker(characterIndex: 66, fontFamilyName: "Arial")),
            out _);
        var withUnrelatedOverride = ConvertSimpleSymbol(
            Cim.PointSymbol(Cim.CharacterMarker(characterIndex: 66, fontFamilyName: "Arial")),
            out _,
            glyphCenteringCorrection: "Arial:65(36,18,-9)");

        var baselineMarker = Assert.IsType<TrueTypeMarkerSymbol>(baseline);
        var withUnrelatedOverrideMarker = Assert.IsType<TrueTypeMarkerSymbol>(withUnrelatedOverride);
        Assert.Equal(baselineMarker.HorizontalOffset, withUnrelatedOverrideMarker.HorizontalOffset);
        Assert.Equal(baselineMarker.VerticalOffset, withUnrelatedOverrideMarker.VerticalOffset);
    }

    [Fact]
    public void PointSymbol_CharacterMarkerCenteringOverride_FontNameIsCaseInsensitive()
    {
        var symbol = ConvertSimpleSymbol(
            Cim.PointSymbol(Cim.CharacterMarker(characterIndex: 65, fontFamilyName: "Arial", size: 36)),
            out _,
            glyphCenteringCorrection: "aRiAl:65(36,18,-9)");

        var marker = Assert.IsType<TrueTypeMarkerSymbol>(symbol);
        Assert.Equal(18f, marker.HorizontalOffset, 3);
        Assert.Equal(-9f, marker.VerticalOffset, 3);
    }

    [Fact]
    public void PointSymbol_CharacterMarkerCenteringOverride_MultipleEntriesInOneString()
    {
        var first = ConvertSimpleSymbol(
            Cim.PointSymbol(Cim.CharacterMarker(characterIndex: 65, fontFamilyName: "Arial", size: 36)),
            out _,
            glyphCenteringCorrection: "Arial:65(36,18,-9),Arial:66(36,4,5)");
        var second = ConvertSimpleSymbol(
            Cim.PointSymbol(Cim.CharacterMarker(characterIndex: 66, fontFamilyName: "Arial", size: 36)),
            out _,
            glyphCenteringCorrection: "Arial:65(36,18,-9),Arial:66(36,4,5)");

        var firstMarker = Assert.IsType<TrueTypeMarkerSymbol>(first);
        var secondMarker = Assert.IsType<TrueTypeMarkerSymbol>(second);
        Assert.Equal(18f, firstMarker.HorizontalOffset, 3);
        Assert.Equal(-9f, firstMarker.VerticalOffset, 3);
        Assert.Equal(4f, secondMarker.HorizontalOffset, 3);
        Assert.Equal(5f, secondMarker.VerticalOffset, 3);
    }

    [Fact]
    public void PointSymbol_CharacterMarkerCenteringOverride_MalformedEntry_WarnsAndIsIgnored()
    {
        var symbol = ConvertSimpleSymbol(
            Cim.PointSymbol(Cim.CharacterMarker(characterIndex: 65, fontFamilyName: "Arial")),
            out var warnings,
            glyphCenteringCorrection: "not-a-valid-entry");

        Assert.IsType<TrueTypeMarkerSymbol>(symbol);
        Assert.Contains(warnings, w => w.Contains("glyph-centering-correction"));
    }

    [Theory]
    // No ink found (blank/missing glyph) -> no correction.
    [InlineData(150f, 10, -1, 96f / 72f, 100f, 0f)]
    // Ink exactly centered on the draw point -> no correction needed.
    [InlineData(150f, 100, 200, 96f / 72f, 100f, 0f)]
    // Ink sits entirely below the draw point (in pixel/screen-down terms) by 96px at
    // measureSize=100 and 96 px/inch (i.e. 1 nominal unit per pixel) -> 1 unit of
    // correction is needed to pull the *next* draw point up so the ink re-centers.
    [InlineData(0f, 96, 96, 1f, 100f, -0.96f)]
    public void ComputeAxisCorrectionFraction_MatchesExpected(
        float drawCenter, int inkMin, int inkMax, float pixelsPerNominalUnit, float measureSize, float expected)
    {
        var result = AprxMapConverter.ComputeAxisCorrectionFraction(drawCenter, inkMin, inkMax, pixelsPerNominalUnit, measureSize);

        Assert.Equal(expected, result, 4);
    }

    // -----------------------------------------------------------------------
    // Line symbols
    // -----------------------------------------------------------------------

    [Fact]
    public void LineSymbol_SolidStroke_ProducesSimpleLineSymbolWithColorAndWidth()
    {
        var symbol = ConvertSimpleSymbol(
            Cim.LineSymbol(Cim.SolidStroke(Cim.Rgb(255, 128, 0), width: 3)),
            out _);

        var line = Assert.IsType<SimpleLineSymbol>(symbol);
        Assert.Equal(255, line.PenColor.R);
        Assert.Equal(128, line.PenColor.G);
        Assert.Equal(3f * 96f / 72f, line.PenWidth); // aprx width is in points, gView PenWidth in pixels @96dpi
    }

    [Fact]
    public void LineSymbol_NoStrokeLayers_ProducesDefaultSimpleLineSymbol()
    {
        var symbol = ConvertSimpleSymbol(Cim.LineSymbol(), out _);

        Assert.IsType<SimpleLineSymbol>(symbol);
    }

    [Fact]
    public void LineSymbol_DisabledStroke_IsSkipped()
    {
        var symbol = ConvertSimpleSymbol(
            Cim.LineSymbol(Cim.SolidStroke(Cim.Rgb(255, 0, 0), enable: false)),
            out _);

        Assert.IsType<SimpleLineSymbol>(symbol); // falls back to the default, not the disabled one
    }

    [Fact]
    public void LineSymbol_MultipleStrokes_ProducesSymbolCollectionInReverseOrder()
    {
        var symbol = ConvertSimpleSymbol(
            Cim.LineSymbol(
                Cim.SolidStroke(Cim.Rgb(255, 0, 0), width: 1),
                Cim.SolidStroke(Cim.Rgb(0, 255, 0), width: 2)),
            out _);

        var collection = Assert.IsType<SymbolCollection>(symbol);
        Assert.Equal(2, collection.Symbols.Count);
        var first = Assert.IsType<SimpleLineSymbol>(collection.Symbols[0].Symbol);
        Assert.Equal(2f * 96f / 72f, first.PenWidth); // second CIM layer drawn last -> first after reversing
    }

    [Fact]
    public void LineSymbol_Width_IsConvertedFromPointsToPixelsAt96Dpi()
    {
        // ArcGIS Pro expresses stroke widths in points (1/72"); gView's PenWidth is plain
        // pixels with an implicit 96dpi baseline. A "1pt" line must come out ~1.33px, not 1px -
        // otherwise every converted line renders visibly thinner than in ArcGIS/AGS.
        var symbol = ConvertSimpleSymbol(
            Cim.LineSymbol(Cim.SolidStroke(Cim.Rgb(0, 0, 0), width: 1)),
            out _);

        var line = Assert.IsType<SimpleLineSymbol>(symbol);
        Assert.Equal(96f / 72f, line.PenWidth, precision: 4);
    }

    [Theory]
    [InlineData(new double[] { 1, 3 }, LineDashStyle.Dot)]      // short dash relative to gap -> dot
    [InlineData(new double[] { 4, 2 }, LineDashStyle.Dash)]
    [InlineData(new double[] { 4, 2, 1, 2 }, LineDashStyle.DashDot)]
    [InlineData(new double[] { 4, 2, 1, 2, 1, 2 }, LineDashStyle.DashDotDot)]
    public void LineSymbol_DashTemplate_ResolvesToClosestDashStyle(double[] template, LineDashStyle expected)
    {
        var symbol = ConvertSimpleSymbol(
            Cim.LineSymbol(Cim.SolidStroke(Cim.Rgb(0, 0, 0), effects: [Cim.Dashes(template)])),
            out _);

        var line = Assert.IsType<SimpleLineSymbol>(symbol);
        Assert.Equal(expected, line.DashStyle);
    }

    [Fact]
    public void LineSymbol_UnknownGeometricEffect_ProducesWarningAndIsIgnored()
    {
        var symbol = ConvertSimpleSymbol(
            Cim.LineSymbol(Cim.SolidStroke(Cim.Rgb(0, 0, 0), effects: [new CimUnknownGeometricEffect { TypeName = "CIMGeometricEffectOffset" }])),
            out var warnings);

        Assert.IsType<SimpleLineSymbol>(symbol);
        Assert.Contains(warnings, w => w.Contains("CIMGeometricEffectOffset"));
    }

    // -----------------------------------------------------------------------
    // Polygon symbols
    // -----------------------------------------------------------------------

    [Fact]
    public void PolygonSymbol_SolidFillAndStroke_ProducesSimpleFillSymbol()
    {
        var symbol = ConvertSimpleSymbol(
            Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(0, 200, 0)), Cim.SolidStroke(Cim.Rgb(0, 0, 0), width: 1)),
            out _);

        var fill = Assert.IsType<SimpleFillSymbol>(symbol);
        Assert.Equal(200, fill.FillColor.G);
        Assert.NotNull(fill.OutlineSymbol);
    }

    [Fact]
    public void PolygonSymbol_StrokeOnly_ProducesTransparentFillWithOutline()
    {
        var symbol = ConvertSimpleSymbol(
            Cim.PolygonSymbol(Cim.SolidStroke(Cim.Rgb(0, 0, 0), width: 1)),
            out _);

        var fill = Assert.IsType<SimpleFillSymbol>(symbol);
        Assert.Equal(0, fill.FillColor.A);
        Assert.NotNull(fill.OutlineSymbol);
    }

    [Fact]
    public void PolygonSymbol_HatchFill_ProducesHatchSymbolWithColorFromInnerLineSymbol()
    {
        var symbol = ConvertSimpleSymbol(
            Cim.PolygonSymbol(Cim.HatchFill(rotation: 45, lineSymbol: Cim.LineSymbol(Cim.SolidStroke(Cim.Rgb(255, 0, 0))))),
            out _);

        var hatch = Assert.IsType<HatchSymbol>(symbol);
        Assert.Equal(HatchStyle.ForwardDiagonal, hatch.HatchStyle);
        Assert.Equal(255, hatch.ForeColor.R);
    }

    [Theory]
    [InlineData(0, HatchStyle.Horizontal)]
    [InlineData(90, HatchStyle.Vertical)]
    [InlineData(45, HatchStyle.ForwardDiagonal)]
    [InlineData(135, HatchStyle.BackwardDiagonal)]
    public void PolygonSymbol_HatchFillRotation_MapsToClosestHatchStyle(double rotation, HatchStyle expected)
    {
        var symbol = ConvertSimpleSymbol(Cim.PolygonSymbol(Cim.HatchFill(rotation)), out _);

        var hatch = Assert.IsType<HatchSymbol>(symbol);
        Assert.Equal(expected, hatch.HatchStyle);
    }

    [Fact]
    public void PolygonSymbol_PictureFill_FallsBackToSimpleFillSymbolWithWarning()
    {
        var symbol = ConvertSimpleSymbol(Cim.PolygonSymbol(Cim.PictureFill("hatch.png")), out var warnings);

        Assert.IsType<SimpleFillSymbol>(symbol);
        Assert.Contains(warnings, w => w.Contains("CIMPictureFill"));
    }

    [Fact]
    public void PolygonSymbol_MultipleFillLayers_ProducesSymbolCollectionWithOutlineOnFirstOnly()
    {
        var symbol = ConvertSimpleSymbol(
            Cim.PolygonSymbol(
                Cim.SolidFill(Cim.Rgb(255, 0, 0)),
                Cim.SolidFill(Cim.Rgb(0, 255, 0)),
                Cim.SolidStroke(Cim.Rgb(0, 0, 0))),
            out _);

        var collection = Assert.IsType<SymbolCollection>(symbol);
        Assert.Equal(2, collection.Symbols.Count);

        // Reversed: index 0 is the second CIM fill layer (drawn last / on top), which - being
        // the *last* fill layer in CIM order, not the first - must NOT carry the outline.
        var top = Assert.IsType<SimpleFillSymbol>(collection.Symbols[0].Symbol);
        Assert.Null(top.OutlineSymbol);

        var bottom = Assert.IsType<SimpleFillSymbol>(collection.Symbols[1].Symbol);
        Assert.NotNull(bottom.OutlineSymbol);
    }

    // -----------------------------------------------------------------------
    // Color conversion (RGB / CMYK / Gray / HSV, ESRI 0-100 alpha)
    // -----------------------------------------------------------------------

    [Fact]
    public void Color_Rgb_MapsDirectly()
    {
        var symbol = ConvertSimpleSymbol(Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(10, 20, 30))), out _);

        var fill = Assert.IsType<SimpleFillSymbol>(symbol);
        Assert.Equal(10, fill.FillColor.R);
        Assert.Equal(20, fill.FillColor.G);
        Assert.Equal(30, fill.FillColor.B);
    }

    [Theory]
    [InlineData(100, 255)]
    [InlineData(50, 127)] // 50/100 * 255 = 127.5 -> truncated to 127 (byte cast)
    public void Color_EsriAlpha_ConvertsFromZeroToHundredScale(double esriAlpha, byte expectedByte)
    {
        var symbol = ConvertSimpleSymbol(Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(255, 0, 0, esriAlpha))), out _);

        var fill = Assert.IsType<SimpleFillSymbol>(symbol);
        Assert.Equal(expectedByte, fill.FillColor.A);
    }

    [Fact]
    public void PolygonSymbol_FullyTransparentFill_ProducesNoFeatureRendererAtAll()
    {
        // esriAlpha=0 used to be covered by the [Theory] above (asserting FillColor.A == 0), but
        // a symbol whose every color is fully transparent is exactly RendererIsFullyTransparent's
        // "invisible anchor" case - the layer now gets no FeatureRenderer at all (same as gView's
        // own "Render features for this layer" checkbox being off) instead of a renderer that
        // always draws an alpha-0 fill.
        var converter = new AprxMapConverter();
        var cimLayer = Cim.FeatureLayer(
            featureTable: Cim.FeatureTable(),
            renderer: Cim.SimpleRenderer(Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(255, 0, 0, 0)))));
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);

        var layer = (FeatureLayer)map.MapElements[0];
        Assert.Null(layer.FeatureRenderer);
    }

    [Fact]
    public void Color_Gray_ReplicatesLevelAcrossAllChannels()
    {
        var symbol = ConvertSimpleSymbol(Cim.PolygonSymbol(Cim.SolidFill(Cim.Gray(128))), out _);

        var fill = Assert.IsType<SimpleFillSymbol>(symbol);
        Assert.Equal(128, fill.FillColor.R);
        Assert.Equal(128, fill.FillColor.G);
        Assert.Equal(128, fill.FillColor.B);
    }

    [Fact]
    public void Color_Cmyk_FullBlack_ProducesBlack()
    {
        var symbol = ConvertSimpleSymbol(Cim.PolygonSymbol(Cim.SolidFill(Cim.Cmyk(0, 0, 0, 100))), out _);

        var fill = Assert.IsType<SimpleFillSymbol>(symbol);
        Assert.Equal(0, fill.FillColor.R);
        Assert.Equal(0, fill.FillColor.G);
        Assert.Equal(0, fill.FillColor.B);
    }

    [Fact]
    public void Color_Cmyk_NoInk_ProducesWhite()
    {
        var symbol = ConvertSimpleSymbol(Cim.PolygonSymbol(Cim.SolidFill(Cim.Cmyk(0, 0, 0, 0))), out _);

        var fill = Assert.IsType<SimpleFillSymbol>(symbol);
        Assert.Equal(255, fill.FillColor.R);
        Assert.Equal(255, fill.FillColor.G);
        Assert.Equal(255, fill.FillColor.B);
    }

    [Theory]
    [InlineData(0, 255, 0, 0)]      // red
    [InlineData(120, 0, 255, 0)]    // green
    [InlineData(240, 0, 0, 255)]    // blue
    public void Color_Hsv_FullSaturationAndValue_ProducesExpectedPrimary(double hue, byte r, byte g, byte b)
    {
        var symbol = ConvertSimpleSymbol(Cim.PolygonSymbol(Cim.SolidFill(Cim.Hsv(hue, 100, 100))), out _);

        var fill = Assert.IsType<SimpleFillSymbol>(symbol);
        Assert.Equal(r, fill.FillColor.R);
        Assert.Equal(g, fill.FillColor.G);
        Assert.Equal(b, fill.FillColor.B);
    }

    // -----------------------------------------------------------------------
    // Layer transparency (ArcGIS Pro's Layer Properties -> Display -> Transparency slider,
    // separate from and multiplicative with each symbol's own color alpha)
    // -----------------------------------------------------------------------

    [Fact]
    public void LayerTransparency_ScalesDownAnOtherwiseOpaqueColor()
    {
        // 50% layer transparency on a fully opaque (alpha=100) fill -> alpha ~50% of 255.
        var symbol = ConvertSimpleSymbol(
            Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(255, 0, 0, alpha: 100))),
            out _,
            transparency: 50);

        var fill = Assert.IsType<SimpleFillSymbol>(symbol);
        Assert.Equal(128, fill.FillColor.A); // 255 * 0.5, rounded
    }

    [Fact]
    public void LayerTransparency_MultipliesWithTheSymbolsOwnAlpha_RatherThanReplacingIt()
    {
        // Symbol itself is already 50% transparent (alpha=50 on Esri's 0-100 scale -> 127/255);
        // 50% layer transparency must multiply on top of that, not override it.
        var symbol = ConvertSimpleSymbol(
            Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(255, 0, 0, alpha: 50))),
            out _,
            transparency: 50);

        var fill = Assert.IsType<SimpleFillSymbol>(symbol);
        Assert.Equal(64, fill.FillColor.A); // 127 (esri 50%) * 0.5, rounded
    }

    [Fact]
    public void LayerTransparency_Zero_LeavesColorAlphaUnchanged()
    {
        var symbol = ConvertSimpleSymbol(
            Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(255, 0, 0, alpha: 100))),
            out _,
            transparency: 0);

        var fill = Assert.IsType<SimpleFillSymbol>(symbol);
        Assert.Equal(255, fill.FillColor.A);
    }

    [Fact]
    public void LayerTransparency_FullyTransparent_ProducesFullyTransparentColor()
    {
        var symbol = ConvertSimpleSymbol(
            Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(255, 0, 0, alpha: 100))),
            out _,
            transparency: 100);

        var fill = Assert.IsType<SimpleFillSymbol>(symbol);
        Assert.Equal(0, fill.FillColor.A);
    }

    [Fact]
    public void LayerTransparency_DoesNotAffectRgbComponents()
    {
        var symbol = ConvertSimpleSymbol(
            Cim.PolygonSymbol(Cim.SolidFill(Cim.Rgb(10, 20, 30))),
            out _,
            transparency: 50);

        var fill = Assert.IsType<SimpleFillSymbol>(symbol);
        Assert.Equal(10, fill.FillColor.R);
        Assert.Equal(20, fill.FillColor.G);
        Assert.Equal(30, fill.FillColor.B);
    }
}
