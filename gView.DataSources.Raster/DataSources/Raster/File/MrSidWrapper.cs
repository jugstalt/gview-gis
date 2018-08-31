using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace gView.DataSources.Raster.File
{
    internal class MrSidWrapper
    {
        //
        //  ToDo: Könnte man das auch mit CSJ2K machen?   https://www.nuget.org/packages/CSJ2K/
        //

        [DllImport("MrSIDLib.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.IntPtr LoadMrSIDReader(string filename,ref MrSidGeoCoord geoCoords);

        [DllImport("MrSIDLib.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.IntPtr LoadMrSIDMemReader(IntPtr data, int data_size, ref MrSidGeoCoord geoCoords);

        [DllImport("MrSIDLib.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet=CharSet.Ansi)]
        public static extern System.IntPtr LoadJP2Reader(string filename, ref MrSidGeoCoord geoCoords);

        [DllImport("MrSIDLib.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.IntPtr LoadJP2MemReader(IntPtr data, int datasize, ref MrSidGeoCoord geoCoords);

        [DllImport("MrSIDLib.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern void FreeReader(IntPtr reader);

        [DllImport("MrSIDLib.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern IntPtr ReadHBitmap(IntPtr reader, int X1, int Y1, int width, int height, double mag);

        [DllImport("MrSIDLib.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern IntPtr Read(IntPtr reader, int X1, int Y1, int width, int height, double mag);

        [DllImport("MrSIDLib.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern void ReleaseHBitmap(IntPtr HBmp);

        [DllImport("MrSIDLib.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern void ReleaseBandData(IntPtr sceneBuffer);

        [DllImport("MrSIDLib.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern Int32 GetTotalCols(IntPtr sceneBuffer);

        [DllImport("MrSIDLib.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern Int32 GetTotalRows(IntPtr sceneBuffer);

        [DllImport("MrSIDLib.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern void ReadBandData(IntPtr sceneBuffer,IntPtr data, uint pixelBytes,uint rowBytes);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct MrSidGeoCoord
    {
        public int iWidth;
        public int iHeight;

        public double X;
        public double Y;

        public double xRes;
        public double yRes;

        public double xRot;
        public double yRot;

        public double MinMagnification;
        public double MaxMagnification;
    }
}
