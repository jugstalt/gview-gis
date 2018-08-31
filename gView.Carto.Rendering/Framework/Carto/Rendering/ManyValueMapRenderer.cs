using System;
using System.Collections;
using System.Collections.Generic;
using gView.Framework;
using gView.Framework.Carto.Rendering;
using gView.Framework.Symbology;
using gView.Framework.Geometry;
using gView.Framework.Data;
using gView.Framework.UI;
using gView.Framework.IO;
using gView.Framework.system;
using System.Reflection;
using gView.Framework.Carto.Rendering.UI;
using System.Text;

namespace gView.Framework.Carto.Rendering
{
    [gView.Framework.system.RegisterPlugIn("73117665-EFA6-4B49-BBFD-0C956876E3FB")]
    public class ManyValueMapRenderer : Cloner, IFeatureRenderer, IPropertyPage, ILegendGroup
    {
        private string _valueField1 = String.Empty, _valueField2 = String.Empty, _valueField3 = String.Empty;
        private Dictionary<string, ISymbol> _symbolTable = new Dictionary<string, ISymbol>();
        //private ISymbol _defaultSymbol = null;
        private geometryType _geometryType = geometryType.Unknown;
        private SymbolRotation _symbolRotation;
        private bool _useRefscale = true;
        private LegendGroupCartographicMethod _cartoMethod = LegendGroupCartographicMethod.Simple;
        private Dictionary<string, List<IFeature>> _features = null;

        public ManyValueMapRenderer()
        {
            _symbolRotation = new SymbolRotation();
        }

        public void Dispose()
        {
            foreach (string key in _symbolTable.Keys)
            {
                ISymbol symbol = (ISymbol)_symbolTable[key];
                if (symbol == null) continue;
                symbol.Release();
            }
            _symbolTable.Clear();
        }

        public string ValueField1
        {
            get { return _valueField1; }
            set
            {
                if (_valueField1 == value) return;

                _valueField1 = value == "<none>" ? String.Empty : value;
            }
        }

        public string ValueField2
        {
            get { return _valueField2; }
            set
            {
                if (_valueField2 == value) return;

                _valueField2 = value == "<none>" ? String.Empty : value;
            }
        }

        public string ValueField3
        {
            get { return _valueField3; }
            set
            {
                if (_valueField3 == value) return;

                _valueField3 = value == "<none>" ? String.Empty : value;
            }
        }

        public ISymbol DefaultSymbol
        {
            get { return this[null]; }
            set
            {
                this[null] = value;
                if (value is ILegendItem)
                {
                    if (((ILegendItem)value).LegendLabel == "") ((ILegendItem)value).LegendLabel = "all other values";
                }
            }
        }

        public geometryType GeometryType
        {
            get { return _geometryType; }
            set { _geometryType = value; }
        }

        public ISymbol this[string key]
        {
            get
            {
                if (key == null)
                    key = "__gview_all_other_values__";

                if (!_symbolTable.ContainsKey(key)) return null;
                return (ISymbol)_symbolTable[key];
            }
            set
            {
                if (key == null)
                    key = "__gview_all_other_values__";

                ISymbol symbol;
                if (value == null)
                {
                    symbol = RendererFunctions.CreateStandardSymbol(_geometryType);
                }
                else
                {
                    if (!(value is ISymbol)) return;
                    symbol = value;
                }

                if (!_symbolTable.ContainsKey(key))
                {
                    _symbolTable.Add(key, symbol);
                }
                else
                {
                    ((ISymbol)_symbolTable[key]).Release();
                    _symbolTable[key] = symbol;
                }
                if (symbol is ILegendItem)
                {
                    if (String.IsNullOrEmpty(((ILegendItem)symbol).LegendLabel)) ((ILegendItem)symbol).LegendLabel =
                        (key == "__gview_all_other_values__") ? "all other values" : key;
                }
            }
        }

        public ICollection Keys
        {
            get
            {
                return _symbolTable.Keys;
            }
        }

