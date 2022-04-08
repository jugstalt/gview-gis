namespace gView.GraphicsEngine.GdiPlus.Extensions
{
    static class BitmapLockModeExtensions
    {
        static public System.Drawing.Imaging.ImageLockMode ToGidImageLockMode(this BitmapLockMode mode)
        {
            switch (mode)
            {
                case BitmapLockMode.ReadWrite:
                    return System.Drawing.Imaging.ImageLockMode.ReadWrite;
                case BitmapLockMode.WriteOnly:
                    return System.Drawing.Imaging.ImageLockMode.WriteOnly;
                default:
                    return System.Drawing.Imaging.ImageLockMode.ReadOnly;
            }
        }
    }
}
