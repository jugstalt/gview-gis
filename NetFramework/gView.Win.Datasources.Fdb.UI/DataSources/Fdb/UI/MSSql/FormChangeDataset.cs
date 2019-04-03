using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.FDB;
using gView.Framework.Db.UI;
using gView.Framework.Db;
using gView.Framework.IO;
using gView.DataSources.Fdb.MSAccess;
using gView.DataSources.Fdb.MSSql;

namespace gView.DataSources.Fdb.UI.MSSql
{
    public partial class FormChangeDataset : Form
    {
        private string _providerId, _connectionString, _dsname;

        public FormChangeDataset(string providerId, string connectionSring)
        {
            InitializeComponent();

            _providerId = providerId;

            _dsname = ConfigTextStream.ExtractValue(connectionSring, "dsname");
            _connectionString = ConfigTextStream.RemoveValue(connectionSring, "dsname");
        }

        private void BuildList()
        {
            lstDatasets.Items.Clear();
            AccessFDB fdb = (_providerId == "mssql" ? new SqlFDB() : new AccessFDB());

            if (!fdb.Open(this.ConnectionString))
            {
                MessageBox.Show(fdb.lastErrorMsg, "Error");
                return;
            }

            string[] dsnames=fdb.DatasetNames;
            string imageSpace;
            if (dsnames != null)
            {
                foreach (string dsname in dsnames)
                {
                    ListViewItem item = new ListViewItem(
                        dsname, fdb.IsImageDataset(dsname, out imageSpace) ? 1 : 0);
                    lstDatasets.Items.Add(item);

                    if (item.Text == _dsname)
                    {
                        lstDatasets.SelectedIndices.Add(lstDatasets.Items.Count - 1);
                    }
                }
            }
        }

        public string ConnectionString
        {
            get
            {
                return _connectionString + ";dsname=" + _dsname;
            }
        }
        private void FormChangeDataset_Load(object sender, EventArgs e)
        {
            BuildList();
        }

        private void lstDatasets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstDatasets.SelectedItems.Count == 1)
            {
                _dsname = lstDatasets.SelectedItems[0].Text;
                btnOK.Enabled = true;
            }
            else
            {
                btnOK.Enabled = false;
            }
        }

        private void btnChangeConnectionString_Click(object sender, EventArgs e)
        {
            AccessFDB fdb = (_providerId == "mssql" ? new SqlFDB() : new AccessFDB());
            fdb.Open(_connectionString);

            DbConnectionString dbConnStr = new DbConnectionString();
            dbConnStr.UseProviderInConnectionString = false;
            FormConnectionString dlg = (dbConnStr.TryFromConnectionString("mssql", fdb.ConnectionString) ?
                new FormConnectionString(dbConnStr) : new FormConnectionString());
            
            dlg.ProviderID = _providerId;
            dlg.UseProviderInConnectionString = false;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                dbConnStr = dlg.DbConnectionString;
                _connectionString = dbConnStr.ConnectionString;

                BuildList();
            }
        }
    }
}