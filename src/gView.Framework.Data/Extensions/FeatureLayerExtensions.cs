#nullable enable

using gView.Framework.Core.Data;
using gView.Framework.Data.Filters;
using System.Linq;

namespace gView.Framework.Data.Extensions;

static public class FeatureLayerExtensions
{
    /// <summary>
    /// For layers whose feature renderer comes from a public request (GeoServices dynamicLayers,
    /// WMS SLD_BODY): every field the renderer adds to the query must be a field of the table.
    /// Throws <see cref="SqlDangerousStatementExceptions"/> otherwise.
    /// </summary>
    static public void CheckRequestRendererFields(this IFeatureLayer layer, ITableClass? tableClass)
    {
        if (layer?.FeatureRenderer == null || tableClass == null)
        {
            return;
        }

        var probe = new QueryFilter();
        layer.FeatureRenderer.PrepareQueryFilter(layer, probe);

        tableClass.ProjectRequestFieldNames(probe.QuerySubFields).ToArray();
    }
}
