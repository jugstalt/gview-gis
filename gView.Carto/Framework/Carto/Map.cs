using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.IO;
using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.Carto.UI;

namespace gView.Framework.Carto
{
    public enum MapTools { ZoomIn, ZoomOut, Pan }

    /// <summary>
    /// Zusammenfassung für Map.
    /// </summary>
    public class Map : Display, IMap, IPersistable, IMetadata, IDebugging
    {
        public virtual event LayerAddedEvent LayerAdded;
        public virtual event LayerRemovedEvent LayerRemoved;
        public virtual event TOCChangedEvent TOCChanged;
        public virtual event NewBitmapEvent NewBitmap;
        public virtual event DoRefreshMapViewEvent DoRefreshMapView;
        public virtual event DrawingLayerEvent DrawingLayer;
        public virtual event DrawingLayerFinishedEvent DrawingLayerFinished;
        public virtual event StartRefreshMapEvent StartRefreshMap;
        public event NewExtentRenderedEvent NewExtentRendered;
        public event EventHandler MapRenamed;

        internal ImageMerger2 m_imageMerger;
        internal TOC _toc;
        private TOC _dataViewTOC;
        private MemoryStream _msGeometry = null, _msSelection = null;
        private SelectionEnvironment _selectionEnv;
        //protected MapDB.mapDB m_mapDB=null;
        protected string m_mapName = "", m_name/*,m_imageName="",m_origImageName=""*/;
        protected int m_mapID = -1;
        protected MapTools m_mapTool = MapTools.ZoomIn;
        protected ArrayList m_activeLayerNames;
        protected bool _drawScaleBar = false;
        internal List<IDataset> _datasets;
        internal List<ILayer> _layers = new List<ILayer>();
        internal bool _debug = true;
        private Envelope _lastRenderExtent = null;

        private IntegerSequence _layerIDSequece = new IntegerSequence();
        public object imageLocker = new object();

        protected List<Exception> _requestExceptions = null;

        public Map()
        {
            _datasets = new List<IDataset>();

            m_imageMerger = new ImageMerger2();

            //m_imageMerger.outputPath=conn.outputPath;
            //m_imageMerger.outputUrl=conn.outputUrl;

            m_activeLayerNames = new ArrayList();
            m_name = "Map1";
            _toc = new TOC(this);
            _selectionEnv = new SelectionEnvironment();

            this.refScale = 500;
        }

        internal Map(Map original, bool writeNamespace)
            : this()
        {
            m_name = original.Name;
            this.Display.MapUnits = original.Display.MapUnits;
            this.Display.DisplayUnits = original.Display.DisplayUnits;
            this.Display.DisplayTransformation.DisplayRotation = original.Display.DisplayTransformation.DisplayRotation;
            this.refScale = original.Display.refScale;
            this.Display.SpatialReference = original.Display.SpatialReference != null ? original.SpatialReference.Clone() as ISpatialReference : null;
            this.LayerDefaultSpatialReference = original.LayerDefaultSpatialReference != null ? original.LayerDefaultSpatialReference.Clone() as ISpatialReference : null;

            _toc = new TOC(this); //original.TOC.Clone() as TOC;

            _datasets = ListOperations<IDataset>.Clone(original._datasets);
            _layers = new List<ILayer>();

            //if (modifyLayerTitles)
            {
                _layerIDSequece = new IntegerSequence();
                Append(original, writeNamespace);
            }
            //else
            {
                //_layerIDSequece = new IntegerSequence(original._layerIDSequece.Number);

                //foreach (IDatasetElement element in original.MapElements)
                //{
                //    if (element is ILayer)
                //    {
                //        ILayer layer = LayerFactory.Create(element.Class, element as ILayer);
                //        if (layer == null)  // Grouplayer (element.Class==null) ???
                //            continue;

                //        ITOCElement tocElement = _toc.GetTOCElement(element as ILayer);
                //        if (tocElement != null)
                //        {
                //            tocElement.RemoveLayer(element as ILayer);
                //            tocElement.AddLayer(layer);
                //        }

                //        //layer.Title = original.Name + ":" + layer.Title;
                //        _layers.Add(layer);
                //    }
                //}
            }
        }

