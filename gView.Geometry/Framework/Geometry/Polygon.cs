using gView.Framework.SpatialAlgorithms;
using gView.Framework.SpatialAlgorithms.Clipper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gView.Framework.Geometry
{
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
                return _rings == null ? 0 : _rings.Count;
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

        public int TotalPointCount
        {
            get
            {
                if (_rings == null)
                {
                    return 0;
                }

                return _rings
                    .Where(r => r != null)
                    .Select(r => r.PointCount)
                    .Sum();
            }
        }

        public void CloseAllRings(double tolerance = GeometryConst.Epsilon)
        {
            if (_rings != null)
            {
                foreach (var ring in _rings)
                {
                    ring.Close(tolerance);
                }
            }
        }

        public void MakeValid__(double tolerance = GeometryConst.Epsilon)
        {
            List<IRing> v = _rings;
            _rings = new List<IRing>();

            foreach (var ring in v)
            {
                _rings.AddRange(Algorithm.SplitRing__(ring, tolerance));
            }

            VerifyHoles();
        }

        public void RemoveLineArtifacts(double tolerance = GeometryConst.Epsilon)
        {
            var  v = _rings;
            _rings = new List<IRing>();

            foreach (var ring in v)
            {
                _rings.AddRange(Algorithm.RemoveLineArtifacts(ring, tolerance));
            }
        }

        //public void MakeValid(double tolerance = GeometryConst.Epsilon)
        //{
        //    var newRings = new List<IRing>();

        //    foreach (IRing ring in _rings)
        //    {
        //        var newRing = new Ring();
        //        IPoint startPoint = null;

        //        for (int p = 0, pointCount = ring.PointCount; p < pointCount; p++)
        //        {
        //            if (p == 0)
        //            {
        //                newRing.AddPoint(startPoint = ring[p]);
        //            }
        //            else if (p < pointCount - 1 && startPoint.Distance(ring[p]) < tolerance)
        //            {
        //                newRings.Add(newRing);

        //                newRing = new Ring();
        //                newRing.AddPoint(startPoint = ring[p]);
        //            }
        //            else
        //            {
        //                newRing.AddPoint(ring[p]);
        //            }
        //        }

        //        newRings.Add(newRing);
        //    }

        //    _rings = new List<IRing>(newRings.Where(r => r.Area > 0.0));
        //}


        #endregion

        #region IGeometry Member

        /// <summary>
        /// The type of the geometry (Polygon).
        /// </summary>
        public gView.Framework.Geometry.GeometryType GeometryType
        {
            get
            {
                return GeometryType.Polygon;
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

        public int VertexCount => RingCount == 0 ? 0 : _rings.Sum(r => r.PointCount);


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


        public IEnumerable<IRing> OuterRings()
        {
            VerifyHoles();

            return _rings.Where(r => !(r is IHole));
        }

        public IEnumerable<IHole> InnerRings(IRing ring)
        {
            VerifyHoles();

            if (ring is IHole)
            {
                return new IHole[0];
            }

            List<IHole> result = new List<IHole>();
            foreach (IHole hole in _rings.Where(r => r is IHole))
            {
                if (SpatialAlgorithms.Algorithm.Jordan(ring, hole))
                {
                    result.Add(hole);
                }
            }
            return result;
        }

        public int OuterRingCount
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
        public int InnerRingCount
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

        public IEnumerable<IRing> Rings { get { return _rings; } }
        public IEnumerable<IHole> Holes
        {
            get
            {
                VerifyHoles();
                return _rings.Where(r => (r is IHole)).Select(h => (IHole)h);
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
            return Equals(obj, 1e-11);
        }
        //public bool Equals(object obj, double epsi)
        //{
        //    if (obj is IPolygon)
        //    {
        //        IPolygon polygon = (IPolygon)obj;
        //        if (polygon.RingCount != this.RingCount)
        //        {
        //            return false;
        //        }

        //        for (int i = 0; i < this.RingCount; i++)
        //        {
        //            IRing r1 = this[i];
        //            IRing r2 = polygon[i];

        //            if (!r1.Equals(r2, epsi))
        //            {
        //                return false;
        //            }
        //        }

        //        return true;
        //    }
        //    return false;
        //}

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

        public double Distance2D(IPolygon candidate)
        {
            if (candidate == null || candidate.RingCount == 0 || this.RingCount == 0)
            {
                return double.MaxValue;
            }

            double dist = double.MaxValue;
            foreach (var candidateRing in candidate.Rings)
            {
                foreach (var candidatePoint in candidateRing.ToArray())
                {
                    dist = Math.Min(Algorithm.Point2ShapeDistance(this, candidatePoint), dist);
                }
            }
            foreach (var ring in this._rings)
            {
                foreach (var point in ring.ToArray())
                {
                    dist = Math.Min(Algorithm.Point2ShapeDistance(candidate, point), dist);
                }
            }
            return dist;
        }

        public bool Equals(object obj, double epsi)
        {
            if (obj is Polygon)
            {
                var polygon = (Polygon)obj;

                if (polygon.RingCount != this.RingCount)
                {
                    return false;
                }

                if (Math.Abs(polygon.Area - this.Area) > epsi)
                {
                    return false;
                }

                var rings = _rings.OrderBy(r => r.Area).ToArray();
                var candidateRings = polygon._rings.OrderBy(r => r.Area).ToArray();

                for (int i = 0; i < rings.Length; i++)
                {
                    var ring = rings[i];
                    ring.ClosePath();

                    var candidateRing = candidateRings[i];
                    candidateRing.ClosePath();

                    //if (ring.PointCount != candidateRing.PointCount)
                    //    return false;

                    if (Math.Abs(ring.Area - candidateRing.Area) > epsi)
                    {
                        return false;
                    }

                    if (!ring.Envelope.Equals(candidateRing.Envelope))
                    {
                        return false;
                    }

                    // ToDo:
                    // Testen, ob die Punkte eines Rings alle auf der Kante des anderen liegen...

                    //var ringPoints = ring.ToArray();
                    //var candidatePoints = candidateRing.ToArray();

                    //foreach(var ringPoint in ringPoints)
                    //{
                    //    if (candidatePoints.Where(p => p.Equals(ringPoint)).Count() == 0)
                    //        return false;
                    //}
                }
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
