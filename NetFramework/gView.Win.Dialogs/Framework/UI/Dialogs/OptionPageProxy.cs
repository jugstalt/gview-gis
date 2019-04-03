using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Web;
using gView.Framework.Globalisation;

namespace gView.Framework.UI.Dialogs
{
    [gView.Framework.system.RegisterPlugIn("399FD7B5-915C-4035-84E6-FA4C5DFE1A95")]
    public partial class OptionPageProxy : Form, IMapOptionPage, IExplorerOptionPage
    {
        public OptionPageProxy()
        {
            InitializeComponent();

            //ProxySettings.Load();
            //gView.Framework.Web.ProxySettings settings = new gView.Framework.Web.ProxySettings();
            cmbUseType.SelectedIndex = (int)ProxySettings.UseProxy;
            txtServer.Text = ProxySettings.Server;
            numPort.Value = Math.Max(ProxySettings.Port, 1);
            txtExceptions.Text = ProxySettings.Exceptions;
            txtDomain.Text = ProxySettings.Domain;
            txtUser.Text = ProxySettings.User;
            txtPassword.Text = "          ";

            cmbUseType_SelectedIndexChanged(cmbUseType, new EventArgs());
        }

        #region IMapOptionPage Member

        public Panel OptionPage(IMapDocument document)
        {
            return this.PagePanel;
        }

        public string Title
        {
            get { return LocalizedResources.GetResString("String.ProxySettings", "Proxy Settings"); }
        }

        public Image Image
        {
            get { return null; }
        }

        public void Commit()
        {
            //gView.Framework.Web.ProxySettings settings = new gView.Framework.Web.ProxySettings();

            ProxySettings.UseProxy = (ProxySettings.UseProxyType)cmbUseType.SelectedIndex;
            ProxySettings.Server = txtServer.Text;
            ProxySettings.Port = (int)numPort.Value;
            ProxySettings.Exceptions = txtExceptions.Text;
            ProxySettings.Domain = txtDomain.Text;
            ProxySettings.User = txtUser.Text;
            if (txtPassword.Text != "          ")
                ProxySettings.Password = txtPassword.Text;

            if (!ProxySettings.Commit())
            {
                MessageBox.Show("ERROR: Can't write config...");
            }
        }

        public bool IsAvailable(IMapDocument document)
        {
            return true;
        }

        #endregion

        #region IExplorerOptionPage Member

        public Panel OptionPage()
        {
            return this.PagePanel;
        }

        #endregion

        private void cmbUseType_SelectedIndexChanged(object sender, EventArgs e)
        {
            groupBox1.Visible = groupBox2.Visible = groupBox3.Visible =
                (ProxySettings.UseProxyType)cmbUseType.SelectedIndex == ProxySettings.UseProxyType.use;
        }

    }
}