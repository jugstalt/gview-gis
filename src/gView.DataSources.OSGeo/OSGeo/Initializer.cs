using System;
using System.Reflection;

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
        static private bool _isRegistered = false;

        static public GdalVersion InstalledVersion = GdalVersion.Unknown;

        static public bool RegisterAll()
        {
            if (_isRegistered)
            {
                return true;
            }

            try
            {
                try
                {
                    Console.WriteLine("Try Register GDAL 3.x");

                    OSGeo_v3.GDAL.Gdal.SetConfigOption("GDAL_DRIVER_PATH", System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "gdalplugins"));

                    OSGeo_v3.GDAL.Gdal.AllRegister();
                    OSGeo_v3.OGR.Ogr.RegisterAll();
                    
                    OSGeo_v3.OSR.Osr.SetPROJSearchPaths(new[] { $"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/gdalproj" });

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

                return false;
            }
        }
    }
}
