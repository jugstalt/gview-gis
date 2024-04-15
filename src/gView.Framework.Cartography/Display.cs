using gView.Framework.Calc;
using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Data.Metadata;
using gView.Framework.Geometry;
using gView.Framework.Geometry.Extensions;
using System;

namespace gView.Framework.Cartography
{
    public class Display : MapMetadata, IDisplay
    {
        public event MapScaleChangedEvent MapScaleChanged;
        public event RenderOverlayImageEvent RenderOverlayImage;

        #region Variablen

        protected double _maxX, _maxY, _minX, _minY;
        protected double _actMinX = -10, _actMinY = -10, _actMaxX = 10, _actMaxY = 10;
        protected int _imageWidth, _imageHeight, m_fixScaleIndex;
        protected double _dpm = 96.0 / 0.0254;  // dots per [m]... 96 dpi werden angenommen
        protected double _scale;
        protected bool _extentChanged;
        protected double _fontsizeFactor, _widthFactor, _refScale;
        //protected ArrayList m_fixScales;
        public GraphicsEngine.Abstraction.IBitmap _bitmap = null;
        public GraphicsEngine.Abstraction.ICanvas _canvas = null;
        protected ISpatialReference _spatialReference = null;
        protected IGeometricTransformer _geoTransformer = null;
        protected float _OX = 0, _OY = 0;
        protected GraphicsEngine.ArgbColor _backgroundColor = GraphicsEngine.ArgbColor.White;
        private GraphicsEngine.ArgbColor _transColor = GraphicsEngine.ArgbColor.White;
        private bool _makeTransparent = false;
        protected ILabelEngine _labelEngine;
        protected GeoUnits _mapUnits = GeoUnits.Unknown, _displayUnits = GeoUnits.Unknown;
        private object lockThis = new object();
        private DisplayScreen _screen;
        private DisplayTransformation _displayTransformation = new DisplayTransformation();

        #endregion

        public Display(IMap map)
        {
            Map = map;

            _extentChanged = true;
            _fontsizeFactor = _widthFactor = _refScale = -1.0;
            //m_fixScales=new ArrayList();

            _labelEngine = new LabelEngine2();

            _screen = new DisplayScreen();
            _screen.RefreshSettings();

            Dpi = GraphicsEngine.Current.Engine.ScreenDpi;
        }

        internal Display(IMap map, bool createLabelEngine)
            : this(map)
        {
            _extentChanged = true;
            _fontsizeFactor = _widthFactor = _refScale = -1.0;
            //m_fixScales = new ArrayList();

            _labelEngine = createLabelEngine ? new LabelEngine2() : null;
        }

        public void World2Image(ref double x, ref double y)
        {
            //x=(x-m_actMinX)*m_iWidth/_wWidth;
            //y=m_iHeight-(y-m_actMinY)*m_iHeight/_wHeight;

            x = (x - _actMinX) * _imageWidth / (_actMaxX - _actMinX);
            y = _imageHeight - (y - _actMinY) * _imageHeight / (_actMaxY - _actMinY);

            x += _OX;
            //y -= _OX;
            y += _OY;

            if (_displayTransformation.UseTransformation)
            {
                _displayTransformation.Transform(this, ref x, ref y);
            }
        }

        public void Image2World(ref double x, ref double y)
        {
            if (_displayTransformation.UseTransformation)
            {
                _displayTransformation.InvTransform(this, ref x, ref y);
            }

            x -= _OX;
            y -= _OY;

            x = x * (_actMaxX - _actMinX) / _imageWidth + _actMinX;
            y = (_imageHeight - y) * (_actMaxY - _actMinY) / _imageHeight + _actMinY;
        }

        public GraphicsEngine.Abstraction.ICanvas Canvas
        {
            get { return _canvas; }
            set { _canvas = value; }
        }

