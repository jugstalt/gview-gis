using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.Framework.Symbology.Vtc.Extensions;
using gView.GraphicsEngine;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace gView.Framework.Symbology.Vtc;

public class PaintSymbol : ISymbol, IBrushColor
{
    private ILineSymbol? _lineSymbol;
    private IFillSymbol? _fillSymbol;
    private IPointSymbol? _pointSymbol;
    private ITextSymbol? _textSymbol;

    private Dictionary<string, IValueFunc> _valueFuncs = new();

    public PaintSymbol()
    {
    }

    public PaintSymbol(
            IPointSymbol? pointSymbol,
            ILineSymbol? lineSymbol,
            IFillSymbol? fillSymbol,
            ITextSymbol? textSymbol
         )
    {
        _pointSymbol = pointSymbol;
        _lineSymbol = lineSymbol;
        _fillSymbol = fillSymbol;
        _textSymbol = textSymbol;
    }

    internal Dictionary<string, IValueFunc> ValueFuncs => _valueFuncs;

    #region ISymbol

    public string Name => "Vtc Paint Symbol";
    public SymbolSmoothing SymbolSmoothingMode { get; set; }
    public string LegendLabel { get; set; } = "";
    public bool ShowInTOC { get; set; } = false;

    public int IconHeight => 0;

    public object Clone()
    {
        var clone = new PaintSymbol(
            _pointSymbol?.Clone() as IPointSymbol,
            _lineSymbol?.Clone() as ILineSymbol,
            _fillSymbol?.Clone() as IFillSymbol,
            _textSymbol?.Clone() as ITextSymbol);

        clone._valueFuncs = _valueFuncs;

        return clone;
    }

    public object Clone(CloneOptions options)
    {
        var clone = new PaintSymbol(
            _pointSymbol?.Clone(options) as IPointSymbol,
            _lineSymbol?.Clone(options) as ILineSymbol,
            _fillSymbol?.Clone(options) as IFillSymbol,
            _textSymbol?.Clone(options) as ITextSymbol);

        clone._valueFuncs = _valueFuncs;

        return clone;
    }

    public void Draw(IDisplay display, IGeometry geometry)
    {

    }

    public void Draw(IDisplay display, IFeature feature, bool modify = true)
    {
        var geometry = feature?.Shape;

        if (geometry is IPoint && _pointSymbol != null)
        {
            if (modify)
            {
                _pointSymbol.ModifyStyles(ValueFuncs, display, feature);
            }
            display.Draw(_pointSymbol, geometry);
        }
        else
        if (geometry is IPolyline && _lineSymbol != null)
        {
            if (modify)
            {
                _lineSymbol.ModifyStyles(_valueFuncs, display, feature);
            }

            display.Draw(_lineSymbol, geometry);
        }
        else if (geometry is IPolygon && _fillSymbol != null)
        {
            if (modify)
            {
                _fillSymbol.ModifyStyles(_valueFuncs, display, feature);
            }

            display.Draw(_fillSymbol, geometry);
        }
        else if (_textSymbol != null)
        {

        }
        else
        {
            // Todo: Exception? Warnung
        }
    }

    public void Load(IPersistStream stream)
    {
        _pointSymbol = stream.Load("PointSymbol", null) as IPointSymbol;
        _lineSymbol = stream.Load("LineSymbol", null) as ILineSymbol;
        _fillSymbol = stream.Load("FillSymbol", null) as IFillSymbol;
    }

    public void Save(IPersistStream stream)
    {
        if (_pointSymbol != null)
        {
            stream.Save("PointSymbol", _pointSymbol);
        }

        if (_lineSymbol != null)
        {
            stream.Save("LineSymbol", _lineSymbol);
        }

        if (_fillSymbol != null)
        {
            stream.Save("PolygonSymbol", _pointSymbol);
        }
    }

    public void Release()
    {
        _pointSymbol?.Release();
        _lineSymbol?.Release();
        _fillSymbol?.Release();
        _textSymbol?.Release();

        _pointSymbol = null;
        _lineSymbol = null;
        _fillSymbol = null;
        _textSymbol = null;
    }

    public bool RequireClone() =>
        _pointSymbol?.RequireClone() == true
        || _lineSymbol?.RequireClone() == true
        || _fillSymbol?.RequireClone() == true;

    public bool SupportsGeometryType(GeometryType type) => true;

    #endregion

    public bool IsLabelSymbol() => _textSymbol != null;

    public ITextSymbol? TextSymbol => _textSymbol;

    #region IBrushColor

    public ArgbColor FillColor
    {
        get
        {
            if (_fillSymbol is IBrushColor brushColor)
            {
                return brushColor.FillColor;
            }

            return ArgbColor.Empty;
        }
        set
        {
            if (_fillSymbol is IBrushColor brushColor)
            {
                brushColor.FillColor = value;
            }
        }
    }

