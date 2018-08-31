using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace gView.Framework.system
{
    // Activate Property
    /*
    public class Serial
    {
        #region -> Private Variables

        private string _BaseString;
        private string _Password;
        private string _SoftName;
        private string _RegFilePath;
        private string _HideFilePath;
        private int _DefDays;
        private string _Text;
        private string _Identifier;

        #endregion

        #region -> Constructor

        public Serial()
            :
            this("geographicView",
            SystemVariables.CommonApplicationData + @"\serial.key",
            SystemVariables.CommonApplicationData + @"\serial.dbf",
            60, "897")
        {
        }

        /// <summary>
        /// Make new TrialMaker class to make software trial
        /// </summary>
        /// <param name="SoftwareName">Name of software to make trial</param>
        /// <param name="RegFilePath">File path to save password(enrypted)</param>
        /// <param name="HideFilePath">file path for saving hidden information</param>
        /// <param name="Text">A text for contacting to you</param>
        /// <param name="TrialDays">Default period days</param>
        /// <param name="TrialRunTimes">How many times user can run as trial</param>
        /// <param name="Identifier">3 Digit string as your identifier to make password</param>
        public Serial(string SoftwareName,
            string RegFilePath, string HideFilePath,
            int TrialDays,
            string Identifier)
        {
            _SoftName = SoftwareName;
            _Identifier = Identifier;

            SetDefaults();

            _DefDays = TrialDays;

            _RegFilePath = RegFilePath;
            _HideFilePath = HideFilePath;

            MakeHideFile();
        }

        private void SetDefaults()
        {
            SystemInfo.UseBaseBoardManufacturer = false;
            SystemInfo.UseBaseBoardProduct = true;
            SystemInfo.UseBiosManufacturer = false;
            SystemInfo.UseBiosVersion = true;
            SystemInfo.UseDiskDriveSignature = true;
            SystemInfo.UsePhysicalMediaSerialNumber = false;
            SystemInfo.UseProcessorID = true;
            SystemInfo.UseVideoControllerCaption = false;
            SystemInfo.UseWindowsSerialNumber = false;

            MakeBaseString();
            MakePassword();
        }

        #endregion

        // Make base string (Computer ID)
        private void MakeBaseString()
        {
            _BaseString = Encryption.Boring(Encryption.InverseByBase(SystemInfo.GetSystemInfo(_SoftName), 10));
        }

        private void MakePassword()
        {
            _Password = Encryption.MakePassword(_BaseString, _Identifier);
        }

        public RunTypes RunAs(string product)
        {
            if (CheckRegister() == true)
                return RunTypes.Full;

            return RunTypes.Trial;
        }

        // save password to Registration file for next time usage
        private void MakeRegFile()
        {
            FileReadWrite.WriteFile(_RegFilePath, _Password);
        }

        // Control Registeration file for password
        // if password saved correctly return true else false
        private bool CheckRegister()
        {
            string Password = FileReadWrite.ReadFile(_RegFilePath);

            if (_Password == Password)
                return true;
            else
                return false;
        }

        // from hidden file
        // indicate how many days can user use program
        // if the file does not exists, make it
        private int DaysToEnd()
        {
            FileInfo hf = new FileInfo(_HideFilePath);
            if (hf.Exists == false)
            {
                MakeHideFile();
                return _DefDays;
            }
            return CheckHideFile();
        }

        // store hidden information to hidden file
        // Date,DaysToEnd,HowManyTimesRuned,BaseString(ComputerID)
        private void MakeHideFile()
        {
            FileInfo fi = new FileInfo(_HideFilePath);
            if (fi.Exists) return;

            string HideInfo;
            HideInfo = DateTime.Now.Ticks + ";";
            HideInfo += _DefDays + ";" + _BaseString;
            FileReadWrite.WriteFile(_HideFilePath, HideInfo);
        }

        // Get Data from hidden file if exists
        private int CheckHideFile()
        {
            string[] HideInfo;
            HideInfo = FileReadWrite.ReadFile(_HideFilePath).Split(';');
            long DiffDays;
            int DaysToEnd;

            if (_BaseString == HideInfo[3])
            {
                DaysToEnd = Convert.ToInt32(HideInfo[1]);
                if (DaysToEnd <= 0)
                {
                    _DefDays = 0;
                    return 0;
                }
                DateTime dt = new DateTime(Convert.ToInt64(HideInfo[0]));
                //DiffDays = DateAndTime.DateDiff(DateInterval.Day,
                //    dt.Date, DateTime.Now.Date,
                //    FirstDayOfWeek.Saturday,
                //    FirstWeekOfYear.FirstFullWeek);

                DiffDays = (long)((TimeSpan)(dt - DateTime.Now)).TotalDays;

                DaysToEnd = Convert.ToInt32(HideInfo[1]);

                DiffDays = Math.Abs(DiffDays);

                _DefDays = DaysToEnd - Convert.ToInt32(DiffDays);
            }
            return _DefDays;
        }

        public enum RunTypes
        {
            Trial = 0,
            Full,
            Expired,
            UnKnown
        }

        #region -> Properties

        /// <summary>
        /// Indicate File path for storing password
        /// </summary>
        public string RegFilePath
        {
            get
            {
                return _RegFilePath;
            }
            set
            {
                _RegFilePath = value;
            }
        }

        /// <summary>
        /// Indicate file path for storing hidden information
        /// </summary>
        public string HideFilePath
        {
            get
            {
                return _HideFilePath;
            }
            set
            {
                _HideFilePath = value;
            }
        }

        /// <summary>
        /// Get default number of days for trial period
        /// </summary>
        public int TrialPeriodDays
        {
            get
            {
                return _DefDays;
            }
        }

        /// <summary>
        /// Get or Set TripleDES key for encrypting files to save
        /// </summary>
        public byte[] TripleDESKey
        {
            get
            {
                return FileReadWrite.key;
            }
            set
            {
                FileReadWrite.key = value;
            }
        }

        #endregion

        #region -> Usage Properties

        public bool UseProcessorID
        {
            get
            {
                return SystemInfo.UseProcessorID;
            }
            set
            {
                SystemInfo.UseProcessorID = value;
            }
        }

        public bool UseBaseBoardProduct
        {
            get
            {
                return SystemInfo.UseBaseBoardProduct;
            }
            set
            {
                SystemInfo.UseBaseBoardProduct = value;
            }
        }

        public bool UseBaseBoardManufacturer
        {
            get
            {
                return SystemInfo.UseBiosManufacturer;
            }
            set
            {
                SystemInfo.UseBiosManufacturer = value;
            }
        }

        public bool UseDiskDriveSignature
        {
            get
            {
                return SystemInfo.UseDiskDriveSignature;
            }
            set
            {
                SystemInfo.UseDiskDriveSignature = value;
            }
        }

        public bool UseVideoControllerCaption
        {
            get
            {
                return SystemInfo.UseVideoControllerCaption;
            }
            set
            {
                SystemInfo.UseVideoControllerCaption = value;
            }
        }

        public bool UsePhysicalMediaSerialNumber
        {
            get
            {
                return SystemInfo.UsePhysicalMediaSerialNumber;
            }
            set
            {
                SystemInfo.UsePhysicalMediaSerialNumber = value;
            }
        }

        public bool UseBiosVersion
        {
            get
            {
                return SystemInfo.UseBiosVersion;
            }
            set
            {
                SystemInfo.UseBiosVersion = value;
            }
        }

        public bool UseBiosManufacturer
        {
            get
            {
                return SystemInfo.UseBiosManufacturer;
            }
            set
            {
                SystemInfo.UseBiosManufacturer = value;
            }
        }

        public bool UseWindowsSerialNumber
        {
            get
            {
                return SystemInfo.UseWindowsSerialNumber;
            }
            set
            {
                SystemInfo.UseWindowsSerialNumber = value;
            }
        }

        #endregion
    }
     * */
}
