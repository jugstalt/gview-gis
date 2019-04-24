using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using gView.Framework.Globalisation;

namespace gView.Plugins.Editor
{
    internal class Globalisation
    {
        static private ResourceManager _resMan = null;

        static public string GetResString(string name)
        {
            return GetResString(name, name);
        }
        static public string GetResString(string name, string defString)
        {
            try
            {
                if (_resMan == null)
                {
                    _resMan = new ResourceManager(
                        "gView.Editor.globalisation",
                        typeof(Globalisation).Assembly);
                }

                string ret = _resMan.GetString(name);
                if (ret == null)
                    return defString;

                return ret;
            }
            catch
            {
                return defString;
            }
        }
    }
}
