using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

//[assembly: InternalsVisibleTo("gView.OGC, PublicKey=0024000004800000940000000602000000240000525341310004000001000100916d0be3f662c2d3589fbe93479f3215e23fd195db9a20e77f42dc1d2942bd48cad3ea36b797f57880e6c31af0c238d2e445898c8ecce990aacbb70ae05a10aff73ab65c7db86366697f934b780238ed8fd1b2e28ba679a97e060b53fce66118e129b91d24f392d4dd3d482fa4173e61f18c74cda9f35721a97e77afbbc96dd2")]


namespace gView.Framework.Geometry
{
    /// <summary>
    /// An ordered collection of points.
    /// </summary>
    public class PointCollection : IPointCollection
    {
        protected List<IPoint> m_points;
        private IEnvelope _cacheEnv = null;

        public PointCollection()
        {
            m_points = new List<IPoint>();
        }
        public PointCollection(List<IPoint> points)
            : this()
        {
            m_points = new List<IPoint>();
            foreach (IPoint point in points)
            {
                if (point == null)
                {
                    continue;
                }

                m_points.Add(new Point(point.X, point.Y));
            }
        }
        public PointCollection(double[] x, double[] y, double[] z)
        {
            setXYZ(x, y, z);
        }
        public PointCollection(double[] xy)
        {
            m_points = new List<IPoint>();
            if (xy == null)
            {
                return;
            }

            for (int i = 0; i < xy.Length - 1; i += 2)
            {
                m_points.Add(new Point(xy[i], xy[i + 1]));
            }
        }
        public PointCollection(IPointCollection pColl)
        {
            if (pColl == null)
            {
                return;
            }

            m_points = new List<IPoint>();
            for (int i = 0; i < pColl.PointCount; i++)
            {
                if (pColl[i] == null)
                {
                    continue;
                }

                m_points.Add(new Point(pColl[i].X, pColl[i].Y, pColl[i].Z));
            }
        }

