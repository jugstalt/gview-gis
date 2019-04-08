using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Geometry;

namespace gView.Interoperability.OGC.UI.SLD
{
    public partial class FormGeometrySelector : Form
    {
        public FormGeometrySelector()
        {
            InitializeComponent();
        }

        public geometryType GeometryType
        {
            get
            {
                if (radioPoint.Checked)
                    return geometryType.Point;
                if (radioLine.Checked)
                    return geometryType.Polyline;
                if (radioPolygon.Checked)
                    return geometryType.Polygon;

                return geometryType.Unknown;
            }
        }
    }
}