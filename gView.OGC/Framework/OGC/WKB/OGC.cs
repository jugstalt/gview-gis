using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using gView.Framework.Geometry;

namespace gView.Framework.OGC.WKB
{
    /*
    public class WKB
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
            wkbGeometryCollection = 7
        }

        public readonly static System.Globalization.NumberFormatInfo numberFormat_EnUS = new System.Globalization.CultureInfo("en-US", false).NumberFormat;

        public static string Envelope2box2(IEnvelope envelope) 
        {
            if (envelope == null) return "";
            string box2 = "box2d('BOX3D(" +
                            envelope.minx.ToString(numberFormat_EnUS) + " " +
                            envelope.miny.ToString(numberFormat_EnUS) + "," +
                            envelope.maxx.ToString(numberFormat_EnUS) + " " +
                            envelope.maxy.ToString(numberFormat_EnUS) + ")'::box3d)";

            return box2;
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

            // Get the type of this geometry.
            uint type = (uint)ReadUInt32(reader, (WkbByteOrder)byteOrder);

            if (!Enum.IsDefined(typeof(WKBGeometryType), type))
                throw new ArgumentException("Geometry type not recognized");

            switch ((WKBGeometryType)type)
            {
                case WKBGeometryType.wkbPoint:
                    return CreatePoint(reader, (WkbByteOrder)byteOrder);

                case WKBGeometryType.wkbLineString:
                    return CreateLineString(reader, (WkbByteOrder)byteOrder);

                case WKBGeometryType.wkbPolygon:
                    return CreatePolygon(reader, (WkbByteOrder)byteOrder);

                case WKBGeometryType.wkbMultiPoint:
                    return CreateMultiPoint(reader, (WkbByteOrder)byteOrder);

                case WKBGeometryType.wkbMultiLineString:
                    return CreateMultiLineString(reader, (WkbByteOrder)byteOrder);

                case WKBGeometryType.wkbMultiPolygon:
                    return CreateMultiPolygon(reader, (WkbByteOrder)byteOrder);

                //case WKBGeometryType.wkbGeometryCollection:
                //    return CreateGeometryCollection(reader, (WkbByteOrder)byteOrder);

                default:
                    throw new NotSupportedException("Geometry type '" + type.ToString() + "' not supported");
            }
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

        public static byte[] GeometryToWKB(IGeometry geometry, WkbByteOrder byteOrder)
        {
            MemoryStream ms = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write((byte)byteOrder);

                if (geometry is IPoint)
                {
                    writer.Write((uint)WKBGeometryType.wkbPoint);
                    WritePoint((IPoint)geometry, writer, byteOrder);
                }
                else if (geometry is IMultiPoint)
                {
                    writer.Write((uint)WKBGeometryType.wkbMultiPoint);
                    WriteMultiPoint((IMultiPoint)geometry, writer, byteOrder);
                }
                else if (geometry is IPolyline)
                {
                    writer.Write((uint)WKBGeometryType.wkbMultiLineString);
                    WriteMultiLineString((IPolyline)geometry, writer, byteOrder);
                }
                else if (geometry is IPolygon)
                {
                    writer.Write((uint)WKBGeometryType.wkbMultiPolygon);
                    WriteMultiPolygon((IPolygon)geometry, writer, byteOrder);
                }
                else
                {
                    throw new NotSupportedException("Geometry type is not supported");
                }
                ms.Position = 0;
                byte[] g = new byte[ms.Length];
                ms.Read(g, 0, g.Length);
                return g;
            }
        }

        private static Point CreatePoint(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // Create and return the point.
            return new Point(ReadDouble(reader, byteOrder), ReadDouble(reader, byteOrder));
        }

        private static void WritePoint(IPoint point, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            WriteDouble(point.X, writer, byteOrder);
            WriteDouble(point.Y, writer, byteOrder);
        }

        private static void ReadPointCollection(BinaryReader reader, WkbByteOrder byteOrder, IPointCollection pColl)
        {
            if (pColl == null) return;

            // Get the number of points in this linestring.
            int numPoints = (int)ReadUInt32(reader, byteOrder);

            // Loop on the number of points in the ring.
            for (int i = 0; i < numPoints; i++)
            {
                // Add the coordinate.
                pColl.AddPoint(new Point(ReadDouble(reader, byteOrder), ReadDouble(reader, byteOrder)));
            }
        }

        private static void WritePointCollection(IPointCollection pColl, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            if (pColl == null) return;

            WriteUInt32((uint)pColl.PointCount, writer, byteOrder);
            for (int i = 0; i < pColl.PointCount; i++)
            {
                WritePoint(pColl[i], writer, byteOrder);
            }
        }

        private static MultiPoint CreateMultiPoint(BinaryReader reader, WkbByteOrder byteOrder)
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
                points.AddPoint(CreatePoint(reader, byteOrder));
            }
            return points;
        }

        private static void WriteMultiPoint(IMultiPoint mpoint, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            WritePointCollection(mpoint, writer, byteOrder);
        }

        private static Polyline CreateLineString(BinaryReader reader, WkbByteOrder byteOrder)
        {
            Polyline pline = new Polyline();
            gView.Framework.Geometry.Path path = new gView.Framework.Geometry.Path();
            ReadPointCollection(reader, byteOrder, path);
            pline.AddPath(path);

            return pline;
        }

        private static void WriteLineString(IPath path, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            WritePointCollection(path, writer, byteOrder);
        }

        private static Polyline CreateMultiLineString(BinaryReader reader, WkbByteOrder byteOrder)
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

                Polyline p = CreateLineString(reader, byteOrder);
                for (int r = 0; r < p.PathCount; r++)
                    pline.AddPath(p[r]);
            }

            // Create and return the MultiLineString.
            return pline;
        }

        private static void WriteMultiLineString(IPolyline polyline, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            WriteUInt32((uint)polyline.PathCount, writer, byteOrder);

            for (int i = 0; i < polyline.PathCount; i++)
            {
                // Header
                writer.Write((byte)byteOrder);
                writer.Write((uint)WKBGeometryType.wkbLineString);

                WritePointCollection(polyline[i], writer, byteOrder);
            }
        }

        private static Polygon CreatePolygon(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // Get the Number of rings in this Polygon.
            int numRings = (int)ReadUInt32(reader, byteOrder);

            Polygon polygon = new Polygon();

            for (int i = 0; i < numRings; i++)
            {
                Ring ring = new Ring();
                ReadPointCollection(reader, byteOrder, ring);
                polygon.AddRing(ring);
            }

            return polygon;
        }

        private static void WritePolygon(IPolygon polygon, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            WriteUInt32((uint)polygon.RingCount, writer, byteOrder);

            for (int i = 0; i < polygon.RingCount; i++)
            {
                WritePointCollection(polygon[i], writer, byteOrder);
            }
        }

        private static Polygon CreateMultiPolygon(BinaryReader reader, WkbByteOrder byteOrder)
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
                Polygon p = CreatePolygon(reader, byteOrder);
                for (int r = 0; r < p.RingCount; r++)
                    polygon.AddRing(p[r]);
            }
            return polygon;
        }

        private static void WriteMultiPolygon(IPolygon polygon, BinaryWriter writer, WkbByteOrder byteOrder)
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

            WritePolygon(polygon, writer, byteOrder);
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
                return reader.ReadUInt32();
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
                writer.Write(val);
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
                return reader.ReadDouble();
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
                writer.Write(val);
        }
    }
     * */
}