    #endregion

    public void AddValueFunc(string key, IValueFunc valueFunc)
    {
        _valueFuncs[key] = valueFunc;
    }
}

public interface IValueFunc
{
    T? Value<T>(IDisplay display, IFeature? feature = null);
}

public class StopsValueFunc : IValueFunc
{
    private KeyValuePair<float, object>[] _stops = [];

    public void AddStop(float scale, object value)
    {
        var list = new List<KeyValuePair<float, object>>(_stops)
        {
            new KeyValuePair<float, object>(scale, value)
        };

        _stops = list.OrderBy(x => x.Key).ToArray();
    }

    public IValueFunc? BaseValueFunc { get; set; } = null;

    public T? Value<T>(IDisplay display, IFeature? feature = null)
    {
        if (_stops.Length == 0)
        {
            return default(T);
        }

        return (this.BaseValueFunc, typeof(T)) switch
        {
            (IValueFunc, Type t) when t == typeof(float) =>
                (T)Convert.ChangeType(
                    CalcProgressiveStopsValue(
                                display,
                                feature,
                                this.BaseValueFunc.Value<float>(display, feature)
                            ),
                typeof(T)),
            (_, _) => CalcStopsValue<T>(display, feature)
        };
    }

    private T CalcStopsValue<T>(IDisplay display, IFeature? feature = null)
    {
        if (typeof(T) == typeof(float))
        {
            return (T)(object)CalcProgressiveStopsValue(display, feature, 1f);
        }

        for (int i = 1; i < _stops.Length; i++)
        {
            if (_stops[i].Key > display.WebMercatorScaleLevel)
            {
                return (T)Convert.ChangeType(_stops[i - 1].Value, typeof(T));
            }
        }

        return (T)Convert.ChangeType(_stops.Last().Value, typeof(T));
    }

    private float CalcProgressiveStopsValue(IDisplay display, IFeature? feature, float @base)
    {
        float? val = null;

        if (@base > 1f) @base *= 4.5f;  // empric !?

        if (display.WebMercatorScaleLevel <= _stops[0].Key)
        {
            val = (float)Convert.ChangeType(_stops[0].Value, typeof(float));
        }
        else if (display.WebMercatorScaleLevel >= _stops.Last().Key)
        {
            val = (float)Convert.ChangeType(_stops.Last().Value, typeof(float));
        }
        else
        {
            for (int i = 1; i < _stops.Length; i++)
            {
                if (_stops[i].Key > display.WebMercatorScaleLevel)
                {
                    float val1 = (float)Convert.ChangeType(_stops[i - 1].Value, typeof(float));
                    float val2 = (float)Convert.ChangeType(_stops[i].Value, typeof(float));

                    float key1 = _stops[i - 1].Key;
                    float key2 = _stops[i].Key;

                    val = val1 + (val2 - val1)
                        * (float)Math.Pow((display.WebMercatorScaleLevel - key1) / (key2 - key1), @base);

                    break;
                }
            }
        }

        return val!.Value;
    }
}

public class CaseValueFunc : IValueFunc
{
    private IValueFunc? _defaultValueFunc;
    private List<(string field, IValueFunc comparisionOperatorFunc, string? comparisionValue, IValueFunc resultValueFunc)> _values = new();

    public void AddCase(string field, IValueFunc comparisionOperatorFunc, object comparisionValue, IValueFunc resultValueFunc)
    {
        _values.Add((field, comparisionOperatorFunc, comparisionValue?.ToString(), resultValueFunc));
    }

    public void SetDefaultValue(IValueFunc defaultValue)
    {
        _defaultValueFunc = defaultValue;
    }

    public T? Value<T>(IDisplay display, IFeature? feature = null)
    {
        if (feature != null)
        {
            foreach (var caseValue in _values)
            {
                var featureValue = feature[caseValue.field]?.ToString();
                if (featureValue == null) continue;

                bool fitCondition = false;
                if (caseValue.comparisionOperatorFunc is LiteralValueFunc)
                {
                    var @operator = caseValue.comparisionOperatorFunc.Value<string>(display, feature); 
                    fitCondition = @operator switch
                    {
                        "==" => featureValue == caseValue.comparisionValue,
                        "!=" => featureValue != caseValue.comparisionValue,
                        _ => throw new ArgumentException($"Unknown case operator '{@operator}'")
                    };
                }
                else
                {
                    fitCondition = caseValue.comparisionOperatorFunc.Value<bool>(display, feature);
                }

                if (fitCondition)
                {
                    return (T?)caseValue.resultValueFunc.Value<T>(display, feature);
                }
            }
        }

        return (T?)_defaultValueFunc == null
            ? default(T)
            : _defaultValueFunc.Value<T>(display, feature);
    }
}

public class ConcatValuesFunc : IValueFunc
{
    private List<IValueFunc> _valueFuncs = new();

