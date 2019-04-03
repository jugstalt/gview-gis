using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormLoadMap : Form
    {
        private Control _parent;

        public FormLoadMap(Control parent)
        {
            InitializeComponent();

            _parent = parent;
            this.Visible = false;
        }

        private delegate void SimpleCallBack();
        public void ShowIt()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SimpleCallBack(ShowIt));
            }
            else
            {
                Rectangle rect = _parent.Bounds;

                this.Location = new Point(rect.X - this.Width / 2, rect.Y - this.Height / 2);
                this.Show();
            }
        }
        public void HideIt()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SimpleCallBack(HideIt));
            }
            else
            {
                this.Visible = false;
            }
        }
    }
}