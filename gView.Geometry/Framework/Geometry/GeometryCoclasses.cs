using gView.Framework.SpatialAlgorithms;
using gView.Framework.SpatialAlgorithms.Clipper;
using gView.Framework.system;
using Proj4Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

//[assembly: InternalsVisibleTo("gView.OGC, PublicKey=0024000004800000940000000602000000240000525341310004000001000100916d0be3f662c2d3589fbe93479f3215e23fd195db9a20e77f42dc1d2942bd48cad3ea36b797f57880e6c31af0c238d2e445898c8ecce990aacbb70ae05a10aff73ab65c7db86366697f934b780238ed8fd1b2e28ba679a97e060b53fce66118e129b91d24f392d4dd3d482fa4173e61f18c74cda9f35721a97e77afbbc96dd2")]


namespace gView.Framework.Geometry
{
    /// <summary>
    /// A Rectange with side paralell to the coordinate system axes.
    /// </summary>
    public sealed class Envelope : IEnvelope, IGeometry
    {
        private double m_minx = 0.0, m_miny = 0.0, m_maxx = 0.0, m_maxy = 0.0;

        public Envelope()
        {
            m_minx = m_miny = m_maxx = m_maxy = 0.0;
        }

        public Envelope(double minX, double minY, double maxX, double maxY)
        {
            m_minx = Math.Min(minX, maxX);
            m_miny = Math.Min(minY, maxY);
            m_maxx = Math.Max(maxX, minX);
            m_maxy = Math.Max(maxY, minY);
        }

        public Envelope(IEnvelope env)
        {
            if (env == null)
            {
                return;
            }

            m_minx = env.minx;
            m_miny = env.miny;
            m_maxx = env.maxx;
            m_maxy = env.maxy;
        }

        public Envelope(XmlNode env)
        {
            if (env == null)
            {
                return;
            }

            minx = Convert.ToDouble(env.Attributes["minx"].Value.Replace(".", ","));
            miny = Convert.ToDouble(env.Attributes["miny"].Value.Replace(".", ","));
            maxx = Convert.ToDouble(env.Attributes["maxx"].Value.Replace(".", ","));
            maxy = Convert.ToDouble(env.Attributes["maxy"].Value.Replace(".", ","));
        }

        public Envelope(IPoint lowerLeft, IPoint upperRight)
        {
            m_minx = Math.Min(lowerLeft.X, upperRight.X);
            m_miny = Math.Min(lowerLeft.Y, upperRight.Y);

            m_maxx = Math.Max(lowerLeft.X, upperRight.X);
            m_maxy = Math.Max(lowerLeft.Y, upperRight.Y);
        }



        /// <summary>
        /// The position of the left side
        /// </summary>
        public double minx
        {
            get { return m_minx; }
            set { m_minx = value; }
        }

        /// <summary>
        /// The position of the bottom side
        /// </summary>
        public double miny
        {
            get { return m_miny; }
            set { m_miny = value; }
        }

        /// <summary>
        /// The position of the right side
        /// </summary>
        public double maxx
        {
            get { return m_maxx; }
            set { m_maxx = value; }
        }

        /// <summary>
        /// The position of the top side
        /// </summary>
        public double maxy
        {
            get { return m_maxy; }
            set { m_maxy = value; }
        }

        public IPoint LowerLeft
        {
            get { return new Point(minx, miny); }
            set
            {
                if (value == null)
                {
                    return;
                }

                m_minx = value.X;
                m_miny = value.Y;
            }
        }
        public IPoint LowerRight
        {
            get { return new Point(maxx, miny); }
            set
            {
                if (value == null)
                {
                    return;
                }

                m_maxx = value.X;
                m_miny = value.Y;
            }
        }
        public IPoint UpperRight
        {
            get { return new Point(maxx, maxy); }
            set
            {
                if (value == null)
                {
                    return;
                }

                m_maxx = value.X;
                m_maxy = value.Y;
            }
        }
        public IPoint UpperLeft
        {
            get { return new Point(minx, maxy); }
            set
            {
                if (value == null)
                {
                    return;
                }

                m_minx = value.X;
                m_maxy = value.Y;
            }
        }
        public IPoint Center
        {
            get
            {
                return new Point(minx * 0.5 + maxx * 0.5, miny * 0.5 + maxy * 0.5);
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                IPoint c = this.Center;
                double dx = value.X - c.X;
                double dy = value.Y - c.Y;

                m_minx += dx; m_maxx += dx;
                m_miny += dy; m_maxy += dy;
            }
        }
        private void CheckMinMax()
        {
            double minx = m_minx, miny = m_miny;
            double maxx = m_maxx, maxy = m_maxy;

            m_minx = Math.Min(minx, maxx);
            m_miny = Math.Min(miny, maxy);
            m_maxx = Math.Max(minx, maxx);
            m_maxy = Math.Max(miny, maxy);
        }

        public Envelope MakeValid()
        {
            if (!double.IsNaN(m_minx) &&
               !double.IsNaN(m_miny) &&
               !double.IsNaN(m_maxx) &&
               !double.IsNaN(m_maxy) &&
               !double.IsInfinity(m_minx) &&
               !double.IsInfinity(m_miny) &&
               !double.IsInfinity(m_maxx) &&
               !double.IsInfinity(m_maxy))
            {
                return this;
            }

            double minx = m_minx, miny = m_miny, maxx = m_maxx, maxy = m_maxy;

            if ((double.IsNaN(minx) || double.IsInfinity(minx)) &&
                !(double.IsNaN(maxx) && double.IsInfinity(maxx)))
            {
                minx = maxx - 0.1;
            }

            if ((double.IsNaN(miny) || double.IsInfinity(miny)) &&
                !(double.IsNaN(maxy) && double.IsInfinity(maxy)))
            {
                miny = maxy - 0.1;
            }

            if ((double.IsNaN(maxx) || double.IsInfinity(maxx)) &&
                !(double.IsNaN(minx) && double.IsInfinity(minx)))
            {
                maxx = minx + 0.1;
            }

            if ((double.IsNaN(maxy) || double.IsInfinity(maxy)) &&
                !(double.IsNaN(miny) && double.IsInfinity(miny)))
            {
                maxy = miny + 0.1;
            }

            if (!double.IsNaN(minx) &&
               !double.IsNaN(miny) &&
               !double.IsNaN(maxx) &&
               !double.IsNaN(maxy) &&
               !double.IsInfinity(minx) &&
               !double.IsInfinity(miny) &&
               !double.IsInfinity(maxx) &&
               !double.IsInfinity(maxy))
            {
                return new Envelope(minx, miny, maxx, maxy);
            }

            return null;
        }

        /// <summary>
        /// Sets the Envelope equal to the union of the base Envelope and the input Envelope
        /// </summary>
        /// <param name="envelope">An object that implements <c>IEnvelope</c></param>
        public void Union(IEnvelope envelope)
        {
            if (envelope == null)
            {
                return;
            }

            m_minx = Math.Min(m_minx, envelope.minx);
            m_miny = Math.Min(m_miny, envelope.miny);
            m_maxx = Math.Max(m_maxx, envelope.maxx);
            m_maxy = Math.Max(m_maxy, envelope.maxy);
        }

        public void Raise(double percent)
        {
            percent /= 100.0;
            double w = Math.Abs(m_maxx - m_minx);
            double h = Math.Abs(m_maxy - m_miny);

            w = (w * percent - w) / 2;
            h = (h * percent - h) / 2;

            m_minx -= w;
            m_miny -= h;
            m_maxx += w;
            m_maxy += h;
        }

        public void Raise(IPoint point, double percent)
        {
            if (point == null)
            {
                return;
            }

            percent /= 100;
            double w1 = point.X - minx, w2 = maxx - point.X;
            double h1 = point.Y - miny, h2 = maxy - point.Y;

            w1 = w1 * percent; w2 = w2 * percent;
            h1 = h1 * percent; h2 = h2 * percent;

            m_minx = point.X - w1;
            m_miny = point.Y - h1;
            m_maxx = point.X + w2;
            m_maxy = point.Y + h2;
        }

