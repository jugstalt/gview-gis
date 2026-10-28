using gView.Framework.Core.Geometry;
using gView.Framework.Geometry.GeoProcessing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gView.Framework.Geometry
{
    /// <summary>
    /// An orderd collection of paths.
    /// </summary>
    public sealed class Polyline : IPolyline, ITopologicalOperation, SpatialNET.IPolyline2D
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
                return _paths == null ? 0 : _paths.Count;
            }
        }

        public IEnumerable<IPath> Paths { get { return _paths; } }

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

        // Foreign IPath implementations are copied, gView paths are passed through
        SpatialNET.IPath2D SpatialNET.IPolyline2D.this[int index]
        {
            get
            {
                IPath path = _paths[index];
                return path as SpatialNET.IPath2D ?? new Path(path);
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
        public GeometryType GeometryType
        {
            get
            {
                return GeometryType.Polyline;
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


        public int VertexCount => PathCount == 0 ? 0 : _paths.Sum(p => p.PointCount);

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

        public void Clean(CleanGemetryMethods methods, double tolerance = 1e-8)
        {
            if (methods.HasFlag(CleanGemetryMethods.IdentNeighbors))
            {
                foreach (var path in _paths)
                {
                    path.RemoveIdentNeighbors(tolerance);
                }
            }

            if (methods.HasFlag(CleanGemetryMethods.ZeroParts))
            {
                var paths = new List<IPath>();

                foreach (var path in _paths)
                {
                    if (path.Length > tolerance)
                    {
                        paths.Add(path);
                    }
                }

                _paths = paths;
            }
        }

        public bool IsEmpty() => this.PathCount == 0;

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
            // SpatialNET buffers all paths in one pass, without merging single path buffers
            if (Engine.GeometryEngineOptions.UseSpatialNetForBuffer)
            {
                return Engine.SpatialNetBridge.Buffer(this, distance);
            }

            IPolygon buffer = Algorithm.PolylineBuffer(this, distance);
            return buffer;
        }

        public void Clip(IEnvelope clipper)
        {
            IGeometry result;
            Clip(clipper, out result);
            CopyGeometry((IPolyline)result);
        }

        // Intersect/Difference/SymDifference/Union run on SpatialNET (ADR 0002, phase 1).
        // The in-place variants need a polyline result; the out variants return whatever the operation yields.

        /// <exception cref="NotSupportedException">The geometry is a point or multipoint: the result would be points.</exception>
        public void Intersect(IGeometry geometry)
        {
            Intersect(geometry, out IGeometry result);
            CopyGeometry(AsPolyline(result, nameof(Intersect)));
        }

        public void Difference(IGeometry geometry)
        {
            // Polyline minus anything is a polyline
            Difference(geometry, out IGeometry result);
            CopyGeometry((IPolyline)result);
        }

        /// <exception cref="NotSupportedException">The geometry is not a polyline: the result would mix dimensions.</exception>
        public void SymDifference(IGeometry geometry)
        {
            SymDifference(geometry, out IGeometry result);
            CopyGeometry((IPolyline)result);
        }

        /// <exception cref="NotSupportedException">The geometry is not a polyline: the result would mix dimensions.</exception>
        public void Union(IGeometry geometry)
        {
            Union(geometry, out IGeometry result);
            CopyGeometry((IPolyline)result);
        }

        private static IPolyline AsPolyline(IGeometry result, string operation)
        {
            return result as IPolyline
                ?? throw new NotSupportedException($"{operation} yields a {result.GeometryType}, which cannot replace a polyline. Use the overload with an out result.");
        }

        public void Clip(IEnvelope clipper, out IGeometry result)
        {
            IGeometry polyline = GeoProcessing.Clip.PerformClip(clipper, this);
            if (!(polyline is IPolyline))
            {
                polyline = null;
            }

            result = polyline;
        }

        /// <summary>Polyline with polygon or polyline gives a polyline, with points a multipoint.</summary>
        public void Intersect(IGeometry geometry, out IGeometry result)
        {
            result = Engine.SpatialNetBridge.Intersection(this, geometry);
        }

        public void Difference(IGeometry geometry, out IGeometry result)
        {
            result = Engine.SpatialNetBridge.Difference(this, geometry);
        }

        /// <exception cref="NotSupportedException">The geometry is not a polyline: the result would mix dimensions.</exception>
        public void SymDifference(IGeometry geometry, out IGeometry result)
        {
            result = Engine.SpatialNetBridge.SymDifference(this, geometry);
        }

        /// <exception cref="NotSupportedException">The geometry is not a polyline: the result would mix dimensions.</exception>
        public void Union(IGeometry geometry, out IGeometry result)
        {
            result = Engine.SpatialNetBridge.Union(this, geometry);
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

        public override int GetHashCode()
        {
            return base.GetHashCode();
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

        public double Distance2D(IPolyline candidate)
        {
            if (candidate == null || candidate.PathCount == 0 || this.PathCount == 0)
            {
                return double.MaxValue;
            }

            double dist = double.MaxValue;
            foreach (var candidatePath in candidate.Paths)
            {
                foreach (var candidatePoint in candidatePath.ToArray())
                {
                    dist = Math.Min(Algorithm.Point2ShapeDistance(this, candidatePoint), dist);
                }
            }
            foreach (var path in this._paths)
            {
                foreach (var point in path.ToArray())
                {
                    dist = Math.Min(Algorithm.Point2ShapeDistance(candidate, point), dist);
                }
            }
            return dist;
        }
    }
}
