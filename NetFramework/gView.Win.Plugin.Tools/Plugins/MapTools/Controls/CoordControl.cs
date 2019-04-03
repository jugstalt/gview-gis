using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Geometry;

namespace gView.Plugins.MapTools.Controls
{
    public partial class CoordControl : UserControl
    {
        ISpatialReference _sRef = null;

        public CoordControl()
        {
            InitializeComponent();

            panelXY.Dock = panelLonLat.Dock = DockStyle.Fill;

            for (int i = 0; i < 12; i++)
            {
                cmbDigits.Items.Add(i);
                cmbDigits2.Items.Add(i);
            }
            cmbDigits.SelectedItem = 2;
            cmbDigits2.SelectedItem = 8;

            chkThousandsSeperator.Checked = numX.ThousandsSeparator;
            cmbDMS.SelectedIndex = 0;

            numLonMin.Location = new System.Drawing.Point(numLon.Location.X + 40, numLon.Location.Y);
            numLatMin.Location = new System.Drawing.Point(numLat.Location.X + 40, numLat.Location.Y);
            numLonSec.Location = new System.Drawing.Point(numLon.Location.X + 75, numLon.Location.Y);
            numLatSec.Location = new System.Drawing.Point(numLat.Location.X + 75, numLat.Location.Y);
        }

        public void Init(IPoint point, ISpatialReference sRef)
        {
            this.Point = point;
            this.SpatialReference = sRef;
        }

        public IPoint Point
        {
            get
            {
                return new gView.Framework.Geometry.Point((double)numX.Value, (double)numY.Value);
            }
            set
            {
                if (value != null)
                {
                    numX.Value = (decimal)value.X;
                    numY.Value = (decimal)value.Y;
                    
                    if (panelLonLat.Visible)
                    {
                        SetDMS();
                    }
                }
            }
        }

        public ISpatialReference SpatialReference
        {
            get { return _sRef; }
            set
            {
                if (value != null &&
                    (_sRef==null ||
                     !value.Equals(_sRef)))
                {
                    IPoint p = GeometricTransformer.Transform2D(
                        this.Point,
                        _sRef,
                        value) as IPoint;

                    _sRef = value;
                    if (_sRef.SpatialParameters.IsGeographic)
                    {
                        panelLonLat.Visible = true;
                        panelXY.Visible = false;
                    }
                    else
                    {
                        panelXY.Visible = true;
                        panelLonLat.Visible = false;
                    }
                    if (p != null)
                        this.Point = p;
                }
            }
        }

        #region PanelXY
        private void cmbDigits_SelectedIndexChanged(object sender, EventArgs e)
        {
            numX.DecimalPlaces = numY.DecimalPlaces = (int)cmbDigits.SelectedItem;
        }

        private void chkThousandsSeperator_CheckedChanged(object sender, EventArgs e)
        {
            numX.ThousandsSeparator = numY.ThousandsSeparator = chkThousandsSeperator.Checked;
        }
        #endregion

        #region PanelLonLat
        private void cmbDigits2_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbDMS_SelectedIndexChanged(sender, e);
        }

