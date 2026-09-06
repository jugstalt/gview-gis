using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Framework.OGC;
using System;
using System.IO;

namespace gView.DataSources.Fdb
{
    /// <summary>
    /// Encodes / decodes the geometry stored in an FDB feature class' <c>FDB_SHAPE</c> blob column.
    /// Only <see cref="GeometryStorageType.Classic"/> (gView proprietary blob) goes through this
    /// seam. Database-native formats (PostGIS / SQL Server <c>geometry</c>, SpatiaLite / GeoPackage
    /// blobs) are handled by the provider's SQL directly - see <see cref="Wkb"/> for the shared
    /// geometry&#8596;WKB helper they use to feed <c>GeomFromWKB</c> and decode <c>ST_AsBinary</c>.
    /// </summary>
    public interface IFdbGeometryCodec
    {
        /// <summary>Serializes <paramref name="shape"/> for the <c>FDB_SHAPE</c> blob parameter.</summary>
        byte[] Encode(IGeometry shape, IGeometryDef geometryDef);

        /// <summary>Materializes a geometry from the raw <c>FDB_SHAPE</c> blob bytes.</summary>
        IGeometry Decode(byte[] bytes, IGeometryDef geometryDef);
    }

    public static class FdbGeometryCodec
    {
        /// <summary>Legacy gView proprietary blob format (<see cref="GeometryStorageType.Classic"/>).</summary>
        public static readonly IFdbGeometryCodec Proprietary = new FdbProprietaryGeometryCodec();

        /// <summary>
        /// Standard WKB helper. Not a storage choice any more - used by the native providers to turn
        /// a geometry into WKB bytes for <c>ST_GeomFromWKB</c> / <c>GeomFromWKB</c> and to decode
        /// <c>ST_AsBinary(...)</c> results.
        /// </summary>
        public static readonly IFdbGeometryCodec Wkb = new FdbWkbGeometryCodec();

        /// <summary>Blob codec for the given storage type; <c>null</c> for every database-native format.</summary>
        public static IFdbGeometryCodec For(GeometryStorageType storageType) => storageType switch
        {
            GeometryStorageType.Classic => Proprietary,
            _ => null
        };

        /// <summary>
        /// Serializes a geometry for a given storage type: the proprietary blob for
        /// <see cref="GeometryStorageType.Classic"/>, WKB bytes for everything else (to feed
        /// <c>ST_GeomFromWKB</c> / <c>GeomFromWKB</c> / <c>geometry::STGeomFromWKB</c>).
        /// </summary>
        public static byte[] Encode(GeometryStorageType storageType, IGeometry shape, IGeometryDef geometryDef)
            => (storageType == GeometryStorageType.Classic ? Proprietary : Wkb).Encode(shape, geometryDef);

        /// <summary>
        /// Blob codec for a feature class' <c>FDB_SHAPE</c> column, resolved from its dataset's
        /// <see cref="ISpatialIndexDef.StorageType"/>. Falls back to <see cref="Proprietary"/> for
        /// anything that is not a blob-stored FDB feature class.
        /// </summary>
        public static IFdbGeometryCodec ForFeatureClass(IGeometryDef geometryDef)
        {
            if (geometryDef is IFeatureClass fc && fc.Dataset is IFDBDataset fdbDataset)
            {
                return For(fdbDataset.SpatialIndexDef?.StorageType ?? GeometryStorageType.Classic) ?? Proprietary;
            }

            return Proprietary;
        }
    }

    /// <summary>gView's proprietary geometry serialization (no header, geometry class taken from the catalog).</summary>
    internal sealed class FdbProprietaryGeometryCodec : IFdbGeometryCodec
    {
        public byte[] Encode(IGeometry shape, IGeometryDef geometryDef)
        {
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);
            shape.Serialize(w, geometryDef);
            w.Flush();
            return ms.ToArray();
        }

        public IGeometry Decode(byte[] bytes, IGeometryDef geometryDef)
        {
            IGeometry geometry = geometryDef.GeometryType switch
            {
                GeometryType.Point => new Point(),
                GeometryType.Multipoint => new MultiPoint(),
                GeometryType.Polyline => new Polyline(),
                GeometryType.Polygon => new Polygon(),
                GeometryType.Aggregate => new AggregateGeometry(),
                _ => null
            };

            if (geometry == null)
            {
                return null;
            }

            using var r = new BinaryReader(new MemoryStream(bytes, writable: false));
            geometry.Deserialize(r, geometryDef);

            return geometry;
        }
    }

    /// <summary>
    /// Standard WKB (2D) / EWKB-style Z,M in the blob column. Reuses
    /// <see cref="gView.Framework.OGC.OGC"/> which reads ISO-WKB, EWKB and curves. No SRID is
    /// written - the feature class' spatial reference is authoritative, and an SRID-less blob stays
    /// portable across tools.
    /// </summary>
    internal sealed class FdbWkbGeometryCodec : IFdbGeometryCodec
    {
        public byte[] Encode(IGeometry shape, IGeometryDef geometryDef)
            => OGC.GeometryToWKB(
                shape,
                srid: 0,
                OGC.WkbByteOrder.Ndr,
                WkbTypeString(geometryDef.GeometryType),
                hasZ: geometryDef.HasZ,
                hasM: geometryDef.HasM);

        public IGeometry Decode(byte[] bytes, IGeometryDef geometryDef)
            => OGC.WKBToGeometry(bytes);

        // Multi-* for line/polygon so a gView Polyline (n paths) / Polygon (outer + holes, or
        // several exteriors) round-trips without loss; OGC.GeometryToWKB only special-cases the
        // exact strings "LINESTRING" / "POLYGON" for the single-part forms.
        private static string WkbTypeString(GeometryType type) => type switch
        {
            GeometryType.Point => "POINT",
            GeometryType.Multipoint => "MULTIPOINT",
            GeometryType.Polyline => "MULTILINESTRING",
            GeometryType.Polygon => "MULTIPOLYGON",
            GeometryType.Aggregate => "GEOMETRYCOLLECTION",
            _ => String.Empty
        };
    }
}
