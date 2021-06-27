using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace gView.GraphicsEngine.Abstraction
{
    public interface IGraphicsEngine
    {
        IBitmap CreateBitmap(int width, int height);
        IBitmap CreateBitmap(int width, int height, PixelFormat format);
        IBitmap CreateBitmap(int width, int height, int stride, PixelFormat format, IntPtr scan0);
        IBitmap CreateBitmap(Stream stream);
        IBitmap CreateBitmap(string filename);

        IPen CreatePen(ArgbColor color, float width);
        IBrush CreateSolidBrush(ArgbColor color);

        IFont CreateFont(string fontFamily, float size, FontStyle fontStyle = FontStyle.Regular);
    }
}
