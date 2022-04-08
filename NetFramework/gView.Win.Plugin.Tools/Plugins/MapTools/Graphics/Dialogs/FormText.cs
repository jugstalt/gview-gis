using System.Windows.Forms;

namespace gView.Plugins.MapTools.Graphics.Dialogs
{
    public partial class FormText : Form
    {
        public FormText(string text)
        {
            InitializeComponent();

            txtText.Text = text;
        }

        public string SelectedText
        {
            get { return txtText.Text; }
            set { txtText.Text = value; }
        }
    }
}