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
