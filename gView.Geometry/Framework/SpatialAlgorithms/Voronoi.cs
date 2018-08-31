using System;
using System.Collections;
using System.Drawing;
using System.Collections.Generic;
using gView.Framework.Geometry;
using gView.Framework.Topology;

namespace gView.Framework.SpatialAlgorithms
{
    public class VoronoiGraph
    {
        public delegate void ProgressEventHandler(int pos, int max);
        public event ProgressEventHandler Progress = null;
        public delegate void ProgressMessageEventHandler(string msg);
        public event ProgressMessageEventHandler ProgressMessage = null;

        private Nodes _nodes = new Nodes();
        private Edges _edges = new Edges();

        public void Calc(Nodes nodes)
        {
            _nodes = new Nodes();
            _edges = new Edges();

            DelaunayTriangulation triangulation = new DelaunayTriangulation();
            if(Progress!=null)
                triangulation.Progress += new DelaunayTriangulation.ProgressEventHandler(triangulation_Progress);
            if (ProgressMessage != null)
                ProgressMessage("Calculate Triangles");
            Triangles triangles = triangulation.Triangulate(nodes);

            int tCount = triangles.Count;
            int pos = 0;
            if (ProgressMessage != null)
                ProgressMessage("Calculate Vertices");
            // Vertices
            foreach (Triangle triangle in triangles)
            {
                if (Progress != null)
                    Progress(pos++, tCount);

                _nodes.Add(Triangle.CircumCircleCenter(
                    nodes[triangle.p1], nodes[triangle.p2], nodes[triangle.p3]));
            }

            pos = 0;
            if (ProgressMessage != null)
                ProgressMessage("Calculate Edges");
            // Edges
            for (int n = 0; n < tCount; n++)
            {
                if (Progress != null)
                    Progress( pos++, tCount);

                Triangle triangle = triangles[n];
                int n1 = NeigbourTriangle(n, new Edge(triangle.p1, triangle.p2), triangles);
                int n2 = NeigbourTriangle(n, new Edge(triangle.p1, triangle.p3), triangles);
                int n3 = NeigbourTriangle(n, new Edge(triangle.p2, triangle.p3), triangles);

                if (n1 >= 0)
                    _edges.Add(new Edge(n, n1));
                if (n2 >= 0)
                    _edges.Add(new Edge(n, n2));
                if (n3 >= 0)
                    _edges.Add(new Edge(n, n3));
            }
        }

        void triangulation_Progress(int pos, int max)
        {
            if (Progress != null)
                Progress(pos, max);
        }

        private int NeigbourTriangle(int canditate,
            Edge edge,
            Triangles triangles)
        {
            int tCount = triangles.Count;
            for (int n = canditate + 1; n < tCount; n++)
            {
                Triangle triangle = triangles[n];
                Edge e1 = new Edge(triangle.p1, triangle.p2);
                Edge e2 = new Edge(triangle.p1, triangle.p3);
                Edge e3 = new Edge(triangle.p2, triangle.p3);

                if (e1.Equals(edge) || e2.Equals(edge) || e3.Equals(edge)) return n;
            }

            return -1;
        }

