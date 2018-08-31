using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Geometry;
using gView.Framework.Topology;

namespace gView.Framework.SpatialAlgorithms
{
    public class DelaunayTriangulation
    {
        public delegate void ProgressEventHandler(int pos, int max);
        public event ProgressEventHandler Progress = null;

        public Triangles Triangulate(Nodes nodes)
        {
            int nv = nodes.Count;
            if (nv < 3)
                throw new ArgumentException("Need at least three vertices for triangulation");

            int trimax = 4 * nv;

            // Find the maximum and minimum vertex bounds.
            // This is to allow calculation of the bounding supertriangle
            double xmin = nodes[0].X;
            double ymin = nodes[0].Y;
            double xmax = xmin;
            double ymax = ymin;
            for (int i = 1; i < nv; i++)
            {
                if (nodes[i].X < xmin) xmin = nodes[i].X;
                if (nodes[i].X > xmax) xmax = nodes[i].X;
                if (nodes[i].Y < ymin) ymin = nodes[i].Y;
                if (nodes[i].Y > ymax) ymax = nodes[i].Y;
            }

            double dx = xmax - xmin;
            double dy = ymax - ymin;
            double dmax = (dx > dy) ? dx : dy;

            double xmid = (xmax + xmin) * 0.5;
            double ymid = (ymax + ymin) * 0.5;


            // Set up the supertriangle
            // This is a triangle which encompasses all the sample points.
            // The supertriangle coordinates are added to the end of the
            // vertex list. The supertriangle is the first triangle in
            // the triangle list.
            nodes.Add(new Point((xmid - 2 * dmax), (ymid - dmax)));
            nodes.Add(new Point(xmid, (ymid + 2 * dmax)));
            nodes.Add(new Point((xmid + 2 * dmax), (ymid - dmax)));
            Triangles Triangle = new Triangles();
            Triangle.Add(new Triangle(nv, nv + 1, nv + 2)); //SuperTriangle placed at index 0

            // Include each point one at a time into the existing mesh
            for (int i = 0; i < nv; i++)
            {
                if (Progress != null)
                    Progress(i, nv);

                Edges Edges = new Edges(); //[trimax * 3];
                // Set up the edge buffer.
                // If the point (Vertex(i).x,Vertex(i).y) lies inside the circumcircle then the
                // three edges of that triangle are added to the edge buffer and the triangle is removed from list.
                for (int j = 0; j < Triangle.Count; j++)
                {
                    if (InCircle(nodes[i], nodes[Triangle[j].p1], nodes[Triangle[j].p2], nodes[Triangle[j].p3]))
                    {
                        Edges.Add(new Edge(Triangle[j].p1, Triangle[j].p2));
                        Edges.Add(new Edge(Triangle[j].p2, Triangle[j].p3));
                        Edges.Add(new Edge(Triangle[j].p3, Triangle[j].p1));
                        Triangle.RemoveAt(j);
                        j--;
                    }
                }
                if (i >= nv) continue; //In case we the last duplicate point we removed was the last in the array

                // Remove duplicate edges
                // Note: if all triangles are specified anticlockwise then all
                // interior edges are opposite pointing in direction.
                for (int j = Edges.Count - 2; j >= 0; j--)
                {
                    for (int k = Edges.Count - 1; k >= j + 1; k--)
                    {
                        if (Edges[j].Equals(Edges[k]))
                        {
                            Edges.RemoveAt(k);
                            Edges.RemoveAt(j);
                            k--;
                            continue;
                        }
                    }
                }
                // Form new triangles for the current point
                // Skipping over any tagged edges.
                // All edges are arranged in clockwise order.
                for (int j = 0; j < Edges.Count; j++)
                {
                    if (Triangle.Count >= trimax)
                        throw new ApplicationException("Exceeded maximum edges");
                    Triangle.Add(new Triangle(Edges[j].p1, Edges[j].p2, i));
                }
                Edges.Clear();
                Edges = null;
            }
            // Remove triangles with supertriangle vertices
            // These are triangles which have a vertex number greater than nv
            for (int i = Triangle.Count - 1; i >= 0; i--)
            {
                if (Triangle[i].p1 >= nv || Triangle[i].p2 >= nv || Triangle[i].p3 >= nv)
                    Triangle.RemoveAt(i);
            }
            //Remove SuperTriangle vertices
            nodes.RemoveAt(nodes.Count - 1);
            nodes.RemoveAt(nodes.Count - 1);
            nodes.RemoveAt(nodes.Count - 1);
            Triangle.TrimExcess();
            return Triangle;
        }
        public Edges TriangleEdges(Triangles triangles)
        {
            if (triangles == null) return null;

            Edges edges = new Edges();
            int max = triangles.Count;
            int pos = 0;

            foreach (Triangle triangle in triangles)
            {
                Edge e1 = new Edge(triangle.p1, triangle.p2);
                Edge e2 = new Edge(triangle.p1, triangle.p3);
                Edge e3 = new Edge(triangle.p2, triangle.p3);

                if (!edges.Contains(e1)) edges.Add(e1);
                if (!edges.Contains(e2)) edges.Add(e2);
                if (!edges.Contains(e3)) edges.Add(e3);

                if (Progress != null)
                    Progress(pos++, max);
            }

            return edges;
        }

        private bool InCircle(IPoint p, IPoint p1, IPoint p2, IPoint p3)
        {
            //Return TRUE if the point (xp,yp) lies inside the circumcircle
            //made up by points (x1,y1) (x2,y2) (x3,y3)
            //NOTE: A point on the edge is inside the circumcircle

            if (System.Math.Abs(p1.Y - p2.Y) < double.Epsilon && System.Math.Abs(p2.Y - p3.Y) < double.Epsilon)
            {
                //INCIRCUM - F - Points are coincident !!
                return false;
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

            double dx = p2.X - xc;
            double dy = p2.Y - yc;
            double rsqr = dx * dx + dy * dy;
            //double r = Math.Sqrt(rsqr); //Circumcircle radius
            dx = p.X - xc;
            dy = p.Y - yc;
            double drsqr = dx * dx + dy * dy;

            return (drsqr <= rsqr);
        } 
    }
}
