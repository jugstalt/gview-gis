using System.Runtime.InteropServices;

namespace gView.GraphicsEngine.Default
{
    static internal class GraphicsPlatform
    {
        static private bool _isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        static private bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        static private bool _isOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        static public bool IsLinux => _isLinux;
        static public bool IsWindows => _isWindows;
        static public bool IsOSX => _isOSX;

        static public string PlatformName
        {
            get
            {
                if (IsLinux)
                {
                    return OSPlatform.Linux.ToString();
                }

                if (IsOSX)
                {
                    return OSPlatform.OSX.ToString();
                }

                if (IsWindows)
                {
                    return OSPlatform.Windows.ToString();
                }

                return "Unknown";
            }
        }
    }
}
