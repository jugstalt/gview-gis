using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Interoperability.Extensions.Features;
using System.Globalization;

namespace gView.Interoperability.OgcApi.Features;

public sealed class FeaturePage
{
    public FeaturePage(List<OgcFeature> features, bool hasMore)
    {
        Features = features;
        HasMore = hasMore;
    }

    public List<OgcFeature> Features { get; }
    public bool HasMore { get; }
}

/// <summary>
/// Executes items/item requests against the feature class of a collection
/// </summary>
internal static class FeatureQuery
{
    public static async Task<FeaturePage> QueryItemsAsync(IServiceMap map, OgcCollectionLayer collection, ItemsQuery query)
    {
        IQueryFilter filter;

        if (query.BBox is not null)
        {
            var bboxSref = query.BBoxCrs.ToSpatialReference();
            var bbox = query.BBox;
            var envelope = query.BBoxCrs.HasNorthingFirst(bboxSref)
                ? new Envelope(bbox[1], bbox[0], bbox[3], bbox[2])
                : new Envelope(bbox[0], bbox[1], bbox[2], bbox[3]);

            filter = new SpatialFilter()
            {
                Geometry = envelope,
                FilterSpatialReference = bboxSref,
                SpatialRelation = spatialRelation.SpatialRelationIntersects
            };
        }
        else
        {
            filter = new QueryFilter();
        }

        // The request part is generated from the CQL2 syntax tree (queryable field names and
        // literals written by the translator), so no further SQL injection check is needed.
        // CheckWhereClauseForSqlInjection would also reject valid filters like "FLAG=TRUE" or "A=B".
        filter.WhereClause = CombineWhereClauses(
                    collection.FilterWhereClause,
                    query.BuildWhereClause(collection));

        // fetch one more feature to know whether there is a next page
        var features = await QueryAsync(map, collection, filter, query.Crs, query.Offset, query.Limit + 1);

        bool hasMore = features.Count > query.Limit;
        if (hasMore)
        {
            features.RemoveAt(features.Count - 1);
        }

        return new FeaturePage(features, hasMore);
    }

    public static async Task<OgcFeature> QueryItemAsync(IServiceMap map, OgcCollectionLayer collection, string featureId, CrsReference crs)
    {
        var featureClass = collection.FeatureClass
            ?? throw OgcApiException.NotFound($"Collection '{collection.Id}' has no features");

        if (String.IsNullOrEmpty(featureClass.IDFieldName)
            || !long.TryParse(featureId, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long oid))
        {
            throw OgcApiException.NotFound($"Feature '{featureId}' not found");
        }

        var filter = new QueryFilter()
        {
            WhereClause = CombineWhereClauses(
                    collection.FilterWhereClause,
                    $"{featureClass.IDFieldName}={oid.ToString(CultureInfo.InvariantCulture)}")
        };

        var features = await QueryAsync(map, collection, filter, crs, 0, 1);

        return features.FirstOrDefault()
            ?? throw OgcApiException.NotFound($"Feature '{featureId}' not found");
    }

    #region Helper

    private static Task<List<OgcFeature>> QueryAsync(
                IServiceMap map,
                OgcCollectionLayer collection,
                IQueryFilter filter,
                CrsReference outCrs,
                int offset,
                int take)
    {
        if (collection.FeatureClass is null)
        {
            throw OgcApiException.NotFound($"Collection '{collection.Id}' has no features");
        }

        return FeatureReader.ReadListAsync(map, collection, filter, outCrs.ToSpatialReference(), offset, take);
    }

    internal static string CombineWhereClauses(params string?[] whereClauses)
        => FeatureReader.CombineWhereClauses(whereClauses);

    #endregion
}
