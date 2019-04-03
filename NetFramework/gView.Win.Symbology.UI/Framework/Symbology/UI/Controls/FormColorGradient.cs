using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Symbology;
using System.Drawing.Drawing2D;

namespace gView.Framework.Symbology.UI.Controls
{
    public partial class FormColorGradient : Form
    {
        private ColorGradient _gradient;

        public FormColorGradient()
        {
            InitializeComponent();

            btnColor1.BackColor = Color.DarkGray;
            btnColor2.BackColor = Color.Red;
        }

        private void FormColorGradient_Load(object sender, EventArgs e)
        {
            
        }

        public ColorGradient ColorGradient
        {
            get
            {
                ColorGradient gradient = new ColorGradient(
                    btnColor1.BackColor,
                    btnColor2.BackColor);
                gradient.Angle = -(float)shadeAngleSetter1.Angle2D;

                return gradient;
            }
            set
            {
                if (value == null) return;

                btnColor1.BackColor = value.Color1;
                btnColor2.BackColor = value.Color2;
                shadeAngleSetter1.Angle2D = -value.Angle;
            }
        }

        public bool ShowAngleSetter
        {
            get { return shadeAngleSetter1.Visible; }
            set { shadeAngleSetter1.Visible = value; }
        }

        private void panelPreview_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0, 0, panelPreview.Width, panelPreview.Height);

            ColorGradient gradient = this.ColorGradient;

            using (LinearGradientBrush brush = gradient.CreateNewLinearGradientBrush(rect))
            {
                e.Graphics.FillRectangle(brush, rect);
            }
        }

        private void btnColor1_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = btnColor1.BackColor;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                btnColor1.BackColor = dlg.Color;
                panelPreview.Refresh();
            }
        }

        private void btnColor2_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = btnColor2.BackColor;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                btnColor2.BackColor = dlg.Color;
                panelPreview.Refresh();
            }
        }

        private void btnTrans1_Click(object sender, EventArgs e)
        {
            FormColorTransparency dlg = new FormColorTransparency(btnColor1.BackColor);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                btnColor1.BackColor = dlg.Color;
            }
        }

        private void btnTrans2_Click(object sender, EventArgs e)
        {
            FormColorTransparency dlg = new FormColorTransparency(btnColor2.BackColor);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                btnColor2.BackColor = dlg.Color;
            }
        }

        private void shadeAngleSetter1_ValueChanged(object sender, EventArgs e)
        {
            panelPreview.Refresh();
        }
    }
}