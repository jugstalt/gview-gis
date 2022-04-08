using System;

namespace gView.GraphicsEngine
{
    public class BitmapPixelData
    {
        public BitmapPixelData(BitmapLockMode lockMode)
        {
            LockMode = lockMode;
        }

        public BitmapLockMode LockMode { get; }

        public int Height { get; set; }
        public PixelFormat PixelFormat { get; set; }
        public int Reserved { get; set; }
        public IntPtr Scan0 { get; set; }
        public int Stride { get; set; }
        public int Width { get; set; }
    }
}
