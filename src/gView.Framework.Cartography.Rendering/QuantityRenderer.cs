using gView.Framework.Cartography.Rendering.UI;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.Framework.Core.system;
using gView.Framework.Core.UI;
using gView.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace gView.Framework.Cartography.Rendering
{
    [RegisterPlugIn("9D278A19-9547-4BC5-824E-E4F45EA1BB7A")]
    public class QuantityRenderer : Cloner, IFeatureRenderer, IDefault, ILegendGroup
    {
        private string _valueField = string.Empty;
        private ISymbol _defaultSymbol = null;
        private List<QuantityClass> _quantityClasses;
        private GeometryType _geometryType = GeometryType.Unknown;
        private SymbolRotation _symbolRotation;
        private bool _useRefscale = true;

        public QuantityRenderer()
        {
            _quantityClasses = new List<QuantityClass>();
            _symbolRotation = new SymbolRotation();
        }

        public void Dispose()
        {
            Release();
        }

        public string ValueField
        {
            get { return _valueField; }
            set
            {
                if (_valueField == value)
                {
                    return;
                }

                _valueField = value;
            }
        }

        public ISymbol DefaultSymbol
        {
            get { return _defaultSymbol; }
            set
            {
                _defaultSymbol = value;
                if (_defaultSymbol is ILegendItem)
                {
                    if (_defaultSymbol.LegendLabel == "")
                    {
                        _defaultSymbol.LegendLabel = "all other values";
                    }
                }
            }
        }

        public List<QuantityClass> QuantityClasses
        {
            get { return ListOperations<QuantityClass>.Clone(_quantityClasses); }
        }

        public bool AddClass(QuantityClass qClass)
        {
            if (qClass == null || _quantityClasses.Contains(qClass))
            {
                return false;
            }

            for (int i = 0; i < _quantityClasses.Count; i++)
            {
                QuantityClass c = _quantityClasses[i];
                if (qClass.Min < c.Max && qClass.Max > c.Min)
                {
                    throw new ArgumentException("Overlapping quantityclasses are not allowed!");
                }
                if (qClass.Max <= c.Min)
                {
                    _quantityClasses.Insert(i, qClass);
                    return true;
                }
            }
            _quantityClasses.Add(qClass);
            return true;
        }
        public bool RemoveClass(QuantityClass qClass)
        {
            if (!_quantityClasses.Contains(qClass))
            {
                return false;
            }

            _quantityClasses.Remove(qClass);
            return true;
        }

        public GeometryType GeometryType
        {
            get { return _geometryType; }
            set { _geometryType = value; }
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

        #region IFeatureRenderer Member

        public void Draw(IDisplay disp, IFeature feature)
        {
            ISymbol symbol = null;
            object od = feature[_valueField];

            bool found = false;
            if (od != null && od != DBNull.Value)
            {
                try
                {
                    double d = Convert.ToDouble(od);

                    foreach (QuantityClass qClass in _quantityClasses)
                    {
                        if (qClass == null)
                        {
                            continue;
                        }

                        if (qClass.Min <= d && qClass.Max >= d)
                        {
                            symbol = qClass.Symbol;
                            found = true;
                            break;
                        }
                    }
                }
                catch
                {
                }
            }
            if (found == false)
            {
                symbol = _defaultSymbol;
            }

            if (symbol == null)
            {
                return;
            }

            if (_symbolRotation.RotationFieldName != "")
            {
                if (symbol is ISymbolRotation)
                {
                    object rot = feature[_symbolRotation.RotationFieldName];
                    if (rot != null && rot != DBNull.Value)
                    {
                        ((ISymbolRotation)symbol).Rotation = (float)_symbolRotation.Convert2DEGAritmetic(Convert.ToDouble(rot));
                    }
                    else
                    {
                        ((ISymbolRotation)symbol).Rotation = 0;
                    }
                }
            }
            if (symbol != null)
            {
                disp.Draw(symbol, feature.Shape);
            }
        }

        public void StartDrawing(IDisplay display) { }

        public void FinishDrawing(IDisplay disp, ICancelTracker cancelTracker)
        {
        }

        public void PrepareQueryFilter(IFeatureLayer layer, IQueryFilter filter)
        {
            if (layer.FeatureClass == null)
            {
                return;
            }

            if (layer.FeatureClass.FindField(_valueField) != null)
            {
                filter.AddField(_valueField);
            }

            if (layer.FeatureClass.FindField(_symbolRotation.RotationFieldName) != null)
            {
                filter.AddField(_symbolRotation.RotationFieldName);
            }
        }

        public bool CanRender(IFeatureLayer layer, IMap map)
        {
            if (layer == null)
            {
                return false;
            }

            if (layer.FeatureClass == null)
            {
                return false;
            }
            /*
if (layer.FeatureClass.GeometryType == geometryType.Unknown ||
   layer.FeatureClass.GeometryType == geometryType.Network) return false;
* */
            if (layer.LayerGeometryType == GeometryType.Unknown ||
                layer.LayerGeometryType == GeometryType.Network)
            {
                return false;
            }

            return true;
        }

        public bool HasEffect(IFeatureLayer layer, IMap map)
        {
            return true;
        }

        public bool UseReferenceScale
        {
            get
            {
                return _useRefscale;
            }
            set
            {
                _useRefscale = value;
            }
        }

        public string Name
        {
            get { return "Quantity Renderer"; ; }
        }

        public string Category
        {
            get { return "Categories"; }
        }

        public bool RequireClone()
        {
            if (_defaultSymbol != null && _defaultSymbol.RequireClone())
            {
                return true;
            }

            return _quantityClasses?.Where(q => q?.Symbol != null && q.Symbol.RequireClone()).FirstOrDefault() != null;
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            Release();

            _valueField = (string)stream.Load("field", "");
            _defaultSymbol = (ISymbol)stream.Load("default");

            QuantityClass qClass;
            while ((qClass = (QuantityClass)stream.Load("qClass", null, new QuantityClass())) != null)
            {
                _quantityClasses.Add(qClass);
            }
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("field", _valueField);
            stream.Save("default", _defaultSymbol);

            if (_quantityClasses != null)
            {
                foreach (QuantityClass qClass in _quantityClasses)
                {
                    if (qClass != null)
                    {
                        stream.Save("qClass", qClass);
                    }
                }
            }
        }

        #endregion

        #region IClone2 Member

        public object Clone(CloneOptions options)
        {
            QuantityRenderer renderer = new QuantityRenderer();
            renderer._valueField = _valueField;
            if (_defaultSymbol != null)
            {
                renderer._defaultSymbol = (ISymbol)_defaultSymbol.Clone(_useRefscale ? options : null);
            }

            foreach (QuantityClass qClass in _quantityClasses)
            {
                if (qClass == null)
                {
                    continue;
                }

                if (qClass.Symbol == null)
                {
                    renderer._quantityClasses.Add(new QuantityClass(qClass.Min, qClass.Max, null));
                }
                else
                {
                    renderer._quantityClasses.Add(new QuantityClass(qClass.Min, qClass.Max,
                        (ISymbol)qClass.Symbol.Clone(_useRefscale ? options : null)));
                }
            }
            renderer._geometryType = _geometryType;
            renderer._symbolRotation = (SymbolRotation)_symbolRotation.Clone();
            return renderer;
        }

        public void Release()
        {
            if (_quantityClasses != null)
            {
                foreach (QuantityClass qClass in _quantityClasses)
                {
                    if (qClass != null)
                    {
                        qClass.Release();
                    }
                }
            }
            _quantityClasses.Clear();

            if (_defaultSymbol != null)
            {
                _defaultSymbol.Release();
                _defaultSymbol = null;
            }
        }

        #endregion

        #region ICreateDefault Member

        public ValueTask DefaultIfEmpty(object initObject)
        {
            if (initObject is IFeatureLayer fLayer)
            {
                if (_defaultSymbol == null && fLayer.FeatureClass is not null)
                {
                    _defaultSymbol = RendererFunctions.CreateStandardSymbol(fLayer.LayerGeometryType);
                }
            }

            return ValueTask.CompletedTask;
        }


        #endregion

        #region ILegendGroup Member

        public int LegendItemCount
        {
            get
            {
                return 1 + _quantityClasses.Count;
            }
        }

        public ILegendItem LegendItem(int index)
        {
            if (index < 0 || index >= LegendItemCount)
            {
                return null;
            }

            if (index == 0)
            {
                return _defaultSymbol;
            }
            else
            {
                QuantityClass qClass = _quantityClasses[index - 1];
                return qClass.Symbol;
            }
        }

        public void SetSymbol(ILegendItem item, ISymbol symbol)
        {
            if (item == symbol || item == null)
            {
                return;
            }

            if (item == _defaultSymbol)
            {
                if (_defaultSymbol != null)
                {
                    _defaultSymbol.Release();
                }

                _defaultSymbol = symbol;
            }
            else
            {
                foreach (QuantityClass qClass in _quantityClasses)
                {
                    if (qClass != null && qClass.Symbol == item)
                    {
                        qClass.Symbol = symbol;
                        return;
                    }
                }
            }
        }

        #endregion

        #region HelperClasses
        public class QuantityClass : IPersistable
        {
            private double _min, _max;
            private ISymbol _symbol;

            public QuantityClass()
                : this(0, 0, null)
            {
            }
            public QuantityClass(double min, double max)
                : this(min, max, null)
            {
            }
            public QuantityClass(double min, double max, ISymbol symbol)
            {
                _min = min;
                _max = max;
                _symbol = symbol;
            }

            public double Min
            {
                get { return _min; }
                set { _min = value; }
            }
            public double Max
            {
                get { return _max; }
                set { _max = value; }
            }
            public ISymbol Symbol
            {
                get { return _symbol; }
                set
                {
                    if (_symbol == value)
                    {
                        return;
                    }

                    if (_symbol != null)
                    {
                        _symbol.Release();
                    }

                    _symbol = value;
                }
            }

            public void Release()
            {
                if (_symbol != null)
                {
                    _symbol.Release();
                    _symbol = null;
                }
            }

            #region IPersistable Member

            public void Load(IPersistStream stream)
            {
                _min = (double)stream.Load("min", (double)0);
                _max = (double)stream.Load("max", (double)0);
                _symbol = (ISymbol)stream.Load("symbol", null);
            }

            public void Save(IPersistStream stream)
            {
                stream.Save("min", _min);
                stream.Save("max", _max);
                if (_symbol != null)
                {
                    stream.Save("symbol", _symbol);
                }
            }

            #endregion
        }
        #endregion

        #region IRenderer Member

        public List<ISymbol> Symbols
        {
            get
            {
                List<ISymbol> symbols = new List<ISymbol>(new ISymbol[] { _defaultSymbol });

                if (_quantityClasses != null)
                {
                    foreach (QuantityClass cls in _quantityClasses)
                    {
                        if (cls != null)
                        {
                            symbols.Add(cls.Symbol);
                        }
                    }
                }

                return symbols;
            }
        }

        public bool Combine(IRenderer renderer)
        {
            return false;
        }

        #endregion
    }
}
