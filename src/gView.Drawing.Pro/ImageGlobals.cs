using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Drawing.Pro
{
    internal static class ImageGlobals
    {
        static public NumberFormatInfo Nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        public static Bitmap CreateGrayscaleImage(int width, int height)
        {
            Bitmap image = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            SetGrayscalePalette(image);
            return image;
        }

        public static void SetGrayscalePalette(Bitmap image)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new Exception("Source image must be 8 bpp image.");

            ColorPalette cp = image.Palette;
            for (int i = 0; i < 256; i++)
            {
                cp.Entries[i] = Color.FromArgb(i, i, i);
            }
            image.Palette = cp;
        }


    }
}
