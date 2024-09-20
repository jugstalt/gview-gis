using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;

namespace gView.Framework.Common.Json.Converters.Extensions;
static public class ExpandoObjectExtensions
{
    static public IDictionary<string, object> ToDictionaryWithNativeTypes(
            this ExpandoObject expandoObject,
            bool ignoreNulls = false)
    {
        var result = new Dictionary<string, object>();
        if (expandoObject is null) return result;

        var dict = (IDictionary<string, object>)expandoObject;

        foreach (var key in dict.Keys)
        {
            var valueObject = dict[key];

            if (valueObject is JsonElement element)
            {
                valueObject = GetJsonElementValue(element);
            }

            if (ignoreNulls && valueObject is null) continue;

            result[key] = valueObject;
        }

        return result;
    }

    private static object GetJsonElementValue(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Null:
                return null;
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Number:
                if (element.TryGetInt64(out long l))
                    return l;
                else if (element.TryGetDouble(out double d))
                    return d;
                else if (element.TryGetDecimal(out decimal m))
                    return m;
                else
                    return element.GetRawText();
            case JsonValueKind.True:
            case JsonValueKind.False:
                return element.GetBoolean();
            case JsonValueKind.Array:
                var list = new List<object>();
                foreach (var item in element.EnumerateArray())
                {
                    list.Add(GetJsonElementValue(item));
                }
                return list;
            case JsonValueKind.Object:
                var dict = new Dictionary<string, object>();
                foreach (var property in element.EnumerateObject())
                {
                    dict[property.Name] = GetJsonElementValue(property.Value);
                }
                return dict;
            default:
                return element.GetRawText();
        }
    }
}
