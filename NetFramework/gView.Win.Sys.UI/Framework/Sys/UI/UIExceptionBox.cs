using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using System.Windows.Forms;

namespace gView.Framework.system.UI
{
    public class UIExceptionBox
    {
        static public void Show(UIException ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
