using System;
using System.Runtime.CompilerServices;

namespace gView.GraphicsEngine.Skia
{
    static public class BitmapOperations
    {
        static public void CopyToArgb(this BitmapPixelData bitmapPixelData, IntPtr targetScan0)
        {
            unsafe
            {
                //var td = DateTime.Now;

                if (bitmapPixelData.PixelFormat == PixelFormat.Gray8)
                {
                    byte* source = (byte*)bitmapPixelData.Scan0.ToPointer();
                    uint* target = (uint*)targetScan0.ToPointer();

                    for (int row = 0, row_to = bitmapPixelData.Height; row < row_to; row++)
                    {
                        for (int col = 0, col_to = bitmapPixelData.Width; col < col_to; col++)
                        {
                            *target++ = MakePixel(*source, *source, *source++, 0xFF);
                        }
                    }
                }
                else if (bitmapPixelData.PixelFormat == PixelFormat.Rgb24)
                {
                    byte* source = (byte*)bitmapPixelData.Scan0.ToPointer();
                    uint* target = (uint*)targetScan0.ToPointer();

                    for (int row = 0, row_to = bitmapPixelData.Height; row < row_to; row++)
                    {
                        for (int col = 0, col_to = bitmapPixelData.Width; col < col_to; col++)
                        {
                            *target++ = MakePixel(*source++, *source++, *source++, 0xFF);
                        }
                    }
                }
                else
                {
                    byte* source = (byte*)bitmapPixelData.Scan0.ToPointer();
                    uint* target = (uint*)targetScan0.ToPointer();

                    for (int row = 0, row_to = bitmapPixelData.Height; row < row_to; row++)
                    {
                        for (int col = 0, col_to = bitmapPixelData.Width; col < col_to; col++)
                        {
                            *target++ = MakePixel(*source++, *source++, *source++, *source++);
                        }
                    }
                }

                //var ms = (DateTime.Now - td).TotalMilliseconds;
                //System.IO.File.AppendAllText("C:\\temp\\CopyToArgb.txt", $"CopyToArgb: { ms }ms{ Environment.NewLine }");
            }
        }

        static public void ReadFromArgb(this BitmapPixelData bitmapPixelData, IntPtr sourceScan0)
        {
            unsafe
            {
                if (bitmapPixelData.PixelFormat == PixelFormat.Gray8)
                {
                    byte* source = (byte*)sourceScan0.ToPointer();
                    byte* target = (byte*)bitmapPixelData.Scan0.ToPointer();

                    for (int row = 0, row_to = bitmapPixelData.Height; row < row_to; row++)
                    {
                        for (int col = 0, col_to = bitmapPixelData.Width; col < col_to; col++)
                        {
                            *target++ = (byte)(((*source++) + (*source++) + (*source++)) / 3);
                            source++;
                        }
                    }
                }
                else if (bitmapPixelData.PixelFormat == PixelFormat.Rgb24)
                {
                    byte* source = (byte*)sourceScan0.ToPointer();
                    byte* target = (byte*)bitmapPixelData.Scan0.ToPointer();

                    for (int row = 0, row_to = bitmapPixelData.Height; row < row_to; row++)
                    {
                        for (int col = 0, col_to = bitmapPixelData.Width; col < col_to; col++)
                        {
                            *target++ = *source++;
                            *target++ = *source++;
                            *target++ = *source++;
                            source++;
                        }
                    }
                }
                else
                {
                    uint* source = (uint*)sourceScan0.ToPointer();
                    uint* target = (uint*)bitmapPixelData.Scan0.ToPointer();

                    for (int row = 0, row_to = bitmapPixelData.Height; row < row_to; row++)
                    {
                        for (int col = 0, col_to = bitmapPixelData.Width; col < col_to; col++)
                        {
                            *target++ = *source++;
                        }
                    }
                }
            }
        }

        #region Helper

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static private uint MakePixel(byte red, byte green, byte blue, byte alpha) =>
            (uint)((alpha << 24) | (blue << 16) | (green << 8) | red);

        #endregion
    }
}
