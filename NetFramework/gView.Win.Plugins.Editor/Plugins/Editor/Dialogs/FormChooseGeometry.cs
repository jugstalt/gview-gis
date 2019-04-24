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
            : this(geometryType.Point)
        {
        }
        public FormChooseGeometry(geometryType type)
        {
            InitializeComponent();

            this.GeometryType = type;
        }

        public geometryType GeometryType
        {
            get
            {
                if (chkPoint.Checked)
                    return geometryType.Point;
                if (chkLine.Checked)
                    return geometryType.Polyline;
                if (chkPolygon.Checked)
                    return geometryType.Polygon;

                return geometryType.Unknown;
            }
            set
            {
                switch (value)
                {
                    case geometryType.Point:
                    case geometryType.Multipoint:
                        chkPoint.Checked=true;
                        break;
                    case geometryType.Polyline:
                        chkLine.Checked = true;
                        break;
                    case geometryType.Polygon:
                        chkPolygon.Checked = true;
                        break;
                }
            }
        }
    }
}