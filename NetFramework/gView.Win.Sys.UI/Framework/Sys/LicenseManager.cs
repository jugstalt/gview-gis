using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Win32;
using System.Reflection;
using gView.Framework.system;

namespace gView.Framework.Sys.UI
{
    public class CLicenseManager
    {
        private string _BaseString;
        private string _Password;
        private string _SoftName, _licName = "";
        private string _Identifier;
        private LicenseTypes _runType = LicenseTypes.Unknown;
        private Guid _cryptoPassword = new Guid("406B7F91-C647-4401-ABC5-EACD27A743F5");
        private List<string> _components = new List<string>();
        private int _durationDays = 60;

        private bool _hasCompanyLicense = false;
        private string _companyName = String.Empty;
        private string _companyStreet = String.Empty;
        private string _companyZipCode = String.Empty;
        private string _companyCity = String.Empty;
        private string _companyCountry = String.Empty;

        public CLicenseManager()
            : this("gView", "738", false)
        {
        }
        public CLicenseManager(bool useRegistryBaseString)
            : this("gView", "738", useRegistryBaseString)
        {
        }
        private CLicenseManager(string softwareName, string identifier, bool useRegistryString)
        {
            _SoftName = softwareName;
            _Identifier = identifier;
            SetDefaults(useRegistryString);

            //FileInfo fi_comp = new FileInfo(SystemVariables.MyCommonApplicationData + @"\" + softwareName + "_comp.lic");
            //if (fi_comp.Exists)
            //{
            //    try
            //    {
            //        StreamReader sr = new StreamReader(fi_comp.FullName, Encoding.ASCII);
            //        string xml = sr.ReadToEnd();
            //        sr.Close();

            //        xml = Crypto.Decrypt(xml, _cryptoPassword.ToString("N")).Trim();
            //        XmlDocument doc = new XmlDocument();
            //        doc.LoadXml(xml);

            //        XmlNode compLicense = doc.SelectSingleNode("CompanyLicense[@Name and @Street and @ZipCode and @City and @Country]");
            //        _companyName = FromUnicodeBase64(compLicense.Attributes["Name"].Value);
            //        _companyStreet = FromUnicodeBase64(compLicense.Attributes["Street"].Value);
            //        _companyZipCode = FromUnicodeBase64(compLicense.Attributes["ZipCode"].Value);
            //        _companyCity = FromUnicodeBase64(compLicense.Attributes["City"].Value);
            //        _companyCountry = FromUnicodeBase64(compLicense.Attributes["Country"].Value);

            //        _hasCompanyLicense = true;
            //    }
            //    catch
            //    {
            //    }
            //}
            FileInfo fi = new FileInfo(SystemVariables.MyCommonApplicationData + @"\" + softwareName + ".lic");
            if (fi.Exists)
            {
                try
                {
                    StreamReader sr = new StreamReader(fi.FullName);
                    string xml = sr.ReadToEnd();
                    sr.Close();

                    xml = Crypto.Decrypt(xml, _cryptoPassword.ToString("N"));
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);

                    XmlNode license = doc.SelectSingleNode("License[@pwd]");
                    if (_Password == license.Attributes["pwd"].Value)
                    {
                        if (license.Attributes["lic"] != null)
                            _licName = license.Attributes["lic"].Value;

                        foreach (XmlNode comp in doc.SelectNodes("License/Components/comp[@name]"))
                        {
                            _components.Add(comp.Attributes["name"].Value.ToLower());
                        }
                        _runType = LicenseTypes.Licensed;
                    }
                    else // falsches Passwort
                    {
                        _runType = LicenseTypes.Unknown;
                        return;
                    }
                }
                catch
                {
                    _runType = LicenseTypes.Unknown;
                }
            }

            if (_runType == LicenseTypes.Unknown)
            {
                //DateTime t1 = SystemVariables.InstallationTime;
                //TimeSpan ts = DateTime.Now - t1;

                //if (ts.Days <= _durationDays)
                //{
                //    _runType = LicenseTypes.Express;
                //}
                //else
                //{
                //    _runType = LicenseTypes.Expired;
                //}
                _runType = LicenseTypes.Express;
            }
        }

