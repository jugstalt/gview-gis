using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using System.Linq;

namespace gView.Blazor.Core.Extensions;

static public class LayerExtensions
{
    static public ISpatialReference? GetSpatialReference(this ILayer layer)
        => layer switch
        {
            IFeatureLayer fLayer => fLayer.FeatureClass?.SpatialReference,
            IRasterLayer rLayer => rLayer.RasterClass?.SpatialReference,
            IWebServiceLayer wLayer => wLayer.WebServiceClass?.SpatialReference,
            _ => null
        };

    static public bool ImplementsSpatialReference(this ILayer layer)
        => layer switch
        {
            IWebServiceTheme => false,
            IFeatureLayer => true,
            IRasterLayer => true,
            IWebServiceLayer => true,
            _ => false
        };

    static public bool IsGroupLayer(this ILayer layer) => layer is IGroupLayer;

    static public bool IsFeatureLyer(this ILayer layer) => layer is IFeatureLayer;

    static public bool HasDataSource(this ILayer layer) => layer.DatasetID >= 0;

    static public bool CanFeatureRender(this ILayer layer) => layer is IFeatureLayer;
    static public bool CanLabelRenderer(this ILayer layer) => layer is IFeatureLayer;

    static public bool ImplementsLayerComposition(this ILayer layer)
        => layer is IFeatureLayerComposition 
           && !(layer is IWebServiceTheme);

    static public bool ImplementsLayerDefinitionFilter(this ILayer layer)
       => layer is IFeatureLayerComposition
           && !(layer is IWebServiceTheme);

    static public bool IsRasterLayer(this ILayer layer) => layer is IRasterLayer;

    static public bool IsWebServiceLayer(this ILayer layer) => layer is IWebServiceLayer;

    static public bool IsLocked(this ILayer layer)
        => layer switch
        {
            IWebServiceTheme theme => theme.Locked,
            _ => false
        };

    static public bool HasLineGeometry(this IFeatureLayer? fLayer)
        => fLayer is not null &&
            (
                fLayer.LayerGeometryType == GeometryType.Polyline
            );
    static public bool CanRotateSymbols(this IFeatureLayer? fLayer)
        => fLayer is not null &&
            (
                fLayer.LayerGeometryType == GeometryType.Point
                || fLayer.LayerGeometryType == GeometryType.Multipoint
            );

    static public bool CanApplySecondaryLabelAlignment(this IFeatureLayer fLayer)
        => fLayer is not null &&
            (
                fLayer.LayerGeometryType == GeometryType.Point
                || fLayer.LayerGeometryType == GeometryType.Multipoint
            );

    static public gView.Framework.Data.Layer? Clone(this gView.Framework.Data.Layer? layer)
    {
        var clone = layer?.PersistedClone();

        if (layer is IFeatureLayer featureLayer 
            && clone is IFeatureLayer clonedFeatureLayer)
        {
            #region reset all the field types

            foreach (var field in featureLayer.Fields?.ToEnumerable() ?? [])
            {
                var clonedField = clonedFeatureLayer.Fields.ToEnumerable()
                                                    .Where(f => f.name == field.name)
                                                    .FirstOrDefault();

                if(clonedField is gView.Framework.Data.Field f)
                {
                    f.type = field.type;
                }
            }

            #endregion
        }

        return clone;
    }
}
