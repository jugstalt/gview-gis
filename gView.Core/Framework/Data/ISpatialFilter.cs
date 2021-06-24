using gView.Framework.Geometry;

namespace gView.Framework.Data
{
    public interface ISpatialFilter : IQueryFilter
	{
		IGeometry Geometry { get; set; }
		//IGeometry GeometryEx { get; }
		ISpatialReference FilterSpatialReference { get; set; }
		//double BufferDistance { get; set ; }
		//bool FuzzyQuery { get ; set ; }
		spatialRelation SpatialRelation { get; set; }
	}
}
