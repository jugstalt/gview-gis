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
            "geometry-type" => feature.Shape switch
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
            ">=" => CompareFloat(featureValue, _value, (n1, n2) => n1 >= n2),
            "<=" => CompareFloat(featureValue, _value, (n1, n2) => n1 <= n2),
            "<" => CompareFloat(featureValue, _value, (n1, n2) => n1 > n2),
            ">" => CompareFloat(featureValue, _value, (n1, n2) => n1 < n2),
            _ => throw new Exception($"unsupported filter operator {_eqOperator}")
        };

        return fits;
    }

    private bool CompareFloat(string val1, string val2, Func<float, float, bool> func)
    {
        if (float.TryParse(val1, CultureInfo.InvariantCulture, out float number1)
            && float.TryParse(val2, CultureInfo.InvariantCulture, out float number2))
        {
            return func(number1, number2);
        }

        return false;
    }
}

