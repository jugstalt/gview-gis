using gView.Framework.Cartography.LayerRenderers;
using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Data;
using gView.Framework.Data.Abstraction;
using gView.Framework.Data.Extensions;
using gView.Framework.Geometry;
using gView.Framework.Geometry.GeoProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace gView.Framework.Cartography;

public class MapRendererInstance : Map, IMapRenderer
{
    private Map _original;
    private MapRendererInstance(Map original)
    {
        _original = original;
    }

    async static public Task<MapRendererInstance> CreateAsync(Map original)
    {
        var mapRenderInstance = new MapRendererInstance(original);

        mapRenderInstance._layers = original._layers;
        mapRenderInstance._datasets = original._datasets;

        mapRenderInstance.m_imageMerger = new ImageMerger2();

        mapRenderInstance.m_name = original.Name;
        mapRenderInstance._toc = original._toc;
        mapRenderInstance.Title = original.Title;

        //serviceMap._ceckLayerVisibilityBeforeDrawing = true;
        mapRenderInstance._mapUnits = original.MapUnits;
        mapRenderInstance._displayUnits = original.DisplayUnits;
        mapRenderInstance.ReferenceScale = original.ReferenceScale;

        mapRenderInstance.SpatialReference = original.Display.SpatialReference;
        mapRenderInstance.LayerDefaultSpatialReference = original.LayerDefaultSpatialReference != null ? original.LayerDefaultSpatialReference.Clone() as ISpatialReference : null;

        mapRenderInstance._drawScaleBar = false;

        // Metadata
        await mapRenderInstance.SetMetadataProviders(await original.GetMetadataProviders());
        mapRenderInstance._debug = false;

        mapRenderInstance._layerDescriptions = original.LayerDescriptions;
        mapRenderInstance._layerCopyrightTexts = original.LayerCopyrightTexts;

        mapRenderInstance.SetResourceContainer(original.ResourceContainer);

        mapRenderInstance.Display.ImageWidth = original.Display.ImageWidth;
        mapRenderInstance.Display.ImageHeight = original.Display.ImageHeight;
        mapRenderInstance.Display.ZoomTo(original.Envelope);
        mapRenderInstance.Display.Dpi = original.Display.Dpi;
        mapRenderInstance.Display.TransparentColor = original.Display.TransparentColor;

        foreach (var element in original.Display.GraphicsContainer.Elements)
        {
            mapRenderInstance.Display.GraphicsContainer.Elements.Add(element);
        }

        //mapRenderInstance.DrawingLayer += (layerName) =>
        //{
        //    original.FireDrawingLayer(layerName);
        //};

        return mapRenderInstance;
    }

    #region IMapRenderer

    #region Events

    public event StartRefreshMapEvent StartRefreshMap;
    public event NewBitmapEvent NewBitmap;
    public event DoRefreshMapViewEvent DoRefreshMapView;
    public event StartDrawingLayerEvent StartDrawingLayer;
    public event FinishedDrawingLayerEvent FinishedDrawingLayer;

    #endregion

    public DrawPhase DrawPhase { get; private set; } = DrawPhase.Empty;

