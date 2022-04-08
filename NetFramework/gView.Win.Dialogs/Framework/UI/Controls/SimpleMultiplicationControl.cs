using gView.Framework.Data.Calc;
using gView.Framework.system;
using System;
using System.Windows.Forms;

namespace gView.Framework.UI.Controls
{
    public partial class SimpleMultiplicationControl : UserControl, IOnLoadObject
    {
        SimpleMultiplication _multiplication = null;

        public SimpleMultiplicationControl()
        {
            InitializeComponent();
        }

        #region IOnLoadObject Member

        public void OnLoadObject(object parameter)
        {
            _multiplication = parameter as SimpleMultiplication;
            if (_multiplication != null)
            {
                numericUpDown1.Value = (decimal)_multiplication.Multiplicator;
            }
        }

        #endregion

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (_multiplication != null)
            {
                _multiplication.Multiplicator = (double)numericUpDown1.Value;
            }
        }
    }
}
