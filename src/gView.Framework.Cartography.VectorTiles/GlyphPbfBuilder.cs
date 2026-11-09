using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;
using System.Runtime.InteropServices;
using System.Text;

namespace gView.Framework.Cartography.VectorTiles;

/// <summary>
/// Glyph PBFs for MapLibre/Mapbox GL (<c>{fontstack}/{start}-{end}.pbf</c>): signed distance fields of 256
/// characters of a font, as the GL renderers expect them (24 px, buffer 3, radius 8, cutoff 0.25, like
/// fontnik/TinySDF). Rendered with gView's graphics engine from the installed fonts, so no font files or
/// external glyph server are needed.
/// </summary>
/// <remarks>
/// Metrics are measured from the rendered ink, not from engine font metrics, so it works the same for every
/// graphics engine: the bottom of an "H" is the baseline, the ink box of a glyph gives its left/top offsets.
/// </remarks>
public static class GlyphPbfBuilder
{
    private const int FontSize = 24;
    private const int Buffer = 3;
    private const double Radius = 8;
    private const double Cutoff = 0.25;
    private const int Super = 4;   // supersampling of the rendered glyph for the distance field

    /// <summary>
    /// The glyph PBF of a font stack (comma separated GL font names, e.g. "Arial Bold,Arial Unicode MS Regular")
    /// for the characters <paramref name="start"/> to <paramref name="start"/> + 255.
    /// </summary>
    public static byte[] Build(string fontStack, int start)
    {
        start -= start % 256;
        int end = start + 255;

        var (family, style) = ResolveFont(fontStack);
        using var font = Current.Engine.CreateFont(family, FontSize * Super, style, GraphicsUnit.Pixel);

        int canvasSize = FontSize * Super * 4;
        int originX = FontSize * Super, originY = FontSize * Super;
        int baselineRow = MeasureBaseline(font, canvasSize, originX, originY);

        var stack = new ProtoBuffer();
        stack.WriteString(1, fontStack);
        stack.WriteString(2, $"{start}-{end}");

        using var measureBitmap = Current.Engine.CreateBitmap(1, 1);
        using var measureCanvas = measureBitmap.CreateCanvas();
        float pairWidth = measureCanvas.MeasureText("xx", font).Width;

        for (int code = start; code <= end; code++)
        {
            if (code > 0xFFFF || (code >= 0xD800 && code <= 0xDFFF) || char.IsControl((char)code))
            {
                continue;
            }

            string text = ((char)code).ToString();
            int advance = (int)Math.Round((measureCanvas.MeasureText("x" + text + "x", font).Width - pairWidth) / Super);

            var glyph = new ProtoBuffer();
            glyph.WriteVarint(1, (ulong)code);

            var ink = RenderAlpha(font, text, canvasSize, originX, originY);
            if (ink.Bounds is var (minX, minY, maxX, maxY))
            {
                int width = (int)Math.Ceiling((maxX - minX + 1) / (double)Super);
                int height = (int)Math.Ceiling((maxY - minY + 1) / (double)Super);

                glyph.WriteBytes(2, DistanceField(ink.Alpha, canvasSize, minX, minY, width, height));
                glyph.WriteVarint(3, (ulong)width);
                glyph.WriteVarint(4, (ulong)height);
                glyph.WriteSInt(5, (int)Math.Floor((minX - originX) / (double)Super));
                glyph.WriteSInt(6, (int)Math.Round((baselineRow - minY) / (double)Super));
            }
            else
            {
                glyph.WriteVarint(3, 0);
                glyph.WriteVarint(4, 0);
                glyph.WriteSInt(5, 0);
                glyph.WriteSInt(6, 0);
            }

            glyph.WriteVarint(7, (ulong)Math.Max(0, advance));
            stack.WriteMessage(3, glyph);
        }

        var glyphs = new ProtoBuffer();
        glyphs.WriteMessage(1, stack);
        return glyphs.ToArray();
    }

    /// <summary>
    /// The first installed family of the stack, with the style words of the GL name ("Arial Bold Italic").
    /// </summary>
    public static (string Family, FontStyle Style) ResolveFont(string fontStack)
    {
        var installed = new HashSet<string>(Current.Engine.GetInstalledFontNames(), StringComparer.OrdinalIgnoreCase);
        (string, FontStyle)? first = null;

        foreach (var name in fontStack.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            var style = FontStyle.Regular;
            while (words.Count > 1)
            {
                string last = words[^1].ToLowerInvariant();
                if (last == "bold") style |= FontStyle.Bold;
                else if (last == "italic" || last == "oblique") style |= FontStyle.Italic;
                else if (last is not ("regular" or "normal" or "book" or "medium")) break;
                words.RemoveAt(words.Count - 1);
            }

            string family = String.Join(' ', words);
            first ??= (family, style);
            if (installed.Contains(family))
            {
                return (family, style);
            }
        }

        return first ?? ("Arial", FontStyle.Regular);
    }

    #region Rendering

    private static int MeasureBaseline(IFont font, int canvasSize, int originX, int originY)
    {
        // "H" stands on the baseline: its lowest ink row is the baseline
        var ink = RenderAlpha(font, "H", canvasSize, originX, originY);
        return ink.Bounds is var (_, _, _, maxY) ? maxY + 1 : originY + FontSize * Super;
    }

