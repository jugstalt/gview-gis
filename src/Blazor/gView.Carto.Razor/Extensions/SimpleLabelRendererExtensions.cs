using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Cartography.UI;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Symbology;
using gView.GraphicsEngine;

namespace gView.Carto.Razor.Extensions;

static public class SimpleLabelRendererExtensions
{
    static public byte[] ToBytes(
        this SimpleLabelRenderer renderer,
        IMap currentMap,
        int width = 100,
        int height = 100,
        bool addCrossHair = true)
    {
        var imageBytes = Array.Empty<byte>();

        if (renderer.TextSymbol is ISymbol)
        {
            using (var bitmap = Current.Engine.CreateBitmap(width, height, PixelFormat.Rgba32))
            using (var canvas = bitmap.CreateCanvas())
            using (var memStream = new MemoryStream())
            {
                var rect = new CanvasRectangle(0, 0, bitmap.Width, bitmap.Height);

                using (var brush = Current.Engine.CreateSolidBrush(ArgbColor.White))
                {
                    canvas.FillRectangle(brush, rect);
                }

                if (addCrossHair)
                {
                    using (var pen = Current.Engine.CreatePen(ArgbColor.Gray, 1f))
                    {
                        pen.DashStyle = LineDashStyle.DashDotDot;
                        canvas.DrawLine(pen, 0f, rect.Height / 2f, rect.Width, rect.Height / 2f);
                        canvas.DrawLine(pen, rect.Width / 2f, 0f, rect.Width / 2f, rect.Height);
                    }
                }

                renderer.TextSymbol.Text = "Label";

                new SymbolPreview(currentMap).Draw(canvas, rect, renderer.TextSymbol, false);

                bitmap.Save(memStream, ImageFormat.Png);
                imageBytes = memStream.ToArray();
            }
        }

        return imageBytes;
    }

    static public string ToBase64ImageSource(
        this SimpleLabelRenderer renderer,
        IMap currentMap,
        int width = 100,
        int height = 100,
        bool addCrossHair = true)
        => $"data:image/png;base64, {Convert.ToBase64String(renderer.ToBytes(currentMap, width, height, addCrossHair))}";
}
