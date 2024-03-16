using gView.DataSources.VectorTileCache.Json.Styles;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.GraphicsEngine;

namespace gView.Framework.Symbology.Vtc;

public class PaintSymbol : ISymbol
{
    private ILineSymbol? _lineSymbol;
    private IFillSymbol? _fillSymbol;
    private IPointSymbol? _pointSymbol;

    private Dictionary<string, IValueFunc> _valueFunc = new();

    public PaintSymbol()
    {
    }

    public PaintSymbol(
            IPointSymbol? pointSymbol,
            ILineSymbol? lineSymbol,
            IFillSymbol? fillSymbol
         )
    {
        _pointSymbol = pointSymbol;
        _lineSymbol = lineSymbol;
        _fillSymbol = fillSymbol;
    }

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
            _fillSymbol?.Clone() as IFillSymbol);

        clone._valueFunc = _valueFunc;

        return clone;
    }

    public object Clone(CloneOptions options) => Clone();

    public void Draw(IDisplay display, IGeometry geometry)
    {

    }

    public void Draw(IDisplay display, IFeature feature)
    {
        var geometry = feature?.Shape;

        //if (geometry is IPoint && _pointSymbol != null)
        //{
        //    display.Draw(_pointSymbol, geometry);
        //}
        //else 
        if (geometry is IPolyline && _lineSymbol != null)
        {
            _lineSymbol.SymbolSmoothingMode = SymbolSmoothing.AntiAlias;

            float opacity = GetValueOrDeafult(StyleProperties.LineOpacity, 1f, display, feature);

            if (_lineSymbol is IPenWidth penWidth)
            {
                penWidth.PenWidth = GetValueOrDeafult(StyleProperties.LineWidth, penWidth.PenWidth, display, null);
            }

            if (_lineSymbol is IPenColor penColor)
            {
                penColor.PenColor = GetValueOrDeafult(StyleProperties.LineColor, penColor.PenColor, display, null);

                if (opacity != 1f)
                {
                    penColor.PenColor = ArgbColor.FromArgb((int)(255 * opacity), penColor.PenColor);
                }
            }

            display.Draw(_lineSymbol, geometry);
        }
        else if (geometry is IPolygon && _fillSymbol != null)
        {
            _fillSymbol.SymbolSmoothingMode =
                GetValueOrDeafult(StyleProperties.FillOpacity, true, display, feature)
                ? SymbolSmoothing.AntiAlias
                : SymbolSmoothing.None;

            float opacity = GetValueOrDeafult(StyleProperties.FillOpacity, 1f, display, feature);

            if (_fillSymbol is IBrushColor brushColor)
            {
                brushColor.FillColor
                    = GetValueOrDeafult([StyleProperties.FillColor, StyleProperties.FillExtrusionColor],
                            brushColor.FillColor,
                            display, feature);

                if (opacity != 1f)
                {
                    brushColor.FillColor = ArgbColor.FromArgb((int)(255 * opacity), brushColor.FillColor);
                }
            }

            display.Draw(_fillSymbol, geometry);
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

        _pointSymbol = null;
        _lineSymbol = null;
        _fillSymbol = null;
    }

    public bool RequireClone() =>
        _pointSymbol?.RequireClone() == true
        || _lineSymbol?.RequireClone() == true
        || _fillSymbol?.RequireClone() == true;

    public bool SupportsGeometryType(GeometryType type) => true;

    #endregion

    public void AddValueFunc(string key, IValueFunc valueFunc)
    {
        _valueFunc[key] = valueFunc;
    }

    private T GetValueOrDeafult<T>(string name, T defaultValue, IDisplay display, IFeature? feature)
    {
        if (!_valueFunc.ContainsKey(name)) return defaultValue;

        return _valueFunc[name].Value<T>(display, feature) ?? defaultValue;
    }

    private T GetValueOrDeafult<T>(string[] names, T defaultValue, IDisplay display, IFeature? feature)
    {
        foreach (var name in names)
        {
            if (_valueFunc.ContainsKey(name))
            {
                return _valueFunc[name].Value<T>(display, feature) ?? defaultValue;
            }
        }

        return defaultValue;
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

public class FloatValueFunc : IValueFunc
{
    private float _value;

    public FloatValueFunc()
    {
    }

    public FloatValueFunc(float value) => _value = value;

    public T? Value<T>(IDisplay display, IFeature? feature = null)
    {
        return (T)Convert.ChangeType(_value, typeof(T));
    }
}

public class ColorValueFunc : IValueFunc
{
    private ArgbColor _value;

    public ColorValueFunc()
    {
    }

    public ColorValueFunc(ArgbColor value) => _value = value;

    public T? Value<T>(IDisplay display, IFeature? feature = null)
    {
        if (typeof(T) == typeof(ArgbColor))
        {
            return (T)(object)(_value);
        }

        return default;
    }
}

public class BooleanValueFunc : IValueFunc
{
    private bool _value;

    public BooleanValueFunc()
    {
    }

    public BooleanValueFunc(bool value) => _value = value;

    public T? Value<T>(IDisplay display, IFeature? feature = null)
    {
        return (T)Convert.ChangeType(_value, typeof(T));
    }
}