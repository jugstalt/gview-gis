using gView.Framework.Geometry;
using System;
using System.IO;
using System.Text;

namespace gView.Framework.OGC
{
    public class OGC
    {
        public enum WkbByteOrder : byte
        {
            /// <summary>
            /// XDR (Big Endian) Encoding of Numeric Types
            /// </summary>
            /// <remarks>
            /// <para>The XDR representation of an Unsigned Integer is Big Endian (most significant byte first).</para>
            /// <para>The XDR representation of a Double is Big Endian (sign bit is first byte).</para>
            /// </remarks>
            Xdr = 0,
            /// <summary>
            /// NDR (Little Endian) Encoding of Numeric Types
            /// </summary>
            /// <remarks>
            /// <para>The NDR representation of an Unsigned Integer is Little Endian (least significant byte first).</para>
            /// <para>The NDR representation of a Double is Little Endian (sign bit is last byte).</para>
            /// </remarks>
            Ndr = 1
        }

        /// <summary>
        /// Enumeration to determine geometrytype in Well-known Binary
        /// </summary>
        internal enum WKBGeometryType : uint
        {
            wkbPoint = 1,
            wkbLineString = 2,
            wkbPolygon = 3,
            wkbMultiPoint = 4,
            wkbMultiLineString = 5,
            wkbMultiPolygon = 6,
            wkbGeometryCollection = 7,
            wkbCircularString = 8,
            wkbCompoundCurve = 9,
            wkbCurvePolygon = 10,
            wkbMultiCurve = 11,
            wkbMultiSurface = 12,
            wkbCurve = 13,
            wkbSurface = 14,
            wkbPolyhedralSurface = 15,
            wkbTIN = 16,
            wkbTriangle = 17,
            wkbCircle = 18,
            wkbGeodesicString = 19,
            wkbEllipticalCurve = 20,
            wkbNurbsCurve = 21,
            wkbClothoid = 22,
            wkbSpiralCurve = 23,
            wkbCompoundSurface = 24
        }

        public readonly static System.Globalization.NumberFormatInfo numberFormat_EnUS = new System.Globalization.CultureInfo("en-US", false).NumberFormat;

        public static string Envelope2box2(IEnvelope envelope, ISpatialReference sRef)
        {
            if (envelope == null)
            {
                return "";
            }

            if (sRef == null)
            {
                string box2 = "box2d('BOX3D(" +
                                envelope.minx.ToString(numberFormat_EnUS) + " " +
                                envelope.miny.ToString(numberFormat_EnUS) + "," +
                                envelope.maxx.ToString(numberFormat_EnUS) + " " +
                                envelope.maxy.ToString(numberFormat_EnUS) + ")'::box3d)";
                return box2;
            }
            else
            {
                string[] srid = sRef.Name.Split(':');
                string box2 = "st_setsrid(box2d('BOX3D(" +
                                envelope.minx.ToString(numberFormat_EnUS) + " " +
                                envelope.miny.ToString(numberFormat_EnUS) + "," +
                                envelope.maxx.ToString(numberFormat_EnUS) + " " +
                                envelope.maxy.ToString(numberFormat_EnUS) + ")'::box3d)," + srid[srid.Length - 1] + ")";
                return box2;
            }
        }

        public static IGeometry WKBToGeometry(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                return WKBToGeometry(reader);
            }
        }

        public static IGeometry WKBToGeometry(BinaryReader reader)
        {
            // Get the first byte in the array.  This specifies if the WKB is in
            // XDR (big-endian) format of NDR (little-endian) format.
            byte byteOrder = reader.ReadByte();

            if (!Enum.IsDefined(typeof(WkbByteOrder), byteOrder))
            {
                throw new ArgumentException("Byte order not recognized");
            }

            return WKBToGeometry(reader, (WkbByteOrder)byteOrder);
        }

