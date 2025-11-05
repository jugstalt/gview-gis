#nullable enable

using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Symbology;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Interoperability.GeoServices.Request.Extensions;

internal static class FeatureLayerExtensions
{
    static async public Task<(bool hasItems, HashSet<string> itemKeys)> HasLegendItems(this IFeatureLayer? featureLayer, IQueryFilter? filter)
    {
        HashSet<string> legendItemKeys = new();

        if (filter is null ||
            featureLayer?.FeatureClass is null ||
            featureLayer?.FeatureRenderer is null)
        {
            return (false, legendItemKeys);
        }

        bool hasItems = false;

        filter.SubFields = "";
        filter.OrderBy = "";
        filter.AddField(featureLayer.FeatureClass.IDFieldName);

        // do not use limits here
        // some features are discard after checking bounding box
        // eg. FDB: does not use exact spatial queries 
        //          loads all features in spatial index blocks and filters
        //          inside IFeatureCurosr if feature intersects Geometry
        //          Limt => is database side => so it is not sure that a LIMIT returns a feature
        //filter.Limit = 1;

        ILegendDependentFields? legendDependentFields = featureLayer.FeatureRenderer as ILegendDependentFields;
        List<string>? legendDependentFieldNames = null;

        if (legendDependentFields is not null)
        {
            legendDependentFieldNames = legendDependentFields.LegendDependentFields?.ToList();
            legendDependentFieldNames?
                .ForEach(field => filter.AddField(field));

            filter.OrderBy = legendDependentFields.LegendSymbolOrderField;
            //filter.Limit = -1;
        }

        using IFeatureCursor cursor = await featureLayer.FeatureClass.GetFeatures(filter);

        if (cursor is null) return (false, legendItemKeys);

        IFeature? feature = null;
        while ((feature = await cursor.NextFeature()) != null)
        {
            hasItems = true;

            if(legendDependentFields is null)
            {
                break;
            }

            legendItemKeys.Add(legendDependentFields?.LegendSymbolKeyFromFeature(feature) ?? "");
        }

        return (hasItems, legendItemKeys);
    }
}
