using gView.Framework.Common;
using gView.GeoJsonService.DTOs;
using System.Linq;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

static public class AttributeFilterExtensions
{
    static public string WhereClauseWithResolveParameters(this AttributeFilter filter)
    {
        if (string.IsNullOrEmpty(filter.WhereClause)
            || filter.Parameters?.Any() != true)
        {
            return filter.WhereClause;
        }

        string whereClause = filter.WhereClause;

        foreach (var parameterName in filter.Parameters.Keys.OrderByDescending(k => k.Length))
        {
            var parameterValue = filter.Parameters[parameterName];
            var replaceText = parameterValue switch
            {
                byte => parameterValue.ToString(),
                short => parameterValue.ToString(),
                int => parameterValue.ToString(),
                long => parameterValue.ToString(),
                
                ushort => parameterValue.ToString(),
                uint => parameterValue.ToString(),
                ulong => parameterValue.ToString(),

                float n => n.ToFloatString(),
                double n => n.ToDoubleString(),
                decimal n => n.ToDecimalString(),

                _ => $"'{parameterValue}'"
            };

            whereClause = whereClause.Replace(parameterName, replaceText);
        }

        return whereClause;
    }
}
