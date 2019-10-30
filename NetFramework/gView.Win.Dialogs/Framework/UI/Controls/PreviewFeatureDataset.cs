using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.Carto;
using gView.Framework.UI;
using gView.Framework.system;
using gView.Framework.Symbology;
using gView.Framework.system.UI;
using System.Threading.Tasks;
using gView.Framework.Sys.UI;
using gView.Framework.IO;

namespace gView.Framework.UI.Controls
{
    internal class PreviewFeatureDataset : DatasetMetadata, IFeatureDataset
    {
        private IEnvelope _envelope = null;
        private ISpatialReference _sRef = null;
        private ILayer _layer = null;

        public PreviewFeatureDataset(IFeatureClass fc)
        {
            if (fc == null) return;
            _envelope = fc.Envelope;
            _sRef = fc.SpatialReference;

            _layer = new PreviewFeatureLayer(fc);
        }
        public PreviewFeatureDataset(IRasterLayer rasterLayer)
        {
            if (rasterLayer == null || rasterLayer.RasterClass != null || rasterLayer.RasterClass.Polygon == null) return;
            _envelope = rasterLayer.RasterClass.Polygon.Envelope;
            _sRef = rasterLayer.RasterClass.SpatialReference;

            _layer = rasterLayer;
        }

        #region IFeatureDataset Members

        public Task<gView.Framework.Geometry.IEnvelope> Envelope()
        {
            return Task.FromResult(_envelope);
        }

        public Task<ISpatialReference> GetSpatialReference()
        {
            return Task.FromResult(_sRef);
        }
        public void SetSpatialReference(ISpatialReference value)
        {
            _sRef = value;
        }
        
        #endregion

        #region IDataset Members

        public void Dispose()
        {

        }

        public string ConnectionString
        {
            get
            {
                return "";
            }
        }
        public Task<bool> SetConnectionString(string value)
        {
            return Task.FromResult(true);
        }
        

        public string DatasetGroupName
        {
            get { return ""; }
        }

        public string DatasetName
        {
            get { return "Preview Dataset"; }
        }

        public string ProviderName
        {
            get { return "gView"; }
        }

        public DatasetState State
        {
            get { return DatasetState.unknown; }
        }

        public Task<bool> Open()
        {
            return Task.FromResult(true);
        }

        public string LastErrorMessage
        {
            get; set;
        }

        public int order
        {
            get
            {
                return 0;
            }
            set
            {

            }
        }

        public IDatasetEnum DatasetEnum
        {
            get { return null; }
        }

        public Task<List<IDatasetElement>> Elements()
        {
            List<IDatasetElement> _elements = new List<IDatasetElement>();
            if (_layer != null)
            {
                _elements.Add(_layer);
            }
            return Task.FromResult(_elements);
        }

        public string Query_FieldPrefix
        {
            get { return ""; }
        }

        public string Query_FieldPostfix
        {
            get { return ""; }
        }

        public gView.Framework.FDB.IDatabase Database
        {
            get { return null; }
        }

        public Task<IDatasetElement> Element(string title)
        {

            if (_layer == null) return null;
            if (_layer.Title == title)
                return Task.FromResult<IDatasetElement>(_layer);

            return Task.FromResult<IDatasetElement>(null); ;
        }

        async public Task RefreshClasses()
        {
        }

        #endregion

        #region IPersistableLoadAsync

        public Task<bool> LoadAsync(IPersistStream stream)
        {
            return Task.FromResult(true);
        }

        public void Save(IPersistStream stream)
        {
        }
        #endregion
    }

    internal class PreviewFeatureLayer : Layer, IFeatureLayer
    {
        private IFeatureClass _fc;
        private IFeatureRenderer _renderer;
        private IFields _fields;

        public PreviewFeatureLayer(IFeatureClass fc)
        {
            _fc = fc;
            if (_fc == null) return;

            this.Title = fc.Name;
            this.Visible = true;

            _renderer = PlugInManager.Create(KnownObjects.Carto_SimpleRenderer) as IFeatureRenderer2;
            if (_renderer is ISymbolCreator)
            {
                ((IFeatureRenderer2)_renderer).Symbol = ((ISymbolCreator)_renderer).CreateStandardSymbol(_fc.GeometryType);
            }
            _fields = new Fields();
        }

        #region IFeatureLayer Members

        public gView.Framework.Carto.IFeatureRenderer FeatureRenderer
        {
            get
            {
                return _renderer;
            }
            set
            {
                _renderer = value;
            }
        }

        public gView.Framework.Carto.IFeatureRenderer SelectionRenderer
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        public gView.Framework.Carto.ILabelRenderer LabelRenderer
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        public bool ApplyRefScale
        {
            get
            {
                return true;
            }
            set
            {
            }
        }
        public bool ApplyLabelRefScale
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        public float MaxRefScaleFactor { get; set; }
        public float MaxLabelRefScaleFactor { get; set; }

        public IFeatureClass FeatureClass
        {
            get { return _fc; }
        }

        public IQueryFilter FilterQuery
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        public IFields Fields
        {
            get { return _fields; }
        }

