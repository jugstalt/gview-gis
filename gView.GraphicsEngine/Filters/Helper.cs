using gView.GraphicsEngine.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Filters
{
    class Helper
    {
        public static IBitmap CreateGrayscaleImage(int width, int height)
        {
            var bitmap = GraphicsEngine.Current.Engine.CreateBitmap(width, height, PixelFormat.Format8bppIndexed);
            SetGrayscalePalette(bitmap);
            return bitmap;
        }

        public static void SetGrayscalePalette(IBitmap image)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new Exception("Source image must be 8 bpp image.");

            // ToDo
            //ColorPalette cp = image.Palette;
            //for (int i = 0; i < 256; i++)
            //{
            //    cp.Entries[i] = Color.FromArgb(i, i, i);
            //}
            //image.Palette = cp;
        }

		public static int GetPixelFormatSize(PixelFormat pixfmt)
		{
			int result = 0;
			switch (pixfmt)
			{
				case PixelFormat.Format16bppArgb1555:
				case PixelFormat.Format16bppGrayScale:
				case PixelFormat.Format16bppRgb555:
				case PixelFormat.Format16bppRgb565:
					result = 16;
					break;
				case PixelFormat.Format1bppIndexed:
					result = 1;
					break;
				case PixelFormat.Format24bppRgb:
					result = 24;
					break;
				case PixelFormat.Format32bppArgb:
				case PixelFormat.Format32bppPArgb:
				case PixelFormat.Format32bppRgb:
					result = 32;
					break;
				case PixelFormat.Format48bppRgb:
					result = 48;
					break;
				case PixelFormat.Format4bppIndexed:
					result = 4;
					break;
				case PixelFormat.Format64bppArgb:
				case PixelFormat.Format64bppPArgb:
					result = 64;
					break;
				case PixelFormat.Format8bppIndexed:
					result = 8;
					break;
			}
			return result;
		}
	}
}
