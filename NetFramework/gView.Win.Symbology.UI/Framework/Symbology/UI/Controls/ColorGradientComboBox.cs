using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using gView.Framework.Symbology;
using gView.Framework.Globalisation;

namespace gView.Framework.Symbology.UI.Controls
{
    public class ColorGradientComboBox : ComboBox
    {
        public event GradientSelectedEventHandler GradientSelected = null;

        public ColorGradientComboBox()
        {
            InitializeComponent();
        }

        public void InsertStandardItems()
        {
            this.Items.Clear();

            this.Items.Add(new ChooseItem());
            this.Items.Add(new UserDefItem());

            AddColorGradient(Color.Black, Color.LightGray);
            AddColorGradient(Color.DarkBlue, Color.LightSkyBlue);
            AddColorGradient(Color.DarkBlue, Color.LightPink);
            AddColorGradient(Color.Black, Color.Red);
            AddColorGradient(Color.Red, Color.Blue);
            AddColorGradient(Color.Red, Color.Yellow);
            AddColorGradient(Color.Turquoise, Color.Wheat);

            AddColorGradientInv(Color.Black, Color.LightGray);
            AddColorGradientInv(Color.DarkBlue, Color.LightSkyBlue);
            AddColorGradientInv(Color.DarkBlue, Color.LightPink);
            AddColorGradientInv(Color.Black, Color.Red);
            AddColorGradientInv(Color.Red, Color.Blue);
            AddColorGradientInv(Color.Red, Color.Yellow);
            AddColorGradientInv(Color.Turquoise, Color.Wheat);

            this.SelectedIndex = 0;
        }

        public void AddColorGradient(Color col1, Color col2)
        {
            AddColorGradient(new ColorGradient(col1, col2));
        }

        public void AddColorGradientInv(Color col1, Color col2)
        {
            AddColorGradient(new ColorGradient(col2, col1));
        }

        public void AddColorGradient(ColorGradient gradient)
        {
            this.Items.Insert(this.Items.Count - 1, gradient);
        }

        void ColorGradientComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= this.Items.Count) return;

            object item = this.Items[e.Index];
            if (item is ColorGradient && e.Bounds.Width > 1 && e.Bounds.Height > 1)
            {
                using (LinearGradientBrush brush = ((ColorGradient)item).CreateNewLinearGradientBrush(e.Bounds))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
            }
            else
            {
                e.Graphics.DrawString(item.ToString(), this.Font, Brushes.Black, e.Bounds);
            }
            if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
            {
                e.Graphics.DrawRectangle(Pens.LightBlue, e.Bounds);
            }
            else
            {
                e.Graphics.DrawRectangle(Pens.White, e.Bounds);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ColorGradientComboBox
            // 
            this.DrawMode = DrawMode.OwnerDrawFixed;
            this.DrawItem += new DrawItemEventHandler(ColorGradientComboBox_DrawItem);
            this.SelectedIndexChanged += new System.EventHandler(this.ColorGradientComboBox_SelectedIndexChanged);
            this.ResumeLayout(false);

        }

        private void ColorGradientComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.SelectedItem is ColorGradient)
            {
                if (GradientSelected != null)
                    GradientSelected(this, new GradientSelectedEventArgs((ColorGradient)this.SelectedItem));
            }
            else if (this.SelectedItem is UserDefItem)
            {
                FormColorGradient dlg = new FormColorGradient();
                dlg.ShowAngleSetter = false;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if (GradientSelected != null)
                        GradientSelected(this, new GradientSelectedEventArgs(dlg.ColorGradient));
                }
            }

            if (this.Items.Count > 0 &&
                this.Items[0] is ChooseItem &&
                !(this.SelectedItem is ChooseItem))
                this.SelectedIndex = 0;
        }

        #region ItemClasses 
        private class ChooseItem
        {
            public override string ToString()
            {
                return LocalizedResources.GetResString("String.Choose", "Choose");
            }
        }
        private class UserDefItem
        {
            public override string ToString()
            {
                return LocalizedResources.GetResString("String.UserDefined", "User Defined");
            }
        }
        #endregion
    }   

    public class GradientSelectedEventArgs : EventArgs
    {
        private ColorGradient _gradient;

        public GradientSelectedEventArgs(ColorGradient gradient)
        {
            _gradient = gradient;
        }

        public ColorGradient ColorGradient
        {
            get { return _gradient; }
        }
    }

    public delegate void GradientSelectedEventHandler(object sender,GradientSelectedEventArgs args);
}
