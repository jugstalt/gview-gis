using gView.Framework.Data.Fields.FieldDomains;
using gView.Framework.system;
using System;
using System.Windows.Forms;

namespace gView.Framework.Data.Fields.UI.FieldDomains
{
    public partial class Control_SimpleValueDomain : UserControl, IInitializeClass
    {
        private SimpleValuesDomain _domain;

        public Control_SimpleValueDomain()
        {
            InitializeComponent();
        }

        #region IInitializeClass Member

        async public void Initialize(object parameter)
        {
            _domain = parameter as SimpleValuesDomain;

            if (_domain != null && await _domain.ValuesAsync() != null)
            {
                foreach (object val in await _domain.ValuesAsync())
                {
                    if (val == null)
                    {
                        continue;
                    }

                    if (!String.IsNullOrEmpty(txtValues.Text))
                    {
                        txtValues.Text += "\r\n";
                    }

                    txtValues.Text += val.ToString();
                }
            }
        }

        #endregion

        private void txtValues_TextChanged(object sender, EventArgs e)
        {
            if (_domain == null)
            {
                return;
            }

            string txt = txtValues.Text.Replace("\n", "");
            _domain.SetValues(txt.Split('\r'));
        }
    }
}
