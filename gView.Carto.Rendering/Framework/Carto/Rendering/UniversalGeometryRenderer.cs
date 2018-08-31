using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.Data;
using System.Reflection;
using gView.Framework.Carto.Rendering.UI;

namespace gView.Framework.Carto.Rendering
{
    [gView.Framework.system.RegisterPlugIn("48EDC5DB-18B6-44cc-8646-461B388F2D94")]
    public class UniversalGeometryRenderer : Cloner, IFeatureRenderer, IDisposable, IPropertyPage, ILegendGroup
    {
        private bool _useRefScale = true;
        UniversalGeometrySymbol _symbol;

        private SymbolRotation _symbolRotation;

        public UniversalGeometryRenderer()
        {
            _symbol = new UniversalGeometrySymbol(UniversalGeometrySymbol.SymbolType.normal);
            _symbolRotation = new SymbolRotation();
        }

        public ISymbol this[geometryType type]
        {
            get { return _symbol[type]; }
            set { _symbol[type] = value; }
        }

        public SymbolRotation SymbolRotation
        {
            get { return _symbolRotation; }
            set
            {
                if (value == null)
                    _symbolRotation.RotationFieldName = "";
                else
                    _symbolRotation = value;
            }
        }

        public bool UsePointSymbol
        {
            get { return (_symbol != null) ? _symbol.UsePointSymbol : false; }
            set { if (_symbol != null) _symbol.UsePointSymbol = value; }
        }
        public bool UseLineSymbol
        {
            get { return (_symbol != null) ? _symbol.UseLineSymbol : false; }
            set { if (_symbol != null) _symbol.UseLineSymbol = value; }
        }
        public bool UsePolygonSymbol
        {
            get { return (_symbol != null) ? _symbol.UsePolygonSymbol : false; }
            set { if (_symbol != null) _symbol.UsePolygonSymbol = value; }
        }

        #region IFeatureRenderer Member

        public void Draw(IDisplay disp, gView.Framework.Data.IFeature feature)
        {
            if (feature == null) return;

            _symbol.Draw(feature.Shape, disp);
        }

        public void FinishDrawing(IDisplay disp, ICancelTracker cancelTracker)
        {
        }

        public void PrepareQueryFilter(gView.Framework.Data.IFeatureLayer layer, gView.Framework.Data.IQueryFilter filter)
        {

        }

        public bool CanRender(gView.Framework.Data.IFeatureLayer layer, IMap map)
        {
            if (layer != null &&
                layer.FeatureClass != null &&
                (layer.FeatureClass.GeometryType == geometryType.Unknown ||
                 layer.FeatureClass.GeometryType == geometryType.Network ||
                 layer.FeatureClass.GeometryType == geometryType.Aggregate)) return true;

            else if (layer is IWebServiceTheme)
                return true;

            return false;
        }

        public bool HasEffect(IFeatureLayer layer, IMap map)
        {
            return true;
        }

        public bool UseReferenceScale
        {
            get
            {
                return _useRefScale;
            }
            set
            {
                _useRefScale = false;
            }
        }

        public string Name
        {
            get { return "Simple Renderer for all Geometries"; }
        }

        public string Category
        {
            get { return "Features"; }
        }

        #endregion

        #region IPersistable Member

        public void Load(gView.Framework.IO.IPersistStream stream)
        {
            _symbol = (UniversalGeometrySymbol)stream.Load("Symbol", new UniversalGeometrySymbol(UniversalGeometrySymbol.SymbolType.normal), new UniversalGeometrySymbol(UniversalGeometrySymbol.SymbolType.normal));
            _symbolRotation = (SymbolRotation)stream.Load("SymbolRotation", _symbolRotation, _symbolRotation);
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            stream.Save("Symbol", _symbol);
            if (_symbolRotation.RotationFieldName != "")
            {
                stream.Save("SymbolRotation", _symbolRotation);
            }
        }

        #endregion

        #region IClone2 Member

        public object Clone(IDisplay display)
        {
            UniversalGeometryRenderer renderer = new UniversalGeometryRenderer();
            if (_symbol != null)
                renderer._symbol = (UniversalGeometrySymbol)_symbol.Clone(_useRefScale ? display : null);

