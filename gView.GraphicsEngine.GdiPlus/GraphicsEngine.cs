using gView.GraphicsEngine.Abstraction;
using System;
using System.Drawing;
using System.IO;

namespace gView.GraphicsEngine.GdiPlus
{
    public class GraphicsEngine : IGraphicsEngine
    {
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

        public IBitmap CreateBitmapFromHbitmap(IntPtr hBitmap)
        {
            return GdiBitmap.FromHbitmap(hBitmap);
        }

        public IFont CreateFont(string fontFamily, float size, FontStyle fontStyle = FontStyle.Regular)
        {
            return new GdiFont(fontFamily, size, fontStyle);
        }

        public IPen CreatePen(ArgbColor color, float width)
        {
            return new GdiPen(color, width);
        }

        public IBrush CreateSolidBrush(ArgbColor color)
        {
            return new GdiSolidBrush(color);
        }
    }
}
