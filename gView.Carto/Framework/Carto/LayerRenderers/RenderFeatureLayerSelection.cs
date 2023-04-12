using gView.Framework.Data;
using gView.Framework.Data.Cursors;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.system;
using gView.GraphicsEngine.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Carto.LayerRenderers
{
    public sealed class RenderFeatureLayerSelection
    {
        private Map _map;
        private IFeatureLayer _layer;
        private ICancelTracker _cancelTracker;

        public RenderFeatureLayerSelection(Map map, IFeatureLayer layer, ICancelTracker cancelTracker)
        {
            _map = map;
            _layer = layer;
            _cancelTracker = ((cancelTracker == null) ? new CancelTracker() : cancelTracker);
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

            IQueryFilter filter = null;
            //List<int> IDs=new List<int>();  // Sollte nicht null sein...
            if (selectionSet is ISpatialIndexedIDSelectionSet)
            {
                List<int> IDs = ((ISpatialIndexedIDSelectionSet)selectionSet).IDsInEnvelope(filterGeom.Envelope);
                filter = new RowIDFilter(fClass.IDFieldName, IDs);
            }
            else if (selectionSet is IIDSelectionSet)
            {
                List<int> IDs = ((IIDSelectionSet)selectionSet).IDs;
                filter = new RowIDFilter(fClass.IDFieldName, IDs);
            }
            else if (selectionSet is ISpatialIndexedGlobalIDSelectionSet)
            {
                List<long> IDs = ((ISpatialIndexedGlobalIDSelectionSet)selectionSet).IDsInEnvelope(filterGeom.Envelope);
                filter = new GlobalRowIDFilter(fClass.IDFieldName, IDs);
            }
            else if (selectionSet is IGlobalIDSelectionSet)
            {
                List<long> IDs = ((IGlobalIDSelectionSet)selectionSet).IDs;
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

            using (IFeatureCursor fCursor = (fClass is ISelectionCache) ? ((ISelectionCache)fClass).GetSelectedFeatures(_map.Display) : await fClass.GetFeatures(filter))
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
                    }
                    fCursor.Dispose();
                }
            }
        }

    }
}
