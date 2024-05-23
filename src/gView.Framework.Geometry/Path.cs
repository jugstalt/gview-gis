using gView.Framework.Core.Geometry;
using gView.Framework.Core.Common;
using gView.Framework.Geometry.GeoProcessing;
using System;
using System.Collections.Generic;

namespace gView.Framework.Geometry
{
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
                if (_points.Count < 2)
                {
                    return 0.0;
                }

                double len = 0.0;
                for (int i = 1; i < _points.Count; i++)
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
            _points = ListOperations<IPoint>.Swap(_points);
        }
        public IPath Trim(double length)
        {
            Path trim = new Path();
            if (_points.Count == 0)
            {
                return trim;
            }

            IPoint prePoint = new Point(_points[0]);
            trim.AddPoint(prePoint);

            double len = 0.0;
            for (int i = 1; i < _points.Count; i++)
            {
                IPoint point = _points[i];
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

        public IPoint MidPoint2D
        {
            get
            {
                if (this.PointCount == 0)
                {
                    return null;
                }

                double length = this.Length;
                if (length == 0)
                {
                    return this[0];
                }

                return Algorithm.PolylinePoint(new Polyline(this), length / 2D);
            }
        }

        public IPolyline ToPolyline()
        {
            var path = new Path(this);
            if (this is Ring)
            {
                path.ClosePath();
            }

            return new Polyline(path);
        }

        #endregion

        #region ICloneable Member

        public virtual object Clone()
        {
            return new Path(_points);
        }

        #endregion

        public void Close(double tolerance = GeometryConst.Epsilon)
        {
            if (_points.Count < 3)
            {
                return;
            }

            var first = _points[0];
            var last = _points[_points.Count - 1];

            var dist2 = first.Distance2(last);

            if (dist2 >= tolerance * tolerance)
            {
                _points.Add(first);
            }
            else if (dist2 > 0.0)  // snap it!
            {
                last.X = first.X;
                last.Y = first.Y;
            }
        }
    }
}
