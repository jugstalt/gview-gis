using System;
using System.Data.Common;

namespace gView.DataSources.MSSqlSpatial.DataSources.Sde.Repo
{
    public class SdeColumn
    {
        public SdeColumn() { }

        public SdeColumn(DbDataReader reader)
        {
            //not supported in SDE 11.x
            //this.DatabaseName = reader["database_name"]?.ToString();

            this.TableName = reader["table_name"]?.ToString();
            this.Owner = reader["owner"]?.ToString();

            this.ColumnName = reader["column_name"]?.ToString();
            this.SdeType = Convert.ToInt32(reader["sde_type"]);

            this.Size = reader["column_size"] != DBNull.Value && reader["column_size"] != null ? (int?)Convert.ToInt32(reader["column_size"]) : null;

            this.Flags = Convert.ToInt32(reader["object_flags"]);
        }

        //public string DatabaseName { get; set; }
        public string TableName { get; set; }
        public string Owner { get; set; }

        public string ColumnName { get; set; }

        public int SdeType { get; set; }

        public int? Size { get; set; }

        public int Flags { get; set; }
    }
}
