using gView.Framework.Common;
using gView.Framework.Common.Collection;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.Framework.Core.Common;
using gView.Framework.Core.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.Cartography.Rendering;

[RegisterPlugIn("73117665-EFA6-4B49-BBFD-0C956876E3FB")]
public class ManyValueMapRenderer : Cloner, IFeatureRenderer, IDefault, ILegendGroup
{
    private string _valueField1 = string.Empty, _valueField2 = string.Empty, _valueField3 = string.Empty;
    private OrderedKeyValuePairs<string, ISymbol> _symbolTable = new();
    private GeometryType _geometryType = GeometryType.Unknown;
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
            ISymbol symbol = _symbolTable[key];
            if (symbol == null)
            {
                continue;
            }

            symbol.Release();
        }
        _symbolTable.Clear();
    }

    public string ValueField1
    {
        get { return _valueField1; }
        set
        {
            if (_valueField1 == value)
            {
                return;
            }

            _valueField1 = value?.Trim() ?? string.Empty;
        }
    }

    public string ValueField2
    {
        get { return _valueField2; }
        set
        {
            if (_valueField2 == value)
            {
                return;
            }

            _valueField2 = value?.Trim() ?? string.Empty;
            if (String.IsNullOrEmpty(_valueField2))
            {
                _valueField3 = string.Empty;
            }
        }
    }

    public string ValueField3
    {
        get { return _valueField3; }
        set
        {
            if (_valueField3 == value)
            {
                return;
            }

            _valueField3 = value?.Trim() ?? string.Empty;
        }
    }

    public GeometryType GeometryType
    {
        get { return _geometryType; }
        set { _geometryType = value; }
    }

    public ISymbol this[string key]
    {
        get
        {
            if (key == null)
            {
                key = ValueMapRenderer.AllOtherValuesKey;
            }

            if (!_symbolTable.ContainsKey(key))
            {
                return null;
            }

            return _symbolTable[key];
        }
        set
        {
            if (key == null)
            {
                key = ValueMapRenderer.AllOtherValuesKey;
            }

            ISymbol symbol;
            if (value == null)
            {
                symbol = RendererFunctions.CreateStandardSymbol(_geometryType);
            }
            else
            {
                if (!(value is ISymbol))
                {
                    return;
                }

                symbol = value;
            }

            if (!_symbolTable.ContainsKey(key))
            {
                _symbolTable.Add(key, symbol);
            }
            else
            {
                _symbolTable[key].Release();
                _symbolTable[key] = symbol;
            }
            if (symbol is ILegendItem)
            {
                if (string.IsNullOrEmpty(symbol.LegendLabel))
                {
                    symbol.LegendLabel =
                    key == ValueMapRenderer.AllOtherValuesKey
                        ? ValueMapRenderer.AllOtherValuesLabel
                        : key;
                }
            }
        }
    }

    public IEnumerable<string> Keys => _symbolTable.Keys.ToArray();

    public void RemoveSymbol(string key)
    {
        ISymbol symbol = _symbolTable[key];
        if (symbol == null)
        {
            return;
        }

        symbol.Release();
        _symbolTable.Remove(key);
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

    public LegendGroupCartographicMethod CartoMethod
    {
        get { return _cartoMethod; }
        set { _cartoMethod = value; }
    }

    #region IFeatureRenderer Member

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
        if (layer.FeatureClass == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(_valueField1) && layer.FeatureClass.FindField(_valueField1) != null)
        {
            filter.AddField(_valueField1);
        }

        if (!string.IsNullOrEmpty(_valueField2) && layer.FeatureClass.FindField(_valueField2) != null)
        {
            filter.AddField(_valueField2);
        }

        if (!string.IsNullOrEmpty(_valueField3) && layer.FeatureClass.FindField(_valueField3) != null)
        {
            filter.AddField(_valueField3);
        }

        if (layer.FeatureClass.FindField(_symbolRotation.RotationFieldName) != null)
        {
            filter.AddField(_symbolRotation.RotationFieldName);
        }
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
            {
                _symbol = _symbolTable[k];
            }

            _symbol = _symbol == null ? this[null] : _symbol;
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
            if (_symbol != null)
            {
                disp.Draw(_symbol, feature.Shape);
            }
        }
        else
        {
            if (_features == null)
            {
                _features = new Dictionary<string, List<IFeature>>();
            }

            if (k == null || k != null && !_symbolTable.ContainsKey(k))
            {
                k = "__gview_all_other_values__";
            }

            if (!_symbolTable.ContainsKey(k))
            {
                return;
            }

            List<IFeature> fList = null;
            if (_features.ContainsKey(k))
            {
                fList = _features[k];
            }
            else
            {
                fList = new List<IFeature>();
                _features.Add(k, fList);
            }

            fList.Add(feature);
        }
    }

    public void StartDrawing(IDisplay display) { }

    public void FinishDrawing(IDisplay disp, ICancelTracker cancelTracker)
    {
        if (cancelTracker == null)
        {
            cancelTracker = new CancelTracker();
        }

        if (_cartoMethod == LegendGroupCartographicMethod.LegendAndSymbolOrdering && cancelTracker.Continue)
        {
            int symbolIndex = 0;

            List<string> keys = new List<string>();
            foreach (string key in _symbolTable.Keys)
            {
                keys.Insert(0, key);
            }

            if (_features != null)
            {
                while (true)
                {
                    bool loop = false;
                    foreach (string key in keys)
                    {
                        if (!_features.ContainsKey(key) || _features[key] == null)
                        {
                            continue;
                        }

                        if (!cancelTracker.Continue)
                        {
                            break;
                        }

                        ISymbol symbol = _symbolTable.ContainsKey(key) ? _symbolTable[key] : null;
                        if (symbol == null)
                        {
                            continue;
                        }

                        if (symbol is ISymbolCollection)
                        {
                            ISymbolCollection symbolCol = (ISymbolCollection)symbol;
                            if (symbolIndex >= symbolCol.Symbols.Count)
                            {
                                continue;
                            }

                            if (symbolCol.Symbols.Count > symbolIndex + 1)
                            {
                                loop = true;
                            }

                            ISymbolCollectionItem symbolItem = symbolCol.Symbols[symbolIndex];
                            if (symbolItem.Visible == false || symbolItem.Symbol == null)
                            {
                                continue;
                            }

                            symbol = symbolItem.Symbol;
                        }
                        else if (symbolIndex > 0)
                        {
                            continue;
                        }

                        List<IFeature> features = _features[key];
                        bool isRotatable = symbol is ISymbolRotation;

                        if (!cancelTracker.Continue)
                        {
                            break;
                        }

                        int counter = 0;
                        foreach (IFeature feature in features)
                        {
                            if (isRotatable && !string.IsNullOrEmpty(_symbolRotation.RotationFieldName))
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
                            {
                                break;
                            }
                        }
                    }

                    if (!loop)
                    {
                        break;
                    }

                    symbolIndex++;
                }
            }
        }
        else if (_cartoMethod == LegendGroupCartographicMethod.LegendOrdering && cancelTracker.Continue)
        {
            List<string> keys = new List<string>();
            foreach (string key in _symbolTable.Keys)
            {
                keys.Insert(0, key);
            }

            foreach (string key in keys)
            {
                if (!_features.ContainsKey(key) || _features[key] == null)
                {
                    continue;
                }

                if (!cancelTracker.Continue)
                {
                    break;
                }

                ISymbol symbol = _symbolTable.ContainsKey(key) ? _symbolTable[key] : null;
                if (symbol == null)
                {
                    continue;
                }

                List<IFeature> features = _features[key];
                bool isRotatable = symbol is ISymbolRotation;

                int counter = 0;
                foreach (IFeature feature in features)
                {
                    if (isRotatable && !string.IsNullOrEmpty(_symbolRotation.RotationFieldName))
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
                    {
                        break;
                    }
                }
            }
        }

        if (_features != null)
        {
            _features.Clear();
            _features = null;
        }
    }

    public bool RequireClone()
    {
        if (_symbolTable != null)
        {
            foreach (var symbol in _symbolTable.Values)
            {
                if (symbol != null && symbol.RequireClone())
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region IPersistable Member

    public void Load(IPersistStream stream)
    {
        ValueField1 = (string)stream.Load("field1", "");
        ValueField2 = (string)stream.Load("field2", "");
        ValueField3 = (string)stream.Load("field3", "");
        // Kompatibilität zu äteren Projekten
        ISymbol defSymbol = (ISymbol)stream.Load("default", null);
        if (defSymbol != null)
        {
            this[null] = defSymbol;
        }

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
            ValueMapRendererSymbol sym = new ValueMapRendererSymbol(key, _symbolTable[key]);
            stream.Save("ValueMapSymbol", sym);
        }
        if (_symbolRotation.RotationFieldName != "")
        {
            stream.Save("SymbolRotation", _symbolRotation);
        }
    }

    #endregion

    #region ICreateDefault Member

    public ValueTask DefaultIfEmpty(object initObject)
    {
        if (initObject is IFeatureLayer fLayer)
        {
            if (_symbolTable.Count == 0 && fLayer.FeatureClass is not null)
            {
                this[null] = RendererFunctions.CreateStandardSymbol(fLayer.LayerGeometryType);
            }
        }

        return ValueTask.CompletedTask;
    }

    #endregion

    #region ILegendGroup Members

    public int LegendItemCount
    {
        get { return /*1 + */_symbolTable.Count; }
    }

    public ILegendItem LegendItem(int index)
        => _symbolTable.ValueAtOrDefault(index);

    public void SetSymbol(ILegendItem item, ISymbol symbol)
    {
        if (item == symbol)
        {
            return;
        }

        foreach (string key in _symbolTable.Keys)
        {
            if (!(_symbolTable[key] is ILegendItem))
            {
                continue;
            }

            if (_symbolTable[key] == item)
            {
                if (symbol is ILegendItem)
                {
                    symbol.LegendLabel = item.LegendLabel;
                }

                _symbolTable[key] = symbol;
                return;
            }
        }
    }
    
    #endregion

    #region IClone2
    public object Clone(CloneOptions options)
    {
        ManyValueMapRenderer renderer = new ManyValueMapRenderer();
        renderer._valueField1 = _valueField1;
        renderer._valueField2 = _valueField2;
        renderer._valueField3 = _valueField3;
   
        foreach (string key in _symbolTable.Keys)
        {
            ISymbol symbol = _symbolTable[key];
            if (symbol != null)
            {
                symbol = (ISymbol)symbol.Clone(_useRefscale ? options : null);
            }

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
        {
            return null;
        }

        StringBuilder sb = new StringBuilder();

        if (!string.IsNullOrEmpty(_valueField1))
        {
            object o = feature[_valueField1];
            if (o != null)
            {
                sb.Append(o.ToString());
            }
        }
        if (!string.IsNullOrEmpty(_valueField2))
        {
            sb.Append("|");
            object o = feature[_valueField2];
            if (o != null)
            {
                sb.Append(o.ToString());
            }
        }
        if (!string.IsNullOrEmpty(_valueField3))
        {
            sb.Append("|");
            object o = feature[_valueField3];
            if (o != null)
            {
                sb.Append(o.ToString());
            }
        }

        return sb.ToString();
    }

    #endregion
}
