using gView.Framework.Core.Geometry;
using System;
using System.Data.SQLite;
using System.Globalization;

namespace gView.DataSources.GeoPackage
{
    /// <summary>
    /// Pure-SQL GeoPackage 1.2 metadata / R-Tree management - fully <c>mod_spatialite</c>-free.
    /// The geometry blob is handled in managed code (<see cref="GpkgGeometry"/>) and the caller
    /// keeps the R-Tree in sync on every insert / update / delete (no SQL triggers, so a GeoPackage
    /// database opens in plain SQLite).
    /// </summary>
    public static class GeoPackageSchema
    {
        public const long ApplicationId = 0x47504B47; // 'GPKG'
        public const long UserVersion = 10200;        // GeoPackage 1.2.1

        private static readonly IFormatProvider _inv = CultureInfo.InvariantCulture;

        private static string Esc(string s) => (s ?? String.Empty).Replace("'", "''");
        private static string Q(string ident) => "\"" + (ident ?? String.Empty).Replace("\"", "\"\"") + "\"";

        /// <summary>OGC geometry type name for the declared (multi-) column type.</summary>
        public static string GeometryTypeName(GeometryType type) => type switch
        {
            GeometryType.Point => "POINT",
            GeometryType.Multipoint => "MULTIPOINT",
            GeometryType.Polyline => "MULTILINESTRING",
            GeometryType.Polygon => "MULTIPOLYGON",
            _ => "GEOMETRY",
        };

        public static string RTreeTable(string table, string geomColumn) => $"rtree_{table}_{geomColumn}";

        public static void EnsureBaseTables(SQLiteConnection connection)
        {
            Exec(connection,
                $"PRAGMA application_id = {ApplicationId}",
                $"PRAGMA user_version = {UserVersion}",

                "CREATE TABLE IF NOT EXISTS gpkg_spatial_ref_sys (" +
                "srs_name TEXT NOT NULL, srs_id INTEGER NOT NULL PRIMARY KEY, organization TEXT NOT NULL, " +
                "organization_coordsys_id INTEGER NOT NULL, definition TEXT NOT NULL, description TEXT)",

                "CREATE TABLE IF NOT EXISTS gpkg_contents (" +
                "table_name TEXT NOT NULL PRIMARY KEY, data_type TEXT NOT NULL, identifier TEXT UNIQUE, " +
                "description TEXT DEFAULT '', last_change DATETIME NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')), " +
                "min_x DOUBLE, min_y DOUBLE, max_x DOUBLE, max_y DOUBLE, srs_id INTEGER, " +
                "CONSTRAINT fk_gc_r_srs_id FOREIGN KEY (srs_id) REFERENCES gpkg_spatial_ref_sys(srs_id))",

                "CREATE TABLE IF NOT EXISTS gpkg_geometry_columns (" +
                "table_name TEXT NOT NULL, column_name TEXT NOT NULL, geometry_type_name TEXT NOT NULL, " +
                "srs_id INTEGER NOT NULL, z TINYINT NOT NULL, m TINYINT NOT NULL, " +
                "CONSTRAINT pk_geom_cols PRIMARY KEY (table_name, column_name), " +
                "CONSTRAINT fk_gc_tn FOREIGN KEY (table_name) REFERENCES gpkg_contents(table_name), " +
                "CONSTRAINT fk_gc_srs FOREIGN KEY (srs_id) REFERENCES gpkg_spatial_ref_sys(srs_id))",

                "CREATE TABLE IF NOT EXISTS gpkg_extensions (" +
                "table_name TEXT, column_name TEXT, extension_name TEXT NOT NULL, definition TEXT NOT NULL, scope TEXT NOT NULL, " +
                "CONSTRAINT ge_tce UNIQUE (table_name, column_name, extension_name))",

                // the three SRS rows every GeoPackage must contain
                "INSERT OR IGNORE INTO gpkg_spatial_ref_sys VALUES " +
                "('Undefined cartesian SRS', -1, 'NONE', -1, 'undefined', 'undefined cartesian coordinate reference system')",
                "INSERT OR IGNORE INTO gpkg_spatial_ref_sys VALUES " +
                "('Undefined geographic SRS', 0, 'NONE', 0, 'undefined', 'undefined geographic coordinate reference system')",
                "INSERT OR IGNORE INTO gpkg_spatial_ref_sys VALUES " +
                "('WGS 84 geodetic', 4326, 'EPSG', 4326, " +
                "'GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563]],PRIMEM[\"Greenwich\",0]," +
                "UNIT[\"degree\",0.0174532925199433]]', 'longitude/latitude coordinates in decimal degrees on the WGS 84 spheroid')");
        }

