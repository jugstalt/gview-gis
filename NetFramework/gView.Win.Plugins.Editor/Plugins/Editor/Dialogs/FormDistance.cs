using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gView.Plugins.Editor.Dialogs
{
    public partial class FormDistance : Form
    {
        public FormDistance()
        {
            InitializeComponent();
        }

        public double Distance
        {
            get
            {
                return (double)numDistance.Value;
            }
            set
            {
                numDistance.Value = (decimal)value;
            }
        }
    }
}