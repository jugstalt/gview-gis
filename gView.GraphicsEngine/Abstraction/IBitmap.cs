using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace gView.GraphicsEngine.Abstraction
{
    public interface IBitmap : IDisposable
    {
        int Width { get; }
        int Height { get; }

        PixelFormat PixelFormat { get; }

        ICanvas CreateCanvas();

        void MakeTransparent();
        void MakeTransparent(ArgbColor color);

        IBitmap Clone(PixelFormat format);

        void Save(string filename, ImageFormat format, int quality = 0);
        void Save(Stream stream, ImageFormat format, int quality = 0);

        BitmapPixelData LockBitmapPixelData(BitmapLockMode lockMode, PixelFormat pixelFormat);
        void UnlockBitmapPixelData(BitmapPixelData bitmapPixelData);

        object EngineElement { get; }
    }
}