    public void Add(IValueFunc valueFunc) => _valueFuncs.Add(valueFunc);

    public T? Value<T>(IDisplay display, IFeature? feature = null)
    {
        StringBuilder result = new StringBuilder();

        foreach(var valueFunc in  _valueFuncs)
        {
            result.Append(valueFunc.Value<string>(display, feature));
        }

        return (T?)Convert.ChangeType(result.ToString(), typeof(T));
    }
}

public class InterpolateValueFunc : IValueFunc
{
    private string[] _methods;  // linar, exp
    private string[] _fields;  // zoom,
    private IValueFunc[] _funcs;

    public InterpolateValueFunc(string[] methods, string[] fields, IValueFunc[] funcs)
    {
        _methods = methods;
        _fields = fields;
        _funcs = funcs;

        if (_funcs.Length % 2 != 0) throw new ArgumentException("Invalid Interpolation Function: number of funcs must be even");
    }

    public T? Value<T>(IDisplay display, IFeature? feature = null)
    {
        if (_fields.Length != 1)
            throw new Exception("Invalid Interpolation Function: only on field/zoom implemented");

        var pointValue = _fields[0] == "zoom"
            ? display.WebMercatorScaleLevel
            : Convert.ToSingle(feature?[_fields[0]] ?? 0);

        T? result = default;

        for (int i = 0; i < _funcs.Length - 1; i += 2)
        {
            var funcPoint1Value = _funcs[i].Value<float>(display, feature);

            if (i == 0 && funcPoint1Value > pointValue)  // take first value
            {
                var funcValue = _funcs[i + 1].Value<T>(display, feature);

                return funcValue;
            }

            if (funcPoint1Value <= pointValue)
            {
                var func1Value = _funcs[i + 1].Value<T>(display, feature);

                if (i + 3 >= _funcs.Length)
                {
                    result = func1Value;
                    continue;
                }

                var funcPoint2Value = _funcs[i + 2].Value<float>(display, feature);
                var func2Value = _funcs[i + 3].Value<T>(display, feature);

                if (_methods[0] == "linear")
                {
                    var factor = (pointValue - funcPoint1Value) / (funcPoint2Value - funcPoint1Value);

                    if (typeof(T) == typeof(float))
                    {
                        float dy = Convert.ToSingle(func2Value) - Convert.ToSingle(func1Value);
                        result = (T)Convert.ChangeType(Convert.ToSingle(func1Value) + dy * factor, typeof(T));
                    }
                }
                else if (_methods[0] == "exponential")
                {
                    var @base = float.Parse(_methods[1], System.Globalization.CultureInfo.InvariantCulture);

                    if (@base > 1f) @base *= 2.3f;  // empric !?

                    if (typeof(T) == typeof(float))
                    {
                        var val = Convert.ToSingle(func1Value)
                            + (Convert.ToSingle(func2Value) - Convert.ToSingle(func1Value))
                            * (float)Math.Pow((pointValue - funcPoint1Value) / (funcPoint2Value - funcPoint1Value), @base);

                        result = (T)Convert.ChangeType(Math.Min(val, Convert.ToSingle(func2Value)), typeof(T));
                    }
                }
                else
                {
                    result = func1Value;
                }
            }
        }

        return result ?? throw new Exception("Interpolation Function: no value found");
    }
}

public class ValueFunc<TValue> : IValueFunc
{
    protected TValue? _value;

    public ValueFunc()
    {
        _value = default;
    }

    public ValueFunc(TValue value) => _value = value;

    virtual public T? Value<T>(IDisplay display, IFeature? feature = null)
    {
        if (typeof(T) == typeof(TValue))
        {
            return (T?)(object?)_value;
        }
        return (T?)Convert.ChangeType(_value, typeof(T));
    }
}

public class FloatValueFunc : ValueFunc<float>
{
    public FloatValueFunc(float value) : base(value) { }
}

public class BooleanValueFunc : ValueFunc<bool>
{
    public BooleanValueFunc(bool value) : base(value) { }
}

public class GetValueFunc : IValueFunc
{
    public string? _fieldName;

    public GetValueFunc(string? fieldName)
        => (_fieldName) = fieldName;

    public T? Value<T>(IDisplay display, IFeature? feature = null)
    {
        if (_fieldName == null || feature == null || feature[_fieldName] == null)
            return default;

        return (T)Convert.ChangeType(feature[_fieldName], typeof(T));
    }
}

public class LiteralValueFunc : ValueFunc<string>
{
    public LiteralValueFunc(string value) : base(value) { }
}

public class ColorValueFunc : ValueFunc<ArgbColor>
{
    public ColorValueFunc(ArgbColor value) : base(value) { }

    public override T? Value<T>(IDisplay display, IFeature? feature = null) where T : default
    {
        if (typeof(T) == typeof(ArgbColor))
        {
            return (T?)(object)_value;
        }

        return default;
    }
}

