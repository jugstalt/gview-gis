using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.MSSqlSpatial.DataSources.Sde.Repo
{
    public class SdeLayer
    {
        public SdeLayer() { }

        public SdeLayer(DbDataReader reader)
        {
            this.LayerId = Convert.ToInt32(reader["layer_id"]);
            this.Description = reader["description"]?.ToString();

            this.DatabaseName = reader["database_name"]?.ToString();
            this.TableName = reader["table_name"]?.ToString();
            this.Owner = reader["owner"]?.ToString();
            this.SpatialColumn = reader["spatial_column"]?.ToString();

            this.EFlags = Convert.ToInt32(reader["eflags"]);

            this.MinX = Convert.ToDouble(reader["minx"]);
            this.MinY = Convert.ToDouble(reader["miny"]);
            this.MaxX = Convert.ToDouble(reader["maxx"]);
            this.MaxY = Convert.ToDouble(reader["maxy"]);

            this.MinZ = reader["minz"] != DBNull.Value && reader["minz"] != null ? (double?)Convert.ToDouble(reader["minz"]) : null;
            this.MaxZ = reader["maxz"] != DBNull.Value && reader["maxz"] != null ? (double?)Convert.ToDouble(reader["maxz"]) : null;

            this.MinM = reader["minm"] != DBNull.Value && reader["minm"] != null ? (double?)Convert.ToDouble(reader["minm"]) : null;
            this.MaxM = reader["maxm"] != DBNull.Value && reader["maxm"] != null ? (double?)Convert.ToDouble(reader["maxm"]) : null;

            this.Srid = Convert.ToInt32(reader["srid"]);
        }

        public int LayerId { get; set; }
        public string Description { get; set; }

        public string DatabaseName { get; set; }
        public string TableName { get; set; }
        public string Owner { get; set; }

        public string SpatialColumn { get; set; }
        public int EFlags { get; set; }

        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }
        public double? MinZ { get; set; }
        public double? MaxZ { get; set; }
        public double? MinM { get; set; }
        public double? MaxM { get; set; }

        public int Srid { get; set; }
    }
}
