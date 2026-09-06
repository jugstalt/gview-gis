using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using System;
using System.Text;

namespace gView.Framework.Data
{
    public class gViewSpatialIndexDef : ISpatialIndexDef
    {
        private IEnvelope _bounds = new Envelope();
        private double _spatialRatio = 0.55;
        private int _maxPerNode = 200;
        private int _levels = 30;
        private ISpatialReference _sRef = null;
        private GeometryStorageType _storageType = GeometryStorageType.Classic;

        public gViewSpatialIndexDef()
        {
        }
        public gViewSpatialIndexDef(IEnvelope bounds, int levels)
        {
            if (bounds != null)
            {
                _bounds = bounds;
            }

            if (_levels > 0 && _levels < 62)
            {
                _levels = levels;
            }
        }
        public gViewSpatialIndexDef(IEnvelope bounds, int levels, int maxPerNode, double spatialRatio)
            : this(bounds, levels)
        {
            if (_maxPerNode > 0)
            {
                _maxPerNode = maxPerNode;
            }

            _spatialRatio = spatialRatio;
        }

        #region ISpatialIndexDef Member

        public GeometryFieldType GeometryType
        {
            get { return GeometryFieldType.Default; }
        }

        /// <summary>
        /// Geometry storage. <see cref="GeometryStorageType.Classic"/> (proprietary blob) keeps the
        /// gView BinaryTree index this def describes; the database-native formats
        /// (<see cref="GeometryStorageType.PostGis"/>, <see cref="GeometryStorageType.SpatiaLite"/>,
        /// <see cref="GeometryStorageType.GeoPackage"/>) set it here and build no gView tree.
        /// </summary>
        public virtual GeometryStorageType StorageType
        {
            get { return _storageType; }
            set { _storageType = value; }
        }

        public IEnvelope SpatialIndexBounds
        {
            get { return _bounds; }
            set { _bounds = value; }
        }

        public double SplitRatio
        {
            get { return _spatialRatio; }
            set { _spatialRatio = value; }
        }

        public int MaxPerNode
        {
            get { return _maxPerNode; }
            set { _maxPerNode = value; }
        }

        public int Levels
        {
            get { return _levels; }
            set { _levels = value; }
        }

        public ISpatialReference SpatialReference
        {
            get { return _sRef; }
            set { _sRef = value; }
        }
        public bool ProjectTo(ISpatialReference sRef, IDatumTransformations datumTransformations)
        {
            if (_bounds == null)
            {
                return false;
            }

            if (_sRef != null && !_sRef.Equals(sRef))
            {
                IGeometry result = GeometricTransformerFactory.Transform2D(_bounds, _sRef, sRef, datumTransformations);
                if (result != null && result.Envelope != null)
                {
                    _bounds = result.Envelope;
                    _sRef = sRef;
                    return true;
                }
            }
            return true;
        }

        #endregion
    }

    /// <summary>
    /// A gView BinaryTree index whose feature class stores geometry as a PostGIS <c>geometry</c>
    /// column. The BinaryTree parameters are irrelevant (PostGIS uses its own GiST index) but the
    /// bounds / spatial reference are still carried for dataset metadata.
    /// </summary>
    public class PostGisSpatialIndexDef : gViewSpatialIndexDef
    {
        public PostGisSpatialIndexDef() { }

        public PostGisSpatialIndexDef(IEnvelope bounds, int levels) : base(bounds, levels) { }

        public override GeometryStorageType StorageType
        {
            get { return GeometryStorageType.PostGis; }
            set { /* fixed */ }
        }
    }

    public enum MSSpatialIndexLevelSize
    {
        NO = 0,
        LOW = 1,
        MEDIUM = 2,
        HIGH = 3
    }

    public class MSSpatialIndex : ISpatialIndexDef
    {
        private IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        private GeometryFieldType _fieldType = GeometryFieldType.MsGeometry;
        private IEnvelope _extent;
        private int _cellsPerObject = 256;
        private MSSpatialIndexLevelSize _level1 = MSSpatialIndexLevelSize.NO;
        private MSSpatialIndexLevelSize _level2 = MSSpatialIndexLevelSize.NO;
        private MSSpatialIndexLevelSize _level3 = MSSpatialIndexLevelSize.NO;
        private MSSpatialIndexLevelSize _level4 = MSSpatialIndexLevelSize.NO;
        private ISpatialReference _sRef = null;

        public MSSpatialIndex()
        {
            IEnvelope _extent = new Envelope();
        }

        #region Properties

        public int CellsPerObject
        {
            get { return this.MaxPerNode; }
            set { this.MaxPerNode = value; }
        }