        public Nodes Nodes
        {
            get { return _nodes; }
        }
        public Edges Edges
        {
            get { return _edges; }
        }
    }
    /*
    public class Voronoi
    {
        #region Declaratoins
        gView.Framework.Geometry.IPointCollection _vertices = new gView.Framework.Geometry.PointCollection();
        List<gView.Framework.Geometry.IPath> _edges = new List<gView.Framework.Geometry.IPath>();
        List<gView.Framework.Geometry.IPath> _vedges = new List<gView.Framework.Geometry.IPath>();
        #endregion

        public bool Calc(gView.Framework.Geometry.IPointCollection pColl)
        {
            if (pColl == null) return false;

            List<Vector> points = new List<Vector>();
            for (int i = 0; i < pColl.PointCount; i++)
            {
                if (pColl[i] == null) continue;
                points.Add(new Vector(pColl[i].X, pColl[i].Y));
            }
            VoronoiGraph graph = ComputeVoronoiGraph(points);
            if (graph == null) return false;

            foreach (Vector vector in graph.Vertizes)
            {
                _vertices.AddPoint(new gView.Framework.Geometry.Point(vector[0], vector[1]));
            }
            foreach (VoronoiEdge edge in graph.Edges)
            {
                gView.Framework.Geometry.Path p = new gView.Framework.Geometry.Path();
                p.AddPoint(new gView.Framework.Geometry.Point(edge.LeftData[0], edge.LeftData[1]));
                p.AddPoint(new gView.Framework.Geometry.Point(edge.RightData[0], edge.RightData[1]));
                _edges.Add(p);
            }
            foreach (VoronoiEdge edge in graph.Edges)
            {
                gView.Framework.Geometry.Path p = new gView.Framework.Geometry.Path();
                p.AddPoint(new gView.Framework.Geometry.Point(edge.VVertexA[0], edge.VVertexA[1]));
                p.AddPoint(new gView.Framework.Geometry.Point(edge.VVertexB[0], edge.VVertexB[1]));
                _vedges.Add(p);
            }
            return true;
        }

        public gView.Framework.Geometry.IPointCollection Vertices
        {
            get { return _vertices; }
        }
        public List<gView.Framework.Geometry.IPath> Edges
        {
            get { return _edges; }
        }
        public List<gView.Framework.Geometry.IPath> VEdges
        {
            get { return _vedges; }
        }

        #region Helper
        private static readonly Vector VVInfinite = new Vector(double.PositiveInfinity, double.PositiveInfinity);
        private static readonly Vector VVUnkown = new Vector(double.NaN, double.NaN);
        private static double ParabolicCut(double x1, double y1, double x2, double y2, double ys)
        {
            //			y1=-y1;
            //			y2=-y2;
            //			ys=-ys;
            //			
            if (Math.Abs(x1 - x2) < 1e-10 && Math.Abs(y1 - y2) < 1e-10)
            {
                //				if(y1>y2)
                //					return double.PositiveInfinity;
                //				if(y1<y2)
                //					return double.NegativeInfinity;
                //				return x;
                throw new Exception("Identical datapoints are not allowed!");
            }

            if (Math.Abs(y1 - ys) < 1e-10 && Math.Abs(y2 - ys) < 1e-10)
                return (x1 + x2) / 2;
            if (Math.Abs(y1 - ys) < 1e-10)
                return x1;
            if (Math.Abs(y2 - ys) < 1e-10)
                return x2;
            double a1 = 1 / (2 * (y1 - ys));
            double a2 = 1 / (2 * (y2 - ys));
            if (Math.Abs(a1 - a2) < 1e-10)
                return (x1 + x2) / 2;
            double xs1 = 0.5 / (2 * a1 - 2 * a2) * (4 * a1 * x1 - 4 * a2 * x2 + 2 * Math.Sqrt(-8 * a1 * x1 * a2 * x2 - 2 * a1 * y1 + 2 * a1 * y2 + 4 * a1 * a2 * x2 * x2 + 2 * a2 * y1 + 4 * a2 * a1 * x1 * x1 - 2 * a2 * y2));
            double xs2 = 0.5 / (2 * a1 - 2 * a2) * (4 * a1 * x1 - 4 * a2 * x2 - 2 * Math.Sqrt(-8 * a1 * x1 * a2 * x2 - 2 * a1 * y1 + 2 * a1 * y2 + 4 * a1 * a2 * x2 * x2 + 2 * a2 * y1 + 4 * a2 * a1 * x1 * x1 - 2 * a2 * y2));
            xs1 = Math.Round(xs1, 10);
            xs2 = Math.Round(xs2, 10);
            if (xs1 > xs2)
            {
                double h = xs1;
                xs1 = xs2;
                xs2 = h;
            }
            if (y1 >= y2)
                return xs2;
            return xs1;
        }
        private static Vector CircumCircleCenter(Vector A, Vector B, Vector C)
        {
            if (A == B || B == C || A == C)
                throw new Exception("Need three different points!");
            double tx = (A[0] + C[0]) / 2;
            double ty = (A[1] + C[1]) / 2;

            double vx = (B[0] + C[0]) / 2;
            double vy = (B[1] + C[1]) / 2;

            double ux, uy, wx, wy;

            if (A[0] == C[0])
            {
                ux = 1;
                uy = 0;
            }
            else
            {
                ux = (C[1] - A[1]) / (A[0] - C[0]);
                uy = 1;
            }

            if (B[0] == C[0])
            {
                wx = -1;
                wy = 0;
            }
            else
            {
                wx = (B[1] - C[1]) / (B[0] - C[0]);
                wy = -1;
            }

            double alpha = (wy * (vx - tx) - wx * (vy - ty)) / (ux * wy - wx * uy);

            return new Vector(tx + alpha * ux, ty + alpha * uy);
        }
        private VoronoiGraph ComputeVoronoiGraph(IEnumerable Datapoints)
        {
            BinaryPriorityQueue PQ = new BinaryPriorityQueue();
            Hashtable CurrentCircles = new Hashtable();
            VoronoiGraph VG = new VoronoiGraph();
            VNode RootNode = null;
            foreach (Vector V in Datapoints)
            {
                PQ.Push(new VDataEvent(V));
            }
            while (PQ.Count > 0)
            {
                VEvent VE = PQ.Pop() as VEvent;
                VDataNode[] CircleCheckList;
                if (VE is VDataEvent)
                {
                    RootNode = VNode.ProcessDataEvent(VE as VDataEvent, RootNode, VG, VE.Y, out CircleCheckList);
                }
                else if (VE is VCircleEvent)
                {
                    CurrentCircles.Remove(((VCircleEvent)VE).NodeN);
                    if (!((VCircleEvent)VE).Valid)
                        continue;
                    RootNode = VNode.ProcessCircleEvent(VE as VCircleEvent, RootNode, VG, VE.Y, out CircleCheckList);
                }
                else throw new Exception("Got event of type " + VE.GetType().ToString() + "!");
                foreach (VDataNode VD in CircleCheckList)
                {
                    if (CurrentCircles.ContainsKey(VD))
                    {
                        ((VCircleEvent)CurrentCircles[VD]).Valid = false;
                        CurrentCircles.Remove(VD);
                    }
                    VCircleEvent VCE = VNode.CircleCheckDataNode(VD, VE.Y);
                    if (VCE != null)
                    {
                        PQ.Push(VCE);
                        CurrentCircles[VD] = VCE;
                    }
                }
                if (VE is VDataEvent)
                {
                    Vector DP = ((VDataEvent)VE).DataPoint;
                    foreach (VCircleEvent VCE in CurrentCircles.Values)
                    {
                        if (MathTools.Dist(DP[0], DP[1], VCE.Center[0], VCE.Center[1]) < VCE.Y - VCE.Center[1] && Math.Abs(MathTools.Dist(DP[0], DP[1], VCE.Center[0], VCE.Center[1]) - (VCE.Y - VCE.Center[1])) > 1e-10)
                            VCE.Valid = false;
                    }
                }
            }
            return VG;
        }
        #endregion

        #region Helper Classes & Interfaces
        private interface IPriorityQueue : ICollection, ICloneable, IList
        {
            int Push(object O);
            object Pop();
            object Peek();
            void Update(int i);
        }
        private class BinaryPriorityQueue : IPriorityQueue, ICollection, ICloneable, IList
        {
            protected ArrayList InnerList = new ArrayList();
            protected IComparer Comparer;

            #region contructors
            public BinaryPriorityQueue()
                : this(System.Collections.Comparer.Default)
            { }
            public BinaryPriorityQueue(IComparer c)
            {
                Comparer = c;
            }
            public BinaryPriorityQueue(int C)
                : this(System.Collections.Comparer.Default, C)
            { }
            public BinaryPriorityQueue(IComparer c, int Capacity)
            {
                Comparer = c;
                InnerList.Capacity = Capacity;
            }

            protected BinaryPriorityQueue(ArrayList Core, IComparer Comp, bool Copy)
            {
                if (Copy)
                    InnerList = Core.Clone() as ArrayList;
                else
                    InnerList = Core;
                Comparer = Comp;
            }

            #endregion
            protected void SwitchElements(int i, int j)
            {
                object h = InnerList[i];
                InnerList[i] = InnerList[j];
                InnerList[j] = h;
            }

            protected virtual int OnCompare(int i, int j)
            {
                return Comparer.Compare(InnerList[i], InnerList[j]);
            }

            #region public methods
            /// <summary>
            /// Push an object onto the PQ
            /// </summary>
            /// <param name="O">The new object</param>
            /// <returns>The index in the list where the object is _now_. This will change when objects are taken from or put onto the PQ.</returns>
            public int Push(object O)
            {
                int p = InnerList.Count, p2;
                InnerList.Add(O); // E[p] = O
                do
                {
                    if (p == 0)
                        break;
                    p2 = (p - 1) / 2;
                    if (OnCompare(p, p2) < 0)
                    {
                        SwitchElements(p, p2);
                        p = p2;
                    }
                    else
                        break;
                } while (true);
                return p;
            }

            /// <summary>
            /// Get the smallest object and remove it.
            /// </summary>
            /// <returns>The smallest object</returns>
            public object Pop()
            {
                object result = InnerList[0];
                int p = 0, p1, p2, pn;
                InnerList[0] = InnerList[InnerList.Count - 1];
                InnerList.RemoveAt(InnerList.Count - 1);
                do
                {
                    pn = p;
                    p1 = 2 * p + 1;
                    p2 = 2 * p + 2;
                    if (InnerList.Count > p1 && OnCompare(p, p1) > 0) // links kleiner
                        p = p1;
                    if (InnerList.Count > p2 && OnCompare(p, p2) > 0) // rechts noch kleiner
                        p = p2;

                    if (p == pn)
                        break;
                    SwitchElements(p, pn);
                } while (true);
                return result;
            }

            /// <summary>
            /// Notify the PQ that the object at position i has changed
            /// and the PQ needs to restore order.
            /// Since you dont have access to any indexes (except by using the
            /// explicit IList.this) you should not call this function without knowing exactly
            /// what you do.
            /// </summary>
            /// <param name="i">The index of the changed object.</param>
            public void Update(int i)
            {
                int p = i, pn;
                int p1, p2;
                do	// aufsteigen
                {
                    if (p == 0)
                        break;
                    p2 = (p - 1) / 2;
                    if (OnCompare(p, p2) < 0)
                    {
                        SwitchElements(p, p2);
                        p = p2;
                    }
                    else
                        break;
                } while (true);
                if (p < i)
                    return;
                do	   // absteigen
                {
                    pn = p;
                    p1 = 2 * p + 1;
                    p2 = 2 * p + 2;
                    if (InnerList.Count > p1 && OnCompare(p, p1) > 0) // links kleiner
                        p = p1;
                    if (InnerList.Count > p2 && OnCompare(p, p2) > 0) // rechts noch kleiner
                        p = p2;

                    if (p == pn)
                        break;
                    SwitchElements(p, pn);
                } while (true);
            }

            /// <summary>
            /// Get the smallest object without removing it.
            /// </summary>
            /// <returns>The smallest object</returns>
            public object Peek()
            {
                if (InnerList.Count > 0)
                    return InnerList[0];
                return null;
            }

            public bool Contains(object value)
            {
                return InnerList.Contains(value);
            }

            public void Clear()
            {
                InnerList.Clear();
            }

            public int Count
            {
                get
                {
                    return InnerList.Count;
                }
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return InnerList.GetEnumerator();
            }

            public void CopyTo(Array array, int index)
            {
                InnerList.CopyTo(array, index);
            }

            public object Clone()
            {
                return new BinaryPriorityQueue(InnerList, Comparer, true);
            }

            public bool IsSynchronized
            {
                get
                {
                    return InnerList.IsSynchronized;
                }
            }

            public object SyncRoot
            {
                get
                {
                    return this;
                }
            }
            #endregion
            #region explicit implementation
            bool IList.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            object IList.this[int index]
            {
                get
                {
                    return InnerList[index];
                }
                set
                {
                    InnerList[index] = value;
                    Update(index);
                }
            }

            int IList.Add(object o)
            {
                return Push(o);
            }

            void IList.RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            void IList.Insert(int index, object value)
            {
                throw new NotSupportedException();
            }

            void IList.Remove(object value)
            {
                throw new NotSupportedException();
            }

            int IList.IndexOf(object value)
            {
                throw new NotSupportedException();
            }

            bool IList.IsFixedSize
            {
                get
                {
                    return false;
                }
            }

            public static BinaryPriorityQueue Syncronized(BinaryPriorityQueue P)
            {
                return new BinaryPriorityQueue(ArrayList.Synchronized(P.InnerList), P.Comparer, false);
            }
            public static BinaryPriorityQueue ReadOnly(BinaryPriorityQueue P)
            {
                return new BinaryPriorityQueue(ArrayList.ReadOnly(P.InnerList), P.Comparer, false);
            }
            #endregion

            public void Sort()
            {
                InnerList.Sort(Comparer);
            }
        }
        #endregion

        #region HashSet & Vector Classes
        private class HashSet : IEnumerable, ICollection
        {
            Hashtable H = new Hashtable();
            object Dummy = new object();
            public HashSet() { }
            public void Add(object O)
            {
                H[O] = Dummy;
            }
            public void AddRange(IEnumerable List)
            {
                foreach (object O in List)
                    Add(O);
            }
            public void Remove(object O)
            {
                H.Remove(O);
            }
            public bool Contains(object O)
            {
                return H.ContainsKey(O);
            }
            public void Clear()
            {
                H.Clear();
            }
            public IEnumerator GetEnumerator()
            {
                return H.Keys.GetEnumerator();
            }
            public int Count
            {
                get
                {
                    return H.Count;
                }
            }

            public bool IsSynchronized
            {
                get
                {
                    return H.IsSynchronized;
                }
            }

            public void CopyTo(Array array, int index)
            {
                H.Keys.CopyTo(array, index);
            }

            public object SyncRoot
            {
                get
                {
                    return H.SyncRoot;
                }
            }
        }
        private class Vector : IEnumerable, IComparable
        {
            /// <summary>
            /// Global precision for any calculation
            /// </summary>
            public static int Precision = 10;
            double[] data;
            public object Tag = null;
            /// <summary>
            /// Build a new vector
            /// </summary>
            /// <param name="dim">The dimension</param>
            public Vector(int dim)
            {
                data = new double[dim];
            }
            /// <summary>
            /// Build a new vector
            /// </summary>
            /// <param name="X">The elements of the vector</param>
            public Vector(params double[] X)
            {
                data = new double[X.Length];
                X.CopyTo(data, 0);
            }
            /// <summary>
            /// Build a new vector as a copy of an existing one
            /// </summary>
            /// <param name="O">The existing vector</param>
            public Vector(Vector O)
                : this(O.data)
            { }
            /// <summary>
            /// Build a new vector from a string
            /// </summary>
            /// <param name="S">A string, as produced by ToString</param>
            public Vector(string S)
            {
                if (S[0] != '(' || S[S.Length - 1] != ')')
                    throw new Exception("Formatfehler!");
                string[] P = MathTools.HighLevelSplit(S.Substring(1, S.Length - 2), ';');
                data = new double[P.Length];
                int i;
                for (i = 0; i < data.Length; i++)
                {
                    data[i] = Convert.ToDouble(P[i]);
                }
            }
            /// <summary>
            /// Gets or sets the value of the vector at the given index
            /// </summary>
            public double this[int i]
            {
                get
                {
                    return data[i];
                }
                set
                {
                    data[i] = Math.Round(value, Precision);
                }
            }

            /// <summary>
            /// The dimension of the vector
            /// </summary>
            public int Dim
            {
                get
                {
                    return data.Length;
                }
            }

            /// <summary>
            /// The squared length of the vector
            /// </summary>
            public double SquaredLength
            {
                get
                {
                    return this * this;
                }
            }

            /// <summary>
            /// The sum of all elements in the vector
            /// </summary>
            public double ElementSum
            {
                get
                {
                    int i;
                    double E = 0;
                    for (i = 0; i < Dim; i++)
                        E += data[i];
                    return E;
                }
            }
            /// <summary>
            /// Reset all elements with ransom values from the given range
            /// </summary>
            /// <param name="Min">Min</param>
            /// <param name="Max">Max</param>
            public void Randomize(double Min, double Max)
            {
                int i;
                for (i = 0; i < data.Length; i++)
                {
                    this[i] = Min + (Max - Min) * MathTools.R.NextDouble();
                }
            }
            /// <summary>
            /// Reset all elements with ransom values from the given range
            /// </summary>
            /// <param name="MinMax">MinMax[0] - Min
            /// MinMax[1] - Max</param>
            public void Randomize(Vector[] MinMax)
            {
                int i;
                for (i = 0; i < data.Length; i++)
                {
                    this[i] = MinMax[0][i] + (MinMax[1][i] - MinMax[0][i]) * MathTools.R.NextDouble();
                }
            }
            /// <summary>
            /// Scale all elements by r
            /// </summary>
            /// <param name="r">The scalar</param>
            public void Multiply(double r)
            {
                int i;
                for (i = 0; i < data.Length; i++)
                {
                    this[i] *= r;
                }
            }
            /// <summary>
            /// Add another vector
            /// </summary>
            /// <param name="V">V</param>
            public void Add(Vector V)
            {
                int i;
                for (i = 0; i < data.Length; i++)
                {
                    this[i] += V[i];
                }
            }
            /// <summary>
            /// Add a constant to all elements
            /// </summary>
            /// <param name="d">The constant</param>
            public void Add(double d)
            {
                int i;
                for (i = 0; i < data.Length; i++)
                {
                    this[i] += d;
                }
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return data.GetEnumerator();
            }

            /// <summary>
            /// Convert the vector into a reconstructable string representation
            /// </summary>
            /// <returns>A string from which the vector can be rebuilt</returns>
            public override string ToString()
            {
                string S = "(";
                int i;
                for (i = 0; i < data.Length; i++)
                {
                    S += data[i].ToString("G4");
                    if (i < data.Length - 1)
                        S += ";";
                }
                S += ")";
                return S;
            }

            /// <summary>
            /// Compares this vector with another one
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                Vector B = obj as Vector;
                if (B == null || data.Length != B.data.Length)
                    return false;
                int i;
                for (i = 0; i < data.Length; i++)
                {
                    if (Math.Abs(data[i] - B.data[i]) > 1e-10)
                        return false;
                }
                return true;
            }

            /// <summary>
            /// Retrieves a hashcode that is dependent on the elements
            /// </summary>
            /// <returns>The hashcode</returns>
            public override int GetHashCode()
            {
                int Erg = 0;
                foreach (double D in data)
                    Erg = Erg ^ Math.Round(D, Precision).GetHashCode();
                return Erg;
            }

            /// <summary>
            /// Subtract two vectors
            /// </summary>
            public static Vector operator -(Vector A, Vector B)
            {
                if (A.Dim != B.Dim)
                    throw new Exception("Vectors of different dimension!");
                Vector Erg = new Vector(A.Dim);
                int i;
                for (i = 0; i < A.Dim; i++)
                    Erg[i] = A[i] - B[i];
                return Erg;
            }

            /// <summary>
            /// Add two vectors
            /// </summary>
            public static Vector operator +(Vector A, Vector B)
            {
                if (A.Dim != B.Dim)
                    throw new Exception("Vectors of different dimension!");
                Vector Erg = new Vector(A.Dim);
                int i;
                for (i = 0; i < A.Dim; i++)
                    Erg[i] = A[i] + B[i];
                return Erg;
            }

            /// <summary>
            /// Get the scalar product of two vectors
            /// </summary>
            public static double operator *(Vector A, Vector B)
            {
                if (A.Dim != B.Dim)
                    throw new Exception("Vectors of different dimension!");
                double Erg = 0;
                int i;
                for (i = 0; i < A.Dim; i++)
                    Erg += A[i] * B[i];
                return Erg;
            }

            /// <summary>
            /// Scale one vector
            /// </summary>
            public static Vector operator *(Vector A, double B)
            {
                Vector Erg = new Vector(A.Dim);
                int i;
                for (i = 0; i < A.Dim; i++)
                    Erg[i] = A[i] * B;
                return Erg;
            }

            /// <summary>
            /// Scale one vector
            /// </summary>
            public static Vector operator *(double A, Vector B)
            {
                return B * A;
            }
            /// <summary>
            /// Interprete the vector as a double-array
            /// </summary>
            public static explicit operator double[](Vector A)
            {
                return A.data;
            }
            /// <summary>
            /// Get the distance of two vectors
            /// </summary>
            public static double Dist(Vector V1, Vector V2)
            {
                if (V1.Dim != V2.Dim)
                    return -1;
                int i;
                double E = 0, D;
                for (i = 0; i < V1.Dim; i++)
                {
                    D = (V1[i] - V2[i]);
                    E += D * D;
                }
                return E;

            }

            /// <summary>
            /// Compare two vectors
            /// </summary>
            public int CompareTo(object obj)
            {
                Vector A = this;
                Vector B = obj as Vector;
                if (A == null || B == null)
                    return 0;
                double Al, Bl;
                Al = A.SquaredLength;
                Bl = B.SquaredLength;
                if (Al > Bl)
                    return 1;
                if (Al < Bl)
                    return -1;
                int i;
                for (i = 0; i < A.Dim; i++)
                {
                    if (A[i] > B[i])
                        return 1;
                    if (A[i] < B[i])
                        return -1;
                }
                return 0;
            }
            /// <summary>
            /// Get a copy of one vector
            /// </summary>
            /// <returns></returns>
            public virtual Vector Clone()
            {
                return new Vector(data);
            }
        }
        #endregion

        #region Graph & Node Classes
        private class VoronoiGraph
        {
            public HashSet Vertizes = new HashSet();
            public HashSet Edges = new HashSet();
        }
        private class VoronoiEdge
        {
            public Vector RightData, LeftData;
            public Vector VVertexA = Voronoi.VVUnkown, VVertexB = Voronoi.VVUnkown;
            public void AddVertex(Vector V)
            {
                if (VVertexA == Voronoi.VVUnkown)
                    VVertexA = V;
                else if (VVertexB == Voronoi.VVUnkown)
                    VVertexB = V;
                else throw new Exception("Tried to add third vertex!");
            }
        }

        // VoronoiVertex or VoronoiDataPoint are represented as Vector

        private abstract class VNode
        {
            private VNode _Parent = null;
            private VNode _Left = null, _Right = null;
            public VNode Left
            {
                get { return _Left; }
                set
                {
                    _Left = value;
                    value.Parent = this;
                }
            }
            public VNode Right
            {
                get { return _Right; }
                set
                {
                    _Right = value;
                    value.Parent = this;
                }
            }
            public VNode Parent
            {
                get { return _Parent; }
                set { _Parent = value; }
            }


            public void Replace(VNode ChildOld, VNode ChildNew)
            {
                if (Left == ChildOld)
                    Left = ChildNew;
                else if (Right == ChildOld)
                    Right = ChildNew;
                else throw new Exception("Child not found!");
                ChildOld.Parent = null;
            }

            public static VDataNode FirstDataNode(VNode Root)
            {
                VNode C = Root;
                while (C.Left != null)
                    C = C.Left;
                return (VDataNode)C;
            }
            public static VDataNode LeftDataNode(VDataNode Current)
            {
                VNode C = Current;
                //1. Up
                do
                {
                    if (C.Parent == null)
                        return null;
                    if (C.Parent.Left == C)
                    {
                        C = C.Parent;
                        continue;
                    }
                    else
                    {
                        C = C.Parent;
                        break;
                    }
                } while (true);
                //2. One Left
                C = C.Left;
                //3. Down
                while (C.Right != null)
                    C = C.Right;
                return (VDataNode)C; // Cast statt 'as' damit eine Exception kommt
            }
            public static VDataNode RightDataNode(VDataNode Current)
            {
                VNode C = Current;
                //1. Up
                do
                {
                    if (C.Parent == null)
                        return null;
                    if (C.Parent.Right == C)
                    {
                        C = C.Parent;
                        continue;
                    }
                    else
                    {
                        C = C.Parent;
                        break;
                    }
                } while (true);
                //2. One Right
                C = C.Right;
                //3. Down
                while (C.Left != null)
                    C = C.Left;
                return (VDataNode)C; // Cast statt 'as' damit eine Exception kommt
            }

            public static VEdgeNode EdgeToRightDataNode(VDataNode Current)
            {
                VNode C = Current;
                //1. Up
                do
                {
                    if (C.Parent == null)
                        throw new Exception("No Left Leaf found!");
                    if (C.Parent.Right == C)
                    {
                        C = C.Parent;
                        continue;
                    }
                    else
                    {
                        C = C.Parent;
                        break;
                    }
                } while (true);
                return (VEdgeNode)C;
            }

            public static VDataNode FindDataNode(VNode Root, double ys, double x)
            {
                VNode C = Root;
                do
                {
                    if (C is VDataNode)
                        return (VDataNode)C;
                    if (((VEdgeNode)C).Cut(ys, x) < 0)
                        C = C.Left;
                    else
                        C = C.Right;
                } while (true);
            }

            /// <summary>
            /// Will return the new root (unchanged except in start-up)
            /// </summary>
            public static VNode ProcessDataEvent(VDataEvent e, VNode Root, VoronoiGraph VG, double ys, out VDataNode[] CircleCheckList)
            {
                if (Root == null)
                {
                    Root = new VDataNode(e.DataPoint);
                    CircleCheckList = new VDataNode[] { (VDataNode)Root };
                    return Root;
                }
                //1. Find the node to be replaced
                VNode C = VNode.FindDataNode(Root, ys, e.DataPoint[0]);
                //2. Create the subtree (ONE Edge, but two VEdgeNodes)
                VoronoiEdge VE = new VoronoiEdge();
                VE.LeftData = ((VDataNode)C).DataPoint;
                VE.RightData = e.DataPoint;
                VE.VVertexA = Voronoi.VVUnkown;
                VE.VVertexB = Voronoi.VVUnkown;
                VG.Edges.Add(VE);

                VNode SubRoot;
                if (Math.Abs(VE.LeftData[1] - VE.RightData[1]) < 1e-10)
                {
                    if (VE.LeftData[0] < VE.RightData[0])
                    {
                        SubRoot = new VEdgeNode(VE, false);
                        SubRoot.Left = new VDataNode(VE.LeftData);
                        SubRoot.Right = new VDataNode(VE.RightData);
                    }
                    else
                    {
                        SubRoot = new VEdgeNode(VE, true);
                        SubRoot.Left = new VDataNode(VE.RightData);
                        SubRoot.Right = new VDataNode(VE.LeftData);
                    }
                    CircleCheckList = new VDataNode[] { (VDataNode)SubRoot.Left, (VDataNode)SubRoot.Right };
                }
                else
                {
                    SubRoot = new VEdgeNode(VE, false);
                    SubRoot.Left = new VDataNode(VE.LeftData);
                    SubRoot.Right = new VEdgeNode(VE, true);
                    SubRoot.Right.Left = new VDataNode(VE.RightData);
                    SubRoot.Right.Right = new VDataNode(VE.LeftData);
                    CircleCheckList = new VDataNode[] { (VDataNode)SubRoot.Left, (VDataNode)SubRoot.Right.Left, (VDataNode)SubRoot.Right.Right };
                }

                //3. Apply subtree
                if (C.Parent == null)
                    return SubRoot;
                C.Parent.Replace(C, SubRoot);
                return Root;
            }
            public static VNode ProcessCircleEvent(VCircleEvent e, VNode Root, VoronoiGraph VG, double ys, out VDataNode[] CircleCheckList)
            {
                VDataNode a, b, c;
                VEdgeNode eu, eo;
                b = e.NodeN;
                a = VNode.LeftDataNode(b);
                c = VNode.RightDataNode(b);
                if (a == null || b.Parent == null || c == null || !a.DataPoint.Equals(e.NodeL.DataPoint) || !c.DataPoint.Equals(e.NodeR.DataPoint))
                {
                    CircleCheckList = new VDataNode[] { };
                    return Root; // Abbruch da sich der Graph verändert hat
                }
                eu = (VEdgeNode)b.Parent;
                CircleCheckList = new VDataNode[] { a, c };
                //1. Create the new Vertex
                Vector VNew = new Vector(e.Center[0], e.Center[1]);
                //			VNew[0] = Fortune.ParabolicCut(a.DataPoint[0],a.DataPoint[1],c.DataPoint[0],c.DataPoint[1],ys);
                //			VNew[1] = (ys + a.DataPoint[1])/2 - 1/(2*(ys-a.DataPoint[1]))*(VNew[0]-a.DataPoint[0])*(VNew[0]-a.DataPoint[0]);
                VG.Vertizes.Add(VNew);
                //2. Find out if a or c are in a distand part of the tree (the other is then b's sibling) and assign the new vertex
                if (eu.Left == b) // c is sibling
                {
                    eo = VNode.EdgeToRightDataNode(a);

                    // replace eu by eu's Right
                    eu.Parent.Replace(eu, eu.Right);
                }
                else // a is sibling
                {
                    eo = VNode.EdgeToRightDataNode(b);

                    // replace eu by eu's Left
                    eu.Parent.Replace(eu, eu.Left);
                }
                eu.Edge.AddVertex(VNew);
                //			///////////////////// uncertain
                //			if(eo==eu)
                //				return Root;
                //			/////////////////////
                eo.Edge.AddVertex(VNew);
                //2. Replace eo by new Edge
                VoronoiEdge VE = new VoronoiEdge();
                VE.LeftData = a.DataPoint;
                VE.RightData = c.DataPoint;
                VE.AddVertex(VNew);
                VG.Edges.Add(VE);

                VEdgeNode VEN = new VEdgeNode(VE, false);
                VEN.Left = eo.Left;
                VEN.Right = eo.Right;
                if (eo.Parent == null)
                    return VEN;
                eo.Parent.Replace(eo, VEN);
                return Root;
            }
            public static VCircleEvent CircleCheckDataNode(VDataNode n, double ys)
            {
                VDataNode l = VNode.LeftDataNode(n);
                VDataNode r = VNode.RightDataNode(n);
                if (l == null || r == null || l.DataPoint == r.DataPoint || l.DataPoint == n.DataPoint || n.DataPoint == r.DataPoint)
                    return null;
                if (MathTools.ccw(l.DataPoint[0], l.DataPoint[1], n.DataPoint[0], n.DataPoint[1], r.DataPoint[0], r.DataPoint[1], false) <= 0)
                    return null;
                Vector Center = Voronoi.CircumCircleCenter(l.DataPoint, n.DataPoint, r.DataPoint);
                VCircleEvent VC = new VCircleEvent();
                VC.NodeN = n;
                VC.NodeL = l;
                VC.NodeR = r;
                VC.Center = Center;
                VC.Valid = true;
                if (VC.Y >= ys)
                    return VC;
                return null;
            }
        }

        private class VDataNode : VNode
        {
            public VDataNode(Vector DP)
            {
                this.DataPoint = DP;
            }
            public Vector DataPoint;
        }

        private class VEdgeNode : VNode
        {
            public VEdgeNode(VoronoiEdge E, bool Flipped)
            {
                this.Edge = E;
                this.Flipped = Flipped;
            }
            public VoronoiEdge Edge;
            public bool Flipped;
            public double Cut(double ys, double x)
            {
                if (!Flipped)
                    return Math.Round(x - Voronoi.ParabolicCut(Edge.LeftData[0], Edge.LeftData[1], Edge.RightData[0], Edge.RightData[1], ys), 10);
                return Math.Round(x - Voronoi.ParabolicCut(Edge.RightData[0], Edge.RightData[1], Edge.LeftData[0], Edge.LeftData[1], ys), 10);
            }
        }


        private abstract class VEvent : IComparable
        {
            public abstract double Y { get;}
            public abstract double X { get;}
            #region IComparable Members

            public int CompareTo(object obj)
            {
                if (!(obj is VEvent))
                    throw new ArgumentException("obj not VEvent!");
                int i = Y.CompareTo(((VEvent)obj).Y);
                if (i != 0)
                    return i;
                return X.CompareTo(((VEvent)obj).X);
            }

            #endregion
        }

        private class VDataEvent : VEvent
        {
            public Vector DataPoint;
            public VDataEvent(Vector DP)
            {
                this.DataPoint = DP;
            }
            public override double Y
            {
                get
                {
                    return DataPoint[1];
                }
            }

            public override double X
            {
                get
                {
                    return DataPoint[0];
                }
            }

        }

        private class VCircleEvent : VEvent
        {
            public VDataNode NodeN, NodeL, NodeR;
            public Vector Center;
            public override double Y
            {
                get
                {
                    return Math.Round(Center[1] + MathTools.Dist(NodeN.DataPoint[0], NodeN.DataPoint[1], Center[0], Center[1]), 10);
                }
            }

            public override double X
            {
                get
                {
                    return Center[0];
                }
            }

            public bool Valid = true;
        }
        #endregion

        #region MathTools Classes
        private abstract class MathTools
        {
            /// <summary>
            /// One static Random instance for use in the entire application
            /// </summary>
            public static readonly Random R = new Random((int)DateTime.Now.Ticks);
            public static double Dist(double x1, double y1, double x2, double y2)
            {
                return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
            }
            public static IList Shuffle(IList S, Random R, bool Copy)
            {
                //			if(S.Rank>1)
                //				throw new Exception("Shuffle only defined on one-dimensional arrays!");
                IList E;
                E = S;
                if (Copy)
                {
                    if (S is ICloneable)
                        E = ((ICloneable)S).Clone() as IList;
                    else
                        throw new Exception("You want it copied, but it can't!");
                }
                int i, r;
                object Temp;
                for (i = 0; i < E.Count - 1; i++)
                {
                    r = i + R.Next(E.Count - i);
                    if (r == i)
                        continue;
                    Temp = E[i];
                    E[i] = E[r];
                    E[r] = Temp;
                }
                return E;
            }
            public static void ShuffleIList(IList A, Random R)
            {
                Shuffle(A, R, false);
            }
            public static void ShuffleIList(IList A)
            {
                Shuffle(A, new Random((int)DateTime.Now.Ticks), false);
            }
            public static IList Shuffle(IList A, bool Copy)
            {
                return Shuffle(A, new Random((int)DateTime.Now.Ticks), Copy);
            }
            public static IList Shuffle(IList A)
            {
                return Shuffle(A, new Random((int)DateTime.Now.Ticks), true);
            }

            public static int[] GetIntArrayRange(int A, int B)
            {
                int[] E = new int[B - A + 1];
                int i;
                for (i = A; i <= B; i++)
                    E[i - A] = i;
                return E;
            }

            public static int[] GetIntArrayConst(int A, int n)
            {
                int[] E = new int[n];
                int i;
                for (i = 0; i < n; i++)
                    E[i] = A;
                return E;
            }


            public static int[] GetIntArray(params int[] P)
            {
                return P;
            }

            public static object[] GetArray(params object[] P)
            {
                return P;
            }
            public static Array CopyToArray(ICollection L, Type T)
            {
                Array Erg = Array.CreateInstance(T, L.Count);
                L.CopyTo(Erg, 0);
                return Erg;
            }
            public static string[] HighLevelSplit(string S, params char[] C)
            {
                ArrayList Erg = new ArrayList();
                Stack CurrentBracket = new Stack();
                int Pos = 0;
                int i, c;

                for (i = 0; i < S.Length; i++)
                {
                    if (S[i] == '(')
                    {
                        CurrentBracket.Push(0);
                        continue;
                    }
                    if (S[i] == '[')
                    {
                        CurrentBracket.Push(1);
                        continue;
                    }
                    if (S[i] == '{')
                    {
                        CurrentBracket.Push(2);
                        continue;
                    }
                    if (S[i] == ')')
                    {
                        if ((int)CurrentBracket.Pop() != 0)
                            throw new Exception("Formatfehler!");
                        continue;
                    }
                    if (S[i] == ']')
                    {
                        if ((int)CurrentBracket.Pop() != 1)
                            throw new Exception("Formatfehler!");
                        continue;
                    }
                    if (S[i] == '}')
                    {
                        if ((int)CurrentBracket.Pop() != 2)
                            throw new Exception("Formatfehler!");
                        continue;
                    }
                    if (CurrentBracket.Count > 0)
                        continue;
                    c = Array.IndexOf(C, S[i]);
                    if (c != -1)
                    {
                        if (C[c] == '\n')
                        {
                            if (i - 2 >= Pos)
                                Erg.Add(S.Substring(Pos, i - Pos - 1));
                            Pos = i + 1;
                        }
                        else
                        {
                            if (i - 1 >= Pos)
                                Erg.Add(S.Substring(Pos, i - Pos));
                            Pos = i + 1;
                        }
                    }
                }
                if (CurrentBracket.Count > 0)
                    throw new Exception("Formatfehler!");
                if (i - 1 >= Pos)
                    Erg.Add(S.Substring(Pos, i - Pos));
                return (string[])CopyToArray(Erg, typeof(string));
            }

            public static RectangleF MaxRectangleFit(RectangleF Target, SizeF Source)
            {
                float W, H;
                // 1. Auf höhe probieren
                H = Target.Height;
                W = Target.Height / Source.Height * Source.Width;
                if (W <= Target.Width)
                {
                    return new RectangleF(Target.X + Target.Width / 2 - W / 2, Target.Y, W, H);
                }
                // 2. Auf weite probieren
                W = Target.Width;
                H = Target.Width / Source.Width * Source.Height;
                return new RectangleF(Target.X, Target.Y + Target.Height / 2 - H / 2, W, H);

            }
            public static double DASkalar(double[] A, double[] B)
            {
                if (A.Length != B.Length)
                    throw new Exception("Error in Skalar!");
                double E = 0;
                int i;
                for (i = 0; i < A.Length; i++)
                {
                    E += A[i] * B[i];
                }
                return E;
            }
            public static double[] DAMult(double[] A, double r)
            {
                double[] E = new double[A.Length];
                int i;
                for (i = 0; i < E.Length; i++)
                {
                    E[i] = A[i] * r;
                }
                return E;
            }

            public static double[] DAAdd(double[] A, double[] B)
            {
                if (A.Length != B.Length)
                    throw new Exception("Error in Skalar!");
                double[] E = new double[A.Length];
                int i;
                for (i = 0; i < A.Length; i++)
                {
                    E[i] += A[i] + B[i];
                }
                return E;
            }

            public static double DADist(double[] A, double[] B)
            {
                if (A.Length != B.Length)
                    throw new Exception("Unterschiedliche Längen!");
                int i;
                double E = 0;
                for (i = 0; i < A.Length; i++)
                    E += (A[i] - B[i]) * (A[i] - B[i]);
                return E;
            }

            public static double DASum(double[] A)
            {
                double Erg = 0;
                foreach (double D in A)
                {
                    Erg += D;
                }
                return Erg;
            }

            public static double DAMean(double[] A)
            {
                return DASum(A) / (double)A.Length;
            }

            public static double DAStdv(double[] A, double M)
            {
                double Erg = 0;
                foreach (double D in A)
                    Erg += (M - D) * (M - D);
                return Erg / (double)A.Length;
            }
            private static int doubleToInt(double f)
            {
                if (f >= 2.147484E+09f)
                {
                    return 2147483647;
                }
                if (f <= -2.147484E+09f)
                {
                    return -2147483648;
                }
                return ((int)f);
            }

            // 0: minimum, +: rising, -: falling, 1: maximum. 
            private static char[][] HSB_map = new char[6][]{new char[]{'1', '+', '0'},
											new char[]{'-', '1', '0'},
											new char[]{'0', '1', '+'},
											new char[]{'0', '-', '1'},
											new char[]{'+', '0', '1'},
											new char[]{'1', '0', '-'}};

            public static double[] HSBtoRGB(int hue, int saturation, int brightness, double[] OldCol)
            {
                // Clip hue at 360: 
                if (hue < 0)
                    hue = 360 - (-hue % 360);
                hue = hue % 360;

                int i = (int)Math.Floor(hue / 60.0), j;
                double[] C;
                if (OldCol == null || OldCol.Length != 3)
                    C = new double[3];
                else
                    C = OldCol;

                double min = 127.0 * (240.0 - saturation) / 240.0;
                double max = 255.0 - 127.0 * (240.0 - saturation) / 240.0;
                if (brightness > 120)
                {
                    min = min + (255.0 - min) * (brightness - 120) / 120.0;
                    max = max + (255.0 - max) * (brightness - 120) / 120.0;
                }
                if (brightness < 120)
                {
                    min = min * brightness / 120.0;
                    max = max * brightness / 120.0;
                }

                for (j = 0; j < 3; j++)
                {
                    switch (HSB_map[i][j])
                    {
                        case '0':
                            C[j] = min;
                            break;
                        case '1':
                            C[j] = max;
                            break;
                        case '+':
                            C[j] = (min + (hue % 60) / 60.0 * (max - min));
                            break;
                        case '-':
                            C[j] = (max - (hue % 60) / 60.0 * (max - min));
                            break;
                    }
                }
                return C;
            }
            public static Color HSBtoRGB(int hue, int saturation, int brightness)
            {
                double[] C = HSBtoRGB(hue, saturation, brightness, null);
                return Color.FromArgb((int)C[0], (int)C[1], (int)C[2]);
            }
            public static double GetAngle(double x, double y)
            {
                if (x == 0)
                {
                    if (y > 0)
                        return Math.PI / 2.0;
                    if (y == 0)
                        return 0;
                    if (y < 0)
                        return Math.PI * 3.0 / 2.0;
                }
                double atan = Math.Atan(y / x);
                if (x > 0 && y >= 0)
                    return atan;
                if (x > 0 && y < 0)
                    return 2 * Math.PI + atan;
                return Math.PI + atan;
            }
            public static double GetAngleTheta(double x, double y)
            {
                double dx, dy, ax, ay;
                double t;
                dx = x; ax = Math.Abs(dx);
                dy = y; ay = Math.Abs(dy);
                t = (ax + ay == 0) ? 0 : dy / (ax + ay);
                if (dx < 0) t = 2 - t; else if (dy < 0) t = 4 + t;
                return t * 90.0;
            }
            public static int ccw(Point P0, Point P1, Point P2, bool PlusOneOnZeroDegrees)
            {
                int dx1, dx2, dy1, dy2;
                dx1 = P1.X - P0.X; dy1 = P1.Y - P0.Y;
                dx2 = P2.X - P0.X; dy2 = P2.Y - P0.Y;
                if (dx1 * dy2 > dy1 * dx2) return +1;
                if (dx1 * dy2 < dy1 * dx2) return -1;
                if ((dx1 * dx2 < 0) || (dy1 * dy2 < 0)) return -1;
                if ((dx1 * dx1 + dy1 * dy1) < (dx2 * dx2 + dy2 * dy2) && PlusOneOnZeroDegrees)
                    return +1;
                return 0;
            }
            public static int ccw(double P0x, double P0y, double P1x, double P1y, double P2x, double P2y, bool PlusOneOnZeroDegrees)
            {
                double dx1, dx2, dy1, dy2;
                dx1 = P1x - P0x; dy1 = P1y - P0y;
                dx2 = P2x - P0x; dy2 = P2y - P0y;
                if (dx1 * dy2 > dy1 * dx2) return +1;
                if (dx1 * dy2 < dy1 * dx2) return -1;
                if ((dx1 * dx2 < 0) || (dy1 * dy2 < 0)) return -1;
                if ((dx1 * dx1 + dy1 * dy1) < (dx2 * dx2 + dy2 * dy2) && PlusOneOnZeroDegrees)
                    return +1;
                return 0;
            }

            public static bool intersect(Point P11, Point P12, Point P21, Point P22)
            {
                return ccw(P11, P12, P21, true) * ccw(P11, P12, P22, true) <= 0
                    && ccw(P21, P22, P11, true) * ccw(P21, P22, P12, true) <= 0;
            }

            public static PointF IntersectionPoint(Point P11, Point P12, Point P21, Point P22)
            {
                double Kx = P11.X, Ky = P11.Y, Mx = P21.X, My = P21.Y;
                double Lx = (P12.X - P11.X), Ly = (P12.Y - P11.Y), Nx = (P22.X - P21.X), Ny = (P22.Y - P21.Y);
                double a = double.NaN, b = double.NaN;
                if (Lx == 0)
                {
                    if (Nx == 0)
                        throw new Exception("No intersect!");
                    b = (Kx - Mx) / Nx;
                }
                else if (Ly == 0)
                {
                    if (Ny == 0)
                        throw new Exception("No intersect!");
                    b = (Ky - My) / Ny;
                }
                else if (Nx == 0)
                {
                    if (Lx == 0)
                        throw new Exception("No intersect!");
                    a = (Mx - Kx) / Lx;
                }
                else if (Ny == 0)
                {
                    if (Ly == 0)
                        throw new Exception("No intersect!");
                    a = (My - Ky) / Ly;
                }
                else
                {
                    b = (Ky + Mx * Ly / Lx - Kx * Ly / Lx - My) / (Ny - Nx * Ly / Lx);
                }
                if (!double.IsNaN(a))
                {
                    return new PointF((float)(Kx + a * Lx), (float)(Ky + a * Ly));
                }
                if (!double.IsNaN(b))
                {
                    return new PointF((float)(Mx + b * Nx), (float)(My + b * Ny));
                }
                throw new Exception("Error in IntersectionPoint");
            }
        }
        #endregion
    }
*/
}
