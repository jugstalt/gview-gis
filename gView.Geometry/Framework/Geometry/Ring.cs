using System;
using System.Collections.Generic;

//[assembly: InternalsVisibleTo("gView.OGC, PublicKey=0024000004800000940000000602000000240000525341310004000001000100916d0be3f662c2d3589fbe93479f3215e23fd195db9a20e77f42dc1d2942bd48cad3ea36b797f57880e6c31af0c238d2e445898c8ecce990aacbb70ae05a10aff73ab65c7db86366697f934b780238ed8fd1b2e28ba679a97e060b53fce66118e129b91d24f392d4dd3d482fa4173e61f18c74cda9f35721a97e77afbbc96dd2")]


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

        public IPolygon ToPolygon()
        {
            return new Polygon(new Ring(this));  // Create new Ring -> Holes get outer rings!!
        }

        #endregion

        public override object Clone()
        {
            return new Ring(m_points);
        }
    }
}
