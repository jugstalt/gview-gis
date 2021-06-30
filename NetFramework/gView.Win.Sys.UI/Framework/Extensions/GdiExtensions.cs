using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;
using System;
using System.Drawing;
using System.IO;

namespace gView.Framework.Sys.UI.Extensions
{
    static public class GdiExtensions
    {
        static public Color ToGdiColor(this ArgbColor argbColor)
        {
            return Color.FromArgb(argbColor.ToArgb());
        }

        static public ArgbColor ToArgbColor(this Color color)
        {
            return ArgbColor.FromArgb(color.ToArgb());
        }

        static public Bitmap ToGdiBitmap(this IBitmap iBitmap)
        {
            if(iBitmap?.EngineElement is Bitmap)
            {
                return (Bitmap)iBitmap.EngineElement;
            }

            BitmapPixelData bmPixelData = null;
            try
            {
                bmPixelData = iBitmap.LockBitmapPixelData(BitmapLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var bm = new Bitmap(bmPixelData.Width,
                                  bmPixelData.Height,
                                  bmPixelData.Stride,
                                  System.Drawing.Imaging.PixelFormat.Format32bppArgb,
                                  bmPixelData.Scan0);

                //bm.Save($"E:\\xxx\\temp\\bitmap_{ Guid.NewGuid().ToString("N").ToString() }.png", System.Drawing.Imaging.ImageFormat.Png);

                return bm;
            }
            finally
            {
                if(bmPixelData!=null)
                {
                    iBitmap.UnlockBitmapPixelData(bmPixelData);
                }
            }
        }

        static public Bitmap CloneToGdiBitmap(this IBitmap iBitmap)
        {
            if (iBitmap?.EngineElement is Bitmap)
            {
                var gdiBitmap = (Bitmap)iBitmap.EngineElement;
                return gdiBitmap.Clone(new Rectangle(0, 0, iBitmap.Width, iBitmap.Height), gdiBitmap.PixelFormat);
            }

            return iBitmap.Clone(PixelFormat.Format32bppArgb).ToGdiBitmap();
        }

        static public IBitmap CloneToIBitmap(this Bitmap bitmap)
        {
            return CloneToIBitmap((Image)bitmap);
        }

        static public IBitmap CloneToIBitmap(this Image image)
        {
            if (image == null)
            {
                return null;
            }

            using (var ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                return GraphicsEngine.Current.Engine.CreateBitmap(ms);
            }
        }

        static public CanvasRectangle ToCanvasRectangle(this Rectangle rectangle)
        {
            return new CanvasRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        static public CanvasRectangleF ToCanvasRectangleF(this Rectangle rectangleF)
        {
            return new CanvasRectangleF(rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);
        }

        static public System.Drawing.FontStyle ToGdiFontStyle(this GraphicsEngine.FontStyle fontStyle)
        {
            switch (fontStyle)
            {
                case GraphicsEngine.FontStyle.Regular:
                    return System.Drawing.FontStyle.Regular;
                case GraphicsEngine.FontStyle.Bold:
                    return System.Drawing.FontStyle.Bold;
                case GraphicsEngine.FontStyle.Italic:
                    return System.Drawing.FontStyle.Italic;
                case GraphicsEngine.FontStyle.Strikeout:
                    return System.Drawing.FontStyle.Strikeout;
                case GraphicsEngine.FontStyle.Underline:
                    return System.Drawing.FontStyle.Underline;
            }

            return System.Drawing.FontStyle.Regular;
        }

        static public GraphicsEngine.FontStyle ToFontStyle(this System.Drawing.FontStyle fontStyle)
        {
            switch (fontStyle)
            {
                case System.Drawing.FontStyle.Regular:
                    return GraphicsEngine.FontStyle.Regular;
                case System.Drawing.FontStyle.Bold:
                    return GraphicsEngine.FontStyle.Bold;
                case System.Drawing.FontStyle.Italic:
                    return GraphicsEngine.FontStyle.Italic;
                case System.Drawing.FontStyle.Strikeout:
                    return GraphicsEngine.FontStyle.Strikeout;
                case System.Drawing.FontStyle.Underline:
                    return GraphicsEngine.FontStyle.Underline;
            }

            return GraphicsEngine.FontStyle.Regular;
        }

        static public CanvasRectangleF ToCanvasRectangleF(this RectangleF rectangleF)
        {
            return new CanvasRectangleF(rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);
        }
    }
}
