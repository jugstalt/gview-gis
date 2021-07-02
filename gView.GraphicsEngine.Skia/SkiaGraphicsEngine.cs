using gView.GraphicsEngine.Abstraction;
using System;
using System.IO;

namespace gView.GraphicsEngine.Skia
{
    public class SkiaGraphicsEngine : IGraphicsEngine
    {
        public SkiaGraphicsEngine(float screenDpi)
        {
            ScreenDpi = screenDpi;
        }
        

        #region IGraphicsEngine

        public float ScreenDpi { get; }

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

        public IBitmap CreateBitmapFromHbitmap(IntPtr hBitmap)
        {
            throw new NotImplementedException();
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

        public IGraphicsPath CreateGraphicsPath()
        {
            return new SkiaGraphicsPath();
        }

        public IBrush CreateHatchBrush(HatchStyle hatchStyle, ArgbColor foreColor, ArgbColor backColor)
        {
            return new SkiaSolidBrush(foreColor);
        }

        public IBrush CreateLinearGradientBrush(CanvasRectangleF rect, ArgbColor col1, ArgbColor col2, float angle)
        {
            return new SkiaSolidBrush(col1);
        }

        public IPen CreatePen(ArgbColor color, float width)
        {
            return new SkiaPen(color, width);
        }

        public IBrush CreateSolidBrush(ArgbColor color)
        {
            return new SkiaSolidBrush(color);
        }

        #endregion
    }
}
