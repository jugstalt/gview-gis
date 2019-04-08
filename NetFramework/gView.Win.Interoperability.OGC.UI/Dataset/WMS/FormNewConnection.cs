using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Interoperability.OGC.Dataset.WMS;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.Web;
using System.Xml;

namespace gView.Interoperability.OGC.UI.Dataset.WMS
{
    public partial class FormNewConnection : Form, IModalDialog, IConnectionString
    {
        public FormNewConnection()
        {
            InitializeComponent();

            cmbSERVICE.SelectedIndex = 0;
        }

        public string ConnectionString
        {
            get
            {
                string connStr = String.Empty;
                string usrpwd = (String.IsNullOrEmpty(txtUser.Text.Trim()) ? String.Empty : ";usr=" + txtUser.Text.Trim() + ";pwd=" + txtPwd.Text);
                switch (cmbSERVICE.SelectedIndex)
                {
                    case 0:
                        connStr = "wms=" + WMSUrl + ";service=WMS;servicename=" + ServiceName + usrpwd;
                        break;
                    case 1:
                        connStr = "wfs=" + WFSUrl + ";service=WFS;servicename=" + ServiceName + usrpwd;
                        break;
                    case 2:
                        connStr = "wms=" + WMSUrl + ";wfs=" + WFSUrl + ";service=WMS_WFS;servicename=" + ServiceName + usrpwd;
                        break;
                }
                return connStr;
            }
            set
            {
            }
        }

        public string ServiceName
        {
            get
            {
                return txtServiceName.Text.Trim();
            }
        }

        private string WMSUrl
        {
            get
            {
                string url = PrepareUrl(txtWMSUrl.Text.Trim());
                if (url == String.Empty) return String.Empty;

                if (txtServiceName.Text.Trim() != String.Empty)
                    url += "ServiceName=" + txtServiceName.Text.Trim();

                return url;
            }
        }

        private string WFSUrl
        {
            get
            {
                string url = PrepareUrl(txtWFSUrl.Text.Trim());
                if (url == String.Empty) return String.Empty;

                if (txtServiceName.Text.Trim() != String.Empty)
                    url += "ServiceName=" + txtServiceName.Text.Trim();

                return url;
            }
        }

        private string PrepareUrl(string url)
        {
            if (url == String.Empty) return String.Empty;
            if (url.EndsWith("?") ||
                url.EndsWith("&"))
            {
                url = url.Substring(0, url.Length - 1);
            }
            string c = "?";
            if (txtWMSUrl.Text.Contains("?"))
            {
                c = "&";
            }

            return url + c;
        }

        public SERVICE_TYPE ServiceType
        {
            get
            {
                return (SERVICE_TYPE)cmbSERVICE.SelectedIndex;
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

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void cmbSERVICE_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbSERVICE.SelectedIndex)
            {
                case 0:
                    txtWMSUrl.Enabled = btnTestWMS.Enabled = true;
                    txtWFSUrl.Enabled = btnTestWFS.Enabled = false;
                    break;
                case 1:
                    txtWMSUrl.Enabled = btnTestWMS.Enabled = false;
                    txtWFSUrl.Enabled = btnTestWFS.Enabled = true;
                    break;
                case 2:
                    txtWMSUrl.Enabled = btnTestWMS.Enabled = true;
                    txtWFSUrl.Enabled = btnTestWFS.Enabled = true;
                    break;
                default:
                    txtWMSUrl.Enabled = btnTestWMS.Enabled = false;
                    txtWFSUrl.Enabled = btnTestWFS.Enabled = false;
                    break;
            }
        }

        private void btnTestWMS_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                string url = txtWMSUrl.Text.Trim();
                if (!String.IsNullOrEmpty(txtServiceName.Text.Trim()))
                {
                    url = WebFunctions.AppendParametersToUrl(url, "ServiceName=" + txtServiceName.Text.Trim());
                }

                string param = "REQUEST=GetCapabilities&VERSION=1.1.1&SERVICE=WMS";
                url = WebFunctions.AppendParametersToUrl(url, param);

                string capabilities = WebFunctions.RemoveDOCTYPE(WebFunctions.DownloadXml(url, txtUser.Text.Trim(), txtPwd.Text));

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(capabilities);

                XmlNode exception = doc.SelectSingleNode("Exception");
                if (exception != null)
                {
                    MessageBox.Show(exception.InnerText, "Exception");
                    return;
                }
                xmlViewTree1.XmlDocument = doc;
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("ERROR: " + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void btnTestWFS_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                string url = txtWFSUrl.Text.Trim();
                if (!String.IsNullOrEmpty(txtServiceName.Text.Trim()))
                {
                    url = WebFunctions.AppendParametersToUrl(url, "ServiceName=" + txtServiceName.Text.Trim());
                }

                string param = "REQUEST=GetCapabilities&VERSION=1.1.1&SERVICE=WFS";
                url = WebFunctions.AppendParametersToUrl(url, param);

                string capabilities = WebFunctions.RemoveDOCTYPE(WebFunctions.DownloadXml(url));

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(capabilities);

                xmlViewTree1.XmlDocument = doc;
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("ERROR: " + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
    }
}