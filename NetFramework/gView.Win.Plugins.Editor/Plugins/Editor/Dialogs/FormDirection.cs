using System;
using System.Windows.Forms;

namespace gView.Plugins.Editor.Dialogs
{
    public partial class FormDirection : Form
    {
        public FormDirection()
        {
            InitializeComponent();
        }

        public double Direction
        {
            get
            {
                return (double)numDirection.Value * Math.PI / 180.0;
            }
            set
            {
                double v = value * 180.0 / Math.PI;
                while (v < -360.0)
                {
                    v += 360.0;
                }

                while (v > 360.0)
                {
                    v -= 360.0;
                }

                numDirection.Value = (decimal)v;
            }
        }
    }
}