        /// <summary>Adds a <c>gpkg_spatial_ref_sys</c> row for <paramref name="srid"/> (EPSG) if it is not present.</summary>
        public static void EnsureSrs(SQLiteConnection connection, int srid, string wkt = null)
        {
            if (srid <= 0)
            {
                return;
            }

            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT count(*) FROM gpkg_spatial_ref_sys WHERE srs_id = {srid}";
            if (Convert.ToInt64(cmd.ExecuteScalar()) > 0)
            {
                return;
            }

            string def = String.IsNullOrWhiteSpace(wkt) ? "undefined" : Esc(wkt);
            cmd.CommandText =
                "INSERT INTO gpkg_spatial_ref_sys (srs_name, srs_id, organization, organization_coordsys_id, definition, description) " +
                $"VALUES ('EPSG:{srid}', {srid}, 'EPSG', {srid}, '{def}', 'added by gView')";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Creates the geometry BLOB column (deferred by the FDB provider) and registers
        /// <paramref name="table"/> / <paramref name="geomColumn"/> as a GeoPackage feature table.
        /// </summary>
        public static void RegisterFeatureTable(
            SQLiteConnection connection, string table, string geomColumn,
            GeometryType geometryType, int srid, bool hasZ, bool hasM)
        {
            if (!ColumnExists(connection, table, geomColumn))
            {
                Exec(connection, $"ALTER TABLE {Q(table)} ADD COLUMN {Q(geomColumn)} BLOB");
            }

            string gType = GeometryTypeName(geometryType);

            Exec(connection,
                $"INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, srs_id) " +
                $"VALUES ('{Esc(table)}', 'features', '{Esc(table)}', {srid})",
                $"INSERT OR REPLACE INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) " +
                $"VALUES ('{Esc(table)}', '{Esc(geomColumn)}', '{gType}', {srid}, {(hasZ ? 1 : 0)}, {(hasM ? 1 : 0)})");
        }

        private static bool ColumnExists(SQLiteConnection connection, string table, string column)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT count(*) FROM pragma_table_info('{Esc(table)}') WHERE name = '{Esc(column)}'";
            return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
        }

        public static void CreateRTree(SQLiteConnection connection, string table, string geomColumn)
        {
            string rt = RTreeTable(table, geomColumn);

            Exec(connection,
                $"DROP TABLE IF EXISTS {Q(rt)}",
                $"CREATE VIRTUAL TABLE {Q(rt)} USING rtree(id, minx, maxx, miny, maxy)",
                "INSERT OR REPLACE INTO gpkg_extensions (table_name, column_name, extension_name, definition, scope) " +
                $"VALUES ('{Esc(table)}', '{Esc(geomColumn)}', 'gpkg_rtree_index', " +
                "'http://www.geopackage.org/spec/#extension_rtree', 'write-only')");
        }

        public static void DropRTree(SQLiteConnection connection, string table, string geomColumn)
        {
            Exec(connection,
                $"DROP TABLE IF EXISTS {Q(RTreeTable(table, geomColumn))}",
                $"DELETE FROM gpkg_extensions WHERE table_name = '{Esc(table)}' AND column_name = '{Esc(geomColumn)}' " +
                "AND extension_name = 'gpkg_rtree_index'");
        }

        /// <summary>Removes the feature-table registration (used when a feature class is dropped).</summary>
        public static void Unregister(SQLiteConnection connection, string table, string geomColumn)
        {
            Exec(connection,
                $"DELETE FROM gpkg_geometry_columns WHERE table_name = '{Esc(table)}'",
                $"DELETE FROM gpkg_contents WHERE table_name = '{Esc(table)}'",
                $"DELETE FROM gpkg_extensions WHERE table_name = '{Esc(table)}'");
        }

        public static void UpdateContentsExtent(SQLiteConnection connection, string table, IEnvelope env)
        {
            if (env == null)
            {
                return;
            }

            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                $"UPDATE gpkg_contents SET min_x = {env.MinX.ToString(_inv)}, min_y = {env.MinY.ToString(_inv)}, " +
                $"max_x = {env.MaxX.ToString(_inv)}, max_y = {env.MaxY.ToString(_inv)} WHERE table_name = '{Esc(table)}'";
            cmd.ExecuteNonQuery();
        }

        // ---- R-Tree row maintenance (the caller keeps it in sync; there are no SQL triggers) ----

        public static string RTreeUpsertSql(string table, string geomColumn)
            => $"INSERT OR REPLACE INTO {Q(RTreeTable(table, geomColumn))} (id, minx, maxx, miny, maxy) " +
               "VALUES (@rt_id, @rt_minx, @rt_maxx, @rt_miny, @rt_maxy)";

        public static string RTreeDeleteSql(string table, string geomColumn)
            => $"DELETE FROM {Q(RTreeTable(table, geomColumn))} WHERE id = @rt_id";

        private static void Exec(SQLiteConnection connection, params string[] statements)
        {
            using var cmd = connection.CreateCommand();
            foreach (var sql in statements)
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
