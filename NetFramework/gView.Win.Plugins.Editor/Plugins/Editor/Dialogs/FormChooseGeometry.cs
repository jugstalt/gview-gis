using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Geometry;

namespace gView.Plugins.Editor.Dialogs
{
    public partial class FormChooseGeometry : Form
    {
        public FormChooseGeometry()
            : this(GeometryType.Point)
        {
        }
        public FormChooseGeometry(GeometryType type)
        {
            InitializeComponent();

            this.GeometryType = type;
        }

        public GeometryType GeometryType
        {
            get
            {
                if (chkPoint.Checked)
                    return GeometryType.Point;
                if (chkLine.Checked)
                    return GeometryType.Polyline;
                if (chkPolygon.Checked)
                    return GeometryType.Polygon;

                return GeometryType.Unknown;
            }
            set
            {
                switch (value)
                {
                    case GeometryType.Point:
                    case GeometryType.Multipoint:
                        chkPoint.Checked=true;
                        break;
                    case GeometryType.Polyline:
                        chkLine.Checked = true;
                        break;
                    case GeometryType.Polygon:
                        chkPolygon.Checked = true;
                        break;
                }
            }
        }
    }
}