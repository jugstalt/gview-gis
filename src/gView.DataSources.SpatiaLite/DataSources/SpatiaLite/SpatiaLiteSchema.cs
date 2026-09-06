using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using System;
using System.Data.SQLite;
using System.Globalization;

namespace gView.DataSources.SpatiaLite
{
    /// <summary>
    /// The spatial SQL of <see cref="SpatiaLiteDataset"/> factored into small, table/column-agnostic
    /// building blocks so it can also be used by the SQLite FDB provider (which owns its own
    /// <c>FC_&lt;name&gt;</c> tables, connections and catalog). Every method that runs SQL expects an
    /// already-open <see cref="SQLiteConnection"/> with <c>mod_spatialite</c> loaded
    /// (<see cref="SpatiaLiteNative.LoadInto"/>) - and, for GeoPackage, amphibious mode enabled.
    /// </summary>
    internal static class SpatiaLiteSchema
    {
        private static readonly IFormatProvider _inv = CultureInfo.InvariantCulture;

        public static SpatiaLiteFlavor FlavorFor(GeometryStorageType storage)
            => storage == GeometryStorageType.GeoPackage ? SpatiaLiteFlavor.GeoPackage : SpatiaLiteFlavor.SpatiaLite;

        /// <summary>OGC geometry type name for the declared (multi-) column type.</summary>
        public static string GeometryTypeName(GeometryType type) => type switch
        {
            GeometryType.Point => "POINT",
            GeometryType.Multipoint => "MULTIPOINT",
            GeometryType.Polyline => "MULTILINESTRING",
            GeometryType.Polygon => "MULTIPOLYGON",
            _ => "GEOMETRY",
        };

        private static string Escape(string s) => (s ?? String.Empty).Replace("'", "''");
        private static string Quote(string ident) => "\"" + (ident ?? String.Empty).Replace("\"", "\"\"") + "\"";

        // ------------------------------------------------------------------ base tables

