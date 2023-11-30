using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;

namespace gView.Framework.Core.Data.Filters
{
    public interface ISpatialFilter : IQueryFilter
    {
        IGeometry Geometry { get; set; }
        //IGeometry GeometryEx { get; }
        ISpatialReference FilterSpatialReference { get; set; }
        //double BufferDistance { get; set ; }
        //bool FuzzyQuery { get ; set ; }
        spatialRelation SpatialRelation { get; set; }

        bool IgnoreFeatureCursorCheckIntersection { get; set; }
    }
}
