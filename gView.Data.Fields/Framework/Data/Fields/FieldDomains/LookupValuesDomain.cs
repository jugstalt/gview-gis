using gView.Framework.Core.Data;
using gView.Framework.Core.IO;
using gView.Framework.Core.system;
using gView.Framework.Core.UI;
using gView.Framework.Db;
using gView.Framework.system;
using System;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;

namespace gView.Framework.Data.Fields.FieldDomains
{

    [RegisterPlugIn("5EE5B5EC-35CC-4d62-8152-6C101D282118")]
    public class LookupValuesDomain : Cloner, IValuesFieldDomain, IPropertyPage
    {
        private DbConnectionString _dbConnString = null;
        private string _sql = String.Empty, _connString = String.Empty;

        public DbConnectionString DbConnectionString
        {
            get { return _dbConnString; }
            set
            {
                _dbConnString = value;
                _connString = (_connString != null) ?
                    _dbConnString.ConnectionString :
                    String.Empty;
            }
        }
        public string SqlStatement
        {
            get { return _sql; }
            set { _sql = value; }
        }

        #region IValuesFieldDomain Member

        async public Task<object[]> ValuesAsync()
        {
            if (String.IsNullOrEmpty(_connString))
            {
                return null;
            }

            try
            {
                using (CommonDbConnection connection = new CommonDbConnection())
                {
                    connection.ConnectionString2 = _connString;

                    DataSet ds = new DataSet();
                    if (!await connection.SQLQuery(ds, _sql, "QUERY"))
                    {
                        return null;
                    }
                    DataTable tab = ds.Tables["QUERY"];

                    object[] values = new object[tab.Rows.Count];
                    int i = 0;
                    foreach (DataRow row in tab.Rows)
                    {
                        values[i++] = row[0];
                    }

                    return values;
                }
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region IFieldDomain Member

        public string Name
        {
            get { return "Lookup Values"; }
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            this.DbConnectionString = (gView.Framework.Db.DbConnectionString)stream.Load("DbConnectionString", null, new DbConnectionString());
            this.SqlStatement = (string)stream.Load("SqlStatement", String.Empty);
        }

        public void Save(IPersistStream stream)
        {
            if (_dbConnString != null)
            {
                stream.Save("DbConnectionString", _dbConnString);
            }

            if (_sql != null)
            {
                stream.Save("SqlStatement", _sql);
            }
        }

        #endregion

        #region IPropertyPage Member

        public object PropertyPage(object initObject)
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"/gView.Win.Data.Fields.UI.dll");

            IInitializeClass p = uiAssembly.CreateInstance("gView.Framework.Data.Fields.UI.FieldDomains.Control_LookupValuesDomain") as IInitializeClass;
            if (p != null)
            {
                p.Initialize(this);
            }
            return p;
        }

        public object PropertyPageObject()
        {
            return this;
        }

        #endregion
    }
}
