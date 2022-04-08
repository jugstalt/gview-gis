using System;
using System.Windows.Forms;

namespace gView.Plugins.Snapping
{
    public partial class FormSnappingLauncher : Form
    {
        public FormSnappingLauncher(Module module)
        {
            InitializeComponent();

            optionPageSnapping1.Module = module;
            optionPageSnapping1.MakeGUI();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            optionPageSnapping1.Commit();
        }
    }
}
