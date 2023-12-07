using gView.Framework.Common;
using gView.Framework.Common.Collection;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.Framework.Core.system;
using gView.Framework.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Framework.Cartography.Rendering;

public enum LegendGroupCartographicMethod { Simple = 0, LegendOrdering = 1, LegendAndSymbolOrdering = 2 }

[RegisterPlugIn("C7A92674-0120-4f3d-BC03-F1210136B5C6")]
public class ValueMapRenderer : Cloner, IFeatureRenderer, IDefault, ILegendGroup
{
    public const string AllOtherValuesKey = "__gview_all_other_values__";
    public const string AllOtherValuesLabel = "All other values";

    private string _valueField = string.Empty;
    private OrderedKeyValuePairs<string, ISymbol> _symbolTable = new();
    private GeometryType _geometryType = GeometryType.Unknown;
    private SymbolRotation _symbolRotation;
    private bool _useRefscale = true;
    private LegendGroupCartographicMethod _cartoMethod = LegendGroupCartographicMethod.Simple;
    private Dictionary<string, List<IFeature>> _features = null;

    public ValueMapRenderer()
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
        get { return this[null]; }
        set
        {
            this[null] = value;
            if (value is ILegendItem)
            {
                if (value.LegendLabel == "")
                {
                    value.LegendLabel = AllOtherValuesLabel;
                }
            }
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
                key = AllOtherValuesKey;
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
                key = AllOtherValuesKey;
            }

            ISymbol symbol;
            if (value == null)
            {
                symbol = RendererFunctions.CreateStandardSymbol(_geometryType, 76, 3, 7);
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
                _symbolTable[key]?.Release();
                _symbolTable[key] = symbol;
            }
            if (symbol is ILegendItem)
            {
                if (string.IsNullOrEmpty(symbol.LegendLabel))
                {
                    symbol.LegendLabel =
                    key == AllOtherValuesKey
                        ? AllOtherValuesLabel
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
            return "Value Map Renderer";
        }
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

    public string Category
    {
        get
        {
            return "Categories";
        }
    }

    public void Draw(IDisplay disp, IFeature feature)
    {
        object value = feature[_valueField];
        string key = value != null ? value.ToString() : null; //feature[_valueField].ToString(); //((FieldValue)feature.Fields[0]).Value.ToString();

        if (_cartoMethod == LegendGroupCartographicMethod.Simple)
        {
            ISymbol _symbol = null;
            if (key != null && _symbolTable.ContainsKey(key))
            {
                _symbol = _symbolTable[key];
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

            if (key == null || key != null && !_symbolTable.ContainsKey(key))
            {
                key = AllOtherValuesKey;
            }

            if (!_symbolTable.ContainsKey(key))
            {
                return;
            }

            List<IFeature> fList = null;
            if (_features.ContainsKey(key))
            {
                fList = _features[key];
            }
            else
            {
                fList = new List<IFeature>();
                _features.Add(key, fList);
            }

            fList.Add(feature);
        }
    }

    public void StartDrawing(IDisplay display)
    {

    }

    public void FinishDrawing(IDisplay disp, ICancelTracker cancelTracker)
    {
        if (cancelTracker == null)
        {
            cancelTracker = new CancelTracker();
        }

        if (_features != null)
        {
            if (_cartoMethod == LegendGroupCartographicMethod.LegendAndSymbolOrdering && cancelTracker.Continue)
            {
                int symbolIndex = 0;

                List<string> keys = new List<string>();
                foreach (string key in _symbolTable.Keys)
                {
                    keys.Insert(0, key);
                }

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

                            disp.Draw(symbol, feature.Shape);

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
                        disp.Draw(symbol, feature.Shape);

                        counter++;
                        if (counter % 100 == 0 && !cancelTracker.Continue)
                        {
                            break;
                        }
                    }
                }
            }

            _features.Clear();
            _features = null;
        }
    }
    #endregion

    #region IPersistable Member

    public void Load(IPersistStream stream)
    {
        _valueField = (string)stream.Load("field", "");
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
        stream.Save("field", _valueField);
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

            if (_geometryType == GeometryType.Unknown)
            {
                _geometryType = fLayer.LayerGeometryType;
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
        ValueMapRenderer renderer = new ValueMapRenderer();

        renderer._valueField = _valueField;
        
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

    #region IRenderRequiresClone

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
}

internal class ValueMapRendererSymbol : IPersistable
{
    public string _key;
    public ISymbol _symbol;

    public ValueMapRendererSymbol()
    {
    }
    public ValueMapRendererSymbol(string key, ISymbol symbol)
    {
        _key = key;
        _symbol = symbol;
    }

    #region IPersistable Member

    public string PersistID
    {
        get
        {
            return null;
        }
    }

    public void Load(IPersistStream stream)
    {
        _key = (string)stream.Load("key");
        if (_key == ValueMapRenderer.AllOtherValuesKey)
        {
            _key = null;
        }

        _symbol = (ISymbol)stream.Load("symbol");
    }

    public void Save(IPersistStream stream)
    {
        if (_key == null)
        {
            stream.Save("key", ValueMapRenderer.AllOtherValuesKey);
        }
        else
        {
            stream.Save("key", _key);
        }

        stream.Save("symbol", _symbol);
    }

    #endregion
}
