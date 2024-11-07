using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using gView.Drawing.Pro.Exif;

namespace gView.Drawing.Pro
{
    public class ImageOperations
    {
        #region AutoRotate

        public static byte[] AutoRotate(Image image)
        {
            try
            {
                return AutoRotate(Image2Bytes(image));
            }
            catch { return null; }
        }

        public static byte[] AutoRotate(MemoryStream ms)
        {
            return AutoRotate(Stream2Bytes(ms));
        }

        public static byte[] AutoRotate(Stream stream)
        {
            using (Image img = Image.FromStream(stream))
            {
                return AutoRotate(img);
            }
        }

        public static byte[] AutoRotate(byte[] imageBytes, ImageMetadata metadata = null)
        {
            try
            {
                using (System.Drawing.Bitmap from = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(new MemoryStream(imageBytes)))
                {
                    var exif = new ExifTagCollection(from);
                    if (metadata != null)
                        metadata.ReadExif(exif);

                    if (exif["Orientation"] != null)
                    {
                        RotateFlipType flip = OrientationToFlipType(exif["Orientation"].Value);

                        if (flip != RotateFlipType.RotateNoneFlipNone) // don't flip of orientation is correct
                        {
                            from.RotateFlip(flip);
                            try
                            {
                                //exif.setTag(0x112, "1"); // Optional: reset orientation tag
                            }
                            catch { }

                            MemoryStream ms = new MemoryStream();
                            from.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                            return Stream2Bytes(ms);
                        }
                        else
                        {
                            return imageBytes;
                        }
                    }
                    else
                    {
                        return imageBytes;
                    }
                }
            }

            catch { }
            return null;
        }

        #endregion

        #region Scaledown 

        public static byte[] Scaledown(Image image, int maxDimension)
        {
            try
            {
                return Scaledown(Image2Bytes(image), maxDimension);
            }
            catch { return null; }
        }

        public static byte[] Scaledown(MemoryStream ms, int maxDimension)
        {
            return Scaledown(Stream2Bytes(ms), maxDimension);
        }

