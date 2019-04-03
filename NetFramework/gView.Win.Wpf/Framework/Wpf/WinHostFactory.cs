using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms.Integration;
using gView.Framework.UI;

namespace gView.Desktop.Wpf
{
    public static class WinHostFactory
    {
        static public WindowsFormsHost ToWindowsHost(System.Windows.Forms.Control control)
        {
            System.Windows.Forms.Control child = null;

            //if (control is IControl)
            //    ((IControl)control).LoadControl();

            if (control is System.Windows.Forms.Form)
            {
                child = new System.Windows.Forms.Panel();

                foreach (System.Windows.Forms.Control childControl in ((System.Windows.Forms.Form)control).Controls)
                {
                    child.Controls.Add(childControl);
                }
            }
            else
            {
                child = control;
            }

            WindowsFormsHost host = new WindowsFormsHost();
            host.Child = child;

            return host;
        }
    }
}
