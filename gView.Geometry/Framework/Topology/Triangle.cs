using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Geometry;

namespace gView.Framework.Topology
{
    public class Triangle
    {
        /// <summary>
        /// First vertex index in triangle
        /// </summary>
        public int p1;
        /// <summary>
        /// Second vertex index in triangle
        /// </summary>
        public int p2;
        /// <summary>
        /// Third vertex index in triangle
        /// </summary>
        public int p3;
        /// <summary>
        /// Initializes a new instance of a triangle
        /// </summary>
        /// <param name="point1">Vertex 1</param>
        /// <param name="point2">Vertex 2</param>
        /// <param name="point3">Vertex 3</param>
        public Triangle(int point1, int point2, int point3)
        {
            p1 = point1; p2 = point2; p3 = point3;
        }

        static public Point CircumCircleCenter(IPoint p1, IPoint p2, IPoint p3)
        {
            if (System.Math.Abs(p1.Y - p2.Y) < double.Epsilon && System.Math.Abs(p2.Y - p3.Y) < double.Epsilon)
            {
                //INCIRCUM - F - Points are coincident !!
                return null;
            }

            double m1, m2;
            double mx1, mx2;
            double my1, my2;
            double xc, yc;

            if (System.Math.Abs(p2.Y - p1.Y) < double.Epsilon)
            {
                m2 = -(p3.X - p2.X) / (p3.Y - p2.Y);
                mx2 = (p2.X + p3.X) * 0.5;
                my2 = (p2.Y + p3.Y) * 0.5;
                //Calculate CircumCircle center (xc,yc)
                xc = (p2.X + p1.X) * 0.5;
                yc = m2 * (xc - mx2) + my2;
            }
            else if (System.Math.Abs(p3.Y - p2.Y) < double.Epsilon)
            {
                m1 = -(p2.X - p1.X) / (p2.Y - p1.Y);
                mx1 = (p1.X + p2.X) * 0.5;
                my1 = (p1.Y + p2.Y) * 0.5;
                //Calculate CircumCircle center (xc,yc)
                xc = (p3.X + p2.X) * 0.5;
                yc = m1 * (xc - mx1) + my1;
            }
            else
            {
                m1 = -(p2.X - p1.X) / (p2.Y - p1.Y);
                m2 = -(p3.X - p2.X) / (p3.Y - p2.Y);
                mx1 = (p1.X + p2.X) * 0.5;
                mx2 = (p2.X + p3.X) * 0.5;
                my1 = (p1.Y + p2.Y) * 0.5;
                my2 = (p2.Y + p3.Y) * 0.5;
                //Calculate CircumCircle center (xc,yc)
                xc = (m1 * mx1 - m2 * mx2 + my2 - my1) / (m1 - m2);
                yc = m1 * (xc - mx1) + my1;
            }

            return new Point(xc, yc);
        }
    }

    public class Triangles : List<Triangle>
    {
    }
}
