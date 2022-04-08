using System;
using System.Drawing;
using System.Windows.Forms;

namespace gView.DataSources.Raster.UI
{
    internal partial class FormImagePreview : Form
    {
        public FormImagePreview()
        {
            InitializeComponent();
        }

        public string Filename
        {
            set
            {
                try
                {
                    Image image = Image.FromFile(value);
                    pictureBox1.Image = image;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void FormImagePreview_FormClosing(object sender, FormClosingEventArgs e)
        {
            Image image = pictureBox1.Image;
            pictureBox1.Image = null;
            if (image != null)
            {
                image.Dispose();
                GC.Collect();
            }
        }
    }
}