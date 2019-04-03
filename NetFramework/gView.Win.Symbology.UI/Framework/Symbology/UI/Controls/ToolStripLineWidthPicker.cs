using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace gView.Framework.Symbology.UI.Controls
{
    public class ToolStripLineWidthPicker : ToolStripDropDownButton
    {
        private Control _parentControl = null;
        private OfficeLineWidthPicker _lineWidthePicker;

        public event EventHandler PenWidthSelected = null;

        public ToolStripLineWidthPicker(Control parentControl)
        {
            _parentControl = parentControl;
            _lineWidthePicker = new OfficeLineWidthPicker();

            _lineWidthePicker.PenWidthSelected += new EventHandler(PenWidth_PenWidthSelected);
        }

        void PenWidth_PenWidthSelected(object sender, EventArgs e)
        {
            if (PenWidthSelected != null) PenWidthSelected(this, EventArgs.Empty);
        }

        public int PenWidth
        {
            get
            {
                return _lineWidthePicker.PenWidth;
            }
        }

        #region Overrides
        protected override void OnClick(EventArgs e)
        {
            Point startPoint = GetOpenPoint();

            int screenW = Screen.PrimaryScreen.Bounds.Width;
            int screenH = Screen.PrimaryScreen.Bounds.Height;

            if (startPoint.X + _lineWidthePicker.Width > screenW)
            {
                startPoint.X -= _lineWidthePicker.Width - this.Width - 5;
            }
            if (startPoint.Y + _lineWidthePicker.Height > screenH)
            {
                startPoint.Y -= this.Height + _lineWidthePicker.Height + 5;
            }

            _lineWidthePicker.Show(_parentControl, startPoint);
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
