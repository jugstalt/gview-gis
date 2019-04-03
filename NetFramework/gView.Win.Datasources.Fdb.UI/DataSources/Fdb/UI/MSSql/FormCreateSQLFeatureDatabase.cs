using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.IO;
using gView.Framework.UI;
using gView.Framework.UI.Dialogs;
using gView.Framework.system;
using System.Drawing.Design;
using gView.DataSources.Fdb.MSSql;

namespace gView.DataSources.Fdb.UI.MSSql
{
    public partial class FormCreateSQLFeatureDatabase : Form
    {
        private IExplorerObject _resultExObject = null;
        private AdvancedSettings _advancedSettings = new AdvancedSettings();

        public FormCreateSQLFeatureDatabase()
        {
            InitializeComponent();

            cmbType.SelectedIndex = 0;
            SqlServerLocator locator = new SqlServerLocator();
            string[] servers = locator.GetServers();
            if (servers != null)
            {
                foreach (string server in servers)
                {
                    cmbServer.Items.Add(server);
                }
            }
        }

        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            lUser.Enabled = lPassword.Enabled = txtUser.Enabled = txtPassword.Enabled = (cmbType.SelectedIndex == 0);
        }

        private void chkCreateConnection_CheckedChanged(object sender, EventArgs e)
        {
            txtObject.Enabled = chkCreateConnection.Checked;
        }

        public string ConnectionString
        {
            get
            {
                switch (cmbType.SelectedIndex)
                {
                    case 0:
                        return "server=" + cmbServer.Text + ";uid=" + txtUser.Text + ";pwd=" + txtPassword.Text;
                    case 1:
                        return "server=" + cmbServer.Text + ";Trusted_Connection=True;";
                }
                return "";
            }
        }

        public string FullConnectionString
        {
            get
            {
                switch (cmbType.SelectedIndex)
                {
                    case 0:
                        return "server=" + cmbServer.Text + ";database=" + txtDatabase.Text + ";uid=" + txtUser.Text + ";pwd=" + txtPassword.Text;
                    case 1:
                        return "server=" + cmbServer.Text + ";database=" + txtDatabase.Text + ";Trusted_Connection=True;";
                }
                return "";
            }
        }

        public bool CreateFromMDF
        {
            get { return chkUseFile.Checked; }
        }
        public string MdfFilename
        {
            get { return txtFilename.Text; }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                SqlFDB fdb = new SqlFDB();

                if (CreateFromMDF && MdfFilename != String.Empty)
                {
                    fdb.Open(ConnectionString);
                    if (!fdb.Create(txtDatabase.Text, MdfFilename))
                    {
                        MessageBox.Show(fdb.lastErrorMsg, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    fdb.Open(ConnectionString);

                    UserData parameters = _advancedSettings.ToUserData();
                    if (btnOnlyRepository.Checked) parameters.SetUserData("CreateDatabase", false);

                    if (!fdb.Create(txtDatabase.Text, parameters))
                    {
                        MessageBox.Show(fdb.lastErrorMsg, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (chkCreateReplicationDatamodel.Checked == true)
                    {
                        string errMsg;
                        if (!gView.Framework.Offline.Replication.CreateRelicationModel(fdb, out errMsg))
                        {
                            MessageBox.Show("RepliCreateRelicationModel failed:\n" + errMsg, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }

                if (chkCreateConnection.Checked)
                {
                    string connStr = FullConnectionString;
                    ConfigTextStream stream = new ConfigTextStream("sqlfdb_connections", true, true);
                    string id = txtObject.Text;
                    stream.Write(FullConnectionString, ref id);
                    stream.Close();

                    _resultExObject = new SqlFDBExplorerObject(null, id, FullConnectionString);
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("FATAL ERROR: " + ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        public IExplorerObject ResultExplorerObject
        {
            get { return _resultExObject; }
        }

        private void txtDatabase_TextChanged(object sender, EventArgs e)
        {
            txtObject.Text = txtDatabase.Text;
        }

        private void chkUseFile_CheckedChanged(object sender, EventArgs e)
        {
            lFilename.Enabled = txtFilename.Enabled = btnGetMDF.Enabled = chkUseFile.Checked;
            btnAdvanced.Enabled = chkCreateReplicationDatamodel.Enabled = !chkUseFile.Checked;

            btnCreateDatabase.Enabled = btnOnlyRepository.Enabled = !chkUseFile.Checked;

            if (chkUseFile.Checked == true)
                btnOK.Enabled = txtFilename.Text.Trim() != String.Empty;
            else
                btnOK.Enabled = true;
        }

        private void btnGetMDF_Click(object sender, EventArgs e)
        {
            if (openMdfFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtFilename.Text = openMdfFileDialog.FileName;
            }
        }

        private void txtFilename_TextChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = txtFilename.Text.Trim() != String.Empty;
        }

        private void btnAdvanced_Click(object sender, EventArgs e)
        {
            FormPropertyGrid grid = new FormPropertyGrid(_advancedSettings);
            grid.ShowDialog();
        }

        #region Classes
        class AdvancedSettings
        {
            private string _filename = String.Empty;
            private string _name = String.Empty;
            int _size = 0, _maxsize = 0, _filegrowth = 0;

            [Category("File")]
            [Editor(typeof(SaveMdfFileEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public string FILENAME
            {
                get { return _filename; }
                set { _filename = value; }
            }
            [Category("Name")]
            public string NAME
            {
                get { return _name; }
                set { _name = value; }
            }
            [Category("Size")]
            public int SIZE
            {
                get { return _size; }
                set { _size = value; }
            }
            [Category("Size")]
            public int MAXSIZE
            {
                get { return _maxsize; }
                set { _maxsize = value; }
            }
            [Category("Size")]
            public int FILEGROWTH
            {
                get { return _filegrowth; }
                set { _filegrowth = value; }
            }

            public UserData ToUserData()
            {
                UserData ud = new UserData();

                if (!String.IsNullOrEmpty(_name))
                    ud.SetUserData("NAME", _name);
                if (!String.IsNullOrEmpty(_filename))
                    ud.SetUserData("FILENAME", _filename);
                if (_size > 0)
                    ud.SetUserData("SIZE", _size.ToString());
                if (_maxsize > 0)
                    ud.SetUserData("MAXSIZE", _maxsize.ToString());
                if (_filegrowth > 0)
                    ud.SetUserData("FILEGROWTH", _filegrowth);

                return ud;
            }
        }

        class SaveMdfFileEditor : UITypeEditor
        {
            public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }

            public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "MDF File (*.mdf)|*.mdf";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    return dlg.FileName;
                }
                return value;
            }
        }
        #endregion
    }
}