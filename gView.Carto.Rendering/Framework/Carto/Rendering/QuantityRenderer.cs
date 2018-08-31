using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.Symbology;
using gView.Framework.Geometry;
using gView.Framework.Data;
using System.Reflection;
using gView.Framework.IO;
using gView.Framework.Carto.Rendering.UI;

namespace gView.Framework.Carto.Rendering
{
    [gView.Framework.system.RegisterPlugIn("9D278A19-9547-4BC5-824E-E4F45EA1BB7A")]
    public class QuantityRenderer : Cloner, IFeatureRenderer, IPropertyPage, ILegendGroup
    {
        private string _valueField = String.Empty;
        private ISymbol _defaultSymbol = null;
        private List<QuantityClass> _quantityClasses;
        private geometryType _geometryType = geometryType.Unknown;
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
                if (_valueField == value) return;
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
                    if (((ILegendItem)_defaultSymbol).LegendLabel == "") ((ILegendItem)_defaultSymbol).LegendLabel = "all other values";
                }
            }
        }

        public List<QuantityClass> QuantityClasses
        {
            get { return ListOperations<QuantityClass>.Clone(_quantityClasses); }
        }

        public bool AddClass(QuantityClass qClass)
        {
            if (qClass == null || _quantityClasses.Contains(qClass)) return false;

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
            if (!_quantityClasses.Contains(qClass)) return false;
            _quantityClasses.Remove(qClass);
            return true;
        }

        public geometryType GeometryType
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
                    _symbolRotation.RotationFieldName = "";
                else
                    _symbolRotation = value;
            }
        }

        #region IFeatureRenderer Member

        public void Draw(IDisplay disp, gView.Framework.Data.IFeature feature)
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
                        if (qClass == null) continue;
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
            if (found == false) symbol = _defaultSymbol;

            if (symbol == null) return;
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
            if (symbol != null) disp.Draw(symbol, feature.Shape);
        }

        public void FinishDrawing(IDisplay disp, ICancelTracker cancelTracker)
        {
        }

        public void PrepareQueryFilter(gView.Framework.Data.IFeatureLayer layer, gView.Framework.Data.IQueryFilter filter)
        {
            if (layer.FeatureClass == null) return;

            if (layer.FeatureClass.FindField(_valueField) != null)
                filter.AddField(_valueField);

            if (layer.FeatureClass.FindField(_symbolRotation.RotationFieldName) != null)
                filter.AddField(_symbolRotation.RotationFieldName);
        }

        public bool CanRender(gView.Framework.Data.IFeatureLayer layer, IMap map)
        {
            if (layer == null) return false;
            if (layer.FeatureClass == null) return false;
            /*
            if (layer.FeatureClass.GeometryType == geometryType.Unknown ||
                layer.FeatureClass.GeometryType == geometryType.Network) return false;
             * */
            if (layer.LayerGeometryType == geometryType.Unknown ||
                layer.LayerGeometryType == geometryType.Network) return false;
            return true;
        }

        public bool HasEffect(gView.Framework.Data.IFeatureLayer layer, IMap map)
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

        #endregion

        #region IPersistable Member

        public void Load(gView.Framework.IO.IPersistStream stream)
        {
            this.Release();

            _valueField = (string)stream.Load("field", "");
            _defaultSymbol = (ISymbol)stream.Load("default");

            QuantityClass qClass;
            while ((qClass = (QuantityClass)stream.Load("qClass", null, new QuantityClass())) != null)
            {
                _quantityClasses.Add(qClass);
            }
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            stream.Save("field", _valueField);
            stream.Save("default", _defaultSymbol);

            if (_quantityClasses != null)
            {
                foreach (QuantityClass qClass in _quantityClasses)
                {
                    if (qClass != null)
                        stream.Save("qClass", qClass);
                }
            }
        }

        #endregion

        #region IClone2 Member

        public object Clone(IDisplay display)
        {
            QuantityRenderer renderer = new QuantityRenderer();
            renderer._valueField = _valueField;
            if (_defaultSymbol != null)
                renderer._defaultSymbol = (ISymbol)_defaultSymbol.Clone(_useRefscale ? display : null);

            foreach (QuantityClass qClass in _quantityClasses)
            {
                if (qClass == null) continue;
                if (qClass.Symbol == null)
                {
                    renderer._quantityClasses.Add(new QuantityClass(qClass.Min, qClass.Max, null));
                }
                else
                {
                    renderer._quantityClasses.Add(new QuantityClass(qClass.Min, qClass.Max,
                        (ISymbol)qClass.Symbol.Clone(_useRefscale ? display : null)));
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
                        qClass.Release();
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

        #region IPropertyPage Member

        public object PropertyPage(object initObject)
        {
            if (initObject is IFeatureLayer)
            {
                IFeatureLayer layer = (IFeatureLayer)initObject;
                if (layer.FeatureClass == null) return null;

                if (_defaultSymbol == null)
                    _defaultSymbol = RendererFunctions.CreateStandardSymbol(layer.LayerGeometryType/*layer.FeatureClass.GeometryType*/);

                string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Carto.Rendering.UI.dll");

                IPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Carto.Rendering.UI.PropertyPage_QuantityRenderer") as IPropertyPanel;
                if (p != null)
                {
                    return p.PropertyPanel(this, (IFeatureLayer)initObject);
                }
            }

            return null;
        }

        public object PropertyPageObject()
        {
            return null;
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
            if (index < 0 || index >= LegendItemCount) return null;

            if (index == 0)
            {
                return (ILegendItem)_defaultSymbol;
            }
            else
            {
                QuantityClass qClass = _quantityClasses[index - 1];
                return (ILegendItem)qClass.Symbol;
            }
        }

        public void SetSymbol(ILegendItem item, ISymbol symbol)
        {
            if (item == symbol || item == null) return;

            if (item == _defaultSymbol)
            {
                if (_defaultSymbol != null) _defaultSymbol.Release();
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
                    if (_symbol == value) return;
                    if (_symbol != null)
                        _symbol.Release();

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
                    stream.Save("symbol", _symbol);
            }

            #endregion
        }
        #endregion

        #region IRenderer Member

        public List<ISymbol> Symbols
        {
            get
            {
                List<ISymbol> symbols = new List<ISymbol>(new ISymbol[]{_defaultSymbol});

                if (_quantityClasses != null)
                {
                    foreach (QuantityClass cls in _quantityClasses)
                    {
                        if (cls != null)
                            symbols.Add(cls.Symbol);
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
