﻿using gView.DataSources.VectorTileCache.Json;
using gView.Framework.Symbology.Vtc;
using gView.GraphicsEngine;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using System.Globalization;
using System.Text.Json;

namespace gView.Framework.Vtc.Extensions;

static internal class ValueFuncCreatorExtensions
{
    static public IValueFunc? ToValueFunc(this JsonElement? jsonElement)
    {
        if (!jsonElement.HasValue)
        {
            return null;
        }

        List<IValueFunc?> valueFuncs = new();

        try
        {
            if (jsonElement.Value.ValueKind == JsonValueKind.String)
            {
                var value = jsonElement.Value.GetString().ToFuncValue();
                valueFuncs.Add(value switch
                {
                    ArgbColor color => new ColorValueFunc(color),
                    float number => new FloatValueFunc(number),
                    bool boolean => new BooleanValueFunc(boolean),
                    string literal => new LiteralValueFunc(literal),
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
                valueFuncs.Add(jsonElement.Value.EnumerateArray().FirstOrDefault().ToString()?.ToLower() switch
                {
                    "case" => jsonElement.Value.EnumerateArray().Skip(1).ToArray().ToCaseValueFunc(),
                    "get" => new GetValueFunc(jsonElement.Value.EnumerateArray().Skip(1).FirstOrDefault().GetString()),
                    "concat" => jsonElement.Value.EnumerateArray().Skip(1).ToArray().ToConcatValueFunc(),
                    "to-string" => ToValueFunc(jsonElement.Value.EnumerateArray().Skip(1).ToArray().First()),
                    "match" => jsonElement.Value.EnumerateArray().Skip(1).ToArray().ToMatchValueFunc(),
                    "step" => jsonElement.Value.EnumerateArray().Skip(1).ToArray().ToStepValueFunc(),
                    "coalesce" => jsonElement.Value.EnumerateArray().Skip(1).ToArray().ToCoalesceValueFunc(),
                    "interpolate" => jsonElement.Value.EnumerateArray().Skip(1).ToArray().ToInterpolateValueFunc(),
                    _ => jsonElement.Value.IsNumberArray()  // number or string
                            ? new ValueFunc<float[]>(jsonElement.Value.ToFloatArray())
                            : jsonElement.Value.IsStringArray()
                                ? new ValueFunc<string[]>(jsonElement.Value.ToStringArray())
                                : throw new ArgumentException($"Unknown array type {jsonElement.ToString()}")
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
                            "stops" => (dict[key].Deserialize<JsonElement[]>() ?? [])
                                            .ToStopValueFunc(dict.ContainsKey("base") 
                                                    ? dict["base"]
                                                    : null),
                            "base" => null,  // ignore
                            _ => throw new ArgumentException($"Unknown function: {key}")
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Parsing Error:\n{jsonElement.Value}\n{ex.Message}");
        }
        return valueFuncs.Where(f => f != null).FirstOrDefault();  // todo: ValueFuncCollection
    }

    static internal StopsValueFunc ToStopValueFunc(
            this JsonElement[] jsonElements,
            JsonElement? baseJsonElement = null
        )
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

        if(baseJsonElement.HasValue)
        {
            stopsFunc.BaseValueFunc = baseJsonElement.ToValueFunc();
        }

        return stopsFunc;
    }

    static internal CaseValueFunc ToCaseValueFunc(this JsonElement[] jsonElements)
    {
        var caseValueFunc = new CaseValueFunc();

        if (jsonElements.Length % 2 != 0)
        {
            var defaultValueFunc = ToValueFunc(jsonElements.Last());
            caseValueFunc.SetDefaultValue(defaultValueFunc!);
            jsonElements = jsonElements.Take(jsonElements.Length - 1).ToArray();
        }

        for (int i = 0; i < jsonElements.Length; i += 2)
        {
            var filter = VtcStyleFilter.FilterFromJsonElement(jsonElements[i]);
            var resultValueFunc = ToValueFunc(jsonElements[i + 1])!;

            caseValueFunc.AddCase(filter, resultValueFunc);
        }

        return caseValueFunc;
    }

    static internal ConcatValuesFunc ToConcatValueFunc(this JsonElement[] jsonElements)
    {
        var concatValueFunc = new ConcatValuesFunc();

        foreach(JsonElement jsonElement in jsonElements)
        {
            var func = ToValueFunc(jsonElement);

            if (func is null) throw new Exception($"Unknown function in concat expression: {jsonElement.ToString()}");

            concatValueFunc.Add(func);
        }

        return concatValueFunc;
    }

    static internal CoalesceValueFunc ToCoalesceValueFunc(this JsonElement[] jsonElements)
    {
        var coalesceValueFunc = new CoalesceValueFunc();

        foreach (JsonElement jsonElement in jsonElements)
        {
            var func = ToValueFunc(jsonElement);

            if (func is null) throw new Exception($"Unknown function in coalesce expression: {jsonElement.ToString()}");

            coalesceValueFunc.Add(func);
        }

        return coalesceValueFunc;
    }

    static public MatchValueFunc ToMatchValueFunc(this JsonElement[] jsonElements)
    {
        if (jsonElements.Length != 4)
            throw new Exception($"Invalid match function {jsonElements.ToString()}");
        
        var valueFunc = ToValueFunc(jsonElements[0]);
        var matchFuncs = jsonElements[1].EnumerateArray()
                                .Select(e => ToValueFunc(e))
                                .ToArray();
        var ifMatchFunc = ToValueFunc(jsonElements[^2]);
        var ifNotMatchFunc = ToValueFunc(jsonElements[^1]);

        return new MatchValueFunc(valueFunc!, matchFuncs!, ifMatchFunc!, ifNotMatchFunc!);
    }

    static internal InterpolateValueFunc ToInterpolateValueFunc(this JsonElement[] jsonElements)
    {
        return new InterpolateValueFunc(
            jsonElements[0].EnumerateArray().Select(e => e.ToString()).ToArray(),
            jsonElements[1].ToStringArray(),
            jsonElements.Skip(2).Select(e=>ToValueFunc(e)).ToArray()!);
    }

    static internal StepValueFunc ToStepValueFunc(this JsonElement[] jsonElements)
    {
        return new StepValueFunc(
            jsonElements[0].ToStringArray(),
            jsonElements.Skip(1).Select(e => ToValueFunc(e)).ToArray()!);
    }

    static internal object ToFuncValue(this JsonElement jsonElement)
        => jsonElement.ValueKind switch
        {
            JsonValueKind.Number => (float)jsonElement.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => jsonElement.GetString().ToFuncValue(),
            JsonValueKind.Array => jsonElement.IsNumberArray()
                    ? jsonElement.ToFloatArray() 
                    : jsonElement.IsStringArray()
                        ? jsonElement.ToStringArray()
                        : throw new Exception($"Can't convert element to a string or number array {jsonElement.ToString()}"),
            _ => throw new Exception($"Unexpected function value kind: {jsonElement.ValueKind}")
        };

    static internal object ToFuncValue(this string? value)
    {
        if (String.IsNullOrEmpty(value))
        {
            return "";
            //throw new ArgumentException("can't convert emtpy string to a function value");
        }

        if (ArgbColor.TryFromString(value, out ArgbColor color))
        {
            return color;
        }

        if (float.TryParse(value, CultureInfo.InvariantCulture, out float number))
        {
            return number;
        }

        return value.ToString();
        //throw new ArgumentException($"Can't convert string to a function value: {value}");
    }

    static internal T? ToFuncValueOfType<T>(this JsonElement jsonElement)
    {
        try
        {
            var value = jsonElement.ToFuncValue();
            if (value?.GetType() == typeof(T))
            {
                return (T)value;
            }
        }
        catch { }

        return default;
    }
}

static internal class JsonElementArrayExtensions
{
    static public bool IsArray(this JsonElement element)
        => element.ValueKind == JsonValueKind.Array;

    static public bool IsArrayOfKind(this JsonElement element, JsonValueKind kind)
        => element.IsArray()
           && element.EnumerateArray().All(e => e.ValueKind == kind);

    static public bool IsNumberArray(this JsonElement array)
        => IsArrayOfKind(array, JsonValueKind.Number);

    static public bool IsStringArray(this JsonElement array)
        => IsArrayOfKind(array, JsonValueKind.String);

    static public float[] ToFloatArray(this JsonElement array)
    {
        if (!IsNumberArray(array))
            throw new ArgumentException("array must be an array of numbers");

        return array.EnumerateArray().Select(e => (float)e.GetDouble()).ToArray();
    }

    static public string[] ToStringArray(this JsonElement array)
    {
        if (!IsStringArray(array))
            throw new ArgumentException("array must be an array of numbers");

        return array.EnumerateArray().Select(e => e.GetString() ?? "").ToArray();
    }
}
