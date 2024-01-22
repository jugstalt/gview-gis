using gView.Framework.Cartography;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Common;
using gView.Framework.Data.Filters;
using gView.Framework.Common;
using gView.GraphicsEngine.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace gView.Framework.Cartography.LayerRenderers
{
    public sealed class RenderFeatureLayerSelection
    {
        private Map _map;
        private IFeatureLayer _layer;
        private ICancelTracker _cancelTracker;
        private IMapRenderer? _mapRenderer;
        private bool _isServiceMap = false;
        private int _maxBulkSize = 10_000;
        public RenderFeatureLayerSelection(
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
            _maxBulkSize = maxBulkSize > 0 ? maxBulkSize: _maxBulkSize;
        }

        async public Task Render()
        {
            if (_layer == null)
            {
                return;
            }

            if (_layer.SelectionRenderer == null)
            {
                return;
            }

            if (!(_layer is IFeatureSelection))
            {
                return;
            }

            IFeatureClass fClass = _layer.FeatureClass;
            if (fClass == null)
            {
                return;
            }

            ISelectionSet selectionSet = ((IFeatureSelection)_layer).SelectionSet;
            if (selectionSet == null)
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

            int skip = 0, counter = 0;
            while (true)
            {
                bool hasMore = false;

                IQueryFilter filter = null;
                //List<int> IDs=new List<int>();  // Sollte nicht null sein...
                if (selectionSet is ISpatialIndexedIDSelectionSet)
                {
                    List<int> IDs = ((ISpatialIndexedIDSelectionSet)selectionSet).IDsInEnvelope(filterGeom.Envelope)
                         .Skip(skip).Take(_maxBulkSize).ToList();
                    
                    if(IDs.Count == 0) 
                    { 
                        break; 
                    }
                    else
                    {
                        skip += _maxBulkSize;
                        hasMore = true;
                    }

                    filter = new RowIDFilter(fClass.IDFieldName, IDs);
                }
                else if (selectionSet is IIDSelectionSet)
                {
                    List<int> IDs = ((IIDSelectionSet)selectionSet).IDs
                        .Skip(skip).Take(_maxBulkSize).ToList();

                    if (IDs.Count == 0)
                    {
                        break;
                    }
                    else
                    {
                        skip += _maxBulkSize;
                        hasMore = true;
                    }

                    filter = new RowIDFilter(fClass.IDFieldName, IDs);
                }
                else if (selectionSet is ISpatialIndexedGlobalIDSelectionSet)
                {
                    List<long> IDs = ((ISpatialIndexedGlobalIDSelectionSet)selectionSet).IDsInEnvelope(filterGeom.Envelope)
                        .Skip(skip).Take(_maxBulkSize).ToList();

                    if (IDs.Count == 0)
                    {
                        break;
                    }
                    else
                    {
                        skip += _maxBulkSize;
                        hasMore = true;
                    }

                    filter = new GlobalRowIDFilter(fClass.IDFieldName, IDs);
                }
                else if (selectionSet is IGlobalIDSelectionSet)
                {
                    List<long> IDs = ((IGlobalIDSelectionSet)selectionSet).IDs
                        .Skip(skip).Take(_maxBulkSize).ToList();

                    if (IDs.Count == 0)
                    {
                        break;
                    }
                    else
                    {
                        skip += _maxBulkSize;
                        hasMore = true;
                    }

                    filter = new GlobalRowIDFilter(fClass.IDFieldName, IDs);
                }
                else if (selectionSet is IQueryFilteredSelectionSet)
                {
                    filter = ((IQueryFilteredSelectionSet)selectionSet).QueryFilter.Clone() as IQueryFilter;
                }

                if (filter == null)
                {
                    return;
                }

                filter.AddField(fClass.ShapeFieldName);

                #region Clone Layer

                IFeatureRenderer selectionRenderer = null;

                GraphicsEngine.Current.Engine?.CloneObjectsLocker.InterLock(() =>
                {
                    // Beim Clonen sprerren...
                    // Da sonst bei der Servicemap bei gleichzeitigen Requests
                    // Exception "Objekt wird bereits an anderer Stelle verwendet" auftreten kann!
                    selectionRenderer = (IFeatureRenderer)_layer.SelectionRenderer.Clone(new CloneOptions(_map,
                                                                                                          false,
                                                                                                          maxLabelRefscaleFactor: _layer.MaxRefScaleFactor));
                });

                #endregion

                selectionRenderer.PrepareQueryFilter(_layer, filter);

                using (IFeatureCursor fCursor = fClass is ISelectionCache 
                                                    ? ((ISelectionCache)fClass).GetSelectedFeatures(_map.Display) 
                                                    : await fClass.GetFeatures(filter))
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

                            selectionRenderer.Draw(_map, feature);

                            counter++;

                            if (_isServiceMap == false)
                            {
                                if (counter % 100 == 0)
                                {
                                    _mapRenderer?.FireRefreshMapView(DrawPhase.Selection);
                                }
                            }
                        }
                        fCursor.Dispose();
                    }
                }
            
                if(!hasMore)
                {
                    break;
                }
            }
        }

    }
}
