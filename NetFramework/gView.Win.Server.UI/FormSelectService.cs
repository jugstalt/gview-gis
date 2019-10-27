using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.MapServer.Connector;

namespace gView.MapServer.Lib.UI
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

                MapServerConnection connection = new MapServerConnection("http://" + txtServer.Text + ":" + numPort.Value.ToString() + "/MapServer");
                foreach (MapServerConnection.MapService service in connection.Services(txtUser.Text, txtPwd.Text))
                {
                    lstServices.Items.Add(new ListViewItem(service.Name, (int)service.Type));
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

            _connectionString = "server=" + txtServer.Text + ":" + numPort.Value + ";user=" +
                username + ";pwd=" + txtPwd.Text + ";service=" + lstServices.SelectedItems[0].Text;
        }

        private void lstServices_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = (lstServices.SelectedItems.Count > 0);
        }

        private void chkRouteThrough_CheckedChanged(object sender, EventArgs e)
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