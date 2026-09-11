using gView.DataSources.GeoPackage;
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
    /// <c>FC_&lt;name&gt;</c> tables, connections and catalog).
    /// <para>
    /// For the <see cref="SpatiaLiteFlavor.SpatiaLite"/> flavor every SQL-running method needs an
    /// open <see cref="SQLiteConnection"/> with <c>mod_spatialite</c> loaded
    /// (<see cref="SpatiaLiteNative.LoadInto"/>). The <see cref="SpatiaLiteFlavor.GeoPackage"/> flavor
    /// is <b>mod_spatialite-free</b>: it delegates to <see cref="GeoPackageSchema"/> (pure SQL) and
    /// the geometry blob is handled in managed code (<see cref="GpkgGeometry"/>).
    /// </para>
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
            if (flavor == SpatiaLiteFlavor.GeoPackage)
            {
                GeoPackageSchema.EnsureBaseTables(connection);
                return;
            }

            if (!TableExists(connection, "geometry_columns"))
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT InitSpatialMetaData(1)";
                cmd.ExecuteNonQuery();
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
            if (flavor == SpatiaLiteFlavor.GeoPackage)
            {
                GeoPackageSchema.EnsureSrs(connection, srid);
                GeoPackageSchema.RegisterFeatureTable(connection, table, geomColumn, geometryType, srid, hasZ: false, hasM: false);
                return;
            }

            using var cmd = connection.CreateCommand();
            if (srid > 0 && !SridExists(connection, "spatial_ref_sys", "srid", srid))
            {
                TryExecute(cmd, $"SELECT InsertEpsgSrid({srid})");
            }

            cmd.CommandText = $"SELECT AddGeometryColumn('{Escape(table)}', '{Escape(geomColumn)}', {srid}, '{GeometryTypeName(geometryType)}', 'XY')";
            cmd.ExecuteNonQuery();
        }

        // ------------------------------------------------------------------ spatial index

        /// <summary>(Re)creates the R-Tree spatial index for the geometry column.</summary>
        public static void AddSpatialIndex(
            SQLiteConnection connection, SpatiaLiteFlavor flavor, string table, string geomColumn)
        {
            if (flavor == SpatiaLiteFlavor.GeoPackage)
            {
                GeoPackageSchema.CreateRTree(connection, table, geomColumn);
                return;
            }

            DropSpatialIndex(connection, flavor, table, geomColumn);

            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT CreateSpatialIndex('{Escape(table)}', '{Escape(geomColumn)}')";
            cmd.ExecuteNonQuery();
        }

        public static void DropSpatialIndex(
            SQLiteConnection connection, SpatiaLiteFlavor flavor, string table, string geomColumn)
        {
            if (flavor == SpatiaLiteFlavor.GeoPackage)
            {
                GeoPackageSchema.DropRTree(connection, table, geomColumn);
                return;
            }

            using var cmd = connection.CreateCommand();
            TryExecute(cmd, $"SELECT DisableSpatialIndex('{Escape(table)}', '{Escape(geomColumn)}')");
            TryExecute(cmd, $"DROP TABLE IF EXISTS \"idx_{table}_{geomColumn}\"");
        }

        /// <summary>Removes the geometry-column registration (used when a feature class is dropped).</summary>
        public static void DiscardGeometryColumn(
            SQLiteConnection connection, SpatiaLiteFlavor flavor, string table, string geomColumn)
        {
            if (flavor == SpatiaLiteFlavor.GeoPackage)
            {
                GeoPackageSchema.Unregister(connection, table, geomColumn);
                return;
            }

            using var cmd = connection.CreateCommand();
            TryExecute(cmd, $"SELECT DiscardGeometryColumn('{Escape(table)}', '{Escape(geomColumn)}')");
        }

        // ------------------------------------------------------------------ SQL fragments

        /// <summary>
        /// Column expression that yields a geometry the SpatiaLite <c>ST_*</c> functions understand.
        /// GeoPackage is handled in managed code (<see cref="GpkgGeometry"/>), so there the raw blob
        /// column is used as-is.
        /// </summary>
        public static string GeometryReadExpression(SpatiaLiteFlavor flavor, string quotedColumn)
            => quotedColumn;

        /// <summary>
        /// INSERT/UPDATE value expression for the geometry column.
        /// <list type="bullet">
        ///   <item><b>GeoPackage</b>: the caller binds a ready GPB blob (<see cref="GpkgGeometry.ToGpb"/>) -
        ///     the expression is just the parameter.</item>
        ///   <item><b>SpatiaLite</b>: the WKB parameter is turned into the strictly-typed native blob
        ///     via <c>GeomFromWKB</c> / <c>CastToMulti*</c> (needs mod_spatialite).</item>
        /// </list>
        /// </summary>
        public static string ShapeInsertExpression(
            SpatiaLiteFlavor flavor, string paramName, GeometryType geometryType, int srid)
        {
            if (flavor == SpatiaLiteFlavor.GeoPackage)
            {
                return paramName;
            }

            string expr = $"GeomFromWKB({paramName}, {srid})";

            if (geometryType == GeometryType.Polygon)
            {
                // ST_MakeValid can return a GEOMETRYCOLLECTION (polygon parts + dangling
                // edges/points) for self-touching input or a "hole outside the shell";
                // keep only the polygon parts so the CastToMultiPolygon below never
                // collapses the whole row to NULL. The RTTOPO stderr warning it emits for
                // such input is harmless - the geometry is still repaired.
                expr = $"ST_CollectionExtract(ST_MakeValid({expr}), 3)";
            }

            expr = GeometryTypeName(geometryType) switch
            {
                "MULTIPOLYGON" => $"CastToMultiPolygon({expr})",
                "MULTILINESTRING" => $"CastToMultiLineString({expr})",
                "MULTIPOINT" => $"CastToMultiPoint({expr})",
                _ => expr,
            };

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

        /// <summary>
        /// Precise-relation SQL predicate (on top of the MBR pre-filter) - <b>SpatiaLite only</b>.
        /// GeoPackage has no in-database <c>ST_Intersects</c> without mod_spatialite; returns an empty
        /// string there and the cursor does the exact per-row check in managed code.
        /// </summary>
        public static string IntersectsPredicate(SpatiaLiteFlavor flavor, string quotedColumn, string wkt, int srid)
            => flavor == SpatiaLiteFlavor.GeoPackage
                ? String.Empty
                : $"ST_Intersects({quotedColumn}, GeomFromText('{Escape(wkt)}', {srid})) = 1";

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
