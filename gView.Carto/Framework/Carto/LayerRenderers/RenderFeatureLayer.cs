using gView.Data.Framework.Data;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.Framework.system;
using System;
using System.Threading.Tasks;

namespace gView.Framework.Carto.LayerRenderers
{
    public sealed class RenderFeatureLayer
    {
        private Map _map;
        private IDatasetCachingContext _datasetCachingContext;
        private IFeatureLayer _layer;
        private ICancelTracker _cancelTracker;
        private static object lockThis = new object();
        private bool _useLabelRenderer = false;
        private FeatureCounter _counter;
        private bool _isServiceMap = false;

        public RenderFeatureLayer(Map map, IDatasetCachingContext datasetCachingContext , IFeatureLayer layer, ICancelTracker cancelTracker, FeatureCounter counter)
        {
            _map = map;
            _datasetCachingContext = datasetCachingContext;
            _isServiceMap = map is IServiceMap;
            _layer = layer;
            _cancelTracker = ((cancelTracker == null) ? new CancelTracker() : cancelTracker);
            _counter = counter;
            _counter.Counter = 0;
        }

        public bool UseLabelRenderer
        {
            get { return _useLabelRenderer; }
            set { _useLabelRenderer = value; }
        }

        async public Task Render()
        {
            if (_layer == null || _map == null)
            {
                return;
            }

            #region JSON FilterCollection

            if (_layer.FilterQuery != null && !String.IsNullOrEmpty(_layer.FilterQuery.JsonWhereClause))
            {
                try
                {
                    DisplayFilterCollection dfc = DisplayFilterCollection.FromJSON(_layer.FilterQuery.JsonWhereClause);
                    if (dfc == null)
                    {
                        return;
                    }

                    IFeatureLayer flayer = (IFeatureLayer)LayerFactory.Create(_layer.Class, _layer);
                    flayer.FilterQuery = (IQueryFilter)_layer.FilterQuery.Clone();
                    foreach (DisplayFilter df in dfc)
                    {
                        // immer neu klonen (wegen PenWidth...)!!
                        flayer.FeatureRenderer = _layer.FeatureRenderer != null ? (IFeatureRenderer)_layer.FeatureRenderer.Clone() : null;

                        flayer.FilterQuery.WhereClause = df.Filter;
                        if (flayer.FeatureRenderer != null)
                        {
                            foreach (ISymbol symbol in flayer.FeatureRenderer.Symbols)
                            {
                                if (df.Color.A > 0)
                                {
                                    if (symbol is IPenColor)
                                    {
                                        ((IPenColor)symbol).PenColor = df.Color;
                                    }

                                    if (symbol is IBrushColor)
                                    {
                                        ((IBrushColor)symbol).FillColor = df.Color;
                                    }

                                    if (symbol is IFontColor)
                                    {
                                        ((IFontColor)symbol).FontColor = df.Color;
                                    }
                                }
                                if (df.PenWidth > 0f && symbol is IPenWidth)
                                {
                                    ((IPenWidth)symbol).PenWidth = df.PenWidth;
                                }
                            }
                        }
                        await Render(flayer);
                    }
                }
                catch { }
                return;
            }

            #endregion

            await Render(_layer);
        }
        async private Task Render(IFeatureLayer layer)
        {
            IFeatureRenderer clonedRenderer = null;

            System.Drawing.Bitmap compositionModeCopyBitmap = null;
            System.Drawing.Graphics compositionModeCopyGraphicsContext = null, originalGraphicsContext = null;


            try
            {
                _map.FireOnUserInterface(true);
                if ((
                    layer.FeatureRenderer == null ||
                    layer.FeatureRenderer.HasEffect(layer, _map) == false)
                    &&
                    (
                    layer.LabelRenderer == null ||
                    _useLabelRenderer == false
                    ))
                {
                    return;
                }

                IFeatureClass fClass = layer.FeatureClass;
                if (fClass == null)
                {
                    return;
                }

                //IDataset dataset = (IDataset)_map[layer];
                //if (dataset == null) return;

                //if (!(dataset is IFeatureDataset)) return;

                IGeometry filterGeom = _map.Display.DisplayTransformation.TransformedBounds(_map.Display); //_map.Display.Envelope;

                if (_map.Display.GeometricTransformer != null)
                {
                    filterGeom = MapHelper.Project(fClass, _map.Display);
                }

                gView.Framework.Data.SpatialFilter filter = new gView.Framework.Data.SpatialFilter();
                filter.DatasetCachingContext = _datasetCachingContext;
                filter.Geometry = filterGeom;
                filter.AddField(fClass.ShapeFieldName);
                //filter.FuzzyQuery = true;
                filter.SpatialRelation = spatialRelation.SpatialRelationMapEnvelopeIntersects;
                filter.MapScale = _map.Display.mapScale;
                filter.CancelTracker = _cancelTracker;

                if (layer.FilterQuery != null)
                {
                    filter.WhereClause = layer.FilterQuery.WhereClause;
                    if (layer.FilterQuery is IBufferQueryFilter)
                    {
                        ISpatialFilter sFilter = await BufferQueryFilter.ConvertToSpatialFilter(layer.FilterQuery as IBufferQueryFilter);
                        if (sFilter == null)
                        {
                            return;
                        }
                        filter.SpatialRelation = spatialRelation.SpatialRelationIntersects;
                        filter.Geometry = sFilter.Geometry;
                    }
                    if (layer.FilterQuery is ISpatialFilter)
                    {
                        //filter.FuzzyQuery = ((ISpatialFilter)layer.FilterQuery).FuzzyQuery;
                        filter.SpatialRelation = ((ISpatialFilter)layer.FilterQuery).SpatialRelation;
                        filter.Geometry = ((ISpatialFilter)layer.FilterQuery).Geometry;
                    }
                }

                // Erst nach dem Clonen anwenden!!!
                //if (layer.FeatureRenderer != null && layer.FeatureRenderer.HasEffect(layer, _map))
                //{
                //    layer.FeatureRenderer.PrepareQueryFilter(layer, filter);
                //}
                //if (layer.LabelRenderer != null && _useLabelRenderer)
                //{
                //    layer.LabelRenderer.PrepareQueryFilter(_map.Display, layer, filter);
                //}

                IDisplay display = _map;
                double refScale = display.refScale;

                #region Layer Clonen

                IFeatureRenderer renderer = null;
                ILabelRenderer labelRenderer = null;

                lock (lockThis)
                {
                    // Beim Clonen sprerren...
                    // Da sonst bei der Servicemap bei gleichzeitigen Requests
                    // Exception "Objekt wird bereits an anderer Stelle verwendet" auftreten kann!
                    if (layer.FeatureRenderer != null && layer.FeatureRenderer.HasEffect(layer, _map))
                    {
                        if (layer.RequiresFeatureRendererClone(display))
                        {
                            renderer = clonedRenderer = (IFeatureRenderer)layer.FeatureRenderer.Clone(
                                new CloneOptions(display, 
                                                 layer.UseWithRefScale(display),
                                                 maxRefScaleFactor: layer.MaxRefScaleFactor));
                        }
                        else
                        {
                            renderer = layer.FeatureRenderer;
                        }
                    }
                    if (layer.LabelRenderer != null && _useLabelRenderer)
                    {
                        if(layer.RequiresLabelRendererClone(display))
                        {
                            labelRenderer = (ILabelRenderer)layer.LabelRenderer.Clone(new CloneOptions(display,
                                                                                                       layer.UseLabelsWithRefScale(display),
                                                                                                       maxLabelRefscaleFactor: layer.MaxLabelRefScaleFactor));
                        } 
                        else  // Clone with null => simple clone
                        {
                            //display.refScale = 0;
                            labelRenderer = (ILabelRenderer)layer.LabelRenderer.Clone(null);
                            //display.refScale = refScale;
                        }
                    }
                }

                #endregion

                #region Prepare filter

                // Prepare erst auf geclonte renderer anwenden!! (Threadsafe)
                if (renderer != null && renderer.HasEffect(layer, _map))
                {
                    renderer.PrepareQueryFilter(layer, filter);
                }
                if (labelRenderer != null && _useLabelRenderer)
                {
                    labelRenderer.PrepareQueryFilter(_map.Display, layer, filter);
                }

                #endregion

                using (IFeatureCursor fCursor = await fClass.GetFeatures(MapHelper.MapQueryFilter(filter)))
                {
                    _map.FireOnUserInterface(false);

                    if (fCursor != null)
                    {
                        IFeature feature;

                        if (renderer != null)
                        {
                            renderer.StartDrawing(_map);

                            bool useCompostionModeCopy = layer is IFeatureLayerComposition &&
                                                         ((IFeatureLayerComposition)layer).CompositionMode == FeatureLayerCompositionMode.Copy;

                            if (useCompostionModeCopy)
                            {
                                originalGraphicsContext = _map.Display.GraphicsContext;
                                compositionModeCopyBitmap = new System.Drawing.Bitmap(_map.Display.Bitmap.Width, _map.Display.Bitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                                compositionModeCopyGraphicsContext = System.Drawing.Graphics.FromImage(compositionModeCopyBitmap);

                                compositionModeCopyBitmap.MakeTransparent();
                                compositionModeCopyBitmap.SetResolution(_map.Display.Bitmap.HorizontalResolution,
                                                                        _map.Display.Bitmap.VerticalResolution);

                                ((Display)_map.Display).GraphicsContext = compositionModeCopyGraphicsContext;
                            }

                            while ((feature = await fCursor.NextFeature()) != null)
                            {
                                if (_cancelTracker != null)
                                {
                                    if (!_cancelTracker.Continue)
                                    {
                                        break;
                                    }
                                }

                                renderer.Draw(_map, feature);

                                if (labelRenderer != null)
                                {
                                    labelRenderer.Draw(_map, feature);
                                }

                                _counter.Counter++;

                                if (_isServiceMap == false)
                                {
                                    if (_counter.Counter % 10000 == 0)
                                    {
                                        _map.FireRefreshMapView();
                                    }
                                }
                            }
                        }
                        else if (labelRenderer != null && _cancelTracker.Continue)
                        {
                            while ((feature = await fCursor.NextFeature()) != null)
                            {
                                if (_cancelTracker != null)
                                {
                                    if (!_cancelTracker.Continue)
                                    {
                                        break;
                                    }
                                }

                                labelRenderer.Draw(_map, feature);
                                _counter.Counter++;
                            }
                        }

                        if (labelRenderer != null)
                        {
                            labelRenderer.Release();
                        }

                        if (renderer != null)
                        {
                            renderer.FinishDrawing(_map, _cancelTracker);
                        }

                        if (compositionModeCopyGraphicsContext != null && compositionModeCopyBitmap != null)
                        {
                            var matrix = new System.Drawing.Imaging.ColorMatrix();
                            //set the opacity  
                            matrix.Matrix33 =  (float)Math.Min(1, (100f - ((IFeatureLayerComposition)layer).CompositionModeCopyTransparency) / 100);
                            //create image attributes  
                            var attributes = new System.Drawing.Imaging.ImageAttributes();

                            //set the color(opacity) of the image  
                            attributes.SetColorMatrix(matrix, System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Bitmap);

                            originalGraphicsContext.DrawImage(compositionModeCopyBitmap,
                                new System.Drawing.Rectangle(0, 0, compositionModeCopyBitmap.Width, compositionModeCopyBitmap.Height),
                                0, 0, compositionModeCopyBitmap.Width, compositionModeCopyBitmap.Height,
                                System.Drawing.GraphicsUnit.Pixel,
                                attributes);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_map is IServiceMap && ((IServiceMap)_map).MapServer != null)
                {
                    await ((IServiceMap)_map).MapServer.LogAsync(
                        ((IServiceMap)_map).Name,
                        "RenderFeatureLayerThread: " + ((layer != null) ? layer.Title : String.Empty),
                        loggingMethod.error,
                        ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
                }
                if (_map != null)
                {
                    _map.AddException(new Exception("RenderFeatureLayerThread: " + ((layer != null) ? layer.Title : String.Empty) + "\n" + ex.Message, ex));
                }
            }
            finally
            {
                if (clonedRenderer != null)
                {
                    clonedRenderer.Release();
                }

                if (originalGraphicsContext != null)
                {
                    ((Display)_map.Display).GraphicsContext = originalGraphicsContext;
                }

                if (compositionModeCopyGraphicsContext != null)
                {
                    compositionModeCopyGraphicsContext.Dispose();
                    compositionModeCopyGraphicsContext = null;
                }

                if (compositionModeCopyBitmap != null)
                {
                    compositionModeCopyBitmap.Dispose();
                    compositionModeCopyBitmap = null;
                }

                _map.FireOnUserInterface(false);
            }
        }
    }
}
