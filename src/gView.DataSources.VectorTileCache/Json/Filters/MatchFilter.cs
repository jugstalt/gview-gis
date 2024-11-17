using gView.DataSources.VectorTileCache.Json.Filters.Abstratctions;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using System;
using System.Linq;

namespace gView.DataSources.VectorTileCache.Json.Filters;

class MatchFilter : IFilter
{
    private string _field;
    private string[] _values;
    private bool _matchResult, _notMatchResult;

    public MatchFilter(string field, string[] values, bool matchResult = true, bool notMatchResult = false)
        => (_field, _values, _matchResult, _notMatchResult) = (field, values, matchResult, notMatchResult);

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

        return _values.Contains(featureValue)
            ? _matchResult
            : _notMatchResult;
    }
}