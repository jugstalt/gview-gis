using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Data.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace gView.DataSources.VectorTileCache.Json;

public enum VtcStyleFilterType
{
    Unknown,
    All,
    Any
}

public class VtcStyleFilter : QueryFilter
{
    private static string[] operators2 = ["has","!has","in","!in"];
    private static string[] operators3 = ["==", "!=", ">=", "<=",">","<", "in", "!in"];
    private static string[] operatorsX = ["in", "!in"];

    private List<(string comparisonOperator, string field, string comparisonValue)> _items = new();

    public VtcStyleFilterType Type { get; set; } = VtcStyleFilterType.Unknown;

    public void AddItem(string comparisonOperator, string field, string comparisonValue)
        => _items.Add((comparisonOperator, field, comparisonValue));

    public bool Filter(IFeature feature)
    {
        if (feature == null || _items.Count() == 0) return false;

        foreach (var item in _items)
        {
            var featureValue = item.field switch
            {
                "$type" => feature.Shape switch
                {
                    IPoint => "Point",
                    IPolyline => "LineString",
                    IPolygon => "Polygon",
                    _ => null
                },
                _ => feature[item.field]?.ToString()  // fieldvalue
            };
            
            if (featureValue == null)
            {
                if (Type == VtcStyleFilterType.All) return false;
                continue;
            }

            bool fits = item.comparisonOperator switch
            {
                // ToDo: Type comparision more save...
                "==" => featureValue == item.comparisonValue,
                "!=" => featureValue != item.comparisonValue,
                ">=" => float.Parse(featureValue, CultureInfo.InvariantCulture) >= float.Parse(item.comparisonValue, CultureInfo.InvariantCulture),
                "<=" => float.Parse(featureValue, CultureInfo.InvariantCulture) <= float.Parse(item.comparisonValue, CultureInfo.InvariantCulture),
                "<" => float.Parse(featureValue, CultureInfo.InvariantCulture) < float.Parse(item.comparisonValue, CultureInfo.InvariantCulture),
                ">" => float.Parse(featureValue, CultureInfo.InvariantCulture) > float.Parse(item.comparisonValue, CultureInfo.InvariantCulture),
                "has" => !String.IsNullOrEmpty(featureValue),
                "!has" => String.IsNullOrEmpty(featureValue),
                "in" => item.comparisonValue.Split(',').Contains(featureValue),
                "!in" => !item.comparisonValue.Split(',').Contains(featureValue),
                _ => throw new Exception($"unsupported filter operator {item.comparisonOperator}")
            };

            if (fits)
            {
                if (Type == VtcStyleFilterType.Any) return true;
            }
            else
            {
                if (Type == VtcStyleFilterType.All) return false;
            }
        }

        if (Type == VtcStyleFilterType.All) return true;

        return false;
    }

    static public VtcStyleFilter FromJsonElement(JsonElement? jsonElement)
    {
        // ["all", ["==", "field1", 11010], ["==", "field2", 5023]]
        // ["any", ["==", "field1", 11010], ["==", "field1", 11011]]

        if (jsonElement == null) return null;

        var items = Array.Empty<JsonElement>();
        if (jsonElement.Value.ValueKind != JsonValueKind.Array)
        {
            throw new Exception($"Unsupported filter format {jsonElement}. Must be an array");
        }

        if (!Enum.TryParse(
                jsonElement.Value.EnumerateArray().FirstOrDefault().GetString(),
                true,
                out VtcStyleFilterType type)
            )
        {
            // single condition?
            // ["==", "fieldname","value"]

            type = VtcStyleFilterType.All; // default?
            items = [jsonElement.Value];
            //throw new Exception($"Unsupported filter type {jsonElement.Value.EnumerateArray().FirstOrDefault().GetString()}: {jsonElement}");
        } 
        else
        {
            items = jsonElement.Value.EnumerateArray().Skip(1).ToArray();
        }

        var filter = new VtcStyleFilter() { Type = type };

        foreach (var item in items)
        {
            if (item.ValueKind != JsonValueKind.Array)
            {
                throw new Exception($"Unsupported filter item format {jsonElement}. Must be an array");
            }

            var itemParts = item.EnumerateArray().ToArray();
            if (itemParts.Length == 3)
            {
                // ["==","$type","Point"]
                // ["==","$type","LineString"]
                // ["==", "fieldname","value"]

                var comparisonOpertator = itemParts[0].GetString();
                if (!operators3.Contains(comparisonOpertator))
                {
                    throw new Exception($"Unknown filter operator {comparisonOpertator}: {jsonElement}");
                }

                filter.AddItem(
                    comparisonOpertator,
                    itemParts[1].GetString(),
                    itemParts[2].ValueKind switch
                    {
                        JsonValueKind.Number => itemParts[2].GetDouble().ToString(CultureInfo.InvariantCulture),
                        JsonValueKind.String => itemParts[2].GetString(),
                        _ => throw new Exception($"Unsported filter item comparison value kind {itemParts[2].ValueKind}")
                    });
            }
            else if (itemParts.Length == 2)
            {
                // ["has","iso_a2"]

                var comparisonOpertator = itemParts[0].GetString();
                if(!operators2.Contains(comparisonOpertator))
                {
                    throw new Exception($"Unknown filter operator {comparisonOpertator}: {jsonElement}");
                }

                filter.AddItem(
                    comparisonOpertator,
                    itemParts[1].GetString(), "");
            }
            else if(itemParts.Length >3)
            {
                var comparisonOpertator = itemParts[0].GetString();
                if (!operatorsX.Contains(comparisonOpertator))
                {
                    throw new Exception($"Unknown filter operator {comparisonOpertator}: {jsonElement}");
                }
                filter.AddItem(
                    comparisonOpertator,
                    itemParts[1].GetString(),
                    String.Join(",", itemParts.Skip(2).Select(i => i.ToString())));
            }
        }
        return filter;
    }
}