        private static IGeometry WKBToGeometry__old_iso_wkb(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // Get the type of this geometry.
            uint type = (uint)ReadUInt32(reader, (WkbByteOrder)byteOrder);

            // https://github.com/jugstalt/gview5/issues/17
            // https://libgeos.org/specifications/wkb/  (ISO or Extended WKB?)
            // check if Extended WKB
            // int newSrid = (type & 0x20000000) != 0 ? reader.ReadInt32() : -1; // < --only if interestested on SRID
            // type = (type & 0xffff) % 1000; // ** < --can type correctly to Enum

            // Otherwise its maybe ISO WKB
            bool hasZ = false, hasM = false;
            if (type >= 1000 && type < 2000)
            {
                hasZ = true;
                type -= 1000;
            }
            else if (type >= 2000 && type < 3000)
            {
                hasM = true;
                type -= 2000;
            }
            else if (type >= 3000 && type < 4000)
            {
                hasZ = hasM = true;
                type -= 3000;
            }

            if (!Enum.IsDefined(typeof(WKBGeometryType), type))
            {
                throw new ArgumentException("Geometry type not recognized");
            }

            switch ((WKBGeometryType)type)
            {
                case WKBGeometryType.wkbPoint:
                    return CreatePoint(reader, byteOrder, hasZ, hasM);

                case WKBGeometryType.wkbLineString:
                    return CreateLineString(reader, byteOrder, hasZ, hasM);

                case WKBGeometryType.wkbPolygon:
                    return CreatePolygon(reader, byteOrder, hasZ, hasM);

                case WKBGeometryType.wkbMultiPoint:
                    return CreateMultiPoint(reader, byteOrder, hasZ, hasM);

                case WKBGeometryType.wkbMultiLineString:
                    return CreateMultiLineString(reader, byteOrder, hasZ, hasM);

                case WKBGeometryType.wkbMultiPolygon:
                    return CreateMultiPolygon(reader, byteOrder, hasZ, hasM);

                case WKBGeometryType.wkbGeometryCollection:
                    return CreateGeometryCollection(reader, byteOrder, hasZ, hasM);

                //case WKBGeometryType.wkbMultiCurve:
                //    return CreateMultiCurve(reader, byteOrder, hasZ, hasM);

                default:
                    throw new NotSupportedException("Geometry type '" + ((WKBGeometryType)type).ToString() + "' not supported");
            }
        }

        private static IGeometry WKBToGeometry(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // Read geometry type
            uint geometryType = (uint)ReadUInt32(reader, byteOrder);

            bool hasZ, hasM, hasSRID;
            int srid = 0;
            uint baseGeometryType;

            // https://libgeos.org/specifications/wkb/
            // Check if the higher bits are set (Extended WKB)
            if ((geometryType & 0xE0000000) != 0)
            {
                // Extract Z, M and SRID flags from geometry type
                hasZ = (geometryType & 0x80000000) != 0;
                hasM = (geometryType & 0x40000000) != 0;
                hasSRID = (geometryType & 0x20000000) != 0;
                baseGeometryType = geometryType & 0x1FFFFFFF;

                if (hasSRID)
                {
                    // Read SRID
                    srid = (int)ReadUInt32(reader, byteOrder);
                }
            }
            else
            {
                // Assume ISO WKB format

                // Extract Z and M values from geometry type
                hasZ = (geometryType / 1000) % 2 != 0;
                hasM = (geometryType / 2000) % 2 != 0;
                baseGeometryType = geometryType % 1000;

                // ISO WKB does not store SRID
                hasSRID = false;
            }

            if (!Enum.IsDefined(typeof(WKBGeometryType), baseGeometryType))
            {
                throw new ArgumentException("Geometry type not recognized");
            }

            IGeometry geometry;

            switch ((WKBGeometryType)baseGeometryType)
            {
                case WKBGeometryType.wkbPoint:
                    geometry = CreatePoint(reader, byteOrder, hasZ, hasM);
                    break;
                case WKBGeometryType.wkbLineString:
                    geometry = CreateLineString(reader, byteOrder, hasZ, hasM);
                    break;
                case WKBGeometryType.wkbPolygon:
                    geometry = CreatePolygon(reader, byteOrder, hasZ, hasM);
                    break;
                case WKBGeometryType.wkbMultiPoint:
                    geometry = CreateMultiPoint(reader, byteOrder, hasZ, hasM);
                    break;
                case WKBGeometryType.wkbMultiLineString:
                    geometry = CreateMultiLineString(reader, byteOrder, hasZ, hasM);
                    break;
                case WKBGeometryType.wkbMultiPolygon:
                    geometry = CreateMultiPolygon(reader, byteOrder, hasZ, hasM);
                    break;
                case WKBGeometryType.wkbGeometryCollection:
                    geometry = CreateGeometryCollection(reader, byteOrder, hasZ, hasM);
                    break;
                //case WKBGeometryType.wkbMultiCurve:
                //    return CreateMultiCurve(reader, byteOrder, hasZ, hasM);

                default:
                    throw new NotSupportedException("Geometry type '" + ((WKBGeometryType)baseGeometryType).ToString() + "' not supported");
            }

            if (hasSRID && srid > 0 && geometry != null)
            {
                geometry.Srs = srid;
            }

            return geometry;
        }