        /// <summary>
        /// One-time per file: create the SpatiaLite <c>spatial_ref_sys</c>/<c>geometry_columns</c>
        /// metadata (<c>InitSpatialMetaData</c>) or the GeoPackage <c>gpkg_*</c> tables
        /// (<c>gpkgCreateBaseTables</c>). Both are guarded so a re-run is harmless.
        /// </summary>
        public static void EnsureBaseTables(SQLiteConnection connection, SpatiaLiteFlavor flavor)
        {
            using var cmd = connection.CreateCommand();

            if (flavor == SpatiaLiteFlavor.GeoPackage)
            {
                if (!TableExists(connection, "gpkg_contents"))
                {
                    cmd.CommandText = "SELECT gpkgCreateBaseTables()";
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                if (!TableExists(connection, "geometry_columns"))
                {
                    cmd.CommandText = "SELECT InitSpatialMetaData(1)";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ------------------------------------------------------------------ geometry column

        /// <summary>
        /// Registers <paramref name="geomColumn"/> of the already-created (still empty) table
        /// <paramref name="table"/> as a geometry column. The column itself is created here
        /// (<c>AddGeometryColumn</c> / <c>gpkgAddGeometryColumn</c>), so it must NOT be part of the
        /// <c>CREATE TABLE</c> statement.
        /// </summary>
        public static void AddGeometryColumn(
            SQLiteConnection connection, SpatiaLiteFlavor flavor,
            string table, string geomColumn, GeometryType geometryType, int srid)
        {
            string typeName = GeometryTypeName(geometryType);
            using var cmd = connection.CreateCommand();

            if (flavor == SpatiaLiteFlavor.GeoPackage)
            {
                if (srid > 0 && !SridExists(connection, "gpkg_spatial_ref_sys", "srs_id", srid))
                {
                    TryExecute(cmd, $"SELECT gpkgInsertEpsgSRID({srid})");
                }

                cmd.CommandText =
                    "INSERT INTO gpkg_contents (table_name, data_type, identifier, srs_id) " +
                    $"VALUES ('{Escape(table)}', 'features', '{Escape(table)}', {srid})";
                cmd.ExecuteNonQuery();

                cmd.CommandText = $"SELECT gpkgAddGeometryColumn('{Escape(table)}', '{Escape(geomColumn)}', '{typeName}', 0, 0, {srid})";
                cmd.ExecuteNonQuery();

                cmd.CommandText = $"SELECT gpkgAddGeometryTriggers('{Escape(table)}', '{Escape(geomColumn)}')";
                cmd.ExecuteNonQuery();
            }
            else
            {
                if (srid > 0 && !SridExists(connection, "spatial_ref_sys", "srid", srid))
                {
                    TryExecute(cmd, $"SELECT InsertEpsgSrid({srid})");
                }

                cmd.CommandText = $"SELECT AddGeometryColumn('{Escape(table)}', '{Escape(geomColumn)}', {srid}, '{typeName}', 'XY')";
                cmd.ExecuteNonQuery();
            }
        }

        // ------------------------------------------------------------------ spatial index

        /// <summary>(Re)creates the R-Tree spatial index for the geometry column.</summary>
        public static void AddSpatialIndex(
            SQLiteConnection connection, SpatiaLiteFlavor flavor, string table, string geomColumn)
        {
            DropSpatialIndex(connection, flavor, table, geomColumn);

            using var cmd = connection.CreateCommand();
            cmd.CommandText = flavor == SpatiaLiteFlavor.GeoPackage
                ? $"SELECT gpkgAddSpatialIndex('{Escape(table)}', '{Escape(geomColumn)}')"
                : $"SELECT CreateSpatialIndex('{Escape(table)}', '{Escape(geomColumn)}')";
            cmd.ExecuteNonQuery();
        }

        public static void DropSpatialIndex(
            SQLiteConnection connection, SpatiaLiteFlavor flavor, string table, string geomColumn)
        {
            using var cmd = connection.CreateCommand();

            if (flavor == SpatiaLiteFlavor.GeoPackage)
            {
                foreach (var suffix in new[] { "insert", "update1", "update2", "update3", "update4", "update5", "update6", "update7", "delete" })
                {
                    TryExecute(cmd, $"DROP TRIGGER IF EXISTS \"rtree_{table}_{geomColumn}_{suffix}\"");
                }
                TryExecute(cmd, $"DROP TABLE IF EXISTS \"rtree_{table}_{geomColumn}\"");
                TryExecute(cmd, $"DELETE FROM gpkg_extensions WHERE lower(table_name)=lower('{Escape(table)}') AND lower(column_name)=lower('{Escape(geomColumn)}') AND extension_name='gpkg_rtree_index'");
            }
            else
            {
                TryExecute(cmd, $"SELECT DisableSpatialIndex('{Escape(table)}', '{Escape(geomColumn)}')");
                TryExecute(cmd, $"DROP TABLE IF EXISTS \"idx_{table}_{geomColumn}\"");
            }
        }

        /// <summary>Removes the geometry-column registration (used when a feature class is dropped).</summary>
        public static void DiscardGeometryColumn(
            SQLiteConnection connection, SpatiaLiteFlavor flavor, string table, string geomColumn)
        {
            using var cmd = connection.CreateCommand();

            if (flavor == SpatiaLiteFlavor.GeoPackage)
            {
                TryExecute(cmd, $"DELETE FROM gpkg_geometry_columns WHERE lower(table_name)=lower('{Escape(table)}')");
                TryExecute(cmd, $"DELETE FROM gpkg_contents WHERE lower(table_name)=lower('{Escape(table)}')");
            }
            else
            {
                TryExecute(cmd, $"SELECT DiscardGeometryColumn('{Escape(table)}', '{Escape(geomColumn)}')");
            }
        }

        // ------------------------------------------------------------------ SQL fragments

        /// <summary>
        /// Column expression that yields a geometry the <c>ST_*</c> functions understand. GeoPackage
        /// GPB blobs are wrapped in <c>CastAutomagic()</c> (even in amphibious mode); SpatiaLite uses
        /// the plain column.
        /// </summary>
        public static string GeometryReadExpression(SpatiaLiteFlavor flavor, string quotedColumn)
            => flavor == SpatiaLiteFlavor.GeoPackage ? $"CastAutomagic({quotedColumn})" : quotedColumn;

        /// <summary>
        /// INSERT/UPDATE value expression: turns the WKB bytes bound as <paramref name="paramName"/>
        /// into the strictly-typed native column blob. Mirrors
        /// <c>SpatiaLiteDataset.InsertShapeParameterExpression</c>.
        /// </summary>
        public static string ShapeInsertExpression(
            SpatiaLiteFlavor flavor, string paramName, GeometryType geometryType, int srid)
        {
            string expr = $"GeomFromWKB({paramName}, {srid})";

            if (geometryType == GeometryType.Polygon)
            {
                expr = $"ST_MakeValid({expr})";
            }

            expr = GeometryTypeName(geometryType) switch
            {
                "MULTIPOLYGON" => $"CastToMultiPolygon({expr})",
                "MULTILINESTRING" => $"CastToMultiLineString({expr})",
                "MULTIPOINT" => $"CastToMultiPoint({expr})",
                _ => expr,
            };

            if (flavor == SpatiaLiteFlavor.GeoPackage)
            {
                expr = $"AsGPB({expr})";
            }

            return expr;
        }

        /// <summary>
        /// R-Tree MBR pre-filter predicate: <c>&lt;idColumn&gt; IN (SELECT ... WHERE &lt;mbr overlap&gt;)</c>.
        /// <paramref name="idColumn"/> is the <c>INTEGER PRIMARY KEY</c> (rowid alias) the R-Tree is keyed on.
        /// </summary>
        public static string SpatialIndexPredicate(
            SpatiaLiteFlavor flavor, string table, string geomColumn, string idColumn,
            double minx, double miny, double maxx, double maxy, int srid)
        {
            string id = Quote(idColumn);
            string x0 = minx.ToString(_inv), y0 = miny.ToString(_inv), x1 = maxx.ToString(_inv), y1 = maxy.ToString(_inv);

            if (flavor == SpatiaLiteFlavor.GeoPackage)
            {
                string rt = Quote($"rtree_{table}_{geomColumn}");
                return $"{id} IN (SELECT id FROM {rt} WHERE minx <= {x1} AND maxx >= {x0} AND miny <= {y1} AND maxy >= {y0})";
            }
            else
            {
                string idx = Quote($"idx_{table}_{geomColumn}");
                return $"{id} IN (SELECT pkid FROM {idx} WHERE xmin <= {x1} AND xmax >= {x0} AND ymin <= {y1} AND ymax >= {y0})";
            }
        }

        /// <summary>Precise-relation predicate (used on top of the MBR pre-filter).</summary>
        public static string IntersectsPredicate(SpatiaLiteFlavor flavor, string quotedColumn, string wkt, int srid)
            => $"ST_Intersects({GeometryReadExpression(flavor, quotedColumn)}, GeomFromText('{Escape(wkt)}', {srid})) = 1";

        // ------------------------------------------------------------------ helpers

        public static bool TableExists(SQLiteConnection connection, string name)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT count(*) FROM sqlite_master WHERE type IN ('table','view') AND lower(name)=lower('{Escape(name)}')";
            return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
        }

        private static bool SridExists(SQLiteConnection connection, string table, string column, int srid)
        {
            try
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = $"SELECT count(*) FROM {table} WHERE {column} = {srid}";
                return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
            }
            catch { return false; }
        }

        private static void TryExecute(SQLiteCommand cmd, string sql)
        {
            try { cmd.CommandText = sql; cmd.ExecuteNonQuery(); }
            catch { /* best effort - cleanup / optional metadata */ }
        }
    }
}
