using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gView.Framework.Carto.Rendering.UI
{
    internal partial class PropertyForm_ValueMapRenderer_Dialog_InsertValue : Form
    {
        public PropertyForm_ValueMapRenderer_Dialog_InsertValue()
        {
            InitializeComponent();
        }

        private void txtKey_TextChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = (txtKey.Text.Length > 0);
        }

        public string Key
        {
            get { return txtKey.Text; }
        }
        public string Label
        {
            get { return txtLabel.Text; }
        }
    }
}