        /// <summary>
        /// Add points with X,Y and Z coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        internal void setXYZ(double[] x, double[] y, double[] z)
        {
            m_points = new List<IPoint>();
            if (x.Length == y.Length)
            {
                int count = x.Length;
                for (int i = 0; i < x.Length; i++)
                {
                    if (z == null)
                    {
                        m_points.Add(new Point(x[i], y[i]));
                    }
                    else
                    {
                        if (z.Length < i)
                        {
                            m_points.Add(new Point(x[i], y[i], z[i]));
                        }
                        else
                        {
                            m_points.Add(new Point(x[i], y[i], 0.0));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the X and Y coordinates of the points.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        internal void getXY(out double[] x, out double[] y)
        {
            int count = m_points.Count;
            x = new double[count];
            y = new double[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = m_points[i].X;
                y[i] = m_points[i].Y;
            }
        }

        /// <summary>
        /// Returns the X, Y and Z coordinates of the points
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        internal void getXYZ(out double[] x, out double[] y, out double[] z)
        {
            int count = m_points.Count;
            x = new double[count];
            y = new double[count];
            z = new double[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = m_points[i].X;
                y[i] = m_points[i].Y;
                z[i] = m_points[i].Z;
            }
        }

        public IPoint[] ToArray(int fromIndex = 0, bool reverse = false)
        {
            if (reverse)
            {
                return ((IEnumerable<Point>)m_points).Reverse().Skip(fromIndex).ToArray();
            }
            else
            {
                return m_points.Skip(fromIndex).ToArray();
            }
        }

        public void Dispose()
        {

        }

        #region IPointCollection Member

        /// <summary>
        /// Adds a point to the collection
        /// </summary>
        /// <param name="point"></param>
        public void AddPoint(IPoint point)
        {
            _cacheEnv = null;
            m_points.Add(point);
        }

        /// <summary>
        /// Adds a point to the collection at a given position.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="pos"></param>
        public void InsertPoint(IPoint point, int pos)
        {
            _cacheEnv = null;
            if (pos > m_points.Count)
            {
                pos = m_points.Count;
            }

            if (pos < 0)
            {
                pos = 0;
            }

            m_points.Insert(pos, point);
        }

        public void AddPoints(IEnumerable<IPoint> points)
        {
            if (points == null)
            {
                return;
            }

            m_points.AddRange(points.Where(p => p != null));

            _cacheEnv = null;
        }

        /// <summary>
        /// Removes the point at a given position.
        /// </summary>
        /// <param name="pos"></param>
        public void RemovePoint(int pos)
        {
            _cacheEnv = null;
            if (pos < 0 || pos >= m_points.Count)
            {
                return;
            }

            m_points.RemoveAt(pos);
        }

        public void RemovePoint(IPoint point)
        {
            _cacheEnv = null;
            m_points.Remove(point);
        }

        /// <summary>
        /// The number of points in the collection
        /// </summary>
        public int PointCount
        {
            get
            {
                return m_points == null ? 0 : m_points.Count;
            }
        }

        /// <summary>
        /// Returns the point at the given position
        /// </summary>
        public IPoint this[int pointIndex]
        {
            get
            {
                if (pointIndex < 0 || pointIndex >= m_points.Count)
                {
                    return null;
                }

                return m_points[pointIndex];
            }
        }

        /// <summary>
        /// Creates a copy of this geometry's envelope and returns it.
        /// </summary>
        public IEnvelope Envelope
        {
            get
            {
                if (_cacheEnv != null)
                {
                    return _cacheEnv;
                }

                if (PointCount == 0)
                {
                    return null;
                }

                bool first = true;
                double minx = 0, miny = 0, maxx = 0, maxy = 0;

                foreach (IPoint point in m_points)
                {
                    if (first)
                    {
                        minx = maxx = point.X;
                        miny = maxy = point.Y;
                        first = false;
                    }
                    else
                    {
                        minx = Math.Min(minx, point.X);
                        miny = Math.Min(miny, point.Y);
                        maxx = Math.Max(maxx, point.X);
                        maxy = Math.Max(maxy, point.Y);
                    }
                }
                _cacheEnv = new Envelope(minx, miny, maxx, maxy);
                return new Envelope(_cacheEnv);
            }
        }


        /// <summary>
        /// For the internal use of the framework
        /// </summary>
        /// <param name="w"></param>
        public void Serialize(BinaryWriter w, IGeometryDef geomDef)
        {
            //w.Write((System.Int32)(-m_points.Count));

            //for (int i = 0; i < m_points.Count; i++)
            //{
            //    if (i == 0)
            //    {
            //        m_points[0].Serialize(w, geomDef);
            //    }
            //    else
            //    {
            //        double dx = (m_points[i].X - m_points[i - 1].X) * 1000.0;
            //        double dy = (m_points[i].Y - m_points[i - 1].Y) * 1000.0;
            //        w.Write(gView.Geometry.Compress.ByteCompressor.Compress((int)dx));
            //        w.Write(gView.Geometry.Compress.ByteCompressor.Compress((int)dy));
            //    }
            //}

            w.Write(m_points.Count);
            foreach (IPoint p in m_points)
            {
                p.Serialize(w, geomDef);
            }
        }
        /// <summary>
        /// For the internal use of the framework
        /// </summary>
        /// <param name="w"></param>
        public void Deserialize(BinaryReader r, IGeometryDef geomDef)
        {
            m_points.Clear();
            int points = r.ReadInt32();

            //if (points >= 0)
            //{

            for (int i = 0; i < points; i++)
            {
                Point p = new Point();
                p.Deserialize(r, geomDef);
                m_points.Add(p);
            }

            //}
            //else
            //{
            //    points = -points;
            //    Point p = new Point();
            //    p.Deserialize(r, geomDef);
            //    m_points.Add(p);

            //    for (int i = 1; i < points; i++)
            //    {
            //        short dx = r.ReadInt16();
            //        short dy = r.ReadInt16();
            //        p = new Point(m_points[i - 1].X + (double)dx / 1000.0, 
            //                      m_points[i - 1].Y + (double)dy / 1000.0);
            //        m_points.Add(p);
            //    }
            //}
        }
        #endregion

        public void RemoveAllPoints()
        {
            m_points.Clear();
        }

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
            if (!(obj is IPointCollection))
            {
                return false;
            }

            IPointCollection pColl = (IPointCollection)obj;
            if (pColl.PointCount != this.PointCount)
            {
                return false;
            }

            for (int i = 0; i < this.PointCount; i++)
            {
                IPoint p1 = this[i];
                IPoint p2 = pColl[i];

                if (!p1.Equals(p2, epsi))
                {
                    return false;
                }
            }

            return true;
        }

        #region IEnumerable<IPoint> Members

        public IEnumerator<IPoint> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_points.GetEnumerator();
        }

        #endregion

        public int? Srs { get; set; }
    }
}
