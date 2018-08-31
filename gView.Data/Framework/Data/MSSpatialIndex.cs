using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Geometry;
using gView.Framework.FDB;

namespace gView.Framework.Data
{
    public class gViewSpatialIndexDef : ISpatialIndexDef
    {
        private IEnvelope _bounds = new Envelope();
        private double _spatialRatio = 0.55;
        private int _maxPerNode = 200;
        private int _levels = 30;
        private ISpatialReference _sRef = null;

        public gViewSpatialIndexDef()
        {
        }
        public gViewSpatialIndexDef(IEnvelope bounds, int levels)
        {
            if (bounds != null)
                _bounds = bounds;
            if (_levels > 0 && _levels < 62)
                _levels = levels;
        }
        public gViewSpatialIndexDef(IEnvelope bounds, int levels, int maxPerNode, double spatialRatio)
            : this(bounds, levels)
        {
            if (_maxPerNode > 0)
                _maxPerNode = maxPerNode;
            _spatialRatio = spatialRatio;
        }

        #region ISpatialIndexDef Member

        public GeometryFieldType GeometryType
        {
            get { return GeometryFieldType.Default; }
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
        public bool ProjectTo(ISpatialReference sRef)
        {
            if (_bounds == null) return false;

            if (_sRef != null && !_sRef.Equals(sRef))
            {
                IGeometry result = GeometricTransformer.Transform2D(_bounds, _sRef, sRef);
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

        public string ToSql(string indexName, string tableName, string colName)
        {
            StringBuilder sb = new StringBuilder();

            if (_fieldType == GeometryFieldType.MsGeography)
            {
                sb.Append("CREATE SPATIAL INDEX " + indexName);
                sb.Append(" ON " + tableName + "(" + colName + ")");
                sb.Append(" USING GEOGRAPHY_GRID WITH (");
                sb.Append("GRIDS = (LEVEL_1 = " + _level1.ToString() + ", LEVEL_2 = " + _level2.ToString() + ", LEVEL_3 = " + _level3.ToString() + ", LEVEL_4 = " + _level4.ToString() + ")");
                sb.Append(",CELLS_PER_OBJECT = " + _cellsPerObject.ToString());
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
                    sb.Append("xmin=" + _extent.minx.ToString(_nhi) + ",");
                    sb.Append("ymin=" + _extent.miny.ToString(_nhi) + ",");
                    sb.Append("xmax=" + _extent.maxx.ToString(_nhi) + ",");
                    sb.Append("ymax=" + _extent.maxy.ToString(_nhi) + "),");
                }
                sb.Append("GRIDS = (LEVEL_1 = " + _level1.ToString() + ", LEVEL_2 = " + _level2.ToString() + ", LEVEL_3 = " + _level3.ToString() + ", LEVEL_4 = " + _level4.ToString() + ")");
                sb.Append(",CELLS_PER_OBJECT = " + _cellsPerObject.ToString());
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
                    _fieldType = value;
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
        public bool ProjectTo(ISpatialReference sRef)
        {
            if (_extent == null) return false;
            
            if (_sRef != null && !_sRef.Equals(sRef))
            {
                IGeometry result = GeometricTransformer.Transform2D(_extent, _sRef, sRef);
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
