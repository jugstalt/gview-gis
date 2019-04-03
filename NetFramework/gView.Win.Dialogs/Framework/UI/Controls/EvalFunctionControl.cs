using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.system;
using gView.Framework.Data.Calc;

namespace gView.Framework.UI.Controls
{
    public partial class EvalFunctionControl : UserControl, IOnLoadObject
    {
        private EvalFunction _function = null;

        public EvalFunctionControl()
        {
            InitializeComponent();
        }

        #region IOnLoadObject Member

        public void OnLoadObject(object parameter)
        {
            _function = parameter as EvalFunction;
            if (_function != null)
                txtFunction.Text = _function.Function;
        }

        #endregion

        private void txtFunction_TextChanged(object sender, EventArgs e)
        {
            if (_function != null)
                _function.Function = txtFunction.Text;
        }

        private void btnCheckSyntax_Click(object sender, EventArgs e)
        {
            try
            {
                double res = _function.Calculate(1.0);
                MessageBox.Show("f(1.0) = " + res.ToString(), "Syntax OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("f(1.0) = '" + ex.Message + "'", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
