using System.Windows.Forms;

namespace gView.MapServer.Lib.UI
{
    public partial class FormAddServiceableDataset : Form
    {
        public FormAddServiceableDataset()
        {
            InitializeComponent();
        }

        public string ServiceName
        {
            get
            {
                return txtServiceName.Text;
            }
            set
            {
                txtServiceName.Text = value;
            }
        }
    }
}