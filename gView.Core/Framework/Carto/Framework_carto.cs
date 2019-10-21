using System;
using System.Collections;
using System.Collections.Generic;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.Framework.UI;
using gView.Framework.IO;
using gView.Framework.system;
using gView.MapServer;
using System.IO;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace gView.Framework.Carto
{
    public enum DrawPhase
    {
        All = 7,
        Geography = 4,
        Selection = 2,
        Graphics = 1
    }

    //public delegate void DatasetAddedEvent(IMap sender,IDataset dataset);
    public delegate void LayerAddedEvent(IMap sender, ILayer layer);
    public delegate void LayerRemovedEvent(IMap sender, ILayer layer);
    public delegate void NewBitmapEvent(global::System.Drawing.Image image);
    public delegate void DoRefreshMapViewEvent();
    public delegate void StartRefreshMapEvent(IMap sender);
    public delegate void DrawingLayerEvent(string layerName);
    public delegate void TOCChangedEvent(IMap sender);
    public delegate void NewExtentRenderedEvent(IMap sender, IEnvelope extent);
    public delegate void DrawingLayerFinishedEvent(IMap sender, ITimeEvent timeEvent);

    public interface IMap : IDisposable, IClone, IMetadata, IDataCopyright, IPersistableLoadAsync
    {
        event LayerAddedEvent LayerAdded;
        event LayerRemovedEvent LayerRemoved;
        event NewBitmapEvent NewBitmap;
        event DoRefreshMapViewEvent DoRefreshMapView;
        event DrawingLayerEvent DrawingLayer;
        event TOCChangedEvent TOCChanged;
        event NewExtentRenderedEvent NewExtentRendered;
        event DrawingLayerFinishedEvent DrawingLayerFinished;
        event StartRefreshMapEvent StartRefreshMap;
        event EventHandler MapRenamed;

        void AddDataset(IDataset dataset, int order);
        void RemoveDataset(IDataset dataset);
        void RemoveAllDatasets();

        void AddLayer(ILayer layer);
        void AddLayer(ILayer layer, int pos);
        void RemoveLayer(ILayer layer);

        IDataset this[int datasetIndex]
        {
            get;
        }
        IDataset this[IDatasetElement element]
        {
            get;
        }
        IEnumerable<IDataset> Datasets { get; } 
        string Name { get; set; }

        Task<List<IDatasetElement>> Elements(string aliasname);
        List<IDatasetElement> MapElements { get; }
        Task<List<IDatasetElement>> ActiveLayers();

        IDatasetElement DatasetElementByClass(IClass cls);

        string ActiveLayerNames
        {
            get;
            set;
        }
        void ClearSelection();

        ITOC TOC { get; }

        Task<bool> RefreshMap(DrawPhase phase, ICancelTracker cancelTracker);

        ISelectionEnvironment SelectionEnvironment { get; }

        void HighlightGeometry(IGeometry geometry, int milliseconds);

        IDisplay Display { get; }

        void Release();

        bool IsRefreshing { get; }

        ISpatialReference LayerDefaultSpatialReference
        {
            get;
            set;
        }

        IEnumerable<string> ErrorMessages { get; }
        bool HasErrorMessages { get; }

        void Compress();

        string GetLayerDescription(int layerId);
        void SetLayerDescription(int layerId, string description);

        string GetLayerCopyrightText(int layerId);
        void SetLayerCopyrightText(int layerId, string copyrightText);
    }

    /*
    public interface IDataView
    {
        string Name { get; set; }

        IMap Map { get; set; }
    }
    */

    //public delegate bool LayerIsVisibleHook(string layername,bool defaultValue);
    public delegate void BeforeRenderLayersEvent(IServiceMap sender, List<ILayer> layers);

    public interface IServiceMap : IMetadata, IServiceRequestContext, IDisposable
    {
        //event LayerIsVisibleHook OverrideLayerIsVisible;
        event BeforeRenderLayersEvent BeforeRenderLayers;

        string Name { get; }

        IDisplay Display { get; }
        ITOC TOC { get; }

        List<IDatasetElement> MapElements { get; }

        void Release();
        Task<bool> Render();
        Task<global::System.Drawing.Bitmap> Legend();
        global::System.Drawing.Bitmap MapImage { get; }
        Task<bool> SaveImage(string path, global::System.Drawing.Imaging.ImageFormat format);
        Task<bool> SaveImage(Stream ms, global::System.Drawing.Imaging.ImageFormat format);
        void ReleaseImage();

        float ScaleSymbolFactor { get; set; }

        T GetModule<T>();
    }

    public delegate void MapScaleChangedEvent(IDisplay sender);
    public delegate void RenderOverlayImageEvent(global::System.Drawing.Bitmap image, bool clearOld);

    // Projective > 0
    // Geographic < 0
    public enum GeoUnits
    {
        Unknown = 0,
        Inches = 1,
        Feet = 2,
        Yards = 3,
        Miles = 4,
        NauticalMiles = 5,
        Millimeters = 6,
        Centimeters = 7,
        Decimeters = 8,
        Meters = 9,
        Kilometers = 10,
        DecimalDegrees = -1,
        DegreesMinutesSeconds = -2
    }

    public interface IDisplay
    {
        event MapScaleChangedEvent MapScaleChanged;
        event RenderOverlayImageEvent RenderOverlayImage;

        IEnvelope Envelope
        {
            get;
        }
        IEnvelope Limit
        {
            get;
            set;
        }
        void ZoomTo(IEnvelope envelope);

        double refScale { get; set; }
        double mapScale { get; set; }

        int iWidth { get; set; }
        int iHeight { get; set; }

        double dpm { get; }
        double dpi { get; set; }

        global::System.Drawing.Image Bitmap { get; }
        global::System.Drawing.Graphics GraphicsContext { get; }
        //IGraphicsEngine GraphicsContext { get; }
        global::System.Drawing.Color BackgroundColor { get; set; }
        global::System.Drawing.Color TransparentColor
        {
            get;
            set;
        }

        bool MakeTransparent
        {
            get;
            set;
        }

        void World2Image(ref double x, ref double y);
        void Image2World(ref double x, ref double y);

        ISpatialReference SpatialReference
        {
            get;
            set;
        }

        IGeometricTransformer GeometricTransformer
        {
            get;
            set;
        }

        void Draw(ISymbol symbol, IGeometry geometry);
        void DrawOverlay(IGraphicsContainer container, bool clearOld);
        void ClearOverlay();

        IGraphicsContainer GraphicsContainer { get; }

        ILabelEngine LabelEngine { get; }

        GeoUnits MapUnits { get; set; }
        GeoUnits DisplayUnits { get; set; }

        IScreen Screen { get; }
        IMap Map { get; }

        IDisplayTransformation DisplayTransformation
        {
            get;
        }
    }

    public interface IGraphicsEngine
    {
        SmoothingMode SmoothingMode { get; set; }

        void TranslateTransform(float x, float y);
        void RotateTransform(float angle);
        void ResetTransform();

        void FillEllipse(Brush brush, float x, float y, float width, float height);
        void DrawEllipse(Pen pen, float x, float y, float width, float height);

        void FillPath(Brush brush, GraphicsPath path);
        void DrawPath(Pen pen, GraphicsPath path);

        void FillRectangle(Brush brush, float x, float y, float width, float height);
        void FillRectangle(Brush brush, Rectangle rect);
        void FillRectangle(Brush brush, RectangleF rect);
        void DrawRectangle(Pen pen, float x, float y, float width, float height);

        System.Drawing.Text.TextRenderingHint TextRenderingHint { get; set; }

        void DrawString(string text, Font font, Brush brush, float x, float y, System.Drawing.StringFormat format);

        void DrawImage(Image image, Rectangle souceRect, Rectangle destRect, GraphicsUnit unit);

        SizeF MeasureString(string text, Font font);
    }

    public interface IDisplayTransformation
    {
        bool UseTransformation
        {
            get;
        }

        double DisplayRotation
        {
            get;
            set;
        }

        void Transform(IDisplay display, ref double x, ref double y);
        void InvTransform(IDisplay display, ref double x, ref double y);
        IEnvelope TransformedBounds(IDisplay display);
    }

    public interface IScreen
    {
        float LargeFontsFactor { get; }
    }

    public enum LabelAppendResult { Succeeded = 0, Overlap = 1, Outside = 2, WrongArguments = 3 }
    public interface ILabelEngine
    {
        void Init(IDisplay display, bool directDraw);
        LabelAppendResult TryAppend(IDisplay display, ITextSymbol symbol, IGeometry geometry, bool chechForOverlap);
        LabelAppendResult TryAppend(IDisplay display, List<IAnnotationPolygonCollision> aPolygons, IGeometry geometry, bool checkForOverlap);
        void Draw(IDisplay display, ICancelTracker cancelTracker);
        void Release();

        global::System.Drawing.Graphics LabelGraphicsContext { get; }
    }

    public interface ISmartLabelPoint : IPoint
    {
        IMultiPoint AlernativeLabelPoints(IDisplay display);
    }

    public interface IGraphicElementList : IEnumerable<IGraphicElement>
    {
        void Add(IGraphicElement element);
        void Remove(IGraphicElement element);
        void Clear();
        void Insert(int i, IGraphicElement element);
        bool Contains(IGraphicElement element);
        int Count { get; }
        IGraphicElement this[int i] { get; }

        IGraphicElementList Clone();
        IGraphicElementList Swap();
    }
    public enum GrabberMode { Pointer, Vertex }
    public interface IGraphicsContainer
    {
        event EventHandler SelectionChanged;

        IGraphicElementList Elements { get; }
        IGraphicElementList SelectedElements { get; }
        GrabberMode EditMode { get; set; }
    }

    public interface IGraphicElement
    {
        void Draw(IDisplay display);
    }

    public interface HitPositions
    {
        object Cursor { get; }
        int HitID { get; }
    }

    public interface IGraphicsElementDesigning
    {
        //bool Selected { get; set; }
        IGraphicElement2 Ghost { get; }
        HitPositions HitTest(IDisplay display, IPoint point);
        void Design(IDisplay display, HitPositions hit, double dx, double dy);
        bool TrySelect(IDisplay display, IEnvelope envelope);
        bool TrySelect(IDisplay display, IPoint point);

        bool RemoveVertex(IDisplay display, int index);
        bool AddVertex(IDisplay display, IPoint point);
    }

    public interface IGraphicElement2 : IGraphicElement, IGraphicsElementScaling, IGraphicsElementRotation, IIGraphicsElementTranslation, IGraphicsElementDesigning
    {
        string Name { get; }
        global::System.Drawing.Image Icon { get; }
        ISymbol Symbol { get; set; }
        void DrawGrabbers(IDisplay display);

        IGeometry Geometry { get; }
    }

    public interface IGraphicsElementScaling
    {
        void Scale(double scaleX, double scaleY);
        void ScaleX(double scale);
        void ScaleY(double scale);
    }
    public interface IGraphicsElementRotation
    {
        double Rotation { get; set; }
    }
    public interface IIGraphicsElementTranslation
    {
        void Translation(double x, double y);
    }

    public interface IRenderer
    {
        List<ISymbol> Symbols { get; }
        bool Combine(IRenderer renderer);
    }

    /// <summary>
    /// Porvide access to members and properties that control the functionality of renderers.
    /// </summary>
    public interface IFeatureRenderer : IRenderer, IPersistable, IClone, IClone2
    {
        /// <summary>
        /// Draws features from the specified Featurecursor on the given display.
        /// </summary>
        /// <param name="disp"></param>
        /// <param name="fCursor"></param>
        /// <param name="drawPhase"></param>
        /// <param name="cancelTracker"></param>
        //void Draw(IDisplay disp,IFeatureCursor fCursor,DrawPhase drawPhase,ICancelTracker cancelTracker);
        void Draw(IDisplay disp, IFeature feature);

        void FinishDrawing(IDisplay disp, ICancelTracker cancelTracker);

        /// <summary>
        /// Prepares the query filter for the rendering process. 
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// This member is called by the framework befor querying the features.
        /// <param name="layer"></param>
        /// <param name="filter">The filter for querying the features</param>
        void PrepareQueryFilter(IFeatureLayer layer, IQueryFilter filter);
        /// <summary>
        /// Indicates if the specified feature class can be rendered on the given display. 
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        bool CanRender(IFeatureLayer layer, IMap map);

        bool HasEffect(IFeatureLayer layer, IMap map);

        bool UseReferenceScale { get; set; }

        /// <summary>
        /// The name of the renderer.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The category for the renderer.
        /// </summary>
        string Category { get; }
    }
    public interface IFeatureRenderer2 : IFeatureRenderer
    {
        ISymbol Symbol { get; set; }
    }

    public enum LabelRenderMode { RenderWithFeature, UseRenderPriority }

    public interface ILabelRenderer : IRenderer, IPersistable, IClone, IClone2
    {
        void PrepareQueryFilter(IDisplay display, IFeatureLayer layer, IQueryFilter filter);
        bool CanRender(IFeatureLayer layer, IMap map);
        string Name { get; }

        LabelRenderMode RenderMode { get; }
        int RenderPriority { get; }

        void Draw(IDisplay disp, IFeature feature);
    }
}