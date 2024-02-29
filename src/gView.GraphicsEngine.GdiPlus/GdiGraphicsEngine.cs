using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Threading;
using System;
using System.Collections.Generic;
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
