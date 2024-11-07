using System;
using System.Collections.Generic;
using gView.Framework.Core.Geometry;

namespace gView.Framework.Geometry
{
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
        public GeometryType GeometryType
        {
            get
            {
                return GeometryType.Multipoint;
            }
        }

        public int VertexCount => base.PointCount;

        public void Clean(CleanGemetryMethods methods, double tolerance = 1e-8)
        {
            if(methods.HasFlag(CleanGemetryMethods.IdentNeighbors))
            {
                base.RemoveIdentNeighbors(tolerance);
            }
        }

        public bool IsEmpty() => this.PointCount == 0;

        #endregion

        #region ICloneable Member

        public object Clone()
        {
            return new MultiPoint(_points);
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
                IPolygon buffer = GeoProcessing.Algorithm.PointBuffer(this[i], distance);
                if (buffer == null)
                {
                    continue;
                }

                polygons.Add(buffer);
            }
            //return gView.Framework.SpatialAlgorithms.Algorithm.MergePolygons(polygons);
            return GeoProcessing.Algorithm.FastMergePolygon(polygons, null, null);
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
}
