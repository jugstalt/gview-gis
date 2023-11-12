using gView.Framework.Data;
using gView.Framework.Geometry;

namespace gView.Carto.Plugins.Extensions;

internal static class LayerExtensions
{
    static public IEnumerable<ILayer> OrderLayersByType(this IEnumerable<ILayer> layers)
        => layers.OrderBy(l => l.Class switch
        {
            IFeatureClass fClass when (fClass.GeometryType == GeometryType.Point) => 0,
            IFeatureClass fClass when (fClass.GeometryType == GeometryType.Multipoint) => 1,
            IFeatureClass fClass when (fClass.GeometryType == GeometryType.Polyline) => 2,
            IFeatureClass fClass when (fClass.GeometryType == GeometryType.Polygon) => 3,
            IFeatureClass fClass when (fClass.GeometryType == GeometryType.Envelope) => 4,
            IFeatureClass fClass when (fClass.GeometryType == GeometryType.Unknown) => 5,
            IRasterCatalogClass => 6,
            IRasterClass => 7,
            _ => 99
        });
}
