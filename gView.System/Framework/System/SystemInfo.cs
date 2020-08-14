using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;

namespace gView.Framework.system
{
    public class SystemInfo
    {
        public static Version Version = new Version(5, 0, 11);

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

        internal static string MashineName
        {
            get
            {
                try
                {
                    return global::System.Net.Dns.GetHostName().Split('.')[0];
                }
                catch
                {
                    try
                    {
                        return global::System.Environment.MachineName.Split('.')[0];
                    }
                    catch { }
                }
                return String.Empty;
            }
        }
        private static string RemoveUseLess(string st)
        {
            char ch;
            for (int i = st.Length - 1; i >= 0; i--)
            {
                ch = char.ToUpper(st[i]);
                
                if ((ch < 'A' || ch > 'Z') &&
                    (ch < '0' || ch > '9'))
                {
                    st = st.Remove(i, 1);
                }
            }
            return st;
        }

        private static string TrimString(string str)
        {
            if (str == null) return "";
            str = str.Trim();
            while (str.IndexOf("0") == 0)
            {
                str = str.Substring(1, str.Length - 1);
            }
            return str;
        }

        static public NumberFormatInfo Nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
        static public NumberFormatInfo Cnf = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;

        static public bool IsLinux = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        static public bool IsWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        static public bool IsOSX = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        static public string Platform
        {
            get
            {
                if (IsLinux)
                    return OSPlatform.Linux.ToString();

                if (IsOSX)
                    return OSPlatform.OSX.ToString();

                if (IsWindows)
                    return OSPlatform.Windows.ToString();

                return "Unknown";
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
    }
}
