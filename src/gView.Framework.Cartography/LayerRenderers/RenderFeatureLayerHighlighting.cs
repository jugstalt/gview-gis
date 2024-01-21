using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Data;
using gView.Framework.Symbology;
using gView.GraphicsEngine.Extensions;
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
                IMapRenderer mapRenderer = null,
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

            IDataset dataset = _map[_layer];
            if (dataset == null)
            {
                return;
            }

            if (!(dataset is IFeatureDataset))
            {
                return;
            }

            IGeometry filterGeom = _map.Display.Envelope;

            if (_map.Display.GeometricTransformer != null)
            {
                filterGeom = MapHelper.Project(fClass, _map.Display);
                //filterGeom = (IGeometry)_map.Display.GeometricTransformer.InvTransform2D(filterGeom);
            }

            int counter = 0;
            while (true)
            {
                bool hasMore = false;

                var filter = ((IFeatureHighlighting)_layer).FeatureHighlightFilter;

                if (filter == null)
                {
                    return;
                }

                filter.SubFields = fClass.ShapeFieldName;
                //filter.AddField(fClass.ShapeFieldName);

                //todo: append layer defintion query
                //todo: intersect with map envelope

                ISymbol symbol = null;
                var pluginManager = new PlugInManager();
                IFeatureRenderer renderer = pluginManager.CreateInstance(KnownObjects.Carto_SimpleRenderer) as IFeatureRenderer;
                if (renderer is ISymbolCreator)
                {
                    symbol = ((ISymbolCreator)renderer).CreateStandardHighlightSymbol(_layer.LayerGeometryType);
                }
                if (symbol == null)
                {
                    return;
                }

                using (IFeatureCursor fCursor = fClass is ISelectionCache ? ((ISelectionCache)fClass).GetSelectedFeatures(_map.Display) : await fClass.GetFeatures(filter))
                {
                    if (fCursor != null)
                    {
                        //_layer.SelectionRenderer.Draw(_map, fCursor, DrawPhase.Geography, _cancelTracker);
                        IFeature feature;
                        while ((feature = await fCursor.NextFeature()) != null)
                        {
                            if (_cancelTracker != null)
                            {
                                if (!_cancelTracker.Continue)
                                {
                                    break;
                                }
                            }

                            symbol.Draw(_map, feature.Shape);

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

                if (!hasMore)
                {
                    break;
                }
            }
        }
    }
}