        public void RemoveSymbol(string key)
        {
            ISymbol symbol = (ISymbol)_symbolTable[key];
            if (symbol == null) return;
            symbol.Release();
            _symbolTable.Remove(key);
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

        public LegendGroupCartographicMethod CartoMethod
        {
            get { return _cartoMethod; }
            set { _cartoMethod = value; }
        }

        public void ReorderLegendItems(string[] keys)
        {
            if (keys == null)
                return;

            Dictionary<string, ISymbol> orderedSymbolTable = new Dictionary<string, ISymbol>();
            foreach (string key in keys)
            {
                if (_symbolTable.ContainsKey(key))
                    orderedSymbolTable.Add(key, _symbolTable[key]);
            }

            foreach (string key in _symbolTable.Keys)
            {
                if (!orderedSymbolTable.ContainsKey(key))
                    orderedSymbolTable.Add(key, _symbolTable[key]);
            }

            _symbolTable = orderedSymbolTable;
        }

        #region IFeatureRenderer Member

        public bool CanRender(IFeatureLayer layer, IMap map)
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

        public bool HasEffect(IFeatureLayer layer, IMap map)
        {
            return true;
        }

        public bool UseReferenceScale
        {
            get { return _useRefscale; }
            set { _useRefscale = value; }
        }

        public string Name
        {
            get
            {
                return "Many Value Map Renderer";
            }
        }

        public void PrepareQueryFilter(IFeatureLayer layer, IQueryFilter filter)
        {
            if (layer.FeatureClass == null) return;

            if (!String.IsNullOrEmpty(_valueField1) && layer.FeatureClass.FindField(_valueField1) != null)
                filter.AddField(_valueField1);
            if (!String.IsNullOrEmpty(_valueField2) && layer.FeatureClass.FindField(_valueField2) != null)
                filter.AddField(_valueField2);
            if (!String.IsNullOrEmpty(_valueField3) && layer.FeatureClass.FindField(_valueField3) != null)
                filter.AddField(_valueField3);

            if (layer.FeatureClass.FindField(_symbolRotation.RotationFieldName) != null)
                filter.AddField(_symbolRotation.RotationFieldName);
        }

        public string Category
        {
            get
            {
                return "Categories";
            }
        }

        public void Draw(IDisplay disp, IFeature feature)
        {
            string k = GetKey(feature);

            if (_cartoMethod == LegendGroupCartographicMethod.Simple)
            {
                ISymbol _symbol = null;
                if (k != null && _symbolTable.ContainsKey(k))
                    _symbol = (ISymbol)_symbolTable[k];
                
                _symbol = (_symbol == null) ? this[null] : _symbol;
                if (_symbolRotation.RotationFieldName != "")
                {
                    if (_symbol is ISymbolRotation)
                    {
                        object rot = feature[_symbolRotation.RotationFieldName];
                        if (rot != null && rot != DBNull.Value)
                        {
                            ((ISymbolRotation)_symbol).Rotation = (float)_symbolRotation.Convert2DEGAritmetic(Convert.ToDouble(rot));
                        }
                        else
                        {
                            ((ISymbolRotation)_symbol).Rotation = 0;
                        }
                    }
                }
                if (_symbol != null) disp.Draw(_symbol, feature.Shape);
            }
            else
            {
                if (_features == null)
                    _features = new Dictionary<string, List<IFeature>>();

                if (k == null || (k != null && !_symbolTable.ContainsKey(k)))
                    k = "__gview_all_other_values__";

                if (!_symbolTable.ContainsKey(k))
                    return;

                List<IFeature> fList = null;
                if (_features.ContainsKey(k))
                    fList = _features[k];
                else
                {
                    fList = new List<IFeature>();
                    _features.Add(k, fList);
                }

                fList.Add(feature);
            }
        }

        public void FinishDrawing(IDisplay disp, ICancelTracker cancelTracker)
        {
            if (cancelTracker == null) cancelTracker = new CancelTracker();

            if (_cartoMethod == LegendGroupCartographicMethod.LegendAndSymbolOrdering && cancelTracker.Continue)
            {
                int symbolIndex = 0;

                List<string> keys = new List<string>();
                foreach (string key in _symbolTable.Keys)
                    keys.Insert(0, key);

                if (_features != null)
                {
                    while (true)
                    {
                        bool loop = false;
                        foreach (string key in keys)
                        {
                            if (!_features.ContainsKey(key) || _features[key] == null)
                                continue;
                            if (!cancelTracker.Continue)
                                break;

                            ISymbol symbol = _symbolTable.ContainsKey(key) ? (ISymbol)_symbolTable[key] : null;
                            if (symbol == null) continue;

                            if (symbol is ISymbolCollection)
                            {
                                ISymbolCollection symbolCol = (ISymbolCollection)symbol;
                                if (symbolIndex >= symbolCol.Symbols.Count)
                                    continue;

                                if (symbolCol.Symbols.Count > symbolIndex + 1)
                                    loop = true;

                                ISymbolCollectionItem symbolItem = symbolCol.Symbols[symbolIndex];
                                if (symbolItem.Visible == false || symbolItem.Symbol == null)
                                    continue;
                                symbol = symbolItem.Symbol;
                            }
                            else if (symbolIndex > 0)
                            {
                                continue;
                            }

                            List<IFeature> features = _features[key];
                            bool isRotatable = symbol is ISymbolRotation;

                            if (!cancelTracker.Continue) break;
                            int counter = 0;
                            foreach (IFeature feature in features)
                            {
                                if (isRotatable && !String.IsNullOrEmpty(_symbolRotation.RotationFieldName))
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
                                symbol.Draw(disp, feature.Shape);

                                counter++;
                                if (counter % 100 == 0 && !cancelTracker.Continue)
                                    break;
                            }
                        }

                        if (!loop)
                            break;
                        symbolIndex++;
                    }
                }
            }
            else if (_cartoMethod == LegendGroupCartographicMethod.LegendOrdering && cancelTracker.Continue)
            {
                List<string> keys = new List<string>();
                foreach (string key in _symbolTable.Keys)
                    keys.Insert(0, key);

                foreach (string key in keys)
                {
                    if (!_features.ContainsKey(key) || _features[key] == null)
                        continue;
                    if (!cancelTracker.Continue)
                        break;

                    ISymbol symbol = _symbolTable.ContainsKey(key) ? (ISymbol)_symbolTable[key] : null;
                    if (symbol == null) continue;

                    List<IFeature> features = _features[key];
                    bool isRotatable = symbol is ISymbolRotation;

                    int counter = 0;
                    foreach (IFeature feature in features)
                    {
                        if (isRotatable && !String.IsNullOrEmpty(_symbolRotation.RotationFieldName))
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
                        symbol.Draw(disp, feature.Shape);

                        counter++;
                        if (counter % 100 == 0 && !cancelTracker.Continue)
                            break;
                    }
                }
            }

            if (_features != null)
            {
                _features.Clear();
                _features = null;
            }
        }
        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            this.ValueField1 = (string)stream.Load("field1", "");
            this.ValueField2 = (string)stream.Load("field2", "");
            this.ValueField3 = (string)stream.Load("field3", "");
            // Kompatibilität zu äteren Projekten
            ISymbol defSymbol = (ISymbol)stream.Load("default", null);
            if (defSymbol != null)
                this[null] = defSymbol;

            _cartoMethod = (LegendGroupCartographicMethod)stream.Load("CartographicMethod", (int)LegendGroupCartographicMethod.Simple);

            ValueMapRendererSymbol sym;
            while ((sym = (ValueMapRendererSymbol)stream.Load("ValueMapSymbol", null, new ValueMapRendererSymbol())) != null)
            {
                this[sym._key] = sym._symbol;
            }
            _symbolRotation = (SymbolRotation)stream.Load("SymbolRotation", _symbolRotation, _symbolRotation);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("field1", _valueField1);
            stream.Save("field2", _valueField2);
            stream.Save("field3", _valueField3);
            //stream.Save("default", _defaultSymbol);
            stream.Save("CartographicMethod", (int)_cartoMethod);

            foreach (string key in _symbolTable.Keys)
            {
                ValueMapRendererSymbol sym = new ValueMapRendererSymbol(key, (ISymbol)_symbolTable[key]);
                stream.Save("ValueMapSymbol", sym);
            }
            if (_symbolRotation.RotationFieldName != "")
            {
                stream.Save("SymbolRotation", _symbolRotation);
            }
        }

