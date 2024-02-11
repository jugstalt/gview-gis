using gView.GraphicsEngine.GdiPlus;
using gView.GraphicsEngine.Skia;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace gView.Framework.Common
{
    public class SystemInfo
    {
        public static Version Version = new Version(6, 24, 701);

        #region -> Private Variables

        public bool UseProcessorID;
        public bool UseBaseBoardProduct;
        public bool UseBaseBoardManufacturer;
        public bool UseDiskDriveSignature;
        public bool UseVideoControllerCaption;
        public bool UsePhysicalMediaSerialNumber;
        public bool UseBiosVersion;
        public bool UseBiosManufacturer;
        public bool UseWindowsSerialNumber;
        public bool UseMashineName = false;

        #endregion

        static public NumberFormatInfo Nhi = CultureInfo.InvariantCulture.NumberFormat;
        static public NumberFormatInfo Cnf = CultureInfo.CurrentCulture.NumberFormat;

        static public bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        static public bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        static public bool IsOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        static public void RegisterGdal1_10_PluginEnvironment()
        {
            if (IsWindows)
            {
                //Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

                Environment.SetEnvironmentVariable("PATH", $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}");
                Environment.SetEnvironmentVariable("GDAL_DRIVER_PATH", $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\gdalplugins");
            }
        }

        static public void RegisterDefaultGraphicEnginges(float dpi = 96)
        {
            new GdiGraphicsEngine(dpi).RegisterGraphcisEngine();
            (GraphicsEngine.Current.Engine = new SkiaGraphicsEngine(dpi)).RegisterGraphcisEngine();
        }

        #region HelperClasses
        class HardDrive
        {
            private string model = null;
            private string type = null;
            private string serialNo = null;
            public string Model
            {
                get { return model; }
                set { model = value; }
            }
            public string Type
            {
                get { return type; }
                set { type = value; }
            }
            public string SerialNo
            {
                get { return serialNo; }
                set { serialNo = value; }
            }
        }
        #endregion
    }
}
