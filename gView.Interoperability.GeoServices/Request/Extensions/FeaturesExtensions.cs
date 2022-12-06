using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.OGC.WMS.Version_1_1_1;
using System.Collections.Generic;
using System.Linq;

namespace gView.Interoperability.GeoServices.Request.Extensions
{
    static internal class FeaturesExtensions
    {
        static public void GeometryMakeValid(this IEnumerable<IFeature> features, IServiceMap map, IFeatureClass featureClass)
        {
            if (features == null || features.Count() == 0)
            {
                return;
            }

            if (map.GetGeometryType(featureClass) == GeometryType.Polygon)
            {
                foreach (var feature in features)
                {
                    var polygon = feature.Shape as IPolygon;
                    
                    if (polygon == null)
                    {
                        continue;
                    }

                    var sRef = polygon.Srs.HasValue && !polygon.Srs.Equals(featureClass.SpatialReference?.EpsgCode) ?
                        SpatialReference.FromID($"epsg:{polygon.Srs.Value}") :
                        featureClass.SpatialReference ?? map.LayerDefaultSpatialReference;

                    if (sRef != null)
                    {
                        feature.Shape.Clean(CleanGemetryMethods.IdentNeighbors | CleanGemetryMethods.ZeroParts);
                    }
                }
            }
        }

        static public GeometryType GetGeometryType(this IServiceMap map, IFeatureClass featureClass)
        {
            if(featureClass.GeometryType!= GeometryType.Unknown)
            {
                return featureClass.GeometryType;
            }

            var layer = map.MapElements.Where(e=>e.Class == featureClass).FirstOrDefault() as IFeatureLayer;

            if (layer != null)
            {
                return layer.LayerGeometryType;
            }

            return GeometryType.Unknown;
        }
    }
}
