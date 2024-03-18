using gView.DataSources.VectorTileCache.Json.Filters.Abstratctions;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using System;
using System.Globalization;

namespace gView.DataSources.VectorTileCache.Json.Filters;

class EqualityFilter : IFilter
{
    private string _eqOperator;
    private string _field;
    private string _value;

    public EqualityFilter(string eqOperator, string field, object value)
    {
        _eqOperator = eqOperator;
        _field = field;
        _value = value?.ToString() ?? "";
    }

    public bool Test(IFeature feature)
    {
        var featureValue = _field switch
        {
            "$type" => feature.Shape switch
            {
                IPoint => "Point",
                IPolyline => "LineString",
                IPolygon => "Polygon",
                _ => null
            },
            _ => feature[_field]?.ToString() ?? "" // fieldvalue
        };

        bool fits = _eqOperator switch
        {
            // ToDo: Type comparision more save...
            "==" => featureValue == _value,
            "!=" => featureValue != _value,
            ">=" => float.Parse(featureValue, CultureInfo.InvariantCulture) >= float.Parse(_value, CultureInfo.InvariantCulture),
            "<=" => float.Parse(featureValue, CultureInfo.InvariantCulture) <= float.Parse(_value, CultureInfo.InvariantCulture),
            "<" => float.Parse(featureValue, CultureInfo.InvariantCulture) < float.Parse(_value, CultureInfo.InvariantCulture),
            ">" => float.Parse(featureValue, CultureInfo.InvariantCulture) > float.Parse(_value, CultureInfo.InvariantCulture),

            _ => throw new Exception($"unsupported filter operator {_eqOperator}")
        };

        return fits;
    }
}
