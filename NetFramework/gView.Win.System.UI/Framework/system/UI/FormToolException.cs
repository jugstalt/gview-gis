using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.system.UI.Framework.system.UI
{
    public partial class FormToolException : Form
    {
        public FormToolException(string title, string header, string message)
        {
            InitializeComponent();

            this.Text = title;
            txtHeader.Text = header;
            txtMessage.Text = message;
        }

        public static void Show(string header, string message, string title="Error")
        {
            new FormToolException(title, header, message).ShowDialog();
        }
    }
}
