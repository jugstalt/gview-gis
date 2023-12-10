using gView.Framework.Core.IO;
using gView.Framework.Core.Common;
using gView.Framework.Core.UI;
using gView.Framework.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace gView.Framework.Common
{
    public class SystemVariables
    {
        static public Version gViewVersion
        {
            get
            {
                return new Version(global::System.Diagnostics.FileVersionInfo.GetVersionInfo(
                    Assembly.GetAssembly(typeof(SystemVariables)).Location).ProductVersion);
                //return new Version(1, 0, 3000, 0);
            }
        }

        static public string CustomApplicationDirectory = string.Empty;
        static public string ApplicationDirectory
        {
            get
            {
                //System.Environment.CurrentDirectory;
                //DirectoryInfo di = new DirectoryInfo(System.Windows.Forms.Application.StartupPath);
                //return di.FullName;

                return PortableRootDirectory;
            }
        }

        static public string StartupDirectory
        {
            get { return ApplicationDirectory; }
        }

        static public string CustomMyApplicationData = string.Empty;
        static public string MyApplicationData
        {
            get
            {
                if (IsPortable)
                {
                    return PortableRootDirectory + @"\AppData";
                }

                if (!string.IsNullOrEmpty(CustomMyApplicationData))
                {
                    return CustomMyApplicationData;
                }

                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"/gView";
                DirectoryInfo di = new DirectoryInfo(path);
                if (!di.Exists)
                {
                    di.Create();
                }

                return di.FullName;
            }
        }

        static public string CustomMyCommonApplicationData = string.Empty;
        static public string MyCommonApplicationData
        {
            get
            {
                if (IsPortable)
                {
                    return PortableRootDirectory + @"/AppData";
                }

                if (!string.IsNullOrEmpty(CustomMyCommonApplicationData))
                {
                    return CustomMyCommonApplicationData;
                }

                string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"/gView";
                DirectoryInfo di = new DirectoryInfo(path);
                if (!di.Exists)
                {
                    di.Create();
                }

                return di.FullName;
            }
        }

        static public string CustomCommonApplicationData = string.Empty;
        static public string CommonApplicationData
        {
            get
            {
                if (IsPortable)
                {
                    return PortableRootDirectory + @"/AppData";
                }

                if (!string.IsNullOrEmpty(CustomCommonApplicationData))
                {
                    return CustomCommonApplicationData;
                }

                string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"/gView";
                DirectoryInfo di = new DirectoryInfo(path);
                if (!di.Exists)
                {
                    di.Create();
                }

                return di.FullName;
            }
        }
        static public string MyApplicationConfigPath
        {
            get
            {
                string path = MyApplicationData + @"/config";
                DirectoryInfo di = new DirectoryInfo(path);
                if (!di.Exists)
                {
                    di.Create();
                }

                return di.FullName;
            }
        }

        static public bool IsPortable
        {
            get
            {
                try
                {
                    string path = PortableRootDirectory;
                    return File.Exists(path + @"/gview_portable.config");
                }
                catch { return false; }
            }
        }

        static public string PortableRootDirectory
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetAssembly(typeof(SystemVariables)).Location);
            }
        }

        static private float _sytemFontScaleFactor = 1f;
        static public float SystemFontsScaleFactor
        {
            get
            {
                return _sytemFontScaleFactor;
            }
            set
            {
                _sytemFontScaleFactor = value;
            }
        }

        static public bool UseDiagnostic { get; set; } = false;
    }

    public class Cloner
    {
        virtual public object Clone()
        {
            if (!(this is IPersistable))
            {
                return null;
            }

            try
            {
                object clone = null;
                if (PlugInManager.IsPlugin(this))
                {
                    PlugInManager compMan = new PlugInManager();
                    clone = compMan.CreateInstance(PlugInManager.PlugInID(this));
                }
                else
                {
                    Assembly assembly = Assembly.GetAssembly(GetType());
                    clone = assembly.CreateInstance(GetType().ToString());
                }
                if (clone == null)
                {
                    return null;
                }

                XmlStream stream = new XmlStream("root");
                ((IPersistable)this).Save(stream);
                ((IPersistable)clone).Load(stream);

                return clone;
            }
            catch
            {
            }
            return null;
        }
    }

    public class Bit
    {
        static public bool Has(object val, object bit)
        {
            return ((int)val & (int)bit) == (int)bit;
        }
    }

    public class ProgressDialog
    {
        static public IProgressTaskDialog CreateProgressDialogInstance()
        {
            Assembly uiAssembly = Assembly.LoadFrom(SystemVariables.ApplicationDirectory + @"/gView.Win.Explorer.UI.dll");

            IProgressTaskDialog p = uiAssembly.CreateInstance("gView.Framework.UI.Dialogs.FormTaskProgress") as IProgressTaskDialog;
            return p;
        }
    }

    public class UserData : IUserData
    {
        private Dictionary<string, object> _dic = new Dictionary<string, object>();

        #region IUserDataDictinoary Member

        public void SetUserData(string type, object val)
        {
            try
            {
                object dummy;
                if (_dic.TryGetValue(type, out dummy))
                {
                    _dic[type] = val;
                }
                else
                {
                    _dic.Add(type, val);
                }
            }
            catch { }
        }

        public void RemoveUserData(string type)
        {
            try
            {
                _dic.Remove(type);
            }
            catch { }
        }
        public object GetUserData(string type)
        {
            object val;
            if (_dic.TryGetValue(type, out val))
            {
                return val;
            }

            return null;
        }

        public string[] UserDataTypes
        {
            get
            {
                List<string> keys = new List<string>();
                foreach (string key in _dic.Keys)
                {
                    keys.Add(key);
                }

                return keys.ToArray();
            }
        }

        public void CopyUserDataTo(IUserData userData)
        {
            if (userData == null)
            {
                return;
            }

            foreach (string key in _dic.Keys)
            {
                userData.SetUserData(key, _dic[key]);
            }
        }
        #endregion
    }
}
