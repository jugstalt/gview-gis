using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.system;
using gView.Framework.Data.Fields.FieldDomains;

namespace gView.Framework.Data.Fields.UI.FieldDomains
{
    public partial class Control_RangeDomain : UserControl, IInitializeClass
    {
        private RangeDomain _domain = null;
        public Control_RangeDomain()
        {
            InitializeComponent();
        }

        #region IInitializeClass Member

        public void Initialize(object parameter)
        {
            numMinValue.Minimum = numMinValue.Minimum = int.MinValue;
            numMinValue.Maximum = numMaxValue.Maximum = int.MaxValue;

            _domain = parameter as RangeDomain;

            if (_domain != null)
            {
                numMinValue.Value = (decimal)Math.Max(_domain.MinValue,(double)numMinValue.Minimum);
                numMaxValue.Value = (decimal)Math.Min(_domain.MaxValue,(double)numMaxValue.Maximum);
            }
        }

        #endregion

        private void Control_RangeDomain_Load(object sender, EventArgs e)
        {
            this.Visible = (_domain != null);
        }

        private void numMinValue_ValueChanged(object sender, EventArgs e)
        {
            if (_domain != null)
                _domain.MinValue = (double)numMinValue.Value;
        }

        private void numMaxValue_ValueChanged(object sender, EventArgs e)
        {
            if (_domain != null)
                _domain.MaxValue = (double)numMaxValue.Value;
        }
    }
}
