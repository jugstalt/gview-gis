using gView.Framework.Data;
using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Data.Framework.Data.Extensions
{
    static public class QueryFilterExtensions
    {
        static public void GeometryToSpatialReference(this IQueryFilter filter, ISpatialReference targetSRef)
        {
            if (targetSRef != null &&
               filter is ISpatialFilter &&
               ((ISpatialFilter)filter).Geometry != null &&
               ((ISpatialFilter)filter).FilterSpatialReference !=null &&
               !targetSRef.Equals(((ISpatialFilter)filter).FilterSpatialReference))
            {
                var spatialFilter = (ISpatialFilter)filter;

                spatialFilter.Geometry =
                        GeometricTransformerFactory.Transform2D(
                            spatialFilter.Geometry,
                            spatialFilter.FilterSpatialReference,
                            targetSRef);
                spatialFilter.FilterSpatialReference = targetSRef;
            }
        }
    }
}
