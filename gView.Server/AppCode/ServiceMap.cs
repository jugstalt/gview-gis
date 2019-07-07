using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system;
using gView.Framework.UI;
using gView.MapServer;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using gView.Framework.IO;

namespace gView.Server.AppCode
{
    public class ServiceMap : Map, IServiceMap, IDisposable
    {
        private XmlNode _layerDefs = null;
        private XmlNodeList _LAYERS = null;
        private IMapServer _mapServer = null;
        private IServiceRequestInterpreter _interpreter = null;
        private ServiceRequest _request = null;
        private IEnumerable<IMapApplicationModule> _modules = null;

        //private bool _ceckLayerVisibilityBeforeDrawing;

        private ServiceMap() { }

        async static public Task<ServiceMap> CreateAsync(Map original, IMapServer mapServer, IEnumerable<IMapApplicationModule> modules)
        {
            var serviceMap = new ServiceMap();

            serviceMap._mapServer = mapServer;
            serviceMap._modules = modules;
            serviceMap._layers = original._layers;
            serviceMap._datasets = original._datasets;

            serviceMap.m_imageMerger = new ImageMerger2();

            serviceMap.m_name = original.Name;
            serviceMap._toc = original._toc;
            //serviceMap._ceckLayerVisibilityBeforeDrawing = true;
            serviceMap._mapUnits = original.MapUnits;
            serviceMap._displayUnits = original.DisplayUnits;
            serviceMap.refScale = original.refScale;

            serviceMap.SpatialReference = original.Display.SpatialReference;
            serviceMap.LayerDefaultSpatialReference = original.LayerDefaultSpatialReference != null ? original.LayerDefaultSpatialReference.Clone() as ISpatialReference : null;

            serviceMap._drawScaleBar = false;

            // Metadata
            await serviceMap.SetProviders(await original.GetProviders());
            serviceMap._debug = false;

            return serviceMap;
        }

        public XmlNode LayerDefs
        {
            set { _layerDefs = value; }
        }
        public XmlNodeList LAYERS
        {
            set { _LAYERS = value; }
        }

