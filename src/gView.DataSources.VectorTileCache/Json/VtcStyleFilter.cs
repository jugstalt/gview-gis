using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data.Filters;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace gView.DataSources.VectorTileCache.Json;

public enum VtcStyleFilterType
{
    Unknown,
    All,
    Any
}

public class VtcStyleFilter : QueryFilter
{
    private List<(string comparisonOperator, string field, string comparisonValue)> _items = new();

    public VtcStyleFilterType Type { get; set; } = VtcStyleFilterType.Unknown;

    public void AddItem(string comparisonOperator, string field, string comparisonValue) 
        => _items.Add((comparisonOperator, field, comparisonValue));

    public bool Filter(IFeature feature)
    {
        if (feature == null || _items.Count() == 0) return false;

        foreach(var item in _items)
        {
            var featureValue = feature[item.field]?.ToString();
            if(featureValue == null)
            {
                if (Type == VtcStyleFilterType.All) return false;
                continue;
            }

            bool fits = item.comparisonOperator switch
            {
                "==" => featureValue == item.comparisonValue,
                "!=" => featureValue != item.comparisonValue,
                _ => throw new Exception($"unsupported filter operator {item.comparisonOperator}")
            };

            if(fits)
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

        if(jsonElement.Value.ValueKind != JsonValueKind.Array)
        {
            throw new Exception("Unsupported filter format. Must be an array");
        }

        if(!Enum.TryParse(
                jsonElement.Value.EnumerateArray().FirstOrDefault().GetString(), 
                true,
                out VtcStyleFilterType type)
            )
        {
            throw new Exception($"Unsupported filter type {jsonElement.Value.EnumerateArray().FirstOrDefault().GetString()}");
        }

        var filter = new VtcStyleFilter() { Type = type };

        var items = jsonElement.Value.EnumerateArray().Skip(1);
        foreach(var item in items)
        {
            if (item.ValueKind != JsonValueKind.Array)
            {
                throw new Exception("Unsupported filter item format. Must be an array");
            }

            var itemParts = item.EnumerateArray().ToArray();
            if(itemParts.Length != 3)
            {
                throw new Exception("Unsported filter item format. Must be an array with three items");
            }

            filter.AddItem(
                itemParts[0].GetString(),
                itemParts[1].GetString(),
                itemParts[2].ValueKind switch
                {
                    JsonValueKind.Number => itemParts[2].GetDouble().ToString(CultureInfo.InvariantCulture),
                    JsonValueKind.String => itemParts[2].GetString(),
                    _ => throw new Exception($"Unsported filter item comparison value kind {itemParts[2].ValueKind}")
                });
        }

        return filter;
    }
}
