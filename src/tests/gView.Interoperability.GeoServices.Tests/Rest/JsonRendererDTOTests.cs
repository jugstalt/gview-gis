using gView.Framework.Cartography.Rendering;
using gView.Framework.Cartography.Rendering.Abstractions;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Symbology;
using gView.Interoperability.GeoServices.Rest.DTOs.DynamicLayers;
using gView.Interoperability.GeoServices.Rest.DTOs.Renderers.SimpleRenderers;
using System.Text.Json;

namespace gView.Interoperability.GeoServices.Tests.Rest;

public class JsonRendererDTOTests
{
    private static string FillJson(int r)
        => $$"""{ "type": "esriSFS", "style": "esriSFSSolid", "color": [{{r}},0,0,255], "outline": { "type": "esriSLS", "style": "esriSLSSolid", "color": [0,0,0,255], "width": 1 } }""";

    private static IFeatureRenderer? Parse(string json)
        => JsonRendererDTO.FromJsonRenderer(JsonSerializer.Deserialize<JsonRendererDTO>(json)!);

    private static ISymbol? FromSymbol(string json)
        => JsonRendererDTO.FromSymbolJObject(JsonDocument.Parse(json).RootElement);

    [Fact]
    public void Simple_Fill_CreatesSimpleRenderer()
    {
        var renderer = Assert.IsType<SimpleRenderer>(Parse($$"""{ "type": "simple", "symbol": {{FillJson(10)}} }"""));

        Assert.IsType<gView.Framework.Symbology.SimpleFillSymbol>(renderer.Symbol);
    }

    [Fact]
    public void UnsupportedType_ReturnsNull()
    {
        Assert.Null(Parse("""{ "type": "heatmap" }"""));
        Assert.Null(Parse("""{ "type": "dotDensity" }"""));
    }

    [Fact]
    public void UniqueValue_SingleField_CreatesValueMapRenderer()
    {
        var renderer = Assert.IsType<ValueMapRenderer>(Parse($$"""
            {
              "type": "uniqueValue", "field1": "TYP", "fieldDelimiter": ", ",
              "defaultSymbol": {{FillJson(1)}},
              "uniqueValueInfos": [
                { "value": "A", "label": "Type A", "symbol": {{FillJson(2)}} },
                { "value": "B", "label": "Type B", "symbol": {{FillJson(3)}} }
              ]
            }
            """));

        Assert.Equal("TYP", renderer.ValueField);
        Assert.Equal("Type A", (renderer["A"] as ILegendItem)?.LegendLabel);
        Assert.Equal("Type B", (renderer["B"] as ILegendItem)?.LegendLabel);
        Assert.NotNull(renderer.DefaultSymbol);
    }

    [Fact]
    public void UniqueValue_MultipleFields_CreatesManyValueMapRenderer()
    {
        var renderer = Assert.IsType<ManyValueMapRenderer>(Parse($$"""
            {
              "type": "uniqueValue", "field1": "A", "field2": "B", "fieldDelimiter": ", ",
              "uniqueValueInfos": [ { "value": "x, y", "label": "xy", "symbol": {{FillJson(2)}} } ]
            }
            """));

        Assert.Equal("A", renderer.ValueField1);
        Assert.Equal("B", renderer.ValueField2);
        Assert.NotNull(renderer["x|y"]);
    }

    [Fact]
    public void UniqueValue_WithoutField_ReturnsNull()
        => Assert.Null(Parse("""{ "type": "uniqueValue", "uniqueValueInfos": [] }"""));