    private static (byte[] Alpha, (int MinX, int MinY, int MaxX, int MaxY)? Bounds) RenderAlpha(IFont font, string text, int size, int originX, int originY)
    {
        using var bitmap = Current.Engine.CreateBitmap(size, size, PixelFormat.Rgba32);
        bitmap.MakeTransparent();
        using (var canvas = bitmap.CreateCanvas())
        using (var brush = Current.Engine.CreateSolidBrush(ArgbColor.Black))
        {
            canvas.DrawText(text, font, brush, (float)originX, (float)originY);
            canvas.Flush();
        }

        var alpha = new byte[size * size];
        var data = bitmap.LockBitmapPixelData(BitmapLockMode.ReadOnly, PixelFormat.Rgba32);
        try
        {
            var row = new byte[data.Stride];
            for (int y = 0; y < size; y++)
            {
                Marshal.Copy(data.Scan0 + y * data.Stride, row, 0, data.Stride);
                for (int x = 0; x < size; x++)
                {
                    alpha[y * size + x] = row[x * 4 + 3];
                }
            }
        }
        finally
        {
            bitmap.UnlockBitmapPixelData(data);
        }

        int minX = int.MaxValue, minY = int.MaxValue, maxX = -1, maxY = -1;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (alpha[y * size + x] > 0)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        return (alpha, maxX < 0 ? null : (minX, minY, maxX, maxY));
    }

    /// <summary>
    /// The distance field of the glyph (width + 2 buffer) x (height + 2 buffer), from the supersampled ink:
    /// 255 - 255 * (d / radius + cutoff), d = signed distance in pixels (positive outside), as TinySDF.
    /// </summary>
    private static byte[] DistanceField(byte[] alpha, int size, int inkMinX, int inkMinY, int width, int height)
    {
        int outW = width + 2 * Buffer, outH = height + 2 * Buffer;

        // supersampled working area around the glyph
        int pad = Buffer * Super;
        int gw = outW * Super, gh = outH * Super;
        int ox = inkMinX - pad, oy = inkMinY - pad;

        var outside = new double[gw * gh];
        var inside = new double[gw * gh];
        for (int y = 0; y < gh; y++)
        {
            for (int x = 0; x < gw; x++)
            {
                int sx = ox + x, sy = oy + y;
                double a = sx >= 0 && sy >= 0 && sx < size && sy < size ? alpha[sy * size + sx] / 255.0 : 0;
                int i = y * gw + x;
                outside[i] = a >= 0.5 ? 0 : double.MaxValue;
                inside[i] = a >= 0.5 ? double.MaxValue : 0;
            }
        }

        EuclideanDistanceTransform(outside, gw, gh);
        EuclideanDistanceTransform(inside, gw, gh);

        var result = new byte[outW * outH];
        for (int y = 0; y < outH; y++)
        {
            for (int x = 0; x < outW; x++)
            {
                int i = (y * Super + Super / 2) * gw + (x * Super + Super / 2);
                double d = (Math.Sqrt(outside[i]) - Math.Sqrt(inside[i])) / Super;
                result[y * outW + x] = (byte)Math.Clamp(Math.Round(255 - 255 * (d / Radius + Cutoff)), 0, 255);
            }
        }

        return result;
    }

    /// <summary>Squared euclidean distance transform (Felzenszwalb &amp; Huttenlocher), in place.</summary>
    private static void EuclideanDistanceTransform(double[] grid, int width, int height)
    {
        int n = Math.Max(width, height);
        var f = new double[n];
        var d = new double[n];
        var v = new int[n];
        var z = new double[n + 1];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++) f[y] = grid[y * width + x];
            Transform1D(f, d, v, z, height);
            for (int y = 0; y < height; y++) grid[y * width + x] = d[y];
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++) f[x] = grid[y * width + x];
            Transform1D(f, d, v, z, width);
            for (int x = 0; x < width; x++) grid[y * width + x] = d[x];
        }
    }

    private static void Transform1D(double[] f, double[] d, int[] v, double[] z, int n)
    {
        const double Inf = 1e20;
        int k = 0;
        v[0] = 0;
        z[0] = -Inf;
        z[1] = Inf;

        for (int q = 1; q < n; q++)
        {
            double fq = Math.Min(f[q], Inf);
            double s;
            do
            {
                int r = v[k];
                s = (fq + (double)q * q - Math.Min(f[r], Inf) - (double)r * r) / (2.0 * q - 2.0 * r);
            } while (s <= z[k] && --k >= 0);

            k++;
            v[k] = q;
            z[k] = s;
            z[k + 1] = Inf;
        }

        k = 0;
        for (int q = 0; q < n; q++)
        {
            while (z[k + 1] < q) k++;
            int r = v[k];
            d[q] = (double)(q - r) * (q - r) + Math.Min(f[r], Inf);
        }
    }

    #endregion

    #region Protobuf

    /// <summary>The few protobuf wire types the glyph format needs.</summary>
    private sealed class ProtoBuffer
    {
        private readonly MemoryStream _stream = new();

        public void WriteVarint(int field, ulong value)
        {
            WriteRawVarint((ulong)(field << 3));
            WriteRawVarint(value);
        }

        public void WriteSInt(int field, int value)
            => WriteVarint(field, (ulong)(uint)((value << 1) ^ (value >> 31)));

        public void WriteBytes(int field, byte[] bytes)
        {
            WriteRawVarint((ulong)((field << 3) | 2));
            WriteRawVarint((ulong)bytes.Length);
            _stream.Write(bytes);
        }

        public void WriteString(int field, string value) => WriteBytes(field, Encoding.UTF8.GetBytes(value));

        public void WriteMessage(int field, ProtoBuffer message) => WriteBytes(field, message.ToArray());

        public byte[] ToArray() => _stream.ToArray();

        private void WriteRawVarint(ulong value)
        {
            while (value >= 0x80)
            {
                _stream.WriteByte((byte)(value | 0x80));
                value >>= 7;
            }
            _stream.WriteByte((byte)value);
        }
    }

    #endregion
}
