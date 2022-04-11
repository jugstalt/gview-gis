using gView.Framework.Carto;
using System.Windows.Forms;

namespace gView.Plugins.MapTools
{
    internal class UnitMenuItem : ToolStripMenuItem
    {
        private GeoUnits _unit;
        private bool _square;

        public UnitMenuItem(GeoUnits unit, bool square)
        {
            _unit = unit;
            _square = square;

            if (!square)
            {
                this.Text = unit.ToString();
            }
            else
            {
                this.Text = "Square " + unit.ToString();
            }
        }

        public GeoUnits Unit
        {
            get { return _unit; }
        }

        public bool Square
        {
            get { return _square; }
        }
    }
}
