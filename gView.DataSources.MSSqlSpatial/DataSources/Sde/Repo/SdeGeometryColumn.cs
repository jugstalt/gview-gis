using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.MSSqlSpatial.DataSources.Sde.Repo
{
    public class SdeGeometryColumn
    {
        public SdeGeometryColumn() { }

        public SdeGeometryColumn(DbDataReader reader)
        {
            this.DatabaseName = reader["f_table_catalog"]?.ToString();
            this.TableName = reader["f_table_name"]?.ToString();
            this.Owner = reader["f_table_schema"]?.ToString();

            this.GeometryColumn = reader["f_geometry_column"]?.ToString();

            this.GeometryType = reader["geometry_type"] != DBNull.Value && reader["geometry_type"] != null ? (int?)Convert.ToInt32(reader["geometry_type"]) : null;
        }

        public string DatabaseName { get; set; }
        public string TableName { get; set; }
        public string Owner { get; set; }

        public string GeometryColumn { get; set; }

        public int? GeometryType { get; set; }
    }
}
