using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace gView.DataSources.OGR.UI
{
    public partial class FormNewOgrDataset : Form
    {
        public FormNewOgrDataset()
        {
            InitializeComponent();
        }

        public FormNewOgrDataset(string connectionString)
            : this()
        {
            this.ConnectionString = connectionString;
        }

        public string ConnectionString
        {
            get { return txtConnectionString.Text; }
            set { txtConnectionString.Text = value; }
        }

        private void txtConnectionString_TextChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = !String.IsNullOrEmpty(txtConnectionString.Text);
        }

        private void btnTestConnecton_Click(object sender, EventArgs e)
        {
            OSGeo.OGR.Ogr.RegisterAll();

            OSGeo.OGR.DataSource dataSource = OSGeo.OGR.Ogr.Open(this.ConnectionString, 0);
            if (dataSource != null)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < Math.Min(dataSource.GetLayerCount(),20); i++)
                {
                    OSGeo.OGR.Layer ogrLayer = dataSource.GetLayerByIndex(i);
                    if (ogrLayer == null) continue;
                    sb.Append("\n"+ogrLayer.GetName());
                }
                MessageBox.Show("Connection succeeded...\n"+sb.ToString(), "Test Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Connection failed...", "Test Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnConnect2Folder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                this.ConnectionString = dlg.SelectedPath;
            }
        }

        private void btnConnect2File_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                this.ConnectionString = dlg.FileName;
            }
        }
    }
}
