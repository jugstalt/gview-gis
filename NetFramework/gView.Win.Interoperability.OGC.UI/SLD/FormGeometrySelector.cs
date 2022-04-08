using gView.Framework.Geometry;
using System.Windows.Forms;

namespace gView.Interoperability.OGC.UI.SLD
{
    public partial class FormGeometrySelector : Form
    {
        public FormGeometrySelector()
        {
            InitializeComponent();
        }

        public GeometryType GeometryType
        {
            get
            {
                if (radioPoint.Checked)
                {
                    return GeometryType.Point;
                }

                if (radioLine.Checked)
                {
                    return GeometryType.Polyline;
                }

                if (radioPolygon.Checked)
                {
                    return GeometryType.Polygon;
                }

                return GeometryType.Unknown;
            }
        }
    }
}