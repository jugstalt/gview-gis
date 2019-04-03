using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.UI;
using System.Windows.Forms;

namespace gView.Framework.system.UI
{
    static public class AppUIGlobals
    {
        static public bool IsAppReadOnly(IApplication app)
        {
            return IsAppReadOnly(app, true);
        }
        static public bool IsAppReadOnly(IApplication app, bool showWarning)
        {
            bool ret = false;

            if (app is IMapApplication && ((IMapApplication)app).ReadOnly == true)
            {
                ret = true;
            }

            if (ret == true && showWarning)
            {
                MessageBox.Show("Not available.\nApplication is in readonly mode.", "Warining", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return ret;
        }
    }
}