        private void cmbDMS_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbDMS.SelectedIndex)
            {
                case 0:
                    numLon.Width = numLat.Width = 150;
                    numLat.DecimalPlaces = numLon.DecimalPlaces = (int)cmbDigits2.SelectedItem;
                    numLonMin.Visible = numLatMin.Visible = false;
                    numLonSec.Visible = numLatSec.Visible = false;
                    break;
                case 1:
                    numLon.Width = numLat.Width = 40;
                    numLat.DecimalPlaces = numLon.DecimalPlaces = 0;
                    numLonMin.Visible = numLatMin.Visible = true;
                    numLonMin.Width = numLatMin.Width = 110;
                    numLonMin.DecimalPlaces = numLatMin.DecimalPlaces = Math.Max((int)cmbDigits2.SelectedItem - 2, 0);
                    numLonSec.Visible = numLatSec.Visible = false;
                    break;
                case 2:
                    numLon.Width = numLat.Width = 40;
                    numLat.DecimalPlaces = numLon.DecimalPlaces = 0;
                    numLonMin.Visible = numLatMin.Visible = true;
                    numLonMin.Width = numLatMin.Width = 35;
                    numLonMin.DecimalPlaces = numLatMin.DecimalPlaces = 0;
                    numLonSec.Visible = numLatSec.Visible = true;
                    numLonSec.Width = numLatSec.Width = 75;
                    numLonSec.DecimalPlaces = numLatSec.DecimalPlaces = Math.Max((int)cmbDigits2.SelectedItem - 4, 0);
                    break;
            }
            SetDMS();
        }

        private void numLonLat_ValueChanged(object sender, EventArgs e)
        {
            GetFromDMS();
        }
        #endregion

        #region DMS
        private void ToDMS(decimal dec, out decimal min, out decimal sec)
        {
            decimal floor = dec - Math.Floor(dec);
            min = floor * (decimal)60;
            decimal minfloor = min - Math.Floor(min);
            sec = minfloor * (decimal)60;
        }
        private decimal FromDMS(decimal d, decimal m, decimal s)
        {
            return d + m / (decimal)60 + s / (decimal)3600;
        }
       
        private void SetDMS()
        {
            numLon.ValueChanged -= new EventHandler(numLonLat_ValueChanged);
            numLat.ValueChanged -= new EventHandler(numLonLat_ValueChanged);
            numLonMin.ValueChanged -= new EventHandler(numLonLat_ValueChanged);
            numLatMin.ValueChanged -= new EventHandler(numLonLat_ValueChanged);
            numLonSec.ValueChanged -= new EventHandler(numLonLat_ValueChanged);
            numLatSec.ValueChanged -= new EventHandler(numLonLat_ValueChanged);

            IPoint point = this.Point;

            cmbLon.SelectedIndex = (numX.Value < (decimal)0) ? 1 : 0;
            cmbLat.SelectedIndex = (numY.Value < (decimal)0) ? 1 : 0;

            decimal lonMin, latMin, lonSec, latSec;
            ToDMS(Math.Abs(numX.Value), out lonMin, out lonSec);
            ToDMS(Math.Abs(numY.Value), out latMin, out latSec);

            switch (cmbDMS.SelectedIndex)
            {
                case 0:
                    numLon.Value = Math.Abs(numX.Value);
                    numLat.Value = Math.Abs(numY.Value);
                    break;
                case 1:
                    numLon.Value = Math.Abs(Math.Floor(numX.Value));
                    numLat.Value = Math.Abs(Math.Floor(numY.Value));
                    numLonMin.Value = lonMin;
                    numLatMin.Value = latMin;
                    break;
                case 2:
                    numLon.Value = Math.Abs(Math.Floor(numX.Value));
                    numLat.Value = Math.Abs(Math.Floor(numY.Value));
                    numLonMin.Value = Math.Floor(lonMin);
                    numLatMin.Value = Math.Floor(latMin);
                    numLonSec.Value = lonSec;
                    numLatSec.Value = latSec;
                    break;
            }

            numLon.ValueChanged += new EventHandler(numLonLat_ValueChanged);
            numLat.ValueChanged += new EventHandler(numLonLat_ValueChanged);
            numLonMin.ValueChanged += new EventHandler(numLonLat_ValueChanged);
            numLatMin.ValueChanged += new EventHandler(numLonLat_ValueChanged);
            numLonSec.ValueChanged += new EventHandler(numLonLat_ValueChanged);
            numLatSec.ValueChanged += new EventHandler(numLonLat_ValueChanged);
        }
        private void GetFromDMS()
        {
            switch (cmbDMS.SelectedIndex)
            {
                case 0:
                    numX.Value = numLon.Value;
                    numY.Value = numLat.Value;
                    break;
                case 1:
                    numX.Value = FromDMS(numLon.Value, numLonMin.Value, 0);
                    numY.Value = FromDMS(numLat.Value, numLatMin.Value, 0);
                    break;
                case 2:
                    numX.Value = FromDMS(numLon.Value, numLonMin.Value, numLonSec.Value);
                    numY.Value = FromDMS(numLat.Value, numLatMin.Value, numLatSec.Value);
                    break;
            }
            if (cmbLon.SelectedIndex == 1)
                numX.Value = -numX.Value;
            if (cmbLat.SelectedIndex == 1)
                numY.Value = -numY.Value;
        }
        #endregion
    }
}
