#nullable enable

using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.Geometry.GeoProcessing.Clipper;
using System.Threading.Tasks;

namespace gView.Framework.Cartography.LayerRenderers
{
    public sealed class RenderFeatureLayerHighlighting
    {
        private Map _map;
        private IFeatureLayer _layer;
        private ICancelTracker _cancelTracker;
        private IMapRenderer? _mapRenderer;
        private bool _isServiceMap = false;
        private int _maxBulkSize = 10_000;
        public RenderFeatureLayerHighlighting(
                Map map,
                IFeatureLayer layer,
                ICancelTracker cancelTracker,
                IMapRenderer? mapRenderer = null,
                int maxBulkSize = -1
            )
        {
            _map = map;
            _layer = layer;
            _cancelTracker = cancelTracker == null ? new CancelTracker() : cancelTracker;
            _mapRenderer = mapRenderer;
            _isServiceMap = map is IServiceMap;
            _maxBulkSize = maxBulkSize > 0 ? maxBulkSize : _maxBulkSize;
        }

        async public Task Render()
        {
            if (_layer == null)
            {
                return;
            }

            if (!(_layer is IFeatureHighlighting))
            {
                return;
            }

            IFeatureClass fClass = _layer.FeatureClass;
            if (fClass == null)
            {
                return;
            }

            var filter = ((IFeatureHighlighting)_layer).FeatureHighlightFilter?.Clone() as IQueryFilter;

            if (filter == null)
            {
                return;
            }

            // clip with map envelope
            if (filter is ISpatialFilter spatialFilter 
                && spatialFilter.FilterSpatialReference is not null
                && _map.SpatialReference is not null)
            {
                var mapBounds = new Envelope(
                    GeometricTransformerFactory.Transform2D(
                        new Envelope(_map.Envelope),
                        _map.SpatialReference,
                        spatialFilter.FilterSpatialReference).Envelope);

                spatialFilter.Geometry = spatialFilter.Geometry switch
                {
                    IEnvelope env => env.ToPolygon(0).Clip(mapBounds),
                    IPolygon poly => poly.Clip(mapBounds),
                    _ => spatialFilter.Geometry
                };

                if(spatialFilter.Geometry == null)
                {
                    return;
                }
            }

            filter.SubFields = fClass.ShapeFieldName;

            // append layer defintion query
            if (_layer is IFeatureLayer featureLayer)
            {
                filter.WhereClause = string.IsNullOrEmpty(filter.WhereClause)
                    ? featureLayer.FilterQuery?.WhereClause
                    : string.IsNullOrEmpty(featureLayer.FilterQuery?.WhereClause)
                        ? filter.WhereClause
                        : $"({filter.WhereClause}) and ({featureLayer.FilterQuery?.WhereClause})";
            }

            ISymbol? symbol = null;
            var pluginManager = new PlugInManager();
            IFeatureRenderer? renderer = pluginManager.CreateInstance(KnownObjects.Carto_SimpleRenderer) as IFeatureRenderer;
            if (renderer is ISymbolCreator)
            {
                symbol = ((ISymbolCreator)renderer).CreateStandardHighlightSymbol(_layer.LayerGeometryType);
            }
            if (symbol == null)
            {
                return;
            }

            int counter = 0;
            using (IFeatureCursor fCursor = fClass is ISelectionCache
                                                ? ((ISelectionCache)fClass).GetSelectedFeatures(_map.Display)
                                                : await fClass.GetFeatures(filter))
            {
                if (fCursor != null)
                {
                    IFeature feature;

                    while ((feature = await fCursor.NextFeature()) != null)
                    {
                        if (_cancelTracker?.Continue == false)
                        {
                            break;
                        }

                        _map.Draw(symbol, feature.Shape);

                        counter++;

                        if (_isServiceMap == false)
                        {
                            if (counter % 100 == 0)
                            {
                                _mapRenderer?.FireRefreshMapView(DrawPhase.Highlighing);
                            }
                        }
                    }
                    fCursor.Dispose();
                }
            }
        }
    }
}
