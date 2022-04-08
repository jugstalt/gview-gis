using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

//[assembly: InternalsVisibleTo("gView.OGC, PublicKey=0024000004800000940000000602000000240000525341310004000001000100916d0be3f662c2d3589fbe93479f3215e23fd195db9a20e77f42dc1d2942bd48cad3ea36b797f57880e6c31af0c238d2e445898c8ecce990aacbb70ae05a10aff73ab65c7db86366697f934b780238ed8fd1b2e28ba679a97e060b53fce66118e129b91d24f392d4dd3d482fa4173e61f18c74cda9f35721a97e77afbbc96dd2")]


namespace gView.Framework.Geometry
{
    /// <summary>
    /// An orderd collection of geometry objects.
    /// </summary>
    public sealed class AggregateGeometry : IAggregateGeometry, ITopologicalOperation
    {
        private List<IGeometry> _childGeometries;

        public AggregateGeometry()
        {
            _childGeometries = new List<IGeometry>();
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

            _childGeometries.Add(geometry);
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

            if (pos > _childGeometries.Count)
            {
                pos = _childGeometries.Count;
            }

            if (pos < 0)
            {
                pos = 0;
            }

            _childGeometries.Insert(pos, geometry);
        }

        /// <summary>
        /// Removes the geometry at the given position. 
        /// </summary>
        /// <param name="pos"></param>
        public void RemoveGeometry(int pos)
        {
            if (pos < 0 || pos >= _childGeometries.Count)
            {
                return;
            }

            _childGeometries.RemoveAt(pos);
        }

        /// <summary>
        /// The number of geometry objects.
        /// </summary>
        public int GeometryCount
        {
            get
            {
                return _childGeometries == null ? 0 : _childGeometries.Count;
            }
        }

        /// <summary>
        /// Returns the geometry object at the given position.
        /// </summary>
        public IGeometry this[int geometryIndex]
        {
            get
            {
                if (geometryIndex < 0 || geometryIndex >= _childGeometries.Count)
                {
                    return null;
                }

                return _childGeometries[geometryIndex];
            }
        }

        public List<IPoint> PointGeometries
        {
            get
            {
                List<IPoint> points = new List<IPoint>();

                foreach (IGeometry geom in _childGeometries)
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
                foreach (IGeometry geom in _childGeometries)
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
                foreach (IGeometry geom in _childGeometries)
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
        public gView.Framework.Geometry.GeometryType GeometryType
        {
            get
            {
                return GeometryType.Aggregate;
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

        public int VertexCount => GeometryCount == 0 ? 0 : _childGeometries.Sum(g => g.VertexCount);

        /// <summary>
        /// For the internal use of the framework
        /// </summary>
        /// <param name="w"></param>
        public void Serialize(BinaryWriter w, IGeometryDef geomDef)
        {
            w.Write(_childGeometries.Count);
            foreach (IGeometry geom in _childGeometries)
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
            _childGeometries.Clear();

            int geoms = r.ReadInt32();
            for (int i = 0; i < geoms; i++)
            {
                IGeometry geom = null;
                switch ((GeometryType)r.ReadInt32())
                {
                    case GeometryType.Aggregate:
                        geom = new AggregateGeometry();
                        break;
                    case GeometryType.Envelope:
                        geom = new Envelope();
                        break;
                    case GeometryType.Multipoint:
                        geom = new MultiPoint();
                        break;
                    case GeometryType.Point:
                        geom = new Point();
                        break;
                    case GeometryType.Polygon:
                        geom = new Polygon();
                        break;
                    case GeometryType.Polyline:
                        geom = new Polyline();
                        break;
                }
                if (geom != null)
                {
                    geom.Deserialize(r, geomDef);
                    _childGeometries.Add(geom);
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
            foreach (IGeometry geom in _childGeometries)
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
            if (mPoint != null && mPoint.PointCount > 0)
            {
                polygons.Add(((ITopologicalOperation)mPoint).Buffer(distance));
            }

            if (mPolyline != null && mPolyline.PathCount > 0)
            {
                polygons.Add(((ITopologicalOperation)mPolyline).Buffer(distance));
            }

            if (mPolygon != null && mPolygon.RingCount > 0)
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
            return _childGeometries.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _childGeometries.GetEnumerator();
        }

        #endregion
    }
}