        public LicenseTypes this[string componentName]
        {
            get
            {
                string[] compNames = componentName.ToLower().Split(';');

                if (_runType == LicenseTypes.Licensed)
                {
                    foreach (string comp in _components)
                    {
                        foreach (string compName in compNames)
                        {
                            if (comp == compName) return LicenseTypes.Licensed;
                        }
                    }
                    return LicenseTypes.Unknown;
                }
                else
                {
                    return _runType;
                }
            }
        }
       
        public string BaseString
        {
            get
            {
                return AddMinus(_BaseString, 5);
            }
        }
        public LicenseTypes RunType
        {
            get { return _runType; }
        }
        public string ProductName
        {
            get { return _licName; }
        }

        #region Company License Properties
        public bool HasCompanyLicense
        {
            get { return _hasCompanyLicense; }
        }
        public string CompanyName
        {
            get { return _companyName; }
        }
        public string CompanyStreet
        {
            get { return _companyStreet; }
        }
        public string CompanyZipCode
        {
            get { return _companyZipCode; }
        }
        public string CompanyCity
        {
            get { return _companyCity; }
        }
        public string CompanyCountry
        {
            get { return _companyCountry; }
        }
        #endregion

        internal FileInfo LicenseFile
        {
            get
            {
                return new FileInfo(SystemVariables.MyCommonApplicationData + @"\" + _SoftName + ".lic");
            }
        }
        internal string[] LicenseComponents
        {
            get
            {
                return _components != null ? _components.ToArray() : null;
            }
        }

        #region Password
        private void SetDefaults(bool useRegistryValues)
        {
            //SystemInfo.UseBaseBoardManufacturer = false;
            //SystemInfo.UseBaseBoardProduct = true;
            //SystemInfo.UseBiosManufacturer = false;
            //SystemInfo.UseBiosVersion = false;
            //SystemInfo.UseDiskDriveSignature = true;
            //SystemInfo.UsePhysicalMediaSerialNumber = true;
            //SystemInfo.UseProcessorID = true;
            //SystemInfo.UseVideoControllerCaption = false;
            //SystemInfo.UseWindowsSerialNumber = false;

            if (useRegistryValues)
            {
                _BaseString = LoadBaseStringFromRegistry();
            }
            else
            {
                MakeBaseString2();
            }
            MakePassword();
        }
        // Make base string (Computer ID)
        static private string MakeBaseString_old(string SoftName)
        {
            SystemInfo si = new SystemInfo();

            si.UseBaseBoardManufacturer = false;
            si.UseBaseBoardProduct = true;
            si.UseBiosManufacturer = false;
            si.UseBiosVersion = false;
            si.UseDiskDriveSignature = true;
            si.UsePhysicalMediaSerialNumber = true;
            si.UseProcessorID = true;
            si.UseVideoControllerCaption = false;
            si.UseWindowsSerialNumber = false;

            //_BaseString = Encryption.Boring(Encryption.InverseByBase(si.GetSystemInfo(_SoftName), 10));
            //SaveBaseStringToRegistry();

            return Encryption.Boring(Encryption.InverseByBase(si.GetSystemInfo(SoftName), 10));
        }

        private void MakeBaseString()
        {
            SystemInfo si = new SystemInfo();

            si.UseMashineName = true;
            si.UseBaseBoardManufacturer = false;
            si.UseBaseBoardProduct = true;
            si.UseBiosManufacturer = false;
            si.UseBiosVersion = false;
            si.UseDiskDriveSignature = true;
            si.UsePhysicalMediaSerialNumber = true;
            si.UseProcessorID = true;
            si.UseVideoControllerCaption = false;
            si.UseWindowsSerialNumber = false;

            _BaseString = Encryption.Boring(Encryption.InverseByBase(si.GetSystemInfo(_SoftName), 10));
            SaveBaseStringToRegistry(_BaseString);
        }

        private void MakeBaseString2()
        {
            SystemInfo si = new SystemInfo();

            si.UseMashineName = true;
            si.UseBaseBoardManufacturer = false;
            si.UseBaseBoardProduct = false;
            si.UseBiosManufacturer = false;
            si.UseBiosVersion = false;
            si.UseDiskDriveSignature = false;
            si.UsePhysicalMediaSerialNumber = false;
            si.UseProcessorID = false;
            si.UseVideoControllerCaption = false;
            si.UseWindowsSerialNumber = false;

            _BaseString = Encryption.Boring(Encryption.InverseByBase(si.GetSystemInfo(_SoftName), 10));
            SaveBaseStringToRegistry(_BaseString);
        }

        private void MakePassword()
        {
            _Password = Encryption.MakePassword(_BaseString, _Identifier);
        }
        private string AddMinus(string str, int blocklen)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < str.Length; i += blocklen)
            {
                string s = str.Substring(i, Math.Min(str.Length - i, blocklen));
                if (s.Length == 0) break;
                if (sb.Length > 0) sb.Append("-");
                sb.Append(s);
            }
            return sb.ToString();
        }

