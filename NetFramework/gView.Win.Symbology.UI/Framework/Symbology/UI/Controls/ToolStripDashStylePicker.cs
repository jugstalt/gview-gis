using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace gView.Framework.Symbology.UI.Controls
{
    public class ToolStripDashStylePicker : ToolStripDropDownButton
    {
        private Control _parentControl = null;
        private OfficeDashStylePicker _dashStylePicker;

        public event EventHandler PenDashStyleSelected = null;

        public ToolStripDashStylePicker(Control parentControl)
        {
            _parentControl = parentControl;
            _dashStylePicker = new OfficeDashStylePicker();

            _dashStylePicker.DashStyleSelected += new EventHandler(DashStylePicker_DashStyleSelected);
        }

        void DashStylePicker_DashStyleSelected(object sender, EventArgs e)
        {
            if (PenDashStyleSelected != null) PenDashStyleSelected(this, EventArgs.Empty);
        }

        public DashStyle PenDashStyle
        {
            get
            {
                return _dashStylePicker.PenDashStyle;
            }
        }
        #region Overrides
        protected override void OnClick(EventArgs e)
        {
            Point startPoint = GetOpenPoint();

            int screenW = Screen.PrimaryScreen.Bounds.Width;
            int screenH = Screen.PrimaryScreen.Bounds.Height;

            if (startPoint.X + _dashStylePicker.Width > screenW)
            {
                startPoint.X -= _dashStylePicker.Width - this.Width - 5;
            }
            if (startPoint.Y + _dashStylePicker.Height > screenH)
            {
                startPoint.Y -= this.Height + _dashStylePicker.Height + 5;
            }

            _dashStylePicker.Show(_parentControl, startPoint);
        }

        private Point GetOpenPoint()
        {
            if (this.Owner == null) return new Point(5, 5);
            int x = 0;
            foreach (ToolStripItem item in this.Parent.Items)
            {
                if (item == this) break;
                x += item.Width;
            }
            return this.Owner.PointToScreen(new Point(x, 0));
        }
        #endregion
    }
}
