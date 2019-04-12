using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using gView.DataSources.Raster.File;

namespace gView.DataSources.Raster.UI
{
    internal partial class FormRasterFileDataset : Form,gView.Framework.Data.IDatasetEnum
    {
        public FormRasterFileDataset()
        {
            InitializeComponent();
        }

        #region IDatasetEnum Members

        int _pos = 0;
        public void Reset()
        {
            _pos = 0;
        }

        public gView.Framework.Data.IDataset Next
        {
            get
            {
                if (_pos > 0) return null;

                RasterFileDataset dataset = new RasterFileDataset();
                foreach (ListViewItem item in listView1.Items)
                {
                    if (!(item is RasterFileItem)) continue;
                    dataset.AddRasterFile(((RasterFileItem)item).Filename);
                }
                _pos++;
                return dataset;
            }
        }

        #endregion

        private void toolStripAddFiles_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (string filename in openFileDialog1.FileNames)
                {
                    listView1.Items.Add(new RasterFileItem(filename));
                }
            }
        }

        private RasterFileItem _contextItem=null;
        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewItem item = listView1.GetItemAt(e.X, e.Y);
            if (item is RasterFileItem)
            {
                _contextItem = (RasterFileItem)item;
                listView1.ContextMenuStrip = contextMenuStripImage;
            }
            else
            {
                _contextItem = null;
                listView1.ContextMenuStrip = null;
            }
        }

        private void previewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_contextItem == null) return;
            FormImagePreview dlg = new FormImagePreview();
            dlg.Filename = _contextItem.Filename;
            dlg.ShowDialog();
        }
    }

    internal class RasterFileItem : ListViewItem
    {
        private string _filename="";
        public RasterFileItem(string filename)
        {
            FileInfo fi = new FileInfo(filename);

            base.Text = fi.Name;
            _filename = filename;
        }

        public string Filename
        {
            get { return _filename; }
        }
    }
}