    async public Task<bool> RefreshMap(DrawPhase drawPhase, ICancelTracker cancelTracker)
    {
        if(this.DrawPhase != DrawPhase.Empty)
        {
            throw new Exception("RefreshMap can't only called once for any instance of MapRendererInstance2. Create a new Instance to render another Map Impage.");
        }

        this.DrawPhase = drawPhase;

        ResetRequestExceptions();

        try
        {
            StartRefreshMap?.Invoke(this);

            using (var datasetCachingContext = new DatasetCachingContext(this))
            {
                _lastException = null;

                #region Start Drawing/Initialisierung

                ZoomTo(m_actMinX, m_actMinY, m_actMaxX, m_actMaxY);

                if (cancelTracker == null)
                {
                    cancelTracker = new CancelTracker();
                }

                IGeometricTransformer geoTransformer = GeometricTransformerFactory.Create();

                _bitmap = GraphicsEngine.Current.Engine.CreateBitmap(ImageWidth, ImageHeight, GraphicsEngine.PixelFormat.Rgba32);
                _bitmap.MakeTransparent();
                _canvas = _bitmap.CreateCanvas();

                // NewBitmap immer aufrufen, da sonst neuer DataView nix mitbekommt
                if (cancelTracker.Continue)
                {
                    NewBitmap?.Invoke(this, _bitmap);
                }

                #endregion

                #region Geometry

                if (Bit.Has(drawPhase, DrawPhase.Geography))
                {
                    LabelEngine.Init(Display, false);

                    GeometricTransformer = geoTransformer;

                    // Thread für MapServer Datasets starten...
                    #region WebServiceLayer

                    List<IWebServiceLayer> webServices;
                    if (TOC != null)
                    {
                        webServices = ListOperations<IWebServiceLayer>.Swap(TOC.VisibleWebServiceLayers);
                    }
                    else
                    {
                        webServices = new List<IWebServiceLayer>();
                        foreach (IDatasetElement layer in MapElements)
                        {
                            if (!(layer is IWebServiceLayer))
                            {
                                continue;
                            }

                            if (((ILayer)layer).Visible)
                            {
                                webServices.Add((IWebServiceLayer)layer);
                            }
                        }
                    }
                    int webServiceOrder = 0;
                    foreach (IWebServiceLayer element in webServices)
                    {
                        if (!element.Visible)
                        {
                            continue;
                        }

                        RenderServiceRequest srt = new RenderServiceRequest(this, element, webServiceOrder++);
                        srt.finish += new RenderServiceRequest.RequestThreadFinished(RenderWebServiceRequestFinished);
                        //Thread thread = new Thread(new ThreadStart(srt.ImageRequest));
                        m_imageMerger.max++;
                        //thread.Start();
                        var task = Task.Run(async () => await srt.ImageRequest());  // start the task..
                        //var task = srt.ImageRequest();  // start the task...
                        //await srt.ImageRequest();
                    }

                    #endregion

                    #region Layerlisten erstellen

                    List<ILayer> layers;
                    if (TOC is not null)
                    {
                        layers = ListOperations<ILayer>.Swap(TOC.VisibleLayers);
                    }
                    else
                    {
                        layers = new List<ILayer>();
                        foreach (IDatasetElement layer in MapElements)
                        {
                            if (!(layer is ILayer))
                            {
                                continue;
                            }

                            if (((ILayer)layer).Visible)
                            {
                                layers.Add((ILayer)layer);
                            }
                        }
                    }

                    List<IFeatureLayer> labelLayers = OrderedLabelLayers(layers);

                    #endregion

                    #region Renderer Features

                    foreach (ILayer layer in layers)
                    {
                        if (!cancelTracker.Continue)
                        {
                            break;
                        }

                        if (!layer.RenderInScale(this))
                        {
                            continue;
                        }

                        SetGeotransformer(layer, geoTransformer);

                        DateTime startTime = DateTime.Now;

                        FeatureCounter fCounter = new FeatureCounter();
                        if (layer is IFeatureLayer)
                        {
                            if (layer.Class?.Dataset is IFeatureCacheDataset featureCache)
                            {
                                await featureCache.InitFeatureCache(datasetCachingContext);
                            }

                            IFeatureLayer fLayer = (IFeatureLayer)layer;
                            if (fLayer.FeatureRenderer == null &&
                                (
                                 fLayer.LabelRenderer == null ||
                                fLayer.LabelRenderer != null && fLayer.LabelRenderer.RenderMode != LabelRenderMode.RenderWithFeature
                                ))
                            {
                                //continue;
                            }
                            else
                            {
                                RenderFeatureLayer rlt = new RenderFeatureLayer(this, datasetCachingContext, fLayer, cancelTracker, fCounter, this);
                                if (fLayer.LabelRenderer != null && fLayer.LabelRenderer.RenderMode == LabelRenderMode.RenderWithFeature)
                                {
                                    rlt.UseLabelRenderer = fLayer.LabelInScale(this);
                                }
                                else
                                {
                                    rlt.UseLabelRenderer = labelLayers.IndexOf(fLayer) == 0;  // letzten Layer gleich mitlabeln
                                }

                                if (rlt.UseLabelRenderer)
                                {
                                    labelLayers.Remove(fLayer);
                                }

                                if (cancelTracker.Continue)
                                {
                                    StartDrawingLayer?.Invoke(this, layer.Title);
                                }

                                await rlt.Render();
                            }
                        }
                        if (layer is IRasterLayer && ((IRasterLayer)layer).RasterClass != null)
                        {
                            IRasterLayer rLayer = (IRasterLayer)layer;
                            if (rLayer.RasterClass.Polygon == null)
                            {
                                continue;
                            }

                            IEnvelope dispEnvelope = DisplayTransformation.TransformedBounds(this); //this.Envelope;
                            if (Display.GeometricTransformer != null)
                            {
                                dispEnvelope = ((IGeometry)Display.GeometricTransformer.InvTransform2D(dispEnvelope)).Envelope;
                            }

                            if (Algorithm.IntersectBox(rLayer.RasterClass.Polygon, dispEnvelope))
                            {
                                if (rLayer.Class is IParentRasterLayer)
                                {
                                    if (cancelTracker.Continue)
                                    {
                                        StartDrawingLayer?.Invoke(this, layer.Title);
                                    }

                                    await DrawRasterParentLayer((IParentRasterLayer)rLayer.Class, cancelTracker, rLayer);
                                }
                                else
                                {
                                    RenderRasterLayer rlt = new RenderRasterLayer(this, rLayer, rLayer, cancelTracker, this);

                                    if (cancelTracker.Continue)
                                    {
                                        StartDrawingLayer?.Invoke(this, layer.Title);
                                    }

                                    await rlt.Render();
                                }
                            }
                        }
                        // Andere Layer (zB IRasterLayer)

                        this.FireDrawingLayerFinished(new TimeEvent("Drawing: " + layer.Title, startTime, DateTime.Now, fCounter.Counter));

                        FireRefreshMapView(drawPhase, 1000);
                    }

                    #endregion

                    #region Label Features

                    if (labelLayers.Count != 0)
                    {
                        //StreamImage(ref _msGeometry, _bitmap);

                        foreach (IFeatureLayer fLayer in labelLayers)
                        {
                            SetGeotransformer(fLayer, geoTransformer);

                            FeatureCounter fCounter = new FeatureCounter();
                            DateTime startTime = DateTime.Now;

                            RenderLabel rlt = new RenderLabel(this, fLayer, cancelTracker, fCounter);

                            if (cancelTracker.Continue)
                            {
                                StartDrawingLayer?.Invoke(this, fLayer.Title);
                            }

                            await rlt.Render();

                            this.FireDrawingLayerFinished(new TimeEvent("Labelling: " + fLayer.Title, startTime, DateTime.Now, fCounter.Counter));

                        }

                        //DrawStream(_canvas, _msGeometry);
                    }

                    LabelEngine.Draw(Display, cancelTracker);
                    LabelEngine.Release();

                    #endregion

                    #region Waiting for Webservices

                    if (cancelTracker.Continue)
                    {
                        if (webServices != null && webServices.Count != 0)
                        {
                            StartDrawingLayer?.Invoke(this, "...Waiting for WebServices...");
                        }

                        while (m_imageMerger.Count < m_imageMerger.max)
                        {
                            await Task.Delay(100);
                        }
                    }
                    if (_drawScaleBar)
                    {
                        m_imageMerger.mapScale = MapScale;
                        m_imageMerger.dpi = Dpi;
                    }
                    if (m_imageMerger.Count > 0)
                    {
                        var clonedBitmap = _bitmap.Clone(GraphicsEngine.PixelFormat.Rgba32);
                        clonedBitmap.MakeTransparent(_backgroundColor);
                        m_imageMerger.Add(new GeorefBitmap(clonedBitmap), 999);

                        if (!m_imageMerger.Merge(_bitmap, Display) &&
                            this is IServiceMap &&
                            ((IServiceMap)this).MapServer != null)
                        {
                            await ((IServiceMap)this).MapServer.LogAsync(
                                Name,
                                "Image Merger:",
                                loggingMethod.error,
                                m_imageMerger.LastErrorMessage);
                        }
                        m_imageMerger.Clear();
                    }

                    //StreamImage(ref _msGeometry, _bitmap);

                    #endregion
                }
                #endregion

                #region Draw Selection

                if (Bit.Has(drawPhase, DrawPhase.Selection))
                {
                    foreach (IDatasetElement layer in MapElements)
                    {
                        if (!cancelTracker.Continue)
                        {
                            break;
                        }

                        if (!(layer is ILayer))
                        {
                            continue;
                        }

                        if (layer is IFeatureLayer &&
                            layer is IFeatureSelection &&
                            ((IFeatureSelection)layer).SelectionSet != null &&
                            ((IFeatureSelection)layer).SelectionSet.Count > 0)
                        {
                            SetGeotransformer((ILayer)layer, geoTransformer);
                            await RenderSelection(layer as IFeatureLayer, cancelTracker);
                        } // Andere Layer (zB IRasterLayer)
                        else if (layer is IWebServiceLayer)
                        {
                            IWebServiceLayer wLayer = (IWebServiceLayer)layer;
                            if (wLayer.WebServiceClass == null)
                            {
                                continue;
                            }

                            foreach (IWebServiceTheme theme in wLayer.WebServiceClass.Themes)
                            {
                                if (theme is IFeatureLayer &&
                                    theme.SelectionRenderer != null &&
                                    theme is IFeatureSelection &&
                                    ((IFeatureSelection)theme).SelectionSet != null &&
                                    ((IFeatureSelection)theme).SelectionSet.Count > 0)
                                {
                                    SetGeotransformer(theme, geoTransformer);
                                    await RenderSelection(theme, cancelTracker);
                                }
                            }
                        }
                    }
                }

                #endregion

                #region Graphics

                if (Bit.Has(drawPhase, DrawPhase.Graphics))
                {
                    foreach (IGraphicElement grElement in Display.GraphicsContainer.Elements)
                    {
                        grElement.Draw(Display);
                    }
                    foreach (IGraphicElement grElement in Display.GraphicsContainer.SelectedElements)
                    {
                        if (grElement is IGraphicElement2)
                        {
                            if (((IGraphicElement2)grElement).Ghost != null)
                            {
                                ((IGraphicElement2)grElement).Ghost.Draw(Display);
                            } ((IGraphicElement2)grElement).DrawGrabbers(Display);
                        }
                    }
                }

                #endregion

                #region Cleanup

                if (geoTransformer != null)
                {
                    GeometricTransformer = null;
                    geoTransformer.Release();
                    geoTransformer = null;
                }

                #endregion

                #region Send Events

                DoRefreshMapView?.Invoke(this);

                #endregion

                return true;
            }
        }
        catch (Exception ex)
        {
            _lastException = ex;
            AddRequestException(ex);
            //System.Windows.Forms.MessageBox.Show(ex.Message+"\n"+ex.InnerException+"\n"+ex.Source);
            return false;
        }
        finally
        {
            if (cancelTracker.Continue)
            {
                AppendRequestExceptionsToImage();

                DoRefreshMapView?.Invoke(this);
            }

            if (_canvas != null)
            {
                _canvas.Dispose();
            }

            _canvas = null;

            DisposeImage();
        }
    }