        public static byte[] Scaledown(byte[] imageBytes, int maxDimension)
        {
            try
            {
                using (System.Drawing.Image from = System.Drawing.Image.FromStream(new MemoryStream(imageBytes)))
                {
                    if (from.Width <= maxDimension && from.Height <= maxDimension)
                    {
                        return imageBytes;
                    }
                    else
                    {
                        int w, h;
                        if (from.Width > from.Height)
                        {
                            w = maxDimension;
                            h = (int)((float)from.Height / (float)from.Width * (float)w);
                        }
                        else
                        {
                            h = maxDimension;
                            w = (int)((float)from.Width / (float)from.Height * (float)h);
                        }

                        using (System.Drawing.Bitmap to = new System.Drawing.Bitmap(w, h))
                        using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(to))
                        {
                            gr.DrawImage(from,
                                new System.Drawing.RectangleF(0f, 0f, (float)w, (float)h),
                                new System.Drawing.RectangleF(0f, 0f, (float)from.Width, (float)from.Height),
                                System.Drawing.GraphicsUnit.Pixel);

                            MemoryStream ms = new MemoryStream();
                            to.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                            return Stream2Bytes(ms);
                        }
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        #endregion

        #region Crop

        public static byte[] Crop(byte[] sourceBytes, RectangleF sourceRect, Size destSize)
        {
            try
            {
                using (Image from = Image.FromStream(new MemoryStream(sourceBytes)))
                using (Bitmap dest = new Bitmap(destSize.Width, destSize.Height))
                using (Graphics gr = Graphics.FromImage(dest))
                {
                    dest.SetResolution(from.HorizontalResolution, from.VerticalResolution);

                    gr.DrawImage(from, new RectangleF(0f, 0f, dest.Width, dest.Height), sourceRect, GraphicsUnit.Pixel);
                    MemoryStream ms = new MemoryStream();
                    dest.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                    return Stream2Bytes(ms);
                }
            }
            catch
            {
            }
            return null;
        }

        #endregion

        #region Ratio

        public static byte[] Ratio(byte[] sourceBytes, float ratio)
        {
            Size sourceSize = ImageSize(sourceBytes);
            if (sourceSize.Width == 0 || sourceSize.Height == 0)
                return sourceBytes;

            double sourceRatio = SizeRatio(sourceSize);
            if ((float)Math.Round(sourceRatio, 7) == (float)Math.Round(ratio, 7))
            {
                return sourceBytes;
            }

            if (sourceRatio > ratio)
            {
                float height = sourceSize.Height;
                float width = (float)sourceSize.Height * ratio;

                RectangleF rect = new RectangleF(sourceSize.Width / 2f - width / 2f, 0, width, height);
                return Crop(sourceBytes, rect, new Size((int)width, (int)height));
            }
            else
            {
                float width = sourceSize.Width;
                float height = (float)sourceSize.Width / ratio;

                RectangleF rect = new RectangleF(0, sourceSize.Height / 2f - height / 2f, width, height);
                return Crop(sourceBytes, rect, new Size((int)width, (int)height));
            }
        }

        #endregion

        #region General

        public static Size ImageSize(byte[] bytes)
        {
            try
            {
                using (Image from = Image.FromStream(new MemoryStream(bytes)))
                {
                    return new Size(from.Width, from.Height);
                }
            }
            catch { }
            return new Size(0, 0);
        }

        public static byte[] ToBytes(MemoryStream ms)
        {
            return Stream2Bytes(ms);
        }

        #endregion

        #region Text

        public static byte[] StringToImage(string text, float size, System.Drawing.Color color)
        {
            using (System.Drawing.Bitmap bm1 = new Bitmap(1, 1))
            using (System.Drawing.Graphics gr1 = System.Drawing.Graphics.FromImage(bm1))
            {
                using (System.Drawing.Font font = new Font("Verdana", size))
                using (System.Drawing.SolidBrush brush = new SolidBrush(color))
                {
                    SizeF textSize = gr1.MeasureString(text, font);
                    using (System.Drawing.Bitmap bm2 = new Bitmap((int)textSize.Width, (int)textSize.Height))
                    using (System.Drawing.Graphics gr2 = System.Drawing.Graphics.FromImage(bm2))
                    {
                        gr2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                        bm2.MakeTransparent();

                        /*
                        StringFormat stringFormat = new StringFormat();
                        stringFormat.Alignment = StringAlignment.Far;
                        stringFormat.LineAlignment = StringAlignment.Far;

                        gr2.DrawString(text, font, brush, new Point(bm2.Width - (int)size / 4, bm2.Height - (int)size / 4), stringFormat);
                         * */
                        gr2.DrawString(text, font, brush, new Point(0, 0));

                        MemoryStream ms = new MemoryStream();
                        bm2.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                        return Stream2Bytes(ms);
                    }
                }
            }
        }

        #endregion

        #region Metadata 



        #endregion

        #region GPS Position

        /*

        public static double[] GpsPosition(Image image)
        {
            try
            {
                return GpsPosition(Image2Bytes(image));
            }
            catch { return null; }
        }

        public static double[] GpsPosition(MemoryStream ms)
        {
            return GpsPosition(Stream2Bytes(ms));
        }

        public static double[] GpsPosition(Stream stream)
        {
            using (Image img = Image.FromStream(stream))
            {
                return GpsPosition(img);
            }
        }

        public static double[] GpsPosition(byte[] imageBytes)
        {
            try
            {
                using (System.Drawing.Bitmap from = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(new MemoryStream(imageBytes)))
                {
                    var exif = new ExifTagCollection(from);

                    if (exif["GPSLatitude"] != null && exif["GPSLongitude"]!=null)
                    {
                        return new double[] { Convert.ToDouble(exif["GPSLongitude"]), Convert.ToDouble(exif["GPSLatitude"]) };
                    }
                }
            }

            catch { }
            return null;
        }
         * 
         * */

        #endregion

        #region Private Members

        internal static byte[] Stream2Bytes(MemoryStream ms)
        {
            byte[] buffer = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        internal static byte[] Image2Bytes(Image image, System.Drawing.Imaging.ImageFormat format = null)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, format == null ? System.Drawing.Imaging.ImageFormat.Jpeg : format);

            return Stream2Bytes(ms);
        }

        private static RotateFlipType OrientationToFlipType(string orientation)
        {
            switch (int.Parse(orientation))
            {
                case 1:
                    return RotateFlipType.RotateNoneFlipNone;
                case 2:
                    return RotateFlipType.RotateNoneFlipX;
                case 3:
                    return RotateFlipType.Rotate180FlipNone;
                case 4:
                    return RotateFlipType.Rotate180FlipX;
                case 5:
                    return RotateFlipType.Rotate90FlipX;
                case 6:
                    return RotateFlipType.Rotate90FlipNone;
                case 7:
                    return RotateFlipType.Rotate270FlipX;
                case 8:
                    return RotateFlipType.Rotate270FlipNone;
                default:
                    return RotateFlipType.RotateNoneFlipNone;
            }
        }

        private static float SizeRatio(Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return 0;

            return (float)size.Width / (float)size.Height;
        }

        #endregion
    }
}
