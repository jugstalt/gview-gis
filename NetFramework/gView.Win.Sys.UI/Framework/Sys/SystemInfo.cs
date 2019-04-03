using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Collections;

namespace gView.Framework.Sys.UI
{
    internal class SystemInfo
    {
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

        public string GetSystemInfo(string SoftwareName)
        {
            //return GetSystemInfo2(SoftwareName);
            if (UseMashineName)
                SoftwareName += MashineName;

            if (UseProcessorID == true)
                SoftwareName += RunQuery("Processor", "ProcessorId");

            if (UseBaseBoardProduct == true)
                SoftwareName += RunQuery("BaseBoard", "Product");

            if (UseBaseBoardManufacturer == true)
                SoftwareName += RunQuery("BaseBoard", "Manufacturer");

            if (UseDiskDriveSignature == true)
                SoftwareName += RunQuery("DiskDrive", "Signature");

            if (UseVideoControllerCaption == true)
                SoftwareName += RunQuery("VideoController", "Caption");

            if (UsePhysicalMediaSerialNumber == true)
                SoftwareName += RunQuery("PhysicalMedia", "SerialNumber");

            if (UseBiosVersion == true)
                SoftwareName += RunQuery("BIOS", "Version");

            if (UseWindowsSerialNumber == true)
                SoftwareName += RunQuery("OperatingSystem", "SerialNumber");

            SoftwareName = RemoveUseLess(SoftwareName);

            if (SoftwareName.Length < 25)
                return GetSystemInfo(SoftwareName);

            return SoftwareName.Substring(0, 25).ToUpper();
        }

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

        private static string RunQuery(string TableName, string MethodName)
        {
            ManagementObjectSearcher MOS = new ManagementObjectSearcher("Select * from Win32_" + TableName);
            foreach (ManagementObject MO in MOS.Get())
            {
                try
                {
                    return TrimString(MO[MethodName].ToString());
                }
                catch/*(Exception ex)*/
                {
                    //System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }
            return "";
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

        public static string GetSystemInfo2(string SoftwareName)
        {
            ArrayList hdCollection = new ArrayList();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");

            foreach (ManagementObject wmi_HD in searcher.Get())
            {
                HardDrive hd = new HardDrive();
                hd.Model = wmi_HD["Model"].ToString();
                hd.Type = wmi_HD["InterfaceType"].ToString();
                hdCollection.Add(hd);
            }

            searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");

            int i = 0;
            foreach (ManagementObject wmi_HD in searcher.Get())
            {
                // get the hard drive from collection
                // using index
                HardDrive hd = (HardDrive)hdCollection[i];

                // get the hardware serial no.
                if (wmi_HD["SerialNumber"] == null)
                    hd.SerialNo = "None";
                else
                    hd.SerialNo = wmi_HD["SerialNumber"].ToString();

                ++i;
            }

            return String.Empty;
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