    async private Task DrawRasterParentLayer(IParentRasterLayer rLayer, ICancelTracker cancelTracker, IRasterLayer rootLayer)
    {
        IRasterPaintContext paintContext = null;

        try
        {
            if (rLayer is ILayer && ((ILayer)rLayer).Class is IRasterClass)
            {
                paintContext = await ((IRasterClass)((ILayer)rLayer).Class).BeginPaint(Display, cancelTracker);
            }
            else if (rLayer is IRasterClass)
            {
                paintContext = await ((IRasterClass)rLayer).BeginPaint(Display, cancelTracker);
            }
            string filterClause = string.Empty;
            if (rootLayer is IRasterCatalogLayer)
            {
                filterClause = ((IRasterCatalogLayer)rootLayer).FilterQuery != null ?
                    ((IRasterCatalogLayer)rootLayer).FilterQuery.WhereClause : string.Empty;
            }

            using (IRasterLayerCursor cursor = await rLayer.ChildLayers(this, filterClause))
            {
                ILayer child;

                int rasterCounter = 0;
                DateTime rasterCounterTime = DateTime.Now;

                if (cursor != null)
                {
                    while ((child = await cursor.NextRasterLayer()) != null)
                    //foreach (ILayer child in ((IParentRasterLayer)rLayer).ChildLayers(this, filterClause))
                    {
                        if (!cancelTracker.Continue)
                        {
                            break;
                        }

                        if (child.Class is IParentRasterLayer)
                        {
                            await DrawRasterParentLayer((IParentRasterLayer)child.Class, cancelTracker, rootLayer);
                            continue;
                        }
                        if (!(child is IRasterLayer))
                        {
                            continue;
                        }

                        IRasterLayer cLayer = (IRasterLayer)child;

                        RenderRasterLayer rlt = new RenderRasterLayer(this, cLayer, rootLayer, cancelTracker, this);

                        if (StartDrawingLayer != null && cancelTracker.Continue)
                        {
                            if (rLayer is ILayer l)
                            {
                                StartDrawingLayer?.Invoke(this, l.Title);
                            }
                        }

                        await rlt.Render();

                        if (rasterCounter++ % 10 == 0 && (DateTime.Now - rasterCounterTime).TotalMilliseconds > 500D)
                        {
                            if (DoRefreshMapView != null && cancelTracker.Continue)
                            {
                                DoRefreshMapView(this);
                            }
                            rasterCounterTime = DateTime.Now;
                        }

                        if (child.Class is IDisposable)
                        {
                            ((IDisposable)child.Class).Dispose();
                        }
                    }

                    if (DoRefreshMapView != null && cancelTracker.Continue)
                    {
                        DoRefreshMapView(this);
                    }
                }
            }
        }
        finally
        {
            if (paintContext != null)
            {
                paintContext.Dispose();
            }
        }
    }

