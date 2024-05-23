using gView.DataSources.VectorTileCache.Json.Filters.Abstratctions;
using gView.Framework.Core.Data;
using System.Linq;

namespace gView.DataSources.VectorTileCache.Json.Filters;

class InFilter : IFilter
{
    private string _inOperator;
    private string _field;
    private string[] _values;

    public InFilter(string eqOperator, string field, object[] values)
    {
        _inOperator = eqOperator;
        _field = field;
        _values = values.Select(v => v?.ToString() ?? "").ToArray();
    }

    public bool Test(IFeature feature)
    {
        var featureValue = feature[_field]?.ToString() ?? "";

        bool fits = _inOperator switch
        {
            "in" => _values.Contains(featureValue) == true,
            "!in" => _values.Contains(featureValue) == false,
            _ => throw new System.Exception($"Unsupported 'in' operator {_inOperator}. must be 'in' or '!in'")
        };

        return fits;
    }
}