        #endregion

        #region IPropertyPage Member

        public object PropertyPageObject()
        {
            return null;
        }

        public object PropertyPage(object initObject)
        {
            if (initObject is IFeatureLayer)
            {
                IFeatureLayer layer = (IFeatureLayer)initObject;
                if (layer.FeatureClass == null) return null;

                if (_symbolTable.Count == 0)
                    this[null] = RendererFunctions.CreateStandardSymbol(layer.LayerGeometryType/*layer.FeatureClass.GeometryType*/);

                string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Carto.Rendering.UI.dll");

                IPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Carto.Rendering.UI.PropertyPage_ManyValueMapRenderer") as IPropertyPanel;
                if (p != null)
                {
                    return p.PropertyPanel(this, (IFeatureLayer)initObject);
                }
            }

            return null;
        }

        #endregion

        #region ILegendGroup Members

        public int LegendItemCount
        {
            get { return 1 + _symbolTable.Count; }
        }

        public ILegendItem LegendItem(int index)
        {
            if (index < 0) return null;
            //if (index == 0) return (ILegendItem)_defaultSymbol;
            if (index <= _symbolTable.Count)
            {
                int count = 0;
                foreach (object lItem in _symbolTable.Values)
                {
                    if (count == index && lItem is ILegendItem) return (LegendItem)lItem;
                    count++;
                }
            }
            return null;
        }

