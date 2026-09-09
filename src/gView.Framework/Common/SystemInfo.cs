using gView.GraphicsEngine.GdiPlus;
using gView.GraphicsEngine.Skia;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace gView.Framework.Common
{
    public class SystemInfo
    {
        public static Version Version = new Version(8, 26, 3703);

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

        static public App CurrentApp { get; private set; }

        static public void RegisterApplicationEnvironment(App currentApp)
        {
            CurrentApp = currentApp;
        }

        static public void RegisterGdal1_10_PluginEnvironment()
        {
            if (IsWindows)
            {
                //Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");
                //Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");

                Console.WriteLine($"SET PATH={Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");
                Environment.SetEnvironmentVariable("PATH", $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");
                Console.WriteLine($"SET GDAL_DRIVER_PATH={Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\gdalplugins");
                Environment.SetEnvironmentVariable("GDAL_DRIVER_PATH", $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\gdalplugins");
            }
        }

        static public void RegisterProj4Lib(string path)
        {
            if(Path.Exists(path))
            {
                Environment.SetEnvironmentVariable("PROJ_LIB", path);
            }
        }

        static public void RegisterDefaultGraphicEngines(float dpi = 96)
        {
            if (IsWindows)
            {
                new GdiGraphicsEngine(dpi).RegisterGraphcisEngine();
            }

            (GraphicsEngine.Current.Engine = new SkiaGraphicsEngine(dpi)).RegisterGraphcisEngine();
        }

        /// <summary>
        /// Registers one or more font directories with the currently active graphics engine
        /// (<see cref="GraphicsEngine.Current"/>), so map rendering can resolve fonts placed
        /// in those directories without the fonts being installed in the operating system.
        /// Call after the engine has been selected (i.e. after
        /// <see cref="RegisterDefaultGraphicEngines"/> and any config-driven override).
        /// </summary>
        static public void RegisterFontDirectories(params string[] paths)
        {
            var engine = GraphicsEngine.Current.Engine;
            if (paths is null || engine is null)
            {
                return;
            }

            foreach (var path in paths)
            {
                if (String.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                try
                {
                    engine.RegisterFontDirectory(path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[fonts] could not register font directory '{path}' " +
                                      $"for engine {engine.EngineName}: {ex.Message}");
                }
            }
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

        #region Enums

        public enum App 
        {
            Server = 0,
            WebApps = 1,
            Command = 2,
        }

        #endregion
    }
}
