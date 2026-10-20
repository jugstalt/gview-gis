using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace gView.DataSources.OSGeo
{
    public enum GdalVersion
    {
        Unknown = 0,
        V1 = 1,
        V3 = 2
    }
    static public class Initializer
    {
        /// <summary>
        /// Directory with the GDAL native libraries, if they are not deployed with gView
        /// (gdal*.dll/libgdal.so*, the *_wrap C# bindings, gdalplugins, gdalproj).
        /// </summary>
        public const string GdalPathEnvironmentVariable = "GVIEW_GDAL_PATH";

        static private bool _isRegistered = false;
        static private bool _registrationFailed = false;
        static private string _gdalDirectory = null;

        static public GdalVersion InstalledVersion = GdalVersion.Unknown;

        static private string ApplicationDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// Resolves the GDAL native libraries from GVIEW_GDAL_PATH, then from the application
        /// directory; otherwise the default search (PATH, system libraries) is used.
        /// </summary>
        static private void RegisterNativeLibraryResolver()
        {
            var gdalPath = Environment.GetEnvironmentVariable(GdalPathEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(gdalPath))
            {
                if (Directory.Exists(gdalPath))
                {
                    _gdalDirectory = Path.GetFullPath(gdalPath);
                    Console.WriteLine($"GDAL: using native libraries from {GdalPathEnvironmentVariable}={_gdalDirectory}");
                }
                else
                {
                    Console.WriteLine($"GDAL warning: {GdalPathEnvironmentVariable}={gdalPath} does not exist");
                }
            }

            if (_gdalDirectory == null)
            {
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // dependencies of gdal_wrap.dll (gdal*.dll, proj, ...) are searched in PATH
                Environment.SetEnvironmentVariable("PATH", $"{_gdalDirectory}{Path.PathSeparator}{Environment.GetEnvironmentVariable("PATH")}");
            }
            else
            {
                // libgdal.so is a dependency of the *_wrap.so bindings. dlopen does not search
                // GVIEW_GDAL_PATH, so preload it; without it, an installed (system) libgdal is used
                var libGdal = Directory.GetFiles(_gdalDirectory, "libgdal.so*")
                                       .OrderByDescending(f => f.Length)
                                       .FirstOrDefault();
                if (libGdal != null && !NativeLibrary.TryLoad(libGdal, out _))
                {
                    Console.WriteLine($"GDAL warning: can't load {libGdal}");
                }
            }

            NativeLibrary.SetDllImportResolver(typeof(Initializer).Assembly, (libraryName, assembly, searchPath) =>
            {
                foreach (var candidate in new[] { libraryName, $"{libraryName}.dll", $"lib{libraryName}.so", $"{libraryName}.so" })
                {
                    var path = Path.Combine(_gdalDirectory, candidate);
                    if (File.Exists(path) && NativeLibrary.TryLoad(path, out var handle))
                    {
                        return handle;
                    }
                }

                return IntPtr.Zero;  // default search
            });
        }

        static private string GdalSubDirectory(string name)
        {
            foreach (var directory in new[] { _gdalDirectory, ApplicationDirectory })
            {
                if (directory != null && Directory.Exists(Path.Combine(directory, name)))
                {
                    return Path.Combine(directory, name);
                }
            }

            return null;
        }

        static public bool RegisterAll()
        {
            if (_isRegistered)
            {
                return true;
            }

            if (_registrationFailed)
            {
                return false;
            }

            try
            {
                RegisterNativeLibraryResolver();

                try
                {
                    Console.WriteLine("Try Register GDAL 3.x");

                    // GDAL_DRIVER_PATH set by the admin wins (GDAL reads it itself)
                    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GDAL_DRIVER_PATH"))
                        && GdalSubDirectory("gdalplugins") is string driverPath)
                    {
                        OSGeo_v3.GDAL.Gdal.SetConfigOption("GDAL_DRIVER_PATH", driverPath);
                    }

                    // Raster-Block-Cache: wichtig bei wiederholtem Lesen / Kacheln / Overviews
                    //OSGeo_v3.GDAL.Gdal.SetConfigOption("GDAL_CACHEMAX", "512");      // MB bei GDAL 3.x

                    // Multithreading, falls Treiber/Operation es nutzt
                    // OSGeo_v3.GDAL.Gdal.SetConfigOption("GDAL_NUM_THREADS", "ALL_CPUS");
                    // OSGeo_v3.GDAL.Gdal.SetConfigOption("GDAL_NUM_THREADS", "2");  // 2 or 4, better for IIS Apps

                    OSGeo_v3.GDAL.Gdal.AllRegister();
                    OSGeo_v3.OGR.Ogr.RegisterAll();
                    
                    // PROJ_DATA set by the admin wins. PROJ_LIB is not checked: gView sets it for its own
                    // proj library (SystemInfo.RegisterProj4Lib), not for GDAL
                    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROJ_DATA"))
                        && GdalSubDirectory("gdalproj") is string projPath)
                    {
                        OSGeo_v3.OSR.Osr.SetPROJSearchPaths(new[] { projPath });
                    }

                    InstalledVersion = GdalVersion.V3;
                }
                catch(Exception ex)
                {
                    Console.WriteLine("GDAL 3.x Exception:");
                    Console.WriteLine(ex.Message);

                    Console.WriteLine("Try Register GDAL 1.x");
                    OSGeo_v1.GDAL.Gdal.AllRegister();
                    OSGeo_v1.OGR.Ogr.RegisterAll();

                    InstalledVersion = GdalVersion.V1;
                }
                _isRegistered = true;

                int driverCount = OSGeo_v3.GDAL.Gdal.GetDriverCount();
                var drivers = new string[driverCount];

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < driverCount; i++)
                {
                    var driver = OSGeo_v3.GDAL.Gdal.GetDriver(i);
                    drivers[i] = driver.LongName;
                    sb.Append($"{driver.ShortName}: {driver.LongName}{Environment.NewLine}");
                }

                Console.WriteLine("GDAL Drivers:");
                Console.WriteLine(sb.ToString());

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GDAL 1.x Exception:");
                Console.WriteLine(ex.Message);

                _registrationFailed = true;
                return false;
            }
        }
    }
}
