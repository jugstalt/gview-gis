using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace gView.DataSources.TileCache.UI
{
    public partial class FormLocalCacheProperties : Form
    {
        public FormLocalCacheProperties()
        {
            InitializeComponent();

            chkUseLocalCaching.Checked = LocalCachingSettings.UseLocalCaching;
            txtFolder.Text = LocalCachingSettings.LocalCachingFolder;
        }

        #region GUI

        public void FillList()
        {
            lstCaches.Items.Clear();
            
            DirectoryInfo di = new DirectoryInfo(LocalCachingSettings.LocalCachingFolder);
            if (di.Exists)
            {
                foreach (DirectoryInfo sub in di.GetDirectories())
                {
                    double size;
                    int count = CountFiles(sub.FullName, out size);

                    lstCaches.Items.Add(new ListViewItem(new string[] { sub.Name, 
                        (count>=1000 ? ">" : "")+count.ToString(),
                        (count>=1000 ? ">" : "")+Math.Round(size, 2).ToString() }));
                }
            }
        }

        #endregion

        #region Events

        private void FormLocalCacheProperties_Load(object sender, EventArgs e)
        {
            FillList();
        }

        private void btnGetFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.SelectedPath = txtFolder.Text;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtFolder.Text = dlg.SelectedPath;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            LocalCachingSettings.UseLocalCaching = chkUseLocalCaching.Checked;
            LocalCachingSettings.LocalCachingFolder = txtFolder.Text;

            LocalCachingSettings.Commit();
        }

        private void lstCaches_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnEraseCache.Enabled = lstCaches.SelectedItems.Count == 1;
        }

        private void btnEraseCache_Click(object sender, EventArgs e)
        {
            if (lstCaches.SelectedItems.Count == 1)
            {
                ListViewItem item = lstCaches.SelectedItems[0];

                try
                {
                    DirectoryInfo di = new DirectoryInfo(LocalCachingSettings.LocalCachingFolder + @"\" + item.Text);
                    di.Delete(true);
                    FillList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Helper

        public int CountFiles(string directory, out double size)
        {
            size = 0D;
            int counter = 0;
            foreach (string filename in System.IO.Directory.GetFiles(directory, "*.*", System.IO.SearchOption.AllDirectories))
            {
                FileInfo fi = new FileInfo(filename);
                size += (double)fi.Length / 1024D / 1024D;
                counter++;
                if (counter > 1000)
                    break;
            }

            return counter;
        }

        #endregion
    }
}
