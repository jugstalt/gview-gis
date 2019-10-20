using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.system;

namespace gView.Interoperability.GeoServices.Dataset
{
    public partial class FormNewConnection : Form
    {
        public FormNewConnection()
        {
            InitializeComponent();

            cmbServerType.SelectedIndex = 0;
            cbUseHttps.Checked = true;
        }

        public string ConnectionString
        {
            get
            {
                var server = txtServer.Text;
                if (!server.ToLower().StartsWith("http://") ||
                    !server.ToLower().StartsWith("https://"))
                {
                    server = (cbUseHttps.Checked == true ?
                        "https://" :
                        "http://") + server;
                }

                server = server.UrlRemoveEndingSlashes();

                if (!server.ToLower().EndsWith("/services"))
                {
                    switch (cmbServerType.SelectedIndex)
                    {
                        case 0:
                            server += "/arcgis/rest/services";
                            break;
                        case 1:
                            server += "/geoservices/rest/services";
                            break;
                    }
                }

                return "server=" + server + ";user=" + txtUser.Text + ";pwd=" + txtPwd.Text;
            }
        }
        private void btnOK_Click(object sender, EventArgs e)
        {

        }

        #region Helper

        #endregion
    }
}