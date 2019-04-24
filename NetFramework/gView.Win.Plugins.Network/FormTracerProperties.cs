using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gView.Plugins.Network
{
    public partial class FormTracerProperties : Form
    {
        public FormTracerProperties(object propertyObject)
        {
            InitializeComponent();

            propertyGrid1.SelectedObject = propertyObject;
        }
    }
}
