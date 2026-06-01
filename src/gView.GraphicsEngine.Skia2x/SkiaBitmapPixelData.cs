namespace gView.GraphicsEngine.Skia
{
    class SkiaBitmapPixelData : BitmapPixelData
    {
        public SkiaBitmapPixelData(BitmapLockMode lockMode)
            : base(lockMode)
        {
            FreeMemory = false;
        }

        internal bool FreeMemory;
    }
}
