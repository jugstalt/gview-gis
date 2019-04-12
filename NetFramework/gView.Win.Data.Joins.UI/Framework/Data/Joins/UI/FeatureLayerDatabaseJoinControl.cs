using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Db;
using gView.Framework.Db.UI;
using gView.Framework.UI;

namespace gView.Framework.Data.Joins.UI
{
    public partial class FeatureLayerDatabaseJoinControl : UserControl, IJoinPropertyPanel, IPage
    {
        private FeatureLayerDatabaseJoin _join;

        public FeatureLayerDatabaseJoinControl()
        {
            InitializeComponent();
        }

        #region Properties

        public DbConnectionString JoinDbConnectionString
        {
            get
            {
                DbConnectionString dbConnStr = new DbConnectionString();
                dbConnStr.FromString(txtConnectionString.Text);
                dbConnStr.UseProviderInConnectionString = true;
                return dbConnStr;
            }
            set
            {
                if (value != null)
                    txtConnectionString.Text = value.ToString();
            }
        }

        public string JoinTableName
        {
            get
            {
                return cmbTable.Text;
            }
            set { cmbTable.Text = value; }
        }

        public string JoinTableField
        {
            get { return cmbJoinTableField.Text; }
            set { cmbJoinTableField.Text = value; }
        }

        public IFields JoinTableFields
        {
            get { return _fields; }
            set
            {
                _fields = value;
                FillJoinTableFieldsList();
            }
        }

        #endregion

        #region Events

        private void getConnectionString_Click(object sender, EventArgs e)
        {
            DbConnectionString dbConnStr = new DbConnectionString();
            dbConnStr.FromString(txtConnectionString.Text);

            FormConnectionString dlg = new FormConnectionString(dbConnStr);
            dlg.UseProviderInConnectionString = true;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                dbConnStr = dlg.DbConnectionString;
                txtConnectionString.Text = dbConnStr.ToString();
            }
        }

        private void cmbTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbJoinTableField.Text = String.Empty;
            FillJoinTableFieldsList();
        }

        private void cmbTable_DropDown(object sender, EventArgs e)
        {
            cmbTable.Items.Clear();

            string[] tabNames = TableNames();
            if (tabNames != null)
            {
                foreach (string tabName in tabNames)
                {
                    cmbTable.Items.Add(tabName);
                }
            }
        }

        private void cmbJoinTableField_DropDown(object sender, EventArgs e)
        {
            cmbJoinTableField.Items.Clear();

            Fields fields = FilterFields(FieldType.unknown);
            if (fields != null)
            {
                foreach (IField field in fields.ToEnumerable())
                {
                    cmbJoinTableField.Items.Add(field.name);
                }
            }
        }

        #endregion

        #region Gui

        IFields _fields = null;
        private void FillJoinTableFieldsList()
        {
            lstJoinTableFields.Items.Clear();

            if (String.IsNullOrEmpty(cmbTable.Text))
                return;

            _fields = FilterFields(FieldType.unknown);
            if (_fields != null)
            {
                foreach (IField field in _fields.ToEnumerable())
                {
                    lstJoinTableFields.Items.Add(field);
                }
            }
        }

        #endregion   

        #region Helper
        public Fields FilterFields(FieldType fType)
        {
            try
            {
                DbConnectionString dbConnStr = this.JoinDbConnectionString;
                if (dbConnStr == null) return null;
                dbConnStr.UseProviderInConnectionString = true;

                DataProvider provider = new DataProvider();
                provider.Open(dbConnStr.ConnectionString);
                string tablename = provider.ToTableName(this.JoinTableName);
                provider.Close();

                CommonDbConnection conn = new CommonDbConnection();
                conn.ConnectionString2 = dbConnStr.ConnectionString;

                if (!conn.GetSchema(tablename))
                {
                    {
                        MessageBox.Show(conn.errorMessage, "Error");
                        return null;
                    }
                }
                Fields fields = new Fields(conn.schemaTable);
                Fields f = new Fields();

                foreach (IField field in fields.ToEnumerable())
                {
                    if (field.type == fType || fType == FieldType.unknown)
                    {
                        f.Add(field);
                    }
                }
                return f;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception");
                return null;
            }
        }
        public string[] TableNames()
        {
            try
            {
                DbConnectionString dbConnStr = this.JoinDbConnectionString;
                if (dbConnStr == null) return null;

                CommonDbConnection conn = new CommonDbConnection();
                conn.ConnectionString2 = dbConnStr.ConnectionString;

                List<string> tables = new List<string>(conn.TableNames());
                tables.Sort();
                return tables.ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception");
                return null;
            }
        }
        #endregion 
    
        #region IJoinPropertyPanel Member

        public object PropertyPanel(IFeatureLayerJoin join, Framework.UI.IMapDocument mapDocument)
        {
            if (join is FeatureLayerDatabaseJoin)
            {
                _join = (FeatureLayerDatabaseJoin)join;

                if (!String.IsNullOrEmpty(_join.JoinConnectionString))
                {
                    gView.Framework.Db.DbConnectionString connStr = new Framework.Db.DbConnectionString();
                    connStr.UseProviderInConnectionString = true;
                    connStr.FromString(_join.JoinConnectionString);
                    this.JoinDbConnectionString = connStr;
                }
                this.JoinTableName = _join.JoinTable;
                this.JoinTableField = _join.JoinField;
                this.JoinTableFields = _join.JoinFields;
            }
            return this;
        }

        #endregion

        #region IPage Member

        public void Commit()
        {
            if (_join != null)
            {
                _join.JoinConnectionString = this.JoinDbConnectionString.ToString();
                _join.JoinTable = this.JoinTableName;
                _join.JoinField = this.JoinTableField;
                _join.JoinFields = this.JoinTableFields;
            }
        }

        #endregion

        
    }
}
