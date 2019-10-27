using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gView.MapServer.Lib.UI
{
    public partial class FormAddServiceableDataset : Form
    {
        public FormAddServiceableDataset()
        {
            InitializeComponent();
        }

        public string ServiceName
        {
            get
            {
                return txtServiceName.Text;
            }
            set
            {
                txtServiceName.Text = value;
            }
        }
    }
}