    public void HighlightGeometry(IGeometry geometry, int milliseconds)
    {
        if (geometry == null || _canvas != null)
        {
            return;
        }

        GeometryType type = GeometryType.Unknown;
        if (geometry is IPolygon)
        {
            type = GeometryType.Polygon;
        }
        else if (geometry is IPolyline)
        {
            type = GeometryType.Polyline;
        }
        else if (geometry is IPoint)
        {
            type = GeometryType.Point;
        }
        else if (geometry is IMultiPoint)
        {
            type = GeometryType.Multipoint;
        }
        else if (geometry is IEnvelope)
        {
            type = GeometryType.Envelope;
        }
        if (type == GeometryType.Unknown)
        {
            return;
        }

        ISymbol symbol = null;
        PlugInManager compMan = new PlugInManager();
        IFeatureRenderer renderer = compMan.CreateInstance(KnownObjects.Carto_SimpleRenderer) as IFeatureRenderer;
        if (renderer is ISymbolCreator)
        {
            symbol = ((ISymbolCreator)renderer).CreateStandardHighlightSymbol(type);
        }
        if (symbol == null)
        {
            return;
        }

        try
        {
            using (var bm = GraphicsEngine.Current.Engine.CreateBitmap(Display.ImageWidth, Display.ImageHeight, GraphicsEngine.PixelFormat.Rgba32))
            using (_canvas = bm.CreateCanvas())
            {
                Draw(symbol, geometry);
                NewBitmap?.Invoke(this, bm);

                DoRefreshMapView?.Invoke(this);

                Thread.Sleep(milliseconds);
                _canvas.Clear();

                DoRefreshMapView?.Invoke(this);
            }
        }
        finally
        {
            _canvas = null;
        }
    }

