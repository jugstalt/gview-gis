using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace gView.Framework.system.UI
{
    public partial class FormException : Form
    {
        public FormException(Exception ex)
            : this(ex, false)
        {
        }
        public FormException(Exception ex, bool showExitApplication)
        {
            InitializeComponent();

            lblMessage.Text = ex.Message;

            StringBuilder sb = new StringBuilder();
            sb.Append("Details:\r\n");
            while (ex != null)
            {
                sb.Append("Message:" + ex.Message + "\r\n");
                sb.Append("Source:" + ex.Source + "\r\n");
                sb.Append("Stacktrace:" + ex.StackTrace + "\r\n");

                ex = ex.InnerException;
                if (ex != null) sb.Append("Inner Exception:\r\n");
            }

            txtDetails.Text = sb.ToString();
            btnCancel.Visible = lblContinue.Visible = showExitApplication;

            btnOK.Focus();
        }
    }
}
