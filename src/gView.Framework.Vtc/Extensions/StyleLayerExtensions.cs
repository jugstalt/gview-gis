using gView.DataSources.VectorTileCache.Json.Styles;
using gView.Framework.Core.Symbology;
using gView.Framework.Symbology;
using gView.Framework.Symbology.Vtc;
using gView.GraphicsEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace gView.Framework.Vtc.Extensions;

static internal class StyleLayerExtensions
{
    static public PaintSymbol ToPaintSymbol(this StyleLayer styleLayer)
    {
        if (styleLayer?.Paint == null)
        {
            return new PaintSymbol();
        }
        ILineSymbol? lineSymbol = null;

        if (styleLayer.Paint.FillColor != null
           || styleLayer.Paint.FillOutlineColor != null)
        {
            lineSymbol = new SimpleLineSymbol();
        }

        if (styleLayer.Layout is not null)
        {
            foreach (var layoutPropertyInfo in styleLayer.Layout.GetType().GetProperties())
            {
                if (layoutPropertyInfo.PropertyType == typeof(JsonElement?))
                {
                    var value = (JsonElement?)layoutPropertyInfo.GetValue(styleLayer.Layout, null);

                    var func = value.ToValueFunc();
                }
            }
        }

        if (styleLayer.Paint is not null)
        {
            foreach (var paintPropertyInfo in styleLayer.Paint.GetType().GetProperties())
            {
                if (paintPropertyInfo.PropertyType == typeof(JsonElement?))
                {
                    var value = (JsonElement?)paintPropertyInfo.GetValue(styleLayer.Paint, null);

                    var func = value.ToValueFunc();
                }
            }
        }

        return new PaintSymbol();
    }
}