        public static string BytesToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                byte b1 = (byte)((b & 0xf0) >> 4);
                byte b2 = (byte)(b & 0x0f);

                sb.Append(HexChar(b1));
                sb.Append(HexChar(b2));
            }
            return sb.ToString();
        }

        private static char HexChar(byte b)
        {
            switch (b & 0x0f)
            {
                case 0:
                    return '0';
                case 1:
                    return '1';
                case 2:
                    return '2';
                case 3:
                    return '3';
                case 4:
                    return '4';
                case 5:
                    return '5';
                case 6:
                    return '6';
                case 7:
                    return '7';
                case 8:
                    return '8';
                case 9:
                    return '9';
                case 10:
                    return 'A';
                case 11:
                    return 'B';
                case 12:
                    return 'C';
                case 13:
                    return 'D';
                case 14:
                    return 'E';
                case 15:
                    return 'F';
            }
            return '0';
        }

        public static byte[] GeometryToWKB(IGeometry geometry, int srid, WkbByteOrder byteOrder)
        {
            return GeometryToWKB(geometry, srid, byteOrder, String.Empty, false, false);
        }
        public static byte[] GeometryToWKB(IGeometry geometry, int srid, WkbByteOrder byteOrder, string typeString,
                                           bool hasZ, bool hasM)
        {
            MemoryStream ms = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write((byte)byteOrder);

                if (geometry is IPoint)
                {
                    WriteGeometryType(writer, (uint)WKBGeometryType.wkbPoint, srid, byteOrder, hasZ, hasM);
                    WritePoint((IPoint)geometry, writer, byteOrder, hasZ, hasM);
                }
                else if (geometry is IMultiPoint)
                {
                    WriteGeometryType(writer, (uint)WKBGeometryType.wkbMultiPoint, srid, byteOrder, hasZ, hasM);
                    WriteMultiPoint((IMultiPoint)geometry, writer, byteOrder, hasZ, hasM);
                }
                else if (geometry is IPolyline)
                {
                    if (typeString == "LINESTRING" && ((IPolyline)geometry).PathCount == 1)
                    {
                        WriteGeometryType(writer, (uint)WKBGeometryType.wkbLineString, srid, byteOrder, hasZ, hasM);
                        WriteLineString(((IPolyline)geometry)[0], writer, byteOrder, hasZ, hasM);
                    }
                    else
                    {
                        WriteGeometryType(writer, (uint)WKBGeometryType.wkbMultiLineString, srid, byteOrder, hasZ, hasM);
                        WriteMultiLineString((IPolyline)geometry, writer, byteOrder, hasZ, hasM);
                    }
                }
                else if (geometry is IPolygon)
                {
                    if (typeString == "POLYGON")
                    {
                        WriteGeometryType(writer, (uint)WKBGeometryType.wkbPolygon, srid, byteOrder, hasZ, hasM);
                        WritePolygon((IPolygon)geometry, writer, byteOrder, hasZ, hasM);
                    }
                    else
                    {
                        WriteGeometryType(writer, (uint)WKBGeometryType.wkbMultiPolygon, srid, byteOrder, hasZ, hasM);
                        WriteMultiPolygon((IPolygon)geometry, writer, byteOrder, hasZ, hasM);
                    }
                }
                else if (geometry is IAggregateGeometry)
                {
                    WriteGeometryType(writer, (uint)WKBGeometryType.wkbGeometryCollection, srid, byteOrder, hasZ, hasM);
                    WriteGeometryCollection((IAggregateGeometry)geometry, writer, byteOrder, hasZ, hasM);
                }
                else
                {
                    throw new NotSupportedException("Geometry type is not supported");
                }

                return ms.ToArray();
            }
        }

        private static void WriteGeometryType(BinaryWriter writer, uint geometryType, int srid, WkbByteOrder byteOrder, bool hasZ, bool hasM)
        {
            // https://libgeos.org/specifications/wkb/
            if (hasZ)
            {
                geometryType += (uint)0x80000000;
            }
            if (hasM)
            {
                geometryType += (uint)0x40000000;
            }

            if (srid == 0)
            {
                WriteUInt32((uint)geometryType, writer, byteOrder);
            }
            else  // Gstalt  (PostGIS) ??!!
            {
                geometryType += (uint)0x20000000;
                WriteUInt32((uint)geometryType, writer, byteOrder);
                writer.Write((uint)srid);
            }
        }

        private static Point CreatePoint(BinaryReader reader, WkbByteOrder byteOrder, bool hasZ, bool hasM)
        {
            // Create and return the point.
            Point p = new Point(ReadDouble(reader, byteOrder), ReadDouble(reader, byteOrder));

            if (hasZ == true && hasM == true)
            {
                p.Z = ReadDouble(reader, byteOrder);
                p.M = ReadDouble(reader, byteOrder);
            }
            else if (hasM == true)
            {
                p.M = ReadDouble(reader, byteOrder);
            }
            else if (hasZ == true)
            {
                p.Z = ReadDouble(reader, byteOrder);
            }

            return p;
        }

        private static void WritePoint(IPoint point, BinaryWriter writer, WkbByteOrder byteOrder, bool hasZ, bool hasM)
        {
            WriteDouble(point.X, writer, byteOrder);
            WriteDouble(point.Y, writer, byteOrder);

            if (hasZ)
            {
                WriteDouble(point.Z, writer, byteOrder);
            }
            if (hasM)
            {
                double M = 0D;
                if (point is PointM)
                {
                    try
                    {
                        M = Convert.ToDouble(((PointM)point).M);
                    }
                    catch { }
                }

                WriteDouble(M, writer, byteOrder);
            }
        }

        private static void ReadPointCollection(BinaryReader reader, WkbByteOrder byteOrder, IPointCollection pColl, bool hasZ, bool hasM)
        {
            if (pColl == null)
            {
                return;
            }

            // Get the number of points in this linestring.
            int numPoints = (int)ReadUInt32(reader, byteOrder);

            // Loop on the number of points in the ring.
            for (int i = 0; i < numPoints; i++)
            {
                // Add the coordinate.
                Point p = new Point(ReadDouble(reader, byteOrder), ReadDouble(reader, byteOrder));
                pColl.AddPoint(p);

                if (hasZ == true && hasM == true)
                {
                    p.Z = ReadDouble(reader, byteOrder);
                    p.M = ReadDouble(reader, byteOrder);
                }
                else if (hasM == true)
                {
                    p.M = ReadDouble(reader, byteOrder);
                }
                else if (hasZ == true)
                {
                    p.Z = ReadDouble(reader, byteOrder);
                }
            }
        }

        private static void WritePointCollection(IPointCollection pColl, BinaryWriter writer, WkbByteOrder byteOrder, bool hasZ, bool hasM)
        {
            if (pColl == null)
            {
                return;
            }

            WriteUInt32((uint)pColl.PointCount, writer, byteOrder);
            for (int i = 0; i < pColl.PointCount; i++)
            {
                WritePoint(pColl[i], writer, byteOrder, hasZ, hasM);
            }
        }

        private static MultiPoint CreateMultiPoint(BinaryReader reader, WkbByteOrder byteOrder, bool hasZ, bool hasM)
        {
            // Get the number of points in this multipoint.
            int numPoints = (int)ReadUInt32(reader, byteOrder);

            // Create a new array for the points.
            MultiPoint points = new MultiPoint();

            // Loop on the number of points.
            for (int i = 0; i < numPoints; i++)
            {
                // Read point header
                reader.ReadByte();
                ReadUInt32(reader, byteOrder);

                // Create the next point and add it to the point array.
                points.AddPoint(CreatePoint(reader, byteOrder, hasZ, hasM));
            }
            return points;
        }

        private static void WriteMultiPoint(IMultiPoint mpoint, BinaryWriter writer, WkbByteOrder byteOrder, bool hasZ, bool hasM)
        {
            WritePointCollection(mpoint, writer, byteOrder, hasZ, hasM);
        }

        private static Polyline CreateLineString(BinaryReader reader, WkbByteOrder byteOrder, bool hasZ, bool hasM)
        {
            Polyline pline = new Polyline();
            gView.Framework.Geometry.Path path = new gView.Framework.Geometry.Path();
            ReadPointCollection(reader, byteOrder, path, hasZ, hasM);
            pline.AddPath(path);

            return pline;
        }

        private static void WriteLineString(IPath path, BinaryWriter writer, WkbByteOrder byteOrder, bool hasZ, bool hasM)
        {
            WritePointCollection(path, writer, byteOrder, hasZ, hasM);
        }

        private static Polyline CreateMultiLineString(BinaryReader reader, WkbByteOrder byteOrder, bool hasZ, bool hasM)
        {
            // Get the number of linestrings in this multilinestring.
            int numLineStrings = (int)ReadUInt32(reader, byteOrder);

            // Create a new array for the linestrings .
            Polyline pline = new Polyline();

            // Loop on the number of linestrings.
            for (int i = 0; i < numLineStrings; i++)
            {
                // Read linestring header
                reader.ReadByte();
                ReadUInt32(reader, byteOrder);

                Polyline p = CreateLineString(reader, byteOrder, hasZ, hasM);
                for (int r = 0; r < p.PathCount; r++)
                {
                    pline.AddPath(p[r]);
                }
            }

            // Create and return the MultiLineString.
            return pline;
        }

        private static void WriteMultiLineString(IPolyline polyline, BinaryWriter writer, WkbByteOrder byteOrder, bool hasZ, bool hasM)
        {
            WriteUInt32((uint)polyline.PathCount, writer, byteOrder);

            for (int i = 0; i < polyline.PathCount; i++)
            {
                // Header
                writer.Write((byte)byteOrder);
                writer.Write((uint)WKBGeometryType.wkbLineString);

                WritePointCollection(polyline[i], writer, byteOrder, hasZ, hasM);
            }
        }

        private static Polygon CreatePolygon(BinaryReader reader, WkbByteOrder byteOrder, bool hasZ, bool hasM)
        {
            // Get the Number of rings in this Polygon.
            int numRings = (int)ReadUInt32(reader, byteOrder);

            Polygon polygon = new Polygon();

            for (int i = 0; i < numRings; i++)
            {
                Ring ring = new Ring();
                ReadPointCollection(reader, byteOrder, ring, hasZ, hasM);
                polygon.AddRing(ring);
            }

            return polygon;
        }

        private static void WritePolygon(IPolygon polygon, BinaryWriter writer, WkbByteOrder byteOrder, bool hasZ, bool hasM)
        {
            WriteUInt32((uint)polygon.RingCount, writer, byteOrder);

            for (int i = 0; i < polygon.RingCount; i++)
            {
                WritePointCollection(polygon[i], writer, byteOrder, hasZ, hasM);
            }
        }

        private static void WriteGeometryCollection(IAggregateGeometry aGeometry, BinaryWriter writer, WkbByteOrder byteOrder, bool hasZ, bool hasM)
        {
            WriteUInt32((uint)aGeometry.GeometryCount, writer, byteOrder);

            for (int i = 0; i < aGeometry.GeometryCount; i++)
            {
                IGeometry geometry = aGeometry[i];
                if (geometry == null)
                {
                    continue;
                }

                // a reserved byte for every part... always 1?
                writer.Write((byte)1);

                if (geometry is IPoint)
                {
                    writer.Write((uint)WKBGeometryType.wkbPoint);
                    WritePoint((IPoint)geometry, writer, byteOrder, hasZ, hasM);
                }
                else if (geometry is IMultiPoint)
                {
                    writer.Write((uint)WKBGeometryType.wkbMultiPoint);
                    WriteMultiPoint((IMultiPoint)geometry, writer, byteOrder, hasZ, hasM);
                }
                else if (geometry is IPolyline)
                {
                    writer.Write((uint)WKBGeometryType.wkbMultiLineString);
                    WriteMultiLineString((IPolyline)geometry, writer, byteOrder, hasZ, hasM);
                }
                else if (geometry is IPolygon)
                {
                    writer.Write((uint)WKBGeometryType.wkbMultiPolygon);
                    WriteMultiPolygon((IPolygon)geometry, writer, byteOrder, hasZ, hasM);
                }
                else if (geometry is IAggregateGeometry)
                {
                    writer.Write((uint)WKBGeometryType.wkbGeometryCollection);
                    WriteGeometryCollection((IAggregateGeometry)geometry, writer, byteOrder, hasZ, hasM);
                }
                else
                {
                    throw new NotSupportedException("Geometry type is not supported");
                }
            }
        }

        private static Polygon CreateMultiPolygon(BinaryReader reader, WkbByteOrder byteOrder, bool hasZ, bool hasM)
        {
            int numPolygons = (int)ReadUInt32(reader, byteOrder);

            // Create a new array for the Polygons.
            Polygon polygon = new Polygon();

            // Loop on the number of polygons.
            for (int i = 0; i < numPolygons; i++)
            {
                // read polygon header
                reader.ReadByte();
                ReadUInt32(reader, byteOrder);

                // TODO: Validate type
                Polygon p = CreatePolygon(reader, byteOrder, hasZ, hasM);
                for (int r = 0; r < p.RingCount; r++)
                {
                    polygon.AddRing(p[r]);
                }
            }
            return polygon;
        }

        private static void WriteMultiPolygon(IPolygon polygon, BinaryWriter writer, WkbByteOrder byteOrder, bool hasZ, bool hasM)
        {
            //int count = 0;
            //for (int i = 0; i < polygon.RingCount; i++)
            //{
            //    if (polygon[0] == null || polygon[i].PointCount < 3) continue;
            //    count++;
            //}
            //WriteUInt32((uint)count, writer, byteOrder);

            //for (int i = 0; i < polygon.RingCount; i++)
            //{
            //    if (polygon[0] == null || polygon[i].PointCount < 3) continue;

            //    // Header
            //    writer.Write((byte)byteOrder);
            //    writer.Write((uint)WKBGeometryType.wkbPolygon);

            //    WritePolygon(polygon[i], writer, byteOrder);
            //}
            WriteUInt32((uint)1, writer, byteOrder);
            // Header
            writer.Write((byte)byteOrder);
            writer.Write((uint)WKBGeometryType.wkbPolygon);

            WritePolygon(polygon, writer, byteOrder, hasZ, hasM);
        }

        private static IAggregateGeometry CreateGeometryCollection(BinaryReader reader, WkbByteOrder byteOrder, bool hasZ, bool hasM)
        {
            // Get the Number of Geometries in this Collection.
            int numGeometries = (int)ReadUInt32(reader, byteOrder);

            AggregateGeometry aGeometry = new AggregateGeometry();

            for (int g = 0; g < numGeometries; g++)
            {
                reader.ReadByte(); // read the reseved byte => always 1?

                IGeometry geometry = WKBToGeometry(reader, byteOrder);

                if (geometry != null)
                {
                    aGeometry.AddGeometry(geometry);
                }
            }

            return aGeometry;
        }

        private static IPolyline CreateMultiCurve(BinaryReader reader, WkbByteOrder byteOrder, bool hasZ, bool hasM)
        {
            // Get the Number of Geometries in this MultiCurve.
            int numGeometries = (int)ReadUInt32(reader, byteOrder);

            Polyline polyline = new Polyline();

            for (int g = 0; g < numGeometries; g++)
            {
                IGeometry geometry = WKBToGeometry(reader, byteOrder);
                if (geometry is IPolyline)
                {
                    for (var p = 0; p < ((IPolyline)geometry).PathCount; p++)
                    {
                        polyline.AddPath(((IPolyline)geometry)[p]);
                    }
                }
            }

            return polyline;
        }

        private static uint ReadUInt32(BinaryReader reader, WkbByteOrder byteOrder)
        {
            if (byteOrder == WkbByteOrder.Xdr)
            {
                byte[] bytes = BitConverter.GetBytes(reader.ReadUInt32());
                Array.Reverse(bytes);
                return BitConverter.ToUInt32(bytes, 0);
            }
            else
            {
                return reader.ReadUInt32();
            }
        }

        private static void WriteUInt32(uint val, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            if (byteOrder == WkbByteOrder.Xdr)
            {
                byte[] bytes = BitConverter.GetBytes(val);
                Array.Reverse(bytes);
                writer.Write(bytes);
            }
            else
            {
                writer.Write(val);
            }
        }

        private static double ReadDouble(BinaryReader reader, WkbByteOrder byteOrder)
        {
            if (byteOrder == WkbByteOrder.Xdr)
            {
                byte[] bytes = BitConverter.GetBytes(reader.ReadDouble());
                Array.Reverse(bytes);
                return BitConverter.ToDouble(bytes, 0);
            }
            else
            {
                return reader.ReadDouble();
            }
        }

        private static void WriteDouble(double val, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            if (byteOrder == WkbByteOrder.Xdr)
            {
                byte[] bytes = BitConverter.GetBytes(val);
                Array.Reverse(bytes);
                writer.Write(bytes);
            }
            else
            {
                writer.Write(val);
            }
        }

        public static double ToDouble(string number)
        {
            double ret = 0D;
            try
            {
                ret = Convert.ToDouble(number, gView.Framework.OGC.OGC.numberFormat_EnUS);
            }
            catch (OverflowException)
            {
                if (number.Trim().StartsWith("-"))
                {
                    return double.MinValue;
                }

                return double.MaxValue;
            }
            catch
            {
                return 0D;
            }
            return ret;
        }
    }
}
