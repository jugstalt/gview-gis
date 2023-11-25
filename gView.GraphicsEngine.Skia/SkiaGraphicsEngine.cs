using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Threading;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gView.GraphicsEngine.Skia
{
    public class SkiaGraphicsEngine : IGraphicsEngine
    {
        public SkiaGraphicsEngine(float screenDpi)
        {
            ScreenDpi = screenDpi;
        }

        #region IGraphicsEngine

        public string EngineName => "SkiaSharp";

        public float ScreenDpi { get; }

        //
        // Skia measures text exact without a padding like GDI+
        // Some Funktions like BlockoutText and IncludesSuperScript requires MeasureText with padding
        //
        public bool MeasuresTextWithPadding => false;  

        #region Bitmap

        public IBitmap CreateBitmap(int width, int height)
        {
            return new SkiaBitmap(width, height);
        }

        public IBitmap CreateBitmap(int width, int height, PixelFormat format)
        {
            return new SkiaBitmap(width, height, format);
        }

        public IBitmap CreateBitmap(int width, int height, int stride, PixelFormat format, IntPtr scan0)
        {
            return new SkiaBitmap(width, height, stride, format, scan0);
        }

        public IBitmap CreateBitmap(Stream stream)
        {
            return new SkiaBitmap(stream);
        }

        public IBitmap CreateBitmap(string filename)
        {
            return new SkiaBitmap(filename);
        }

        #endregion

        public IDrawTextFormat CreateDrawTextFormat()
        {
            return new SkiaDrawTextFormat();
        }

        public IFont CreateFont(string fontFamily, float size, FontStyle fontStyle = FontStyle.Regular, GraphicsUnit grUnit = GraphicsUnit.Point)
        {
            return new SkiaFont(fontFamily, size, fontStyle, grUnit);
        }

        private static string[] _installedFontNames = null;
        public IEnumerable<string> GetInstalledFontNames()
        {
            if (_installedFontNames is null)
            {
                using (var fontManager = SKFontManager.Default)
                {
                    _installedFontNames = fontManager.FontFamilies?.ToArray() ?? [];
                }
            }

            return _installedFontNames;
        }

        private static readonly string[] CommonFonts = new string[]
        {
            "Times New Roman", "Verdana", "Tahoma", "Arial", "Helvetica",
            "Georgia", "Trebuchet MS", "Lucida Sans","Courier New"
        };
        private static string _defaultFontName = null;
        public string GetDefaultFontName()
        {
            if (_defaultFontName is null)
            {
                var availableFonts = GetInstalledFontNames();

                foreach (var font in CommonFonts)
                {
                    if (availableFonts.Any(f => f.Equals(font, StringComparison.OrdinalIgnoreCase)))
                    {
                        return _defaultFontName = font;
                    }
                }

                // Fallback
                return _defaultFontName = "Sans-serif";
            }

            return _defaultFontName;
        }

        public IGraphicsPath CreateGraphicsPath()
        {
            return new SkiaGraphicsPath();
        }

        public IBrushCollection CreateHatchBrush(HatchStyle hatchStyle, ArgbColor foreColor, ArgbColor backColor)
        {
            return SkiaHatchBrush.CreateCollection(hatchStyle, foreColor, backColor);
        }

        public IBrush CreateLinearGradientBrush(CanvasRectangleF rect, ArgbColor col1, ArgbColor col2, float angle)
        {
            return new SkiaLinearGradientBrush(rect, col1, col2, angle);
        }

        public IPen CreatePen(ArgbColor color, float width)
        {
            return new SkiaPen(color, width);
        }

        public IBrush CreateSolidBrush(ArgbColor color)
        {
            return new SkiaSolidBrush(color);
        }

        public void DrawTextOffestPointsToFontUnit(ref CanvasPointF offset)
        {
            //offset.X = offset.X.PointsToPixels() / 1.2f;
            //offset.Y = offset.Y.PointsToPixels() / 1.2f;
        }

        public IThreadLocker CloneObjectsLocker => null;
        
        #endregion
    }
}
