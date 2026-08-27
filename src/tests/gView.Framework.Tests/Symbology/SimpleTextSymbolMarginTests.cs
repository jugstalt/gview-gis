using System.Linq;
using gView.Framework.Cartography;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.GraphicsEngine;

namespace gView.Framework.Tests.Symbology;

/// <summary>
/// Regression tests for <c>SimpleTextSymbol.Margin</c>: on engines that measure text pixel-exact
/// (Skia; see <c>IGraphicsEngine.MeasuresTextWithPadding</c>), <c>AnnotationPolygon</c>'s
/// collision box used to match only the bare glyphs - even for <see cref="GlowingTextSymbol"/>
/// and <see cref="BlockoutTextSymbol"/>, whose actually-drawn footprint is visibly larger (a
/// halo/glow border, a background box). A neighbouring label's overlap check never knew that
/// border/box was there, so it could sit right on top of it. <c>Margin</c> lets each subclass
/// report how much further it draws so the collision box matches reality.
/// </summary>
public class SimpleTextSymbolMarginTests
{
    private static Display NewDisplay(int size = 200)
    {
        var display = new Display(null);
        display.Dpi = Current.Engine.ScreenDpi;

        var env = new Envelope(0, 0, size, size);
        display.Limit = env;
        display.ImageWidth = size;
        display.ImageHeight = size;
        display.ZoomTo(env.MinX, env.MinY, env.MaxX, env.MaxY);

        display.Canvas = Current.Engine.CreateBitmap(size, size).CreateCanvas();

        return display;
    }

    private static CanvasRectangleF EnvelopeOf(ITextSymbol symbol, IDisplay display, IGeometry geometry)
    {
        var env = symbol.AnnotationPolygon(display, geometry, TextSymbolAlignment.Center)!.Single().Envelope;
        return new CanvasRectangleF(env.MinX, env.MinY, env.MaxX - env.MinX, env.MaxY - env.MinY);
    }

    [Fact]
    public void GlowingTextSymbol_InflatesCollisionBoxByGlowingWidthOnEverySide()
    {
        var display = NewDisplay();
        var point = new Point(100, 100);

        var plain = new SimpleTextSymbol { Text = "Hello", Font = Current.Engine.CreateFont("Arial", 12f) };
        var glow = new GlowingTextSymbol { Text = "Hello", Font = Current.Engine.CreateFont("Arial", 12f), GlowingWidth = 5 };

        var plainRect = EnvelopeOf(plain, display, point);
        var glowRect = EnvelopeOf(glow, display, point);

        // Grown symmetrically by GlowingWidth(5) on every side - matches exactly, since
        // GlowingWidth is a plain pixel count (unlike Blockout's font-derived padding below).
        Assert.Equal(plainRect.Left - 5f, glowRect.Left, precision: 3);
        Assert.Equal(plainRect.Top - 5f, glowRect.Top, precision: 3);
        Assert.Equal(plainRect.Width + 10f, glowRect.Width, precision: 3);
        Assert.Equal(plainRect.Height + 10f, glowRect.Height, precision: 3);
    }

    [Fact]
    public void GlowingTextSymbol_ZeroGlowingWidth_FallsBackToFontSizeTenthLikeDrawAtPointDoes()
    {
        var display = NewDisplay();
        var point = new Point(100, 100);

        var plain = new SimpleTextSymbol { Text = "Hello", Font = Current.Engine.CreateFont("Arial", 20f) };
        // Size/10 = 2, unambiguously above the 1px floor - GlowingWidth left at its 0 ("auto") default.
        var glow = new GlowingTextSymbol { Text = "Hello", Font = Current.Engine.CreateFont("Arial", 20f), GlowingWidth = 0 };

        var plainRect = EnvelopeOf(plain, display, point);
        var glowRect = EnvelopeOf(glow, display, point);

        Assert.Equal(plainRect.Width + 4f, glowRect.Width, precision: 3);  // 2 * (20/10)
    }

    [Fact]
    public void BlockoutTextSymbol_InflatesCollisionBoxToCoverItsBackgroundBoxPadding()
    {
        var display = NewDisplay();
        var point = new Point(100, 100);

        var plain = new SimpleTextSymbol { Text = "Hello", Font = Current.Engine.CreateFont("Arial", 12f) };
        var blockout = new BlockoutTextSymbol { Text = "Hello", Font = Current.Engine.CreateFont("Arial", 12f) };

        var plainRect = EnvelopeOf(plain, display, point);
        var blockoutRect = EnvelopeOf(blockout, display, point);

        // Exact factor mirrors CanvasSizeExtensions.AddPadding (an implementation detail this
        // deliberately doesn't hard-code) - just confirm the box is genuinely inflated and grows
        // symmetrically around the same center, not shifted to one side.
        Assert.True(blockoutRect.Width > plainRect.Width, "blockout's collision box must be wider than the bare text");
        Assert.True(blockoutRect.Height > plainRect.Height, "blockout's collision box must be taller than the bare text");
        Assert.Equal(plainRect.Center.X, blockoutRect.Center.X, precision: 3);
    }

    [Fact]
    public void BlockoutTextSymbol_InflatesCollisionBoxByPaddingAndHalfBorderWidthOnEverySide()
    {
        var display = NewDisplay();
        var point = new Point(100, 100);

        var plain = new BlockoutTextSymbol { Text = "Hello", Font = Current.Engine.CreateFont("Arial", 12f) };
        var padded = new BlockoutTextSymbol
        {
            Text = "Hello",
            Font = Current.Engine.CreateFont("Arial", 12f),
            Padding = 6f,
            // A transparent (the default) border colour draws nothing, so it must not count
            // towards Margin either - only a genuinely visible border does.
            BorderColor = ArgbColor.Black,
            BorderWidth = 4f // a stroke straddles its path - only half extends beyond the fill rect
        };

        var plainRect = EnvelopeOf(plain, display, point);
        var paddedRect = EnvelopeOf(padded, display, point);

        // Grown symmetrically by Padding(6) + BorderWidth/2(2) = 8 on every side.
        Assert.Equal(plainRect.Left - 8f, paddedRect.Left, precision: 3);
        Assert.Equal(plainRect.Top - 8f, paddedRect.Top, precision: 3);
        Assert.Equal(plainRect.Width + 16f, paddedRect.Width, precision: 3);
        Assert.Equal(plainRect.Height + 16f, paddedRect.Height, precision: 3);
    }
}
