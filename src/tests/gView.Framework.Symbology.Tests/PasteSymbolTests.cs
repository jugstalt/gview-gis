using gView.Framework.Core.Geometry;
using gView.Framework.Symbology.Extensions;
using gView.GraphicsEngine;

namespace gView.Framework.Symbology.Tests;

public class PasteSymbolTests
{
    private static readonly ArgbColor Red = ArgbColor.FromArgb(255, 255, 0, 0);
    private static readonly ArgbColor Green = ArgbColor.FromArgb(255, 0, 200, 0);
    private static readonly ArgbColor Blue = ArgbColor.FromArgb(255, 0, 0, 255);

    private static SimpleFillSymbol Polygon(ArgbColor fill, int fillAlpha, ArgbColor outline, float width, string label = "")
        => new SimpleFillSymbol()
        {
            FillColor = ArgbColor.FromArgb(fillAlpha, fill),
            OutlineSymbol = new SimpleLineSymbol() { Color = outline, Width = width },
            LegendLabel = label
        };

    [Fact]
    public void CreatePasteSymbol_ReturnsCloneWithTargetLabel()
    {
        var template = new SimpleLineSymbol() { Color = Red, Width = 3, LegendLabel = "template" };
        var target = new SimpleLineSymbol() { Color = Green, LegendLabel = "class A" };

        var pasted = template.CreatePasteSymbol(target, keepColors: false, out bool colorsKept);

        Assert.NotSame(template, pasted);
        Assert.False(colorsKept);
        Assert.Equal("class A", pasted.LegendLabel);
        Assert.Equal("template", template.LegendLabel);
        Assert.Equal(Red, ((SimpleLineSymbol)pasted).PenColor);
        Assert.Equal(3f, ((SimpleLineSymbol)pasted).Width);
    }

    [Fact]
    public void CreatePasteSymbol_LineKeepColors_TakesTargetColorAndTemplateStyle()
    {
        var template = new SimpleLineSymbol() { Color = ArgbColor.FromArgb(128, Red), Width = 3, DashStyle = LineDashStyle.Dash };
        var target = new SimpleLineSymbol() { Color = Green, Width = 1 };

        var pasted = (SimpleLineSymbol)template.CreatePasteSymbol(target, keepColors: true, out bool colorsKept);

        Assert.True(colorsKept);
        Assert.Equal(ArgbColor.FromArgb(128, Green), pasted.PenColor);
        Assert.Equal(3f, pasted.Width);
        Assert.Equal(LineDashStyle.Dash, pasted.PenDashStyle);
    }

    [Fact]
    public void CreatePasteSymbol_PolygonKeepColors_KeepsFillAndOutlineSeparately()
    {
        var template = Polygon(Red, 50, Red, 4);
        var target = Polygon(Green, 200, Blue, 1);

        var pasted = (SimpleFillSymbol)template.CreatePasteSymbol(target, keepColors: true, out bool colorsKept);

        Assert.True(colorsKept);
        Assert.Equal(ArgbColor.FromArgb(50, Green), pasted.FillColor);
        Assert.Equal(Blue, pasted.PenColor);
        Assert.Equal(4f, pasted.PenWidth);
    }

    [Fact]
    public void CreatePasteSymbol_TargetCollection_KeepsTemplateColors()
    {
        var template = new SimpleLineSymbol() { Color = Red };
        var target = new SymbolCollection([new SimpleLineSymbol() { Color = Green }]);

        var pasted = template.CreatePasteSymbol(target, keepColors: true, out bool colorsKept);

        Assert.False(colorsKept);
        Assert.Equal(Red, ((SimpleLineSymbol)pasted).PenColor);
    }

    [Fact]
    public void CreatePasteSymbol_TemplateCollection_IsClonedWithoutKeepingColors()
    {
        var template = new SymbolCollection([new SimpleLineSymbol() { Color = Red }, new SimpleLineSymbol() { Color = Blue }]);
        var target = new SimpleLineSymbol() { Color = Green, LegendLabel = "class A" };

        var pasted = template.CreatePasteSymbol(target, keepColors: true, out bool colorsKept);

        Assert.False(colorsKept);
        Assert.IsType<SymbolCollection>(pasted);
        Assert.NotSame(template, pasted);
        Assert.Equal("class A", pasted.LegendLabel);
    }

    [Theory]
    [InlineData(GeometryType.Polygon, true)]
    [InlineData(GeometryType.Envelope, true)]
    [InlineData(GeometryType.Polyline, false)]
    [InlineData(GeometryType.Point, false)]
    public void FitsGeometryType_FillSymbol(GeometryType layerGeometryType, bool expected)
    {
        Assert.Equal(expected, new SimpleFillSymbol().FitsGeometryType(layerGeometryType));
    }

    [Fact]
    public void FitsGeometryType_PointSymbolOnMultipoint_True()
    {
        Assert.True(new SimplePointSymbol().FitsGeometryType(GeometryType.Multipoint));
    }
}
