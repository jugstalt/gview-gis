using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormAddNetworkDirectory : Form
    {
        public FormAddNetworkDirectory()
        {
            InitializeComponent();
        }

        private void btnGetPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.SelectedPath = txtPath.Text;

            if (dlg.ShowDialog() == DialogResult.OK)
                txtPath.Text = dlg.SelectedPath;
        }

        public string Path
        {
            get { return txtPath.Text; }
        }
    }
}
