using System;
using System.IO;
using System.Collections.Generic;
using gView.Framework.Data;
using gView.Framework.Geometry;

namespace gView.DataSources.Shape
{
    internal struct MainFileHeader
    {
        public int FileCode;
        public int Unused1;
        public int Unused2;
        public int Unused3;
        public int Unused4;
        public int Unused5;
        public int FileLength;
        public int Version;
        public ShapeType ShapeType;
        public double Xmin;
        public double Ymin;
        public double Xmax;
        public double Ymax;
        public double Zmin;
        public double Zmax;
        public double Mmin;
        public double Mmax;
    }

    public enum ShapeType
    {
        /// <summary>Shape with no geometric data</summary>
        NullShape = 0,
        /// <summary>2D point</summary>
        Point = 1,
        /// <summary>2D polyline</summary>
        PolyLine = 3,
        /// <summary>2D polygon</summary>
        Polygon = 5,
        /// <summary>Set of 2D points</summary>
        MultiPoint = 8,
        /// <summary>3D point</summary>
        PointZ = 11,
        /// <summary>3D polyline</summary>
        PolyLineZ = 13,
        /// <summary>3D polygon</summary>
        PolygonZ = 15,
        /// <summary>Set of 3D points</summary>
        MultiPointZ = 18,
        /// <summary>3D point with measure</summary>
        PointM = 21,
        /// <summary>3D polyline with measure</summary>
        PolyLineM = 23,
        /// <summary>3D polygon with measure</summary>
        PolygonM = 25,
        /// <summary>Set of 3d points with measures</summary>
        MultiPointM = 28,
        /// <summary>Collection of surface patches</summary>
        MultiPatch = 31
    }

    /// <summary>
    /// 
    /// </summary>
    internal class SHPFile
    {
        private string _file_SHP = "";
        private string _file_SHX = "";
        private string _file_SBX = "";
        private string _file_IDX = "";
        private string _file_DBF = "";
        private string _file_PRJ = "";
        private BinaryReader _shx = null;
        private BinaryReader _shp = null;
        private MainFileHeader _header = new MainFileHeader();
        private long _entities = 0;
        private string _title;
        private DBFFile _dbfFile;

        public SHPFile(string filename)
        {
            try
            {
                FileInfo fi = new FileInfo(filename);
                if (!fi.Exists) return;

                _file_SHP = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".shp";
                _file_SHX = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".shx";
                _file_SBX = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".sbx";
                _file_IDX = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".idx";
                _file_DBF = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".dbf";
                _file_PRJ = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".prj";

                fi = new FileInfo(_file_SHP);
                if (!fi.Exists) return;
                fi = new FileInfo(_file_SHX);
                if (!fi.Exists) return;

                _entities = (fi.Length - 100) / 8;
                _title = fi.Name.Substring(0, fi.Name.Length - 4);

                this.Open();

                ReadHeader();

                fi = new FileInfo(_file_DBF);
                if (fi.Exists) _dbfFile = new DBFFile(_file_DBF);
            }
            catch
            {
                Close();
            }
        }

        public SHPFile(SHPFile file)
        {
            try
            {
                _file_SHP = file._file_SHP;
                _file_SHX = file._file_SHX;
                _file_IDX = file._file_IDX;
                _file_DBF = file._file_DBF;

                FileInfo fi = new FileInfo(_file_SHP);
                if (!fi.Exists) return;
                fi = new FileInfo(_file_SHX);
                if (!fi.Exists) return;

                _entities = (fi.Length - 100) / 8;
                _title = fi.Name.Substring(0, fi.Name.Length - 4);

                this.Open();

                ReadHeader();

                fi = new FileInfo(_file_DBF);
                if (fi.Exists) _dbfFile = new DBFFile(_file_DBF);
            }
            catch
            {
                Close();
            }
        }

