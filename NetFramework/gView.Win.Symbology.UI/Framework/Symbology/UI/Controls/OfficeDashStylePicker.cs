using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace gView.Framework.Symbology.UI.Controls
{
    public partial class OfficeDashStylePicker : UserControl
    {
        #region Static Methods
        /// <summary>
        /// The preferred height to span the control to
        /// </summary>
        public int PreferredHeight = Enum.GetValues(typeof(DashStyle)).Length * 22;
        /// <summary>
        /// The preferred width to span the control to
        /// </summary>
        public int PreferredWidth = 146;

        #endregion

        #region Events
        public event EventHandler DashStyleSelected = null;
        #endregion

        #region Ctor
        public OfficeDashStylePicker()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
        }
        #endregion

        #region Properties
        private int _selectedIndex = -1;
        #endregion

        #region Public Methods

        private ContextMenuForm _contextForm;
        private Control _parentControl;

        public void Show(Control parent, Point startLocation)
        {
            _parentControl = parent;
            // Creates new contextmenu form, adds the control to it, display it.      
            ContextMenuForm frm = new ContextMenuForm();
            frm.SetContainingControl(this);
            frm.Height = PreferredHeight;
            _contextForm = frm;
            frm.Show(parent, startLocation, PreferredWidth);
        }

        public DashStyle PenDashStyle
        {
            get
            {
                if (_selectedIndex >= 0)
                {
                    return (DashStyle)Enum.GetValues(typeof(DashStyle)).GetValue(_selectedIndex);
                }
                return DashStyle.Solid;
            }
        }
        #endregion

        #region Overrides
        protected override void OnPaint(PaintEventArgs e)
        {
            using (Brush selectedBrush = new SolidBrush(CustomColors.ButtonHoverDark))
            using (Pen selectedPen = new Pen(CustomColors.SelectedBorder))
            using (Pen pen = new Pen(Color.Black, 3))
            {
                int x = 0, y = 0, i = 0;
                foreach (DashStyle style in Enum.GetValues(typeof(DashStyle)))
                {
                    pen.DashStyle = style;

                    if (_selectedIndex == i)
                    {
                        Rectangle buttonRec = new Rectangle(x, y, PreferredWidth, 22);

                        e.Graphics.FillRectangle(selectedBrush, buttonRec);
                        e.Graphics.DrawRectangle(selectedPen, buttonRec);
                    }
                    e.Graphics.DrawLine(pen, 3, y + 11, PreferredWidth - 6, y + 11);
                    y += 22;
                    i++;
                }
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            _selectedIndex = e.Y / 22;
            this.Refresh();
        }
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (_selectedIndex != -1)
            {
                if (DashStyleSelected != null) DashStyleSelected(this, EventArgs.Empty);
            }
            if (_contextForm != null)
                _contextForm.Hide();
            _contextForm = null;
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            _selectedIndex = -1;
            this.Refresh();
        }
        #endregion
    }
}
