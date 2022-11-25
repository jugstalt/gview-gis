using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gView.Framework.Geometry
{
    /// <summary>
    /// An ordered collection of points.
    /// </summary>
    public class PointCollection : IPointCollection
    {
        protected List<IPoint> _points;
        private IEnvelope _cacheEnv = null;

        public PointCollection()
        {
            _points = new List<IPoint>();
        }
        public PointCollection(List<IPoint> points)
            : this()
        {
            _points = new List<IPoint>();
            foreach (IPoint point in points)
            {
                if (point == null)
                {
                    continue;
                }

                _points.Add(new Point(point.X, point.Y));
            }
        }
        public PointCollection(double[] x, double[] y, double[] z)
        {
            setXYZ(x, y, z);
        }
        public PointCollection(double[] xy)
        {
            _points = new List<IPoint>();
            if (xy == null)
            {
                return;
            }

            for (int i = 0; i < xy.Length - 1; i += 2)
            {
                _points.Add(new Point(xy[i], xy[i + 1]));
            }
        }
        public PointCollection(IPointCollection pColl)
        {
            if (pColl == null)
            {
                return;
            }

            _points = new List<IPoint>();
            for (int i = 0; i < pColl.PointCount; i++)
            {
                if (pColl[i] == null)
                {
                    continue;
                }

                _points.Add(new Point(pColl[i].X, pColl[i].Y, pColl[i].Z));
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
            _points = new List<IPoint>();
            if (x.Length == y.Length)
            {
                int count = x.Length;
                for (int i = 0; i < x.Length; i++)
                {
                    if (z == null)
                    {
                        _points.Add(new Point(x[i], y[i]));
                    }
                    else
                    {
                        if (z.Length < i)
                        {
                            _points.Add(new Point(x[i], y[i], z[i]));
                        }
                        else
                        {
                            _points.Add(new Point(x[i], y[i], 0.0));
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
            int count = _points.Count;
            x = new double[count];
            y = new double[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = _points[i].X;
                y[i] = _points[i].Y;
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
            int count = _points.Count;
            x = new double[count];
            y = new double[count];
            z = new double[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = _points[i].X;
                y[i] = _points[i].Y;
                z[i] = _points[i].Z;
            }
        }

        public IPoint[] ToArray(int fromIndex = 0, bool reverse = false)
        {
            if (reverse)
            {
                return ((IEnumerable<Point>)_points).Reverse().Skip(fromIndex).ToArray();
            }
            else
            {
                return _points.Skip(fromIndex).ToArray();
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
            _points.Add(point);
        }

        /// <summary>
        /// Adds a point to the collection at a given position.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="pos"></param>
        public void InsertPoint(IPoint point, int pos)
        {
            _cacheEnv = null;
            if (pos > _points.Count)
            {
                pos = _points.Count;
            }

            if (pos < 0)
            {
                pos = 0;
            }

            _points.Insert(pos, point);
        }

        public void AddPoints(IEnumerable<IPoint> points)
        {
            if (points == null)
            {
                return;
            }

            _points.AddRange(points.Where(p => p != null));

            _cacheEnv = null;
        }

        /// <summary>
        /// Removes the point at a given position.
        /// </summary>
        /// <param name="pos"></param>
        public void RemovePoint(int pos)
        {
            _cacheEnv = null;
            if (pos < 0 || pos >= _points.Count)
            {
                return;
            }

            _points.RemoveAt(pos);
        }

        public void RemovePoint(IPoint point)
        {
            _cacheEnv = null;
            _points.Remove(point);
        }

        /// <summary>
        /// The number of points in the collection
        /// </summary>
        public int PointCount
        {
            get
            {
                return _points == null ? 0 : _points.Count;
            }
        }

        /// <summary>
        /// Returns the point at the given position
        /// </summary>
        public IPoint this[int pointIndex]
        {
            get
            {
                if (pointIndex < 0 || pointIndex >= _points.Count)
                {
                    return null;
                }

                return _points[pointIndex];
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

                foreach (IPoint point in _points)
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

            w.Write(_points.Count);
            foreach (IPoint p in _points)
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
            _points.Clear();
            int points = r.ReadInt32();

            //if (points >= 0)
            //{

            for (int i = 0; i < points; i++)
            {
                Point p = new Point();
                p.Deserialize(r, geomDef);
                _points.Add(p);
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
            _points.Clear();
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
            return _points.GetEnumerator();
        }

        #endregion

        public int? Srs { get; set; }
    }
}
