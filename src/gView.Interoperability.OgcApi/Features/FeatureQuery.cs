using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data.Extensions;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
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
        var featureClass = collection.FeatureClass;

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

    private static async Task<List<OgcFeature>> QueryAsync(
                IServiceMap map,
                OgcCollectionLayer collection,
                IQueryFilter filter,
                CrsReference outCrs,
                int offset,
                int take)
    {
        var featureClass = collection.FeatureClass;
        var propertyFields = collection.PropertyFields.ToList();
        var datumTransformations = map.Display?.DatumTransformations;
        var outSref = outCrs.ToSpatialReference();

        if (String.IsNullOrWhiteSpace(filter.WhereClause))
        {
            filter.WhereClause = "1=1";
        }

        filter.SubFields = String.Join(",",
            new[] { featureClass.IDFieldName }
                .Concat(propertyFields.Select(f => f.name))
                .Where(n => !String.IsNullOrEmpty(n))
                .Distinct());
        if (!String.IsNullOrEmpty(featureClass.ShapeFieldName))
        {
            filter.AddField(featureClass.ShapeFieldName);
        }

        #region Paging

        int skip = 0;
        if (featureClass is ISupportsQueryPaging)
        {
            filter.BeginRecord = offset + 1;  // 1-based by IQueryFilter definition
            filter.Limit = take;
            if (!String.IsNullOrEmpty(featureClass.IDFieldName))
            {
                filter.OrderBy = featureClass.IDFieldName;  // stable pages
            }
        }
        else
        {
            // data source can't page => read from the beginning and skip the records here
            filter.BeginRecord = 1;
            filter.Limit = offset + take;
            skip = offset;
        }

        #endregion

        #region Spatial Reference

        IGeometricTransformer? transformer = null;

        if (featureClass.SpatialReference is not null)
        {
            filter.SetFeatureSpatialReference(outSref, datumTransformations);
        }
        else if (collection.SpatialReference is not null)
        {
            // feature class without spatial reference => use the map's layer default spatial reference
            transformer = GeometricTransformerFactory.Create(datumTransformations);
            transformer.SetSpatialReferences(collection.SpatialReference, outSref);

            if (filter is ISpatialFilter spatialFilter
                && spatialFilter.FilterSpatialReference is not null
                && spatialFilter.FilterSpatialReference.EpsgCode != collection.SpatialReference.EpsgCode)
            {
                using var filterTransformer = GeometricTransformerFactory.Create(datumTransformations);
                filterTransformer.SetSpatialReferences(spatialFilter.FilterSpatialReference, collection.SpatialReference);
                spatialFilter.Geometry = filterTransformer.Transform2D(spatialFilter.Geometry) as IGeometry;
                spatialFilter.FilterSpatialReference = collection.SpatialReference;
            }
        }

        #endregion

        var result = new List<OgcFeature>();

        try
        {
            using var cursor = await featureClass.GetFeatures(filter);
            if (cursor is null)
            {
                return result;
            }

            IFeature feature;
            while (result.Count < take && (feature = await cursor.NextFeature()) != null)
            {
                if (skip > 0)
                {
                    skip--;
                    continue;
                }

                var ogcFeature = new OgcFeature()
                {
                    Geometry = transformer is not null && feature.Shape is not null
                        ? transformer.Transform2D(feature.Shape) as IGeometry
                        : feature.Shape
                };

                foreach (var fieldValue in feature.Fields)
                {
                    if (fieldValue.Name == featureClass.IDFieldName)
                    {
                        ogcFeature.Id = fieldValue.Value is DBNull ? null : fieldValue.Value;
                    }
                }

                foreach (var field in propertyFields)
                {
                    var value = feature[field.name];
                    ogcFeature.Properties.Add(new KeyValuePair<string, object?>(field.name, value is DBNull ? null : value));
                }

                if (ogcFeature.Id is null && feature.OID > 0)
                {
                    ogcFeature.Id = feature.OID;
                }

                result.Add(ogcFeature);
            }
        }
        finally
        {
            transformer?.Dispose();
        }

        return result;
    }

    internal static string CombineWhereClauses(params string?[] whereClauses)
    {
        var clauses = whereClauses.Where(w => !String.IsNullOrWhiteSpace(w)).ToArray();

        return clauses.Length switch
        {
            0 => "",
            1 => clauses[0]!,
            _ => String.Join(" AND ", clauses.Select(c => $"({c})"))
        };
    }

    #endregion
}