    [Fact]
    public void ClassBreaks_CreatesTouchingClasses()
    {
        var renderer = Assert.IsType<QuantityRenderer>(Parse($$"""
            {
              "type": "classBreaks", "field": "POP", "minValue": 0,
              "classBreakInfos": [
                { "classMaxValue": 200, "label": "100-200", "symbol": {{FillJson(3)}} },
                { "classMaxValue": 100, "label": "0-100", "symbol": {{FillJson(2)}} }
              ]
            }
            """));

        Assert.Equal("POP", renderer.ValueField);

        var classes = renderer.QuantityClasses.OrderBy(c => c.Min).ToArray();
        Assert.Equal(2, classes.Length);
        Assert.Equal((0d, 100d), (classes[0].Min, classes[0].Max));
        Assert.Equal((100d, 200d), (classes[1].Min, classes[1].Max));
        Assert.Equal("0-100", (classes[0].Symbol as ILegendItem)?.LegendLabel);
    }

    [Fact]
    public void ClassBreaks_Output_WritesClassBreakInfos()
    {
        var input = Parse($$"""
            {
              "type": "classBreaks", "field": "POP", "minValue": 0,
              "classBreakInfos": [
                { "classMaxValue": 100, "label": "a", "symbol": {{FillJson(2)}} },
                { "classMaxValue": 200, "label": "b", "symbol": {{FillJson(3)}} }
              ]
            }
            """);

        var dto = JsonRendererDTO.FromFeatureRenderer(input!);

        Assert.NotNull(dto);
        Assert.Equal("classBreaks", dto.Type);
        Assert.Equal("POP", dto.Field);
        Assert.Equal(0d, dto.MinValue);
        Assert.Equal(2, dto.ClassBreakInfos.Length);
    }

    [Fact]
    public void UniqueValue_Output_SkipsAllOtherValuesKey()
    {
        var input = Parse($$"""
            {
              "type": "uniqueValue", "field1": "TYP", "defaultSymbol": {{FillJson(1)}},
              "uniqueValueInfos": [ { "value": "A", "label": "a", "symbol": {{FillJson(2)}} } ]
            }
            """);

        var dto = JsonRendererDTO.FromFeatureRenderer(input!);

        Assert.NotNull(dto);
        var info = Assert.IsType<JsonRendererDTO.UniqueValueInfo>(Assert.Single(dto.UniqueValueInfos));
        Assert.Equal("A", info.Value);
        Assert.Equal("a", info.Label);
        Assert.NotNull(dto.DefaultSymbol);
    }

    [Theory]
    [InlineData("esriSFSBackwardDiagonal")]
    [InlineData("esriSFSForwardDiagonal")]
    [InlineData("esriSFSCross")]
    [InlineData("esriSFSDiagonalCross")]
    [InlineData("esriSFSHorizontal")]
    [InlineData("esriSFSVertical")]
    public void FillStyle_Hatch_CreatesHatchSymbol(string style)
    {
        var symbol = FromSymbol($$"""{ "type": "esriSFS", "style": "{{style}}", "color": [10,20,30,255] }""");

        var hatch = Assert.IsType<gView.Framework.Symbology.HatchSymbol>(symbol);
        Assert.Equal(style, ((SimpleFillSymbolDTO)JsonRendererDTO.FromSymbol(hatch)!).Style);
    }

    [Fact]
    public void FillStyle_Null_HasTransparentFill()
    {
        var symbol = FromSymbol("""{ "type": "esriSFS", "style": "esriSFSNull", "color": [10,20,30,255], "outline": { "type": "esriSLS", "style": "esriSLSSolid", "color": [0,0,0,255], "width": 2 } }""");

        var fill = Assert.IsType<gView.Framework.Symbology.SimpleFillSymbol>(symbol);
        Assert.Equal(0, fill.FillColor.A);
        Assert.NotNull(fill.OutlineSymbol);
    }

    [Fact]
    public void LineStyle_Null_IsTransparent()
    {
        var symbol = FromSymbol("""{ "type": "esriSLS", "style": "esriSLSNull", "color": [10,20,30,255], "width": 2 }""");

        var line = Assert.IsType<gView.Framework.Symbology.SimpleLineSymbol>(symbol);
        Assert.Equal(0, line.Color.A);
    }

