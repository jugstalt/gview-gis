namespace gView.GraphicsEngine.Skia3x
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