        public MSSpatialIndexLevelSize Level1
        {
            get { return _level1; }
            set { _level1 = value; }
        }
        public MSSpatialIndexLevelSize Level2
        {
            get { return _level2; }
            set { _level2 = value; }
        }
        public MSSpatialIndexLevelSize Level3
        {
            get { return _level3; }
            set { _level3 = value; }
        }
        public MSSpatialIndexLevelSize Level4
        {
            get { return _level4; }
            set { _level4 = value; }
        }
        #endregion

        // SQL Server only accepts LOW / MEDIUM / HIGH for a grid level. "NO" (the unset default,
        // e.g. a native dataset created without explicit levels) must be mapped to a real value.
        private static string GridLevel(MSSpatialIndexLevelSize level)
            => level == MSSpatialIndexLevelSize.NO ? "MEDIUM" : level.ToString();

        private string GridsClause()
            => "GRIDS = (LEVEL_1 = " + GridLevel(_level1) + ", LEVEL_2 = " + GridLevel(_level2)
               + ", LEVEL_3 = " + GridLevel(_level3) + ", LEVEL_4 = " + GridLevel(_level4) + ")";

        public string ToSql(string indexName, string tableName, string colName)
        {
            StringBuilder sb = new StringBuilder();

            int cellsPerObject = (_cellsPerObject >= 1 && _cellsPerObject <= 8192) ? _cellsPerObject : 16;

            if (_fieldType == GeometryFieldType.MsGeography)
            {
                sb.Append("CREATE SPATIAL INDEX " + indexName);
                sb.Append(" ON " + tableName + "(" + colName + ")");
                sb.Append(" USING GEOGRAPHY_GRID WITH (");
                sb.Append(GridsClause());
                sb.Append(",CELLS_PER_OBJECT = " + cellsPerObject.ToString());
                sb.Append(")");
            }
            else if (_fieldType == GeometryFieldType.MsGeometry)
            {
                sb.Append("CREATE SPATIAL INDEX " + indexName);
                sb.Append(" ON " + tableName + "(" + colName + ")");
                sb.Append(" USING GEOMETRY_GRID WITH (");
                if (_extent != null)
                {
                    sb.Append("BOUNDING_BOX = (");
                    sb.Append("xmin=" + _extent.MinX.ToString(_nhi) + ",");
                    sb.Append("ymin=" + _extent.MinY.ToString(_nhi) + ",");
                    sb.Append("xmax=" + _extent.MaxX.ToString(_nhi) + ",");
                    sb.Append("ymax=" + _extent.MaxY.ToString(_nhi) + "),");
                }
                sb.Append(GridsClause());
                sb.Append(",CELLS_PER_OBJECT = " + cellsPerObject.ToString());
                sb.Append(")");
            }
            return sb.ToString();
        }

        #region ISpatialIndexDef Member

        public GeometryFieldType GeometryType
        {
            get { return _fieldType; }
            set
            {
                if (value == GeometryFieldType.MsGeography ||
                    value == GeometryFieldType.MsGeometry)
                {
                    _fieldType = value;
                }
            }
        }

        public GeometryStorageType StorageType
        {
            get
            {
                return _fieldType == GeometryFieldType.MsGeography
                    ? GeometryStorageType.SqlServerGeography
                    : GeometryStorageType.SqlServerGeometry;
            }
        }

        public IEnvelope SpatialIndexBounds
        {
            get { return _extent; }
            set { _extent = value; }
        }

        public double SplitRatio
        {
            get { return 0.0; }
        }

        public int MaxPerNode
        {
            get { return _cellsPerObject; }
            set { _cellsPerObject = value; }
        }

        public int Levels
        {
            get
            {
                return (int)_level1 +
                    ((int)_level2 << 4) +
                    ((int)_level3 << 8) +
                    ((int)_level4 << 12);
            }
            set
            {
                _level1 = (MSSpatialIndexLevelSize)(value & 0xf);
                _level2 = (MSSpatialIndexLevelSize)((value >> 4) & 0xf);
                _level3 = (MSSpatialIndexLevelSize)((value >> 8) & 0xf);
                _level4 = (MSSpatialIndexLevelSize)((value >> 12) & 0xf);
            }

        }
        public ISpatialReference SpatialReference
        {
            get { return _sRef; }
            set { _sRef = value; }
        }
        public bool ProjectTo(ISpatialReference sRef, IDatumTransformations datumTransformations)
        {
            if (_extent == null)
            {
                return false;
            }

            if (_sRef != null && !_sRef.Equals(sRef))
            {
                IGeometry result = GeometricTransformerFactory.Transform2D(_extent, _sRef, sRef, datumTransformations);
                if (result != null && result.Envelope != null)
                {
                    _extent = result.Envelope;
                    _sRef = sRef;
                    return true;
                }
            }
            return true;
        }
        #endregion

    }
}
