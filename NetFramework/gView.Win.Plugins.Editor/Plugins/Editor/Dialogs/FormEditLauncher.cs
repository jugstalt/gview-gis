using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace gView.Plugins.Editor.Dialogs
{
    internal partial class FormEditLauncher : Form
    {
        public FormEditLauncher()
        {
            InitializeComponent();
        }

        public FormEditLauncher(Module module)
            : this()
        {
            optionPageEditing1.Module = module;

            optionPageEditing1.MakeGUI();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            optionPageEditing1.Commit();
        }
    }
}
