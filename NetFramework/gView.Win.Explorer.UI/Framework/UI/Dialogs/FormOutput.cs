using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Explorer.UI;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormOutput : Form, IDockableToolWindow, IOutput
    {
        public FormOutput()
        {
            InitializeComponent();
        }

        #region IDockableWindow Members

        public string Name
        {
            get { return this.Text; }
            set { this.Text = value; }
        }
        private DockWindowState _dockState = DockWindowState.bottom;
        public DockWindowState DockableWindowState
        {
            get
            {
                return _dockState;
            }
            set
            {
                _dockState=value;
            }
        }

        public Image Image
        {
            get { return null; }
        }

        #endregion

        #region IOutput
        private delegate void Append2StandardOutputCallback(string text);
        public void Append2StandardOutput(string text)
        {
            if (txtOutput.InvokeRequired)
            {
                Append2StandardOutputCallback d = new Append2StandardOutputCallback(Append2StandardOutput);
                this.BeginInvoke(d,new object [] { text });
            }
            else
            {
                txtOutput.Text += text;
            }
        }
        #endregion
    }
}