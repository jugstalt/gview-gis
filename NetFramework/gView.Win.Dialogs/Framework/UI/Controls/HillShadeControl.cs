using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using gView.Framework.LinAlg;
using System.Drawing.Drawing2D;

namespace gView.Framework.UI.Controls
{
    public partial class HillShadeControl : UserControl
    {
        private double _dx = 0.0, _dy = 1.0, _dz = 0.0;
        private Color _shaddow = Color.Black, _light = Color.White;

        #region ctor
        public HillShadeControl()
        {
            InitializeComponent();
        }

        public HillShadeControl(IContainer container)
        {
            container.Add(this);

            InitializeComponent();

            //btnLight.BackColor = _light;
            //btnShaddow.BackColor = _shaddow;
        }
        #endregion

        #region Properties
        public double Dx
        {
            get { return _dx; }
            set
            {
                _dx = value;
                numDx.Value = (decimal)_dx;
            }
        }
        public double Dy
        {
            get { return _dy; }
            set
            {
                _dy = value;
                numDy.Value = (decimal)_dy;
            }
        }
        public double Dz
        {
            get { return _dz; }
            set
            {
                _dz = value;
                numDz.Value = (decimal)_dz;
            }
        }
        /*
        public Color ColorLight
        {
            get { return _light; }
            set { _light = value; }
        }
        public Color ColorShaddow
        {
            get { return _shaddow; }
            set { _shaddow = value; }
        }
        */
        #endregion

        #region PanelCone Events
        private void panelCone_Paint(object sender, PaintEventArgs e)
        {
            int width = panelCone.Width;
            int height = panelCone.Height;

            int cx = width / 2;
            int cy = height / 2;

            Color color;

            double step=Math.PI / 50;
            Vector3d o1 = new Vector3d(0, 0, 1);
            Vector3d sun = new Vector3d(_dx, _dy, _dz);
            sun.Normalize();
            for (double w = 0; w < 2.0 * Math.PI; w += step)
            {
                Vector3d o2 = new Vector3d(Math.Cos(w), Math.Sin(w), 0);
                Vector3d o3 = new Vector3d(Math.Cos(w + step), Math.Sin(w + step), 0);

                Vector3d v1 = o2 - o1; v1.Normalize();
                Vector3d v2 = o3 - o1; v2.Normalize();

                Vector3d vn = v1 % v2; vn.Normalize();
                double h = Math.Min(Math.Max(0.0, sun * vn), 1.0);

                color = GradientColor(h);

                PointF[] points = new PointF[]
                {
                    new PointF((float)(cx+Math.Cos(w)*cx),(float)(cy-Math.Sin(w)*cy)),
                    new PointF((float)(cx+Math.Cos(w+step)*cx),(float)(cy-Math.Sin(w+step)*cy)),
                    new PointF(cx,cy)
                };

                using (GraphicsPath path = new GraphicsPath())
                {
                    path.StartFigure();
                    path.AddPolygon(points);
                    path.CloseFigure();

                    using (SolidBrush brush = new SolidBrush(color))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                }
            }
        }

        private bool _coneButtonDown = false;
        private void panelCone_MouseDown(object sender, MouseEventArgs e)
        {
            int width = panelCone.Width;
            int height = panelCone.Height;

            int rx = width / 2;
            int ry = height / 2;

            if (Math.Abs(_coneMx) < rx &&
                Math.Abs(_coneMy) < ry)
            {
                _coneButtonDown = true;

                double l = Math.Sqrt(_coneMx * _coneMx + _coneMy * _coneMy);
                _dx = _coneMx / l;
                _dy = _coneMy / l;
            }
        }

        private int _coneMx, _coneMy;
        private void panelCone_MouseMove(object sender, MouseEventArgs e)
        {
            int width = panelCone.Width;
            int height = panelCone.Height;

            int cx = width / 2;
            int cy = height / 2;

            _coneMx = e.X - cx;
            _coneMy = -e.Y + cy;

            if (_coneButtonDown)
            {
                double l = Math.Sqrt(_coneMx * _coneMx + _coneMy * _coneMy);
                _dx = _coneMx / l;
                _dy = _coneMy / l;

                panelCone.Refresh();
            }
        }