        public bool Intersects(IEnvelope envelope)
        {
            if (envelope.maxx >= m_minx &&
                envelope.minx <= m_maxx &&
                envelope.maxy >= m_miny &&
                envelope.miny <= m_maxy)
            {
                return true;
            }

            return false;
        }

        public bool Contains(IEnvelope envelope)
        {
            if (envelope.minx < m_minx
                || envelope.maxx > m_maxx)
            {
                return false;
            }

            if (envelope.miny < m_miny
                || envelope.maxy > m_maxy)
            {
                return false;
            }

            return true;
        }

        public bool Contains(IPoint point)
        {
            return point.X >= m_minx &&
                   point.X <= m_maxx &&
                   point.Y >= m_miny &&
                   point.Y <= m_maxy;
        }

        #region IGeometry Member

        /// <summary>
        /// The type of the geometry (Envelope)
        /// </summary>
        public gView.Framework.Geometry.geometryType GeometryType
        {
            get
            {
                return geometryType.Envelope;
            }
        }

        /// <summary>
        /// Creates a copy of this geometry's envelope and returns it.
        /// </summary>
        IEnvelope gView.Framework.Geometry.IGeometry.Envelope
        {
            get
            {
                return new Envelope(this);
            }
        }

        public int? Srs { get; set; }

        #endregion

        public IPolygon ToPolygon(int accuracy = 0)
        {
            if (accuracy < 0)
            {
                accuracy = 0;
            }

            double stepx = this.Width / (accuracy + 1);
            double stepy = this.Height / (accuracy + 1);

            IPolygon polygon = new Polygon();
            polygon.AddRing(new Ring());

            polygon[0].AddPoint(new Point(m_minx, m_miny));
            for (int i = 0; i < accuracy; i++)
            {
                polygon[0].AddPoint(new Point(m_minx, m_miny + stepy * (i + 1)));
            }

            polygon[0].AddPoint(new Point(m_minx, m_maxy));
            for (int i = 0; i < accuracy; i++)
            {
                polygon[0].AddPoint(new Point(m_minx + stepx * (i + 1), m_maxy));
            }

            polygon[0].AddPoint(new Point(m_maxx, m_maxy));
            for (int i = 0; i < accuracy; i++)
            {
                polygon[0].AddPoint(new Point(m_maxx, m_maxy - stepy * (i + 1)));
            }

            polygon[0].AddPoint(new Point(m_maxx, m_miny));
            for (int i = 0; i < accuracy; i++)
            {
                polygon[0].AddPoint(new Point(m_maxx - stepx * (i + 1), m_miny));
            }

            polygon[0].AddPoint(new Point(m_minx, m_miny));

            return polygon;
        }

        public IPointCollection ToPointCollection(int accuracy)
        {
            if (accuracy < 0)
            {
                accuracy = 0;
            }

            double stepx = this.Width / (accuracy + 1);
            double stepy = this.Height / (accuracy + 1);

            PointCollection pColl = new PointCollection();
            for (int y = 0; y <= accuracy + 1; y++)
            {
                for (int x = 0; x <= accuracy + 1; x++)
                {
                    pColl.AddPoint(new Point(this.minx + stepx * x, this.miny + stepy * y));
                }
            }
            return pColl;
        }
        public static double[] minBounds(IEnvelope e)
        {

            double[] min = new double[4];
            min[0] = e.minx;
            min[1] = e.miny;
            min[2] = min[3] = 0;
            return min;

        }
        public static double[] maxBounds(IEnvelope e)
        {

            double[] max = new double[4];
            max[0] = e.maxx;
            max[1] = e.maxy;
            max[2] = max[3] = 0;
            return max;

        }

        #region IEnvelope Member


        public double Width
        {
            get { return Math.Abs(m_maxx - m_minx); }
        }

        public double Height
        {
            get { return Math.Abs(m_maxy - m_miny); }
        }

        public void Translate(double mx, double my)
        {
            m_minx += mx;
            m_miny += my;
            m_maxx += mx;
            m_maxy += my;
        }

        public void TranslateTo(double mx, double my)
        {
            double cx = m_minx * 0.5 + m_maxx * 0.5;
            double cy = m_miny * 0.5 + m_maxy * 0.5;

            Translate(mx - cx, my - cy);
        }

        public string ToBBoxString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(m_minx);
            sb.Append(",");
            sb.Append(m_miny);
            sb.Append(",");
            sb.Append(m_maxx);
            sb.Append(",");
            sb.Append(m_maxy);

            return sb.ToString();
        }
        #endregion

        public override bool Equals(object obj)
        {
            return Equals(obj, 0.0);
        }
        public bool Equals(object obj, double epsi)
        {
            if (!(obj is IEnvelope))
            {
                return false;
            }

            IEnvelope env2 = (IEnvelope)obj;
            return
                Math.Abs(minx - env2.minx) <= epsi &&
                Math.Abs(miny - env2.miny) <= epsi &&
                Math.Abs(maxx - env2.maxx) <= epsi &&
                Math.Abs(maxy - env2.maxy) <= epsi;
        }

        #region ICloneable Member

        public object Clone()
        {
            return new Envelope(this);
        }

        #endregion

        /// <summary>
        /// For the internal use of the framework
        /// </summary>
        /// <param name="w"></param>
        public void Serialize(BinaryWriter w, IGeometryDef geomDef)
        {
            w.Write(m_minx);
            w.Write(m_miny);
            w.Write(m_maxx);
            w.Write(m_maxy);
            if (geomDef.HasZ)
            {
                w.Write(0);
            }
        }
        /// <summary>
        /// For the internal use of the framework
        /// </summary>
        /// <param name="w"></param>
        public void Deserialize(BinaryReader r, IGeometryDef geomDef)
        {
            m_minx = r.ReadDouble();
            m_miny = r.ReadDouble();
            m_maxx = r.ReadDouble();
            m_maxy = r.ReadDouble();
            if (geomDef.HasZ)
            {
                r.ReadDouble();
            }
        }