    private DateTime _lastRefresh = DateTime.UtcNow;
    public void FireRefreshMapView(DrawPhase drawPhase, double suppressPeriode = 500)
    {
        if (DoRefreshMapView != null)
        {
            if ((DateTime.UtcNow - _lastRefresh).TotalMilliseconds > suppressPeriode)
            {
                DoRefreshMapView(this);
                _lastRefresh = DateTime.UtcNow;
            }
        }
    }

    #endregion

    #region Private Members

    async private Task RenderSelection(IFeatureLayer fLayer, ICancelTracker cancelTracker)
    {
        if (fLayer == null || !(fLayer is IFeatureSelection))
        {
            return;
        }

        if (fLayer.SelectionRenderer == null)
        {
            return;
        }

        IFeatureSelection fSelection = (IFeatureSelection)fLayer;
        if (fSelection.SelectionSet == null || fSelection.SelectionSet.Count == 0)
        {
            return;
        }

        RenderFeatureLayerSelection rlt = new RenderFeatureLayerSelection(this, fLayer, cancelTracker, this);
        //rlt.Render();

        //Thread thread = new Thread(new ThreadStart(rlt.Render));
        //thread.Start();

        StartDrawingLayer?.Invoke(this, fLayer.Title);

        await rlt.Render();
        //while (thread.IsAlive)
        //{
        //    Thread.Sleep(10);
        //    if (DoRefreshMapView != null && (count % 100) == 0 && cancelTracker.Continue)
        //    {
        //        DoRefreshMapView();
        //    }
        //    count++;
        //}
        if (cancelTracker.Continue)
        {
            DoRefreshMapView?.Invoke(this);
        }
    }

