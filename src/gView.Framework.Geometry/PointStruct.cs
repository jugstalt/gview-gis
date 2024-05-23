using System;
using System.IO;
using gView.Framework.Core.Geometry;

namespace gView.Framework.Geometry
{
    public struct PointStruct : IPoint
    {
        public PointStruct(double x, double y)
           => (X, Y) = (x, y);

        public PointStruct(IPoint point)
           => (X, Y, Z, M, Srs) = (
                point?.X ?? 0D, 
                point?.Y ?? 0D, 
                point?.Z ?? 0D, 
                point?.M ?? 0D, 
                point?.Srs
            );

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double M { get; set; }

        public GeometryType GeometryType => GeometryType.Point;

        public IEnvelope Envelope => new Envelope(X, Y, X, Y);

        public int? Srs { get; set; }

        public int VertexCount => 1;

        public void Clean(CleanGemetryMethods methods, double tolerance = 1E-08)
        {
            // Nothing to do for points
        }

        public object Clone()
        {
            return new PointStruct() { X = this.X, Y = this.Y, Z = this.Z, M = this.M, Srs = this.Srs };
        }

        public void Deserialize(BinaryReader r, IGeometryDef geomDef)
        {
            X = r.ReadDouble();
            Y = r.ReadDouble();
            if (geomDef.HasZ)
            {
                Z = r.ReadDouble();
            }

            if (geomDef.HasM)
            {
                M = r.ReadDouble();
            }
        }

        public double Distance(IPoint p)
        {
            if (p == null)
            {
                return double.MaxValue;
            }

            return Math.Sqrt((p.X - X) * (p.X - X) + (p.Y - Y) * (p.Y - Y));
        }

        public double Distance2(IPoint p)
        {
            if (p == null)
            {
                return double.MaxValue;
            }

            return (p.X - X) * (p.X - X) + (p.Y - Y) * (p.Y - Y);
        }

        public double Distance2D(IPoint p)
        {
            if (p == null)
            {
                return double.MaxValue;
            }

            return Math.Sqrt((p.X - X) * (p.X - X) + (p.Y - Y) * (p.Y - Y));
        }

        public bool Equals(object obj, double epsi)
        {
            if (obj is IPoint)
            {
                IPoint point = (IPoint)obj;
                return Math.Abs(point.X - X) <= epsi &&
                       Math.Abs(point.Y - Y) <= epsi &&
                       Math.Abs(point.Z - Z) <= epsi;
            }
            return false;
        }

        public bool IsEmpty() => false;

        public void Serialize(BinaryWriter w, IGeometryDef geomDef)
        {
            w.Write(X);
            w.Write(Y);
            if (geomDef.HasZ)
            {
                w.Write(Z);
            }

            if (geomDef.HasM)
            {
                w.Write(M);
            }
        }
    }
}