    [Fact]
    public void PictureMarker_ImageData_CreatesRasterMarker()
    {
        // 1x1 png
        const string png = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==";

        var symbol = FromSymbol($$"""{ "type": "esriPMS", "imageData": "{{png}}", "contentType": "image/png", "width": 12, "height": 14, "angle": 0, "xoffset": 1, "yoffset": 2 }""");

        var marker = Assert.IsType<gView.Framework.Symbology.RasterMarkerSymbol>(symbol);
        Assert.True(File.Exists(marker.Filename));
        Assert.Equal(12f, marker.SizeX);
        Assert.Equal(14f, marker.SizeY);
    }

    [Fact]
    public void PictureMarker_UrlOnly_IsNotSupported()
        => Assert.Null(FromSymbol("""{ "type": "esriPMS", "url": "http://localhost/x.png", "width": 12, "height": 14 }"""));

    [Fact]
    public void PictureMarker_NoImageContent_IsNotSupported()
        => Assert.Null(FromSymbol("""{ "type": "esriPMS", "imageData": "AAAA", "width": 12, "height": 14 }"""));

    [Theory]
    [InlineData("esriServerPointLabelPlacementAboveRight", TextSymbolAlignment.leftAlignOver)]
    [InlineData("esriServerPointLabelPlacementAboveLeft", TextSymbolAlignment.rightAlignOver)]
    [InlineData("esriServerPointLabelPlacementBelowCenter", TextSymbolAlignment.Under)]
    [InlineData("esriServerPointLabelPlacementCenterRight", TextSymbolAlignment.leftAlignCenter)]
    [InlineData("esriServerPointLabelPlacementCenterCenter", TextSymbolAlignment.Center)]
    [InlineData("esriServerLinePlacementAboveAlong", TextSymbolAlignment.Over)]
    [InlineData("esriServerLinePlacementBelowStart", TextSymbolAlignment.Under)]
    public void LabelPlacement_MapsToAlignment(string placement, TextSymbolAlignment expected)
        => Assert.Equal(expected, LabelingInfoDTO.TextAlignmentFromPlacement(placement));

    [Fact]
    public void LabelPlacement_Unknown_IsNull()
    {
        Assert.Null(LabelingInfoDTO.TextAlignmentFromPlacement(null!));
        Assert.Null(LabelingInfoDTO.TextAlignmentFromPlacement("esriServerPolygonPlacementAlwaysHorizontal"));
    }

    [Fact]
    public void Labeling_WithoutScales_CreatesSimpleLabelRenderer()
    {
        var labeling = JsonSerializer.Deserialize<LabelingInfoDTO>("""
            { "labelExpression": "[NAME]", "minScale": 0, "maxScale": 0,
              "symbol": { "type": "esriTS", "color": [0,0,0,255], "font": { "family": "Arial", "size": 10 } } }
            """)!;

        var renderer = Assert.IsType<SimpleLabelRenderer>(labeling.ToLabelRenderer());
        Assert.True(renderer.UseExpression);
        Assert.Equal("[NAME]", renderer.LabelExpression);
    }

    [Fact]
    public void Labeling_WithScales_CreatesScaleDependentRenderer_WithSwappedLimits()
    {
        var labeling = JsonSerializer.Deserialize<LabelingInfoDTO>("""
            { "labelExpression": "[NAME]", "minScale": 50000, "maxScale": 1000,
              "symbol": { "type": "esriTS", "color": [0,0,0,255], "font": { "family": "Arial", "size": 10 } } }
            """)!;

        var renderer = Assert.IsType<ScaleDependentLabelRenderer>(labeling.ToLabelRenderer());
        var item = Assert.IsAssignableFrom<IScaledependent>(Assert.Single(renderer.RendererItems));
        Assert.Equal(1000, item.MinimumScale);
        Assert.Equal(50000, item.MaximumScale);
    }
}
