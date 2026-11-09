using gView.Framework.Common;
using gView.Framework.Core.Common;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Geometry;
using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;
using System.Text.Json.Nodes;

namespace gView.Framework.Cartography.VectorTiles;

/// <summary>
/// A symbol the GL style refers to by <c>icon-image</c> (<see cref="Pattern"/> = false) or <c>fill-pattern</c>.
/// <see cref="Size"/> is the icon's base size in CSS pixels (the style scales it per zoom with <c>icon-size</c>).
/// </summary>
public sealed record SpriteSymbol(string Id, ISymbol Symbol, bool Pattern, float Size)
{
    /// <summary>Base size of icons whose symbol has no size of its own (picture markers, ...).</summary>
    public const float DefaultIconSize = 24;

    /// <summary>Edge of a fill pattern in CSS pixels; a multiple of 8 so the usual hatches repeat seamlessly.</summary>
    public const float PatternSize = 32;
}

/// <summary>
/// Renders the sprite sheet of a GL style (<c>sprite.png</c> + <c>sprite.json</c>, or the <c>@2x</c> variant):
/// every <see cref="SpriteSymbol"/> is drawn by gView itself, so pictures, svg and font markers and hatches look
/// as in the raster maps.
/// </summary>
public static class SpriteSheetBuilder
{
    private const int MaxSheetWidth = 1024;
    private const int Gap = 1;

    public static (byte[] Png, JsonObject Index) Build(IReadOnlyList<SpriteSymbol> sprites, int pixelRatio = 1)
    {
        pixelRatio = Math.Clamp(pixelRatio, 1, 4);

        var images = new List<(SpriteSymbol Sprite, IBitmap Bitmap)>();
        try
        {
            foreach (var sprite in sprites)
            {
                var bitmap = Render(sprite, pixelRatio);
                if (bitmap != null)
                {
                    images.Add((sprite, bitmap));
                }
            }

            #region Shelf packing

            int maxWidth = MaxSheetWidth * pixelRatio;
            int x = 0, y = 0, rowHeight = 0, sheetWidth = 1;
            var positions = new List<(int X, int Y)>();
            foreach (var (_, bitmap) in images)
            {
                if (x > 0 && x + bitmap.Width > maxWidth)
                {
                    x = 0;
                    y += rowHeight + Gap;
                    rowHeight = 0;
                }

                positions.Add((x, y));
                x += bitmap.Width + Gap;
                rowHeight = Math.Max(rowHeight, bitmap.Height);
                sheetWidth = Math.Max(sheetWidth, x);
            }
            int sheetHeight = Math.Max(1, y + rowHeight);

            #endregion

            var index = new JsonObject();
            using var sheet = Current.Engine.CreateBitmap(sheetWidth, sheetHeight, PixelFormat.Rgba32);
            sheet.MakeTransparent();
            using (var canvas = sheet.CreateCanvas())
            {
                for (int i = 0; i < images.Count; i++)
                {
                    var (sprite, bitmap) = images[i];
                    var (px, py) = positions[i];
                    canvas.DrawBitmap(bitmap, new CanvasPoint(px, py));

                    index[sprite.Id] = new JsonObject
                    {
                        ["x"] = px,
                        ["y"] = py,
                        ["width"] = bitmap.Width,
                        ["height"] = bitmap.Height,
                        ["pixelRatio"] = pixelRatio
                    };
                }
                canvas.Flush();
            }

            using var ms = new MemoryStream();
            sheet.Save(ms, ImageFormat.Png);
            return (ms.ToArray(), index);
        }
        finally
        {
            foreach (var (_, bitmap) in images)
            {
                bitmap.Dispose();
            }
        }
    }

    /// <summary>
    /// One symbol as image: icons centered on a canvas with room for outlines and offsets, patterns filling the
    /// whole canvas (without outline, the style draws it as line layer).
    /// </summary>
    private static IBitmap? Render(SpriteSymbol sprite, int pixelRatio)
    {
        int edge = sprite.Pattern
            ? (int)Math.Round(sprite.Size * pixelRatio)
            : (int)Math.Ceiling(sprite.Size * 2 * pixelRatio) + 4;
        if (edge <= 0)
        {
            return null;
        }

        // not 20 x 20: gView draws legend item symbols (that size) shrunk
        if (edge == 20)
        {
            edge = 22;
        }

        var bitmap = Current.Engine.CreateBitmap(edge, edge, PixelFormat.Rgba32);
        bitmap.MakeTransparent();

        using var canvas = bitmap.CreateCanvas();
        var display = new Display(null)
        {
            Dpi = 96.0 * pixelRatio,
            ImageWidth = edge,
            ImageHeight = edge
        };
        display.Limit = new Envelope(0, edge, edge, 0);
        display.ZoomTo(0, 0, edge, edge);
        display.Canvas = canvas;

        IGeometry geometry = sprite.Pattern
            ? Square(-2, edge + 2)
            : new Point(edge / 2.0, edge / 2.0);

        var symbol = sprite.Symbol.Clone(new CloneOptions(display, false)) as ISymbol ?? sprite.Symbol;
        if (sprite.Pattern && symbol is IOutlineSymbol outlined)
        {
            outlined.OutlineSymbol = null;
        }

        try
        {
            symbol.Draw(display, geometry);
        }
        finally
        {
            if (!ReferenceEquals(symbol, sprite.Symbol))
            {
                symbol.Release();
            }
        }

        canvas.Flush();
        return bitmap;
    }

    private static IPolygon Square(double min, double max)
    {
        var ring = new Ring();
        ring.AddPoint(new Point(min, min));
        ring.AddPoint(new Point(max, min));
        ring.AddPoint(new Point(max, max));
        ring.AddPoint(new Point(min, max));
        ring.Close();

        var polygon = new Polygon();
        polygon.AddRing(ring);
        return polygon;
    }
}
