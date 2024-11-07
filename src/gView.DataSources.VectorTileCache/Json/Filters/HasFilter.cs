using gView.DataSources.VectorTileCache.Json.Filters.Abstratctions;
using gView.Framework.Core.Data;

namespace gView.DataSources.VectorTileCache.Json.Filters;

class HasFilter : IFilter
{
    private string _hasOperator; // has, !has
    private string[] _fields;

    public HasFilter(string hasOperator, string[] fields)
    {
        _hasOperator = hasOperator;
        _fields = fields;
    }

    public bool Test(IFeature feature)
    {
        for (int i = 0; i < _fields.Length; i++)
        {
            bool fits = _hasOperator switch
            {
                "has" => feature.FindField(_fields[i]) != null,
                "!has" => feature.FindField(_fields[i]) == null,
                _ => throw new System.Exception($"Unsupported 'has' operator {_hasOperator}. must be 'has' or '!has'")
            };

            if (!fits) return false;
        }

        return true;
    }
}
