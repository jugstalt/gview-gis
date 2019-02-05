using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.MSSqlSpatial.DataSources.Sde.Repo
{
    public class SdeSpatialReference
    {
        public SdeSpatialReference()
        {

        }

        public SdeSpatialReference(DbDataReader reader)
        {
            this.Srid = Convert.ToInt32(reader["srid"]);

            this.Description = reader["description"]?.ToString();

            this.AuthName = reader["auth_name"]?.ToString();
            this.AuthSrid = reader["auth_srid"] != DBNull.Value && reader["auth_srid"] != null ? (int?)Convert.ToInt32(reader["auth_srid"]) : null;

            this.SrText = reader["srtext"]?.ToString();
        }

        public int Srid { get; set; }
        public string Description { get; set; }

        public string AuthName { get; set; }
        public int? AuthSrid { get; set; }

        public string SrText { get; set; }
    }
}
