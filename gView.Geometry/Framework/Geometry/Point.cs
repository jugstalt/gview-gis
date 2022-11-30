using System;
using System.IO;

namespace gView.Framework.Geometry
{
    /// <summary>
    /// A three dimensional point object
    /// </summary>
    public class Point : IPoint, ITopologicalOperation
    {
        private double m_x, m_y, m_z, m_m;

        public Point()
        {
        }
        public Point(double x, double y)
        {
            m_x = x;
            m_y = y;
            m_z = m_m = 0.0;
        }
        public Point(double x, double y, double z)
        {
            m_x = x;
            m_y = y;
            m_z = z;
            m_m = 0.0;
        }
        public Point(IPoint point)
        {
            if (point != null)
            {
                m_x = point.X;
                m_y = point.Y;
                m_z = point.Z;
                m_m = point.M;
            }
            else
            {
                m_x = m_y = m_z = m_m = 0.0;
            }
        }

        #region IPoint Member

        /// <summary>
        /// The X coordinate.
        /// </summary>
        public double X
        {
            get
            {
                return m_x;
            }
            set
            {
                m_x = value;
            }
        }

        /// <summary>
        /// The Y coordinate.
        /// </summary>
        public double Y
        {
            get
            {
                return m_y;
            }
            set
            {
                m_y = value;
            }
        }

        /// <summary>
        /// The Z coordinate or the height attribute.
        /// </summary>
        public double Z
        {
            get
            {
                return m_z;
            }
            set
            {
                m_z = value;
            }
        }

        public double M
        {
            get { return m_m; }
            set { m_m = value; }
        }

        public double Distance(IPoint p)
        {
            if (p == null)
            {
                return double.MaxValue;
            }

            return Math.Sqrt((p.X - m_x) * (p.X - m_x) + (p.Y - m_y) * (p.Y - m_y));
        }

        public double Distance2(IPoint p)
        {
            if (p == null)
            {
                return double.MaxValue;
            }

            return (p.X - m_x) * (p.X - m_x) + (p.Y - m_y) * (p.Y - m_y);
        }

        /// <summary>
        /// For the internal use of the framework
        /// </summary>
        /// <param name="w"></param>
        public void Serialize(BinaryWriter w, IGeometryDef geomDef)
        {
            w.Write(m_x);
            w.Write(m_y);
            if (geomDef.HasZ)
            {
                w.Write(m_z);
            }

            if (geomDef.HasM)
            {
                w.Write(m_m);
            }
        }
        /// <summary>
        /// For the internal use of the framework
        /// </summary>
        /// <param name="w"></param>
        public void Deserialize(BinaryReader r, IGeometryDef geomDef)
        {
            m_x = r.ReadDouble();
            m_y = r.ReadDouble();
            if (geomDef.HasZ)
            {
                m_z = r.ReadDouble();
            }

            if (geomDef.HasM)
            {
                m_m = r.ReadDouble();
            }
        }

        public void Clean(CleanGemetryMethods methods, double tolerance = 1e-8)
        {
            // Nothing to do for points
        }

        public bool IsEmpty() => false;

        #endregion

        #region IGeometry Member

        /// <summary>
        /// The type of the geometry (Point)
        /// </summary>
        public gView.Framework.Geometry.GeometryType GeometryType
        {
            get
            {
                return GeometryType.Point;
            }
        }

        /// <summary>
        /// Creates a copy of this geometry's envelope and returns it.
        /// </summary>
        public IEnvelope Envelope
        {
            get
            {
                return new Envelope(m_x, m_y, m_x, m_y);
            }
        }

        public int? Srs { get; set; }

        public int VertexCount => 1;

        #endregion

        #region ITopologicalOperation Member

        public IPolygon Buffer(double distance)
        {
            IPolygon buffer = gView.Framework.SpatialAlgorithms.Algorithm.PointBuffer(this, distance);
            return buffer;
        }

        public void Clip(IEnvelope clipper)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Intersect(IGeometry geometry)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Difference(IGeometry geometry)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SymDifference(IGeometry geometry)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Union(IGeometry geometry)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Clip(IEnvelope clipper, out IGeometry result)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Intersect(IGeometry geometry, out IGeometry result)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Difference(IGeometry geometry, out IGeometry result)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SymDifference(IGeometry geometry, out IGeometry result)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Union(IGeometry geometry, out IGeometry result)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region ICloneable Member

        public object Clone()
        {
            return new Point(m_x, m_y, m_z);
        }

        #endregion

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj, 0.0);
        }
        public bool Equals(object obj, double epsi)
        {
            if (obj is IPoint)
            {
                IPoint point = (IPoint)obj;
                return Math.Abs(point.X - m_x) <= epsi &&
                       Math.Abs(point.Y - m_y) <= epsi &&
                       Math.Abs(point.Z - m_z) <= epsi;
            }
            return false;
        }

        public double Distance2D(IPoint p)
        {
            if (p == null)
            {
                return double.MaxValue;
            }

            return Math.Sqrt((p.X - m_x) * (p.X - m_x) + (p.Y - m_y) * (p.Y - m_y));
        }
    }
}
