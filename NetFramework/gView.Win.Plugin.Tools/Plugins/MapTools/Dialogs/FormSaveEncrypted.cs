using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace gView.Plugins.Tools.MapTools.Dialogs
{
    public partial class FormSaveEncrypted : Form
    {
        public FormSaveEncrypted()
        {
            InitializeComponent();
        }

        public bool SaveEncrypted
        {
            get
            {
                return chkSaveEncrypted.Checked;
            }
        }
    }
}