    private void RenderWebServiceRequestFinished(RenderServiceRequest sender, bool succeeded, GeorefBitmap image, int order)
    {
        if (FinishedDrawingLayer != null && sender != null && sender.WebServiceLayer != null)
        {
            try
            {
                IDataset ds = this[sender.WebServiceLayer.DatasetID];
                FinishedDrawingLayer(this, new TimeEvent("Map Request: " +
                    sender.WebServiceLayer.Title +
                    (ds != null ? " (" + ds.DatasetName + ")" : string.Empty),
                    sender.StartTime, sender.FinishTime));
            }
            catch { }
        }
        if (succeeded)
        {
            m_imageMerger.Add(image, order);
        }
        else
        {
            m_imageMerger.max--;
        }
    }

    private void FireDrawingLayerFinished(ITimeEvent timeEvent)
    {
        FinishedDrawingLayer?.Invoke(this, timeEvent);
    }

    #endregion

    #region Disposing

    public override void Dispose()
    {
        if (_original != null)
        {
            _original = null;
        }

        base.Dispose();
    }

    internal bool DisposeImage()
    {
        if (_bitmap != null)
        {
            if (_canvas != null)
            {
                return false;  // irgendwas tut sich noch
                               //lock (_canvas)
                               //{
                               //    _canvas.Dispose();
                               //    _canvas = null;
                               //}
            }
            NewBitmap?.Invoke(this, null);

            _bitmap.Dispose();
            _bitmap = null;

            //DisposeStreams();
        }
        return true;
    }

    public bool DisposeGraphics()
    {
        if (_canvas == null)
        {
            return true;
        }

        lock (_canvas)
        {
            _canvas.Dispose();
            _canvas = null;
        }
        return true;
    }

    #endregion
}
