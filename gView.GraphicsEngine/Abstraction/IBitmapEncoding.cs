using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace gView.GraphicsEngine.Abstraction
{
    public interface IBitmapEncoding
    {
        void Encode(IBitmap bitmap, string filename, ImageFormat format, int quality = 0);

        void Encode(IBitmap bitmap, Stream stream, ImageFormat format, int quality = 0);
    }
}
