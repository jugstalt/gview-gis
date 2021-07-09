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
            var bitmap = GraphicsEngine.Current.Engine.CreateBitmap(width, height, PixelFormat.Gray8);
            SetGrayscalePalette(bitmap);
            return bitmap;
        }

        public static void SetGrayscalePalette(IBitmap image)
        {
            if (image.PixelFormat != PixelFormat.Gray8)
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
				case PixelFormat.Rgb24:
					result = 24;
					break;
				case PixelFormat.Rgba32:
				case PixelFormat.Rgb32:
					result = 32;
					break;
				case PixelFormat.Gray8:
					result = 8;
					break;
			}
			return result;
		}
	}
}
