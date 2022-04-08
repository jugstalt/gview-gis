using System.Windows.Forms;

namespace gView.Plugins.Editor.Dialogs
{
    public partial class FormXY : Form
    {
        public FormXY()
        {
            InitializeComponent();
        }

        public double X
        {
            get
            {
                return (double)numX.Value;
            }
            set
            {
                numX.Value = (decimal)value;
            }
        }

        public double Y
        {
            get
            {
                return (double)numY.Value;
            }
            set
            {
                numY.Value = (decimal)value;
            }
        }
    }
}