using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.GdiPlus.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace gView.GraphicsEngine.GdiPlus
{
    class GdiBitmap : IBitmap
    {
        private Bitmap _bitmap;

        public GdiBitmap(int width, int height)
        {
            _bitmap = new Bitmap(width, height);
        }

        public GdiBitmap(int width, int height, PixelFormat format)
        {
            _bitmap = new Bitmap(width, height, format.ToPixelFormat());
        }

        public GdiBitmap(int width, int height, int stride, PixelFormat format, IntPtr scan0)
        {
            _bitmap = new Bitmap(width, height, stride, format.ToPixelFormat(), scan0);
        }

        public ICanvas CreateCanvas()
        {
            return new Canvas(_bitmap);
        }

        public void Dispose()
        {
            if (_bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }
        }

        public void Save(string filename, ImageFormat format, int quality = 0)
        {
            _bitmap.Save(filename, format.ToImageFormat());
        }

        public void Save(Stream stream, ImageFormat format, int quality = 0)
        {
            _bitmap.Save(stream, format.ToImageFormat());
        }
    }
}
