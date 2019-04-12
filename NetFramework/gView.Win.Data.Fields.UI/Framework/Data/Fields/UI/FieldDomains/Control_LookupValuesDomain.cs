using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.system;
using gView.Framework.Data.Fields.FieldDomains;
using gView.Framework.Db.UI;

namespace gView.Framework.Data.Fields.UI.FieldDomains
{
    public partial class Control_LookupValuesDomain : UserControl,IInitializeClass
    {
        private LookupValuesDomain _domain = null;

        public Control_LookupValuesDomain()
        {
            InitializeComponent();
        }

        #region IInitializeClass Member

        public void Initialize(object parameter)
        {
            _domain = parameter as LookupValuesDomain;

            if (_domain != null)
            {
                if (_domain.DbConnectionString != null)
                    txtConnection.Text = _domain.DbConnectionString.SchemaName;
                txtSQL.Text = _domain.SqlStatement;
            }
        }

        #endregion

        private void btnGetConnectionString_Click(object sender, EventArgs e)
        {
            if (_domain == null) return;

            FormConnectionString dlg = new FormConnectionString(_domain.DbConnectionString);
            dlg.ProviderID = "mssql,oledb,oracle";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _domain.DbConnectionString = dlg.DbConnectionString;
                if (_domain.DbConnectionString != null)
                    txtConnection.Text = _domain.DbConnectionString.SchemaName;
                else
                    txtConnection.Text = String.Empty;
            }
        }

        private void Control_LookupValuesDomain_Load(object sender, EventArgs e)
        {
            this.Visible = (_domain != null);
        }

        private void txtSQL_TextChanged(object sender, EventArgs e)
        {
            if (_domain != null)
                _domain.SqlStatement = txtSQL.Text;
        }
    }
}
