using System.Data.SqlTypes;
using System.IO;
using gView.Framework.Core.Geometry;

namespace gView.Framework.Geometry
{
    internal class GeometrySerialization
    {
        internal delegate void serializeDelegate(BinaryWriter bw, IGeometryDef gd);

        static public SqlBytes GeometryToSqlBytes(IGeometry geometry)
        {
            GeometryDef geomDef = new GeometryDef();
            geomDef.HasZ = geomDef.HasM = false;

            serializeDelegate serialize = null;

            if (geometry != null && geometry.GeometryType != GeometryType.Unknown)
            {
                geomDef.GeometryType = geometry.GeometryType;
                serialize = geometry.Serialize;
            }

            if (serialize != null)
            {
                using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
                {
                    writer.Write((int)geomDef.GeometryType);
                    serialize(writer, geomDef);

                    byte[] bytes = new byte[writer.BaseStream.Length];
                    writer.BaseStream.Position = 0;
                    writer.BaseStream.ReadExactly(bytes, (int)0, (int)writer.BaseStream.Length);
                    writer.Close();

                    return new SqlBytes(bytes);
                }
            }
            return null;
        }

        static public IGeometry SqlBytesToGeometry(SqlBytes sqlbytes)
        {
            try
            {
                GeometryDef geomDef = new GeometryDef();
                geomDef.HasZ = geomDef.HasM = false;


                using (BinaryReader r = new BinaryReader(new MemoryStream()))
                {
                    r.BaseStream.Write(sqlbytes.Buffer, 0, sqlbytes.Buffer.Length);
                    r.BaseStream.Position = 0;

                    switch ((GeometryType)r.ReadInt32())
                    {
                        case GeometryType.Aggregate:
                            AggregateGeometry ageom = new AggregateGeometry();
                            ageom.Deserialize(r, geomDef);
                            return ageom;
                        case GeometryType.Envelope:
                            Envelope env = new Envelope();
                            env.Deserialize(r, geomDef);
                            return env;
                        case GeometryType.Multipoint:
                            MultiPoint mp = new MultiPoint();
                            mp.Deserialize(r, geomDef);
                            return mp;
                        case GeometryType.Point:
                            Point p = new Point();
                            p.Deserialize(r, geomDef);
                            return p;
                        case GeometryType.Polygon:
                            Polygon polygon = new Polygon();
                            polygon.Deserialize(r, geomDef);
                            return polygon;
                        case GeometryType.Polyline:
                            Polyline line = new Polyline();
                            line.Deserialize(r, geomDef);
                            return line;
                        default:
                            return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
