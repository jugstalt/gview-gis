using gView.Framework.IO;
using System.Windows.Forms;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormMetadata_old : Form
    {
        public FormMetadata_old(IPersistStream stream)
        {
            InitializeComponent();

            persistStreamGrid1.PersistStream = stream;
        }

        public IPersistStream Stream
        {
            get { return persistStreamGrid1.PersistStream; }
        }
    }
}