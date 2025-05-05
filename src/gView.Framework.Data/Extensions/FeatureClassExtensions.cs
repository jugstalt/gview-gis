#nullable enable

using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Framework.Data.Extensions;

static public class FeatureClassExtensions
{
    static public Task<IFeatureCursor> GetFeaturesOrderedIfRequired(this IFeatureClass featureClass, IQueryFilter filter)
    {
        if (!(featureClass is IFeatureCursorRequiresWrapperForOrdering)
            || string.IsNullOrEmpty(filter?.OrderBy))
        {
            return featureClass.GetFeatures(filter);
        }

        return WrapFeatureCursor(featureClass, filter);
    }

    static private async Task<IFeatureCursor> WrapFeatureCursor(IFeatureClass featureClass, IQueryFilter filter)
    {
        List<OrderingFeatureCursor.Order> orderByItems = new List<OrderingFeatureCursor.Order>();

        foreach (var orderbyField in filter.OrderBy.Split(',').Select(f => f.Trim()))
        {
            var parts = orderbyField.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string fieldName = parts[0].Replace("[", "").Replace("]", "").Replace("\"", "");

            filter.AddField(fieldName);
            orderByItems.Add(new OrderingFeatureCursor.Order()
            {
                Field = fieldName,
                Descending = parts.Length == 2 && "desc".Equals(parts[1], StringComparison.OrdinalIgnoreCase)
            });
        }

        return OrderingFeatureCursor.Create(await featureClass.GetFeatures(filter), orderByItems.ToArray());
    }
}
