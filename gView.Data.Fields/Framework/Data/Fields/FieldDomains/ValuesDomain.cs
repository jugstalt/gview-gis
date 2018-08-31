using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.system;
using gView.Framework.UI;
using System.Reflection;
using System.Data;
using gView.Framework.Db;

namespace gView.Framework.Data.Fields.FieldDomains
{
    [gView.Framework.system.RegisterPlugIn("9EDDDC6F-74DC-41fb-84C4-124343880341")]
    public class SimpleValuesDomain : Cloner, IValuesFieldDomain, IPropertyPage
    {
        List<object> _values = new List<object>();

        #region IValuesFieldDomain Member

        public object[] Values
        {
            get { return _values.ToArray(); }
            set
            {
                _values.Clear();
                if (value == null) return;

                foreach (object o in value)
                {
                    _values.Add(o);
                }
            }
        }

        #endregion

        #region IFieldDomain Member

        virtual public string Name
        {
            get { return "Simple Values"; }
        }

        #endregion

        #region IPersistable Member

        public void Load(gView.Framework.IO.IPersistStream stream)
        {
            _values.Clear();
            object o;
            while ((o = stream.Load("Value", null)) != null)
            {
                _values.Add(o);
            }
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            foreach (object o in _values)
            {
                if (o == null) continue;
                stream.Save("Value", o);
            }
        }

        #endregion

        public object PropertyPage(object initObject)
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Data.Fields.UI.dll");

            IInitializeClass p = uiAssembly.CreateInstance("gView.Framework.Data.Fields.UI.FieldDomains.Control_SimpleValueDomain") as IInitializeClass;
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
    }

    [gView.Framework.system.RegisterPlugIn("5EE5B5EC-35CC-4d62-8152-6C101D282118")]
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

        public object[] Values
        {
            get
            {
                if(String.IsNullOrEmpty(_connString))
                    return null;

                try
                {
                    using (CommonDbConnection connection = new CommonDbConnection())
                    {
                        connection.ConnectionString2 = _connString;

                        DataSet ds = new DataSet();
                        if (!connection.SQLQuery(ref ds, _sql, "QUERY"))
                        {
                            return null;
                        } 
                        DataTable tab = ds.Tables["QUERY"];

                        object[] values = new object[tab.Rows.Count];
                        int i=0;
                        foreach (DataRow row in tab.Rows)
                            values[i++] = row[0];

                        return values;
                    }
                }
                catch
                {
                    return null;
                }
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

        public void Load(gView.Framework.IO.IPersistStream stream)
        {
            this.DbConnectionString = (gView.Framework.Db.DbConnectionString)stream.Load("DbConnectionString", null, new DbConnectionString());
            this.SqlStatement = (string)stream.Load("SqlStatement", String.Empty);
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            if (_dbConnString != null)
                stream.Save("DbConnectionString", _dbConnString);

            if (_sql != null)
                stream.Save("SqlStatement", _sql);
        }

        #endregion

        #region IPropertyPage Member

        public object PropertyPage(object initObject)
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Data.Fields.UI.dll");

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