        #region Static Members
        public static bool IsNull(IEnvelope envelope)
        {
            if (envelope == null)
            {
                return true;
            }

            if (Math.Abs(envelope.minx) < double.Epsilon &&
                Math.Abs(envelope.miny) < double.Epsilon &&
                Math.Abs(envelope.maxx) < double.Epsilon &&
                Math.Abs(envelope.maxy) < double.Epsilon)
            {
                return true;
            }

            if (double.IsNaN(envelope.minx) ||
                double.IsNaN(envelope.miny) ||
                double.IsNaN(envelope.maxx) ||
                double.IsNaN(envelope.maxy))
            {
                return true;
            }

            return false;
        }
        #endregion
    }

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
        #endregion

        #region IGeometry Member

        /// <summary>
        /// The type of the geometry (Point)
        /// </summary>
        public gView.Framework.Geometry.geometryType GeometryType
        {
            get
            {
                return geometryType.Point;
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
    }

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

        public void AddPoints(IPointCollection pColl)
        {
            if (pColl == null)
            {
                return;
            }

            for (int i = 0, to = pColl.PointCount; i < to; i++)
            {
                if (pColl[i] == null)
                {
                    continue;
                }

                m_points.Add(pColl[i]);
            }

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
                return m_points.Count;
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

    /// <summary>
    /// A orderd collection of points.
    /// </summary>
    public sealed class MultiPoint : PointCollection, IMultiPoint, ITopologicalOperation
    {
        public MultiPoint()
            : base()
        {
        }
        public MultiPoint(List<IPoint> points)
            : base(points)
        {
        }
        public MultiPoint(IPointCollection pColl)
            : base(pColl)
        {
        }
        #region IGeometry Member

        /// <summary>
        /// The type of the geometry (Multipoint)
        /// </summary>
        public gView.Framework.Geometry.geometryType GeometryType
        {
            get
            {
                return geometryType.Multipoint;
            }
        }

        #endregion

        #region ICloneable Member

        public object Clone()
        {
            return new MultiPoint(m_points);
        }

        #endregion

        #region ITopologicalOperation Member

        public IPolygon Buffer(double distance)
        {
            if (distance <= 0.0)
            {
                return null;
            }

            List<IPolygon> polygons = new List<IPolygon>();

            for (int i = 0; i < this.PointCount; i++)
            {
                IPolygon buffer = gView.Framework.SpatialAlgorithms.Algorithm.PointBuffer(this[i], distance);
                if (buffer == null)
                {
                    continue;
                }

                polygons.Add(buffer);
            }
            //return gView.Framework.SpatialAlgorithms.Algorithm.MergePolygons(polygons);
            return gView.Framework.SpatialAlgorithms.Algorithm.FastMergePolygon(polygons, null, null);
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
    }

    /// <summary>
    /// A sequence of connected segments.
    /// </summary>
    public class Path : PointCollection, IPath, ICloneable
    {
        public Path()
            : base()
        {
        }
        public Path(List<IPoint> points)
            : base(points)
        {
        }
        public Path(IPointCollection pColl)
            : base(pColl)
        {
        }

        #region IPath Member

        /// <summary>
        /// The length of the curve.
        /// </summary>
        public double Length
        {
            get
            {
                if (m_points.Count < 2)
                {
                    return 0.0;
                }

                double len = 0.0;
                for (int i = 1; i < m_points.Count; i++)
                {
                    len += Math.Sqrt(
                        (this[i - 1].X - this[i].X) * (this[i - 1].X - this[i].X) +
                        (this[i - 1].Y - this[i].Y) * (this[i - 1].Y - this[i].Y));
                }
                return len;
            }
        }

        public void ClosePath()
        {
            if (this.PointCount < 3)
            {
                return;
            }

            IPoint ps = this[0];
            IPoint pe = this[this.PointCount - 1];

            if (ps == null || pe == null ||
                ps.Equals(pe, 1e-10))
            {
                return;
            }

            this.AddPoint(new Point(ps));
        }

        public void ChangeDirection()
        {
            m_points = ListOperations<IPoint>.Swap(m_points);
        }
        public IPath Trim(double length)
        {
            Path trim = new Path();
            if (m_points.Count == 0)
            {
                return trim;
            }

            IPoint prePoint = new Point(m_points[0]);
            trim.AddPoint(prePoint);

            double len = 0.0;
            for (int i = 1; i < m_points.Count; i++)
            {
                IPoint point = m_points[i];
                double l = prePoint.Distance(point);
                if (len + l > length)
                {
                    double dx = point.X - prePoint.X;
                    double dy = point.Y - prePoint.Y;
                    double dz = point.Z - prePoint.Z;
                    point.X = prePoint.X + (length - len) / l * dx;
                    point.Y = prePoint.Y + (length - len) / l * dy;
                    point.Z = prePoint.Z + (length - len) / l * dz;
                    trim.AddPoint(point);
                    break;
                }
                else
                {
                    trim.AddPoint(point);
                }
                len += l;
                prePoint = point;
            }
            return trim;
        }
        #endregion

        #region ICloneable Member

        public virtual object Clone()
        {
            return new Path(m_points);
        }

        #endregion

        public void Close()
        {
            if (m_points.Count < 3)
            {
                return;
            }

            if (!m_points[0].Equals(m_points[m_points.Count - 1]))
            {
                m_points.Add(new Point(m_points[0]));
            }
        }
    }

    /// <summary>
    /// An area bounded by one, closed sequence of connected segments.
    /// </summary>
    public class Ring : Path, IRing
    {
        public Ring()
            : base()
        {
        }
        public Ring(List<IPoint> points)
            : base(points)
        {
        }

        public Ring(IPointCollection pointCollection)
            : base(pointCollection)
        {

        }

        internal Ring(IRing ring)
            : base()
        {
            for (int i = 0; i < ring.PointCount; i++)
            {
                this.AddPoint(new Point(ring[i].X, ring[i].Y, ring[i].Z));
            }
        }

        #region IRingMember

        /// <summary>
        /// A area of the ring. (Not implemented!)
        /// </summary>
        public double Area
        {
            get
            {
                return Math.Abs(RealArea);
            }
        }

        private double RealArea
        {
            get
            {
                /*
                 * var F=0,max=shape_vertexX.length;
		            if(max<3) return 0;
		            for(var i=0;i<max;i++) {
			            var y1=(i==max-1) ? shape_vertexY[0]     : shape_vertexY[i+1];
			            var y2=(i==0)     ? shape_vertexY[max-1] : shape_vertexY[i-1];
			            F+=0.5*shape_vertexX[i]*(y1-y2);	
		        }
                 * */
                if (PointCount < 3)
                {
                    return 0.0;
                }

                int max = PointCount;

                double A = 0.0;
                for (int i = 0; i < max; i++)
                {
                    double y1 = (i == max - 1) ? this[0].Y : this[i + 1].Y;
                    double y2 = (i == 0) ? this[max - 1].Y : this[i - 1].Y;

                    A += 0.5 * this[i].X * (y1 - y2);
                }
                return A;
            }
        }
        public IPoint Centroid
        {
            get
            {
                double cx = 0, cy = 0, A = RealArea;
                if (A == 0.0)
                {
                    return null;
                }

                int to = PointCount;
                if (this[0].X != this[to - 1].X ||
                    this[0].Y != this[to - 1].Y)
                {
                    to += 1;
                }
                IPoint p0 = this[0], p1;
                for (int i = 1; i < to; i++)
                {
                    p1 = (i < PointCount) ? this[i] : this[0];
                    double h = (p0.X * p1.Y - p1.X * p0.Y);
                    cx += (p0.X + p1.X) * h / 6.0;
                    cy += (p0.Y + p1.Y) * h / 6.0;
                    p0 = p1;
                }
                return new Point(cx / A, cy / A);
            }
        }
        #endregion

        public override object Clone()
        {
            return new Ring(m_points);
        }
    }

    public class Hole : Ring, IHole
    {
        public Hole()
            : base()
        {
        }
        public Hole(List<IPoint> points)
            : base(points)
        {
        }

        internal Hole(IRing ring)
            : base(ring)
        {
        }

        public override object Clone()
        {
            return new Hole(m_points);
        }
    }

    /// <summary>
    /// An orderd collection of paths.
    /// </summary>
    public sealed class Polyline : IPolyline, ITopologicalOperation
    {
        private List<IPath> _paths;

        public Polyline()
        {
            _paths = new List<IPath>();
        }
        public Polyline(IPath path)
            : this()
        {
            _paths.Add(path);
        }
        public Polyline(List<IPath> paths)
            : this()
        {
            if (paths != null)
            {
                foreach (Path path in paths)
                {
                    if (path != null)
                    {
                        _paths.Add(path);
                    }
                }
            }
        }
        public Polyline(IPolygon polygon)
            : this()
        {
            if (polygon == null)
            {
                return;
            }

            for (int i = 0; i < polygon.RingCount; i++)
            {
                IRing ring = polygon[i];
                if (ring == null)
                {
                    continue;
                }

                Path path = new Path(ring);
                path.Close();
                _paths.Add(path);
            }
        }

        #region IPolyline Member

        /// <summary>
        /// Adds a path.
        /// </summary>
        /// <param name="path"></param>
        public void AddPath(IPath path)
        {
            if (path == null)
            {
                return;
            }

            _paths.Add(path);
        }

        /// <summary>
        /// Adds a path at a given position.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pos"></param>
        public void InsertPath(IPath path, int pos)
        {
            if (path == null)
            {
                return;
            }

            if (pos > _paths.Count)
            {
                pos = _paths.Count;
            }

            if (pos < 0)
            {
                pos = 0;
            }

            _paths.Insert(pos, path);
        }

        /// <summary>
        /// Removes the path at a given position (index).
        /// </summary>
        /// <param name="pos"></param>
        public void RemovePath(int pos)
        {
            if (pos < 0 || pos >= _paths.Count)
            {
                return;
            }

            _paths.RemoveAt(pos);
        }

        /// <summary>
        /// The number of paths.
        /// </summary>
        public int PathCount
        {
            get
            {
                return _paths.Count;
            }
        }

        /// <summary>
        /// Returns the path at the given position (index).
        /// </summary>
        public IPath this[int pathIndex]
        {
            get
            {
                if (pathIndex < 0 || pathIndex >= _paths.Count)
                {
                    return null;
                }

                return _paths[pathIndex];
            }
        }

        public double Length
        {
            get
            {
                if (_paths == null || _paths.Count == 0)
                {
                    return 0D;
                }

                double len = 0D;
                foreach (var path in _paths)
                {
                    if (path != null)
                    {
                        len += path.Length;
                    }
                }

                return len;
            }
        }

        #endregion

        #region IGeometry Member

        /// <summary>
        /// The type of the geometry (Polyline)
        /// </summary>
        public gView.Framework.Geometry.geometryType GeometryType
        {
            get
            {
                return geometryType.Polyline;
            }
        }


        /// <summary>
        /// Creates a copy of this geometry's envelope and returns it.
        /// </summary>
        public IEnvelope Envelope
        {
            get
            {
                if (PathCount == 0)
                {
                    return null;
                }

                IEnvelope env = this[0].Envelope;
                for (int i = 1; i < PathCount; i++)
                {
                    env.Union(this[i].Envelope);
                }
                return env;
            }
        }


        /// <summary>
        /// For the internal use of the framework
        /// </summary>
        /// <param name="w"></param>
        public void Serialize(BinaryWriter w, IGeometryDef geomDef)
        {
            w.Write(_paths.Count);
            foreach (IPath path in _paths)
            {
                path.Serialize(w, geomDef);
            }
        }
        /// <summary>
        /// For the internal use of the framework
        /// </summary>
        /// <param name="w"></param>
        public void Deserialize(BinaryReader r, IGeometryDef geomDef)
        {
            int paths = r.ReadInt32();
            for (int i = 0; i < paths; i++)
            {
                Path path = new Path();
                path.Deserialize(r, geomDef);
                _paths.Add(path);
            }
        }

        public int? Srs { get; set; }

        #endregion

        private void CopyGeometry(IPolyline polyline)
        {
            if (polyline == null)
            {
                return;
            }

            _paths.Clear();

            for (int i = 0; i < polyline.PathCount; i++)
            {
                _paths.Add(polyline[i]);
            }
        }

        #region ITopologicalOperation Member

        public IPolygon Buffer(double distance)
        {
            IPolygon buffer = gView.Framework.SpatialAlgorithms.Algorithm.PolylineBuffer(this, distance);
            return buffer;
        }

        public void Clip(IEnvelope clipper)
        {
            IGeometry result;
            Clip(clipper, out result);
            CopyGeometry((IPolyline)result);
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
            IGeometry polyline = gView.Framework.SpatialAlgorithms.Clip.PerformClip(clipper, this);
            if (!(polyline is IPolyline))
            {
                polyline = null;
            }

            result = polyline;
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
            Polyline pLine = new Polyline();
            foreach (IPath path in _paths)
            {
                if (path == null)
                {
                    continue;
                }

                pLine.AddPath(path.Clone() as IPath);
            }
            return pLine;
        }

        #endregion

        public override bool Equals(object obj)
        {
            return Equals(obj, 0.0);
        }
        public bool Equals(object obj, double epsi)
        {
            if (obj is IPolyline)
            {
                IPolyline polyline = (IPolyline)obj;
                if (polyline.PathCount != this.PathCount)
                {
                    return false;
                }

                for (int i = 0; i < this.PathCount; i++)
                {
                    IPath p1 = this[i];
                    IPath p2 = polyline[i];

                    if (!p1.Equals(p2, epsi))
                    {
                        return false;
                    }
                }

                return true;
            }
            return false;
        }

        #region IEnumerable<IPath> Members

        public IEnumerator<IPath> GetEnumerator()
        {
            return _paths.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _paths.GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// An ordered collection of rings.
    /// </summary>
    public sealed class Polygon : IPolygon, ITopologicalOperation
    {
        private List<IRing> _rings;
        private int _ringsChecked;

        public Polygon()
        {
            _rings = new List<IRing>();
            _ringsChecked = 0;
        }
        public Polygon(IRing ring)
            : this()
        {
            _rings.Add(ring);
        }
        public Polygon(IPolygon polygon)
            : this()
        {
            if (polygon == null)
            {
                return;
            }

            for (int i = 0; i < polygon.RingCount; i++)
            {
                if (polygon[i] == null)
                {
                    continue;
                }

                this.AddRing(new Ring(polygon[i]));
            }
        }
        #region IPolygon Member

        /// <summary>
        /// Adds a ring.
        /// </summary>
        /// <param name="ring"></param>
        public void AddRing(IRing ring)
        {
            if (ring == null)
            {
                return;
            }

            _rings.Add(ring);

            _ringsChecked = -1;
        }

        /// <summary>
        /// Adds a ring at the given position (index).
        /// </summary>
        /// <param name="ring"></param>
        /// <param name="pos"></param>
        public void InsertRing(IRing ring, int pos)
        {
            if (ring == null)
            {
                return;
            }

            if (pos > _rings.Count)
            {
                pos = _rings.Count;
            }

            if (pos < 0)
            {
                pos = 0;
            }

            _rings.Insert(pos, ring);
        }

        /// <summary>
        /// Removes a ring at the given position
        /// </summary>
        /// <param name="pos"></param>
        public void RemoveRing(int pos)
        {
            if (pos < 0 || pos >= _rings.Count)
            {
                return;
            }

            _rings.RemoveAt(pos);
        }

        /// <summary>
        /// The number of rings.
        /// </summary>
        public int RingCount
        {
            get
            {
                return _rings.Count;
            }
        }

        /// <summary>
        /// The ring at the given position.
        /// </summary>
        public IRing this[int ringIndex]
        {
            get
            {
                if (ringIndex < 0 || ringIndex >= _rings.Count)
                {
                    return null;
                }

                return _rings[ringIndex];
            }
        }

        public double Area
        {
            get
            {
                //
                // Hier sollte getestet werden, welche ringe löcher sind und welche nicht...
                //
                VerifyHoles();

                double A = 0.0;
                for (int i = 0; i < RingCount; i++)
                {
                    double a = this[i].Area;
                    if (this[i] is IHole)
                    {
                        A -= a;
                    }
                    else
                    {
                        A += a;
                    }
                }
                return A;
            }
        }



        #endregion

        #region IGeometry Member

        /// <summary>
        /// The type of the geometry (Polygon).
        /// </summary>
        public gView.Framework.Geometry.geometryType GeometryType
        {
            get
            {
                return geometryType.Polygon;
            }
        }

        /// <summary>
        /// Creates a copy of this geometry's envelope and returns it.
        /// </summary>
        public IEnvelope Envelope
        {
            get
            {
                if (RingCount == 0)
                {
                    return null;
                }

                IEnvelope env = this[0].Envelope;
                for (int i = 1; i < RingCount; i++)
                {
                    env.Union(this[i].Envelope);
                }
                return env;
            }
        }


        /// <summary>
        /// For the internal use of the framework
        /// </summary>
        /// <param name="w"></param>
        public void Serialize(BinaryWriter w, IGeometryDef geomDef)
        {
            w.Write(_rings.Count);
            foreach (IRing ring in _rings)
            {
                ring.Serialize(w, geomDef);
            }
        }
        /// <summary>
        /// For the internal use of the framework
        /// </summary>
        /// <param name="w"></param>
        public void Deserialize(BinaryReader r, IGeometryDef geomDef)
        {
            int rings = r.ReadInt32();
            for (int i = 0; i < rings; i++)
            {
                Ring ring = new Ring();
                ring.Deserialize(r, geomDef);
                _rings.Add(ring);
            }
        }

        public int? Srs { get; set; }

        #endregion

        // 
        // Ringe werden aus anderen polygonfeature übernommen...
        // nicht wirklich kopiert
        //
        private void CopyGeometry(IPolygon polygon)
        {
            if (polygon == null)
            {
                return;
            }

            _rings.Clear();

            for (int i = 0; i < polygon.RingCount; i++)
            {
                _rings.Add(polygon[i]);
            }
        }

        internal void SortRings()
        {
            _rings.Sort(new RingComparerArea());
        }
        internal void SortRingsInv()
        {
            _rings.Sort(new RingComparerAreaInv());
        }
        public void VerifyHoles()
        {
            if (_ringsChecked == _rings.Count)
            {
                return;
            }

            if (_rings.Count == 0 || _rings.Count == 1)
            {
                _ringsChecked = _rings.Count;
                return;
            }

            List<IRing> v = _rings;
            _rings = new List<IRing>();

            v.Sort(new RingComparerAreaInv());
            foreach (IRing ring in v)
            {
                bool hole = false;
                foreach (IRing P in _rings)
                {
                    if (SpatialAlgorithms.Algorithm.Jordan(P, ring))
                    {
                        hole = !(P is IHole);
                    }
                }
                if (!hole)
                {
                    if (ring is IHole)
                    {
                        _rings.Add(new Ring(ring));
                    }
                    else
                    {
                        _rings.Add(ring);
                    }
                }
                else
                {
                    if (ring is IHole)
                    {
                        _rings.Add(ring);
                    }
                    else
                    {
                        _rings.Add(new Hole(ring));
                    }
                }
            }

            _ringsChecked = _rings.Count;
        }

        internal int OuterRingCount
        {
            get
            {
                VerifyHoles();

                int counter = 0;
                for (int i = 0; i < this.RingCount; i++)
                {
                    IRing r = this[i];
                    if (r == null || r is IHole)
                    {
                        continue;
                    }

                    counter++;
                }
                return counter;
            }
        }
        internal int InnerRingCount
        {
            get
            {
                VerifyHoles();

                int counter = 0;
                for (int i = 0; i < this.RingCount; i++)
                {
                    IRing r = this[i];
                    if (r == null || !(r is IHole))
                    {
                        continue;
                    }

                    counter++;
                }
                return counter;
            }
        }

        public double CalcArea()
        {
            VerifyHoles();

            double area = 0.0;
            foreach (IRing ring in _rings)
            {
                if (ring == null)
                {
                    continue;
                }

                if (ring is IHole)
                {
                    area -= ring.Area;
                }
                else
                {
                    area += ring.Area;
                }
            }
            return area;
        }

        #region ITopologicalOperation Member

        public IPolygon Buffer(double distance)
        {
            VerifyHoles();
            IPolygon buffer = gView.Framework.SpatialAlgorithms.Algorithm.PolygonBuffer(this, distance);
            return buffer;
        }

        public void Clip(IEnvelope clipper)
        {
            IGeometry result;
            Clip(clipper, out result);
            CopyGeometry((IPolygon)result);
        }

        public void Intersect(IGeometry geometry)
        {
            IGeometry result;
            Intersect(geometry, out result);
            CopyGeometry((IPolygon)result);
        }

        public void Difference(IGeometry geometry)
        {
            IGeometry result;
            Intersect(geometry, out result);
            CopyGeometry((IPolygon)result);
        }

        public void SymDifference(IGeometry geometry)
        {
            IGeometry result;
            Intersect(geometry, out result);
            CopyGeometry((IPolygon)result);
        }

        public void Union(IGeometry geometry)
        {
            if (geometry is IPolygon)
            {
                CopyGeometry(new List<IPolygon>(new IPolygon[] { this, (IPolygon)geometry }).Merge());
            }
        }

        public void Clip(IEnvelope clipper, out IGeometry result)
        {
            VerifyHoles();
            IGeometry polygon = gView.Framework.SpatialAlgorithms.Clip.PerformClip(clipper, this);
            if (!(polygon is IPolygon))
            {
                polygon = null;
            }

            result = polygon;
        }

        public void Intersect(IGeometry geometry, out IGeometry result)
        {
            VerifyHoles();
            IGeometry polygon = gView.Framework.SpatialAlgorithms.Clip.PerformClip(geometry, this);
            if (!(polygon is IPolygon))
            {
                polygon = null;
            }

            result = polygon;
        }

        //public void Difference(IGeometry geometry, out IGeometry result)
        //{
        //    VerifyHoles();
        //    GeomPolygon geomPolygon = null;
        //    if (geometry is IPolygon)
        //    {
        //        geomPolygon = new GeomPolygon((IPolygon)geometry);
        //    }
        //    else if (geometry is IEnvelope)
        //    {
        //        geomPolygon = new GeomPolygon((IEnvelope)geometry);
        //    }
        //    else
        //    {
        //        result = null;
        //        return;
        //    }

        //    GeomPolygon thisPolygon = new GeomPolygon(this);

        //    GeomPolygon res = thisPolygon.Clip(ClipOperation.Difference, geomPolygon);
        //    result = res.ToPolygon();
        //}

        //public void SymDifference(IGeometry geometry, out IGeometry result)
        //{
        //    VerifyHoles();
        //    GeomPolygon geomPolygon = null;
        //    if (geometry is IPolygon)
        //    {
        //        geomPolygon = new GeomPolygon((IPolygon)geometry);
        //    }
        //    else if (geometry is IEnvelope)
        //    {
        //        geomPolygon = new GeomPolygon((IEnvelope)geometry);
        //    }
        //    else
        //    {
        //        result = null;
        //        return;
        //    }
        //    GeomPolygon thisPolygon = new GeomPolygon(this);

        //    GeomPolygon res = thisPolygon.Clip(ClipOperation.XOr, geomPolygon);
        //    result = res.ToPolygon();
        //}

        //public void Union(IGeometry geometry, out IGeometry result)
        //{
        //    VerifyHoles();
        //    GeomPolygon geomPolygon = null;
        //    if (geometry is IPolygon)
        //    {
        //        geomPolygon = new GeomPolygon((IPolygon)geometry);
        //    }
        //    else if (geometry is IEnvelope)
        //    {
        //        geomPolygon = new GeomPolygon((IEnvelope)geometry);
        //    }
        //    else
        //    {
        //        result = null;
        //        return;
        //    }
        //    GeomPolygon thisPolygon = new GeomPolygon(this);

        //    GeomPolygon res = thisPolygon.Clip(ClipOperation.Union, geomPolygon);
        //    result = res.ToPolygon();
        //}

        #endregion

        #region ICloneable Member

        public object Clone()
        {
            Polygon poly = new Polygon();
            foreach (IRing ring in _rings)
            {
                if (ring == null)
                {
                    continue;
                }

                poly.AddRing(ring.Clone() as IRing);
            }
            return poly;
        }

        #endregion

        public override bool Equals(object obj)
        {
            return Equals(obj, 0.0);
        }
        public bool Equals(object obj, double epsi)
        {
            if (obj is IPolygon)
            {
                IPolygon polygon = (IPolygon)obj;
                if (polygon.RingCount != this.RingCount)
                {
                    return false;
                }

                for (int i = 0; i < this.RingCount; i++)
                {
                    IRing r1 = this[i];
                    IRing r2 = polygon[i];

                    if (!r1.Equals(r2, epsi))
                    {
                        return false;
                    }
                }

                return true;
            }
            return false;
        }

        #region IEnumerable<IRing> Members

        public IEnumerator<IRing> GetEnumerator()
        {
            return _rings.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _rings.GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// An orderd collection of geometry objects.
    /// </summary>
    public sealed class AggregateGeometry : IAggregateGeometry, ITopologicalOperation
    {
        private List<IGeometry> m_geoms;

        public AggregateGeometry()
        {
            m_geoms = new List<IGeometry>();
        }

        #region IAggregateGeometry Member

        /// <summary>
        /// Adds a geometry.
        /// </summary>
        /// <param name="geometry"></param>
        public void AddGeometry(IGeometry geometry)
        {
            if (geometry == null)
            {
                return;
            }

            m_geoms.Add(geometry);
        }

        /// <summary>
        /// Adds a geometry at the given position.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="pos"></param>
        public void InsertGeometry(IGeometry geometry, int pos)
        {
            if (geometry == null)
            {
                return;
            }

            if (pos > m_geoms.Count)
            {
                pos = m_geoms.Count;
            }

            if (pos < 0)
            {
                pos = 0;
            }

            m_geoms.Insert(pos, geometry);
        }

        /// <summary>
        /// Removes the geometry at the given position. 
        /// </summary>
        /// <param name="pos"></param>
        public void RemoveGeometry(int pos)
        {
            if (pos < 0 || pos >= m_geoms.Count)
            {
                return;
            }

            m_geoms.RemoveAt(pos);
        }

        /// <summary>
        /// The number of geometry objects.
        /// </summary>
        public int GeometryCount
        {
            get
            {
                return m_geoms.Count;
            }
        }

        /// <summary>
        /// Returns the geometry object at the given position.
        /// </summary>
        public IGeometry this[int geometryIndex]
        {
            get
            {
                if (geometryIndex < 0 || geometryIndex >= m_geoms.Count)
                {
                    return null;
                }

                return m_geoms[geometryIndex];
            }
        }

        public List<IPoint> PointGeometries
        {
            get
            {
                List<IPoint> points = new List<IPoint>();

                foreach (IGeometry geom in m_geoms)
                {
                    if (geom is IPoint)
                    {
                        points.Add(geom as IPoint);
                    }
                    else if (geom is IMultiPoint)
                    {
                        for (int i = 0; i < ((IMultiPoint)geom).PointCount; i++)
                        {
                            if (((IMultiPoint)geom)[i] == null)
                            {
                                continue;
                            }

                            points.Add(((IMultiPoint)geom)[i]);
                        }
                    }
                    else if (geom is IAggregateGeometry)
                    {
                        foreach (IPoint point in ((IAggregateGeometry)geom).PointGeometries)
                        {
                            points.Add(point);
                        }
                    }
                }
                return points;
            }
        }

        public IMultiPoint MergedPointGeometries
        {
            get
            {
                MultiPoint mPoint = new MultiPoint();

                foreach (IPoint point in this.PointGeometries)
                {
                    mPoint.AddPoint(point);
                }

                return mPoint;
            }
        }

        public List<IPolyline> PolylineGeometries
        {
            get
            {
                List<IPolyline> polylines = new List<IPolyline>();
                foreach (IGeometry geom in m_geoms)
                {
                    if (geom is IPolyline)
                    {
                        polylines.Add(geom as IPolyline);
                    }
                    else if (geom is IAggregateGeometry)
                    {
                        foreach (IPolyline polyline in ((IAggregateGeometry)geom).PolylineGeometries)
                        {
                            polylines.Add(polyline);
                        }
                    }
                }
                return polylines;
            }
        }

        public IPolyline MergedPolylineGeometries
        {
            get
            {
                List<IPolyline> polylines = this.PolylineGeometries;
                if (polylines == null || polylines.Count == 0)
                {
                    return null;
                }

                Polyline mPolyline = new Polyline();
                foreach (IPolyline polyline in polylines)
                {
                    for (int i = 0; i < polyline.PathCount; i++)
                    {
                        if (polyline[i] == null)
                        {
                            continue;
                        }

                        mPolyline.AddPath(polyline[i]);
                    }
                }

                return mPolyline;
            }
        }

        public List<IPolygon> PolygonGeometries
        {
            get
            {
                List<IPolygon> polygons = new List<IPolygon>();
                foreach (IGeometry geom in m_geoms)
                {
                    if (geom is IPolygon)
                    {
                        polygons.Add(geom as IPolygon);
                    }
                    else if (geom is IAggregateGeometry)
                    {
                        foreach (IPolygon polygon in ((IAggregateGeometry)geom).PolygonGeometries)
                        {
                            polygons.Add(polygon);
                        }
                    }
                }
                return polygons;
            }
        }
        public IPolygon MergedPolygonGeometries
        {
            get
            {
                List<IPolygon> polygons = this.PolygonGeometries;
                if (polygons == null || polygons.Count == 0)
                {
                    return null;
                }

                return SpatialAlgorithms.Algorithm.MergePolygons(polygons);
            }
        }
        #endregion

        #region IGeometry Member

        /// <summary>
        /// The type of the geometry.
        /// </summary>
        public gView.Framework.Geometry.geometryType GeometryType
        {
            get
            {
                return geometryType.Aggregate;
            }
        }

        /// <summary>
        /// Creates a copy of this geometry's envelope and returns it.
        /// </summary>
        public IEnvelope Envelope
        {
            get
            {
                if (GeometryCount == 0)
                {
                    return null;
                }

                IEnvelope env = this[0].Envelope;
                for (int i = 1; i < GeometryCount; i++)
                {
                    env.Union(this[i].Envelope);
                }
                return env;
            }
        }


        /// <summary>
        /// For the internal use of the framework
        /// </summary>
        /// <param name="w"></param>
        public void Serialize(BinaryWriter w, IGeometryDef geomDef)
        {
            w.Write(m_geoms.Count);
            foreach (IGeometry geom in m_geoms)
            {
                w.Write((System.Int32)geom.GeometryType);
                geom.Serialize(w, geomDef);
            }
        }
        /// <summary>
        /// For the internal use of the framework
        /// </summary>
        /// <param name="w"></param>
        public void Deserialize(BinaryReader r, IGeometryDef geomDef)
        {
            m_geoms.Clear();

            int geoms = r.ReadInt32();
            for (int i = 0; i < geoms; i++)
            {
                IGeometry geom = null;
                switch ((geometryType)r.ReadInt32())
                {
                    case geometryType.Aggregate:
                        geom = new AggregateGeometry();
                        break;
                    case geometryType.Envelope:
                        geom = new Envelope();
                        break;
                    case geometryType.Multipoint:
                        geom = new MultiPoint();
                        break;
                    case geometryType.Point:
                        geom = new Point();
                        break;
                    case geometryType.Polygon:
                        geom = new Polygon();
                        break;
                    case geometryType.Polyline:
                        geom = new Polyline();
                        break;
                }
                if (geom != null)
                {
                    geom.Deserialize(r, geomDef);
                    m_geoms.Add(geom);
                }
                else
                {
                    break;
                }
            }
        }

        public int? Srs { get; set; }

        #endregion

        #region ICloneable Member

        public object Clone()
        {

            AggregateGeometry aggregate = new AggregateGeometry();
            foreach (IGeometry geom in m_geoms)
            {
                if (geom == null)
                {
                    continue;
                }

                aggregate.AddGeometry(geom.Clone() as IGeometry);
            }
            return aggregate;
        }

        #endregion

        #region ITopologicalOperation Member

        public IPolygon Buffer(double distance)
        {
            IMultiPoint mPoint = this.MergedPointGeometries;
            IPolyline mPolyline = this.MergedPolylineGeometries;
            IPolygon mPolygon = this.MergedPolygonGeometries;

            List<IPolygon> polygons = new List<IPolygon>();
            if (mPoint != null)
            {
                polygons.Add(((ITopologicalOperation)mPoint).Buffer(distance));
            }

            if (mPolyline != null)
            {
                polygons.Add(((ITopologicalOperation)mPolyline).Buffer(distance));
            }

            if (mPolygon != null)
            {
                polygons.Add(((ITopologicalOperation)mPolygon).Buffer(distance));
            }

            if (polygons.Count == 0)
            {
                return null;
            }

            if (polygons.Count == 1)
            {
                return polygons[0];
            }

            return SpatialAlgorithms.Algorithm.MergePolygons(polygons);
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

        public override bool Equals(object obj)
        {
            return Equals(obj, 0.0);
        }
        public bool Equals(object obj, double epsi)
        {
            if (obj is IAggregateGeometry)
            {
                IAggregateGeometry aGeometry = (IAggregateGeometry)obj;
                if (aGeometry.GeometryCount != this.GeometryCount)
                {
                    return false;
                }

                for (int i = 0; i < this.GeometryCount; i++)
                {
                    IGeometry g1 = this[i];
                    IGeometry g2 = aGeometry[i];

                    if (!g1.Equals(g2, epsi))
                    {
                        return false;
                    }
                }

                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #region IEnumerable<IGeometry> Members

        public IEnumerator<IGeometry> GetEnumerator()
        {
            return m_geoms.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_geoms.GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// Performs transformation between two spatial referernces.
    /// </summary>
    public sealed class GeometricTransformer : gView.Framework.Geometry.IGeometricTransformer, IDisposable
    {
        private CoordinateReferenceSystem _fromSrs = null, _toSrs = null;
        private bool _toProjective = true, _fromProjective = true;

        private const double RAD2DEG = (180.0 / Math.PI);
        //static private object lockThis = new object();

        private CoordinateReferenceSystemFactory _factory = new CoordinateReferenceSystemFactory();
        private ISpatialReference _fromSRef = null, _toSRef = null;

        #region IGeometricTransformer Member

        private ISpatialReference FromSpatialReference
        {
            get
            {
                return _fromSRef;
            }
            set
            {
                _fromSRef = value;
                _fromSrs = _factory.CreateFromParameters("from", allParameters(value));
                _fromProjective = (value.SpatialParameters.IsGeographic == false);
            }
        }

        private ISpatialReference ToSpatialReference
        {
            get
            {
                return _toSRef;
            }
            set
            {
                _toSRef = value;
                _toSrs = _factory.CreateFromParameters("to", allParameters(value));
                _toProjective = (value.SpatialParameters.IsGeographic == false);
            }
        }

        public void SetSpatialReferences(ISpatialReference from, ISpatialReference to)
        {
            if (from == null)
            {
                this.FromSpatialReference = from;
            }

            if (to == null)
            {
                this.ToSpatialReference = to;
            }

            if ((from != null && from.Datum == null) && (to != null && to.Datum != null))
            {
                ISpatialReference toSRef = (ISpatialReference)to.Clone();
                toSRef.Datum = null;
                this.ToSpatialReference = toSRef;
                this.FromSpatialReference = from;
            }
            else if ((from != null && from.Datum != null) && (to != null && to.Datum == null))
            {
                this.ToSpatialReference = to;
                ISpatialReference fromSRef = (ISpatialReference)from.Clone();
                fromSRef.Datum = null;
                this.FromSpatialReference = fromSRef;
            }
            else
            {
                this.ToSpatialReference = to;
                this.FromSpatialReference = from;
            }
        }

        private string[] allParameters(ISpatialReference sRef)
        {
            if (sRef == null)
            {
                return "".Split();
            }

            if (sRef.Datum == null)
            {
                return sRef.Parameters;
            }

            string parameters = "";
            foreach (string param in sRef.Parameters)
            {
                parameters += param + " ";
            }
            parameters += sRef.Datum.Parameter;

            return parameters.Split(' ');
        }
        private string allParametersString(ISpatialReference sRef)
        {
            if (sRef == null)
            {
                return String.Empty;
            }

            StringBuilder sb = new StringBuilder();
            foreach (string param in sRef.Parameters)
            {
                if (sb.Length > 0)
                {
                    sb.Append(" ");
                }

                sb.Append(param.Trim());
            }
            if (sRef.Datum != null && !String.IsNullOrEmpty(sRef.Datum.Parameter))
            {
                if (sb.Length > 0)
                {
                    sb.Append(" ");
                }

                sb.Append(sRef.Datum.Parameter);
            }
            return sb.ToString();
        }
        private bool ParametersContain(ISpatialReference sRef, string p)
        {
            foreach (string param in sRef.Parameters)
            {
                if (param == p)
                {
                    return true;
                }
            }
            return false;
        }

        public object Transform2D(object geometry)
        {
            return Transform2D_(geometry, _fromSrs, _toSrs, _fromProjective, _toProjective);
        }
        public object InvTransform2D(object geometry)
        {
            return Transform2D_(geometry, _toSrs, _fromSrs, _toProjective, _fromProjective);
        }

        private object Transform2D_(object geometry, CoordinateReferenceSystem from, CoordinateReferenceSystem to, bool fromProjective, bool toProjektive)
        {
            if (geometry == null)
            {
                return null;
            }

            if (from == null || to == null)
            {
                return geometry;
            }

            if (geometry is PointCollection)
            {
                IPointCollection pColl = (IPointCollection)geometry;
                int pointCount = pColl.PointCount;
                if (pointCount == 0)
                {
                    return geometry;
                }

                IPointCollection target = null;

                var projectionPipeline = ProjectionPipeline(from, to);

                for (int p = 0, p_to = projectionPipeline.Length; p < p_to - 1; p++)
                {
                    BasicCoordinateTransform t = new BasicCoordinateTransform(projectionPipeline[p], projectionPipeline[p + 1]);

                    if (pColl is IRing)
                    {
                        target = new Ring();
                    }
                    else if (pColl is IPath)
                    {
                        target = new Path();
                    }
                    else if (pColl is IMultiPoint)
                    {
                        target = new MultiPoint();
                    }
                    else
                    {
                        target = new PointCollection();
                    }

                    for (int i = 0; i < pointCount; i++)
                    {
                        ProjCoordinate cFrom = new ProjCoordinate(pColl[i].X, pColl[i].Y), cTo = new ProjCoordinate();
                        t.Transform(cFrom, cTo);
                        target.AddPoint(new Point(cTo.X, cTo.Y));
                    }

                    pColl = target;
                }
                return target;
            }
            if (geometry is IPoint)
            {
                IPoint target = null;
                var projectionPipeline = ProjectionPipeline(from, to);
                for (int p = 0, p_to = projectionPipeline.Length; p < p_to - 1; p++)
                {
                    BasicCoordinateTransform t = new BasicCoordinateTransform(projectionPipeline[p], projectionPipeline[p + 1]);

                    ProjCoordinate cFrom = new ProjCoordinate(((IPoint)geometry).X, ((IPoint)geometry).Y), cTo = new ProjCoordinate();
                    t.Transform(cFrom, cTo);
                    target = new Point(cTo.X, cTo.Y);

                    geometry = target;
                }
                return target;
            }
            if (geometry is IEnvelope)
            {
                return Transform2D_(((IEnvelope)geometry).ToPolygon(10), from, to, fromProjective, toProjektive);
            }
            if (geometry is IPolyline)
            {
                int count = ((IPolyline)geometry).PathCount;
                IPolyline polyline = new Polyline();
                for (int i = 0; i < count; i++)
                {
                    polyline.AddPath((IPath)Transform2D_(((IPolyline)geometry)[i], from, to, fromProjective, toProjektive));
                }
                return polyline;
            }
            if (geometry is IPolygon)
            {
                int count = ((IPolygon)geometry).RingCount;
                IPolygon polygon = new Polygon();
                for (int i = 0; i < count; i++)
                {
                    polygon.AddRing((IRing)Transform2D_(((IPolygon)geometry)[i], from, to, fromProjective, toProjektive));
                }
                return polygon;
            }

            if (geometry is IAggregateGeometry)
            {
                int count = ((IAggregateGeometry)geometry).GeometryCount;
                IAggregateGeometry aGeom = new AggregateGeometry();
                for (int i = 0; i < count; i++)
                {
                    aGeom.AddGeometry((IGeometry)Transform2D_(((IAggregateGeometry)geometry)[i], from, to, fromProjective, toProjektive));
                }
                return aGeom;
            }

            return null;
        }

        private CoordinateReferenceSystem[] ProjectionPipeline(CoordinateReferenceSystem from, CoordinateReferenceSystem to)
        {
            //
            //  Proj4net berücksichtigt nadgrids=@null nicht, wodurch bei Koordinatensystem mit diesem Parametern ein Fehler im Hochwert entsteht!
            //  Workaround: Zuerst nach WGS84 projezieren und dann weiter...
            //
            if (from.Parameters.Contains("+nadgrids=@null") || to.Parameters.Contains("+nadgrids=@null"))
            {
                var wgs84 = _factory.CreateFromParameters("epsg:4326", "+proj=longlat +ellps=WGS84 +datum=WGS84 +towgs84=0,0,0,0,0,0,0");

                if (!IsEqual(wgs84, from) && !IsEqual(wgs84, to))
                {
                    return new CoordinateReferenceSystem[]
                    {
                        from,
                        wgs84,
                        to
                    };
                }
            }

            return new CoordinateReferenceSystem[] { from, to };
        }

        private bool IsEqual(CoordinateReferenceSystem c1, CoordinateReferenceSystem c2)
        {
            if (c1.Parameters.Length != c2.Parameters.Length)
            {
                return false;
            }

            foreach (var p in c1.Parameters)
            {
                if (!c2.Parameters.Contains(p))
                {
                    return false;
                }
            }

            return true;
        }

        private void ToDeg(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                return;
            }

            for (int i = 0; i < x.Length; i++)
            {
                x[i] *= RAD2DEG;
                y[i] *= RAD2DEG;
            }

        }
        private void ToDeg(IntPtr buffer, int count)
        {
            unsafe
            {
                double* b = (double*)buffer;
                for (int i = 0; i < count; i++)
                {
                    b[i] = b[i] * RAD2DEG;
                }
            }
        }

        //private void ToRad(double[] x, double[] y)
        //{
        //    if (x.Length != y.Length) return;

        //    for (int i = 0; i < x.Length; i++)
        //    {
        //        x[i] = Math.Min(180, Math.Max(-180, x[i]));
        //        y[i] = Math.Min(90, Math.Max(-90, y[i]));

        //        x[i] /= RAD2DEG;
        //        y[i] /= RAD2DEG;
        //    }
        //}
        //private void ToRad(IntPtr buffer, int count)
        //{
        //    unsafe
        //    {
        //        double* b = (double*)buffer;
        //        for (int i = 0; i < count; i++)
        //        {
        //            b[i] = Math.Min(180, Math.Max(-180, b[i]));
        //            b[i] /= RAD2DEG;
        //        }
        //    }
        //}

        public void Release()
        {

        }

        #endregion

        static public IGeometry Transform2D(IGeometry geometry, ISpatialReference from, ISpatialReference to)
        {
            if (geometry == null)
            {
                return null;
            }

            if (from == null || to == null || from.Equals(to))
            {
                return geometry;
            }

            using (GeometricTransformer transformer = new GeometricTransformer())
            {
                //transformer.FromSpatialReference = from;
                //transformer.ToSpatialReference = to;
                transformer.SetSpatialReferences(from, to);
                IGeometry transformed = transformer.Transform2D(geometry) as IGeometry;
                transformer.Release();

                return transformed;
            }
        }

        static public IGeometry InvTransform2D(IGeometry geometry, ISpatialReference from, ISpatialReference to)
        {
            return Transform2D(geometry, to, from);
        }

        #region IDisposable Member

        public void Dispose()
        {
            this.Release();
        }

        #endregion
    }

    public sealed class GeometryDef : IGeometryDef
    {
        private bool _hasZ = false, _hasM = false;
        private ISpatialReference _sRef = null;
        private geometryType _geomType = geometryType.Unknown;
        //private gView.Framework.Data.GeometryFieldType _fieldType = gView.Framework.Data.GeometryFieldType.Default;

        public GeometryDef()
        {
        }
        public GeometryDef(geometryType geomType)
        {
            _geomType = geomType;
        }
        public GeometryDef(geometryType geomType, ISpatialReference sRef)
            : this(geomType)
        {
            _sRef = sRef;
        }
        public GeometryDef(geometryType geomType, ISpatialReference sRef, bool hasZ)
            : this(geomType, sRef)
        {
            _hasZ = hasZ;
        }
        public GeometryDef(IGeometryDef geomDef)
        {
            if (geomDef != null)
            {
                _hasZ = geomDef.HasZ;
                _hasM = geomDef.HasM;
                _sRef = ((geomDef.SpatialReference != null) ? (ISpatialReference)geomDef.SpatialReference.Clone() : null);
                _geomType = geomDef.GeometryType;
                //_fieldType = geomDef.GeometryFieldType;
            }
        }
        #region IGeometryDef Member

        public bool HasZ
        {
            get { return _hasZ; }
            set { _hasM = value; }
        }

        public bool HasM
        {
            get { return _hasM; }
            set { _hasM = value; }
        }
        public ISpatialReference SpatialReference
        {
            get { return _sRef; }
            set { _sRef = value; }
        }

        public geometryType GeometryType
        {
            get { return _geomType; }
            set { _geomType = value; }
        }

        //public gView.Framework.Data.GeometryFieldType GeometryFieldType
        //{
        //    get
        //    {
        //        return _fieldType;
        //    }
        //    set
        //    {
        //        _fieldType = value;
        //    }
        //}
        #endregion

        static public void VerifyGeometryType(IGeometry geometry, IGeometryDef geomDef)
        {
            if (geomDef == null)
            {
                throw new ArgumentException("VerifyGeometryType - IGeometryDef Argument is null!");
            }

            if (geometry == null)
            {
                throw new ArgumentException("VerifyGeometryType - IGeometry Argument is null!");
            }

            switch (geomDef.GeometryType)
            {
                case geometryType.Envelope:
                    if (geometry is IEnvelope)
                    {
                        return;
                    }

                    break;
                case geometryType.Point:
                    if (geometry is IPoint)
                    {
                        return;
                    }

                    break;
                case geometryType.Multipoint:
                    if (geometry is IMultiPoint)
                    {
                        return;
                    }

                    break;
                case geometryType.Polyline:
                    if (geometry is IPolyline)
                    {
                        return;
                    }

                    break;
                case geometryType.Polygon:
                    if (geometry is IPolygon)
                    {
                        return;
                    }

                    break;
                case geometryType.Aggregate:
                    if (geometry is IAggregateGeometry)
                    {
                        return;
                    }

                    break;
            }

            throw new ArgumentException("Wrong Geometry for geometry type "
                + geomDef.GeometryType.ToString() + ": "
                + geometry.GetType().ToString());
        }
    }

    static public class GeometryDefExtensions
    {
        static public IGeometry ConvertTo(this IGeometryDef geomDef, IGeometry geometry)
        {
            if (geomDef.GeometryType == geometryType.Point)
            {
                if (geometry is IPoint)
                {
                    return geometry;
                }
                if(geometry is IMultiPoint)
                {
                    if(((IMultiPoint)geometry).PointCount==1)
                    {
                        return geomDef.ConvertTo(((IMultiPoint)geometry)[0]);
                    }
                }
            }

            if (geomDef.GeometryType == geometryType.Multipoint)
            {
                if(geometry is IMultiPoint)
                {
                    return geometry;
                }
                if(geometry is IPoint)
                {
                    var multiPoint = new MultiPoint();
                    multiPoint.AddPoint((IPoint)geometry);

                    return geomDef.ConvertTo(multiPoint);
                }
            }

            if(geomDef.GeometryType==geometryType.Polyline)
            {
                if(geometry is IPolygon)
                {
                    return geometry;
                }
            }

            if (geomDef.GeometryType == geometryType.Polygon)
            {
                if (geometry is IPolygon)
                {
                    return geometry;
                }
            }

            if(geomDef.GeometryType==geometryType.Envelope)
            {
                if(geometry is IEnvelope)
                {
                    return geometry;
                }
                else if(geometry!=null)
                {
                    return geomDef.ConvertTo(geometry.Envelope);
                }
            }

            if(geomDef.GeometryType == geometryType.Aggregate)
            {
                if(geometry is IAggregateGeometry)
                {
                    return geometry;
                }
                else if(geometry!=null)
                {
                    var aggregateGeometry = new AggregateGeometry();
                    aggregateGeometry.AddGeometry(geometry);

                    return geomDef.ConvertTo(aggregateGeometry);
                }
            }

            throw new ArgumentException("Unconvertable for geometrys type "
                + geomDef.GeometryType.ToString() + ": "
                + (geometry != null ? geometry.GetType().ToString() : "NULL"));
        }
    }
}
