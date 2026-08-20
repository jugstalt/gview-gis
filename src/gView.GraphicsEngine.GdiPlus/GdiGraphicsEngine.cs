using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Threading;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace gView.GraphicsEngine.GdiPlus
{
    public class GdiGraphicsEngine : IGraphicsEngine
    {
        private static IThreadLocker _cloneObjectsLocker = new ThreadLocker();

        public GdiGraphicsEngine(float screenDpi)
        {
            ScreenDpi = screenDpi;
        }

        public string EngineName => "GdiPlus";

        public string EngineDisplayName => "GDI+ (GdiPlus)";

        public float ScreenDpi { get; }

        public bool MeasuresTextWithPadding => true;

        public IBitmap CreateBitmap(int width, int height)
        {
            return new GdiBitmap(width, height);
        }

        public IBitmap CreateBitmap(int width, int height, PixelFormat format)
        {
            return new GdiBitmap(width, height, format);
        }

        public IBitmap CreateBitmap(int width, int height, int stride, PixelFormat format, IntPtr scan0)
        {
            return new GdiBitmap(width, height, stride, format, scan0);
        }

        public IBitmap CreateBitmap(Stream stream)
        {
            return new GdiBitmap(stream);
        }

        public IBitmap CreateBitmap(string filename)
        {
            return new GdiBitmap(filename);
        }

        /// <summary>
        /// GDI+ has no SVG renderer available (Skia is the engine SVG markers are
        /// designed for; GDI+ is the legacy/opt-in fallback). Rather than fail the
        /// draw call - and with it the whole map render, since a thrown exception
        /// here would bubble out of the calling symbol's draw - this returns a
        /// generic placeholder marker instead. The map still renders; the affected
        /// symbols just don't show their actual SVG artwork under GDI+.
        /// </summary>
        public IBitmap RasterizeSvg(byte[] svgBytes, int pixelWidth, int pixelHeight)
        {
            pixelWidth = Math.Max(1, pixelWidth);
            pixelHeight = Math.Max(1, pixelHeight);

            var bitmap = CreateBitmap(pixelWidth, pixelHeight, PixelFormat.Rgba32);
            var gdiBitmap = (Bitmap)bitmap.EngineElement;

            using (var g = Graphics.FromImage(gdiBitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var inset = Math.Max(1f, Math.Min(pixelWidth, pixelHeight) * 0.1f);
                var rect = new RectangleF(inset, inset, pixelWidth - 2 * inset, pixelHeight - 2 * inset);
                var penWidth = Math.Max(1f, Math.Min(pixelWidth, pixelHeight) / 12f);

                using (var fill = new SolidBrush(Color.FromArgb(160, 160, 160, 160)))
                using (var pen = new Pen(Color.FromArgb(220, 90, 90, 90), penWidth))
                {
                    g.FillEllipse(fill, rect);
                    g.DrawEllipse(pen, rect);
                }
            }

            return bitmap;
        }

        public IDrawTextFormat CreateDrawTextFormat()
        {
            return new DrawTextFormat();
        }

        public IFont CreateFont(string fontFamily, float size, FontStyle fontStyle = FontStyle.Regular, GraphicsUnit grUnit = GraphicsUnit.Point)
        {
            return new GdiFont(fontFamily, size, fontStyle);
        }

        private static string[] _installedFontNames = null;
        public IEnumerable<string> GetInstalledFontNames()
        {
            if(_installedFontNames is null)
            {
                _installedFontNames = System.Drawing.FontFamily.Families.Select(f => f.Name).ToArray();
            }

            return _installedFontNames ?? Array.Empty<string>();
        }

        private static string _defaultFontName = null;
        public string GetDefaultFontName()
        {
            if (String.IsNullOrEmpty(_defaultFontName))
            {
                _defaultFontName = System.Drawing.FontFamily.GenericSansSerif.Name;
            }

            return _defaultFontName;
        }

        public IGraphicsPath CreateGraphicsPath()
        {
            return new GdiGraphicsPath();
        }

        public IPen CreatePen(ArgbColor color, float width)
        {
            return new GdiPen(color, width);
        }

        public IBrush CreateSolidBrush(ArgbColor color)
        {
            return new GdiSolidBrush(color);
        }

        public IBrush CreateLinearGradientBrush(CanvasRectangleF rect, ArgbColor col1, ArgbColor col2, float angle)
        {
            return new GdiLinearGradientBrush(rect, col1, col2, angle);
        }

        public IBrushCollection CreateHatchBrush(HatchStyle hatchStyle, ArgbColor foreColor, ArgbColor backColor)
        {
            return new BrushCollection(new IBrush[] { new GdiHatchBrush(hatchStyle, foreColor, backColor) });
        }

        public void DrawTextOffestPointsToFontUnit(ref CanvasPointF offset)
        {
            // System.Drawing use unit "points" => do nothing
        }

        public IThreadLocker CloneObjectsLocker => _cloneObjectsLocker;
    }
}
