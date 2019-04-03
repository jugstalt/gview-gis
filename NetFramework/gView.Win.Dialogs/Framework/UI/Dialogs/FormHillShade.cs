using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gView.Framework.UI.Dialogs
{
    internal partial class FormHillShade : Form
    {
        public FormHillShade(double dx,double dy,double dz)
        {
            InitializeComponent();

            HillShadeControl.Dx = dx;
            HillShadeControl.Dy = dy;
            HillShadeControl.Dz = dz;
        }

        public double Dx
        {
            get { return HillShadeControl.Dx; }
        }
        public double Dy
        {
            get { return HillShadeControl.Dy; }
        }
        public double Dz
        {
            get { return HillShadeControl.Dz; }
        }
    }
}