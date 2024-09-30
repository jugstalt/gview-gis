using gView.Carto.Core;
using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry.Extensions;
using gView.Framework.OGC.WMS.Version_1_1_1;
using OSGeo_v3.OGR;
using static gView.Interoperability.GeoServices.Rest.DTOs.JsonMapServiceDTO;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("2511914E-2A19-459B-8CFD-27039BBD1F2F")]
internal class ZoomToLayer : ICartoButton
{
    public string Name => "Zoom To";

    public string ToolTip => "";

    public string Icon => "webgis:extent";

    public CartoToolTarget Target => CartoToolTarget.SelectedTocItem;

    public int SortOrder => 90;

    public bool IsEnabled(ICartoApplicationScopeService scope)
    {
        foreach (var layer in scope.SelectedTocTreeNode?.TocElement?.Layers ?? [])
        {
            if ((layer is IFeatureLayer featureLayer && featureLayer.FeatureClass is not null) ||
                (layer is IRasterLayer rasterLayer && rasterLayer.RasterClass is not null) ||
                (layer is IWebServiceLayer webLayer && webLayer.WebServiceClass is not null) ||
                (layer is IGroupLayer))
            {
                return true;
            }
        }

        return false;
    }

    async public Task<bool> OnClick(ICartoApplicationScopeService scope)
    {
        IEnvelope? extent = null;

        foreach (var layer in scope.SelectedTocTreeNode?.TocElement?.Layers ?? [])
        {
            extent = GetLayerExtent(scope, layer, extent);
        }


        if(extent is null) return false;

        scope.Document.Map.Display.ZoomTo(extent);

        await scope.EventBus.FireMapZoomToAsync(extent);

        return true;
    }

    private IEnvelope? GetLayerExtent(
                ICartoApplicationScopeService scope, 
                ILayer layer, 
                IEnvelope? unionWith = null
        )
    {
        var mapSref = scope.Document?.Map.Display.SpatialReference;

        IEnvelope? layerExtent = layer switch
        {
            IFeatureLayer featureLayer => scope.GeoTransformer.Transform(
                                                    featureLayer.FeatureClass?.Envelope, 
                                                    featureLayer.FeatureClass?.SpatialReference, 
                                                    mapSref
                                                )?.Envelope,
            IRasterLayer rasterLayer => scope.GeoTransformer.Transform(
                                                    rasterLayer.RasterClass?.Polygon?.Envelope, 
                                                    rasterLayer.RasterClass?.SpatialReference, 
                                                    mapSref
                                                )?.Envelope,
            IWebServiceLayer webLayer => scope.GeoTransformer.Transform(
                                                    webLayer.WebServiceClass?.Envelope, 
                                                    webLayer.WebServiceClass?.SpatialReference, 
                                                    mapSref
                                                )?.Envelope,
            IGroupLayer groupLayer => groupLayer.ChildLayers
                                                .Select(l => GetLayerExtent(scope, l))
                                                .ToUnion(),
            _ => null
        };

        if (unionWith is not null && layerExtent is not null)
        {
            layerExtent.Union(unionWith);
        }

        return layerExtent;
    }
}