        private void panelCone_MouseUp(object sender, MouseEventArgs e)
        {
            _coneButtonDown = false;

            numDx.Value = (decimal)_dx;
            numDy.Value = (decimal)_dy;
        }
        #endregion

        #region PanelElevation Events
        private void panelElevation_Paint(object sender, PaintEventArgs e)
        {
            int width = panelElevation.Width;
            int height = panelElevation.Height;

            Color color;
            for (int i = 0; i < height; i++)
            {
                color = GradientColor(1.0 - (double)i / (double)height);
                using (Pen pen = new Pen(color, 1))
                {
                    e.Graphics.DrawLine(pen, 0, i, width, i);
                }
            }
        }
        private void panelElevationSlider_Paint(object sender, PaintEventArgs e)
        {
            int height = panelElevation.Height;

            int y = (int)((1.0 - _dz) * height);
            using (SolidBrush brush = new SolidBrush(Color.Yellow))
            {
                e.Graphics.FillEllipse(brush, 0, y, 10, 10);
            }
        }

        private bool _elevButtonDown = false;
        private void panelElevation_MouseDown(object sender, MouseEventArgs e)
        {
            _elevButtonDown = true;
            _dz = 1.0 - (double)_elevMy / (double)panelElevation.Height;
            _dz = Math.Max(Math.Min(1.0, _dz), 0.0);

            panelCone.Refresh();
            panelElevationSlider.Refresh();
        }

        int _elevMy;
        private void panelElevation_MouseMove(object sender, MouseEventArgs e)
        {
            int height = panelElevation.Height;
            _elevMy = e.Y;

            if (_elevButtonDown)
            {
                _dz = 1.0 - (double)e.Y / (double)height;
                _dz = Math.Max(Math.Min(1.0, _dz), 0.0);

                panelCone.Refresh();
                panelElevationSlider.Refresh();  
            }
        }

        private void panelElevation_MouseUp(object sender, MouseEventArgs e)
        {
            _elevButtonDown = false;

            numDz.Value = (decimal)_dz;
        }
        #endregion

        #region Helper
        private Color GradientColor(double h)
        {
            double r = (_light.R - _shaddow.R) * h;
            double g = (_light.G - _shaddow.G) * h;
            double b = (_light.B - _shaddow.B) * h;

            return Color.FromArgb(
                _shaddow.R + (int)r,
                _shaddow.G + (int)g,
                _shaddow.B + (int)b);
        }
        #endregion

        
        private void HillShadeControl_Load(object sender, EventArgs e)
        {
            btnLight.BackColor = _light;
            btnShaddow.BackColor = _shaddow;

            numDx.Value = (decimal)_dx;
            numDy.Value = (decimal)_dy;
            numDz.Value = (decimal)_dz;
        }

        private void btnLight_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = btnLight.BackColor;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                btnLight.BackColor = _light = dlg.Color;
                panelCone.Refresh();
                panelElevation.Refresh();
            }
        }

        private void btnShaddow_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = btnShaddow.BackColor;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                btnShaddow.BackColor = _shaddow = dlg.Color;
                panelCone.Refresh();
                panelElevation.Refresh();
            }
        }

        private void numDz_ValueChanged(object sender, EventArgs e)
        {
            _dz = (double)numDz.Value;

            panelCone.Refresh();
            panelElevationSlider.Refresh();
        }

        private void numDy_ValueChanged(object sender, EventArgs e)
        {
            _dy = (double)numDy.Value;

            panelCone.Refresh();
            panelElevationSlider.Refresh();
        }

        private void numDx_ValueChanged(object sender, EventArgs e)
        {
            _dx = (double)numDx.Value;

            panelCone.Refresh();
            panelElevationSlider.Refresh();
        }
    }
}