        public FeatureLayerJoins Joins
        {
            get { return null; }
            set { }
        }

        public geometryType LayerGeometryType
        {
            get
            {
                return this.FeatureClass != null ? this.FeatureClass.GeometryType : geometryType.Unknown;
            }
            set
            {
                
            }
        }

        #endregion  
    }

    internal class ExplorerMapApplication : License, IMapApplication
    {
        IGUIApplication _application = null;
        //private MapView _mapView = null;
        private PreviewControl _control = null;
        public event EventHandler OnApplicationStart;

        public ExplorerMapApplication(IGUIApplication application, PreviewControl control)
        {
            _application = application;
            _control = control;
        }
        public ExplorerMapApplication(IGUIApplication application, PreviewControl control, object mapView)
            : this(application, control)
        {
            //_mapView = mapView;
        }

        #region IMapApplication Members

        public event AfterLoadMapDocumentEvent AfterLoadMapDocument;
        public event ActiveMapToolChangedEvent ActiveMapToolChanged;
        public event OnCursorPosChangedEvent OnCursorPosChanged;

        public bool InvokeRequired
        {
            get
            {
                return _application.InvokeRequired;
            }
        }
        public object Invoke(Delegate method)
        {
            return _application.Invoke(method);
        }

        async public Task LoadMapDocument(string filename)
        {
            if (_application is IMapApplication)
            {
                await ((IMapApplication)_application).LoadMapDocument(filename);
            }
        }

        async public Task RefreshActiveMap(DrawPhase drawPhase)
        {
            if (_application is IMapApplication)
            {
                await ((IMapApplication)_application).RefreshActiveMap(drawPhase);
            }
            else if (_control != null)
            {
                await _control.RefreshMap();
            }
        }

        async public Task RefreshTOC()
        {
        }
        public void RefreshTOCElement(IDatasetElement element)
        {
        }
        public void SaveMapDocument(string filename, bool performEncryption)
        {
            if (_application is IMapApplication)
                ((IMapApplication)_application).SaveMapDocument(filename, performEncryption);
        }

        public string DocumentFilename
        {
            get
            {
                return String.Empty;
            }
            set
            {

            }
        }

        public void SaveMapDocument()
        {
            if (_application is IMapApplication)
                ((IMapApplication)_application).SaveMapDocument();
        }

        public IDocumentWindow DocumentWindow
        {
            get
            {
                if (_application is IMapApplication)
                    return ((IMapApplication)_application).DocumentWindow;

                return null;
            }
        }

        public IMapApplicationModule IMapApplicationModule(Guid guid)
        {
            return null;
        }

        public bool ReadOnly { get { return false; } }

        public void DrawReversibleGeometry(IGeometry geometry, System.Drawing.Color color)
        {

        }
        #endregion

        #region IGUIApplication Members

        public void AddDockableWindow(IDockableWindow window, string ParentDockableWindowName)
        {
            if (_application != null) _application.AddDockableWindow(window, ParentDockableWindowName);
        }

        public void AddDockableWindow(IDockableWindow window, DockWindowState state)
        {
            if (_application != null) _application.AddDockableWindow(window, state);
        }

        public IGUIAppWindow ApplicationWindow
        {
            get
            {
                if (_application == null) return null;
                return _application.ApplicationWindow;
            }
        }

        public event DockWindowAddedEvent DockWindowAdded;

        public List<IDockableWindow> DockableWindows
        {
            get
            {
                if (_application == null) return new List<IDockableWindow>();
                return _application.DockableWindows;
            }
        }

        public event OnShowDockableWindowEvent OnShowDockableWindow;

        public void ShowDockableWindow(IDockableWindow window)
        {
            if (_application != null) _application.ShowDockableWindow(window);
        }

        public void SetCursor(object cursor)
        {
        }

        //public List<object> ToolStrips
        //{
        //    get
        //    {
        //        if (_application == null) return new List<object>();
        //        return _application.ToolStrips;
        //    }
        //}

        public ITool Tool(Guid guid)
        {
            if (_control != null) return _control.Tool(guid);
            return null;
        }
        public List<ITool> Tools
        {
            get { return new List<ITool>(); }
        }
        public IToolbar Toolbar(Guid guid)
        {
            return null;
        }
        public List<IToolbar> Toolbars
        {
            get
            {
                return new List<IToolbar>();
            }
        }
        public ITool ActiveTool
        {
            get { return null; }
            set { }
        }
        public void ValidateUI()
        {
            if (_application != null) _application.ValidateUI();
        }

        public IStatusBar StatusBar
        {
            get
            {
                return null;
            }
        }

        public void AppendContextMenuItems(ContextMenuStrip strip, object context)
        {

        }
        #endregion

        #region IApplication Members

        public void Exit()
        {
            if (_application != null) _application.Exit();
        }

        public string Title
        {
            get
            {
                if (_application == null) return "";
                return _application.Title;
            }
            set
            {
                if (_application != null) _application.Title = value;
            }
        }

        #endregion

        public void ShowBackstageMenu()
        {
        }

        public void HideBackstageMenu()
        {
        }
    }
}
