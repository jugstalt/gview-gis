using gView.DataSources.VectorTileCache.Json.GLStyles;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.Framework.Symbology.Vtc.Extensions;
using gView.GraphicsEngine;

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
            if(modify)
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
        else if(_textSymbol != null)
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

    public T? Value<T>(IDisplay display, IFeature? feature = null)
    {
        if (_stops.Length == 0)
        {
            return default(T);
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
}

public class CaseValueFunc : IValueFunc
{
    private object? _defaultValue;
    private List<(string field, string comparisionOperator, string? comparisionValue, object resultValue)> _values = new();

    public void AddCase(string field, string comparisionOperator, object comparisionValue, object resultValue)
    {
        _values.Add((field, comparisionOperator, comparisionValue?.ToString(), resultValue));
    }

    public void SetDefaultValue(object defaultValue)
    {
        _defaultValue = defaultValue;
    }

    public T? Value<T>(IDisplay display, IFeature? feature = null)
    {
        if (feature != null)
        {
            foreach (var caseValue in _values)
            {
                var featureValue = feature[caseValue.field]?.ToString();
                if (featureValue == null) continue;

                var fitCondition = caseValue.comparisionOperator switch
                {
                    "==" => featureValue == caseValue.comparisionValue,
                    "!=" => featureValue != caseValue.comparisionValue,
                    _ => throw new ArgumentException($"Unknown case operator '{caseValue.comparisionOperator}'")
                };

                if (fitCondition)
                {
                    return (T?)caseValue.resultValue;
                }
            }
        }

        return (T?)_defaultValue;
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
        if(typeof(T) == typeof(TValue))
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

public class LiberalValueFunc : ValueFunc<string>
{
    public LiberalValueFunc(string value) : base(value) { }
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

