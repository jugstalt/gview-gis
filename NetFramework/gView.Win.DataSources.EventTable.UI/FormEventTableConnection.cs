using gView.Framework.Data;
using gView.Framework.Db;
using gView.Framework.Db.UI;
using gView.Framework.Geometry;
using gView.Framework.UI;
using gView.Framework.UI.Dialogs;
using System;
using System.Windows.Forms;

namespace gView.DataSources.EventTable.UI
{
    public partial class FormEventTableConnection : Form, IConnectionStringDialog
    {
        public FormEventTableConnection()
        {
            InitializeComponent();
        }

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

        public DbConnectionString DbConnectionString
        {
            get
            {
                DbConnectionString dbConnStr = new DbConnectionString();
                dbConnStr.FromString(txtConnectionString.Text);
                return dbConnStr;
            }
            set
            {
                if (value != null)
                {
                    txtConnectionString.Text = value.ToString();
                }
            }
        }

        public string TableName
        {
            get
            {
                return cmbTable.Text;
            }
            set { cmbTable.Text = value; }
        }

        public string IdField
        {
            get { return cmbID.Text; }
            set { cmbID.Text = value; }
        }

        public string XField
        {
            get { return cmbX.Text; }
            set { cmbX.Text = value; }
        }

        public string YField
        {
            get { return cmbY.Text; }
            set { cmbY.Text = value; }
        }

        public ISpatialReference SpatialReference
        {
            get
            {
                if (String.IsNullOrEmpty(txtSR.Text))
                {
                    return null;
                }

                SpatialReference sr = new SpatialReference();
                sr.FromXmlString(txtSR.Text);

                return sr;
            }
            set
            {
                if (value == null)
                {
                    txtSR.Text = String.Empty;
                }
                else
                {
                    txtSR.Text = value.ToXmlString();
                }
            }
        }

        private void btnGetSR_Click(object sender, EventArgs e)
        {
            FormSpatialReference dlg = new FormSpatialReference(this.SpatialReference);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                this.SpatialReference = dlg.SpatialReference;
            }
        }

        private void cmbID_DropDown(object sender, EventArgs e)
        {
            cmbID.Items.Clear();

            Fields fields = FilterFields(FieldType.integer);
            if (fields != null)
            {
                foreach (IField field in fields.ToEnumerable())
                {
                    cmbID.Items.Add(field.name);
                }
            }
        }

        private void cmbX_DropDown(object sender, EventArgs e)
        {
            cmbX.Items.Clear();

            Fields fields = FilterFields(FieldType.Double);
            if (fields != null)
            {
                foreach (IField field in fields.ToEnumerable())
                {
                    cmbX.Items.Add(field.name);
                }
            }
        }

        private void cmbY_DropDown(object sender, EventArgs e)
        {
            cmbY.Items.Clear();

            Fields fields = FilterFields(FieldType.Double);
            if (fields != null)
            {
                foreach (IField field in fields.ToEnumerable())
                {
                    cmbY.Items.Add(field.name);
                }
            }
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

        #region Helper
        public Fields FilterFields(FieldType fType)
        {
            try
            {
                DbConnectionString dbConnStr = this.DbConnectionString;
                if (dbConnStr == null)
                {
                    return null;
                }

                CommonDbConnection conn = new CommonDbConnection();
                conn.ConnectionString2 = dbConnStr.ConnectionString;

                if (!conn.GetSchema(this.TableName))
                {
                    MessageBox.Show(conn.errorMessage, "Error");
                    return null;
                }
                Fields fields = new Fields(conn.schemaTable);
                Fields f = new Fields();

                foreach (IField field in fields.ToEnumerable())
                {
                    if (field.type == fType)
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
                DbConnectionString dbConnStr = this.DbConnectionString;
                if (dbConnStr == null)
                {
                    return null;
                }

                CommonDbConnection conn = new CommonDbConnection();
                conn.ConnectionString2 = dbConnStr.ConnectionString;

                return conn.TableNames();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception");
                return null;
            }
        }
        #endregion

        #region IConnectionStringDialog Member

        public string ShowConnectionStringDialog(string initConnectionString)
        {
            EventTableConnection conn = new EventTableConnection();
            try
            {
                conn.FromXmlString(initConnectionString);
            }
            catch { }

            this.DbConnectionString = conn.DbConnectionString;
            this.TableName = conn.TableName;
            this.IdField = conn.IdFieldName;
            this.XField = conn.XFieldName;
            this.YField = conn.YFieldName;
            this.SpatialReference = conn.SpatialReference;

            if (this.ShowDialog() == DialogResult.OK)
            {
                conn = new EventTableConnection(
                    this.DbConnectionString,
                    this.TableName,
                    this.IdField,
                    this.XField,
                    this.YField,
                    this.SpatialReference);

                return conn.ToXmlString();
            }
            return String.Empty;
        }

        #endregion
    }
}