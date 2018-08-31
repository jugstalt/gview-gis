using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.Carto
{
    internal class GDI
    {
        public static int R2_XORPEN = 7;

        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        public static extern int SetROP2(
                IntPtr hdc,		// Handle to a Win32 device context
                int enDrawMode	// Drawing mode
                );

        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        public static extern int SetPixel(
            IntPtr hdc,
            int x,int y,
            long color
            );

        [System.Runtime.InteropServices.DllImportAttribute(@"D:\privat\bin\ras.dll")]
        public static extern int Resample(
            IntPtr hdc1,
            int minx, int miny, int maxx, int maxy,
            IntPtr hdc2);

        [System.Runtime.InteropServices.DllImportAttribute(@"D:\privat\bin\ras.dll")]
        public static extern int setPixel(
            IntPtr hdc,
            int x, int y,
            long col);
    }
}
