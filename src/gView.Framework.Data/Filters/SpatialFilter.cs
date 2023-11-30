using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.system;
using gView.Framework.Geometry;

namespace gView.Framework.Data.Filters
{
    public class SpatialFilter : QueryFilter, ISpatialFilter, IClone
    {
        private IGeometry m_geom = null;
        //private double m_bufferDist;
        //private bool m_fuzzy=false;
        private ISpatialReference _sRef = null;
        private spatialRelation _spatialRelation = spatialRelation.SpatialRelationIntersects;

        public SpatialFilter()
            : base()
        {
            //m_bufferDist = 0.0;
            //_spatialRelation = spatialRelation.SpatialRelationIntersects;
        }

        public SpatialFilter(IQueryFilter filter) :
            base(filter)
        {
            ISpatialFilter spatialFilter = filter as ISpatialFilter;
            if (spatialFilter == null)
            {
                return;
            }

            this.Geometry = spatialFilter.Geometry;
            this.FilterSpatialReference = spatialFilter.FilterSpatialReference;
            this.SpatialRelation = spatialFilter.SpatialRelation;
            this.IgnoreFeatureCursorCheckIntersection=spatialFilter.IgnoreFeatureCursorCheckIntersection;
        }

        #region ISpatialFilter Member

        public IGeometry Geometry
        {
            get
            {
                return m_geom;
            }
            set
            {
                m_geom = value;
            }
        }

        //public double BufferDistance 
        //{
        //    get { return m_bufferDist; }
        //    set { m_bufferDist=value; }
        //}

        //public bool FuzzyQuery 
        //{
        //    get { return m_fuzzy; }
        //    set { m_fuzzy=value; }
        //}

        public override object Clone()
        {
            SpatialFilter filter = new SpatialFilter(this);
            return filter;
        }

        public ISpatialReference FilterSpatialReference
        {
            get
            {
                return _sRef;
            }
            set
            {
                _sRef = value;
            }
        }

        //public IGeometry GeometryEx
        //{
        //    get { return null; }
        //}

        public spatialRelation SpatialRelation
        {
            get
            {
                return _spatialRelation;
            }
            set
            {
                _spatialRelation = value;
            }
        }

        public bool IgnoreFeatureCursorCheckIntersection { get; set; } = false;

        #endregion

        public static ISpatialFilter Project(ISpatialFilter filter, ISpatialReference to)
        {
            if (filter == null ||
                to == null || filter.FilterSpatialReference == null ||
                to.Equals(filter.FilterSpatialReference))
            {
                return filter;
            }

            SpatialFilter pFilter = new SpatialFilter(filter);
            pFilter.FilterSpatialReference = to;

            pFilter.Geometry = GeometricTransformerFactory.Transform2D(
                filter.Geometry,
                filter.FilterSpatialReference,
                to);

            return pFilter;
        }
    }
}
