using gView.Framework.Cartography.UI;
using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.UI;
using gView.Framework.Data;
using gView.Framework.Data.Extensions;
using gView.Framework.Geometry;
using gView.Framework.IO;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.Cartography
{
    /// <summary>
    /// Zusammenfassung für Map.
    /// </summary>
    public class Map : Display, IMap, IPersistableLoadAsync, IMetadata, IDebugging, IRefreshSequences
    {
        public const int MapDescriptionId = -1;
        public const int MapCopyrightTextId = -1;

        public virtual event LayerAddedEvent LayerAdded;
        public virtual event LayerRemovedEvent LayerRemoved;
        public virtual event TOCChangedEvent TOCChanged;
        public event EventHandler MapRenamed;
        public event UserIntefaceEvent OnUserInterface;

        public ImageMerger2 m_imageMerger;
        public Toc _toc;
        private Toc _dataViewTOC;

        private SelectionEnvironment _selectionEnv;
        //protected MapDB.mapDB m_mapDB=null;
        protected string m_mapName = "", m_name/*,m_imageName="",m_origImageName=""*/;
        protected int m_mapID = -1;
        protected MapTools m_mapTool = MapTools.ZoomIn;
        protected ArrayList m_activeLayerNames;
        protected bool _drawScaleBar = false;
        public List<IDataset> _datasets;
        public List<ILayer> _layers = new List<ILayer>();
        public bool _debug = true;
        private ConcurrentBag<string> _errorMessages = new ConcurrentBag<string>();

        private IntegerSequence _layerIDSequece = new IntegerSequence();
        private IResourceContainer _resourceContainer = new ResourceContainer();
        private IMapEventHooks _eventHooks = new MapEventHooks();

        public Map()
            : base(null)
        {
            _datasets = new List<IDataset>();

            m_imageMerger = new ImageMerger2();

            m_activeLayerNames = new ArrayList();
            m_name = "Map1";
            _toc = new Toc(this);
            _selectionEnv = new SelectionEnvironment();

            ReferenceScale = 500;

            MapServiceProperties = new MapServiceProperties();
        }

        public Map(Map original, bool writeNamespace)
            : this()
        {
            m_name = original.Name;
            Display.MapUnits = original.Display.MapUnits;
            Display.DisplayUnits = original.Display.DisplayUnits;
            Display.DisplayTransformation.DisplayRotation = original.Display.DisplayTransformation.DisplayRotation;
            Display.BackgroundColor = original.Display.BackgroundColor;
            ReferenceScale = original.Display.ReferenceScale;
            Display.SpatialReference = original.Display.SpatialReference != null ? original.SpatialReference.Clone() as ISpatialReference : null;
            LayerDefaultSpatialReference = original.LayerDefaultSpatialReference != null ? original.LayerDefaultSpatialReference.Clone() as ISpatialReference : null;
            WebMercatorScaleBehavoir = original.WebMercatorScaleBehavoir;

            _toc = new Toc(this); //original.TOC.Clone() as TOC;

            _datasets = ListOperations<IDataset>.Clone(original._datasets);
            _layers = new List<ILayer>();

            Title = original.Title;
            _layerDescriptions = original._layerDescriptions;
            _layerCopyrightTexts = original._layerCopyrightTexts;

            MapServiceProperties = 
                original.MapServiceProperties?.Clone()
                ?? new MapServiceProperties();

            SetResourceContainer(original.ResourceContainer);
            SetMapEventHooks(original.MapEventHooks);

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

        public void Append(Map map, bool writeNamespace)
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
                        if (wClass == null)
                        {
                            continue;
                        }

                        layer = LayerFactory.Create(wClass, element as ILayer);
                        layer.ID = _layerIDSequece.Number;

                        ITocElement tocElement = _toc.GetTOCElement(element as ILayer);
                        if (tocElement != null)
                        {
                            tocElement.RemoveLayer(element as ILayer);
                            tocElement.AddLayer(layer);
                        }

                        if (writeNamespace)
                        {
                            layer.Namespace = map.Name;
                        }

                        AddLayer(layer);

                        foreach (IWebServiceTheme theme in wClass.Themes)
                        {
                            theme.ID = _layerIDSequece.Number;
                            if (writeNamespace)
                            {
                                theme.Namespace = map.Name;
                            }
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
                        {
                            layer = LayerFactory.Create(element.Class, element as ILayer);
                        }

                        layer.ID = _layerIDSequece.Number;

                        ITocElement tocElement = _toc.GetTOCElement(element as ILayer);
                        if (tocElement != null)
                        {
                            tocElement.RemoveLayer(element as ILayer);
                            tocElement.AddLayer(layer);
                        }
                        //_layers.Add(layer);


                        if (writeNamespace)
                        {
                            layer.Namespace = map.Name;
                        }

                        AddLayer(layer);

                        if (layer is IGroupLayer && element is IGroupLayer)
                        {
                            groupLayers.Add(element as IGroupLayer, layer as IGroupLayer);
                        }
                    }

                    if (layer == null)
                    {
                        continue;
                    }

                    ITocElement newTocElement = _toc.GetTOCElement(layer);
                    ITocElement oriTocElement = map.TOC.GetTOCElement(element as ILayer);
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
                                ITocElement newGroupElement = _toc.GetTOCElement(newGroupLayer);
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
            if (m_imageMerger != null)
            {
                m_imageMerger.Dispose();
                m_imageMerger = null;
            }
        }

        public void Release()
        {
            Dispose();
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
                    {
                        MapRenamed(this, new EventArgs());
                    }
                }
            }
        }

        public string Title { get; set; }

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



        #region getLayer

        private int m_datasetNr, m_layerNr;
        private void resetGetLayer()
        {
            m_datasetNr = m_layerNr = 0;
        }
        async private Task<IDatasetElement> getNextLayer(string layername)
        {
            while (this[m_datasetNr] != null)
            {
                IFeatureDataset fDataset = this[m_datasetNr] as IFeatureDataset;

                if (fDataset != null)
                {
                    for (int i = m_layerNr; i < (await fDataset.Elements()).Count; i++)
                    {
                        m_layerNr++;
                        IDatasetElement layer = (await fDataset.Elements())[i];
                        string name = layer.Title;

                        if (layername == name)
                        {
                            return layer;
                        }
                    }
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
                if (!(layer is IFeatureLayer))
                {
                    continue;
                }

                if (!layer.LabelInScale(this))
                {
                    continue;
                }

                IFeatureLayer fLayer = (IFeatureLayer)layer;
                if (fLayer.LabelRenderer == null)
                {
                    continue;
                }

                if (fLayer.LabelRenderer.RenderMode == LabelRenderMode.RenderWithFeature)
                {
                    continue;
                }

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
                if (x == null || y == null || x == y)
                {
                    return 0;
                }

                IPriority p1 = x.LabelRenderer as IPriority;
                IPriority p2 = y.LabelRenderer as IPriority;
                if (p1 == null || p2 == null)
                {
                    return 0;
                }

                if (p1.Priority < p2.Priority)
                {
                    return -1;
                }
                else if (p1.Priority > p2.Priority)
                {
                    return 1;
                }

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
                if (datasetIndex < 0 || datasetIndex >= _datasets.Count)
                {
                    return null;
                }

                return _datasets[datasetIndex];
            }
        }

        public IDataset this[IDatasetElement layer]
        {
            get
            {
                if (layer is ServiceFeatureLayer)
                {
                    layer = ((ServiceFeatureLayer)layer).FeatureLayer;
                }

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
                {
                    return new List<IDataset>();
                }

                return new List<IDataset>(_datasets);
            }
        }

        public void Compress()
        {
            #region remove unused dataset

            var datasetIds = _layers.Where(l => l.Class != null).Select(l => l.DatasetID).Distinct().OrderBy(id => id).ToArray();

            if (datasetIds.Length == 0)
            {
                return;
            }

            for (var datasetId = 0; datasetId < datasetIds.Max(); datasetId++)
            {
                if (!datasetIds.Contains(datasetId))
                {
                    _datasets.RemoveAt(datasetId);
                    foreach (var datasetElement in _layers.Where(l => l.DatasetID > datasetId))
                    {
                        datasetElement.DatasetID -= 1;
                    }
                    Compress();
                    return;
                }
            }

            #endregion

            #region remove double datasets

            for (int datasetId = 0; datasetId < _datasets.Count() - 1; datasetId++)
            {
                var dataset = _datasets[datasetId];

                for (int candidateId = datasetId + 1; candidateId < _datasets.Count(); candidateId++)
                {
                    var candidate = _datasets[candidateId];

                    if (dataset.GetType().ToString() == candidate.GetType().ToString() && dataset.ConnectionString == candidate.ConnectionString)
                    {
                        foreach (var datasetElement in _layers.Where(l => l.DatasetID == candidateId))
                        {
                            datasetElement.DatasetID = datasetId;
                        }

                        _datasets.RemoveAt(candidateId);
                        foreach (var datasetElement in _layers.Where(l => l.DatasetID > candidateId))
                        {
                            datasetElement.DatasetID -= 1;
                        }

                        Compress();
                    }
                }
            }

            #endregion

            foreach (var removeLayer in _toc.Layers.Where(l => l.Class == null).ToArray())
            {
                _toc.RemoveLayer(removeLayer);
            }

            var newLayers = new List<ILayer>();

            foreach (ILayer layer in _layers)
            {
                if (layer is IGroupLayer)
                {
                    if (((GroupLayer)layer).ChildLayers != null && ((GroupLayer)layer).ChildLayers.Count > 0)
                    {
                        newLayers.Add(layer);
                    }
                }
                else /*if (layer is IRasterCatalogLayer)*/
                {
                    if (layer.Class != null)
                    {
                        newLayers.Add(layer);
                    }
                }
                //else if (layer is IRasterLayer)
                //{
                //    if (layer.Class != null)
                //    {
                //        newLayers.Add(layer);
                //    }
                //}
                //else if (layer is IWebServiceLayer)
                //{
                //    if (layer.Class != null)
                //    {
                //        newLayers.Add(layer);
                //    }
                //}
                //else if (layer is IFeatureLayer)
                //{
                //    if (layer.Class != null)
                //    {
                //        newLayers.Add(layer);
                //    }
                //}
            }

            _layers = newLayers; //_layers.Where(l => l.Class != null).ToList();
        }

        public void RemoveDataset(IDataset dataset)
        {
            int index = _datasets.IndexOf(dataset);
            if (index == -1)
            {
                return;
            }

            foreach (ILayer layer in ListOperations<ILayer>.Clone(_layers))
            {
                if (layer.DatasetID == index)
                {
                    RemoveLayer(layer, false);
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
            if (_toc != null)
            {
                _toc.RemoveAllElements();
            }

            _layers.Clear();
        }

        public string ActiveLayerNames
        {
            get
            {
                string names = "";
                foreach (string activeLayerName in m_activeLayerNames)
                {
                    if (names != "")
                    {
                        names += ";";
                    }

                    names += activeLayerName;
                }
                return names;
            }
            set
            {
                m_activeLayerNames = new ArrayList();
                foreach (string activeLayerName in value.Split(';'))
                {
                    if (m_activeLayerNames.IndexOf(activeLayerName) != -1)
                    {
                        continue;
                    }

                    m_activeLayerNames.Add(activeLayerName);
                }
            }
        }

        public void SetNewLayerID(ILayer layer)
        {
            if (layer == null)
            {
                return;
            }

            while (LayerIDExists(layer.ID))
            {
                layer.ID = _layerIDSequece.Number;
            }
        }

        public void AddLayer(ILayer layer)
        {
            AddLayer(layer, -1);
        }
        public void AddLayer(ILayer layer, int pos)
        {
            if (layer == null)
            {
                return;
            }

            SetNewLayerID(layer);

            _layers.Add(layer);

            if (pos < 0)
            {
                _toc.AddLayer(layer, null);
            }
            else
            {
                _toc.AddLayer(layer, null, pos);
            }

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
                        dataset.ConnectionString == layer.Class.Dataset.ConnectionString &&
                        dataset.GetType().Equals(layer.Class.Dataset.GetType()))
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
                TocElement parent = _toc.GetTOCElement(layer.GroupLayer) as TocElement;
                if (parent != null)
                {
                    TocElement tocElement = _toc.GetTOCElement(layer) as TocElement;
                    if (tocElement != null)
                    {
                        tocElement.ParentGroup = parent;
                    }
                }
                else if (layer is Layer)
                {
                    ((Layer)layer).GroupLayer = null;
                }
            }

            CheckDatasetCopyright();

            if (LayerAdded != null)
            {
                LayerAdded(this, layer);
            }
        }

        public void ReplaceLayer(ILayer oldLayer, ILayer newLayer)
        {
            if (oldLayer.ID != newLayer.ID)
            {
                throw new Exception("Can't replace layers with differnt Ids");
            }

            if (oldLayer.DatasetID != newLayer.DatasetID)
            {
                throw new Exception("Can't replace layer with differnt Dataset-Ids.");
            }

            int index = _layers.IndexOf(oldLayer);
            if (index < 0)
            {
                throw new Exception("Layer is not part of map");
            }
            _layers.Remove(oldLayer);
            _layers.Insert(index, newLayer);

            if (_toc is not null)
            {
                ITocElement tocElement = _toc.GetTocElementByLayerId(oldLayer.ID);
                if (tocElement is not null)
                {
                    //var tocIndex = tocElement.Layers.IndexOf(oldLayer);
                    //tocElement.Layers.Remove(oldLayer);
                    //tocElement.Layers.Insert(Math.Max(0, tocIndex), newLayer);
                    tocElement.RemoveLayer(oldLayer);
                    tocElement.AddLayer(newLayer);

                    if (newLayer is IGroupLayer newGroupLayer)
                    {
                        _toc.Elements.Where(t => t.ParentGroup == tocElement)
                             .ToList()
                             .ForEach(t =>
                             {
                                 foreach (ILayer layer in t.Layers?.Where(l => l is Layer && l.GroupLayer == oldLayer) ?? [])
                                 {
                                     ((Layer)layer).GroupLayer = newGroupLayer;
                                 }
                             });
                    }
                }
            }
        }

        public void RemoveLayer(ILayer layer)
        {
            RemoveLayer(layer, true);
        }
        private void RemoveLayer(ILayer layer, bool removeUnusedDataset)
        {
            if (layer == null)
            {
                return;
            }

            IDataset layerDataset = this[layer];

            if (layer is IGroupLayer)
            {
                foreach (ILayer cLayer in ((IGroupLayer)layer).ChildLayers.ToArray())
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

            // also remove von grouplayers
            foreach(var groupLayer in _layers.Where(l => l is IGroupLayer)
                                             .Select(l => (IGroupLayer)l))
            {
                groupLayer.TryRemoveLayer(layer);
            }

            if (LayerRemoved != null)
            {
                LayerRemoved(this, layer);
            }

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
                    RemoveDataset(layerDataset);
                }

                CheckDatasetCopyright();
            }
        }

        async public Task<List<IDatasetElement>> ActiveLayers()
        {
            List<IDatasetElement> e = new List<IDatasetElement>();

            foreach (string activeLayerName in m_activeLayerNames)
            {
                resetGetLayer();
                IDatasetElement layer = await getNextLayer(activeLayerName);
                while (layer != null)
                {
                    e.Add(layer);
                    layer = await getNextLayer(activeLayerName);
                }
            }
            return e;
        }

        async public Task<List<IDatasetElement>> Elements(string aliasname)
        {
            List<IDatasetElement> e = new List<IDatasetElement>();

            resetGetLayer();
            IDatasetElement layer = await getNextLayer(aliasname);
            while (layer != null)
            {
                e.Add(layer);
                layer = await getNextLayer(aliasname);
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
                if (element == null)
                {
                    continue;
                }

                if (element.Class == cls)
                {
                    return element;
                }
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
                        if (!(theme is IFeatureSelection))
                        {
                            continue;
                        }

                        ISelectionSet themeSelSet = ((IFeatureSelection)theme).SelectionSet;
                        if (themeSelSet == null)
                        {
                            continue;
                        }

                        if (themeSelSet.Count > 0)
                        {
                            ((IFeatureSelection)theme).ClearSelection();
                            ((IFeatureSelection)theme).FireSelectionChangedEvent();
                        }
                    }
                }

                if (!(layer is IFeatureSelection))
                {
                    continue;
                }

                ISelectionSet selSet = ((IFeatureSelection)layer).SelectionSet;
                if (selSet == null)
                {
                    continue;
                }

                if (selSet.Count > 0)
                {
                    ((IFeatureSelection)layer).ClearSelection();
                    ((IFeatureSelection)layer).FireSelectionChangedEvent();
                }
            }
        }

        public IToc TOC
        {
            get
            {
                if (_dataViewTOC != null)
                {
                    return _dataViewTOC;
                }

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
            if (_layers == null)
            {
                return false;
            }

            foreach (ILayer layer in _layers)
            {
                if (layer.ID == layerID)
                {
                    return true;
                }
            }
            return false;
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
            if (SpatialReference == null)
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
            {
                layerSR = LayerDefaultSpatialReference;
            }

            if (layerSR == null)
            {
                Display.GeometricTransformer = null;
                return;
            }

            if (SpatialReference.Equals(layerSR))
            {
                Display.GeometricTransformer = null;
                return;
            }

            geotransformer.SetSpatialReferences(layerSR, SpatialReference);
            Display.GeometricTransformer = geotransformer;
        }

        #region IPersistable Member

        public string PersistID
        {
            get
            {
                return null;
            }
        }

        async public Task<bool> LoadAsync(IPersistStream stream)
        {
            m_name = (string)stream.Load("name", "");
            _minX = (double)stream.Load("minx", 0.0);
            _minY = (double)stream.Load("miny", 0.0);
            _maxX = (double)stream.Load("maxx", 0.0);
            _maxY = (double)stream.Load("maxy", 0.0);

            Title = (string)stream.Load("title", string.Empty);

            _actMinX = (double)stream.Load("act_minx", 0.0);
            _actMinY = (double)stream.Load("act_miny", 0.0);
            _actMaxX = (double)stream.Load("act_maxx", 0.0);
            _actMaxY = (double)stream.Load("act_maxy", 0.0);

            _refScale = (double)stream.Load("refScale", 0.0);

            _imageWidth = (int)stream.Load("iwidth", 1);
            _imageHeight = (int)stream.Load("iheight", 1);

            _backgroundColor = GraphicsEngine.ArgbColor.FromArgb(
                (int)stream.Load("background", GraphicsEngine.ArgbColor.White.ToArgb()));

            _mapUnits = (GeoUnits)stream.Load("MapUnits", 0);
            _displayUnits = (GeoUnits)stream.Load("DisplayUnits", 0);

            ISpatialReference sRef = new SpatialReference();
            SpatialReference = (ISpatialReference)stream.Load("SpatialReference", null, sRef);
            //LayerDefaultSpatialReference
            ISpatialReference ldsRef = new SpatialReference();
            LayerDefaultSpatialReference = (ISpatialReference)stream.Load("LayerDefaultSpatialReference", null, ldsRef);
            WebMercatorScaleBehavoir = (WebMercatorScaleBehavoir)stream.Load("WebMercatorScaleBehavoir", (int)WebMercatorScaleBehavoir.Default);

            _layerIDSequece = (IntegerSequence)stream.Load("layerIDSequence", new IntegerSequence(), new IntegerSequence());

            var resources = (IResourceContainer)stream.Load("MapResources", null, new ResourceContainer());
            if (resources != null)
            {
                _resourceContainer = resources;
            }

            var eventHooks = (IMapEventHooks)stream.Load("MapEventHooks", null, new MapEventHooks());
            if (eventHooks != null)
            {
                _eventHooks = eventHooks;
            }

            IDataset dataset;
            while ((dataset = await stream.LoadPluginAsync<IDataset>("IDataset", new UnknownDataset())) != null)
            {
                try
                {
                    if (dataset.State != DatasetState.opened)
                    {
                        await dataset.Open();
                    }
                    if (!string.IsNullOrWhiteSpace(dataset.LastErrorMessage))
                    {
                        _errorMessages.Add(dataset.LastErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    _errorMessages.Add(ex.Message);
                }

                _datasets.Add(dataset);
            }

            GroupLayer gLayer;
            while ((gLayer = (GroupLayer)stream.Load("IGroupLayer", null, new GroupLayer())) != null)
            {
                while (LayerIDExists(gLayer.ID))
                {
                    gLayer.ID = _layerIDSequece.Number;
                }

                _layers.Add(gLayer);
            }

            FeatureLayer fLayer;
            while ((fLayer = (FeatureLayer)stream.Load("IFeatureLayer", null, new FeatureLayer())) != null)
            {
                string errorMessage = string.Empty;
                if (fLayer.DatasetID < _datasets.Count)
                {
                    if (!string.IsNullOrEmpty(_datasets[fLayer.DatasetID]?.LastErrorMessage))
                    {
                        continue;
                    }

                    IDatasetElement element = await _datasets[fLayer.DatasetID].Element(fLayer.Title);
                    if (element != null && element.Class is IFeatureClass)
                    {
                        fLayer = LayerFactory.Create(element.Class, fLayer) as FeatureLayer;
                        //fLayer.SetFeatureClass(element.Class as IFeatureClass);
                    }
                    errorMessage = _datasets[fLayer.DatasetID].LastErrorMessage;
                }
                else
                {
                    //fLayer.DatasetID = -1;
                }
                while (LayerIDExists(fLayer.ID))
                {
                    fLayer.ID = _layerIDSequece.Number;
                }

                if (fLayer.Class == null)
                {
                    _errorMessages.Add("Invalid layer: " + fLayer.Title + "\n" + errorMessage);
                }

                _layers.Add(fLayer);

                if (LayerAdded != null)
                {
                    LayerAdded(this, fLayer);
                }
            }

            RasterCatalogLayer rcLayer;
            while ((rcLayer = (RasterCatalogLayer)stream.Load("IRasterCatalogLayer", null, new RasterCatalogLayer())) != null)
            {
                string errorMessage = string.Empty;
                if (rcLayer.DatasetID < _datasets.Count)
                {
                    if (!string.IsNullOrEmpty(_datasets[rcLayer.DatasetID]?.LastErrorMessage))
                    {
                        continue;
                    }

                    IDatasetElement element = await _datasets[rcLayer.DatasetID].Element(rcLayer.Title);
                    if (element != null && element.Class is IRasterCatalogClass)
                    {
                        rcLayer = LayerFactory.Create(element.Class, rcLayer) as RasterCatalogLayer;
                    }
                    errorMessage = _datasets[rcLayer.DatasetID].LastErrorMessage;
                }
                else
                {
                }
                SetNewLayerID(rcLayer);

                if (rcLayer.Class == null)
                {
                    _errorMessages.Add("Invalid layer: " + rcLayer.Title + "\n" + errorMessage);
                }

                _layers.Add(rcLayer);
                if (LayerAdded != null)
                {
                    LayerAdded(this, rcLayer);
                }
            }

            RasterLayer rLayer;
            while ((rLayer = (RasterLayer)stream.Load("IRasterLayer", null, new RasterLayer())) != null)
            {
                string errorMessage = string.Empty;
                if (rLayer.DatasetID < _datasets.Count)
                {
                    if (!string.IsNullOrEmpty(_datasets[rLayer.DatasetID]?.LastErrorMessage))
                    {
                        continue;
                    }

                    IDatasetElement element = await _datasets[rLayer.DatasetID].Element(rLayer.Title);
                    if (element != null && element.Class is IRasterClass)
                    {
                        rLayer.SetRasterClass(element.Class as IRasterClass);
                    }
                    errorMessage = _datasets[rLayer.DatasetID].LastErrorMessage;
                }
                else
                {
                }
                while (LayerIDExists(rLayer.ID))
                {
                    rLayer.ID = _layerIDSequece.Number;
                }

                if (rLayer.Class == null)
                {
                    _errorMessages.Add("Invalid layer: " + rLayer.Title + "\n" + errorMessage);
                }

                _layers.Add(rLayer);

                if (LayerAdded != null)
                {
                    LayerAdded(this, rLayer);
                }
            }

            WebServiceLayer wLayer;
            while ((wLayer = (WebServiceLayer)stream.Load("IWebServiceLayer", null, new WebServiceLayer())) != null)
            {
                string errorMessage = string.Empty;
                if (wLayer.DatasetID <= _datasets.Count)
                {
                    if (!string.IsNullOrEmpty(_datasets[wLayer.DatasetID]?.LastErrorMessage))
                    {
                        continue;
                    }

                    IDatasetElement element = await _datasets[wLayer.DatasetID].Element(wLayer.Title);
                    if (element != null && element.Class is IWebServiceClass)
                    {
                        //wLayer = LayerFactory.Create(element.Class, wLayer) as WebServiceLayer;
                        wLayer.SetWebServiceClass(element.Class as IWebServiceClass);
                    }
                    errorMessage = _datasets[fLayer.DatasetID].LastErrorMessage;
                }
                else
                {
                }
                while (LayerIDExists(wLayer.ID))
                {
                    wLayer.ID = _layerIDSequece.Number;
                }

                if (fLayer.Class == null)
                {
                    _errorMessages.Add("Invalid layer: " + wLayer.Title + "\n" + errorMessage);
                }

                _layers.Add(wLayer);

                if (wLayer.WebServiceClass != null && wLayer.WebServiceClass.Themes != null)
                {
                    foreach (IWebServiceTheme theme in wLayer.WebServiceClass.Themes)
                    {
                        while (LayerIDExists(theme.ID) || theme.ID == 0)
                        {
                            theme.ID = _layerIDSequece.Number;
                        }
                    }

                    if (LayerAdded != null)
                    {
                        LayerAdded(this, wLayer);
                    }
                }
            }

            stream.Load("IClasses", null, new PersistableClasses(_layers));
            _toc = (Toc)await stream.LoadAsync<IToc>("ITOC", new Toc(this));

            stream.Load("IGraphicsContainer", null, GraphicsContainer);

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

            string layerDescriptionKeys = (string)stream.Load("LayerDescriptionKeys", string.Empty);
            if (!string.IsNullOrWhiteSpace(layerDescriptionKeys))
            {
                foreach (int key in layerDescriptionKeys
                    .Split(',')
                    .Where(i => int.TryParse(i, out int x))
                    .Select(i => int.Parse(i)))
                {
                    SetLayerDescription(key, Encoding.Unicode.GetString(
                        Convert.FromBase64String((string)stream.Load($"LayerDescription_{key}", string.Empty))));
                }
            }

            string layerCopyrightTextKeys = (string)stream.Load("LayerCopyrightTextKeys", string.Empty);
            if (!string.IsNullOrWhiteSpace(layerCopyrightTextKeys))
            {
                foreach (int key in layerCopyrightTextKeys
                    .Split(',')
                    .Where(i => int.TryParse(i, out int x))
                    .Select(i => int.Parse(i)))
                {
                    SetLayerCopyrightText(key, Encoding.Unicode.GetString(
                        Convert.FromBase64String((string)stream.Load($"LayerCopyrightText_{key}", string.Empty))));
                }
            }

            #region Metadata

            #region Metadata

            var persistMetadataProviders = new PersistMetadataProviders();
            stream.Load("MetadataProviders", null, persistMetadataProviders);
            await SetMetadataProviders(persistMetadataProviders.Providers);

            #endregion

            #region MapServiceProperties

            MapServiceProperties.Load(stream);

            #endregion

            #endregion

            foreach (var eventHook in _eventHooks.EventHooks?.Where(h => h.Type == HookEventType.OnLoaded) ?? [])
            {
                try
                {
                    await eventHook.InvokeAsync(this);
                }
                catch (MapEventHookWarningException wex)
                {
                    _errorMessages.Add($"WARNING: Hook {eventHook.GetType()}: {wex.Message}");
                }
                catch (Exception ex)
                {
                    _errorMessages.Add($"ERROR: Hook {eventHook.GetType()}: {ex.Message}");
                }
            }

            if (stream.Warnings != null)
            {
                foreach (var warning in stream.Warnings)
                {
                    _errorMessages.Add(warning);
                }
            }

            if (stream.Errors != null)
            {
                foreach (var error in stream.Errors)
                {
                    _errorMessages.Add(error);
                }
            }

            stream.ClearErrorsAndWarnings();

            return true;
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("name", m_name);
            stream.Save("minx", _minX);
            stream.Save("miny", _minY);
            stream.Save("maxx", _maxX);
            stream.Save("maxy", _maxY);

            stream.Save("title", Title ?? string.Empty);

            stream.Save("act_minx", _actMinX);
            stream.Save("act_miny", _actMinY);
            stream.Save("act_maxx", _actMaxX);
            stream.Save("act_maxy", _actMaxY);

            stream.Save("refScale", _refScale);

            stream.Save("iwidth", ImageWidth);
            stream.Save("iheight", ImageHeight);

            stream.Save("background", _backgroundColor.ToArgb());

            if (SpatialReference != null)
            {
                stream.Save("SpatialReference", SpatialReference);
            }
            if (LayerDefaultSpatialReference != null)
            {
                stream.Save("LayerDefaultSpatialReference", LayerDefaultSpatialReference);
            }
            stream.Save("WebMercatorScaleBehavoir", (int)this.WebMercatorScaleBehavoir);

            stream.Save("layerIDSequence", _layerIDSequece);

            stream.Save("MapUnits", (int)MapUnits);
            stream.Save("DisplayUnits", (int)DisplayUnits);

            foreach (IDataset dataset in _datasets)
            {
                stream.Save("IDataset", dataset);
            }

            foreach (ILayer layer in _layers)
            {
                if (layer is IGroupLayer)
                {
                    stream.Save("IGroupLayer", layer);
                }
                else if (layer is IRasterCatalogLayer)
                {
                    stream.Save("IRasterCatalogLayer", layer);
                }
                else if (layer is IRasterLayer)
                {
                    stream.Save("IRasterLayer", layer);
                }
                else if (layer is IWebServiceLayer)
                {
                    stream.Save("IWebServiceLayer", layer);
                }
                else if (layer is IFeatureLayer)
                {
                    stream.Save("IFeatureLayer", layer);
                }
            }

            stream.Save("IClasses", new PersistableClasses(_layers));
            stream.Save("ITOC", _toc);
            stream.Save("IGraphicsContainer", Display.GraphicsContainer);

            if (_layerDescriptions != null)
            {
                var descriptionsKeys = _layerDescriptions.Keys
                    .Where(i => !string.IsNullOrWhiteSpace(_layerDescriptions[i]))
                    .Select(i => i.ToString());

                stream.Save("LayerDescriptionKeys", string.Join(",", descriptionsKeys));

                foreach (var key in _layerDescriptions.Keys)
                {
                    stream.Save($"LayerDescription_{key}", Convert.ToBase64String(
                        Encoding.Unicode.GetBytes(_layerDescriptions[key])));
                }
            }
            if (_layerCopyrightTexts != null)
            {
                var copyrightTextKeys = _layerCopyrightTexts.Keys
                    .Where(i => !string.IsNullOrWhiteSpace(_layerCopyrightTexts[i]))
                    .Select(i => i.ToString());

                stream.Save("LayerCopyrightTextKeys", string.Join(",", copyrightTextKeys));

                foreach (var key in _layerCopyrightTexts.Keys)
                {
                    stream.Save($"LayerCopyrightText_{key}", Convert.ToBase64String(
                        Encoding.Unicode.GetBytes(_layerCopyrightTexts[key])));
                }
            }

            if (_resourceContainer.HasResources)
            {
                stream.Save("MapResources", _resourceContainer);
            }

            if (_eventHooks?.EventHooks?.Any() == true)
            {
                stream.Save("MapEventHooks", _eventHooks);
            }

            #region Metadata

            //this.WriteMetadata(stream).Wait();
            var metadataProviders = GetMetadataProviders().Result;
            stream.Save("MetadataProviders", new PersistMetadataProviders(new List<IMetadataProvider>(metadataProviders)));

            #endregion

            #region MapServiceProperties

            MapServiceProperties.Save(stream);

            #endregion
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
                if (_layers == null)
                {
                    return;
                }

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
                {
                    return;
                }

                stream.Save("LayerID", _layer.ID);
                stream.Save("Stream", _layer.Class);
            }

            #endregion
        }

        private class PersistMetadataProviders : IPersistable
        {
            private readonly ICollection<IMetadataProvider> _providers;
            public PersistMetadataProviders(ICollection<IMetadataProvider> providers = null)
            {
                _providers = providers ?? new List<IMetadataProvider>();
            }

            public ICollection<IMetadataProvider> Providers => _providers;

            #region IPersistable

            public void Load(IPersistStream stream)
            {
                IMetadataProvider provider;
                while ((provider = (IMetadataProvider)stream.Load("IMetadataProvider")) != null)
                {
                    _providers.Add(provider);
                }
            }

            public void Save(IPersistStream stream)
            {
                if (_providers != null)
                {
                    foreach (var provider in _providers)
                    {
                        stream.Save("IMetadataProvider", provider);
                    }
                }
            }

            #endregion
        }

        #endregion

        public Toc DataViewTOC
        {
            set
            {
                _dataViewTOC = value;
                if (TOCChanged != null)
                {
                    TOCChanged(this);
                }
            }
        }
        public IToc PublicTOC
        {
            get { return _toc; }
        }
        public bool drawScaleBar
        {
            get { return _drawScaleBar; }
            set { _drawScaleBar = value; }
        }

        #region IClone Member

        public object Clone()
        {
            return new Map(this, false);
        }

        #endregion

        #region IDebugging

        protected Exception _lastException = null;

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

        private ConcurrentBag<Exception> _requestExceptions = null;
        private object exceptionLocker = new object();

        public void AddRequestException(Exception ex)
        {
            lock (exceptionLocker)
            {
                if (_requestExceptions == null)
                {
                    _requestExceptions = new ConcurrentBag<Exception>();
                }

                _requestExceptions.Add(ex);
            }
        }

        public void AppendRequestExceptionsToImage()
        {
            if (_requestExceptions == null || Display.Canvas == null)
            {
                return;
            }

            lock (exceptionLocker)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception ex in _requestExceptions)
                {
                    sb.Append("Exception: " + ex.Message + "\r\n");
                    sb.Append(ex.StackTrace + "\r\n");
                }

                using (var font = GraphicsEngine.Current.Engine.CreateFont("Arial", 12))
                using (var backgroundBrush = GraphicsEngine.Current.Engine.CreateSolidBrush(GraphicsEngine.ArgbColor.LightGray))
                using (var borderPen = GraphicsEngine.Current.Engine.CreatePen(GraphicsEngine.ArgbColor.Black, 2f))
                using (var textBrush = GraphicsEngine.Current.Engine.CreateSolidBrush(GraphicsEngine.ArgbColor.Red))
                {
                    var sizeF = Display.Canvas.MeasureText(sb.ToString().ToString(), font);
                    int mx = 30, //Display.ImageWidth / 2 - (int)sizeF.Width / 2, 
                        my = 30; //Display.ImageHeight / 2 - (int)sizeF.Height / 2;
                    Display.Canvas.FillRectangle(backgroundBrush, new GraphicsEngine.CanvasRectangle(mx - 30, my - 30, (int)sizeF.Width + 60, (int)sizeF.Height + 60));
                    Display.Canvas.DrawRectangle(borderPen, new GraphicsEngine.CanvasRectangle(mx - 30, my - 30, (int)sizeF.Width + 60, (int)sizeF.Height + 60));
                    //Display.Canvas.DrawText(sb.ToString(), font, textBrush, new GraphicsEngine.CanvasPoint(mx, my));
                    Display.Canvas.DrawText(sb.ToString(), font, textBrush, new GraphicsEngine.CanvasRectangleF(mx - 30, my - 30, (int)sizeF.Width + 60, (int)sizeF.Height + 60));
                }
            }
        }

        public void ResetRequestExceptions()
        {
            lock (exceptionLocker)
            {
                _requestExceptions = null;
            }
        }

        public bool HasRequestExceptions
        {
            get
            {
                return _requestExceptions != null && _requestExceptions.Count > 0;
            }
        }

        public IEnumerable<Exception> RequestExceptions { get { return _requestExceptions?.ToArray(); } }

        public IEnumerable<string> ErrorMessages
        {
            get { return _errorMessages.ToArray(); }
        }

        public bool HasErrorMessages { get { return _errorMessages != null && _errorMessages.Count > 0; } }

        #endregion

        internal void FireOnUserInterface(bool lockUI)
        {
            OnUserInterface?.Invoke(this, lockUI);

        }
        protected void SetResourceContainer(IResourceContainer resourceContainer)
        {
            _resourceContainer = resourceContainer ?? _resourceContainer;
        }
        public IResourceContainer ResourceContainer => _resourceContainer;

        protected void SetMapEventHooks(IMapEventHooks eventHooks)
        {
            _eventHooks = eventHooks;
        }
        public IMapEventHooks MapEventHooks => _eventHooks;

        public IMapServiceProperties MapServiceProperties { get; protected set; }

        #region Map / Layer Description

        protected ConcurrentDictionary<int, string> _layerDescriptions = null;
        public string GetLayerDescription(int layerId)
        {
            if (_layerDescriptions != null && _layerDescriptions.ContainsKey(layerId))
            {
                return _layerDescriptions[layerId];
            }

            return string.Empty;
        }
        public void SetLayerDescription(int layerId, string description)
        {
            if (_layerDescriptions == null)
            {
                _layerDescriptions = new ConcurrentDictionary<int, string>();
            }
            _layerDescriptions[layerId] = description;
        }

        protected ConcurrentDictionary<int, string> _layerCopyrightTexts = null;
        public string GetLayerCopyrightText(int layerId)
        {
            if (_layerCopyrightTexts != null && _layerCopyrightTexts.ContainsKey(layerId))
            {
                return _layerCopyrightTexts[layerId];
            }

            return string.Empty;
        }

        public void SetLayerCopyrightText(int layerId, string copyrightText)
        {
            if (_layerCopyrightTexts == null)
            {
                _layerCopyrightTexts = new ConcurrentDictionary<int, string>();
            }
            _layerCopyrightTexts[layerId] = copyrightText;
        }

        public ConcurrentDictionary<int, string> LayerDescriptions => _layerDescriptions;
        public ConcurrentDictionary<int, string> LayerCopyrightTexts => _layerCopyrightTexts;

        #endregion

        #region IRefreshSequences

        public void RefreshSequences()
        {
            var maxLayerId = MapElements.Select(e => e.ID).Max();

            _layerIDSequece.SetToIfLower(maxLayerId + 1);
        }

        #endregion
    }
}
