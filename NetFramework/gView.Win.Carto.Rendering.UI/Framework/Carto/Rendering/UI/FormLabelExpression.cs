using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;

namespace gView.Framework.Carto.Rendering.UI
{
    internal partial class FormLabelExpression : Form
    {
        private IFeatureClass _fc;

        public FormLabelExpression(IFeatureClass fc)
        {
            InitializeComponent();

            _fc = fc;
            MakeGUI();
        }

        private void MakeGUI()
        {
            if (_fc == null) return;

            lstFields.Items.Clear();

            foreach (IField field in _fc.Fields.ToEnumerable())
            {
                if (field == null) continue;

                lstFields.Items.Add(field.name);
            }
        }

        public string Expression
        {
            get { return txtExpression.Text; }
            set { txtExpression.Text = value; }
        }

        private void lstFields_DoubleClick(object sender, EventArgs e)
        {
            if (lstFields.SelectedItems.Count != 1) return;

            string s1 = txtExpression.Text.Substring(0, txtExpression.SelectionStart);
            string s2 = txtExpression.Text.Substring(txtExpression.SelectionStart + txtExpression.SelectionLength, txtExpression.Text.Length - txtExpression.SelectionStart - txtExpression.SelectionLength);
            txtExpression.Text = s1 + "[" + lstFields.SelectedItems[0].Text + "]" + s2;
            txtExpression.SelectionStart = s1.Length + lstFields.SelectedItems[0].Text.Length + 2;
        }
    }
}