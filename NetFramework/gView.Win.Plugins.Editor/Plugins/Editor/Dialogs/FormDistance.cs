using System.Windows.Forms;

namespace gView.Plugins.Editor.Dialogs
{
    public partial class FormDistance : Form
    {
        public FormDistance()
        {
            InitializeComponent();
        }

        public double Distance
        {
            get
            {
                return (double)numDistance.Value;
            }
            set
            {
                numDistance.Value = (decimal)value;
            }
        }
    }
}