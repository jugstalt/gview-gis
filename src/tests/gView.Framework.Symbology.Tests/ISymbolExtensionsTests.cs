using gView.Framework.Symbology.Extensions;
using gView.Framework.Symbology.Models;
using gView.GraphicsEngine;

namespace gView.Framework.Symbology.Tests;

public class ISymbolExtensionsTests
{
    private static readonly ArgbColor Red = ArgbColor.FromArgb(255, 255, 0, 0);
    private static readonly ArgbColor Green = ArgbColor.FromArgb(255, 0, 200, 0);

    private static SimpleFillSymbol CreatePolygonSymbol()
        => new SimpleFillSymbol()
        {
            FillColor = ArgbColor.FromArgb(85, Red),
            OutlineSymbol = new SimpleLineSymbol() { Color = Red, Width = 1 }
        };

    [Fact]
    public void CanBulkEdit_SimpleSymbols_True()
    {
        Assert.True(CreatePolygonSymbol().CanBulkEdit());
        Assert.True(new SimpleLineSymbol().CanBulkEdit());
        Assert.True(new SimplePointSymbol().CanBulkEdit());
    }

    [Fact]
    public void CanBulkEdit_SymbolCollection_FalseAndNothingApplied()
    {
        var collection = new SymbolCollection([new SimpleLineSymbol(), new SimpleLineSymbol()]);

        Assert.False(collection.CanBulkEdit());
        Assert.False(collection.ApplyColor(Green));
        Assert.False(collection.ApplyProperties(new SymbolPropertyValues() { Width = 3 }));
    }

    [Fact]
    public void ApplyColor_PolygonBoth_KeepsAlphaOfFillAndOutline()
    {
        var symbol = CreatePolygonSymbol();

        Assert.True(symbol.ApplyColor(Green, SymbolColorTarget.Both));

        Assert.Equal(ArgbColor.FromArgb(85, Green), symbol.FillColor);
        Assert.Equal(Green, symbol.PenColor);
    }

    [Fact]
    public void ApplyColor_PolygonFillOnly_KeepsOutline()
    {
        var symbol = CreatePolygonSymbol();

        symbol.ApplyColor(Green, SymbolColorTarget.Fill);

        Assert.Equal(ArgbColor.FromArgb(85, Green), symbol.FillColor);
        Assert.Equal(Red, symbol.PenColor);
    }

    [Fact]
    public void ApplyColor_PolygonOutlineOnly_KeepsFill()
    {
        var symbol = CreatePolygonSymbol();

        symbol.ApplyColor(Green, SymbolColorTarget.Outline);

        Assert.Equal(ArgbColor.FromArgb(85, Red), symbol.FillColor);
        Assert.Equal(Green, symbol.PenColor);
    }

    [Fact]
    public void ApplyColor_Line_SetsPenColorAndIgnoresTarget()
    {
        var symbol = new SimpleLineSymbol();

        symbol.ApplyColor(Green, SymbolColorTarget.Fill);

        Assert.Equal(Green, symbol.PenColor);
    }

    [Fact]
    public void ApplyColor_Point_SetsFillAndKeepsOutline()
    {
        var symbol = new SimplePointSymbol();
        var outline = symbol.PenColor;

        symbol.ApplyColor(Green);

        Assert.Equal(Green, symbol.FillColor);
        Assert.Equal(outline, symbol.PenColor);
    }

    [Fact]
    public void ApplyProperties_OnlySetValuesAreApplied()
    {
        var symbol = new SimpleLineSymbol() { Color = Green, Width = 1 };

        symbol.ApplyProperties(new SymbolPropertyValues() { Width = 4, DashStyle = LineDashStyle.Dash });

        Assert.Equal(4f, symbol.Width);
        Assert.Equal(LineDashStyle.Dash, symbol.PenDashStyle);
        Assert.Equal(Green, symbol.PenColor);
    }

    [Fact]
    public void ApplyProperties_PolygonOpacity_ChangesFillAlphaOnly()
    {
        var symbol = CreatePolygonSymbol();

        symbol.ApplyProperties(new SymbolPropertyValues() { Opacity = 100 });

        Assert.Equal(Red, symbol.FillColor);
    }

    [Fact]
    public void ApplyProperties_PolygonWidth_SetsOutlineWidth()
    {
        var symbol = CreatePolygonSymbol();

        symbol.ApplyProperties(new SymbolPropertyValues() { Width = 2.5f });

        Assert.Equal(2.5f, symbol.PenWidth);
    }

    [Fact]
    public void ApplyProperties_PointSize()
    {
        var symbol = new SimplePointSymbol();

        symbol.ApplyProperties(new SymbolPropertyValues() { Size = 12 });

        Assert.Equal(12f, symbol.Size);
    }

    [Fact]
    public void ReadPropertyValues_Polygon_ReturnsOpaqueFillColorAndOpacity()
    {
        var values = CreatePolygonSymbol().ReadPropertyValues();

        Assert.Equal(Red, values.Color);
        Assert.Equal(33f, values.Opacity);
        Assert.Equal(1f, values.Width);
    }

    [Fact]
    public void ReadPropertyValues_SymbolCollection_IsEmpty()
    {
        var values = new SymbolCollection([new SimpleLineSymbol()]).ReadPropertyValues();

        Assert.True(values.IsEmpty);
    }
}
