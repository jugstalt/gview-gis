using System;
using System.Windows.Forms;

namespace gView.MapServer.Lib.UI
{
    public partial class FormNewConnection : Form
    {
        public FormNewConnection()
        {
            InitializeComponent();
        }

        public string ConnectionString
        {
            get
            {
                return "server=" + Server.Connector.ServerConnection.ServerUrl(txtServer.Text, Convert.ToInt32(numPort.Value)) + ";user=" + txtUser.Text + ";pwd=" + txtPwd.Text;
            }
        }
        private void btnOK_Click(object sender, EventArgs e)
        {

        }
    }
}