        public string IDX_Filename
        {
            get { return _file_IDX; }
        }
        public bool IDX_Exists
        {
            get
            {
                try
                {
                    FileInfo fi = new FileInfo(IDX_Filename);
                    if (fi.Exists)
                    {
                        if (fi.Length < 8) return false;
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        public string PRJ_Filename
        {
            get { return _file_PRJ; }
        }
        public bool PRJ_Exists
        {
            get
            {
                try
                {
                    FileInfo fi = new FileInfo(PRJ_Filename);
                    return fi.Exists;
                }
                catch
                {
                    return false;
                }
            }
        }

        public string Title
        {
            get { return _title; }
        }
        public long Entities
        {
            get { return _entities; }
        }

        public void Open()
        {
            FileStream stream = new FileStream(_file_SHX, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _shx = new BinaryReader(stream);

            stream = new FileStream(_file_SHP, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _shp = new BinaryReader(stream);

            //FileInfo fi = new FileInfo(_file_IDX);
            //if (!fi.Exists || fi.LastWriteTime < this.LastWriteTime)
            //{
            //    DualTree tree = new DualTree(500);

            //    CreateSpatialIndexTree creator = new CreateSpatialIndexTree(this, tree, (IEnvelope)(new Envelope(this.Header.Xmin, this.Header.Ymin, this.Header.Xmax, this.Header.Ymax)));
            //    creator.Create();
            //}
        }

        public void Close()
        {
            if (_shp != null) _shp.Close();
            _shp = null;
            if (_shx != null) _shx.Close();
            _shx = null;
        }

        public bool Rename(string newName)
        {
            this.Close();

            try
            {
                FileInfo fi = new FileInfo(_file_SHP);
                string filter = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length) + ".*";

                fi = new FileInfo(fi.Directory + @"\" + newName + ".shp");
                if (fi.Exists) return false;

                FileInfo newFi = new FileInfo(fi.Directory.FullName + @"\" + newName);
                if (newFi.Extension.ToLower() != ".shp")
                    newFi = new FileInfo(newFi.FullName + ".shp");
                string name = newFi.Name.Substring(0, newFi.Name.Length - newFi.Extension.Length);

                foreach (FileInfo f in fi.Directory.GetFiles(filter))
                {
                    try
                    {
                        f.MoveTo(f.Directory.FullName + @"\" + name + f.Extension);
                    }
                    catch
                    {
                    }
                }

                fi = new FileInfo(_file_SHP);
                string filename = fi.Directory.FullName + @"\" + newName;

                fi = new FileInfo(filename);

                _file_SHP = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".shp";
                _file_SHX = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".shx";
                _file_SBX = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".sbx";
                _file_IDX = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".idx";
                _file_DBF = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".dbf";

            }
            catch
            {
                this.Open();
                return false;
            }
            this.Open();
            return true;
        }
        public bool Delete()
        {
            this.Close();

            try
            {
                FileInfo fi = new FileInfo(_file_SHP);
                string filter = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length) + ".*";

                foreach (FileInfo f in fi.Directory.GetFiles(filter))
                {
                    string ext = f.Extension.ToLower();
                    if (ext == ".dbf" ||
                        ext == ".idx" ||
                        ext == ".sbn" ||
                        ext == ".sbx" ||
                        ext == ".shp" ||
                        ext == ".shx" ||
                        ext == ".pr4" ||
                        ext == ".prj")
                    {
                        try
                        {
                            f.Delete();
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public DateTime LastWriteTime
        {
            get
            {
                DateTime t = new DateTime(0);
                FileInfo fi = new FileInfo(_file_SHX);
                if (fi.Exists) t = fi.LastWriteTime;

                fi = new FileInfo(_file_SHP);
                if (fi.Exists && t < fi.LastWriteTime) t = fi.LastWriteTime;

                return t;
            }
        }
        static public uint SwapWord(uint word)
        {
            uint b1 = word & 0x000000ff;
            uint b2 = (word & 0x0000ff00) >> 8;
            uint b3 = (word & 0x00ff0000) >> 16;
            uint b4 = (word & 0xff000000) >> 24;

            return (b1 << 24) + (b2 << 16) + (b3 << 8) + b4;
        }

        private void ReadHeader()
        {
            if (_shp == null) return;

            _shp.BaseStream.Position = 0;
            _header.FileCode = (int)SwapWord((uint)_shp.ReadInt32());
            _header.Unused1 = (int)SwapWord((uint)_shp.ReadInt32());
            _header.Unused2 = (int)SwapWord((uint)_shp.ReadInt32());
            _header.Unused3 = (int)SwapWord((uint)_shp.ReadInt32());
            _header.Unused4 = (int)SwapWord((uint)_shp.ReadInt32());
            _header.Unused5 = (int)SwapWord((uint)_shp.ReadInt32());
            _header.FileLength = (int)SwapWord((uint)_shp.ReadInt32());
            _header.Version = _shp.ReadInt32();
            _header.ShapeType = (ShapeType)_shp.ReadInt32();
            _header.Xmin = _shp.ReadDouble();
            _header.Ymin = _shp.ReadDouble();
            _header.Xmax = _shp.ReadDouble();
            _header.Ymax = _shp.ReadDouble();
            _header.Zmin = _shp.ReadDouble();
            _header.Zmax = _shp.ReadDouble();
            _header.Mmin = _shp.ReadDouble();
            _header.Mmax = _shp.ReadDouble();
        }

        public MainFileHeader Header
        {
            get { return _header; }
        }

        private IEnvelope ReadEnvelopeFromPos(int pos)
        {
            try
            {
                _shp.BaseStream.Position = pos * 2;  // 16-bit Word
                uint recNumber = SwapWord((uint)_shp.ReadInt32());
                _shp.BaseStream.Position += 4;

                ShapeType sType = (ShapeType)_shp.ReadInt32();
                switch (sType)
                {
                    case ShapeType.NullShape:
                        return null;
                    case ShapeType.Point:
                        Point p = new Point(_shp.ReadDouble(), _shp.ReadDouble());
                        return new Envelope(p.X, p.Y, p.X, p.Y);
                    default:
                        return new Envelope(
                            _shp.ReadDouble(),
                            _shp.ReadDouble(),
                            _shp.ReadDouble(),
                            _shp.ReadDouble());
                }
            }
            catch
            {
                return null;
            }
        }

        private IEnvelope ReadEnvelopeFromCurrentPos()
        {
            unsafe
            {
                fixed (void* v = _shp.ReadBytes(32))
                {
                    double* p = (double*)v;
                    return new Envelope(*p++, *p++, *p++, *p++);
                }
            }
        }

        public IEnvelope ReadEnvelope(uint index)
        {
            if (_shx == null) return null;
            try
            {
                _shx.BaseStream.Position = 100 + index * 8;
                uint offset = SwapWord((uint)_shx.ReadInt32());
                uint contentLength = SwapWord((uint)_shx.ReadInt32());

                _shp.BaseStream.Position = offset * 2;  // 16-bit Word
                uint recNumber = SwapWord((uint)_shp.ReadInt32());
                _shp.BaseStream.Position += 4;

                ShapeType sType = (ShapeType)_shp.ReadInt32();
                unsafe
                {
                    switch (sType)
                    {
                        case ShapeType.NullShape:
                            return null;
                        case ShapeType.PointM:
                        case ShapeType.PointZ:
                        case ShapeType.Point:
                            fixed (void* v = _shp.ReadBytes(sizeof(double) * 4))
                            {
                                double* pp = (double*)v;
                                Point p = new Point(*pp++, *pp++);
                                return new Envelope(p.X, p.Y, p.X, p.Y);
                            }
                        default:
                            fixed (void* v = _shp.ReadBytes(sizeof(double) * 4))
                            {
                                double* p = (double*)v;
                                return new Envelope(*p++, *p++, *p++, *p++);
                            }
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public IFeature ReadShape(uint index)
        {
            if (_shx == null) return null;
            try
            {
                _shx.BaseStream.Position = 100 + index * 8;
                uint offset = SwapWord((uint)_shx.ReadInt32());
                uint contentLength = SwapWord((uint)_shx.ReadInt32());

                _shp.BaseStream.Position = offset * 2;  // 16-bit Word
                uint recNumber = SwapWord((uint)_shp.ReadInt32());
                _shp.BaseStream.Position += 4;

                Feature feat = new Feature();
                feat.OID = (int)recNumber;
                ShapeType sType = (ShapeType)_shp.ReadInt32();
                feat.Shape = ReadGeometry(sType);
                return feat;
            }
            catch
            {
                return null;
            }
        }

        public IFeature ReadShape(uint index, Envelope envelope)
        {
            if (_shx == null) return null;
            try
            {
                _shx.BaseStream.Position = 100 + index * 8;
                uint offset = SwapWord((uint)_shx.ReadInt32());
                uint contentLength = SwapWord((uint)_shx.ReadInt32());

                _shp.BaseStream.Position = offset * 2;  // 16-bit Word
                uint recNumber = SwapWord((uint)_shp.ReadInt32());
                _shp.BaseStream.Position += 4;

                Feature feat = new Feature();
                feat.OID = (int)recNumber;
                ShapeType sType = (ShapeType)_shp.ReadInt32();
                feat.Shape = ReadGeometry(sType);
                return feat;
            }
            catch
            {
                return null;
            }
        }

        private IGeometry ReadGeometry(ShapeType sType)
        {
            int numPoints = 0, numParts = 0;
            int[] parts;

            IGeometry geometry = null;
            switch (sType)
            {
                case ShapeType.NullShape:
                    break;
                case ShapeType.Point:
                    geometry = new Point(_shp.ReadDouble(), _shp.ReadDouble());
                    break;
                case ShapeType.PointM:
                    geometry = new Point(_shp.ReadDouble(), _shp.ReadDouble());
                    //double m = _shp.ReadDouble();
                    break;
                case ShapeType.PointZ:
                    geometry = new Point(_shp.ReadDouble(), _shp.ReadDouble(), _shp.ReadDouble());
                    //double m=_shp.ReadDouble();
                    break;
                case ShapeType.MultiPointZ:
                case ShapeType.MultiPointM:
                case ShapeType.MultiPoint:
                    MultiPoint mPoint = new MultiPoint();
                    _shp.BaseStream.Position += 32; // BoundingBox
                    numPoints = _shp.ReadInt32();
                    ReadPoints(numPoints, (PointCollection)mPoint);

                    if (sType == ShapeType.MultiPointZ)
                    {
                        ReadZRange();
                        ReadZ(mPoint);
                    }
                    if (sType == ShapeType.MultiPointM || sType == ShapeType.MultiPointZ)
                    {
                        ReadMRange();
                        ReadM(mPoint);
                    }
                    geometry = mPoint;
                    break;
                case ShapeType.PolyLineM:
                case ShapeType.PolyLineZ:
                case ShapeType.PolyLine:
                    _shp.BaseStream.Position += 32; // BoundingBox
                    numParts = _shp.ReadInt32();
                    numPoints = _shp.ReadInt32();
                    parts = ReadParts(numParts);
                    Polyline polyline = new Polyline();
                    for (int i = 0; i < numParts; i++)
                    {
                        gView.Framework.Geometry.Path path = new gView.Framework.Geometry.Path();
                        ReadPart(i, parts, numPoints, path);
                        polyline.AddPath(path);
                    }
                    if (sType == ShapeType.PolyLineZ)
                    {
                        ReadZRange();
                        for (int i = 0; i < polyline.PathCount; i++)
                            ReadZ(polyline[i]);
                    }
                    if (sType == ShapeType.PolyLineM /* || sType == ShapeType.PolyLineZ*/)
                    {
                        ReadMRange();
                        for (int i = 0; i < polyline.PathCount; i++)
                            ReadM(polyline[i]);
                    }
                    geometry = polyline;
                    break;
                case ShapeType.PolygonM:
                case ShapeType.PolygonZ:
                case ShapeType.Polygon:
                    _shp.BaseStream.Position += 32; // BoundingBox

                    numParts = _shp.ReadInt32();
                    numPoints = _shp.ReadInt32();
                    parts = ReadParts(numParts);
                    Polygon polygon = new Polygon();
                    for (int i = 0; i < numParts; i++)
                    {
                        Ring ring = new Ring();
                        ReadPart(i, parts, numPoints, ring);
                        polygon.AddRing(ring);
                    }
                    if (sType == ShapeType.PolygonZ)
                    {
                        ReadZRange();
                        for (int i = 0; i < polygon.RingCount; i++)
                            ReadZ(polygon[i]);
                    }
                    if (sType == ShapeType.PolygonM || sType == ShapeType.PolygonZ)
                    {
                        ReadMRange();
                        for (int i = 0; i < polygon.RingCount; i++)
                            ReadM(polygon[i]);
                    }
                    geometry = polygon;
                    break;
            }
            return geometry;
        }
        public uint GetIndexFromRecNumber(uint recNumber)
        {
            try
            {
                _shx.BaseStream.Position = 100 + (recNumber - 1) * 8;
                uint offset = SwapWord((uint)_shx.ReadInt32());
                uint contentLength = SwapWord((uint)_shx.ReadInt32());

                _shp.BaseStream.Position = offset * 2;  // 16-bit Word
                uint rec = SwapWord((uint)_shp.ReadInt32());

                if (rec == recNumber) return recNumber - 1;

                for (uint index = 0; index < this.Entities; index++)
                {
                    _shx.BaseStream.Position = 100 + index * 8;
                    offset = SwapWord((uint)_shx.ReadInt32());
                    contentLength = SwapWord((uint)_shx.ReadInt32());

                    _shp.BaseStream.Position = offset * 2;  // 16-bit Word
                    rec = SwapWord((uint)_shp.ReadInt32());

                    if (rec == recNumber) return index;
                }
                return (uint)this.Entities + 1;
            }
            catch
            {
                return (uint)this.Entities + 1;
            }
        }

        #region Write
        public static bool Create(string filename, IGeometryDef geomDef, Fields fields)
        {
            if (geomDef == null || fields == null) return false;

            try
            {
                FileInfo fi = new FileInfo(filename);

                string file_SHP = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".shp";
                string file_SHX = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".shx";
                string file_DBF = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".dbf";

                FileInfo fi_SHP = new FileInfo(file_SHP);
                FileInfo fi_SHX = new FileInfo(file_SHX);
                FileInfo fi_DBF = new FileInfo(file_DBF);

                if (fi_SHP.Exists) fi_SHP.Delete();
                if (fi_SHX.Exists) fi_SHX.Delete();
                if (fi_DBF.Exists) fi_DBF.Delete();

                #region DBF
                if (!DBFFile.Create(file_DBF, fields))
                    return false;
                #endregion

                #region SHP
                ShapeType type = ShapeType.NullShape;
                switch (geomDef.GeometryType)
                {
                    case geometryType.Point:
                        type = ShapeType.Point;
                        if (geomDef.HasZ) type = ShapeType.PointZ;
                        break;
                    case geometryType.Multipoint:
                        type = ShapeType.MultiPoint;
                        if (geomDef.HasZ) type = ShapeType.MultiPointZ;
                        break;
                    case geometryType.Polyline:
                        type = ShapeType.PolyLine;
                        if (geomDef.HasZ) type = ShapeType.PolyLineZ;
                        break;
                    case geometryType.Polygon:
                        type = ShapeType.Polygon;
                        if (geomDef.HasZ) type = ShapeType.PolygonZ;
                        break;
                }

                StreamWriter sw = new StreamWriter(file_SHP);
                BinaryWriter bw = new BinaryWriter(sw.BaseStream);

                bw.Write((int)SwapWord((uint)9994));     // FileCode
                bw.Write((int)0);                        // Unused1;
                bw.Write((int)0);                        // Unused2;
                bw.Write((int)0);                        // Unused3;
                bw.Write((int)0);                        // Unused4;
                bw.Write((int)0);                        // Unused5;
                bw.Write((int)SwapWord((uint)50));      // FileLength;
                bw.Write((int)1000);                     // Version
                bw.Write((int)type);                     // ShapeType
                bw.Write((double)0.0);                   // Xmin
                bw.Write((double)0.0);                   // Ymin
                bw.Write((double)0.0);                   // Xmax
                bw.Write((double)0.0);                   // Ymax
                bw.Write((double)0.0);                   // Zmin
                bw.Write((double)0.0);                   // Zmax
                bw.Write((double)0.0);                   // Mmin
                bw.Write((double)0.0);                   // Mmax

                bw.Flush();
                sw.Flush();
                sw.Close();
                #endregion

                #region SHX
                sw = new StreamWriter(file_SHX);
                bw = new BinaryWriter(sw.BaseStream);

                bw.Write((int)SwapWord((uint)9994));     // FileCode
                bw.Write((int)0);                        // Unused1;
                bw.Write((int)0);                        // Unused2;
                bw.Write((int)0);                        // Unused3;
                bw.Write((int)0);                        // Unused4;
                bw.Write((int)0);                        // Unused5;
                bw.Write((int)SwapWord((uint)50));      // FileLength;
                bw.Write((int)1000);                     // Version
                bw.Write((int)type);                     // ShapeType
                bw.Write((double)0.0);                   // Xmin
                bw.Write((double)0.0);                   // Ymin
                bw.Write((double)0.0);                   // Xmax
                bw.Write((double)0.0);                   // Ymax
                bw.Write((double)0.0);                   // Zmin
                bw.Write((double)0.0);                   // Zmax
                bw.Write((double)0.0);                   // Mmin
                bw.Write((double)0.0);                   // Mmax

                bw.Flush();
                sw.Flush();
                sw.Close();
                #endregion

                return true;
            }
            catch
            {
                return false;
            }
        }

        internal bool WriteShape(IFeature feature)
        {
            if (feature == null) return false;

            StreamWriter sw_shx = null;
            StreamWriter sw_shp = null;
            BinaryWriter bw_shp = null;
            BinaryWriter bw_shx = null;
            FileStream fs_shx = null, fs_shp = null;

            try
            {
                //this.Close();

                sw_shx = new StreamWriter(_file_SHX, true);
                sw_shp = new StreamWriter(_file_SHP, true);
                bw_shx = new BinaryWriter(sw_shx.BaseStream);
                bw_shp = new BinaryWriter(sw_shp.BaseStream);

                //sw_shx.BaseStream.Position = fi_shp.Length;
                //sw_shx.BaseStream.Position = fi_shx.Length;

                long pos1 = sw_shp.BaseStream.Position;
                uint recNumber = (uint)(sw_shx.BaseStream.Length - 100) / 8 + 1;

                HeaderEnvelope he = new HeaderEnvelope();

                long contentsLenthPos = 0;
                switch (_header.ShapeType)
                {
                    case ShapeType.NullShape:
                        break;
                    case ShapeType.PointM:
                    case ShapeType.PointZ:
                    case ShapeType.Point:
                        if (!(feature.Shape is IPoint)) return false;

                        IPoint p = (IPoint)feature.Shape;
                        he.minx = he.maxx = p.X;
                        he.miny = he.maxy = p.Y;
                        if (_header.ShapeType == ShapeType.PointZ)
                            he.minz = he.maxz = p.Z;

                        contentsLenthPos = WriteFeatureHeader(bw_shp, recNumber);
                        WritePoint(bw_shp, (IPoint)feature.Shape);
                        if (_header.ShapeType == ShapeType.PointZ)
                        {
                            bw_shp.Write(((IPoint)feature.Shape).Z);
                        }
                        if (_header.ShapeType == ShapeType.PointM || _header.ShapeType == ShapeType.PointZ)
                        {
                            //bw_shp.Write(((IPoint)feature.Shape).M);
                            bw_shp.Write((double)0.0);
                        }
                        break;
                    case ShapeType.MultiPointM:
                    case ShapeType.MultiPointZ:
                    case ShapeType.MultiPoint:
                        if (feature.Shape is IPoint)
                        {
                            contentsLenthPos = WriteFeatureHeader(bw_shp, recNumber);
                            WriteEnvelope(bw_shp, feature.Shape.Envelope, he);
                            bw_shp.Write((int)1);
                            WritePoint(bw_shp, (IPoint)feature.Shape);
                            if (_header.ShapeType == ShapeType.MultiPointZ)
                            {
                                bw_shp.Write(((IPoint)feature.Shape).Z);
                            }
                            if (_header.ShapeType == ShapeType.MultiPointM || _header.ShapeType == ShapeType.MultiPointZ)
                            {
                                //bw_shp.Write(((IPoint)feature.Shape).M);
                                bw_shp.Write((double)0.0);
                            }
                        }
                        else if (feature.Shape is IPointCollection)
                        {
                            contentsLenthPos = WriteFeatureHeader(bw_shp, recNumber);
                            WriteEnvelope(bw_shp, feature.Shape.Envelope, he);
                            bw_shp.Write((int)((IPointCollection)feature.Shape).PointCount);
                            WritePoints(bw_shp, (IPointCollection)feature.Shape);
                            if (_header.ShapeType == ShapeType.MultiPointZ)
                            {
                                WritePointsZRange(bw_shp, (IPointCollection)feature.Shape);
                                WritePointsZ(bw_shp, (IPointCollection)feature.Shape);
                            }
                            if (_header.ShapeType == ShapeType.MultiPointM || _header.ShapeType == ShapeType.MultiPointZ)
                            {
                                //bw_shp.Write(((IPoint)feature.Shape).M);
                                WritePointsMRange(bw_shp, (IPointCollection)feature.Shape);
                                WritePointsM(bw_shp, (IPointCollection)feature.Shape);
                            }
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    case ShapeType.PolyLineM:
                    case ShapeType.PolyLineZ:
                    case ShapeType.PolyLine:
                        if (!(feature.Shape is IPolyline)) return false;
                        IPolyline pline = (IPolyline)feature.Shape;

                        contentsLenthPos = WriteFeatureHeader(bw_shp, recNumber);
                        WriteEnvelope(bw_shp, feature.Shape.Envelope, he);

                        bw_shp.Write((int)pline.PathCount);
                        WritePointCount(bw_shp, pline);
                        WriteParts(bw_shp, pline);

                        for (int i = 0; i < pline.PathCount; i++)
                            WritePoints(bw_shp, pline[i]);

                        if (_header.ShapeType == ShapeType.PolyLineM || _header.ShapeType == ShapeType.PolyLineZ)
                        {
                            IPointCollection pColl = gView.Framework.SpatialAlgorithms.Algorithm.GeometryPoints(pline, false);
                            if (_header.ShapeType == ShapeType.PolyLineZ)
                            {
                                WritePointsZRange(bw_shp, pColl);
                                WritePointsZ(bw_shp, pColl);
                            }
                            //bw_shp.Write(((IPoint)feature.Shape).M);
                            WritePointsMRange(bw_shp, pColl);
                            WritePointsM(bw_shp, pColl);
                        }
                        break;
                    case ShapeType.PolygonM:
                    case ShapeType.PolygonZ:
                    case ShapeType.Polygon:
                        if (!(feature.Shape is IPolygon)) return false;
                        IPolygon poly = (IPolygon)feature.Shape;

                        contentsLenthPos = WriteFeatureHeader(bw_shp, recNumber);
                        WriteEnvelope(bw_shp, feature.Shape.Envelope, he);

                        bw_shp.Write((int)poly.RingCount);
                        WritePointCount(bw_shp, poly);
                        WriteParts(bw_shp, poly);

                        for (int i = 0; i < poly.RingCount; i++)
                            WritePoints(bw_shp, poly[i]);

                        if (_header.ShapeType == ShapeType.PolygonM || _header.ShapeType == ShapeType.PolygonZ)
                        {
                            IPointCollection pColl = gView.Framework.SpatialAlgorithms.Algorithm.GeometryPoints(poly, false);
                            if (_header.ShapeType == ShapeType.PolygonZ)
                            {
                                WritePointsZRange(bw_shp, pColl);
                                WritePointsZ(bw_shp, pColl);
                            }
                            //bw_shp.Write(((IPoint)feature.Shape).M);
                            WritePointsMRange(bw_shp, pColl);
                            WritePointsM(bw_shp, pColl);
                        }
                        break;
                    default:
                        return false;
                }

                sw_shp.Flush();

                uint contentsSize = (uint)(sw_shp.BaseStream.Position - pos1 - 8) / 2; // -8 weil recnumber und nullword nicht mitzählen. Erst Shapetype und coordinaten,...
                bw_shx.Write((int)SwapWord((uint)(pos1 / 2))); // 16 bit Words
                bw_shx.Write((int)SwapWord(contentsSize));

                sw_shx.Flush();

                sw_shx.Close(); sw_shx = null;
                sw_shp.Close(); sw_shp = null;

                fs_shp = new FileStream(_file_SHP, FileMode.Open);
                fs_shx = new FileStream(_file_SHX, FileMode.Open);

                bw_shp = new BinaryWriter(fs_shp);
                bw_shx = new BinaryWriter(fs_shx);

                if (contentsLenthPos != 0)
                {
                    bw_shp.BaseStream.Position = contentsLenthPos;
                    bw_shp.Write((int)SwapWord((uint)contentsSize));
                }

                UpdateHeaderEnvelope(bw_shp, he);
                UpdateHeaderEnvelope(bw_shx, he);

                fs_shp.Flush();
                fs_shx.Flush();

                //this.Open();

                _dbfFile.WriteRecord(recNumber, feature);
                return true;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return false;
            }
            finally
            {
                if (sw_shx != null) sw_shx.Close();
                if (sw_shp != null) sw_shp.Close();
                if (fs_shp != null) fs_shp.Close();
                if (fs_shx != null) fs_shx.Close();
            }
        }

        private void UpdateHeaderEnvelope(BinaryWriter bw, HeaderEnvelope he)
        {
            try
            {
                double MinX = (_header.Xmin != 0.0) ? _header.Xmin : he.minx;
                double MinY = (_header.Ymin != 0.0) ? _header.Ymin : he.miny;
                double MaxX = (_header.Xmax != 0.0) ? _header.Xmax : he.maxx;
                double MaxY = (_header.Ymax != 0.0) ? _header.Ymax : he.maxy;
                double MinZ = (_header.Zmin != 0.0) ? _header.Zmin : he.minz;
                double MaxZ = (_header.Zmax != 0.0) ? _header.Zmax : he.maxz;
                double MinM = (_header.Mmin != 0.0) ? _header.Mmin : he.minm;
                double MaxM = (_header.Mmax != 0.0) ? _header.Mmax : he.maxm;

                bw.BaseStream.Position = 36;
                bw.Write((double)(_header.Xmin = Math.Min(MinX, he.minx)));
                bw.Write((double)(_header.Ymin = Math.Min(MinY, he.miny)));
                bw.Write((double)(_header.Xmax = Math.Max(MaxX, he.maxx)));
                bw.Write((double)(_header.Ymax = Math.Max(MaxY, he.maxy)));
                bw.Write((double)(_header.Zmin = Math.Min(MinZ, he.minz)));
                bw.Write((double)(_header.Zmax = Math.Max(MaxZ, he.maxz)));
                bw.Write((double)(_header.Mmin = Math.Min(MinM, he.minm)));
                bw.Write((double)(_header.Mmax = Math.Max(MaxM, he.maxm)));

                // FileLength in 16 bit Words
                bw.BaseStream.Position = 24;
                bw.Write((int)SwapWord((uint)(bw.BaseStream.Length / 2)));
                bw.BaseStream.Flush();
            }
            catch (Exception ex)
            {
                string err = ex.Message;
            }
        }

        private long WriteFeatureHeader(BinaryWriter bw, uint recNumber)
        {
            bw.Write((int)SwapWord(recNumber));
            long contentsLengthPos = bw.BaseStream.Position;
            bw.Write((int)0);
            bw.Write((int)_header.ShapeType);

            return contentsLengthPos;
        }
        private void WriteEnvelope(BinaryWriter bw, IEnvelope envelope, HeaderEnvelope he)
        {
            if (envelope == null)
            {
                bw.Write((double)0.0);
                bw.Write((double)0.0);
                bw.Write((double)0.0);
                bw.Write((double)0.0);
            }
            else
            {
                bw.Write(he.minx = envelope.minx);
                bw.Write(he.miny = envelope.miny);
                bw.Write(he.maxx = envelope.maxx);
                bw.Write(he.maxy = envelope.maxy);
            }
        }
        private void WritePoint(BinaryWriter bw, IPoint point)
        {
            bw.Write((double)(point.X));
            bw.Write((double)(point.Y));

            if (_header.ShapeType == ShapeType.PointZ ||
                _header.ShapeType == ShapeType.MultiPointZ ||
                _header.ShapeType == ShapeType.PolyLineZ ||
                _header.ShapeType == ShapeType.PolygonZ)
            {
                bw.Write((double)point.Z);
            }
        }
        private void WritePoints(BinaryWriter bw, IPointCollection pColl)
        {
            for (int i = 0; i < pColl.PointCount; i++)
                WritePoint(bw, pColl[i]);
        }
        private void WritePointsMRange(BinaryWriter bw, IPointCollection pColl)
        {
            double min = 0;
            double max = 0;
            //for (int i = 0; i < pColl.PointCount; i++)
            //{
            //    if (i == 0)
            //    {
            //        min = pColl[i].M;
            //        max = pColl[i].M;
            //    }
            //    else
            //    {
            //        min = Math.Min(min, pColl[i].M);
            //        max = Math.Max(max, pColl[i].M);
            //    }
            //}
            bw.Write(min);
            bw.Write(max);
        }
        private void WritePointsM(BinaryWriter bw, IPointCollection pColl)
        {
            for (int i = 0; i < pColl.PointCount; i++)
            {
                //bw.Write(pColl[i].M);
                bw.Write((double)0);
            }
        }
        private void WritePointsZRange(BinaryWriter bw, IPointCollection pColl)
        {
            double min = 0;
            double max = 0;
            for (int i = 0; i < pColl.PointCount; i++)
            {
                if (i == 0)
                {
                    min = pColl[i].Z;
                    max = pColl[i].Z;
                }
                else
                {
                    min = Math.Min(min, pColl[i].Z);
                    max = Math.Max(max, pColl[i].Z);
                }
            }
            bw.Write(min);
            bw.Write(max);
        }
        private void WritePointsZ(BinaryWriter bw, IPointCollection pColl)
        {
            for (int i = 0; i < pColl.PointCount; i++)
            {
                bw.Write(pColl[i].Z);
            }
        }
        private void WriteParts(BinaryWriter bw, IGeometry geometry)
        {
            if (geometry is IPolyline)
            {
                IPolyline pLine = (IPolyline)geometry;
                int c = 0;
                for (int i = 0; i < pLine.PathCount; i++)
                {
                    bw.Write((int)c);
                    c += pLine[i].PointCount;
                }
            }
            else if (geometry is IPolygon)
            {
                IPolygon poly = (IPolygon)geometry;
                int c = 0;
                for (int i = 0; i < poly.RingCount; i++)
                {
                    bw.Write((int)c);
                    c += poly[i].PointCount;
                }
            }
        }
        private void WritePointCount(BinaryWriter bw, IGeometry geometry)
        {
            if (geometry is IPolyline)
            {
                IPolyline pLine = (IPolyline)geometry;
                int c = 0;
                for (int i = 0; i < pLine.PathCount; i++)
                {
                    c += pLine[i].PointCount;
                }
                bw.Write((int)c);
            }
            else if (geometry is IPolygon)
            {
                IPolygon poly = (IPolygon)geometry;
                int c = 0;
                for (int i = 0; i < poly.RingCount; i++)
                {
                    c += poly[i].PointCount;
                }
                bw.Write((int)c);
            }
            else
            {
                bw.Write((int)0);
            }
        }
        #endregion

        private void ReadPoints(int numPoints, PointCollection pointCol)
        {
            unsafe
            {
                fixed (void* b = _shp.ReadBytes(sizeof(double) * 2 * numPoints))
                {
                    double* p = (double*)b;
                    for (int i = 0; i < numPoints; i++)
                    {
                        pointCol.AddPoint(new Point(*p++, *p++));
                    }
                }
            }
            /*

			for(int i=0;i<numPoints;i++) 
			{
				pointCol.AddPoint(new Point(_shp.ReadDouble(),_shp.ReadDouble()));
			}*/
        }
        private void ReadMRange()
        {
            _shp.ReadDouble();  // minM
            _shp.ReadDouble();  // maxM
        }
        private void ReadM(IPointCollection pointCol)
        {
            // M gibts noch nicht im gView
            for (int i = 0; i < pointCol.PointCount; i++)
            {
                pointCol[i].M = _shp.ReadDouble();
            }
        }
        private void ReadZRange()
        {
            _shp.ReadDouble();  // minZ
            _shp.ReadDouble();  // maxZ
        }
        private void ReadZ(IPointCollection pointCol)
        {
            for (int i = 0; i < pointCol.PointCount; i++)
            {
                pointCol[i].Z = _shp.ReadDouble();
            }
        }
        private int[] ReadParts(int numParts)
        {
            /*
			int [] parts=new int[numParts];
			for(int i=0;i<numParts;i++)
				parts[i]=_shp.ReadInt32();
			return parts;
             * */
            unsafe
            {
                fixed (void* v = _shp.ReadBytes(4 * numParts))
                {
                    int* p = (int*)v;
                    int[] parts = new int[numParts];
                    for (int i = 0; i < numParts; i++)
                        parts[i] = *p++;
                    return parts;
                }
            }
        }
        private void ReadPart(int partNr, int[] parts, int numPoints, PointCollection pointCol)
        {
            partNr++;
            int num = 0;
            if (partNr >= parts.Length)
                num = numPoints - parts[partNr - 1];
            else
                num = parts[partNr] - parts[partNr - 1];
            ReadPoints(num, pointCol);
        }

        public DBFDataReader DBFDataReader(string fieldnames)
        {
            return new DBFDataReader(_dbfFile, fieldnames);
        }

        public IFields Fields
        {
            get
            {
                if (_dbfFile != null)
                {
                    return _dbfFile.Fields;
                }
                return new Fields();
            }
        }

        public bool DeleteIDX()
        {
            try
            {
                FileInfo fi = new FileInfo(_file_IDX);
                if (fi.Exists) fi.Delete();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private class HeaderEnvelope
        {
            public double minx = 0.0, maxx = 0.0;
            public double miny = 0.0, maxy = 0.0;
            public double minz = 0.0, maxz = 0.0;
            public double minm = 0.0, maxm = 0.0;
        }
    }
}
