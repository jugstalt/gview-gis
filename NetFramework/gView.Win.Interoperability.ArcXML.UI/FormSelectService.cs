using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Interoperability.ArcXML.Dataset;
using gView.Framework.UI;
using gView.Framework.Data;

namespace gView.Interoperability.ArcXML.UI
{
    public partial class FormSelectService : Form,IModalDialog,IConnectionString
    {
        public FormSelectService()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                lstServices.Items.Clear();
                dotNETConnector connector = new dotNETConnector();

                connector.setAuthentification(txtUser.Text, txtPwd.Text);
                List<string> services = connector.getServiceNames(txtServer.Text);

                foreach (string service in services)
                {
                    lstServices.Items.Add(service,0);
                }
            }
            catch(Exception ex)
            {
                Cursor = Cursors.Default;

                MessageBox.Show(ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        #region IModalDialog Member

        public bool OpenModal()
        {
            if (this.ShowDialog() == DialogResult.OK)
                return true;

            return false;
        }

        #endregion

        #region IConnectionString Member

        private string _connectionString = "";
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }

        #endregion

        private void btnOK_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text;
            if (chkRouteThroughUser.Checked) username = "$";
            if (chkRouteThroughRole.Checked) username = "#";

            _connectionString = "server=" + txtServer.Text + 
                ";user=" + username + 
                ";pwd=" + txtPwd.Text + ";service=" + lstServices.SelectedItems[0].Text;
        }

        private void lstServices_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = (lstServices.SelectedItems.Count > 0);
        }

        private void chkRouteThroughUser_CheckedChanged(object sender, EventArgs e)
        {
            txtUser.Enabled = txtPwd.Enabled = !chkRouteThroughUser.Checked;
        }

        private void chkRouteThroughRole_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRouteThroughRole.Checked)
            {
                chkRouteThroughUser.Checked = true;
            }
        }
    }
}