        internal void Append(Map map, bool writeNamespace)
        {
            Dictionary<IGroupLayer, IGroupLayer> groupLayers = new Dictionary<IGroupLayer, IGroupLayer>();

            foreach (IDatasetElement element in map.MapElements)
            {
                if (element is ILayer)
                {
                    ILayer layer = null;
                    if (element.Class is IWebServiceClass)
                    {
                        IWebServiceClass wClass = ((IWebServiceClass)element.Class).Clone() as IWebServiceClass;
                        if (wClass == null) continue;

                        layer = LayerFactory.Create(wClass, element as ILayer);
                        layer.ID = _layerIDSequece.Number;

                        ITOCElement tocElement = _toc.GetTOCElement(element as ILayer);
                        if (tocElement != null)
                        {
                            tocElement.RemoveLayer(element as ILayer);
                            tocElement.AddLayer(layer);
                        }

                        if (writeNamespace)
                            layer.Namespace = map.Name;

                        AddLayer(layer);

                        foreach (IWebServiceTheme theme in wClass.Themes)
                        {
                            theme.ID = _layerIDSequece.Number;
                            if (writeNamespace)
                                theme.Namespace = map.Name;
                        }
                    }
                    else
                    {
                        if (element.Class == null && !(element is IGroupLayer))
                        {
                            layer = new NullLayer();
                            layer.Title = element.Title;
                            layer.DatasetID = element.DatasetID;
                        }
                        else
                            layer = LayerFactory.Create(element.Class, element as ILayer);

                        layer.ID = _layerIDSequece.Number;

                        ITOCElement tocElement = _toc.GetTOCElement(element as ILayer);
                        if (tocElement != null)
                        {
                            tocElement.RemoveLayer(element as ILayer);
                            tocElement.AddLayer(layer);
                        }
                        //_layers.Add(layer);


                        if (writeNamespace)
                            layer.Namespace = map.Name;


                        AddLayer(layer);

                        if (layer is IGroupLayer && element is IGroupLayer)
                            groupLayers.Add(element as IGroupLayer, layer as IGroupLayer);
                    }

                    if (layer == null) continue;
                    ITOCElement newTocElement = _toc.GetTOCElement(layer);
                    ITOCElement oriTocElement = map.TOC.GetTOCElement(element as ILayer);
                    if (newTocElement != null && oriTocElement != null)
                    {
                        _toc.RenameElement(newTocElement, oriTocElement.Name);

                        // Layergruppierung
                        if (oriTocElement.ParentGroup != null &&
                            oriTocElement.ParentGroup.Layers.Count == 1 &&
                            oriTocElement.ParentGroup.Layers[0] is IGroupLayer)
                        {
                            IGroupLayer newGroupLayer;
                            if (groupLayers.TryGetValue(oriTocElement.ParentGroup.Layers[0] as IGroupLayer, out newGroupLayer))
                            {
                                ITOCElement newGroupElement = _toc.GetTOCElement(newGroupLayer);
                                if (newGroupLayer != null)
                                {
                                    _toc.Add2Group(newTocElement, newGroupElement);
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void Dispose()
        {
            DisposeGraphicsAndImage();
            if (m_imageMerger != null)
            {
                m_imageMerger.Dispose();
                m_imageMerger = null;
            }
        }

        public void Release()
        {
            this.Dispose();
        }

        internal void DisposeGraphicsAndImage()
        {
            if (_graphics != null)
            {
                try { _graphics.Dispose(); }
                catch { }
                _graphics = null;
            }
            if (_image != null)
            {
                _image.Dispose();
                _image = null;
            }
        }

        public string Name
        {
            get { return m_name; }
            set
            {
                if (m_name != value)
                {
                    m_name = value;
                    if (MapRenamed != null)
                        MapRenamed(this, new EventArgs());
                }
            }
        }

        public MapTools MapTool
        {
            get { return m_mapTool; }
            set { m_mapTool = value; }
        }

        /*
        public string mapImage 
        {
            get 
            {
                return m_imageName;
            }
        }
        */

        internal void MapRequestThread_finished(ServiceRequestThread sender, bool succeeded, GeorefBitmap image, int order)
        {
            if (DrawingLayerFinished != null && sender != null && sender.WebServiceLayer != null)
            {
                IDataset ds = this[sender.WebServiceLayer.DatasetID];
                DrawingLayerFinished(this, new TimeEvent("Map Request: " +
                    sender.WebServiceLayer.Title +
                    ((ds != null) ? " (" + ds.DatasetName + ")" : String.Empty),
                    sender.StartTime, sender.FinishTime));
            }
            if (succeeded)
                m_imageMerger.Add(image, order);
            else
                m_imageMerger.max--;
        }

        internal bool DisposeImage()
        {
            if (_image != null)
            {
                if (_graphics != null)
                {
                    return false;  // irgendwas tut sich noch
                    lock (_graphics)
                    {
                        _graphics.Dispose();
                        _graphics = null;
                    }
                }
                if (NewBitmap != null) NewBitmap(null);
                _image.Dispose();
                _image = null;
                DisposeStreams();
            }
            return true;
        }
        private void DisposeStreams()
        {
            if (_msGeometry != null)
            {
                _msGeometry.Dispose();
                _msGeometry = null;
            }
            if (_msSelection != null)
            {
                _msSelection.Dispose();
                _msSelection = null;
            }
        }
        internal bool DisposeGraphics()
        {
            if (_graphics == null) return true;
            lock (_graphics)
            {
                _graphics.Dispose();
                _graphics = null;
            }
            return true;
        }
        #region getLayer
        private int m_datasetNr, m_layerNr;
        private void resetGetLayer()
        {
            m_datasetNr = m_layerNr = 0;
        }
        private IDatasetElement getNextLayer(string layername)
        {
            while (this[m_datasetNr] != null)
            {
                IDataset dataset = this[m_datasetNr];
                if (!(dataset is IFeatureDataset)) continue;
                IFeatureDataset fDataset = (IFeatureDataset)dataset;

                for (int i = m_layerNr; i < fDataset.Elements.Count; i++)
                {
                    m_layerNr++;
                    IDatasetElement layer = (IDatasetElement)fDataset.Elements[i];
                    string name = layer.Title;

                    if (layername == name) return layer;
                }
                m_layerNr = 0;
                m_datasetNr++;
            }
            return null;
        }
        #endregion

        protected List<IFeatureLayer> OrderedLabelLayers(List<ILayer> layers)
        {
            //////////////////////////////////////////
            //
            // Die zu labelden layer ermittel
            //
            List<IFeatureLayer> labelLayers = new List<IFeatureLayer>();
            foreach (ILayer layer in layers)
            {
                if (!(layer is IFeatureLayer)) continue;

                if (layer.MinimumScale > 1 && layer.MinimumScale > this.mapScale) continue;
                if (layer.MaximumScale > 1 && layer.MaximumScale < this.mapScale) continue;

                if (layer.MinimumLabelScale > 1 && layer.MinimumLabelScale > this.mapScale) continue;
                if (layer.MaximumLabelScale > 1 && layer.MaximumLabelScale < this.mapScale) continue;

                IFeatureLayer fLayer = (IFeatureLayer)layer;
                if (fLayer.LabelRenderer == null) continue;
                if (fLayer.LabelRenderer.RenderMode == LabelRenderMode.RenderWithFeature) continue;

                labelLayers.Add(fLayer);
            }
            labelLayers = ListOperations<IFeatureLayer>.Swap(labelLayers);
            labelLayers = ListOperations<IFeatureLayer>.Sort(labelLayers, new LabelLayersPrioritySorter());
            return labelLayers;
            ////////////////////////////////////////
        }
        private class LabelLayersPrioritySorter : IComparer<IFeatureLayer>
        {

            #region IComparer<IFeatureLayer> Member

            public int Compare(IFeatureLayer x, IFeatureLayer y)
            {
                if (x == null || y == null || x == y) return 0;

                IPriority p1 = x.LabelRenderer as IPriority;
                IPriority p2 = y.LabelRenderer as IPriority;
                if (p1 == null || p2 == null) return 0;

                if (p1.Priority < p2.Priority)
                    return -1;
                else if (p1.Priority > p2.Priority)
                    return 1;
                return 0;
            }

            #endregion
        }

        #region IMap
        public void AddDataset(IDataset service, int order)
        {
            /*
            service.order = order;
            m_datasets.Add(service);

            foreach (IDatasetElement layer in ((IDataset)service).Elements)
            {
                ((TOC)this.TOC).AddLayer(layer, null);
            }

            if (DatasetAdded != null) DatasetAdded(this, service);
            if (SpatialReference == null && service is IFeatureDataset)
            {
                SpatialReference = ((IFeatureDataset)service).SpatialReference;
            }
             * */
        }
        public IDataset this[int datasetIndex]
        {
            get
            {
                if (datasetIndex < 0 || datasetIndex >= _datasets.Count) return null;
                return (IDataset)_datasets[datasetIndex];
            }
        }

        public IDataset this[IDatasetElement layer]
        {
            get
            {
                if (layer is ServiceFeatureLayer)
                    layer = ((ServiceFeatureLayer)layer).FeatureLayer;

                foreach (ILayer element in _layers)
                {
                    if (element == layer)
                    {
                        if (element.DatasetID >= 0 && element.DatasetID < _datasets.Count)
                        {
                            return _datasets[element.DatasetID];
                        }
                    }
                    if (element is IWebServiceLayer && ((IWebServiceLayer)element).WebServiceClass != null && ((IWebServiceLayer)element).WebServiceClass.Themes != null)
                    {
                        foreach (IWebServiceTheme theme in ((IWebServiceLayer)element).WebServiceClass.Themes)
                        {
                            if (theme == layer)
                            {
                                if (element.DatasetID >= 0 && element.DatasetID < _datasets.Count)
                                {
                                    return _datasets[element.DatasetID];
                                }
                            }
                        }
                    }
                }

                // Zukünftig:
                // Weiters die Liste mit den Standalone Tables durchsuchen...

                return null;
            }
        }

        public IEnumerable<IDataset> Datasets
        {
            get
            {
                if (_datasets == null)
                    return new List<IDataset>();

                return new List<IDataset>(_datasets);
            }
        }

        public void RemoveDataset(IDataset dataset)
        {
            int index = _datasets.IndexOf(dataset);
            if (index == -1)
                return;

            foreach (ILayer layer in ListOperations<ILayer>.Clone(_layers))
            {
                if (layer.DatasetID == index)
                {
                    this.RemoveLayer(layer, false);
                }
                else if (layer.DatasetID > index)
                {
                    layer.DatasetID = layer.DatasetID - 1;
                }
            }

            _datasets.Remove(dataset);
            CheckDatasetCopyright();
        }

        public void RemoveAllDatasets()
        {
            _datasets.Clear();
            if (_toc != null) _toc.RemoveAllElements();
            _layers.Clear();
        }

        public string ActiveLayerNames
        {
            get
            {
                string names = "";
                foreach (string activeLayerName in m_activeLayerNames)
                {
                    if (names != "") names += ";";
                    names += activeLayerName;
                }
                return names;
            }
            set
            {
                m_activeLayerNames = new ArrayList();
                foreach (string activeLayerName in value.Split(';'))
                {
                    if (m_activeLayerNames.IndexOf(activeLayerName) != -1) continue;
                    m_activeLayerNames.Add(activeLayerName);
                }
            }
        }

        internal void SetNewLayerID(ILayer layer)
        {
            if (layer == null) return;

            while (LayerIDExists(layer.ID))
                layer.ID = _layerIDSequece.Number;
        }

        public void AddLayer(ILayer layer)
        {
            AddLayer(layer, -1);
        }
        public void AddLayer(ILayer layer, int pos)
        {
            if (layer == null) return;

            SetNewLayerID(layer);

            _layers.Add(layer);

            if (pos < 0)
                _toc.AddLayer(layer, null);
            else
                _toc.AddLayer(layer, null, pos);

            if (SpatialReference == null)
            {
                if (layer is IFeatureLayer && ((IFeatureLayer)layer).FeatureClass != null &&
                    ((IFeatureLayer)layer).FeatureClass.SpatialReference is SpatialReference)
                {
                    SpatialReference = new SpatialReference(((IFeatureLayer)layer).FeatureClass.SpatialReference as SpatialReference);
                }
                else if (layer is IRasterLayer && ((IRasterLayer)layer).RasterClass != null &&
                    ((IRasterLayer)layer).RasterClass.SpatialReference is SpatialReference)
                {
                    SpatialReference = new SpatialReference(((IRasterLayer)layer).RasterClass.SpatialReference as SpatialReference);
                }
                else if (layer is IWebServiceLayer && ((IWebServiceLayer)layer).WebServiceClass != null &&
                    ((IWebServiceLayer)layer).WebServiceClass.SpatialReference is SpatialReference)
                {
                    SpatialReference = new SpatialReference(((IWebServiceLayer)layer).WebServiceClass.SpatialReference as SpatialReference);
                }
            }

            int datasetID = -1;
            if (layer.Class != null && layer.Class.Dataset != null)
            {
                foreach (IDataset dataset in _datasets)
                {
                    if (dataset == layer.Class.Dataset ||
                        (dataset.ConnectionString == layer.Class.Dataset.ConnectionString &&
                        dataset.GetType().Equals(layer.Class.Dataset.GetType())))
                    {
                        datasetID = _datasets.IndexOf(dataset);
                        break;
                    }
                }
                if (datasetID == -1)
                {
                    _datasets.Add(layer.Class.Dataset);
                    datasetID = _datasets.IndexOf(layer.Class.Dataset);
                }
            }
            layer.DatasetID = datasetID;

            if (layer.GroupLayer != null)
            {
                TOCElement parent = _toc.GetTOCElement(layer.GroupLayer) as TOCElement;
                if (parent != null)
                {
                    TOCElement tocElement = _toc.GetTOCElement(layer) as TOCElement;
                    if (tocElement != null) tocElement.ParentGroup = parent;
                }
                else if (layer is Layer)
                {
                    ((Layer)layer).GroupLayer = null;
                }
            }

            CheckDatasetCopyright();
            
            if (LayerAdded != null) LayerAdded(this, layer);
        }

        public void RemoveLayer(ILayer layer)
        {
            RemoveLayer(layer, true);
        }
        private void RemoveLayer(ILayer layer, bool removeUnusedDataset)
        {
            if (layer == null) return;
            IDataset layerDataset = this[layer];

            if (layer is IGroupLayer)
            {
                foreach (ILayer cLayer in ((IGroupLayer)layer).ChildLayer)
                {
                    RemoveLayer(cLayer);
                }
                _layers.Remove(layer);
                _toc.RemoveLayer(layer);
            }
            else if (layer is IWebServiceLayer)
            {
                foreach (ILayer wLayer in ((IWebServiceLayer)layer).WebServiceClass.Themes)
                {
                    RemoveLayer(wLayer);
                }
                _layers.Remove(layer);
                _toc.RemoveLayer(layer);
            }
            else
            {
                _layers.Remove(layer);
                _toc.RemoveLayer(layer);
            }

            if (LayerRemoved != null) LayerRemoved(this, layer);

            if (removeUnusedDataset)
            {
                bool found = false;
                foreach (ILayer l in _layers)
                {
                    if (this[l] == layerDataset)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    this.RemoveDataset(layerDataset);
                }

                CheckDatasetCopyright();
            }
        }

        public List<IDatasetElement> ActiveLayers
        {
            get
            {
                List<IDatasetElement> e = new List<IDatasetElement>();

                foreach (string activeLayerName in m_activeLayerNames)
                {
                    this.resetGetLayer();
                    IDatasetElement layer = this.getNextLayer(activeLayerName);
                    while (layer != null)
                    {
                        e.Add(layer);
                        layer = this.getNextLayer(activeLayerName);
                    }
                }
                return e;
            }
        }

        public List<IDatasetElement> Elements(string aliasname)
        {
            List<IDatasetElement> e = new List<IDatasetElement>();

            this.resetGetLayer();
            IDatasetElement layer = this.getNextLayer(aliasname);
            while (layer != null)
            {
                e.Add(layer);
                layer = this.getNextLayer(aliasname);
            }
            return e;
        }
        public List<IDatasetElement> MapElements
        {
            get
            {
                List<IDatasetElement> e = new List<IDatasetElement>();

                foreach (ILayer layer in _layers)
                {
                    e.Add(layer);
                }
                // + Standalone Tables

                return e;
            }
        }
        public IDatasetElement DatasetElementByClass(IClass cls)
        {
            foreach (IDatasetElement element in _layers)
            {
                if (element == null) continue;
                if (element.Class == cls)
                    return element;
            }
            return null;
        }
        public void ClearSelection()
        {
            foreach (IDatasetElement layer in MapElements)
            {
                if (layer is IWebServiceLayer && ((IWebServiceLayer)layer).WebServiceClass != null && ((IWebServiceLayer)layer).WebServiceClass.Themes != null)
                {
                    foreach (IWebServiceTheme theme in ((IWebServiceLayer)layer).WebServiceClass.Themes)
                    {
                        if (!(theme is IFeatureSelection)) continue;

                        ISelectionSet themeSelSet = ((IFeatureSelection)theme).SelectionSet;
                        if (themeSelSet == null) continue;

                        if (themeSelSet.Count > 0)
                        {
                            ((IFeatureSelection)theme).ClearSelection();
                            ((IFeatureSelection)theme).FireSelectionChangedEvent();
                        }
                    }
                }

                if (!(layer is IFeatureSelection)) continue;

                ISelectionSet selSet = ((IFeatureSelection)layer).SelectionSet;
                if (selSet == null) continue;

                if (selSet.Count > 0)
                {
                    ((IFeatureSelection)layer).ClearSelection();
                    ((IFeatureSelection)layer).FireSelectionChangedEvent();
                }
            }
        }

        virtual public bool RefreshMap(DrawPhase phase, ICancelTracker cancelTracker)
        {
            _requestExceptions = null;
            bool printerMap = (this.GetType() == typeof(PrinterMap));

            if (StartRefreshMap != null) StartRefreshMap(this);

            Thread mapRefreshThread = new Thread(new ParameterizedThreadStart(MapViewRefreshThread));
            try
            {
                _lastException = null;

                if (_graphics != null && phase == DrawPhase.Graphics) return true;

                #region Start Drawing/Initialisierung
                this.ZoomTo(m_actMinX, m_actMinY, m_actMaxX, m_actMaxY);

                if (cancelTracker == null) cancelTracker = new CancelTracker();

                GeometricTransformer geoTransformer = new GeometricTransformer();
                //geoTransformer.ToSpatialReference = this.SpatialReference;
                if (!printerMap)
                {
                    if (_image != null && (_image.Width != iWidth || _image.Height != iHeight))
                    {

                        if (!DisposeImage()) return false;
                    }

                    if (_image == null)
                    {
                        DisposeStreams();
                        _image = new System.Drawing.Bitmap(iWidth, iHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        //if (NewBitmap != null && cancelTracker.Continue) NewBitmap(_image);
                    }
                    // NewBitmap immer aufrufen, da sonst neuer DataView nix mitbekommt
                    if (NewBitmap != null && cancelTracker.Continue) NewBitmap(_image);


                    _graphics = System.Drawing.Graphics.FromImage(_image);
                    this.dpi = _graphics.DpiX;

                    using (System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(_backgroundColor))
                    {
                        _graphics.FillRectangle(brush, 0, 0, _image.Width, _image.Height);
                        brush.Dispose();
                    }


                    if (DoRefreshMapView != null)
                    {
                        mapRefreshThread.Start(cancelTracker);
                    }
                }
                #endregion

                #region Geometry
                if (Bit.Has(phase, DrawPhase.Geography))
                //if (phase == DrawPhase.All || phase == DrawPhase.Geography)
                {
                    LabelEngine.Init(this.Display, printerMap);

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
                            if (!(layer is IWebServiceLayer)) continue;
                            if (((ILayer)layer).Visible)
                                webServices.Add((IWebServiceLayer)layer);
                        }
                    }
                    int webServiceOrder = 0;
                    foreach (IWebServiceLayer element in webServices)
                    {
                        if (!element.Visible) continue;
                        ServiceRequestThread srt = new ServiceRequestThread(this, element, webServiceOrder++);
                        srt.finish += new ServiceRequestThread.RequestThreadFinished(MapRequestThread_finished);
                        Thread thread = new Thread(new ThreadStart(srt.ImageRequest));
                        m_imageMerger.max++;
                        thread.Start();
                    }
                    #endregion

                    #region Layerlisten erstellen
                    List<ILayer> layers;
                    if (this.TOC != null)
                    {
                        if (this.ToString() == "gView.MapServer.Instance.ServiceMap")
                            layers = ListOperations<ILayer>.Swap(this.TOC.Layers);
                        else
                            layers = ListOperations<ILayer>.Swap(this.TOC.VisibleLayers);
                    }
                    else
                    {
                        layers = new List<ILayer>();
                        foreach (IDatasetElement layer in this.MapElements)
                        {
                            if (!(layer is ILayer)) continue;
                            if (((ILayer)layer).Visible)
                                layers.Add((ILayer)layer);
                        }
                    }

                    List<IFeatureLayer> labelLayers = this.OrderedLabelLayers(layers);
                    #endregion

                    #region Renderer Features
                    foreach (ILayer layer in layers)
                    {
                        if (!cancelTracker.Continue) break;

                        if (layer.MinimumScale > 1 && layer.MinimumScale > this.mapScale) continue;
                        if (layer.MaximumScale > 1 && layer.MaximumScale < this.mapScale) continue;

                        SetGeotransformer(layer, geoTransformer);

                        DateTime startTime = DateTime.Now;

                        Thread thread = null;
                        FeatureCounter fCounter = new FeatureCounter();
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
                                RenderFeatureLayerThread rlt = new RenderFeatureLayerThread(this, fLayer, cancelTracker, fCounter);
                                if (fLayer.LabelRenderer != null && fLayer.LabelRenderer.RenderMode == LabelRenderMode.RenderWithFeature)
                                    rlt.UseLabelRenderer = true;
                                else
                                    rlt.UseLabelRenderer = labelLayers.IndexOf(fLayer) == 0;  // letzten Layer gleich mitlabeln

                                if (rlt.UseLabelRenderer) labelLayers.Remove(fLayer);

                                thread = new Thread(new ThreadStart(rlt.Render));
                                thread.Start();

                                if (DrawingLayer != null && cancelTracker.Continue) DrawingLayer(layer.Title);
                            }
                        }
                        if (layer is IRasterLayer && ((IRasterLayer)layer).RasterClass != null)
                        {
                            IRasterLayer rLayer = (IRasterLayer)layer;
                            if (rLayer.RasterClass.Polygon == null) continue;

                            IEnvelope dispEnvelope = this.DisplayTransformation.TransformedBounds(this); //this.Envelope;
                            if (Display.GeometricTransformer != null)
                            {
                                dispEnvelope = (IEnvelope)((IGeometry)Display.GeometricTransformer.InvTransform2D(dispEnvelope)).Envelope;
                            }

                            if (gView.Framework.SpatialAlgorithms.Algorithm.IntersectBox(rLayer.RasterClass.Polygon, dispEnvelope))
                            {
                                if (rLayer.Class is IParentRasterLayer)
                                {
                                    DrawRasterParentLayer((IParentRasterLayer)rLayer.Class, cancelTracker, rLayer);
                                    thread = null;
                                }
                                else
                                {
                                    RenderRasterLayerThread rlt = new RenderRasterLayerThread(this, rLayer, rLayer, cancelTracker);

                                    thread = new Thread(new ThreadStart(rlt.Render));
                                    thread.Start();

                                    if (DrawingLayer != null && cancelTracker.Continue) DrawingLayer(layer.Title);
                                }
                            }
                        }
                        // Andere Layer (zB IRasterLayer)

                        if (thread == null) continue;
                        thread.Join();

                        if (DrawingLayerFinished != null)
                        {
                            DrawingLayerFinished(this, new gView.Framework.system.TimeEvent("Drawing: " + layer.Title, startTime, DateTime.Now, fCounter.Counter));
                        }
                        //int count = 0;

                        //while (thread.IsAlive)
                        //{
                        //    Thread.Sleep(10);
                        //    if (DoRefreshMapView != null && (count % 100) == 0 && cancelTracker.Continue) DoRefreshMapView();
                        //    count++;
                        //}
                        //if (DoRefreshMapView != null && cancelTracker.Continue) DoRefreshMapView();
                    }
                    #endregion

                    #region Label Features
                    if (labelLayers.Count != 0)
                    {
                        StreamImage(ref _msGeometry, _image);
                        foreach (IFeatureLayer fLayer in labelLayers)
                        {
                            this.SetGeotransformer(fLayer, geoTransformer);

                            DateTime startTime = DateTime.Now;

                            RenderLabelThread rlt = new RenderLabelThread(this, fLayer, cancelTracker);
                            Thread thread = new Thread(new ThreadStart(rlt.Render));
                            thread.Start();

                            if (DrawingLayer != null && cancelTracker.Continue) DrawingLayer(fLayer.Title);

                            if (thread == null) continue;
                            thread.Join();

                            if (DrawingLayerFinished != null)
                            {
                                DrawingLayerFinished(this, new gView.Framework.system.TimeEvent("Labelling: " + fLayer.Title, startTime, DateTime.Now));
                            }

                            //int count = 0;
                            //while (thread.IsAlive)
                            //{
                            //    Thread.Sleep(10);
                            //    if (DoRefreshMapView != null && (count % 100) == 0 && cancelTracker.Continue)
                            //    {
                            //        _labelEngine.Draw(this.Display, cancelTracker);
                            //        DoRefreshMapView();
                            //    }
                            //    count++;
                            //}
                            //if (DoRefreshMapView != null && cancelTracker.Continue) DoRefreshMapView();
                        }
                        DrawStream(_msGeometry);
                    }

                    if (!printerMap) LabelEngine.Draw(this.Display, cancelTracker);
                    LabelEngine.Release();
                    #endregion

                    #region Waiting for Webservices
                    if (cancelTracker.Continue)
                    {
                        if (DrawingLayer != null && m_imageMerger.max > 0) DrawingLayer("...Waiting for WebServices...");
                        while (m_imageMerger.Count < m_imageMerger.max)
                        {
                            Thread.Sleep(100);
                        }
                    }
                    if (_drawScaleBar)
                    {
                        m_imageMerger.mapScale = this.mapScale;
                        m_imageMerger.dpi = this.dpi;
                    }
                    if (m_imageMerger.Count > 0)
                    {
                        lock (this.imageLocker)
                        {
                            System.Drawing.Bitmap clonedBitmap = _image.Clone(new System.Drawing.Rectangle(0, 0, _image.Width, _image.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                            clonedBitmap.MakeTransparent(_backgroundColor);
                            m_imageMerger.Add(new GeorefBitmap(clonedBitmap), 999);
                            if (!m_imageMerger.Merge(_image, this.Display) &&
                                (this is IServiceMap) &&
                                ((IServiceMap)this).MapServer != null)
                            {
                                ((IServiceMap)this).MapServer.Log(
                                    "Image Merger:",
                                    loggingMethod.error,
                                    m_imageMerger.LastErrorMessage);
                            }
                            m_imageMerger.Clear();
                        }
                    }

                    //if (DoRefreshMapView != null && cancelTracker.Continue) DoRefreshMapView();

                    StreamImage(ref _msGeometry, _image);
                    #endregion
                }
                #endregion

                #region Draw Selection
                if (Bit.Has(phase, DrawPhase.Selection))
                //if (phase == DrawPhase.All || phase == DrawPhase.Selection)
                {
                    if (phase != DrawPhase.All) DrawStream(_msGeometry);

                    foreach (IDatasetElement layer in this.MapElements)
                    {
                        if (!cancelTracker.Continue) break;
                        if (!(layer is ILayer)) continue;

                        if (layer is IFeatureLayer &&
                            layer is IFeatureSelection &&
                            ((IFeatureSelection)layer).SelectionSet != null &&
                            ((IFeatureSelection)layer).SelectionSet.Count > 0)
                        {
                            SetGeotransformer((ILayer)layer, geoTransformer);
                            RenderSelection(layer as IFeatureLayer, cancelTracker);
                        } // Andere Layer (zB IRasterLayer)
                        else if (layer is IWebServiceLayer)
                        {
                            IWebServiceLayer wLayer = (IWebServiceLayer)layer;
                            if (wLayer.WebServiceClass == null) continue;

                            foreach (IWebServiceTheme theme in wLayer.WebServiceClass.Themes)
                            {
                                if (theme is IFeatureLayer &&
                                    theme.SelectionRenderer != null &&
                                    theme is IFeatureSelection &&
                                    ((IFeatureSelection)theme).SelectionSet != null &&
                                    ((IFeatureSelection)theme).SelectionSet.Count > 0)
                                {
                                    SetGeotransformer((ILayer)theme, geoTransformer);
                                    RenderSelection(theme as IFeatureLayer, cancelTracker);
                                }
                            }
                        }
                    }

                    StreamImage(ref _msSelection, _image);
                }
                #endregion

                #region Graphics
                if (Bit.Has(phase, DrawPhase.Graphics))
                //if (phase == DrawPhase.All || phase == DrawPhase.Graphics)
                {
                    if (phase != DrawPhase.All)
                    {
                        DrawStream((_msSelection != null) ? _msSelection : _msGeometry);
                    }

                    foreach (IGraphicElement grElement in Display.GraphicsContainer.Elements)
                    {
                        grElement.Draw(Display);
                    }
                    foreach (IGraphicElement grElement in Display.GraphicsContainer.SelectedElements)
                    {
                        if (grElement is IGraphicElement2)
                        {
                            if (((IGraphicElement2)grElement).Ghost != null)
                                ((IGraphicElement2)grElement).Ghost.Draw(Display);

                            ((IGraphicElement2)grElement).DrawGrabbers(Display);
                        }
                    }
                }
                #endregion

                #region Cleanup
                if (geoTransformer != null)
                {
                    this.GeometricTransformer = null;
                    geoTransformer.Release();
                    geoTransformer = null;
                }
                #endregion

                #region Send Events
                // Überprüfen, ob sich Extent seit dem letztem Zeichnen geändert hat...
                if (cancelTracker.Continue)
                {
                    if (_lastRenderExtent == null) _lastRenderExtent = new Envelope();
                    if (NewExtentRendered != null)
                    {
                        if (!_lastRenderExtent.Equals(Display.Envelope))
                        {
                            NewExtentRendered(this, Display.Envelope);
                        }
                    }
                    _lastRenderExtent.minx = Display.Envelope.minx;
                    _lastRenderExtent.miny = Display.Envelope.miny;
                    _lastRenderExtent.maxx = Display.Envelope.maxx;
                    _lastRenderExtent.maxy = Display.Envelope.maxy;
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                _lastException = ex;
                AddException(ex);
                //System.Windows.Forms.MessageBox.Show(ex.Message+"\n"+ex.InnerException+"\n"+ex.Source);
                return false;
            }
            finally
            {
                if (mapRefreshThread.IsAlive)
                {
                    mapRefreshThread.Abort();
                    mapRefreshThread.Join();
                }

                AppendExceptionsToImage();

                if (!printerMap)
                {
                    if (_graphics != null) _graphics.Dispose();
                    _graphics = null;
                }
            }
        }

        public void HighlightGeometry(IGeometry geometry, int milliseconds)
        {
            if (geometry == null || _image == null || _graphics != null) return;

            geometryType type = geometryType.Unknown;
            if (geometry is IPolygon)
            {
                type = geometryType.Polygon;
            }
            else if (geometry is IPolyline)
            {
                type = geometryType.Polyline;
            }
            else if (geometry is IPoint)
            {
                type = geometryType.Point;
            }
            else if (geometry is IMultiPoint)
            {
                type = geometryType.Multipoint;
            }
            else if (geometry is IEnvelope)
            {
                type = geometryType.Envelope;
            }
            if (type == geometryType.Unknown) return;

            ISymbol symbol = null;
            PlugInManager compMan = new PlugInManager();
            IFeatureRenderer renderer = compMan.CreateInstance(gView.Framework.system.KnownObjects.Carto_SimpleRenderer) as IFeatureRenderer;
            if (renderer is ISymbolCreator)
            {
                symbol = ((ISymbolCreator)renderer).CreateStandardHighlightSymbol(type);
            }
            if (symbol == null) return;

            System.Drawing.Bitmap bm = new System.Drawing.Bitmap(_image.Width, _image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            _graphics = System.Drawing.Graphics.FromImage(bm);
            _graphics.DrawImage(_image, 0, 0);

            this.Draw(symbol, geometry);
            if (NewBitmap != null) NewBitmap(bm);
            if (DoRefreshMapView != null) DoRefreshMapView();
            Thread.Sleep(milliseconds);
            if (NewBitmap != null) NewBitmap(_image);
            if (DoRefreshMapView != null) DoRefreshMapView();

            bm.Dispose();
            bm = null;
            _graphics.Dispose();
            _graphics = null;
        }

        public ITOC TOC
        {
            get
            {
                if (_dataViewTOC != null) return _dataViewTOC;
                return _toc;
            }
        }

        public ISelectionEnvironment SelectionEnvironment
        {
            get { return _selectionEnv; }
        }

        public IDisplay Display
        {
            get { return this; }
        }

        public ISpatialReference LayerDefaultSpatialReference
        {
            get;
            set;
        }
        #endregion

        private bool LayerIDExists(int layerID)
        {
            if (_layers == null) return false;
            foreach (ILayer layer in _layers)
            {
                if (layer.ID == layerID) return true;
            }
            return false;
        }

        private void RenderSelection(IFeatureLayer fLayer, ICancelTracker cancelTracker)
        {
            if (fLayer == null || !(fLayer is IFeatureSelection)) return;
            if (fLayer.SelectionRenderer == null) return;
            IFeatureSelection fSelection = (IFeatureSelection)fLayer;
            if (fSelection.SelectionSet == null || fSelection.SelectionSet.Count == 0) return;

            RenderFeatureLayerSelectionThread rlt = new RenderFeatureLayerSelectionThread(this, fLayer, cancelTracker);
            //rlt.Render();

            Thread thread = new Thread(new ThreadStart(rlt.Render));
            thread.Start();

            if (DrawingLayer != null && cancelTracker.Continue) DrawingLayer(fLayer.Title);

            int count = 0;
            while (thread.IsAlive)
            {
                Thread.Sleep(10);
                if (DoRefreshMapView != null && (count % 100) == 0 && cancelTracker.Continue)
                {
                    DoRefreshMapView();
                }
                count++;
            }
            if (DoRefreshMapView != null && cancelTracker.Continue) DoRefreshMapView();
        }

        /// <summary>
        /// Setzt die Display.Geotransformer variable, abhängig davon, ob ein Layer Transformiert werden muss.
        /// Soll keine Transformation ausgeführt werden wird Display.Geotransformer auf null gesetzt...
        /// !!! Transformiert wird in den unterliegenden Thread (Display.DrawSymbol,...) immer dann, wenn Display.Geotransformater != null ist!!!
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="geotransformer"></param>
        protected void SetGeotransformer(ILayer layer, IGeometricTransformer geotransformer)
        {
            if (geotransformer == null)
            {
                Display.GeometricTransformer = null;
                return;
            }
            if (this.SpatialReference == null)
            {
                Display.GeometricTransformer = null;
                return;
            }

            ISpatialReference layerSR = null;
            if (layer is IFeatureLayer)
            {
                if (((IFeatureLayer)layer).FeatureClass == null)
                {
                    Display.GeometricTransformer = null;
                    return;
                }
                layerSR = ((IFeatureLayer)layer).FeatureClass.SpatialReference;
            }
            else if (layer is IRasterLayer && ((IRasterLayer)layer).RasterClass != null)
            {
                layerSR = ((IRasterLayer)layer).RasterClass.SpatialReference;
            }

            if (layerSR == null)
                layerSR = this.LayerDefaultSpatialReference;

            if (layerSR == null)
            {
                Display.GeometricTransformer = null;
                return;
            }

            if (this.SpatialReference.Equals(layerSR))
            {
                Display.GeometricTransformer = null;
                return;
            }

            geotransformer.SetSpatialReferences(layerSR, this.SpatialReference);
            Display.GeometricTransformer = geotransformer;
        }

        virtual protected void DrawRasterParentLayer(IParentRasterLayer rLayer, ICancelTracker cancelTracker, IRasterLayer rootLayer)
        {
            if (rLayer is ILayer && ((ILayer)rLayer).Class is IRasterClass)
            {
                ((IRasterClass)((ILayer)rLayer).Class).BeginPaint(this.Display, cancelTracker);
            }
            else if (rLayer is IRasterClass)
            {
                ((IRasterClass)rLayer).BeginPaint(this.Display, cancelTracker);
            }
            string filterClause = String.Empty;
            if (rootLayer is IRasterCatalogLayer)
            {
                filterClause = ((((IRasterCatalogLayer)rootLayer).FilterQuery != null) ?
                    ((IRasterCatalogLayer)rootLayer).FilterQuery.WhereClause : String.Empty);
            }

            using (IRasterLayerCursor cursor = ((IParentRasterLayer)rLayer).ChildLayers(this, filterClause))
            {
                ILayer child;

                while ((child = cursor.NextRasterLayer) != null)
                //foreach (ILayer child in ((IParentRasterLayer)rLayer).ChildLayers(this, filterClause))
                {
                    if (!cancelTracker.Continue) break;
                    if (child.Class is IParentRasterLayer)
                    {
                        DrawRasterParentLayer((IParentRasterLayer)child.Class, cancelTracker, rootLayer);
                        continue;
                    }
                    if (!(child is IRasterLayer)) continue;
                    IRasterLayer cLayer = (IRasterLayer)child;

                    RenderRasterLayerThread rlt = new RenderRasterLayerThread(this, cLayer, rootLayer, cancelTracker);

                    Thread thread = new Thread(new ThreadStart(rlt.Render));
                    thread.Start();

                    if (DrawingLayer != null && cancelTracker.Continue)
                    {
                        if (rLayer is ILayer) DrawingLayer(((ILayer)rLayer).Title);
                    }
                    // WarteSchleife
                    int counter = 0;

                    while (thread.IsAlive)
                    {
                        Thread.Sleep(100);
                        if (DoRefreshMapView != null && (counter % 10) == 0 && cancelTracker.Continue) DoRefreshMapView();
                        counter++;
                    }
                    if (DoRefreshMapView != null && cancelTracker.Continue) DoRefreshMapView();

                    if (child.Class is IDisposable)
                        ((IDisposable)child.Class).Dispose();
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

        #region IPersistable Member

        public string PersistID
        {
            get
            {
                return null;
            }
        }

        public void Load(IPersistStream stream)
        {
            m_name = (string)stream.Load("name", "");
            m_minX = (double)stream.Load("minx", 0.0);
            m_minY = (double)stream.Load("miny", 0.0);
            m_maxX = (double)stream.Load("maxx", 0.0);
            m_maxY = (double)stream.Load("maxy", 0.0);

            m_actMinX = (double)stream.Load("act_minx", 0.0);
            m_actMinY = (double)stream.Load("act_miny", 0.0);
            m_actMaxX = (double)stream.Load("act_maxx", 0.0);
            m_actMaxY = (double)stream.Load("act_maxy", 0.0);

            m_refScale = (double)stream.Load("refScale", 0.0);

            m_iWidth = (int)stream.Load("iwidth", 1);
            m_iHeight = (int)stream.Load("iheight", 1);

            _backgroundColor = System.Drawing.Color.FromArgb(
                (int)stream.Load("background", (int)System.Drawing.Color.White.ToArgb()));

            _mapUnits = (GeoUnits)stream.Load("MapUnits", 0);
            _displayUnits = (GeoUnits)stream.Load("DisplayUnits", 0);

            ISpatialReference sRef = new SpatialReference();
            this.SpatialReference = (ISpatialReference)stream.Load("SpatialReference", null, sRef);
            //LayerDefaultSpatialReference
            ISpatialReference ldsRef = new SpatialReference();
            this.LayerDefaultSpatialReference = (ISpatialReference)stream.Load("LayerDefaultSpatialReference", null, ldsRef);

            _layerIDSequece = (IntegerSequence)stream.Load("layerIDSequence", new IntegerSequence(), new IntegerSequence());

            IDataset dataset;
            //while ((dataset = (IDataset)stream.Load("IDataset", /*new gView.Carto.Framework.Carto.UnknownDataset()*/ null)) != null)
            while ((dataset = stream.LoadPlugin<IDataset>("IDataset", new gView.Carto.Framework.Carto.UnknownDataset()/* null*/)) != null)
            {
                if (dataset.State != DatasetState.opened)
                {
                    dataset.Open();
                }
                _datasets.Add(dataset);
            }

            GroupLayer gLayer;
            while ((gLayer = (GroupLayer)stream.Load("IGroupLayer", null, new GroupLayer())) != null)
            {
                while (LayerIDExists(gLayer.ID))
                    gLayer.ID = _layerIDSequece.Number;

                _layers.Add(gLayer);
            }

            FeatureLayer fLayer;
            while ((fLayer = (FeatureLayer)stream.Load("IFeatureLayer", null, new FeatureLayer())) != null)
            {
                if (fLayer.DatasetID < _datasets.Count)
                {
                    IDatasetElement element = _datasets[fLayer.DatasetID][fLayer.Title];
                    if (element != null && element.Class is IFeatureClass)
                    {
                        fLayer = LayerFactory.Create(element.Class, fLayer) as FeatureLayer;
                        //fLayer.SetFeatureClass(element.Class as IFeatureClass);
                    }
                }
                else
                {
                    //fLayer.DatasetID = -1;
                }
                while (LayerIDExists(fLayer.ID))
                    fLayer.ID = _layerIDSequece.Number;

                _layers.Add(fLayer);

                if (LayerAdded != null) LayerAdded(this, fLayer);
            }

            RasterCatalogLayer rcLayer;
            while ((rcLayer = (RasterCatalogLayer)stream.Load("IRasterCatalogLayer", null, new RasterCatalogLayer())) != null)
            {
                if (rcLayer.DatasetID < _datasets.Count)
                {
                    IDatasetElement element = _datasets[rcLayer.DatasetID][rcLayer.Title];
                    if (element != null && element.Class is IRasterCatalogClass)
                    {
                        rcLayer = LayerFactory.Create(element.Class, rcLayer) as RasterCatalogLayer;
                    }
                }
                else
                {
                }
                SetNewLayerID(rcLayer);

                _layers.Add(rcLayer);
                if (LayerAdded != null) LayerAdded(this, fLayer);
            }

            RasterLayer rLayer;
            while ((rLayer = (RasterLayer)stream.Load("IRasterLayer", null, new RasterLayer())) != null)
            {
                if (rLayer.DatasetID < _datasets.Count)
                {
                    IDatasetElement element = _datasets[rLayer.DatasetID][rLayer.Title];
                    if (element != null && element.Class is IRasterClass)
                    {
                        rLayer.SetRasterClass(element.Class as IRasterClass);
                    }
                }
                else
                {
                }
                while (LayerIDExists(rLayer.ID))
                    rLayer.ID = _layerIDSequece.Number;

                _layers.Add(rLayer);

                if (LayerAdded != null) LayerAdded(this, rLayer);
            }

            WebServiceLayer wLayer;
            while ((wLayer = (WebServiceLayer)stream.Load("IWebServiceLayer", null, new WebServiceLayer())) != null)
            {
                if (wLayer.DatasetID <= _datasets.Count)
                {
                    IDatasetElement element = _datasets[wLayer.DatasetID][wLayer.Title];
                    if (element != null && element.Class is IWebServiceClass)
                    {
                        //wLayer = LayerFactory.Create(element.Class, wLayer) as WebServiceLayer;
                        wLayer.SetWebServiceClass(element.Class as IWebServiceClass);
                    }
                }
                else
                {
                }
                while (LayerIDExists(wLayer.ID))
                    wLayer.ID = _layerIDSequece.Number;

                _layers.Add(wLayer);
                if (wLayer.WebServiceClass != null && wLayer.WebServiceClass.Themes != null)
                {
                    foreach (IWebServiceTheme theme in wLayer.WebServiceClass.Themes)
                    {
                        while (LayerIDExists(theme.ID) || theme.ID == 0)
                            theme.ID = _layerIDSequece.Number;
                    }

                    if (LayerAdded != null) LayerAdded(this, wLayer);
                }
            }

            stream.Load("IClasses", null, new PersistableClasses(_layers));
            _toc = (TOC)stream.Load("ITOC", null, new TOC(this));
            stream.Load("IGraphicsContainer", null, this.GraphicsContainer);

            foreach (ILayer layer in _layers)
            {
                if (layer is IFeatureLayer && ((IFeatureLayer)layer).Joins != null)
                {
                    foreach (IFeatureLayerJoin join in ((IFeatureLayer)layer).Joins)
                    {
                        join.OnCreate(this);
                    }
                    layer.FirePropertyChanged();
                }
            }
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("name", m_name);
            stream.Save("minx", m_minX);
            stream.Save("miny", m_minY);
            stream.Save("maxx", m_maxX);
            stream.Save("maxy", m_maxY);

            stream.Save("act_minx", m_actMinX);
            stream.Save("act_miny", m_actMinY);
            stream.Save("act_maxx", m_actMaxX);
            stream.Save("act_maxy", m_actMaxY);

            stream.Save("refScale", m_refScale);

            stream.Save("iwidth", iWidth);
            stream.Save("iheight", iHeight);

            stream.Save("background", _backgroundColor.ToArgb());

            if (this.SpatialReference != null)
            {
                stream.Save("SpatialReference", this.SpatialReference);
            }
            if (this.LayerDefaultSpatialReference != null)
            {
                stream.Save("LayerDefaultSpatialReference", this.LayerDefaultSpatialReference);
            }

            stream.Save("layerIDSequence", _layerIDSequece);

            stream.Save("MapUnits", (int)this.MapUnits);
            stream.Save("DisplayUnits", (int)this.DisplayUnits);

            foreach (IDataset dataset in _datasets)
            {
                stream.Save("IDataset", dataset);
            }

            foreach (ILayer layer in _layers)
            {
                if (layer is IGroupLayer)
                    stream.Save("IGroupLayer", layer);
                else if (layer is IRasterCatalogLayer)
                    stream.Save("IRasterCatalogLayer", layer);
                else if (layer is IRasterLayer)
                    stream.Save("IRasterLayer", layer);
                else if (layer is IWebServiceLayer)
                    stream.Save("IWebServiceLayer", layer);
                else if (layer is IFeatureLayer)
                    stream.Save("IFeatureLayer", layer);
            }

            stream.Save("IClasses", new PersistableClasses(_layers));
            stream.Save("ITOC", _toc);
            stream.Save("IGraphicsContainer", Display.GraphicsContainer);
        }

        private class PersistableClasses : IPersistable
        {
            private List<ILayer> _layers;
            public PersistableClasses(List<ILayer> layers)
            {
                _layers = layers;
            }

            #region IPersistable Member

            public void Load(IPersistStream stream)
            {
                PersistableClass pClass;
                while ((pClass = (PersistableClass)stream.Load("IClass", null, new PersistableClass(_layers))) != null)
                {
                }
            }

            public void Save(IPersistStream stream)
            {
                foreach (ILayer layer in _layers)
                {
                    if (layer != null && layer.Class is IPersistable)
                    {
                        stream.Save("IClass", new PersistableClass(layer));
                    }
                }
            }

            #endregion
        }
        private class PersistableClass : IPersistable
        {
            private List<ILayer> _layers;
            private ILayer _layer;
            public PersistableClass(ILayer layer)
            {
                _layer = layer;
            }
            public PersistableClass(List<ILayer> layers)
            {
                _layers = layers;
            }

            #region IPersistable Member

            public void Load(IPersistStream stream)
            {
                if (_layers == null) return;

                int layerID = (int)stream.Load("LayerID", -99);
                foreach (ILayer layer in _layers)
                {
                    if (layer != null && layer.ID == layerID &&
                        layer.Class is IPersistable)
                    {
                        stream.Load("Stream", null, layer.Class);
                    }
                }
            }

            public void Save(IPersistStream stream)
            {
                if (_layer == null || !(_layer.Class is IPersistable))
                    return;

                stream.Save("LayerID", _layer.ID);
                stream.Save("Stream", _layer.Class);
            }

            #endregion
        }
        #endregion

        public TOC DataViewTOC
        {
            set
            {
                _dataViewTOC = value;
                if (TOCChanged != null) TOCChanged(this);
            }
        }
        public ITOC PublicTOC
        {
            get { return _toc; }
        }
        public bool drawScaleBar
        {
            get { return _drawScaleBar; }
            set { _drawScaleBar = value; }
        }

        protected virtual void DrawStream(Stream stream)
        {
            if (stream == null || _graphics == null) return;
            try
            {
                stream.Position = 0;
                using (System.Drawing.Image image = System.Drawing.Image.FromStream(stream))
                {
                    _graphics.DrawImage(image, new System.Drawing.Point(0, 0));
                    image.Dispose();
                }
            }
            catch
            {
            }
        }
        protected virtual void StreamImage(ref MemoryStream stream, System.Drawing.Image image)
        {
            try
            {
                if (image == null) return;
                if (stream != null) stream.Dispose();
                stream = new MemoryStream();
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception ex)
            {
            }
        }

        private void MapViewRefreshThread(object canceltracker)
        {
            if (DoRefreshMapView == null) return;

            ICancelTracker cancelTracker = canceltracker as ICancelTracker;
            while (cancelTracker != null ? cancelTracker.Continue : true)
            {
                Thread.Sleep(500);
                DoRefreshMapView();
            }
        }

        #region IClone Member

        public object Clone()
        {
            return new Map(this, false);
        }

        #endregion

        #region IDebugging
        private Exception _lastException = null;
        public Exception LastException
        {
            get
            {
                return _lastException;
            }
            set
            {
                _lastException = value;
            }
        }
        #endregion

        private bool _hasCopyright = false;
        private void CheckDatasetCopyright()
        {
            _hasCopyright = false;
            foreach (IDataset dataset in _datasets)
            {
                if (dataset is IDataCopyright && ((IDataCopyright)dataset).HasDataCopyright)
                {
                    _hasCopyright = true;
                    break;
                }
            }
        }

        #region IDataCopyright Member

        public bool HasDataCopyright
        {
            get { return _hasCopyright; }
        }

        public string DataCopyrightText
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                foreach (IDataset dataset in _datasets)
                {
                    if (dataset is IDataCopyright && ((IDataCopyright)dataset).HasDataCopyright)
                    {
                        sb.Append("<h1>Dataset: " + dataset.DatasetName + "</h1>");
                        sb.Append(((IDataCopyright)dataset).DataCopyrightText + "<hr/>");
                    }
                }

                return sb.ToString();
            }
        }

        #endregion

        #region Exeption Reporting

        private object exceptionLocker = new object();
        public void AddException(Exception ex)
        {
            lock (exceptionLocker)
            {
                if (_requestExceptions == null) _requestExceptions = new List<Exception>();
                _requestExceptions.Add(ex);
            }
        }

        public void AppendExceptionsToImage()
        {
            if (_requestExceptions == null || this.Display.GraphicsContext == null)
                return;

            lock (exceptionLocker)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception ex in _requestExceptions)
                {
                    sb.Append("Exception: " + ex.Message + "\r\n");
                    sb.Append(ex.StackTrace + "\r\n");
                }

                using (System.Drawing.Font font = new System.Drawing.Font("Arial", 12))
                {
                    System.Drawing.SizeF size = this.Display.GraphicsContext.MeasureString(sb.ToString().ToString(), font);
                    int mx = this.Display.iWidth / 2 - (int)size.Width / 2, my = this.Display.iHeight/2 - (int)size.Height / 2;
                    this.Display.GraphicsContext.FillRectangle(System.Drawing.Brushes.LightGray, new System.Drawing.Rectangle(mx - 30, my - 30, (int)size.Width + 60, (int)size.Height + 60));
                    this.Display.GraphicsContext.DrawRectangle(System.Drawing.Pens.Black, new System.Drawing.Rectangle(mx - 30, my - 30, (int)size.Width + 60, (int)size.Height + 60));
                    this.Display.GraphicsContext.DrawString(sb.ToString(), font, System.Drawing.Brushes.Red, mx, my);
                }
            }
        }

        #endregion
    }

    public class Display : MapMetadata, gView.Framework.Carto.IDisplay
    {
        public event MapScaleChangedEvent MapScaleChanged;
        public event RenderOverlayImageEvent RenderOverlayImage;

        #region Variablen
        protected double m_maxX, m_maxY, m_minX, m_minY;
        protected double m_actMinX=-10, m_actMinY=-10, m_actMaxX=10, m_actMaxY=10;
        protected int m_iWidth, m_iHeight, m_fixScaleIndex;
        protected double m_dpm = (96.0 / 0.0254);  // dots per [m]... 96 dpi werden angenommen
        protected double m_scale;
        protected bool m_extentChanged;
        protected double m_fontsizeFactor, m_widthFactor, m_refScale;
        //protected ArrayList m_fixScales;
        internal System.Drawing.Bitmap _image = null;
        internal System.Drawing.Graphics _graphics = null;
        protected ISpatialReference _spatialReference = null;
        protected IGeometricTransformer _geoTransformer = null;
        protected float _OX = 0, _OY = 0;
        protected System.Drawing.Color _backgroundColor = System.Drawing.Color.White;
        private System.Drawing.Color _transColor = System.Drawing.Color.White;
        private bool _makeTransparent = false;
        protected ILabelEngine _labelEngine;
        protected GeoUnits _mapUnits = GeoUnits.Unknown, _displayUnits = GeoUnits.Unknown;
        private object lockThis = new object();
        private DisplayScreen _screen;
        private DisplayTransformation _displayTransformation = new DisplayTransformation();
        #endregion

        public Display()
        {
            m_extentChanged = true;
            m_fontsizeFactor = m_widthFactor = m_refScale = -1.0;
            //m_fixScales=new ArrayList();

            _labelEngine = new LabelEngine2();
            initEnvironment();
        }

        internal Display(bool createLabelEngine)
        {
            m_extentChanged = true;
            m_fontsizeFactor = m_widthFactor = m_refScale = -1.0;
            //m_fixScales = new ArrayList();

            _labelEngine = (createLabelEngine) ? new LabelEngine2() : null;
            initEnvironment();
        }

        private void initEnvironment()
        {
            try
            {
                _screen = new DisplayScreen(
                    SystemVariables.PrimaryScreenDPI / 96f);

                //this.dpi = System.Drawing.Graphics.FromHwndInternal((IntPtr)0).DpiX;
            }
            catch { }
        }
        public void setLimit(double minx, double miny, double maxx, double maxy)
        {
            m_minX = m_actMinX = minx;
            m_maxX = m_actMaxX = maxx;
            m_minY = m_actMinY = miny;
            m_maxY = m_actMaxY = maxy;
            this.ZoomTo(m_actMinX, m_actMinY, m_actMaxX, m_actMaxY);
        }

        public void World2Image(ref double x, ref double y)
        {
            //x=(x-m_actMinX)*m_iWidth/_wWidth;
            //y=m_iHeight-(y-m_actMinY)*m_iHeight/_wHeight;

            x = (x - m_actMinX) * m_iWidth / (m_actMaxX - m_actMinX);
            y = m_iHeight - (y - m_actMinY) * m_iHeight / (m_actMaxY - m_actMinY);

            x += _OX;
            //y -= _OX;
            y += _OY;

            if (_displayTransformation.UseTransformation)
                _displayTransformation.Transform(this, ref x, ref y);
        }

        public void Image2World(ref double x, ref double y)
        {
            if (_displayTransformation.UseTransformation)
                _displayTransformation.InvTransform(this, ref x, ref y);

            x -= _OX;
            y -= _OY;

            x = x * (m_actMaxX - m_actMinX) / m_iWidth + m_actMinX;
            y = (m_iHeight - y) * (m_actMaxY - m_actMinY) / m_iHeight + m_actMinY;
        }

        public System.Drawing.Graphics GraphicsContext
        {
            get { return _graphics; }
            set { _graphics = value; }
        }

        public ISpatialReference SpatialReference
        {
            get { return _spatialReference; }
            set
            {
                _spatialReference = value;
                if (value != null)
                {
                    if (_spatialReference.SpatialParameters.IsGeographic && (int)_mapUnits >= 0)
                    {
                        _displayUnits = _mapUnits = GeoUnits.DecimalDegrees;
                    }
                    else if (!_spatialReference.SpatialParameters.IsGeographic && (int)_mapUnits < 0)
                    {
                        _displayUnits = _mapUnits = GeoUnits.Unknown;
                    }
                }
            }
        }

        public IGeometricTransformer GeometricTransformer
        {
            get { return _geoTransformer; }
            set
            {
                _geoTransformer = value;
            }
        }

        public void Draw(ISymbol symbol, IGeometry geometry)
        {
            try
            {
                IGeometry geom;
                if (_geoTransformer != null)
                {
                    geom = (IGeometry)_geoTransformer.Transform2D(geometry);
                }
                else
                {
                    geom = geometry;
                }

                symbol.Draw(this, geom);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }

        public void DrawOverlay(IGraphicsContainer container, bool clearOld)
        {
            if (RenderOverlayImage == null || _graphics != null) return;
            if (container == null) ClearOverlay();

            System.Drawing.Bitmap bm = null;
            try
            {
                bm = new System.Drawing.Bitmap(iWidth, iHeight);
                bm.MakeTransparent();

                _graphics = System.Drawing.Graphics.FromImage(bm);
                foreach (IGraphicElement element in container.Elements)
                {
                    element.Draw(this);
                }

                RenderOverlayImage(bm, clearOld);
            }
            catch
            {
            }
            finally
            {
                if (_graphics != null) _graphics.Dispose();
                _graphics = null;
                if (bm != null) bm.Dispose();
            }
        }

        public void ClearOverlay()
        {
            RenderOverlayImage(null, true);
        }

        public void ZoomTo(IEnvelope envelope)
        {
            if (envelope == null) return;
            this.ZoomTo(envelope.minx, envelope.miny, envelope.maxx, envelope.maxy);
        }

        private IGraphicsContainer _graphicsContainer = new GraphicsContainer();
        public IGraphicsContainer GraphicsContainer
        {
            get { return _graphicsContainer; }
        }

        #region Eigenschaften
        public int iWidth
        {
            get { return m_iWidth; }
            set { m_iWidth = value; }
        }
        public int iHeight
        {
            get { return m_iHeight; }
            set { m_iHeight = value; }
        }

        public double left
        {
            get { return m_actMinX; }
        }
        public double right
        {
            get { return m_actMaxX; }
        }
        public double bottom
        {
            get { return m_actMinY; }
        }
        public double top
        {
            get { return m_actMaxY; }
        }
        public IEnvelope Limit
        {
            get
            {
                return new Envelope(m_minX, m_minY, m_maxX, m_maxY);
            }
            set
            {
                if (value != null)
                {
                    m_minX = value.minx;
                    m_minY = value.miny;
                    m_maxX = value.maxx;
                    m_maxY = value.maxy;
                }
            }
        }
        public double refScale
        {
            get
            {
                return (_mapUnits == GeoUnits.Unknown) ? 0.0 : m_refScale;
            }
            set
            {
                m_refScale = value;
                if (m_refScale > 0.0)
                {
                    m_widthFactor = m_fontsizeFactor = m_refScale / Math.Max(m_scale, 1.0);
                }
                else
                {
                    m_widthFactor = m_fontsizeFactor = -1;
                }
            }
        }

        public double dpm
        {
            get { return m_dpm; }
        }
        public double dpi
        {
            get { return dpm * 0.0254; }
            set { m_dpm = value / 0.0254; }
        }
        public IEnvelope Envelope
        {
            get
            {
                return new Envelope(m_actMinX, m_actMinY, m_actMaxX, m_actMaxY);
            }
            set
            {
                m_actMinX = value.minx;
                m_actMinY = value.miny;
                m_actMaxX = value.maxx;
                m_actMaxY = value.maxy;
            }
        }
        public System.Drawing.Image Bitmap
        {
            get { return _image; }
        }
        public System.Drawing.Color BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }

        public System.Drawing.Color TransparentColor
        {
            get { return _transColor; }
            set { _transColor = value; }
        }

        public bool MakeTransparent
        {
            get { return _makeTransparent; }
            set { _makeTransparent = value; }
        }
        #endregion

        #region Scale

        private double CalcScale()
        {
            double phi = 0.0;
            if (_mapUnits == GeoUnits.DecimalDegrees)
            {
                phi = Math.Min(90.0, m_actMaxY) * 0.5 + Math.Max(-90.0, m_actMinY) * 0.5;
            }

            double w = Math.Abs(m_actMaxX - m_actMinX);
            double h = Math.Abs(m_actMaxY - m_actMinY);

            GeoUnitConverter converter = new GeoUnitConverter();
            double dpu = converter.Convert(1.0, GeoUnits.Meters, _mapUnits, 1, phi);

            double s1 = iWidth > 0 ? Math.Abs(w) / iWidth * dpm : 1; s1 /= dpu;
            double s2 = iHeight > 0 ? Math.Abs(h) / iHeight * dpm : 1; s2 /= dpu;
            double scale = iWidth > 0 && iHeight > 0 ? Math.Max(s1, s2) : Math.Max(mapScale, 1);

            return scale;

            #region Old
            /*
            double phi1 = 0.0, phi2 = 0.0;
            if (_mapUnits == GeoUnits.DecimalDegrees)
            {
                phi1 = Math.Abs(m_actMinY);
                phi2 = Math.Abs(m_actMaxY);
            }

            double w = Math.Abs(m_actMaxX - m_actMinX);
            double h = Math.Abs(m_actMaxY - m_actMinY);

            GeoUnitConverter converter = new GeoUnitConverter();
            double Wm = converter.Convert(w, _mapUnits, GeoUnits.Meters, 1, Math.Min(phi1, phi2));
            double Hm = converter.Convert(h, _mapUnits, GeoUnits.Meters);

            double s1 = Math.Abs(Wm) / iWidth * dpm;
            double s2 = Math.Abs(Hm) / iHeight * dpm;
            double scale = Math.Max(s1, s2);

            return scale;
            */
            #endregion
        }

        private void CalcExtent(double scale, double cx, double cy)
        {
            double phi = 0.0;
            if (_mapUnits == GeoUnits.DecimalDegrees)
            {
                phi = m_actMaxY * 0.5 + m_actMinY * 0.5;
            }

            GeoUnitConverter converter = new GeoUnitConverter();
            double dpu = converter.Convert(1.0, GeoUnits.Meters, _mapUnits, 1, phi);

            double w = (iWidth / dpm) * scale; w *= dpu;
            double h = (iHeight / dpm) * scale; h *= dpu;

            m_actMinX = cx - w * 0.5;
            m_actMaxX = cx + w * 0.5;
            m_actMinY = cy - h * 0.5;
            m_actMaxY = cy + h * 0.5;

            #region Old
            /*
            double Wm = (iWidth / dpm) * scale;
            double Hm = (iHeight / dpm) * scale;

            double phi = 0.0;
            if (_mapUnits == GeoUnits.DecimalDegrees)
            {
                phi = m_actMaxY * 0.5 + m_actMinY * 0.5;
            }
            GeoUnitConverter converter = new GeoUnitConverter();
            double w = converter.Convert(Wm, GeoUnits.Meters, _mapUnits, 1, phi);
            double h = converter.Convert(Hm, GeoUnits.Meters, _mapUnits);

            m_actMinX = cx - w * 0.5;
            m_actMaxX = cx + w * 0.5;
            m_actMinY = cy - h * 0.5;
            m_actMaxY = cy + h * 0.5;
             * */
            #endregion
        }

        #endregion

        #region Zoom
        public void setScale(double scale, double cx, double cy)
        {
            if (scale == 0.0)
            {
                return;
            }

            CalcExtent(scale, cx, cy);
            m_scale = scale;

            #region old
            /*
			m_scale=scale;
			double w=(iWidth/dpm)*scale;
			double h=(iHeight/dpm)*scale;
			double maxW=m_maxX-m_minX,
				maxH=m_maxY-m_minY;
			double m_old_actMinX=m_actMinX,
				m_old_actMaxX=m_actMaxX,
				m_old_actMinY=m_actMinY,
				m_old_actMaxY=m_actMaxY;

			if(maxW<w && (int)maxW>0) 
			{
				double CX=m_maxX*0.5+m_minX*0.5;
				m_actMinX=CX-w*0.5;
				m_actMaxX=CX+w*0.5;
			} 
			else 
			{
				m_actMinX=cx-w*0.5;
				m_actMaxX=cx+w*0.5;
				if(m_actMinX<m_minX && (int)maxW!=0) { m_actMinX=m_minX; m_actMaxX=m_actMinX+w; }
				if(m_actMaxX>m_maxX && (int)maxW!=0) { m_actMaxX=m_maxX; m_actMinX=m_actMaxX-w; }
			}
			if(maxH<h && (int)maxH>0) 
			{
				double CY=m_maxY*0.5+m_minY*0.5;
				m_actMinY=CY-h*0.5;
				m_actMaxY=CY+h*0.5;
			}
			else
			{
				m_actMinY=cy-h*0.5;
				m_actMaxY=cy+h*0.5;
				if(m_actMinY<m_minY && (int)maxH>0) { m_actMinY=m_minY; m_actMaxY=m_actMinY+h; }
				if(m_actMaxY>m_maxY && (int)maxH>0) { m_actMaxY=m_maxY; m_actMinY=m_actMaxY-h; }
			}
			if( Math.Abs(m_actMinX-m_old_actMinX)>1e-7 ||
				Math.Abs(m_actMinY-m_old_actMinY)>1e-7 ||
				Math.Abs(m_actMaxX-m_old_actMaxX)>1e-7 ||
				Math.Abs(m_actMaxY-m_old_actMaxY)>1e-7)
			{  
				m_extentChanged=true;
			}
			if(refScale>0.0) 
			{
				m_widthFactor=m_fontsizeFactor=m_refScale/Math.Max(m_scale,1.0);
			}
            */
            #endregion

            if (MapScaleChanged != null) MapScaleChanged(this);
        }
        public void setScale(double scale)
        {
            double cx = m_actMaxX * 0.5 + m_actMinX * 0.5;
            double cy = m_actMaxY * 0.5 + m_actMinY * 0.5;
            setScale(scale, cx, cy);
        }

        public double mapScale
        {
            get { return m_scale; }
            set
            {
                setScale(value);
            }
        }

        public void ZoomTo(double minx, double miny, double maxx, double maxy)
        {
            #region AutoResize
            double dx = Math.Abs(maxx - minx), mx = (maxx + minx) / 2.0;
            double dy = Math.Abs(maxy - miny), my = (maxy + miny) / 2.0;

            if (dx < dy)
            {
                double W = Math.Max((double)iWidth, 1) * dy / Math.Max((double)iHeight, 1);
                minx = mx - W / 2.0;
                maxx = mx + W / 2.0;
            }
            else
            {
                double H = Math.Max((double)iHeight, 1) * dx / Math.Max((double)iWidth, 1);
                miny = my - H / 2.0;
                maxy = my + H / 2.0;
            }
            #endregion

            double epsi = double.Epsilon;
            if (Math.Abs(maxx - minx) < epsi)
            {
                minx -= epsi; maxx += epsi;
            }
            if (Math.Abs(maxy - miny) < epsi)
            {
                miny -= epsi; maxy += epsi;
            }
            double cx = maxx * 0.5 + minx * 0.5,
                cy = maxy * 0.5 + miny * 0.5;

            dx = Math.Abs(maxx - minx);
            dy = Math.Abs(maxy - miny);

            m_actMinX = cx - dx / 2.0; m_actMaxX = cx + dx / 2.0;
            m_actMinY = cy - dy / 2.0; m_actMaxY = cy + dy / 2.0;
            m_scale = CalcScale();

            if (m_scale < 1.0)
            {
                this.setScale(1.0);
            }
            else
            {
                if (MapScaleChanged != null) MapScaleChanged(this);
            }


            #region AutoResize old Method
            /*
            double dx = Math.Abs(maxx - minx), mx = (maxx + minx) / 2.0;
            double dy = Math.Abs(maxy - miny), my = (maxy + miny) / 2.0;

            double phi = 0.0;
            if (_mapUnits == GeoUnits.DecimalDegrees)
            {
                phi = Math.Min(90.0, maxy) * 0.5 + Math.Max(-90.0, miny) * 0.5;
            }
            GeoUnitConverter converter = new GeoUnitConverter();
            double dx_ = converter.Convert(dx, _mapUnits, GeoUnits.Meters, 1, phi);
            double dy_ = converter.Convert(dy, _mapUnits, GeoUnits.Meters);

            if (dx_ < dy_)
            {
                double W = (double)iWidth * dy_ / (double)iHeight;
                W = converter.Convert(W, GeoUnits.Meters, _mapUnits, 1, phi);
                minx = mx - W / 2.0;
                maxx = mx + W / 2.0;
            }
            else
            {
                double H = (double)iHeight * dx_ / (double)iWidth;
                H = converter.Convert(H, GeoUnits.Meters, _mapUnits);
                miny = my - H / 2.0;
                maxy = my + H / 2.0;
            }

            ////////////////////////////

            
             * */
            #endregion
        }
        #endregion

        #region Pan
        public void Pan(double px, double py)
        {
            double cx = m_actMaxX * 0.5 + m_actMinX * 0.5;
            double cy = m_actMaxY * 0.5 + m_actMinY * 0.5;
            cx += px;
            cy += py;
            setScale((double)m_scale, cx, cy);
        }
        public void PanW()
        {
            Pan(-Math.Abs(m_actMaxX - m_actMinX) * 0.5, 0.0);
        }
        public void PanE()
        {
            Pan(Math.Abs(m_actMaxX - m_actMinX) * 0.5, 0.0);
        }
        public void PanN()
        {
            Pan(0.0, Math.Abs(m_actMaxY - m_actMinY) * 0.5);
        }
        public void PanS()
        {
            Pan(0.0, -Math.Abs(m_actMaxY - m_actMinY) * 0.5);
        }
        #endregion

        public ILabelEngine LabelEngine { get { return _labelEngine; } }

        public IGeometry Transform(IGeometry geometry, ISpatialReference geometrySpatialReference)
        {
            if (geometry == null) return null;
            if (geometrySpatialReference == null || _spatialReference == null || geometrySpatialReference.Equals(_spatialReference)) return geometry;

            GeometricTransformer transformer = new GeometricTransformer();

            //transformer.FromSpatialReference = geometrySpatialReference;
            //transformer.ToSpatialReference = _spatialReference;
            transformer.SetSpatialReferences(geometrySpatialReference, _spatialReference);

            IGeometry geom = transformer.Transform2D(geometry) as IGeometry;
            transformer.Release();

            return geom;
        }


        public GeoUnits MapUnits
        {
            get
            {
                return _mapUnits;
            }
            set
            {
                _mapUnits = value;
            }
        }

        public GeoUnits DisplayUnits
        {
            get
            {
                return _displayUnits;
            }
            set
            {
                _displayUnits = value;
            }
        }

        virtual public IScreen Screen
        {
            get
            {
                return _screen;
            }
        }

        virtual public IMap Map
        {
            get { return this as IMap; }
        }

        private class DisplayScreen : IScreen
        {
            private float _fac = 1f;

            public DisplayScreen(float fac)
            {
                _fac = fac;
            }
            #region IScreen Member

            public float LargeFontsFactor
            {
                get { return _fac; }
            }

            #endregion
        }

        public IDisplayTransformation DisplayTransformation
        {
            get { return _displayTransformation; }
        }
    }

    public class DisplayTransformation : IDisplayTransformation
    {
        private bool _useTransformation = false;
        private double _cos = 1.0, _sin = 0.0, _rotation = 0.0;

        public DisplayTransformation()
        {
        }

        public bool UseTransformation
        {
            get { return _useTransformation; }
        }

        public double DisplayRotation
        {
            set
            {
                _rotation = value;
                _cos = Math.Cos(_rotation * Math.PI / 180.0);
                _sin = Math.Sin(_rotation * Math.PI / 180.0);

                _useTransformation = (_rotation != 0.0);
            }
            get { return _rotation; }
        }

        public void Transform(IDisplay display, ref double x, ref double y)
        {
            if (display == null || _useTransformation == false)
                return;

            x -= display.iWidth / 2.0;
            y -= display.iHeight / 2.0;

            double x_ = x, y_ = y;

            x = x_ * _cos + y_ * _sin;
            y = -x_ * _sin + y_ * _cos;

            x += display.iWidth / 2.0;
            y += display.iHeight / 2.0;
        }

        public void InvTransform(IDisplay display, ref double x, ref double y)
        {
            if (display == null || _useTransformation == false)
                return;

            x -= display.iWidth / 2.0;
            y -= display.iHeight / 2.0;

            double x_ = x, y_ = y;

            x = x_ * _cos - y_ * _sin;
            y = x_ * _sin + y_ * _cos;

            x += display.iWidth / 2.0;
            y += display.iHeight / 2.0;
        }

        public IEnvelope TransformedBounds(IDisplay display)
        {
            if (display == null)
                return new Envelope();

            if (_useTransformation == false)
                return display.Envelope;

            IEnvelope oBounds = display.Envelope;
            Envelope bounds = new Envelope(display.Envelope);
            bounds.TranslateTo(0.0, 0.0);

            IPointCollection pColl = bounds.ToPointCollection(0);
            for (int i = 0; i < pColl.PointCount; i++)
            {
                IPoint point = pColl[i];
                double x = point.X * _cos + point.Y * _sin;
                double y = -point.X * _sin + point.Y * _cos;
                point.X = x;
                point.Y = y;
            }
            bounds = new Envelope(pColl.Envelope);
            bounds.TranslateTo(oBounds.Center.X, oBounds.Center.Y);
            return bounds;
        }
    }

    internal sealed class ServiceRequestThread
    {
        private Map _map;
        private IWebServiceLayer _layer;
        private int _order = 0;
        private DateTime _startTime, _finishTime;

        public delegate void RequestThreadFinished(ServiceRequestThread sender, bool succeeded, GeorefBitmap image, int order);
        public RequestThreadFinished finish = null;

        public ServiceRequestThread(Map map, IWebServiceLayer layer, int order)
        {
            _map = map;
            _layer = layer;
            _order = order;
        }

        public DateTime StartTime
        {
            get { return _startTime; }
        }
        public DateTime FinishTime
        {
            get { return _finishTime; }
        }
        public IWebServiceLayer WebServiceLayer
        {
            get { return _layer; }
        }
        public void ImageRequest()
        {
            _startTime = DateTime.Now;

            if (_layer == null || _layer.WebServiceClass == null)
            {
                _finishTime = DateTime.Now;
                if (finish != null) finish(this, false, null, 0);
                return;
            }

            if (_layer.WebServiceClass.MapRequest(_map.Display))
            {
                _finishTime = DateTime.Now;
                if (finish != null) finish(this, true, _layer.WebServiceClass.Image, _order);
            }
            else
            {
                _finishTime = DateTime.Now;
                if (finish != null) finish(this, false, null, _order);
            }
        }
    }

    internal sealed class RenderFeatureLayerThread
    {
        private Map _map;
        private IFeatureLayer _layer;
        private ICancelTracker _cancelTracker;
        private static object lockThis = new object();
        private bool _useLabelRenderer = false;
        private FeatureCounter _counter;

        public RenderFeatureLayerThread(Map map, IFeatureLayer layer, ICancelTracker cancelTracker, FeatureCounter counter)
        {
            _map = map;
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

        public void Render()
        {
            if (_layer == null || _map == null) return;

            #region JSON FilterCollection
            if (_layer.FilterQuery != null && !String.IsNullOrEmpty(_layer.FilterQuery.JsonWhereClause))
            {
                try
                {
                    DisplayFilterCollection dfc = DisplayFilterCollection.FromJSON(_layer.FilterQuery.JsonWhereClause);
                    if (dfc == null)
                        return;

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
                                        ((IPenColor)symbol).PenColor = df.Color;
                                    if (symbol is IBrushColor)
                                        ((IBrushColor)symbol).FillColor = df.Color;
                                    if (symbol is IFontColor)
                                        ((IFontColor)symbol).FontColor = df.Color;
                                }
                                if (df.PenWidth > 0f && symbol is IPenWidth)
                                    ((IPenWidth)symbol).PenWidth = df.PenWidth;
                            }
                        }
                        Render(flayer);
                    }
                }
                catch { }
                return;
            }
            #endregion

            Render(_layer);
        }
        private void Render(IFeatureLayer layer)
        {
            try
            {
                if ((
                    layer.FeatureRenderer == null ||
                    layer.FeatureRenderer.HasEffect(layer, _map) == false)
                    &&
                    (
                    layer.LabelRenderer == null ||
                    _useLabelRenderer == false
                    ))
                    return;

                IFeatureClass fClass = layer.FeatureClass;
                if (fClass == null) return;

                //IDataset dataset = (IDataset)_map[layer];
                //if (dataset == null) return;

                //if (!(dataset is IFeatureDataset)) return;

                IGeometry filterGeom = _map.Display.DisplayTransformation.TransformedBounds(_map.Display); //_map.Display.Envelope;

                if (_map.Display.GeometricTransformer != null)
                {
                    filterGeom = MapHelper.Project(fClass, _map.Display);
                }

                gView.Framework.Data.SpatialFilter filter = new gView.Framework.Data.SpatialFilter();
                filter.Geometry = filterGeom;
                filter.AddField(fClass.ShapeFieldName);
                //filter.FuzzyQuery = true;
                filter.SpatialRelation = spatialRelation.SpatialRelationMapEnvelopeIntersects;

                if (layer.FilterQuery != null)
                {
                    filter.WhereClause = layer.FilterQuery.WhereClause;
                    if (layer.FilterQuery is IBufferQueryFilter)
                    {
                        ISpatialFilter sFilter = BufferQueryFilter.ConvertToSpatialFilter(layer.FilterQuery as IBufferQueryFilter);
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
                if (layer.FeatureRenderer != null && layer.FeatureRenderer.HasEffect(layer, _map))
                {
                    layer.FeatureRenderer.PrepareQueryFilter(layer, filter);
                }
                if (layer.LabelRenderer != null && _useLabelRenderer)
                {
                    layer.LabelRenderer.PrepareQueryFilter(_map.Display, layer, filter);
                }

                IDisplay display = (IDisplay)_map;
                double refScale = display.refScale;
                using (IFeatureCursor fCursor = fClass.GetFeatures(MapHelper.MapQueryFilter(filter)))
                {
                    if (fCursor != null)
                    {
                        IFeatureRenderer renderer = null;
                        ILabelRenderer labelRenderer = null;

                        lock (lockThis)
                        {
                            // Beim Clonen sprerren...
                            // Da sonst bei der Servicemap bei gleichzeitigen Requests
                            // Exception "Objekt wird bereits an anderer Stelle verwendet" auftreten kann!
                            if (layer.FeatureRenderer != null && layer.FeatureRenderer.HasEffect(layer, _map))
                            {
                                renderer = (layer.FeatureRenderer.UseReferenceScale && layer.ApplyRefScale) ? (IFeatureRenderer)layer.FeatureRenderer.Clone(display) : layer.FeatureRenderer;
                            }
                            if (layer.LabelRenderer != null && _useLabelRenderer)
                            {
                                if (layer.ApplyLabelRefScale == false && display.refScale > 0)
                                {
                                    //display.refScale = 0;
                                    labelRenderer = (ILabelRenderer)layer.LabelRenderer.Clone(null);
                                    //display.refScale = refScale;
                                }
                                else
                                {
                                    labelRenderer = (ILabelRenderer)layer.LabelRenderer.Clone(display);
                                }
                            }
                        }
                        IFeature feature;

                        if (renderer != null)
                        {
                            while ((feature = fCursor.NextFeature) != null)
                            {
                                if (_cancelTracker != null)
                                    if (!_cancelTracker.Continue)
                                        break;

                                renderer.Draw(_map, feature);
                                if (labelRenderer != null) labelRenderer.Draw(_map, feature);
                                _counter.Counter++;
                            }
                        }
                        else if (labelRenderer != null)
                        {
                            while ((feature = fCursor.NextFeature) != null)
                            {
                                if (_cancelTracker != null)
                                    if (!_cancelTracker.Continue)
                                        break;

                                labelRenderer.Draw(_map, feature);
                                _counter.Counter++;
                            }
                        }
                        if (labelRenderer != null) labelRenderer.Release();
                        if (renderer != null)
                            renderer.FinishDrawing(_map, _cancelTracker);
                        if (renderer != null && layer.FeatureRenderer.UseReferenceScale && layer.ApplyRefScale) renderer.Release();
                    }
                }
            }
            catch (Exception ex)
            {
                if (_map is IServiceMap && ((IServiceMap)_map).MapServer != null)
                {
                    ((IServiceMap)_map).MapServer.Log(
                        "RenderFeatureLayerThread: " + ((layer != null) ? layer.Title : String.Empty),
                        loggingMethod.error,
                        ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
                }
                if (_map != null) _map.AddException(new Exception("RenderFeatureLayerThread: " + ((layer != null) ? layer.Title : String.Empty) + "\n" + ex.Message, ex));
            }
        }
    }

    internal sealed class RenderLabelThread
    {
        private Map _map;
        private IFeatureLayer _layer;
        private ICancelTracker _cancelTracker;
        private static object lockThis = new object();

        public RenderLabelThread(Map map, IFeatureLayer layer, ICancelTracker cancelTracker)
        {
            _map = map;
            _layer = layer;
            _cancelTracker = ((cancelTracker == null) ? new CancelTracker() : cancelTracker);
        }

        public void Render()
        {
            try
            {
                if (_layer == null || _layer.LabelRenderer == null || _map == null) return;

                IFeatureClass fClass = _layer.FeatureClass;
                if (fClass == null) return;

                IGeometry filterGeom = _map.Display.Envelope;

                if (_map.Display.GeometricTransformer != null)
                {
                    filterGeom = MapHelper.Project(fClass, _map.Display);
                    //filterGeom = (IGeometry)_map.Display.GeometricTransformer.InvTransform2D(filterGeom);
                }

                gView.Framework.Data.SpatialFilter filter = new gView.Framework.Data.SpatialFilter();
                filter.Geometry = filterGeom;
                filter.AddField(fClass.ShapeFieldName);
                filter.SpatialRelation = spatialRelation.SpatialRelationMapEnvelopeIntersects;

                if (_layer.FilterQuery != null)
                {
                    filter.WhereClause = _layer.FilterQuery.WhereClause;
                    if (_layer.FilterQuery is ISpatialFilter)
                    {
                        //filter.FuzzyQuery = ((ISpatialFilter)_layer.FilterQuery).FuzzyQuery;
                        filter.SpatialRelation = ((ISpatialFilter)_layer.FilterQuery).SpatialRelation;
                        filter.Geometry = ((ISpatialFilter)_layer.FilterQuery).Geometry;
                    }
                }

                _layer.LabelRenderer.PrepareQueryFilter(_map.Display, _layer, filter);

                using (IFeatureCursor fCursor = fClass.GetFeatures(filter))
                {
                    if (fCursor != null)
                    {
                        ILabelRenderer labelRenderer = null;

                        lock (lockThis)
                        {
                            // Beim Clonen sprerren...
                            // Da sonst bei der Servicemap bei gleichzeitigen Requests
                            // Exception "Objekt wird bereits an anderer Stelle verwendet" auftreten kann!
                            labelRenderer = (ILabelRenderer)_layer.LabelRenderer.Clone((IDisplay)_map);
                        }
                        IFeature feature;

                        while ((feature = fCursor.NextFeature) != null)
                        {
                            if (_cancelTracker != null)
                                if (!_cancelTracker.Continue)
                                    break;

                            labelRenderer.Draw(_map, feature);
                        }

                        labelRenderer.Release();
                    }
                }
            }
            catch (Exception ex)
            {
                if (_map is IServiceMap && ((IServiceMap)_map).MapServer != null)
                {
                    ((IServiceMap)_map).MapServer.Log(
                        "RenderLabelThread:" + ((_layer != null) ? _layer.Title : String.Empty),
                        loggingMethod.error,
                        ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
                }
                if (_map != null)
                    if (_map != null) _map.AddException(new Exception("RenderLabelThread: " + ((_layer != null) ? _layer.Title : String.Empty) + "\n" + ex.Message, ex));
            }
        }
    }

    internal sealed class RenderRasterLayerThread
    {
        protected Map _map;
        private IRasterLayer _layer;
        private ICancelTracker _cancelTracker;
        private InterpolationMethod _interpolMethod = InterpolationMethod.Fast;
        private float _transparency = 0.0f;
        private System.Drawing.Color _transColor = System.Drawing.Color.Transparent;
        static private IRasterLayer _lastRasterLayer = null;

        public RenderRasterLayerThread(Map map, IRasterLayer layer, IRasterLayer rLayer, ICancelTracker cancelTracker)
        {
            _map = map;
            _layer = layer;
            _cancelTracker = cancelTracker;
            if (rLayer != null)
            {
                _interpolMethod = rLayer.InterpolationMethod;
                _transparency = rLayer.Transparency;
                _transColor = rLayer.TransparentColor;
            }
        }

        public void Render()
        {
            try
            {
                if (_layer == null || _map == null || _cancelTracker == null) return;
                if (_layer.RasterClass.Polygon == null) return;

                IEnvelope env = _layer.RasterClass.Polygon.Envelope;
                double minx = env.minx, miny = env.miny, maxx = env.maxx, maxy = env.maxy;
                _map.World2Image(ref minx, ref miny);
                _map.World2Image(ref maxx, ref maxy);
                int iWidth = 0, iHeight = 0;
                int min_x = Math.Max(0, (int)Math.Min(minx, maxx) - 1);
                int min_y = Math.Max(0, (int)Math.Min(miny, maxy) - 1);
                int max_x = Math.Min(iWidth = _map.iWidth, (int)Math.Max(minx, maxx) + 1);
                int max_y = Math.Min(iHeight = _map.iHeight, (int)Math.Max(miny, maxy) + 1);

                // _lastRasterLayer bei ImageServer vermeiden,
                // weil sonst das Bild gelöscht wird bevor es gezeichnet wurde
                // nicht Threadsicher!!!!!!!
                /*
                if (_lastRasterLayer != null && _lastRasterLayer != _layer)
                {
                    _lastRasterLayer.EndPaint();
                }
                */

                _layer.RasterClass.BeginPaint(_map.Display, _cancelTracker);
                if (_layer.RasterClass.Bitmap == null) return;

                //System.Windows.Forms.MessageBox.Show("begin");

                double W = (_map.Envelope.maxx - _map.Envelope.minx);
                double H = (_map.Envelope.maxy - _map.Envelope.miny);
                double MinX = _map.Envelope.minx;
                double MinY = _map.Envelope.miny;

                //_lastRasterLayer = _layer;

                System.Drawing.Graphics gr = _map.Display.GraphicsContext;
                if (gr == null) return;
                gr.InterpolationMode = (System.Drawing.Drawing2D.InterpolationMode)_interpolMethod;

                // Transformation berechnen
                System.Drawing.RectangleF rect;
                switch (gr.InterpolationMode)
                {
                    case System.Drawing.Drawing2D.InterpolationMode.Bilinear:
                    case System.Drawing.Drawing2D.InterpolationMode.Bicubic:
                        rect = new System.Drawing.RectangleF(0, 0, _layer.RasterClass.Bitmap.Width - 1f, _layer.RasterClass.Bitmap.Height - 1f);
                        break;
                    case System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor:
                        rect = new System.Drawing.RectangleF(-0.5f, -0.5f, _layer.RasterClass.Bitmap.Width, _layer.RasterClass.Bitmap.Height);
                        //rect = new System.Drawing.RectangleF(0f, 0f, _layer.RasterClass.Bitmap.Width, _layer.RasterClass.Bitmap.Height);
                        break;
                    default:
                        rect = new System.Drawing.RectangleF(0, 0, _layer.RasterClass.Bitmap.Width, _layer.RasterClass.Bitmap.Height);
                        break;
                }
                System.Drawing.PointF[] points = new System.Drawing.PointF[3];

                if (_layer.RasterClass is IRasterClass2)
                {
                    IPoint p1 = ((IRasterClass2)_layer.RasterClass).PicPoint1;
                    IPoint p2 = ((IRasterClass2)_layer.RasterClass).PicPoint2;
                    IPoint p3 = ((IRasterClass2)_layer.RasterClass).PicPoint3;
                    if (_map.Display.GeometricTransformer != null)
                    {
                        p1 = (IPoint)_map.Display.GeometricTransformer.Transform2D(p1);
                        p2 = (IPoint)_map.Display.GeometricTransformer.Transform2D(p2);
                        p3 = (IPoint)_map.Display.GeometricTransformer.Transform2D(p3);
                    }

                    double X = p1.X, Y = p1.Y;
                    _map.Display.World2Image(ref X, ref Y);
                    points[0] = new System.Drawing.PointF(ToPixelFloat(X), ToPixelFloat(Y));

                    X = p2.X; Y = p2.Y;
                    _map.Display.World2Image(ref X, ref Y);
                    points[1] = new System.Drawing.PointF(ToPixelFloat(X), ToPixelFloat(Y));

                    X = p3.X; Y = p3.Y;
                    _map.Display.World2Image(ref X, ref Y);
                    points[2] = new System.Drawing.PointF(ToPixelFloat(X), ToPixelFloat(Y));

                    RoundGraphicPixelPoints(points);
                }
                else
                {
                    double X1 = _layer.RasterClass.oX - _layer.RasterClass.dx1 / 2.0 - _layer.RasterClass.dy1 / 2.0;
                    double Y1 = _layer.RasterClass.oY - _layer.RasterClass.dx2 / 2.0 - _layer.RasterClass.dy2 / 2.0;
                    double X = X1;
                    double Y = Y1;
                    if (_map.Display.GeometricTransformer != null)
                    {
                        IPoint p = (IPoint)_map.Display.GeometricTransformer.Transform2D(new Point(X, Y));
                        X = p.X; Y = p.Y;
                    }
                    _map.Display.World2Image(ref X, ref Y);
                    points[0] = new System.Drawing.PointF(ToPixelFloat(X), ToPixelFloat(Y));
                    X = X1 + (_layer.RasterClass.Bitmap.Width) * _layer.RasterClass.dx1;
                    Y = Y1 + (_layer.RasterClass.Bitmap.Width) * _layer.RasterClass.dx2;
                    if (_map.Display.GeometricTransformer != null)
                    {
                        IPoint p = (IPoint)_map.Display.GeometricTransformer.Transform2D(new Point(X, Y));
                        X = p.X; Y = p.Y;
                    }
                    _map.Display.World2Image(ref X, ref Y);
                    points[1] = new System.Drawing.PointF(ToPixelFloat(X), ToPixelFloat(Y));
                    X = X1 + (_layer.RasterClass.Bitmap.Height) * _layer.RasterClass.dy1;
                    Y = Y1 + (_layer.RasterClass.Bitmap.Height) * _layer.RasterClass.dy2;
                    if (_map.Display.GeometricTransformer != null)
                    {
                        IPoint p = (IPoint)_map.Display.GeometricTransformer.Transform2D(new Point(X, Y));
                        X = p.X; Y = p.Y;
                    }
                    _map.Display.World2Image(ref X, ref Y);
                    points[2] = new System.Drawing.PointF(ToPixelFloat(X), ToPixelFloat(Y));
                }

                if (_transColor.ToArgb() != System.Drawing.Color.Transparent.ToArgb())
                {
                    try
                    {
                        // kann OutOfMemoryException auslösen...
                        _layer.RasterClass.Bitmap.MakeTransparent(_transColor);
                    }
                    catch (Exception ex)
                    {
                        if (_map is IServiceMap && ((IServiceMap)_map).MapServer != null)
                        {
                            ((IServiceMap)_map).MapServer.Log(
                                "RenderRasterLayerThread: " + ((_layer != null) ? _layer.Title : String.Empty),
                                loggingMethod.error,
                                ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
                        }
                        if (_map != null)
                            if (_map != null) _map.AddException(new Exception("RenderRasterLayerThread: " + ((_layer != null) ? _layer.Title : String.Empty) + "\n" + ex.Message, ex));
                    }
                }

                //var comQual = gr.CompositingQuality;
                //gr.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                float opaque = 1.0f - _transparency;
                if (opaque > 0f && opaque < 1f)
                {
                    float[][] ptsArray ={ 
                            new float[] {1, 0, 0, 0, 0},
                            new float[] {0, 1, 0, 0, 0},
                            new float[] {0, 0, 1, 0, 0},
                            new float[] {0, 0, 0, opaque, 0}, 
                            new float[] {0, 0, 0, 0, 1}};

                    System.Drawing.Imaging.ColorMatrix clrMatrix = new System.Drawing.Imaging.ColorMatrix(ptsArray);
                    System.Drawing.Imaging.ImageAttributes imgAttributes = new System.Drawing.Imaging.ImageAttributes();
                    imgAttributes.SetColorMatrix(clrMatrix,
                                                 System.Drawing.Imaging.ColorMatrixFlag.Default,
                                                 System.Drawing.Imaging.ColorAdjustType.Bitmap);

                    gr.DrawImage(_layer.RasterClass.Bitmap, points, rect, System.Drawing.GraphicsUnit.Pixel, imgAttributes);
                }
                else
                {
                    gr.DrawImage(_layer.RasterClass.Bitmap, points, rect, System.Drawing.GraphicsUnit.Pixel);
                    //using (System.Drawing.Bitmap bm__ = new System.Drawing.Bitmap(100, 100))
                    //using (System.Drawing.Graphics gr__ = System.Drawing.Graphics.FromImage(bm__))
                    //{
                    //    gr__.DrawImage(_layer.RasterClass.Bitmap, new System.Drawing.Point(0, 0),);
                    //}
                } 
            }
            catch (Exception ex)
            {
                if (_map is IServiceMap && ((IServiceMap)_map).MapServer != null)
                {
                    ((IServiceMap)_map).MapServer.Log(
                        "RenderRasterLayerThread:" + ((_layer != null) ? _layer.Title : String.Empty), loggingMethod.error,
                        ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
                }
                if (_map != null)
                    if (_map != null) _map.AddException(new Exception("RenderRasterLayerThread: " + ((_layer != null) ? _layer.Title : String.Empty) + "\n" + ex.Message, ex));
            }
            finally
            {
                _layer.RasterClass.EndPaint(_cancelTracker);
            }
        }

        /*
        public void Render2()
        {
            if (_layer == null || _map == null || _cancelTracker == null) return;
            if (_layer.Polygon == null) return;

            IEnvelope env = _layer.Polygon.Envelope;
            double minx = env.minx, miny = env.miny, maxx = env.maxx, maxy = env.maxy;
            _map.World2Image(ref minx, ref miny);
            _map.World2Image(ref maxx, ref maxy);
            int iWidth = 0, iHeight = 0;
            int min_x = Math.Max(0, (int)Math.Min(minx, maxx) - 1);
            int min_y = Math.Max(0, (int)Math.Min(miny, maxy) - 1);
            int max_x = Math.Min(iWidth = _map.iWidth, (int)Math.Max(minx, maxx) + 1);
            int max_y = Math.Min(iHeight = _map.iHeight, (int)Math.Max(miny, maxy) + 1);

            if (_lastRasterLayer != null && _lastRasterLayer != _layer)
            {
                _lastRasterLayer.EndPaint();
            }
            _layer.BeginPaint();
            //System.Windows.Forms.MessageBox.Show("begin");

            double W = (_map.Envelope.maxx - _map.Envelope.minx);
            double H = (_map.Envelope.maxy - _map.Envelope.miny);
            double MinX = _map.Envelope.minx;
            double MinY = _map.Envelope.miny;

            
            _lastRasterLayer = _layer;

            System.Drawing.Graphics gr = _map.Display.GraphicsContext;
            // Transformation berechnen
            System.Drawing.RectangleF rect = new System.Drawing.Rectangle(
                0, 0, _layer.Bitmap.Width, _layer.Bitmap.Height);
            System.Drawing.PointF[] points = new System.Drawing.PointF[3];

            double X = _layer.oX;
            double Y = _layer.oY;
            _map.Display.World2Image(ref X, ref Y);
            points[0] = new System.Drawing.PointF((float)X, (float)Y);
            X = _layer.oX + rect.Width * _layer.dx1;
            Y = _layer.oY + rect.Width * _layer.dx2;
            _map.Display.World2Image(ref X, ref Y);
            points[1] = new System.Drawing.PointF((float)X, (float)Y);
            X = _layer.oX + rect.Height * _layer.dy1;
            Y = _layer.oY + rect.Height * _layer.dy2;
            _map.Display.World2Image(ref X, ref Y);
            points[2] = new System.Drawing.PointF((float)X, (float)Y);

            gr.ResetTransform();
            gr.Transform = new System.Drawing.Drawing2D.Matrix(rect, points);
            gr.DrawImage(
                _layer.Bitmap, rect, rect, System.Drawing.GraphicsUnit.Pixel);

            gr.ResetTransform();

            //_layer.EndPaint();
        }
    */

        private float ToPixelFloat(double d)
        {
            return (float)d;
            //return (float)Math.Round(d, 2);
        }

        private void RoundGraphicPixelPoints(System.Drawing.PointF[] points)
        {
            float espsi = .1f;
            if(points.Length==3)
            {
                points[0].X -= espsi;
                points[0].Y -= espsi;

                points[1].X += espsi;
                points[1].Y -= espsi;

                points[2].X -= espsi;
                points[2].Y += espsi;
            }
        }
    }

    internal sealed class RenderFeatureLayerSelectionThread
    {
        private Map _map;
        private IFeatureLayer _layer;
        private ICancelTracker _cancelTracker;

        public RenderFeatureLayerSelectionThread(Map map, IFeatureLayer layer, ICancelTracker cancelTracker)
        {
            _map = map;
            _layer = layer;
            _cancelTracker = ((cancelTracker == null) ? new CancelTracker() : cancelTracker);
        }

        public void Render()
        {
            if (_layer == null) return;
            if (_layer.SelectionRenderer == null) return;
            if (!(_layer is IFeatureSelection)) return;

            IFeatureClass fClass = _layer.FeatureClass;
            if (fClass == null) return;

            ISelectionSet selectionSet = ((IFeatureSelection)_layer).SelectionSet;
            if (selectionSet == null) return;

            IDataset dataset = (IDataset)_map[_layer];
            if (dataset == null) return;

            if (!(dataset is IFeatureDataset)) return;

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
                return;

            filter.AddField(fClass.ShapeFieldName);
            _layer.SelectionRenderer.PrepareQueryFilter(_layer, filter);

            using (IFeatureCursor fCursor = (fClass is ISelectionCache) ? ((ISelectionCache)fClass).GetSelectedFeatures(_map.Display) : fClass.GetFeatures(filter))
            {
                if (fCursor != null)
                {
                    //_layer.SelectionRenderer.Draw(_map, fCursor, DrawPhase.Geography, _cancelTracker);
                    IFeature feature;
                    while ((feature = fCursor.NextFeature) != null)
                    {
                        if (_cancelTracker != null)
                            if (!_cancelTracker.Continue)
                                break;

                        _layer.SelectionRenderer.Draw(_map, feature);
                    }
                    fCursor.Dispose();
                }
            }
        }

    }

    internal sealed class RenderLabelEngineThread
    {
        private IDisplay _display;
        private ICancelTracker _cancelTracker;
        private ILabelEngine _labelEngine;

        public RenderLabelEngineThread(IDisplay display, ILabelEngine labelEngine, ICancelTracker cancelTracker)
        {
            _display = display;
            _cancelTracker = cancelTracker;
            _labelEngine = labelEngine;
        }

        public void Render()
        {
            if (_display == null || _labelEngine == null) return;

            _labelEngine.Draw(_display, _cancelTracker);
            _labelEngine.Release();
        }
    }

    internal class MapHelper
    {
        static public IEnvelope Project3(IFeatureClass fc, IDisplay display)
        {
            if (fc == null || display == null) return null;

            if (display.GeometricTransformer == null) return display.Envelope;

            //Feature feature=new Feature();
            //feature.Shape=fClass.Envelope;
            //_layer.FeatureRenderer.Draw(_map.Display, feature);

            IEnvelope classEnvelope = ((IGeometry)display.GeometricTransformer.Transform2D(fc.Envelope)).Envelope;

            classEnvelope.minx = Math.Max(classEnvelope.minx, display.Envelope.minx);
            classEnvelope.miny = Math.Max(classEnvelope.miny, display.Envelope.miny);
            classEnvelope.maxx = Math.Min(classEnvelope.maxx, display.Envelope.maxx);
            classEnvelope.maxy = Math.Min(classEnvelope.maxy, display.Envelope.maxy);

            IEnvelope filterGeom = ((IGeometry)display.GeometricTransformer.InvTransform2D(classEnvelope)).Envelope;
            //feature = new Feature();
            //feature.Shape = filterGeom;
            //_layer.FeatureRenderer.Draw(_map.Display, feature);

            return filterGeom;
        }
        static public IEnvelope Project(IFeatureClass fc, IDisplay display)
        {
            if (fc == null || display == null) return null;
            if (display.GeometricTransformer == null) return display.Envelope;

            IPointCollection pColl = display.Envelope.ToPointCollection(0);

            IPointCollection pColl2 = (IPointCollection)display.GeometricTransformer.InvTransform2D(pColl);
            IPointCollection pColl3 = (IPointCollection)display.GeometricTransformer.Transform2D(pColl2);

            double epsi = 0.0;
            if (display.SpatialReference.SpatialParameters.IsGeographic)
            {
                // ???
                epsi = Math.Max(display.Envelope.Width, display.Envelope.Height) / 1e2;
            }
            else
            {
                // ???
                epsi = Math.Max(display.Envelope.Width, display.Envelope.Height) / 1e3;
            }
            if (!((PointCollection)pColl).Equals(pColl3, epsi))
                return null;
            else
                return pColl2.Envelope;
        }
        static public IEnvelope Project2(IFeatureClass fc, IDisplay display)
        {
            if (display == null) return null;
            if (display.GeometricTransformer == null) return display.Envelope;

            IPointCollection pColl = display.Envelope.ToPointCollection(100);

            pColl = (IPointCollection)display.GeometricTransformer.InvTransform2D(pColl);

            //MultiPoint mPoint = new MultiPoint(pColl);
            //ISymbol pSymbol = PlugInManager.Create(KnownObjects.Symbology_SimplePointSymbol) as ISymbol;
            //display.Draw(pSymbol, mPoint);

            return pColl.Envelope;
        }

        static public IQueryFilter MapQueryFilter(IQueryFilter filter)
        {
            if (filter is ISpatialFilter)
            {
                if (((ISpatialFilter)filter).Geometry == null)
                {
                    return new QueryFilter(filter);
                }
            }
            return filter;
        }
    }

    internal class FeatureCounter
    {
        public int Counter;
    }
}
