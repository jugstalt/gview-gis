using System;
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
