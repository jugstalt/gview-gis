using System;
using System.Collections.Generic;

//[assembly: InternalsVisibleTo("gView.OGC, PublicKey=0024000004800000940000000602000000240000525341310004000001000100916d0be3f662c2d3589fbe93479f3215e23fd195db9a20e77f42dc1d2942bd48cad3ea36b797f57880e6c31af0c238d2e445898c8ecce990aacbb70ae05a10aff73ab65c7db86366697f934b780238ed8fd1b2e28ba679a97e060b53fce66118e129b91d24f392d4dd3d482fa4173e61f18c74cda9f35721a97e77afbbc96dd2")]


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
        public gView.Framework.Geometry.GeometryType GeometryType
        {
            get
            {
                return GeometryType.Multipoint;
            }
        }

        public int VertexCount => base.PointCount;

        #endregion

        #region ICloneable Member

        public object Clone()
        {
            return new MultiPoint(m_points);
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
                IPolygon buffer = gView.Framework.SpatialAlgorithms.Algorithm.PointBuffer(this[i], distance);
                if (buffer == null)
                {
                    continue;
                }

                polygons.Add(buffer);
            }
            //return gView.Framework.SpatialAlgorithms.Algorithm.MergePolygons(polygons);
            return gView.Framework.SpatialAlgorithms.Algorithm.FastMergePolygon(polygons, null, null);
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
