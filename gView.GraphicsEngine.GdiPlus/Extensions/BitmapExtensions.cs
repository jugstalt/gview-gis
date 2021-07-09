using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace gView.GraphicsEngine.GdiPlus.Extensions
{
    static public class BitmapExtensions
    {
        static public void SetGrayscalePalette(this Bitmap bitmap)
        {
            if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
                ColorPalette cp = bitmap.Palette;
                for (int i = 0; i < 256; i++)
                {
                    cp.Entries[i] = Color.FromArgb(i, i, i);
                }
                bitmap.Palette = cp;
            }
        }
    }
}
