using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace gView.Framework.Symbology.UI.Controls
{
    /// <summary>
    /// Represents  an office ComboBox control that holds Color Picker control to select color from.
    /// </summary>
    [ToolboxBitmap(typeof(ComboBoxColorPicker), "ComboBoxColorPicker"),
     DefaultEvent("SelectedColorChanged"), DefaultProperty("Color"), 
    ToolboxItem(true), Description("Displays a list of colors in a drop down menu to select color from")]
    public partial class ComboBoxColorPicker : ComboBox
    {
        #region Events
        /// <summary>
        /// Occurs when the value of the Color property changes. 
        /// </summary>
        [Category("Behavior"), Description("Occurs when the value of the Color property changes.")]
        public event EventHandler SelectedColorChanged;
        #endregion
        
        #region Properties
               
        /// <summary>
        /// Gets or sets the selected color from the OfficeColorPicker
        /// </summary>
        public Color Color
        {
            get
            {
                if (_colorPicker != null) return _colorPicker.Color;
                return Color.Empty;
            }
            set
            {
                if (_colorPicker != null) _colorPicker.Color = value;
                Refresh();
            }
        }

        #endregion

        #region Private members
        /// <summary>
        /// The OfficeColorPicker control that the combobox should hold
        /// </summary>
        OfficeColorPicker _colorPicker = new OfficeColorPicker();
        
        #endregion

        #region Ctor
        /// <summary>
        /// Initialize a new instance of the 
        /// ComboBoxColorPicker representing an office ComboBox control
        /// that holds color picker control to select color from.
        /// </summary>
        public ComboBoxColorPicker() : base()
        {
            InitializeComponent();
            // Dummy item to allow painting of the color.
            Items.Add("Color");
            SelectedIndex = 0;
            this.DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            // Event of the son (color picker) fires this control event
            _colorPicker.SelectedColorChanged += new EventHandler(HandleSelectedColorChanged);
            _colorPicker.BorderStyle = BorderStyle.None;
            // Optimized painting using this settings
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);            
        }
        /// <summary>
        /// Initialize a new instance of the 
        /// ComboBoxColorPicker representing an office ComboBox control
        /// that holds color picker control to select color from.
        /// </summary>
        /// <param name="startingColor">Starting color to the OfficeColorPicker control</param>
        public ComboBoxColorPicker(Color startingColor)
            : this()
        {
            Color = startingColor;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Fires the SelectedColorChanged event
        /// </summary>
        /// <param name="e"></param>
        public void OnSelectedColorChanged(EventArgs e)
        {
            if (SelectedColorChanged != null)
                SelectedColorChanged(this, e);
            this.Refresh();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Handles color changed - fires the SelectedColorChanged event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleSelectedColorChanged(object sender, EventArgs e)
        {
            OnSelectedColorChanged(EventArgs.Empty);
        }
        /// <summary>
        /// Opens the drop down box with the OfficeColorPicker
        /// </summary>
        private void ShowDropDown() 
        {            
            if (_colorPicker != null) 
            {
                _colorPicker.Show(this, 0, this.Height);
            }
        }
        /// <summary>
        /// Overrides, paint rectangle in the item regions instead of text
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index > -1)
            {
                // Paints the rectangle by the current color.
                e.Graphics.FillRectangle(new SolidBrush(Color), e.Bounds);
                Rectangle rec = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
                
                if ((e.State & DrawItemState.Focus) == 0)
                {
                    // Draw the border rectangle
                    using (Pen pen = new Pen(CustomColors.ButtonBorder))
                    {
                        e.Graphics.DrawRectangle(pen, rec);
                    }                    
                }
                else
                {
                    // Draw the border rectangle
                    using (Pen borderPen = new Pen(CustomColors.ButtonBorder, 3))
                    {
                        e.Graphics.DrawRectangle(borderPen, rec);
                    }
                    // Draw the dashed focus rectangle
                    Pen focusPen = new Pen(CustomColors.FocusDashedBorder, 1);
                    focusPen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
                    e.Graphics.DrawRectangle(focusPen, rec);
                    focusPen.Dispose();
                }
            }
        }
        #endregion

        #region Windows API overrides
        // This overrides help control to drop down handling to show the color picker.
        private const int WM_USER = 0x0400,
                        WM_REFLECT = WM_USER + 0x1C00,
                        WM_COMMAND = 0x0111,
                        CBN_DROPDOWN = 7;

        public static int HIWORD(int n) 
        {
                return (n >> 16) & 0xffff;
        }
        
        protected override void WndProc(ref Message m) 
        {
            if (m.Msg == ( WM_REFLECT + WM_COMMAND)) 
            {
                if (HIWORD((int)m.WParam) == CBN_DROPDOWN) 
                {  
                    ShowDropDown();
                    return;
                }
            }
            base.WndProc(ref m);
        }
        #endregion

    }
}