        static private string LoadBaseStringFromRegistry()
        {
            try
            {
                string guid = new Guid("7CB0A712-FCC7-478e-A7DB-0C7A9AC3F8CB").ToString();
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"Licenses\" + guid, false);

                string a = Crypto.Decrypt((string)key.GetValue("a"), guid.ToString());
                return a;
            }
            catch
            {
                return String.Empty;
            }
        }
        static private void SaveBaseStringToRegistry(string BaseString)
        {
            try
            {
                string guid = new Guid("7CB0A712-FCC7-478e-A7DB-0C7A9AC3F8CB").ToString();
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"Licenses\" + guid, true);
                if (key == null)
                {
                    key = Registry.ClassesRoot.CreateSubKey(@"Licenses\" + guid, RegistryKeyPermissionCheck.ReadWriteSubTree);
                }
                string a = Crypto.Encrypt(BaseString, guid.ToString());
                if (key.GetValue("a") != null)
                {
                    if (key.GetValue("a").ToString() == a)
                    {
                        key.Close();
                        return;
                    }
                }
                key.SetValue("a", a);
                key.Close();
            }
            catch
            {
            }
        }

        static private void DeleteLicInfoFromRegistry()
        {
            string guid = new Guid("7CB0A712-FCC7-478e-A7DB-0C7A9AC3F8CB").ToString();
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"Licenses\" + guid, true);

            try { key.DeleteValue("x"); }
            catch { }
            try { key.DeleteValue("m"); }
            catch { }
            try { key.DeleteValue("a"); }
            catch { }
        }

        static public bool OldBaseStringInstalled(bool IfTrueDelete)
        {
            String regBaseString = LoadBaseStringFromRegistry();
            if (regBaseString == String.Empty) return false;

            if (regBaseString.Equals(MakeBaseString_old("gView")))
            {
                if (IfTrueDelete)
                {
                    DeleteLicInfoFromRegistry();
                    FileInfo fi = new FileInfo(SystemVariables.ApplicationDirectory + @"/gView.lic");
                    if (fi.Exists)
                        fi.MoveTo(SystemVariables.ApplicationDirectory + @"/gView_old_" + regBaseString + ".lic");
                }
                return true;
            }
            return false;
        }
        static public bool CompareMachineNames(bool IfDifferentCreateNew)
        {
            try
            {
                string guid = new Guid("7CB0A712-FCC7-478e-A7DB-0C7A9AC3F8CB").ToString();
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"licenses/" + guid, false);

                if (key == null || key.GetValue("m") == null) return true;  // ???

                string m = (string)key.GetValue("m");
                key.Close();

                if (m.Equals(Identity.HashPassword(SystemInfo.MashineName)))
                    return true;

                if (IfDifferentCreateNew)
                {
                    key = Registry.ClassesRoot.OpenSubKey(@"licenses/" + guid, true);
                    if (key != null)  // ???
                    {
                        try { key.DeleteValue("m"); }
                        catch { }
                        try { key.DeleteValue("a"); }
                        catch { }
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        static public bool LicenseFileIsValid(string filename)
        {
            try
            {
                FileInfo fi = new FileInfo(filename);
                if (fi.Exists)
                {
                    CLicenseManager licMan = new CLicenseManager(false);

                    StreamReader sr = new StreamReader(fi.FullName);
                    string xml = sr.ReadToEnd();
                    sr.Close();

                    xml = Crypto.Decrypt(xml, licMan._cryptoPassword.ToString("N"));
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);

                    XmlNode license = doc.SelectSingleNode("License[@pwd]");
                    if (licMan._Password == license.Attributes["pwd"].Value)
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }
            return false;
        }
        static public bool CompanyLicenseFileIsValid(string filename)
        {
            try
            {
                FileInfo fi = new FileInfo(filename);
                if (fi.Exists)
                {
                    CLicenseManager licMan = new CLicenseManager(false);

                    StreamReader sr = new StreamReader(fi.FullName, Encoding.ASCII);
                    string xml = sr.ReadToEnd();
                    sr.Close();

                    xml = Crypto.Decrypt(xml, licMan._cryptoPassword.ToString("N")).Trim();
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);

                    XmlNode compLicense = doc.SelectSingleNode("CompanyLicense[@Name and @Street and @ZipCode and @City and @Country]");
                    return compLicense != null;
                }
            }
            catch/*(Exception ex)*/
            {

            }
            return false;
        }
        #endregion

        public bool GenerateFreeCompanyReaderLicense()
        {
            if (!_hasCompanyLicense)
                return false;

            MemoryStream ms = new MemoryStream();
            XmlTextWriter xw = new XmlTextWriter(ms, Encoding.ASCII);

            string baseString = _BaseString.Replace("-", "");
            if (baseString.Length != 25)
            {
                return false;
            }
            string password = Encryption.MakePassword(_BaseString, "738");

            xw.WriteStartDocument();
            xw.WriteStartElement("License");
            xw.WriteAttributeString("pwd", password);
            xw.WriteAttributeString("lic", "gView.Reader <" + _companyName + ">");

            xw.WriteStartElement("Components");

            xw.WriteStartElement("comp");
            xw.WriteAttributeString("name", "gView");
            xw.WriteEndElement(); // comp;
            xw.WriteStartElement("comp");
            xw.WriteAttributeString("name", "gView.Reader");
            xw.WriteEndElement(); // comp;
            xw.WriteStartElement("comp");
            xw.WriteAttributeString("name", "gView.PersonalMapServer");
            xw.WriteEndElement(); // comp;


            xw.WriteEndElement(); // Components
            xw.WriteEndElement(); // License
            xw.WriteEndDocument();

            xw.Flush();
            xw.Close();

            string str = Encoding.ASCII.GetString(ms.ToArray());

            string filename = SystemVariables.MyCommonApplicationData + @"\gview.lic";
            StreamWriter sw = new StreamWriter(filename);
            sw.Write(Crypto.Encrypt(str, _cryptoPassword.ToString("N")));
            sw.Flush();
            sw.Close();

            return true;
        }

        private string FromUnicodeBase64(string str)
        {
            return Encoding.Unicode.GetString(Convert.FromBase64String(str));
        }
    }

    public class License : ILicense
    {
        CLicenseManager _lic;
        public License()
        {
            _lic = new CLicenseManager(false);
        }
        public License(bool useRegistryKey)
        {
            _lic = new CLicenseManager(useRegistryKey);
        }
        #region ILicense Member

        public LicenseTypes ComponentLicenseType(string componentName)
        {
            return _lic[componentName];
        }

        public string ProductID
        {
            get { return _lic.BaseString; }
        }

        public LicenseTypes LicenseType
        {
            get { return _lic.RunType; }
        }

        public string ProductName
        {
            get { return _lic.ProductName; }
        }

        public string LicenseFile
        {
            get
            {
                return _lic.LicenseFile.FullName;
            }
        }
        public bool LicenseFileExists
        {
            get
            {
                return _lic.LicenseFile.Exists;
            }
        }
        public string[] LicenseComponents
        {
            get
            {
                return _lic.LicenseComponents;
            }
        }

        #endregion
    }
}
