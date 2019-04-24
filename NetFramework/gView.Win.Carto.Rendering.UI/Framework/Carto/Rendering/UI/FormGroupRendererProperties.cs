using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gView.Framework.Carto.Rendering.UI
{
    public partial class FormGroupRendererProperties : Form
    {
        public FormGroupRendererProperties(Control panel)
        {
            InitializeComponent();

            Controls.Add(panel);
            panel.BringToFront();
        }
    }
}