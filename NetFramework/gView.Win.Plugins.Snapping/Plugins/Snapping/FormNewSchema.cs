using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gView.Plugins.Snapping
{
    internal partial class FormNewSchema : Form
    {
        public FormNewSchema(string name)
        {
            InitializeComponent();

            txtName.Text = name;
        }

        public string SnapSchemaName
        {
            get { return txtName.Text; }
            set { txtName.Text = value; }
        }
    }
}