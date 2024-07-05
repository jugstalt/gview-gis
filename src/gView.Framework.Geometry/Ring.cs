using System;
using System.Collections.Generic;
using gView.Framework.Core.Geometry;

namespace gView.Framework.Geometry
{
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
                return Math.Abs(SignedArea);
            }
        }

        private double SignedArea
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
                //return CalculateCentroid();

                double cx = 0, cy = 0, A = SignedArea;
                if (A == 0.0)
                {
                    return null;
                }

                int pointCount = _points.Count, to = pointCount;
                if (Math.Abs(this[0].X - this[pointCount - 1].X) > GeometryConst.Epsilon ||
                    Math.Abs(this[0].Y - this[pointCount - 1].Y) > GeometryConst.Epsilon)
                {
                    to += 1;
                }

                IPoint p0 = this[0], p1, envCenter = Envelope.Center;
                double envCenterX = envCenter.X;
                double envCenterY = envCenter.Y;

                for (int i = 1; i < to; i++)
                {
                    p1 = (i < PointCount) ? this[i % _points.Count] : this[0];

                    //
                    // calc all in centered coordiantes
                    // otherweise, the may be rounding errors
                    //
                    double p0X = p0.X - envCenterX,
                           p0Y = p0.Y - envCenterY;
                    double p1X = p1.X - envCenterX,
                           p1Y = p1.Y - envCenterY;

                    double h = (p0X * p1Y - p1X * p0Y);
                    cx += (p0X + p1X) * h / 6.0;
                    cy += (p0Y + p1Y) * h / 6.0;

                    p0 = p1;
                }

                return new Point(
                                (cx / A) + envCenterX, 
                                (cy / A) + envCenterY
                           );
            }
        }

        public IPolygon ToPolygon()
        {
            return new Polygon(new Ring(this));  // Create new Ring -> Holes get outer rings!!
        }

        #endregion

        public override object Clone()
        {
            return new Ring(_points);
        }

        #region Helper

        //public IPoint CalculateCentroid()
        //{
        //    double centroidX = 0;
        //    double centroidY = 0;
        //    double signedArea = 0;
        //    double x0 = 0; // Current vertex X
        //    double y0 = 0; // Current vertex Y
        //    double x1 = 0; // Next vertex X
        //    double y1 = 0; // Next vertex Y
        //    double a = 0;  // Partial signed area

        //    // For all vertices
        //    int i = 0;
        //    for (i = 0; i < _points.Count - 1; ++i)
        //    {
        //        x0 = _points[i].X;
        //        y0 = _points[i].Y;
        //        x1 = _points[i + 1].X;
        //        y1 = _points[i + 1].Y;
        //        a = x0 * y1 - x1 * y0;
        //        signedArea += a;
        //        centroidX += (x0 + x1) * a;
        //        centroidY += (y0 + y1) * a;
        //    }

        //    // Do last vertex separately to close the polygon
        //    if (Math.Abs(this[0].X - this[_points.Count - 1].X) > 1e-7 ||
        //        Math.Abs(this[0].Y - this[_points.Count - 1].Y) > 1e-1)
        //    {
        //        x0 = _points[i].X;
        //        y0 = _points[i].Y;
        //        x1 = _points[0].X;
        //        y1 = _points[0].Y;
        //        a = x0 * y1 - x1 * y0;
        //        signedArea += a;
        //        centroidX += (x0 + x1) * a;
        //        centroidY += (y0 + y1) * a;
        //    }

        //    signedArea *= 0.5;
        //    centroidX /= (6 * signedArea);
        //    centroidY /= (6 * signedArea);

        //    return new Point { X = centroidX, Y = centroidY };
        //}

        #endregion
    }
}
