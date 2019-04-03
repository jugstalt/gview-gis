using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gView.Plugins.MapTools.Dialogs
{
    internal partial class FormImportRenderers : Form
    {
        public FormImportRenderers()
        {
            InitializeComponent();
        }

        public bool FeatureRenderer
        {
            get { return chkFeatureRenderer.Checked; }
        }
        public bool LabelRenderer
        {
            get { return chkLabelRenderer.Checked; }
        }
        public bool SelectionRenderer
        {
            get { return chkSelectionRenderer.Checked; }
        }
    }
}