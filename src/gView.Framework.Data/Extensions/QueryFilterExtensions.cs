using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using System;
using System.Text;

namespace gView.Framework.Data.Extensions
{
    static public class QueryFilterExtensions
    {
        static public void GeometryToSpatialReference(
                    this IQueryFilter filter, 
                    ISpatialReference targetSRef,
                    IDatumTransformations datumTransformations)
        {
            if (targetSRef != null &&
               filter is ISpatialFilter &&
               ((ISpatialFilter)filter).Geometry != null &&
               ((ISpatialFilter)filter).FilterSpatialReference != null &&
               !targetSRef.Equals(((ISpatialFilter)filter).FilterSpatialReference))
            {
                var spatialFilter = (ISpatialFilter)filter;

                spatialFilter.Geometry =
                        GeometricTransformerFactory.Transform2D(
                            spatialFilter.Geometry,
                            spatialFilter.FilterSpatialReference,
                            targetSRef,
                            datumTransformations);
                spatialFilter.FilterSpatialReference = targetSRef;
            }
        }

        static public string ToSubFieldName(this string fieldName, string fieldPrefix, string fieldPostfix)
        {
            StringBuilder subField = new StringBuilder();

            if (fieldName == "*")
            {
                subField.Append(fieldName);
            }
            else
            {
                if (!fieldName.StartsWith(fieldPrefix) && !fieldName.Contains("(") && !fieldName.Contains(")"))
                {
                    subField.Append(fieldPrefix);
                }

                subField.Append(fieldName);

                if (!fieldName.EndsWith(fieldPostfix) && !fieldName.Contains("(") && !fieldName.Contains(")"))
                {
                    subField.Append(fieldPostfix);
                }
            }

            return subField.ToString();
        }

        static public bool IsSubFieldFunction(this string subField)
        {
            return !string.IsNullOrEmpty(subField) && (subField.Contains("(") || subField.Contains(")") || subField.ToLower().Contains(" as "));
        }
    }
}
