using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.LinAlg;
using System.Drawing.Drawing2D;

namespace gView.Framework.Symbology.UI.Controls
{
    public partial class ShadeAngleSetter : UserControl
    {
        private Color _light = Color.White;
        private Color _shaddow = Color.Black;
        double _dx = 1.0, _dy = 0, _dz = 1.0;

        public event EventHandler ValueChanged = null;

        public ShadeAngleSetter()
        {
            InitializeComponent();

            numAngle.Value = (decimal)this.Angle2D;
        }

        #region Properties
        public double Dx
        {
            get { return _dx; }
            set { _dx = value; }
        }
        public double Dy
        {
            get { return _dy; }
            set { _dy = value; }
        }
        public double Dz
        {
            get { return _dz; }
            set { _dz = value; }
        }
        public double Angle2D
        {
            get
            {
                double angle = Math.Atan2(_dy, _dx);
                if (angle < 0.0) angle += 2.0 * Math.PI;

                return angle * 180.0 / Math.PI;
            }
            set
            {
                _dx = Math.Cos(value * Math.PI / 180.0);
                _dy = Math.Sin(value * Math.PI / 180.0);

                numAngle.Value = (decimal)value;
            }
        }
        #endregion

        #region EventHandlers
        private void ShadeAngleSetter_Paint(object sender, PaintEventArgs e)
        {
            int width = panelCone.Width;
            int height = panelCone.Height;

            int cx = width / 2;
            int cy = height / 2;

            Color color;

            double step = Math.PI / 50;
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
        private void ShadeAngleSetter_MouseDown(object sender, MouseEventArgs e)
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

                numAngle.ValueChanged -= new EventHandler(numAngle_ValueChanged);
                numAngle.Value = (decimal)this.Angle2D;
                numAngle.ValueChanged += new EventHandler(numAngle_ValueChanged);
            }
        }

        private int _coneMx, _coneMy;
        private void ShadeAngleSetter_MouseMove(object sender, MouseEventArgs e)
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
                numAngle.ValueChanged -= new EventHandler(numAngle_ValueChanged);
                numAngle.Value = (decimal)this.Angle2D;
                numAngle.ValueChanged += new EventHandler(numAngle_ValueChanged);

                if (ValueChanged != null)
                    ValueChanged(this, new EventArgs());
            }
        }

        private void ShadeAngleSetter_MouseUp(object sender, MouseEventArgs e)
        {
            _coneButtonDown = false;

            if (ValueChanged != null)
                ValueChanged(this, new EventArgs());
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

        private void numAngle_ValueChanged(object sender, EventArgs e)
        {
            this.Angle2D = (double)numAngle.Value;
            panelCone.Refresh();

            if (ValueChanged != null)
                ValueChanged(this, new EventArgs());
        }
    }
}