            renderer._symbolRotation = (SymbolRotation)_symbolRotation.Clone();

            return renderer;
        }

        public void Release()
        {
            Dispose();
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion

        #region IPropertyPage Member

        public object PropertyPage(object initObject)
        {
            if (initObject is IFeatureLayer)
            {
                if (((IFeatureLayer)initObject).FeatureClass == null
                    && !(initObject is IWebServiceTheme)) return null;

                string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Carto.Rendering.UI.dll");

                IPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Carto.Rendering.UI.PropertyForm_UniversalGeometryRenderer") as IPropertyPanel;
                if (p != null)
                {
                    return p.PropertyPanel(this, (IFeatureLayer)initObject);
                }
            }

            return null;
        }

        public object PropertyPageObject()
        {
            return this;
        }

        #endregion

        #region ILegendGroup Member

        public int LegendItemCount
        {
            get
            {
                if (_symbol == null) return 0;

                int count = 0;
                if (_symbol.UsePointSymbol) count++;
                if (_symbol.UseLineSymbol) count++;
                if (_symbol.UsePolygonSymbol) count++;

                return count;
            }
        }

        public ILegendItem LegendItem(int index)
        {
            if (_symbol == null) return null;

            switch (index)
            {
                case 0:
                    if (_symbol.UsePointSymbol)
                        return _symbol[geometryType.Point];
                    if (_symbol.UseLineSymbol)
                        return _symbol[geometryType.Polyline];
                    if (_symbol.UsePolygonSymbol)
                        return _symbol[geometryType.Polygon];
                    break;
                case 1:
                    if (_symbol.UsePointSymbol &&
                        _symbol.UseLineSymbol)
                        return _symbol[geometryType.Polyline];
                    if (_symbol.UseLineSymbol &&
                        _symbol.UsePolygonSymbol)
                        return _symbol[geometryType.Polygon];
                    break;
                case 2:
                    if (_symbol.UsePointSymbol &&
                        _symbol.UseLineSymbol &&
                        _symbol.UsePolygonSymbol)
                        return _symbol[geometryType.Polygon];
                    break;
            }
            return null;
        }

        public void SetSymbol(ILegendItem item, ISymbol symbol)
        {
            if (_symbol == null) return;

            if (item == _symbol[geometryType.Point])
                _symbol[geometryType.Point] = symbol;
            if (item == _symbol[geometryType.Polyline])
                _symbol[geometryType.Polyline] = symbol;
            if (item == _symbol[geometryType.Polygon])
                _symbol[geometryType.Polygon] = symbol;
        }

        #endregion

        #region IRenderer Member

        public List<ISymbol> Symbols
        {
            get
            {
                if (_symbol != null)
                    return _symbol.Symbols;
                return new List<ISymbol>();
            }
        }

        public bool Combine(IRenderer renderer)
        {
            return false;
        }
        #endregion
    }

    class UniversalGeometrySymbol : IPersistable, IClone2
    {
        public enum SymbolType { normal, selection, highlight }
        private ISymbol _pointSymbol, _lineSymbol, _polygonSymbol;
        private bool _usePointSymbol = true,
                     _useLineSymbol = true,
                     _usePolygonSymbol = true;

        private UniversalGeometrySymbol() { }
        public UniversalGeometrySymbol(SymbolType type)
        {
            switch (type)
            {
                case SymbolType.normal:
                    _pointSymbol = RendererFunctions.CreateStandardSymbol(geometryType.Point);
                    _lineSymbol = RendererFunctions.CreateStandardSymbol(geometryType.Polyline);
                    _polygonSymbol = RendererFunctions.CreateStandardSymbol(geometryType.Polygon);
                    break;
                case SymbolType.selection:
                    _pointSymbol = RendererFunctions.CreateStandardSelectionSymbol(geometryType.Point);
                    _lineSymbol = RendererFunctions.CreateStandardSelectionSymbol(geometryType.Polyline);
                    _polygonSymbol = RendererFunctions.CreateStandardSelectionSymbol(geometryType.Polygon);
                    break;
                case SymbolType.highlight:
                    _pointSymbol = RendererFunctions.CreateStandardHighlightSymbol(geometryType.Point);
                    _lineSymbol = RendererFunctions.CreateStandardHighlightSymbol(geometryType.Polyline);
                    _polygonSymbol = RendererFunctions.CreateStandardHighlightSymbol(geometryType.Polygon);
                    break;
            }
        }

