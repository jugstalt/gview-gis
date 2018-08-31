using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Xml;

namespace gView.Framework.Db
{
    public interface ICommonDbConnection : IDisposable
    {
        void CloseConnection();
        System.Data.Common.DbConnection OpenConnection();
        DbConnectionString DbConnectionString { set; }

        string ConnectionString { get; set; }
        DBType dbType { get; set; }
        string dbTypeString { get; set; }

        string errorMessage { get; }
        Exception lastException { get; }

        bool SQLQuery(ref DataSet ds, string sql, string table, bool writeable);
        bool Update(DataTable table);
        bool UpdateData(ref DataSet ds, string table);
        void ReleaseUpdateAdapter();

        DataTable Select(string fields, string from);
        DataTable Select(string fields, string from, string where);
        DataTable Select(string fields, string from, string where, string orderBy);
        DataTable Select(string fields, string from, string where, string orderBy, bool writeable);
        bool SQLQuery(ref DataSet ds, string sql, string table);
        bool SQLQuery(ref DataSet ds, string sql, string table, DataRow refRow);
        bool SQLQuery(string sql, ref XmlNode feature);
        bool SQLQuery(string sql, ref XmlNode feature, bool one2n);

        DbDataReader DataReader(string sql, out DbConnection connection);

        object QuerySingleField(string sql, string FieldName);

        //bool insertInto(string tab, string[] fields, string[] values);
        bool createIndex(string name, string table, string field, bool unique);
        bool createIndex(string name, string table, string field, bool unique, bool grouped);

        bool createTable(string name, string[] fields, string[] dataTypes);

        bool ExecuteNoneQuery(string sql);

        bool RenameTable(string name, string newName);
        bool dropIndex(string name);

        bool dropTable(string name);
        bool GetSchema(string name);
        DataTable GetSchema2(string name);

        bool createTable(DataTable tab, bool data);

        dataType getFieldType(string fieldname);
        int getFieldSize(string fieldname);

        string[] TableNames();
    }
}
