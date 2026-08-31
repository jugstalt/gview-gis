using gView.Framework.Cartography;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.GraphicsEngine;
using Path = gView.Framework.Geometry.Path;

namespace gView.Framework.Cartography.Rendering.Tests;

/// <summary>
/// Tests for <see cref="AdvancedLabelRenderer"/>'s <see cref="AdvancedLabelRenderer.ColorExpression"/>
/// and, complementarily, that <see cref="SimpleLabelRenderer"/>'s new
/// <see cref="SimpleLabelRenderer.ResolveDynamicColor"/> extension point is a true no-op for
/// plain <see cref="SimpleLabelRenderer"/> instances that never use it.
/// </summary>
public class AdvancedLabelRendererTests
{
    private static Polyline Line(double x1, double y1, double x2, double y2) =>
        new(new Path(new List<IPoint> { new Point(x1, y1), new Point(x2, y2) }));

    private static Display NewDisplay() => new(null);

    // -----------------------------------------------------------------------
    // Regression: a plain SimpleLabelRenderer is unaffected by the new hook
    // -----------------------------------------------------------------------

    [Fact]
    public void SimpleLabelRenderer_ColorNeverChangesAcrossFeatures()
    {
        var symbol = new SimpleTextSymbol { Color = ArgbColor.FromArgb(10, 20, 30) };
        var renderer = new SimpleLabelRenderer { UseExpression = true, LabelExpression = "[NAME]", TextSymbol = symbol };
        var display = NewDisplay();

        foreach (var name in new[] { "A", "B", "C" })
        {
            var feature = new Feature { Shape = Line(0, 0, 1, 1) };
            feature.Fields.Add(new FieldValue("NAME", name));
            renderer.Draw(display, null!, feature);
        }

        var color = ((IFontColor)renderer.TextSymbol).FontColor;
        Assert.Equal(10, color.R);
        Assert.Equal(20, color.G);
        Assert.Equal(30, color.B);
    }

    // -----------------------------------------------------------------------
    // AdvancedLabelRenderer.ColorExpression - per-feature override + fallback + no re-creation
    // -----------------------------------------------------------------------

    private static (string Text, ArgbColor Color) DrawAndGetTextAndColor(
        AdvancedLabelRenderer renderer, IGeometry shape, params (string Name, object Value)[] fields)
    {
        var feature = new Feature { Shape = shape };
        foreach (var (name, value) in fields)
        {
            feature.Fields.Add(new FieldValue(name, value));
        }

        renderer.Draw(NewDisplay(), null!, feature);

        return (renderer.TextSymbol.Text, ((IFontColor)renderer.TextSymbol).FontColor);
    }

