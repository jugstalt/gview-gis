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
            if (iBitmap.EngineElement == null)
            {
                return new Bitmap(1, 1);
            }

            if (iBitmap?.EngineElement is Bitmap)
            {
                return (Bitmap)iBitmap.EngineElement;
            }

            BitmapPixelData bmPixelData = null;
            try
            {
                bmPixelData = iBitmap.LockBitmapPixelData(BitmapLockMode.Copy, PixelFormat.Rgba32);
                var bm = new Bitmap(bmPixelData.Width,
                                    bmPixelData.Height,
                                    bmPixelData.Stride,
                                    System.Drawing.Imaging.PixelFormat.Format32bppArgb,
                                    bmPixelData.Scan0);

                bmPixelData.Scan0 = IntPtr.Zero;  // Don't give it back...

                //bm.Save($"C:\\temp\\gr_{ Guid.NewGuid().ToString() }.png", System.Drawing.Imaging.ImageFormat.Png);

                return bm;
            }
            finally
            {
                if (bmPixelData != null)
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

            return iBitmap.ToGdiBitmap();
        }

        //static public IBitmap CloneToIBitmap(this Bitmap bitmap)
        //{
        //    return CloneToIBitmap((Image)bitmap);
        //}

        static public IBitmap CloneToIBitmap(this Image image)
        {
            if (image == null)
            {
                return null;
            }

            using (var ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

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
            var result = System.Drawing.FontStyle.Regular;

            if (fontStyle.HasFlag(GraphicsEngine.FontStyle.Bold))
            {
                result |= System.Drawing.FontStyle.Bold;
            }

            if (fontStyle.HasFlag(GraphicsEngine.FontStyle.Italic))
            {
                result |= System.Drawing.FontStyle.Italic;
            }

            if (fontStyle.HasFlag(GraphicsEngine.FontStyle.Strikeout))
            {
                result |= System.Drawing.FontStyle.Strikeout;
            }

            if (fontStyle.HasFlag(GraphicsEngine.FontStyle.Underline))
            {
                result |= System.Drawing.FontStyle.Underline;
            }

            return result;
        }

        static public GraphicsEngine.FontStyle ToFontStyle(this System.Drawing.FontStyle fontStyle)
        {
            var result = GraphicsEngine.FontStyle.Regular;

            if (fontStyle.HasFlag(System.Drawing.FontStyle.Bold))
            {
                result |= GraphicsEngine.FontStyle.Bold;
            }

            if (fontStyle.HasFlag(System.Drawing.FontStyle.Italic))
            {
                result |= GraphicsEngine.FontStyle.Italic;
            }

            if (fontStyle.HasFlag(System.Drawing.FontStyle.Strikeout))
            {
                result |= GraphicsEngine.FontStyle.Strikeout;
            }

            if (fontStyle.HasFlag(System.Drawing.FontStyle.Underline))
            {
                result |= GraphicsEngine.FontStyle.Underline;
            }

            return result;
        }

        static public CanvasRectangleF ToCanvasRectangleF(this RectangleF rectangleF)
        {
            return new CanvasRectangleF(rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);
        }
    }
}
