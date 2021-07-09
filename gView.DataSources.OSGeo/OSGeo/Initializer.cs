using System;
using System.Text;

namespace gView.DataSources.OSGeo
{
    enum GdalVersion
    {
        Unknown = 0,
        V1 = 1,
        V3 = 2
    }
    static class Initializer
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
                    OSGeo_v3.GDAL.Gdal.AllRegister();
                    InstalledVersion = GdalVersion.V3;
                }
                catch
                {
                    OSGeo_v1.GDAL.Gdal.AllRegister();
                    InstalledVersion = GdalVersion.V1;
                }
                _isRegistered = true;

                int driverCount = OSGeo_v3.GDAL.Gdal.GetDriverCount();
                var drivers = new string[driverCount];
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < driverCount; i++)
                {
                    var driver = OSGeo_v1.GDAL.Gdal.GetDriver(i);
                    drivers[i] = driver.LongName;
                    sb.Append($"{ driver.ShortName }: { driver.LongName }\n");
                }


                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
