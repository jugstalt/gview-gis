using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;

namespace gView.Plugins.MapTools.Dialogs
{
    internal partial class FormLegend : Form
    {
        private IMapDocument _doc;

        public FormLegend(IMapDocument mapDocument)
        {
            InitializeComponent();

            _doc = mapDocument;
            
            if (_doc.Application is IMapApplication)
            {
                this.ShowInTaskbar = false;
                this.TopLevel = true;
                this.Owner = ((IMapApplication)_doc.Application).DocumentWindow as Form;
            }
        }

        private void FormLegend_Load(object sender, EventArgs e)
        {
            RefreshLegend();
        }

        public void RefreshLegend()
        {
            if (!this.Visible) return;

            if (_doc != null && _doc.FocusMap != null && _doc.FocusMap.TOC != null)
            {
                pictureBox1.Image = _doc.FocusMap.TOC.Legend();
                pictureBox1.Width = pictureBox1.Image.Width;
                pictureBox1.Height = pictureBox1.Image.Height;
            }
        }
    }
}