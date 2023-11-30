using System;

namespace gView.Framework.Cartography
{
    internal class GDI
    {
        public static int R2_XORPEN = 7;

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern int SetROP2(
                nint hdc,		// Handle to a Win32 device context
                int enDrawMode	// Drawing mode
                );

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern int SetPixel(
            nint hdc,
            int x, int y,
            long color
            );

        [System.Runtime.InteropServices.DllImport(@"D:\privat\bin\ras.dll")]
        public static extern int Resample(
            nint hdc1,
            int minx, int miny, int maxx, int maxy,
            nint hdc2);

        [System.Runtime.InteropServices.DllImport(@"D:\privat\bin\ras.dll")]
        public static extern int setPixel(
            nint hdc,
            int x, int y,
            long col);
    }
}