        public ISymbol this[geometryType type]
        {
            get
            {
                switch (type)
                {
                    case geometryType.Point:
                        return _pointSymbol;
                    case geometryType.Polyline:
                        return _lineSymbol;
                    case geometryType.Polygon:
                        return _polygonSymbol;
                }
                return null;
            }
            set
            {
                if (value == null) return;

                switch (type)
                {
                    case geometryType.Point:
                        if (value != _pointSymbol)
                        {
                            _pointSymbol.Release();
                            _pointSymbol = value;
                        }
                        break;
                    case geometryType.Polyline:
                        if (value != _lineSymbol)
                        {
                            _lineSymbol.Release();
                            _lineSymbol = value;
                        }
                        break;
                    case geometryType.Polygon:
                        if (value != _polygonSymbol)
                        {
                            _polygonSymbol.Release();
                            _polygonSymbol = value;
                        }
                        break;
                }
            }
        }

        public bool UsePointSymbol
        {
            get { return _usePointSymbol; }
            set { _usePointSymbol = value; }
        }
        public bool UseLineSymbol
        {
            get { return _useLineSymbol; }
            set { _useLineSymbol = value; }
        }
        public bool UsePolygonSymbol
        {
            get { return _usePolygonSymbol; }
            set { _usePolygonSymbol = value; }
        }
        public void Draw(IGeometry geometry, IDisplay display)
        {
            if (display == null || geometry == null) return;

            if (geometry is IPoint)
            {
                display.Draw(_pointSymbol, geometry);
            }
            else if (geometry is IPointCollection)
            {
                for (int i = 0; i < ((IPointCollection)geometry).PointCount; i++)
                    Draw(((IPointCollection)geometry)[i], display);
            }
            else if (geometry is IPolyline)
            {
                display.Draw(_lineSymbol, geometry);
            }
            else if (geometry is IPolygon)
            {
                display.Draw(_polygonSymbol, geometry);
            }
            else if (geometry is IAggregateGeometry)
            {
                for (int i = 0; i < ((IAggregateGeometry)geometry).GeometryCount; i++)
                    Draw(((IAggregateGeometry)geometry)[i], display);
            }
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _usePointSymbol = (bool)stream.Load("UsePointSymbol", true);
            _useLineSymbol = (bool)stream.Load("UseLineSymbol", true);
            _usePolygonSymbol = (bool)stream.Load("UsePolygonSymbol", true);

            _pointSymbol = (ISymbol)stream.Load("PointSymbol");
            _lineSymbol = (ISymbol)stream.Load("LineSymbol");
            _polygonSymbol = (ISymbol)stream.Load("PolygonSymbol");
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("UsePointSymbol", _usePointSymbol);
            stream.Save("UseLineSymbol", _useLineSymbol);
            stream.Save("UsePolygonSymbol", _usePolygonSymbol);

            stream.Save("PointSymbol", _pointSymbol);
            stream.Save("LineSymbol", _lineSymbol);
            stream.Save("PolygonSymbol", _polygonSymbol);
        }

        #endregion

        #region IClone2 Member

        public object Clone(IDisplay display)
        {
            UniversalGeometrySymbol symbol = new UniversalGeometrySymbol();
            symbol._pointSymbol = (ISymbol)_pointSymbol.Clone(display);
            symbol._lineSymbol = (ISymbol)_lineSymbol.Clone(display);
            symbol._polygonSymbol = (ISymbol)_polygonSymbol.Clone(display);

            return symbol;
        }

        public void Release()
        {
            if (_pointSymbol != null) _pointSymbol.Release();
            if (_lineSymbol != null) _lineSymbol.Release();
            if (_polygonSymbol != null) _polygonSymbol.Release();

            _pointSymbol = null;
            _lineSymbol = null;
            _polygonSymbol = null;
        }

        #endregion

        public List<ISymbol> Symbols
        {
            get
            {
                return new List<ISymbol>(new ISymbol[] { _pointSymbol, _lineSymbol, _polygonSymbol });
            }
        }
    }
}
