using gView.Framework.Cartography.Rendering.UI;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.Framework.Core.Common;
using gView.Framework.Core.UI;
using gView.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using gView.Framework.Symbology;
using System.Linq.Expressions;
using gView.Framework.Cartography.UI;

namespace gView.Framework.Cartography.Rendering
{
    [RegisterPlugIn("48EDC5DB-18B6-44cc-8646-461B388F2D94")]
    public class UniversalGeometryRenderer : Cloner, IFeatureRenderer, IDisposable, IDefault, ILegendGroup
    {
        private bool _useRefScale = true;
        UniversalGeometrySymbol _symbol;

        private SymbolRotation _symbolRotation;

        public UniversalGeometryRenderer()
        {
            _symbol = new UniversalGeometrySymbol(UniversalGeometrySymbol.SymbolType.normal);
            _symbolRotation = new SymbolRotation();
        }

        public ISymbol this[GeometryType type]
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
                {
                    _symbolRotation.RotationFieldName = "";
                }
                else
                {
                    _symbolRotation = value;
                }
            }
        }

        public bool UsePointSymbol
        {
            get { return _symbol != null ? _symbol.UsePointSymbol : false; }
            set
            {
                if (_symbol != null)
                {
                    _symbol.UsePointSymbol = value;
                }
            }
        }
        public bool UseLineSymbol
        {
            get { return _symbol != null ? _symbol.UseLineSymbol : false; }
            set
            {
                if (_symbol != null)
                {
                    _symbol.UseLineSymbol = value;
                }
            }
        }
        public bool UsePolygonSymbol
        {
            get { return _symbol != null ? _symbol.UsePolygonSymbol : false; }
            set
            {
                if (_symbol != null)
                {
                    _symbol.UsePolygonSymbol = value;
                }
            }
        }

        #region IFeatureRenderer Member

        public void Draw(IDisplay disp, IFeature feature)
        {
            if (feature == null)
            {
                return;
            }

            _symbol.Draw(feature.Shape, disp);
        }

        public void StartDrawing(IDisplay display) { }

        public void FinishDrawing(IDisplay disp, ICancelTracker cancelTracker)
        {
        }

        public void PrepareQueryFilter(IFeatureLayer layer, IQueryFilter filter)
        {

        }

        public bool CanRender(IFeatureLayer layer, IMap map)
        {
            if (layer != null &&
                layer.FeatureClass != null &&
                (layer.FeatureClass.GeometryType == GeometryType.Unknown ||
                 layer.FeatureClass.GeometryType == GeometryType.Network ||
                 layer.FeatureClass.GeometryType == GeometryType.Aggregate))
            {
                return true;
            }
            else if (layer is IWebServiceTheme)
            {
                return true;
            }

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

        public bool RequireClone()
        {
            return _symbol?.Symbols?.Where(s => s != null && s.RequireClone()).FirstOrDefault() != null;
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _symbol = (UniversalGeometrySymbol)stream.Load("Symbol", new UniversalGeometrySymbol(UniversalGeometrySymbol.SymbolType.normal), new UniversalGeometrySymbol(UniversalGeometrySymbol.SymbolType.normal));
            _symbolRotation = (SymbolRotation)stream.Load("SymbolRotation", _symbolRotation, _symbolRotation);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("Symbol", _symbol);
            if (_symbolRotation.RotationFieldName != "")
            {
                stream.Save("SymbolRotation", _symbolRotation);
            }
        }

        #endregion

        #region IClone2 Member

        public object Clone(CloneOptions options)
        {
            UniversalGeometryRenderer renderer = new UniversalGeometryRenderer();
            if (_symbol != null)
            {
                renderer._symbol = (UniversalGeometrySymbol)_symbol.Clone(_useRefScale ? options : null);
            }

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

        #region ICreateDefault Member

        public ValueTask DefaultIfEmpty(object initObject)
        {
            return ValueTask.CompletedTask;
        }


        #endregion

        #region ILegendGroup Member

        public int LegendItemCount
        {
            get
            {
                if (_symbol == null)
                {
                    return 0;
                }

                int count = 0;
                if (_symbol.UsePointSymbol)
                {
                    count++;
                }

                if (_symbol.UseLineSymbol)
                {
                    count++;
                }

                if (_symbol.UsePolygonSymbol)
                {
                    count++;
                }

                return count;
            }
        }

        public ILegendItem LegendItem(int index)
        {
            if (_symbol == null)
            {
                return null;
            }

            switch (index)
            {
                case 0:
                    if (_symbol.UsePointSymbol)
                    {
                        return _symbol[GeometryType.Point];
                    }

                    if (_symbol.UseLineSymbol)
                    {
                        return _symbol[GeometryType.Polyline];
                    }

                    if (_symbol.UsePolygonSymbol)
                    {
                        return _symbol[GeometryType.Polygon];
                    }

                    break;
                case 1:
                    if (_symbol.UsePointSymbol &&
                        _symbol.UseLineSymbol)
                    {
                        return _symbol[GeometryType.Polyline];
                    }

                    if (_symbol.UseLineSymbol &&
                        _symbol.UsePolygonSymbol)
                    {
                        return _symbol[GeometryType.Polygon];
                    }

                    break;
                case 2:
                    if (_symbol.UsePointSymbol &&
                        _symbol.UseLineSymbol &&
                        _symbol.UsePolygonSymbol)
                    {
                        return _symbol[GeometryType.Polygon];
                    }

                    break;
            }
            return null;
        }

        public void SetSymbol(ILegendItem item, ISymbol symbol)
        {
            if (_symbol == null)
            {
                return;
            }

            if (item == _symbol[GeometryType.Point])
            {
                _symbol[GeometryType.Point] = symbol;
            }

            if (item == _symbol[GeometryType.Polyline])
            {
                _symbol[GeometryType.Polyline] = symbol;
            }

            if (item == _symbol[GeometryType.Polygon])
            {
                _symbol[GeometryType.Polygon] = symbol;
            }
        }

        #endregion

        #region IRenderer Member

        public List<ISymbol> Symbols
        {
            get
            {
                if (_symbol != null)
                {
                    return _symbol.Symbols;
                }

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
                    _pointSymbol = RendererFunctions.CreateStandardSymbol(GeometryType.Point);
                    _lineSymbol = RendererFunctions.CreateStandardSymbol(GeometryType.Polyline);
                    _polygonSymbol = RendererFunctions.CreateStandardSymbol(GeometryType.Polygon);
                    break;
                case SymbolType.selection:
                    _pointSymbol = RendererFunctions.CreateStandardSelectionSymbol(GeometryType.Point);
                    _lineSymbol = RendererFunctions.CreateStandardSelectionSymbol(GeometryType.Polyline);
                    _polygonSymbol = RendererFunctions.CreateStandardSelectionSymbol(GeometryType.Polygon);
                    break;
                case SymbolType.highlight:
                    _pointSymbol = RendererFunctions.CreateStandardHighlightSymbol(GeometryType.Point);
                    _lineSymbol = RendererFunctions.CreateStandardHighlightSymbol(GeometryType.Polyline);
                    _polygonSymbol = RendererFunctions.CreateStandardHighlightSymbol(GeometryType.Polygon);
                    break;
            }
        }

        public ISymbol this[GeometryType type]
        {
            get
            {
                switch (type)
                {
                    case GeometryType.Point:
                        return _pointSymbol;
                    case GeometryType.Polyline:
                        return _lineSymbol;
                    case GeometryType.Polygon:
                        return _polygonSymbol;
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                switch (type)
                {
                    case GeometryType.Point:
                        if (value != _pointSymbol)
                        {
                            _pointSymbol.Release();
                            _pointSymbol = value;
                        }
                        break;
                    case GeometryType.Polyline:
                        if (value != _lineSymbol)
                        {
                            _lineSymbol.Release();
                            _lineSymbol = value;
                        }
                        break;
                    case GeometryType.Polygon:
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
            if (display == null || geometry == null)
            {
                return;
            }

            if (_usePointSymbol && geometry is IPoint)
            {
                display.Draw(_pointSymbol, geometry);
            }
            else if (_usePointSymbol && geometry is IPointCollection)
            {
                for (int i = 0; i < ((IPointCollection)geometry).PointCount; i++)
                {
                    Draw(((IPointCollection)geometry)[i], display);
                }
            }
            else if (_useLineSymbol && geometry is IPolyline)
            {
                display.Draw(_lineSymbol, geometry);
            }
            else if (_usePolygonSymbol && geometry is IPolygon)
            {
                display.Draw(_polygonSymbol, geometry);
            }
            else if (geometry is IAggregateGeometry)
            {
                for (int i = 0; i < ((IAggregateGeometry)geometry).GeometryCount; i++)
                {
                    Draw(((IAggregateGeometry)geometry)[i], display);
                }
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

        public object Clone(CloneOptions options)
        {
            UniversalGeometrySymbol symbol = new UniversalGeometrySymbol();

            symbol._usePointSymbol = _usePointSymbol;
            symbol._useLineSymbol = _useLineSymbol;
            symbol._usePolygonSymbol = _usePolygonSymbol;

            symbol._pointSymbol = (ISymbol)_pointSymbol.Clone(options);
            symbol._lineSymbol = (ISymbol)_lineSymbol.Clone(options);
            symbol._polygonSymbol = (ISymbol)_polygonSymbol.Clone(options);

            return symbol;
        }

        public void Release()
        {
            if (_pointSymbol != null)
            {
                _pointSymbol.Release();
            }

            if (_lineSymbol != null)
            {
                _lineSymbol.Release();
            }

            if (_polygonSymbol != null)
            {
                _polygonSymbol.Release();
            }

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
