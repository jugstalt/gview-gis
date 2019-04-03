using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using gView.Framework.Db.UI;
using gView.Framework.Db;
using System.Windows.Forms;

namespace gView.DataSources.Fdb.UI.MSSql
{
    public class SqlFdbConnectionStringDialog : IConnectionStringDialog
    {

        #region IConnectionStringDialog Member

        public string ShowConnectionStringDialog(string initConnectionString)
        {
            FormChangeDataset dlg = new FormChangeDataset("mssql", initConnectionString);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                return dlg.ConnectionString;
            }
            return String.Empty;
        }

        #endregion
    }
}