        public void SetSymbol(ILegendItem item, ISymbol symbol)
        {
            if (item == symbol) return;

            //if (item == _defaultSymbol)
            //{
            //    _defaultSymbol.Release();
            //    _defaultSymbol = symbol;
            //}
            //else
            {
                foreach (string key in _symbolTable.Keys)
                {
                    if (!(_symbolTable[key] is ILegendItem)) continue;

                    if (_symbolTable[key] == item)
                    {
                        if (symbol is ILegendItem)
                            ((ILegendItem)symbol).LegendLabel = item.LegendLabel;

                        _symbolTable[key] = symbol;
                        return;
                    }
                }
            }
        }
        #endregion

        #region IClone2
        public object Clone(IDisplay display)
        {
            ManyValueMapRenderer renderer = new ManyValueMapRenderer();
            renderer._valueField1 = _valueField1;
            renderer._valueField2 = _valueField2;
            renderer._valueField3 = _valueField3;
            //if (_defaultSymbol != null)
            //    renderer._defaultSymbol = (ISymbol)_defaultSymbol.Clone(_useRefscale ? display : null);
            foreach (string key in _symbolTable.Keys)
            {
                ISymbol symbol = (ISymbol)_symbolTable[key];
                if (symbol != null) symbol = (ISymbol)symbol.Clone(_useRefscale ? display : null);
                renderer._symbolTable.Add(key, symbol);
            }
            renderer._geometryType = _geometryType;
            renderer._symbolRotation = (SymbolRotation)_symbolRotation.Clone();
            renderer._cartoMethod = _cartoMethod;
            return renderer;
        }
        public void Release()
        {
            Dispose();
        }
        #endregion

        #region IRenderer Member

        public List<ISymbol> Symbols
        {
            get
            {
                return new List<ISymbol>(_symbolTable.Values);
            }
        }

        public bool Combine(IRenderer renderer)
        {
            return false;
        }
        #endregion

        #region Helper

        public string GetKey(IFeature feature)
        {
            if (feature == null)
                return null;

            StringBuilder sb = new StringBuilder();

            if (!String.IsNullOrEmpty(_valueField1))
            {
                object o = feature[_valueField1];
                if (o != null) sb.Append(o.ToString());
            }
            if (!String.IsNullOrEmpty(_valueField2))
            {
                sb.Append("|");
                object o = feature[_valueField2];
                if (o != null) sb.Append(o.ToString());
            }
            if (!String.IsNullOrEmpty(_valueField3))
            {
                sb.Append("|");
                object o = feature[_valueField3];
                if (o != null) sb.Append(o.ToString());
            }

            return sb.ToString();
        }

        #endregion
    }
}