        public ISpatialReference SpatialReference
        {
            get { return _spatialReference; }
            set
            {
                _spatialReference = value;
                if (value != null)
                {
                    if (_spatialReference.SpatialParameters.IsGeographic && _mapUnits >= 0)
                    {
                        _displayUnits = _mapUnits = GeoUnits.DecimalDegrees;
                    }
                    else if (!_spatialReference.SpatialParameters.IsGeographic && _mapUnits < 0)
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
            if (RenderOverlayImage == null || _canvas != null)
            {
                return;
            }

            if (container == null)
            {
                ClearOverlay();
            }

            GraphicsEngine.Abstraction.IBitmap bm = null;
            try
            {
                bm = GraphicsEngine.Current.Engine.CreateBitmap(ImageWidth, ImageHeight);
                bm.MakeTransparent();

                _canvas = bm.CreateCanvas();
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
                if (_canvas != null)
                {
                    _canvas.Dispose();
                }

                _canvas = null;
                if (bm != null)
                {
                    bm.Dispose();
                }
            }
        }

        public void ClearOverlay()
        {
            RenderOverlayImage(null, true);
        }

        public void ZoomTo(IEnvelope envelope)
        {
            if (envelope == null)
            {
                return;
            }

            ZoomTo(envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY);
        }

        private IGraphicsContainer _graphicsContainer = new GraphicsContainer();
        public IGraphicsContainer GraphicsContainer
        {
            get { return _graphicsContainer; }
        }

        #region Properties

        public int ImageWidth
        {
            get { return _imageWidth; }
            set { _imageWidth = value; }
        }
        public int ImageHeight
        {
            get { return _imageHeight; }
            set { _imageHeight = value; }
        }

        public IEnvelope Limit
        {
            get
            {
                return new Envelope(_minX, _minY, _maxX, _maxY);
            }
            set
            {
                if (value != null)
                {
                    _minX = value.MinX;
                    _minY = value.MinY;
                    _maxX = value.MaxX;
                    _maxY = value.MaxY;
                }
            }
        }
        public double ReferenceScale
        {
            get
            {
                return _mapUnits == GeoUnits.Unknown ? 0.0 : _refScale;
            }
            set
            {
                _refScale = value;
                if (_refScale > 0.0)
                {
                    _widthFactor = _fontsizeFactor = _refScale / Math.Max(_scale, 1.0);
                }
                else
                {
                    _widthFactor = _fontsizeFactor = -1;
                }
            }
        }

        public double Dpm
        {
            get { return _dpm; }
        }
        public double Dpi
        {
            get { return Dpm * 0.0254; }
            set { _dpm = value / 0.0254; }
        }
        public IEnvelope Envelope
        {
            get
            {
                return new Envelope(_actMinX, _actMinY, _actMaxX, _actMaxY);
            }
            set
            {
                _actMinX = value.MinX;
                _actMinY = value.MinY;
                _actMaxX = value.MaxX;
                _actMaxY = value.MaxY;
            }
        }

        public GraphicsEngine.Abstraction.IBitmap Bitmap
        {
            get { return _bitmap; }
        }

        public GraphicsEngine.ArgbColor BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }

        public GraphicsEngine.ArgbColor TransparentColor
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

        private const double DegToRad = Math.PI / 180.0;

        private double CalcScale()
        {
            double phi = 0.0, webMercatorCosPhi = 1.0;
            if (_mapUnits == GeoUnits.DecimalDegrees)
            {
                phi = Math.Min(90.0, _actMaxY) * 0.5 + Math.Max(-90.0, _actMinY) * 0.5;
            }

            if (this.SpatialReference.IsWebMercator()
                && WebMercatorScaleBehavoir == WebMercatorScaleBehavoir.ApplyLatitudeCosPhi)
            {
                webMercatorCosPhi = Math.Cos(this.Envelope.Center.WebMercatorToGeographic().Y * DegToRad);
            }

            double w = Math.Abs(_actMaxX - _actMinX);
            double h = Math.Abs(_actMaxY - _actMinY);

            GeoUnitConverter converter = new GeoUnitConverter();
            double dpu = converter.Convert(1.0, GeoUnits.Meters, _mapUnits, 1, phi);

            double s1 = ImageWidth > 0 ? Math.Abs(w) / ImageWidth * Dpm : 1; s1 /= dpu;
            double s2 = ImageHeight > 0 ? Math.Abs(h) / ImageHeight * Dpm : 1; s2 /= dpu;
            double scale = ImageWidth > 0 && ImageHeight > 0 ? Math.Max(s1, s2) : Math.Max(MapScale, 1);

            return scale * webMercatorCosPhi;
        }

        private void CalcExtent(double scale, double cx, double cy)
        {
            double phi = 0.0, webMercatorCosPhi = 1.0;
            if (_mapUnits == GeoUnits.DecimalDegrees)
            {
                phi = _actMaxY * 0.5 + _actMinY * 0.5;
            }
            
            if (this.SpatialReference.IsWebMercator()
                && WebMercatorScaleBehavoir == WebMercatorScaleBehavoir.ApplyLatitudeCosPhi)
            {
                webMercatorCosPhi = Math.Cos(this.Envelope.Center.WebMercatorToGeographic().Y * DegToRad);
            }

            GeoUnitConverter converter = new GeoUnitConverter();
            double dpu = converter.Convert(1.0, GeoUnits.Meters, _mapUnits, 1, phi);

            double w = ImageWidth / Dpm * scale; w *= dpu;
            double h = ImageHeight / Dpm * scale; h *= dpu;

            _actMinX = (cx - w * 0.5) / webMercatorCosPhi;
            _actMaxX = (cx + w * 0.5) / webMercatorCosPhi;
            _actMinY = (cy - h * 0.5) / webMercatorCosPhi;
            _actMaxY = (cy + h * 0.5) / webMercatorCosPhi;
        }

        #endregion

        #region Zoom

        private void SetScale(double scale, double cx, double cy)
        {
            if (scale == 0.0)
            {
                return;
            }

            CalcExtent(scale, cx, cy);
            _scale = scale;

            if (MapScaleChanged != null)
            {
                MapScaleChanged(this);
            }
        }
        private void SetScale(double scale)
        {
            double cx = _actMaxX * 0.5 + _actMinX * 0.5;
            double cy = _actMaxY * 0.5 + _actMinY * 0.5;

            SetScale(scale, cx, cy);
        }

        public double MapScale
        {
            get { return _scale; }
            set
            {
                SetScale(value);
            }
        }

        public float WebMercatorScaleLevel => (float)WebMercatorCalc.Zoom(_scale);
        public WebMercatorScaleBehavoir WebMercatorScaleBehavoir { get; set; } = WebMercatorScaleBehavoir.Default;

        public void ZoomTo(double minx, double miny, double maxx, double maxy)
        {
            #region AutoResize

            double dx = Math.Abs(maxx - minx), mx = (maxx + minx) / 2.0;
            double dy = Math.Abs(maxy - miny), my = (maxy + miny) / 2.0;

            if (dx < dy)
            {
                double W = Math.Max((double)ImageWidth, 1) * dy / Math.Max((double)ImageHeight, 1);
                minx = mx - W / 2.0;
                maxx = mx + W / 2.0;
            }
            else
            {
                double H = Math.Max((double)ImageHeight, 1) * dx / Math.Max((double)ImageWidth, 1);
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

            _actMinX = cx - dx / 2.0; _actMaxX = cx + dx / 2.0;
            _actMinY = cy - dy / 2.0; _actMaxY = cy + dy / 2.0;
            _scale = CalcScale();

            if (_scale < 1.0)
            {
                SetScale(1.0);
            }
            else
            {
                if (MapScaleChanged != null)
                {
                    MapScaleChanged(this);
                }
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
            double cx = _actMaxX * 0.5 + _actMinX * 0.5;
            double cy = _actMaxY * 0.5 + _actMinY * 0.5;
            cx += px;
            cy += py;
            SetScale(_scale, cx, cy);
        }

        #endregion

        public ILabelEngine LabelEngine { get { return _labelEngine; } }

        public IGeometry Transform(IGeometry geometry, ISpatialReference geometrySpatialReference)
        {
            if (geometry == null)
            {
                return null;
            }

            if (geometrySpatialReference == null || _spatialReference == null || geometrySpatialReference.Equals(_spatialReference))
            {
                return geometry;
            }

            IGeometricTransformer transformer = GeometricTransformerFactory.Create();

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

        private IMap _map = null;
        virtual public IMap Map
        {
            get
            {
                return _map ?? this as IMap;
            }
            set { _map = value; }
        }

        private class DisplayScreen : IScreen
        {
            private float _fac = 1f;

            public DisplayScreen()
            {
            }

            #region IScreen Member

            public float LargeFontsFactor
            {
                get { return _fac; }
            }

            public void RefreshSettings(bool forceReloadAll = true)
            {
                _fac = SystemVariables.SystemFontsScaleFactor; //SystemVariables.PrimaryScreenDPI(forceReloadAll) / 96f;
            }

            #endregion
        }

        public IDisplayTransformation DisplayTransformation
        {
            get { return _displayTransformation; }
        }
    }
}