        async public Task<bool> SaveImage(string path, System.Drawing.Imaging.ImageFormat format)
        {
            if (_image == null)
            {
                return false;
            }

            try
            {
                try
                {
                    if (Display.MakeTransparent &&
                        format != ImageFormat.Jpeg &&
                        format != ImageFormat.Gif)
                    {
                        _image.MakeTransparent(Display.TransparentColor);
                    }
                }
                catch (Exception ex)
                {
                    if (this.MapServer != null)
                    {
                        await this.MapServer.LogAsync(
                            this.Name,
                            "RenderRasterLayerThread", loggingMethod.error,
                            "Image.MakeTransparent\nPath='" + path + "'\nFormat=" + format.ToString() + "\n" +
                            ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
                    }
                }
                //_image.Save(path, format);
                _image.SaveOrUpload(path, format);
                _image.Dispose();
                _image = null;
                return true;
            }
            catch (Exception ex)
            {
                if (this.MapServer != null)
                {
                    await this.MapServer.LogAsync(
                        this.Name,
                        "RenderRasterLayerThread", loggingMethod.error,
                        "Image.Save\nPath='" + path + "'\nFormat=" + format.ToString() + "\n" +
                        ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
                }
                return false;
            }
        }

        async public Task<bool> SaveImage(Stream ms, System.Drawing.Imaging.ImageFormat format)
        {
            if (_image == null)
            {
                return false;
            }

            try
            {
                try
                {
                    if (Display.MakeTransparent &&
                        format != ImageFormat.Jpeg &&
                        format != ImageFormat.Gif)
                    {
                        _image.MakeTransparent(Display.TransparentColor);
                    }
                }
                catch (Exception ex)
                {
                    if (this.MapServer != null)
                    {
                        await this.MapServer.LogAsync(
                            this.Name,
                            "RenderRasterLayerThread", loggingMethod.error,
                            "Image.MakeTransparent\n'\nFormat=" + format.ToString() + "\n" +
                            ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
                    }
                }
                _image.Save(ms, format);
                _image.Dispose();
                _image = null;
                return true;
            }
            catch (Exception ex)
            {
                if (this.MapServer != null)
                {
                    await this.MapServer.LogAsync(
                        this.Name,
                        "RenderRasterLayerThread", loggingMethod.error,
                        "Image.Save\n'\nFormat=" + format.ToString() + "\n" +
                        ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
                }
                return false;
            }
        }

        public void ReleaseImage()
        {
            if (_image == null)
            {
                return;
            }

            try
            {
                _image.Dispose();
                _image = null;
            }
            catch { }
        }

        public System.Drawing.Bitmap MapImage
        {
            get { return _image; }
        }
        async public Task<System.Drawing.Bitmap> Legend()
        {
            ITOC toc = _toc.Clone(this) as ITOC;

            #region WebServiceLayer
            List<IWebServiceLayer> webServices;
            if (this.TOC != null)
            {
                webServices = ListOperations<IWebServiceLayer>.Swap(this.TOC.VisibleWebServiceLayers);
            }
            else
            {
                webServices = new List<IWebServiceLayer>();
                foreach (IDatasetElement layer in this.MapElements)
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
            foreach (IWebServiceLayer element in webServices)
            {
                if (!(element is ILayer))
                {
                    continue;
                }

                if (!element.Visible)
                {
                    continue;
                }

                IWebServiceLayer wsLayer = LayerFactory.Create(element.WebServiceClass.Clone() as IClass, element as ILayer) as IWebServiceLayer;
                if (wsLayer == null || wsLayer.WebServiceClass == null)
                {
                    continue;
                }

                if (BeforeRenderLayers != null)
                {
                    // layer im geklonten TOC austauschen...
                    // Besser layer als layer.Class verwendenden, weil Class von mehrerenen Layern
                    // verwendet werden kann zB bei gesplitteten Layern...
                    //ITOCElement tocElement = toc.GetTOCElement(element.Class);
                    ITOCElement tocElement = toc.GetTOCElement(element);
                    tocElement.RemoveLayer(element as ILayer);
                    tocElement.AddLayer(wsLayer);

                    List<ILayer> modLayers = new List<ILayer>();
                    foreach (IWebServiceTheme theme in wsLayer.WebServiceClass.Themes)
                    {
                        if (theme is ILayer)
                        {
                            modLayers.Add(theme as ILayer);
                        }
                    }
                    BeforeRenderLayers(this, modLayers);
                }
            }
            #endregion

            List<ILayer> layers = new List<ILayer>();
            if (this.TOC != null)
            {
                if (this.GetType().Equals(typeof(ServiceMap)))
                {
                    layers = ListOperations<ILayer>.Swap(this.TOC.Layers);
                }
                else
                {
                    layers = ListOperations<ILayer>.Swap(this.TOC.VisibleLayers);
                }
            }
            else
            {
                layers = new List<ILayer>();
                foreach (IDatasetElement layer in this.MapElements)
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

            if (BeforeRenderLayers != null)
            {
                //
                // Kopie der Original Layer erstellen
                // ACHTUNG: Renderer werden nicht kopiert!
                // dürfen in BeforeRenderLayers nicht verändert werden...
                // Eine zuweisung eines neuen Renderers ist jedoch legitim.
                //
                List<ILayer> modLayers = new List<ILayer>();
                foreach (IDatasetElement element in layers)
                {
                    if (!(element is ILayer) || element is IWebServiceTheme)
                    {
                        continue;
                    }

                    ILayer layer = (ILayer)element;
                    if (layer.MinimumScale > 1 && layer.MinimumScale > this.mapScale)
                    {
                        continue;
                    }

                    if (layer.MaximumScale > 1 && layer.MaximumScale < this.mapScale)
                    {
                        continue;
                    }

                    ILayer newLayer = null;
                    modLayers.Add(newLayer = LayerFactory.Create(layer.Class, layer));

                    // layer im geklonten TOC austauschen...
                    if (element is ILayer && newLayer != null)
                    {
                        // Besser layer als layer.Class verwendenden, weil Class von mehrerenen Layern
                        // verwendet werden kann zB bei gesplitteten Layern...
                        //ITOCElement tocElement = toc.GetTOCElement(layer.Class);
                        ITOCElement tocElement = toc.GetTOCElement(layer);
                        tocElement.RemoveLayer(element as ILayer);
                        tocElement.AddLayer(newLayer);
                    }
                }
                BeforeRenderLayers(this, modLayers);
                layers = modLayers;
            }

            return await toc.Legend();
        }

        async override public Task<bool> RefreshMap(DrawPhase phase, ICancelTracker cancelTracker)
        {
            base._requestExceptions = null;

            if (_graphics != null && phase == DrawPhase.Graphics)
            {
                return true;
            }

            this.ZoomTo(m_actMinX, m_actMinY, m_actMaxX, m_actMaxY);

            if (cancelTracker == null)
            {
                cancelTracker = new CancelTracker();
            }

            GeometricTransformer geoTransformer = new GeometricTransformer();
            //geoTransformer.ToSpatialReference = this.SpatialReference;

            if (_image == null)
            {
                _image = new System.Drawing.Bitmap(iWidth, iHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }

            _graphics = System.Drawing.Graphics.FromImage(_image);
            //_graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            this.dpi = _graphics.DpiX * this.ScaleSymbolFactor;

            if (BackgroundColor.A != 0 && !Display.MakeTransparent)
            {
                using (System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(BackgroundColor))
                {
                    _graphics.FillRectangle(brush, 0, 0, _image.Width, _image.Height);
                }
            }

            if (phase == DrawPhase.All || phase == DrawPhase.Geography)
            {
                this.GeometricTransformer = geoTransformer;

                // Thread für MapServer Datasets starten...

                #region WebServiceLayer
                List<IWebServiceLayer> webServices;
                if (this.TOC != null)
                {
                    webServices = ListOperations<IWebServiceLayer>.Swap(this.TOC.VisibleWebServiceLayers);
                }
                else
                {
                    webServices = new List<IWebServiceLayer>();
                    foreach (IDatasetElement layer in this.MapElements)
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
                int webServiceOrder = 0, webServiceOrder2 = 1;
                foreach (IWebServiceLayer element in webServices)
                {
                    if (!element.Visible)
                    {
                        continue;
                    }

                    IWebServiceLayer wsLayer = LayerFactory.Create(element.WebServiceClass.Clone() as IClass, element as ILayer) as IWebServiceLayer;

                    if (wsLayer == null || wsLayer.WebServiceClass == null)
                    {
                        continue;
                    }

                    wsLayer.WebServiceClass.SpatialReference = this.SpatialReference;

                    List<IWebServiceClass> additionalWebServices = new List<IWebServiceClass>();
                    if (BeforeRenderLayers != null)
                    {
                        List<ILayer> modLayers = new List<ILayer>();
                        foreach (IWebServiceTheme theme in wsLayer.WebServiceClass.Themes)
                        {
                            if (theme is ILayer)
                            {
                                modLayers.Add(theme as ILayer);
                            }
                        }
                        BeforeRenderLayers(this, modLayers);

                        foreach (ILayer additionalLayer in MapServerHelper.FindAdditionalWebServiceLayers(wsLayer.WebServiceClass, modLayers))
                        {
                            IWebServiceClass additionalWebService = MapServerHelper.CloneNonVisibleWebServiceClass(wsLayer.WebServiceClass);
                            MapServerHelper.CopyWebThemeProperties(additionalWebService, additionalLayer);

                            if (MapServerHelper.HasVisibleThemes(additionalWebService))
                            {
                                additionalWebServices.Add(additionalWebService);
                            }
                        }
                    }


                    ServiceRequestThread srt = new ServiceRequestThread(this, wsLayer, webServiceOrder++);
                    srt.finish += new ServiceRequestThread.RequestThreadFinished(MapRequestThread_finished);
                    //Thread thread = new Thread(new ThreadStart(srt.ImageRequest));
                    m_imageMerger.max++;
                    //thread.Start();
                    var task = srt.ImageRequest(); // start Task and continue...


                    foreach (IWebServiceClass additionalWebService in additionalWebServices)
                    {
                        wsLayer = LayerFactory.Create(additionalWebService, element as ILayer) as IWebServiceLayer;
                        if (wsLayer == null || wsLayer.WebServiceClass == null)
                        {
                            continue;
                        }

                        wsLayer.WebServiceClass.SpatialReference = this.SpatialReference;

                        srt = new ServiceRequestThread(this, wsLayer, (++webServiceOrder2) + webServices.Count);
                        srt.finish += new ServiceRequestThread.RequestThreadFinished(MapRequestThread_finished);
                        //thread = new Thread(new ThreadStart(srt.ImageRequest));
                        m_imageMerger.max++;
                        //thread.Start();
                        var additionalTask = srt.ImageRequest(); // start task and continue...
                    }
                }
                #endregion

                List<ILayer> layers = new List<ILayer>();
                if (this.TOC != null)
                {
                    if (this.GetType().Equals(typeof(ServiceMap)))
                    {
                        layers = ListOperations<ILayer>.Swap(this.TOC.Layers);
                    }
                    else
                    {
                        layers = ListOperations<ILayer>.Swap(this.TOC.VisibleLayers);
                    }
                }
                else
                {
                    layers = new List<ILayer>();
                    foreach (IDatasetElement layer in this.MapElements)
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

                if (BeforeRenderLayers != null)
                {
                    //
                    // Kopie der Original Layer erstellen
                    // ACHTUNG: Renderer werden nicht kopiert!
                    // dürfen in BeforeRenderLayers nicht verändert werden...
                    // Eine zuweisung eines neuen Renderers ist jedoch legitim.
                    //
                    List<ILayer> modLayers = new List<ILayer>();
                    foreach (IDatasetElement element in layers)
                    {
                        if (!(element is ILayer) || element is IWebServiceTheme)
                        {
                            continue;
                        }

                        ILayer layer = (ILayer)element;
                        if (layer.MinimumScale > 1 && layer.MinimumScale > this.mapScale)
                        {
                            continue;
                        }

                        if (layer.MaximumScale > 1 && layer.MaximumScale < this.mapScale)
                        {
                            continue;
                        }

                        modLayers.Add(LayerFactory.Create(layer.Class, layer));
                    }
                    BeforeRenderLayers(this, modLayers);
                    layers = modLayers;
                }
                //layers = ModifyLayerList(layers);
                List<IFeatureLayer> labelLayers = this.OrderedLabelLayers(layers);

                LabelEngine.Init(this.Display, false);
                foreach (IDatasetElement element in layers)
                {
                    if (!cancelTracker.Continue)
                    {
                        break;
                    }

                    if (!(element is ILayer))
                    {
                        continue;
                    }

                    ILayer layer = (ILayer)element;

                    //if (_ceckLayerVisibilityBeforeDrawing)
                    //{
                    //    if (!LayerIsVisible(layer)) continue;
                    //}
                    if (!layer.Visible)
                    {
                        continue;
                    }

                    if (layer.MinimumScale > 1 && layer.MinimumScale > this.mapScale)
                    {
                        continue;
                    }

                    if (layer.MaximumScale > 1 && layer.MaximumScale < this.mapScale)
                    {
                        continue;
                    }
#if (DEBUG)
                    //Logger.LogDebug("Drawing Layer:" + element.Title);
#endif
                    SetGeotransformer((ILayer)element, geoTransformer);

                    if (layer is IFeatureLayer)
                    {
                        IFeatureLayer fLayer = (IFeatureLayer)layer;
                        if (fLayer.FeatureRenderer == null &&
                             (
                              fLayer.LabelRenderer == null ||
                              (fLayer.LabelRenderer != null && fLayer.LabelRenderer.RenderMode != LabelRenderMode.RenderWithFeature)
                             ))
                        {
                            //continue;
                        }
                        else
                        {
                            RenderFeatureLayerThread rlt = new RenderFeatureLayerThread(this, fLayer, cancelTracker, new FeatureCounter());
                            if (fLayer.LabelRenderer != null && fLayer.LabelRenderer.RenderMode == LabelRenderMode.RenderWithFeature)
                            {
                                rlt.UseLabelRenderer = true;
                            }
                            else
                            {
                                rlt.UseLabelRenderer = labelLayers.IndexOf(fLayer) == 0;  // letzten Layer gleich mitlabeln
                            }

                            if (rlt.UseLabelRenderer)
                            {
                                labelLayers.Remove(fLayer);
                            }

                            await rlt.Render();
                        }
                        //thread = new Thread(new ThreadStart(rlt.Render));
                        //thread.Start();
                    }
                    if (layer is IRasterLayer && ((IRasterLayer)layer).RasterClass != null)
                    {
                        IRasterLayer rLayer = (IRasterLayer)layer;
                        if (rLayer.RasterClass.Polygon == null)
                        {
                            continue;
                        }

                        IEnvelope dispEnvelope = this.Envelope;
                        if (Display.GeometricTransformer != null)
                        {
                            dispEnvelope = ((IGeometry)Display.GeometricTransformer.InvTransform2D(dispEnvelope)).Envelope;
                        }

                        if (gView.Framework.SpatialAlgorithms.Algorithm.IntersectBox(rLayer.RasterClass.Polygon, dispEnvelope))
                        {
                            if (rLayer.Class is IParentRasterLayer)
                            {
                                await DrawRasterParentLayer((IParentRasterLayer)rLayer.Class, cancelTracker, rLayer);
                            }
                            else
                            {
                                RenderRasterLayerThread rlt = new RenderRasterLayerThread(this, rLayer, rLayer, cancelTracker);
                                await rlt.Render();

                                //thread = new Thread(new ThreadStart(rlt.Render));
                                //thread.Start();
                            }
                        }
                    }
                    // Andere Layer (zB IRasterLayer)

#if(DEBUG)          
                    //Logger.LogDebug("Finished drawing layer: " + element.Title);
#endif
                }

                // Label Features
                if (labelLayers.Count != 0)
                {
                    foreach (IFeatureLayer fLayer in labelLayers)
                    {
                        this.SetGeotransformer(fLayer, geoTransformer);

                        if (!fLayer.Visible)
                        {
                            continue;
                        }

                        RenderLabelThread rlt = new RenderLabelThread(this, fLayer, cancelTracker);
                        await rlt.Render();
                    }
                }

                LabelEngine.Draw(this.Display, cancelTracker);
                LabelEngine.Release();

                if (cancelTracker.Continue)
                {
                    while (m_imageMerger.Count < m_imageMerger.max)
                    {
                        await Task.Delay(10);
                    }
                }
                if (_drawScaleBar)
                {
                    m_imageMerger.mapScale = this.mapScale;
                    m_imageMerger.dpi = this.dpi;
                }
#if(DEBUG)
                //Logger.LogDebug("Merge Images");
#endif
                m_imageMerger.Merge(_image, this.Display);
                m_imageMerger.Clear();
#if(DEBUG)
                //Logger.LogDebug("Merge Images Finished");
#endif
            }

            if (phase == DrawPhase.All || phase == DrawPhase.Graphics)
            {
                foreach (IGraphicElement grElement in Display.GraphicsContainer.Elements)
                {
                    grElement.Draw(Display);
                }
            }

            base.AppendExceptionsToImage();

            if (_graphics != null)
            {
                _graphics.Dispose();
            }

            _graphics = null;

            if (geoTransformer != null)
            {
                this.GeometricTransformer = null;
                geoTransformer.Release();
                geoTransformer = null;
            }
            return true;
        }

        async override protected Task DrawRasterParentLayer(IParentRasterLayer rLayer, ICancelTracker cancelTracker, IRasterLayer rootLayer)
        {
            if (rLayer is ILayer && ((ILayer)rLayer).Class is IRasterClass)
            {
                await ((IRasterClass)((ILayer)rLayer).Class).BeginPaint(this.Display, cancelTracker);
            }
            else if (rLayer is IRasterClass)
            {
                await ((IRasterClass)rLayer).BeginPaint(this.Display, cancelTracker);
            }
            string filterClause = String.Empty;
            if (rootLayer is IRasterCatalogLayer)
            {
                filterClause = ((((IRasterCatalogLayer)rootLayer).FilterQuery != null) ?
                    ((IRasterCatalogLayer)rootLayer).FilterQuery.WhereClause : String.Empty);
            }

            using (IRasterLayerCursor cursor = await rLayer.ChildLayers(this, filterClause))
            {
                ILayer child;

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

                    RenderRasterLayerThread rlt = new RenderRasterLayerThread(this, cLayer, rootLayer, cancelTracker);
                    await rlt.Render();

                    if (child.Class is IDisposable)
                    {
                        ((IDisposable)child.Class).Dispose();
                    }
                }
            }
            if (rLayer is ILayer && ((ILayer)rLayer).Class is IRasterClass)
            {
                ((IRasterClass)((ILayer)rLayer).Class).EndPaint(cancelTracker);
            }
            else if (rLayer is IRasterClass)
            {
                ((IRasterClass)rLayer).EndPaint(cancelTracker);
            }
        }

        protected override void DrawStream(System.IO.Stream stream)
        {

        }
        protected override void StreamImage(ref System.IO.MemoryStream stream, System.Drawing.Image image)
        {

        }

        #region IServiceMap Member

        //public event LayerIsVisibleHook OverrideLayerIsVisible;
        public event BeforeRenderLayersEvent BeforeRenderLayers;

        async public Task<bool> Render()
        {
            return await RefreshMap(DrawPhase.All, null);
        }

        private float _scaleSymbolFactor = 1f;
        public float ScaleSymbolFactor
        {
            get
            {
                return _scaleSymbolFactor;
            }
            set
            {
                _scaleSymbolFactor = value;
            }
        }

        public void SetRequestContext(IServiceRequestContext context)
        {
            if (context != null)
            {
                _mapServer = context.MapServer;
                _interpreter = context.ServiceRequestInterpreter;
                _request = context.ServiceRequest;
            }
        }

        public T GetModule<T>()
        {
            if (_modules == null)
            {
                return default(T);
            }

            var module = _modules.Where(m => m.GetType().Equals(typeof(T))).FirstOrDefault();
            return (T)module;
        }

        #endregion

        #region IServiceRequestContext Member

        public IMapServer MapServer
        {
            get { return _mapServer; }
        }

        public IServiceRequestInterpreter ServiceRequestInterpreter
        {
            get { return _interpreter; }
        }

        public ServiceRequest ServiceRequest
        {
            get { return _request; }
        }

        Task<IServiceMap> IServiceRequestContext.CreateServiceMapInstance()
        {
            throw new NotImplementedException();
            //return this;
        }

        #endregion

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