static internal class ValueFuncCreatorExtensions
{
    static public IValueFunc? ToValueFunc(this JsonElement? jsonElement)
    {
        if (!jsonElement.HasValue)
        {
            return null;
        }

        List<IValueFunc> valueFuncs = new();

        try
        {
            if (jsonElement.Value.ValueKind == JsonValueKind.String)
            {
                var value = jsonElement.Value.GetString().ToFuncValue();
                valueFuncs.Add(value switch
                {
                    ArgbColor color => new IntegerValueFunc(color.ToArgb()),
                    float number => new FloatValueFunc(number),
                    bool boolean => new BooleanValueFunc(boolean),
                    _ => throw new ArgumentException($"Can't convert string {value} to a value")
                });
            }
            if (jsonElement.Value.ValueKind == JsonValueKind.Number)
            {
                valueFuncs.Add(new FloatValueFunc((float)jsonElement.Value.GetDouble()));
            }
            if (jsonElement.Value.ValueKind == JsonValueKind.True)
            {
                valueFuncs.Add(new BooleanValueFunc(true));
            }
            if (jsonElement.Value.ValueKind == JsonValueKind.False)
            {
                valueFuncs.Add(new BooleanValueFunc(false));
            }
            else if (jsonElement.Value.ValueKind == JsonValueKind.Array)
            {
                valueFuncs.Add(jsonElement.Value.EnumerateArray().FirstOrDefault().GetString()?.ToLower() switch
                {
                    "case" => jsonElement.Value.EnumerateArray().Skip(1).ToArray().ToCaseValueFunc(),
                    _ => throw new ArgumentException($"Unknown array type")
                });
            }
            else if (jsonElement.Value.ValueKind == JsonValueKind.Object)
            {
                var dict = jsonElement.Value.Deserialize<Dictionary<string, JsonElement>>();

                if (dict is not null)
                {
                    foreach (var key in dict.Keys)
                    {
                        valueFuncs.Add(key.ToLower() switch
                        {
                            "stops" => (dict["stops"].Deserialize<JsonElement[]>() ?? []).ToStopValueFunc(),
                            _ => throw new ArgumentException($"Unknown function: {key}")
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Paring Error:\n{jsonElement.Value}\n{ex.Message}");
        }
        return valueFuncs.FirstOrDefault();  // todo: ValueFuncCollection
    }

    static private StopsValueFunc ToStopValueFunc(this JsonElement[] jsonElements)
    {
        var stopsFunc = new StopsValueFunc();

        foreach (var stopElement in jsonElements)
        {
            var pair = stopElement.Deserialize<JsonElement[]>();

            float scale;
            object value;

            if (pair != null && pair.Length == 2 && pair[0].ValueKind == JsonValueKind.Number)
            {
                scale = (float)pair[0].GetDouble();
                value = pair[1].ToFuncValue();
            }
            else
            {
                throw new ArgumentException($"Unknown Stops pair: {stopElement}");
            }

            stopsFunc.AddStop(scale, value);
        }

        return stopsFunc;
    }

    static private CaseValueFunc ToCaseValueFunc(this JsonElement[] jsonElements)
    {
        var caseValueFunc = new CaseValueFunc();

        if (jsonElements.Length % 2 != 0)
        {
            caseValueFunc.SetDefaultValue(jsonElements.Last().GetString().ToFuncValue());
            jsonElements = jsonElements.Take(jsonElements.Length - 1).ToArray();
        }

        for (int i = 0; i < jsonElements.Length; i += 2)
        {
            var conditionElement = jsonElements[i];
            var resultValue = jsonElements[i + 1].ToFuncValue();

            if (conditionElement.ValueKind == JsonValueKind.Array)
            {
                // ["==", 12, ["number", ["get", "the_attribute"]]],  WTF!
                var conditionElements = conditionElement.EnumerateArray().ToArray();

                if (conditionElements.Length == 3)
                {
                    var comparisonOperator = conditionElements[0].GetString();
                    if (string.IsNullOrEmpty(comparisonOperator))
                    {
                        throw new ArgumentException("Comparision operator is empty!");
                    }

                    var comparisonValue = conditionElements[1].ToFuncValue();

                    if (conditionElements[2].ValueKind == JsonValueKind.Array)
                    {
                        var comparisionDef = conditionElements[2].EnumerateArray().ToArray();
                        if (comparisionDef.Length == 2 && comparisionDef[1].ValueKind == JsonValueKind.Array)
                        {
                            var fieldDef = comparisionDef[1].EnumerateArray().ToArray();
                            if (fieldDef.Length == 2)
                            {
                                var comparisionField = fieldDef[1].GetString();

                                if (String.IsNullOrEmpty(comparisionField))
                                {
                                    throw new ArgumentException("comparision field is empty");
                                }

                                caseValueFunc.AddCase(comparisionField, comparisonOperator, comparisonValue, resultValue);
                            }
                            else
                            {
                                throw new Exception($"Unknown case format: {conditionElement}");
                            }
                        }
                        else
                        {
                            throw new Exception($"Unknown case format: {conditionElement}");
                        }
                    }
                    else
                    {
                        throw new Exception($"Unknown case format: {conditionElement}");
                    }
                }
                else
                {
                    throw new Exception($"Unknown case format: {conditionElement}");
                }
            }
            else
            {
                throw new Exception($"Unknown case contition: {conditionElement}");
            }
        }

        return caseValueFunc;
    }

    static private object ToFuncValue(this JsonElement jsonElement)
        => jsonElement.ValueKind switch
        {
            JsonValueKind.Number => (float)jsonElement.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => jsonElement.GetString().ToFuncValue(),
            _ => throw new Exception($"Unexpected function value kind: {jsonElement.ValueKind}")
        };

    static private object ToFuncValue(this string? value)
    {
        if (String.IsNullOrEmpty(value))
        {
            throw new ArgumentException("can't convert emtpy string to a function value");
        }

        if (ArgbColor.TryFromString(value, out ArgbColor color))
        {
            return color;
        }

        if (float.TryParse(value, CultureInfo.InvariantCulture, out float number))
        {
            return number;
        }

        throw new ArgumentException($"Can't convert string to a function value: {value}");
    }
}
