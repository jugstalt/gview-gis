using gView.Framework.Carto.UI;
using gView.Framework.Symbology;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace gView.Blazor.Core.Extensions;

static public class SymbolExtensions
{
    static public byte[] ToBytes(this ISymbol? symbol, int width = 30, int height = 20)
    {
        var imageBytes = Array.Empty<byte>();

        if (symbol is not null)
        {

            using (var bitmap = GraphicsEngine.Current.Engine.CreateBitmap(width, height, GraphicsEngine.PixelFormat.Rgba32))
            using (var canvas = bitmap.CreateCanvas())
            using (var memStream = new MemoryStream())
            {
                bitmap.MakeTransparent();
                new SymbolPreview(null).Draw(canvas, new GraphicsEngine.CanvasRectangle(0, 0, width, height), symbol, false);
                bitmap.Save(memStream, GraphicsEngine.ImageFormat.Png);

                imageBytes = memStream.ToArray();

            }
        }

        return imageBytes;
    }

    static public string ToBase64ImageSource(this ISymbol? symbol, int width = 30, int height = 20)
        => $"data:image/png;base64, {Convert.ToBase64String(symbol.ToBytes(width, height))}";
}