    [Fact]
    public void ColorExpression_PlainFieldReference_ResolvesPerFeature()
    {
        var renderer = new AdvancedLabelRenderer
        {
            UseExpression = true,
            LabelExpression = "[NAME]",
            ColorExpression = "[COLOR]",
            TextSymbol = new SimpleTextSymbol(),
        };

        var (_, color) = DrawAndGetTextAndColor(renderer, Line(0, 0, 1, 1), ("NAME", "A"), ("COLOR", "rgb(255,0,0)"));

        Assert.Equal(255, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(0, color.B);
    }

    [Fact]
    public void ColorExpression_ConditionalScript_ResolvesPerFeature()
    {
        const string colorScript = "@@start\n@@if([TYP],A)\nrgb(255,0,0)\n@@endif\n@@if([TYP],B)\nrgb(0,255,0)\n@@endif\n@@end";
        var renderer = new AdvancedLabelRenderer
        {
            UseExpression = true,
            LabelExpression = "[TYP]",
            ColorExpression = colorScript,
            TextSymbol = new SimpleTextSymbol(),
        };

        var (_, colorA) = DrawAndGetTextAndColor(renderer, Line(0, 0, 1, 1), ("TYP", "A"));
        Assert.Equal((255, 0, 0), (colorA.R, colorA.G, colorA.B));

        var (_, colorB) = DrawAndGetTextAndColor(renderer, Line(0, 0, 1, 1), ("TYP", "B"));
        Assert.Equal((0, 255, 0), (colorB.R, colorB.G, colorB.B));
    }

    [Fact]
    public void ColorExpression_NoBranchMatches_FallsBackToSymbolsOwnBaseColor()
    {
        const string colorScript = "@@start\n@@if([TYP],A)\nrgb(255,0,0)\n@@endif\n@@end";
        var baseColor = ArgbColor.FromArgb(1, 2, 3);
        var renderer = new AdvancedLabelRenderer
        {
            UseExpression = true,
            LabelExpression = "[TYP]",
            ColorExpression = colorScript,
            TextSymbol = new SimpleTextSymbol { Color = baseColor },
        };

        // TYP="A" matches -> override color
        var (_, colorA) = DrawAndGetTextAndColor(renderer, Line(0, 0, 1, 1), ("TYP", "A"));
        Assert.Equal((255, 0, 0), (colorA.R, colorA.G, colorA.B));

        // TYP="X" matches nothing -> must fall back to the symbol's own base color, not "leak"
        // the previous feature's override.
        var (_, colorX) = DrawAndGetTextAndColor(renderer, Line(0, 0, 1, 1), ("TYP", "X"));
        Assert.Equal((baseColor.R, baseColor.G, baseColor.B), (colorX.R, colorX.G, colorX.B));
    }

    [Fact]
    public void ColorExpression_Empty_NeverTouchesSymbolColor()
    {
        var baseColor = ArgbColor.FromArgb(9, 9, 9);
        var renderer = new AdvancedLabelRenderer
        {
            UseExpression = true,
            LabelExpression = "[NAME]",
            ColorExpression = "",
            TextSymbol = new SimpleTextSymbol { Color = baseColor },
        };

        var (_, color) = DrawAndGetTextAndColor(renderer, Line(0, 0, 1, 1), ("NAME", "A"));

        Assert.Equal((baseColor.R, baseColor.G, baseColor.B), (color.R, color.G, color.B));
    }

    [Fact]
    public void TextSymbolReassignment_UpdatesBaseColorUsedForFallback()
    {
        const string colorScript = "@@start\n@@if([TYP],A)\nrgb(255,0,0)\n@@endif\n@@end";
        var renderer = new AdvancedLabelRenderer
        {
            UseExpression = true,
            LabelExpression = "[TYP]",
            ColorExpression = colorScript,
            TextSymbol = new SimpleTextSymbol { Color = ArgbColor.FromArgb(1, 1, 1) },
        };

        // Reassign to a symbol with a different base color.
        var newBase = ArgbColor.FromArgb(50, 60, 70);
        renderer.TextSymbol = new SimpleTextSymbol { Color = newBase };

        var (_, color) = DrawAndGetTextAndColor(renderer, Line(0, 0, 1, 1), ("TYP", "nomatch"));

        Assert.Equal((newBase.R, newBase.G, newBase.B), (color.R, color.G, color.B));
    }

    // -----------------------------------------------------------------------
    // Critical (per explicit review feedback): the symbol/font instance must never be recreated
    // per feature - only its brush color mutated. A new IFont under Skia takes a constructor lock,
    // so recreating it per feature would be a serious performance regression under gView.Server.
    // -----------------------------------------------------------------------

    [Fact]
    public void SymbolInstance_StaysReferenceEqualAcrossFeaturesWithDifferentColors()
    {
        const string colorScript = "@@start\n@@if([TYP],A)\nrgb(255,0,0)\n@@endif\n@@if([TYP],B)\nrgb(0,255,0)\n@@endif\n@@end";
        var renderer = new AdvancedLabelRenderer
        {
            UseExpression = true,
            LabelExpression = "[TYP]",
            ColorExpression = colorScript,
            TextSymbol = new SimpleTextSymbol(),
        };

        var symbolBefore = renderer.TextSymbol;

        DrawAndGetTextAndColor(renderer, Line(0, 0, 1, 1), ("TYP", "A"));
        var symbolAfterFirst = renderer.TextSymbol;

        DrawAndGetTextAndColor(renderer, Line(0, 0, 1, 1), ("TYP", "B"));
        var symbolAfterSecond = renderer.TextSymbol;

        Assert.Same(symbolBefore, symbolAfterFirst);
        Assert.Same(symbolBefore, symbolAfterSecond);
    }

    // -----------------------------------------------------------------------
    // Save/Load/Clone
    // -----------------------------------------------------------------------

    [Fact]
    public void SaveLoad_RoundTripsColorExpression()
    {
        var renderer = new AdvancedLabelRenderer
        {
            FieldName = "NAME",
            TextSymbol = new SimpleTextSymbol(),
            ColorExpression = "[COLOR]",
        };
        var stream = new FakePersistStream();

        renderer.Save(stream);

        var loaded = new AdvancedLabelRenderer();
        loaded.Load(stream);

        Assert.Equal("[COLOR]", loaded.ColorExpression);
    }

    [Fact]
    public void Load_StreamWithoutColorExpressionKey_DefaultsToEmpty()
    {
        // Simulates loading a renderer saved before this feature existed.
        var renderer = new SimpleLabelRenderer { FieldName = "NAME", TextSymbol = new SimpleTextSymbol() };
        var stream = new FakePersistStream();
        renderer.Save(stream); // no "ColorExpression" key at all

        var loaded = new AdvancedLabelRenderer();
        loaded.Load(stream);

        Assert.Equal("", loaded.ColorExpression);
    }

    [Fact]
    public void Clone_CopiesColorExpression()
    {
        var renderer = new AdvancedLabelRenderer
        {
            FieldName = "NAME",
            TextSymbol = new SimpleTextSymbol(),
            ColorExpression = "[COLOR]",
        };

        var clone = (AdvancedLabelRenderer)renderer.Clone(new gView.Framework.Core.Common.CloneOptions(null, false));

        Assert.Equal("[COLOR]", clone.ColorExpression);
        Assert.NotSame(renderer, clone);
    }
}
