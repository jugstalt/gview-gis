using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace gView.Framework.Symbology.UI.Controls
{
    public partial class OfficeLineWidthPicker : UserControl
    {
        #region Static Methods
        /// <summary>
        /// The preferred height to span the control to
        /// </summary>
        public int PreferredHeight = 22 * 15;
        /// <summary>
        /// The preferred width to span the control to
        /// </summary>
        public int PreferredWidth = 146;

        private bool _clicked = false;
        #endregion

        #region Events
        public event EventHandler PenWidthSelected = null;
        #endregion

        #region Ctor
        public OfficeLineWidthPicker()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
        }

        private IWindowsFormsEditorService _wfes = null;
        public OfficeLineWidthPicker(IWindowsFormsEditorService wfes)
            : this()
        {
            _wfes = wfes;
        }
        #endregion

        #region Properties
        private int _selectedIndex = -1;
        private int _maxWidth = 15;
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

        public int PenWidth
        {
            get { return _selectedIndex; }
        }
        #endregion

        #region Overrides
        protected override void OnPaint(PaintEventArgs e)
        {
            using (Brush selectedBrush = new SolidBrush(CustomColors.ButtonHoverDark))
            using (Pen selectedPen = new Pen(CustomColors.SelectedBorder))
            using (Pen pen = new Pen(Color.Black, 1))
            {
                int x = 0, y = 0;
                for (int i = 0; i < _maxWidth; i++)
                {
                    pen.Width = i+1;
                    
                    if (_selectedIndex == i)
                    {
                        Rectangle buttonRec = new Rectangle(x, y, PreferredWidth, 22);

                        e.Graphics.FillRectangle(selectedBrush, buttonRec);
                        e.Graphics.DrawRectangle(selectedPen, buttonRec);
                    }
                    e.Graphics.DrawLine(pen, 3, y + 11, PreferredWidth - 6, y + 11);
                    y += 22;
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
            _clicked = true;
            if (_selectedIndex != -1)
            {
                if (PenWidthSelected != null) PenWidthSelected(this, EventArgs.Empty);
            }
            if (_contextForm != null)
                _contextForm.Hide();
            _contextForm = null;

            if (_wfes != null) _wfes.CloseDropDown();
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            if (!_clicked)
            {
                _selectedIndex = -1;
                this.Refresh();
            }
        }
        #endregion
    }
}
