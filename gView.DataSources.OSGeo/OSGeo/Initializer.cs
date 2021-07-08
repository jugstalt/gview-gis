using System;

namespace gView.DataSources.OSGeo
{
    static class Initializer
    {
        static private bool _isRegistered = false;

        static public bool RegisterAll()
        {
            if (_isRegistered)
            {
                return true;
            }

            try
            {
                OSGeo_v1.GDAL.Gdal.AllRegister();
                _isRegistered = true;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
