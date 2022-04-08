using gView.Framework.Sys.UI.Extensions;
using gView.GraphicsEngine;
using System;
using System.Windows.Forms;

namespace gView.Framework.Symbology.UI.Controls
{
    public partial class OfficeDashStylePicker : UserControl
    {
        #region Static Methods
        /// <summary>
        /// The preferred height to span the control to
        /// </summary>
        public int PreferredHeight = Enum.GetValues(typeof(LineDashStyle)).Length * 22;
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

        public void Show(Control parent, System.Drawing.Point startLocation)
        {
            _parentControl = parent;
            // Creates new contextmenu form, adds the control to it, display it.      
            ContextMenuForm frm = new ContextMenuForm();
            frm.SetContainingControl(this);
            frm.Height = PreferredHeight;
            _contextForm = frm;
            frm.Show(parent, startLocation, PreferredWidth);
        }

        public LineDashStyle PenDashStyle
        {
            get
            {
                if (_selectedIndex >= 0)
                {
                    return (LineDashStyle)Enum.GetValues(typeof(LineDashStyle)).GetValue(_selectedIndex);
                }
                return LineDashStyle.Solid;
            }
        }
        #endregion

        #region Overrides

        protected override void OnPaint(PaintEventArgs e)
        {
            var count = Enum.GetValues(typeof(LineDashStyle)).Length;


            using (var bitmap = Current.Engine.CreateBitmap(PreferredWidth, (count + 1) * 22))
            using (var canvas = bitmap.CreateCanvas())
            using (var selectedBrush = Current.Engine.CreateSolidBrush(CustomColors.ButtonHoverDark.ToArgbColor()))
            using (var selectedPen = Current.Engine.CreatePen(CustomColors.SelectedBorder.ToArgbColor(), 1))
            using (var pen = Current.Engine.CreatePen(ArgbColor.Black, 3))
            {
                int x = 0, y = 0, i = 0;
                foreach (LineDashStyle style in Enum.GetValues(typeof(LineDashStyle)))
                {
                    pen.DashStyle = style;

                    if (_selectedIndex == i)
                    {
                        var buttonRec = new CanvasRectangle(x, y, PreferredWidth, 22);

                        canvas.FillRectangle(selectedBrush, buttonRec);
                        canvas.DrawRectangle(selectedPen, buttonRec);
                    }
                    canvas.DrawLine(pen, 3, y + 11, PreferredWidth - 6, y + 11);
                    y += 22;
                    i++;
                }

                e.Graphics.DrawImage(bitmap.ToGdiBitmap(), 0, 0);
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
                if (DashStyleSelected != null)
                {
                    DashStyleSelected(this, EventArgs.Empty);
                }
            }
            if (_contextForm != null)
            {
                _contextForm.Hide();
            }

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
